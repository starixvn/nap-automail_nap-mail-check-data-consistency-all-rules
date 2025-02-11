using Fanex.CheckDataConsistency.Business.Services;
using Fanex.MailDelivery.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Rule4849Controller(MainRule4849Service service) : ControllerBase
{
    [HttpGet]
    public JobMessage Get()
    {
        var result = service.Execute();

        return result;
    }
}