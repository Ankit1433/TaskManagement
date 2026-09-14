using Microsoft.AspNetCore.Mvc;

namespace TaskManagementMvc.Controllers;

public class ErrorController : Controller
{
    [HttpGet]
    public IActionResult Api(int statusCode, string message)
    {
        ViewBag.StatusCode = statusCode;
        ViewBag.Message = message;

        return View();
    }
}