using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DelFileController(DeleteFilesService delFileService) : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            delFileService.ExecuteAsync().Wait();
            return "Files deleted";
        }
    }
}