namespace Example.Api.Infrastructure.Http
{
    using System;
    using System.Net;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public class PathRestrictMiddleware
    {
        private readonly RequestDelegate next;

        private readonly string path;

        private readonly IPNetwork[] networks;

        public PathRestrictMiddleware(RequestDelegate next, PathRestrictConfig config)
        {
            if ((config.Path is null) || (config.Networks is null))
            {
                throw new ArgumentException("Invalid setting.", nameof(config));
            }

            this.next = next;
            path = config.Path;
            networks = config.Networks;
        }

        public async Task Invoke(HttpContext context)
        {
            if ((context.Request.HttpContext.Connection.RemoteIpAddress is null) ||
                (context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase) &&
                 !IsAddressAllowed(context.Request.HttpContext.Connection.RemoteIpAddress)))
            {
                context.Response.StatusCode = 403;
                return;
            }

            await next(context);
        }

        private bool IsAddressAllowed(IPAddress address)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < networks.Length; i++)
            {
                if (networks[i].Contains(address))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
