using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Rule200201202203Controller(MainRule200201202203Service mainService) : ControllerBase
{
    [HttpGet]
    public void Get()
    {
        mainService.Execute();
    }
}