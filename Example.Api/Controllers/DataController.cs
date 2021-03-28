namespace Example.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]/[action]")]
    public class DataController : ControllerBase
    {
        [HttpGet]
        public IActionResult List()
        {
            return Ok();
        }
    }
}
