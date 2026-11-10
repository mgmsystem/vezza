using Microsoft.AspNetCore.Mvc;

namespace Store.Api.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    //health-check
    [HttpGet("health-check")]
    public IActionResult Get()
    {
        return Ok();
    }
}