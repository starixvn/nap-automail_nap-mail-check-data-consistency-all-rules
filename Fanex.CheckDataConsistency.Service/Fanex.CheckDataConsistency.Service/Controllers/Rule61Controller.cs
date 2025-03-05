using Fanex.CheckDataConsistency.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Rule61Controller(MainRule61Service service) : ControllerBase
{
    [HttpGet]
    public void Get()
    {
        service.Execute();
    }
}