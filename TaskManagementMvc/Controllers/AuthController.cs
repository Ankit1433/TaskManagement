using Microsoft.AspNetCore.Mvc;
using TaskManagementMvc.Models.ApiModels.Auth;
using TaskManagementMvc.Services.Interfaces;

namespace TaskManagementMvc.Controllers;

public class AuthController : Controller
{
    private readonly IApiClient _apiClient;

    public AuthController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var response =
                await _apiClient.PostAsync<LoginRequest, LoginResponse>(
                    "api/auth/login",
                    request);

            if (response == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(request);
            }

            HttpContext.Session.SetString(
                "Token",
                response.Token);

            HttpContext.Session.SetInt32(
                "UserId",
                response.UserId);

            HttpContext.Session.SetString(
                "Name",
                response.Name);

            HttpContext.Session.SetString(
                "Role",
                response.Role);

            return RedirectToAction(
                "Index",
                "Home");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "",
                "Unable to connect to API.");

            return View(request);
        }
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }
}