using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Rule206Controller(MainRule206Service mainService) : ControllerBase
{
    [HttpGet]
    public void Get()
    {
        mainService.Execute();
    }
}