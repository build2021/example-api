namespace Example.Api
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Net;
    using System.Text.Encodings.Web;
    using System.Text.Unicode;

    using AutoMapper;

    using Example.Api.Infrastructure;
    using Example.Api.Infrastructure.ApplicationModels;
    using Example.Api.Infrastructure.Filters;
    using Example.Api.Infrastructure.Json;
    using Example.Api.Services;
    using Example.Api.Settings;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;

    using Prometheus;

    using Smart.Data;
    using Smart.Data.Mapper;
    using Smart.Data.SqlClient;

    using StackExchange.Profiling;
    using StackExchange.Profiling.Data;

    using Swashbuckle.AspNetCore.SwaggerGen;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddHttpContextAccessor();

            // Data

            // TODO Data

            // Settings
            var serverSetting = Configuration.GetSection("Server").Get<ServerSetting>();
            services.AddSingleton(serverSetting);

            // Mvc
            services.AddExceptionLogging();
            services.AddTimeLogging(options =>
            {
                options.Threshold = serverSetting.LongTimeThreshold;
            });

            services.Configure<RouteOptions>(options =>
            {
                options.AppendTrailingSlash = true;
            });

            services
                .AddControllers(options =>
                {
                    options.Filters.AddExceptionLogging();
                    options.Filters.AddTimeLogging();
                    options.Conventions.Add(new LowercaseControllerModelConvention());
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                });

            // API versioning
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Health
            services.AddHealthChecks();

            // Mapper
            services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(c =>
            {
                c.AddProfile<MappingProfile>();
            })));

            // Database
            var connectionString = Configuration.GetConnectionString("Default");
            services.AddSingleton<IDbProvider>(serverSetting.EnableProfiler
                ? new DelegateDbProvider(() => new SqlConnection(connectionString))
                : new DelegateDbProvider(() => new ProfiledDbConnection(new SqlConnection(connectionString), MiniProfiler.Current)));

            services.AddSingleton<IDialect, SqlDialect>();

            SqlMapperConfig.Default.ConfigureTypeMap(map =>
            {
                map[typeof(DateTime)] = DbType.DateTime2;
            });

            // Service
            services.AddSingleton<DataService>();

            // Swagger
            if (serverSetting.EnableSwagger)
            {
                services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
                services.AddSwaggerGen(options =>
                {
                    options.OperationFilter<SwaggerDefaultValues>();
                });
            }

            // Profiler
            if (serverSetting.EnableProfiler)
            {
                services.AddMiniProfiler(options =>
                {
                    options.RouteBasePath = "/profiler";
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider, ServerSetting serverSetting)
        {
            app.UseForwardedHeaders();

            app.UseRouting();

            // Health
            app.UseHealthChecks("/health");

            // Swagger
            if (serverSetting.EnableSwagger)
            {
                app.UsePathRestrict("/swagger", serverSetting.AllowSwagger?.Select(IPNetwork.Parse).ToArray());
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
            }

            // Metrics
            if (serverSetting.EnableMetrics)
            {
                app.UsePathRestrict("/metrics", serverSetting.AllowMetrics?.Select(IPNetwork.Parse).ToArray());
                app.UseHttpMetrics();
            }

            // Profiler
            if (serverSetting.EnableProfiler)
            {
                app.UseMiniProfiler();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", async context => await context.Response.WriteAsync("API server"));

                if (serverSetting.EnableMetrics)
                {
                    endpoints.MapMetrics();
                }
            });
        }

        public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider provider;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
            {
                this.provider = provider;
            }

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                }
            }

            private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
            {
                var info = new OpenApiInfo()
                {
                    Title = "Example API",
                    Version = description.ApiVersion.ToString()
                };

                if (description.IsDeprecated)
                {
                    info.Description += " (Deprecated)";
                }

                return info;
            }
        }

        public sealed class SwaggerDefaultValues : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var apiDescription = context.ApiDescription;
                operation.Deprecated |= apiDescription.IsDeprecated();
                if (operation.Parameters is not null)
                {
                    foreach (var parameter in operation.Parameters)
                    {
                        var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                        parameter.Description ??= description.ModelMetadata.Description;

                        parameter.Required |= description.IsRequired;
                    }
                }
            }
        }
    }
}
