using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagementMvc.Filters;

public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(
        ActionExecutingContext context)
    {
        var token = context.HttpContext.Session.GetString("Token");

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult(
                "Login",
                "Auth",
                null);
        }
    }
}