using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartCVFilter.API.Controllers;

[AllowAnonymous]
public class TestController : Controller
{
    public IActionResult Index()
    {
        return Ok("Application is running successfully!");
    }
}
