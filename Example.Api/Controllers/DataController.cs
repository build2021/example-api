namespace Example.Api.Controllers
{
    using System.Threading.Tasks;

    using AutoMapper;

    using Example.Api.Metrics;
    using Example.Api.Models.Api;
    using Example.Api.Models.Entity;
    using Example.Api.Services;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("v{version:apiVersion}/[controller]/[action]")]
    [ApiVersion("1.0")]
    public class DataController : ControllerBase
    {
        private ILogger<DataController> Log { get; }

        private IMapper Mapper { get; }

        private DataService DataService { get; }

        public DataController(
            ILogger<DataController> log,
            IMapper mapper,
            DataService dataService)
        {
            Log = log;
            Mapper = mapper;
            DataService = dataService;
        }

        [HttpGet("{id}")]
        public async ValueTask<IActionResult> Get(int id)
        {
            Log.LogInformation("Get data. id=[{id}]", id);
            ApiMetrics.IncrementDataGet();

            var entity = await DataService.QueryDataAsync(id);
            if (entity is null)
            {
                return NotFound();
            }

            return Ok(Mapper.Map<DataGetResponse>(entity));
        }

        [HttpPost]
        public async ValueTask<IActionResult> Post(DataPostRequest request)
        {
            Log.LogInformation("Post data. id=[{id}]", request.Id);
            ApiMetrics.IncrementDataPost();

            if (!await DataService.InsertDataAsync(Mapper.Map<DataEntity>(request)))
            {
                return Conflict();
            }

            return Ok();
        }
    }
}
