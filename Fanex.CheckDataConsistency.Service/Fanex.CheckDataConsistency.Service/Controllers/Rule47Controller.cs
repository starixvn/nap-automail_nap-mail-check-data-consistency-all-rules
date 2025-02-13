using Fanex.CheckDataConsistency.Business.Services;
using Fanex.MailDelivery.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.CheckDataConsistency.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class Rule47Controller(MainRule47Service service) : ControllerBase
{
    [HttpGet]
    public JobMessage Get()
    {
        return service.Execute();
    }
}