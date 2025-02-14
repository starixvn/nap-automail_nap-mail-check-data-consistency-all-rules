using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Rule207Controller(MainRule207Service mainService) : ControllerBase
{
    [HttpGet]
    public void Get()
    {
        mainService.Execute();
    }
}