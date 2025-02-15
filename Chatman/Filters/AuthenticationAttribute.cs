using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chatman.Filters
{
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        // 允許匿名訪問的頁面和 API
        private readonly Dictionary<string, string[]> _allowedRoutes = new()
        {
            { "User", new[] { "Login", "Register", "UserLogin" } }  // UserLogin 是 API endpoint
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // 檢查是否為允許匿名訪問的路由
            if (controller != null && _allowedRoutes.ContainsKey(controller))
            {
                if (_allowedRoutes[controller].Contains(action))
                {
                    return;
                }
            }

            // 獲取 token
            var token = context.HttpContext.Request.Cookies["auth_token"];

            if (string.IsNullOrEmpty(token))
            {
                // 如果是 API 請求，返回 401
                if (IsApiRequest(context.HttpContext.Request))
                {
                    context.Result = new UnauthorizedResult();
                }
                else
                {
                    // 如果是頁面請求，重定向到登入頁
                    context.Result = new RedirectToActionResult("Login", "User", null);
                }
            }
        }

        private bool IsApiRequest(HttpRequest request)
        {
            // 檢查是否為 API 請求的邏輯
            return request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers["Accept"].ToString().Contains("application/json") == true ||
                   request.Path.ToString().StartsWith("/api/");
        }
    }
}