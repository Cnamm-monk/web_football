using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Web_Stadium.Controllers;

namespace Web_Stadium.Filters
{
    // Attribute để gắn vào Controller để yêu cầu đăng nhập
    // Dùng không tham số: [YeuCauDangNhap]
    // Dùng có tham số: [YeuCauDangNhap("Admin")]
    public class YeuCauDangNhapAttribute : ActionFilterAttribute
    {
        private readonly string? _vaiTro;

        public YeuCauDangNhapAttribute(string? vaiTro = null)
        {
            _vaiTro = vaiTro;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>();
            var token = context.HttpContext.Request.Cookies["jwt"];

            // Chưa đăng nhập -> chuyển về login
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var principal = TokenHelper.DocToken(token, config);

            // Token hết hạn hoặc không hợp lệ -> chuyển về login
            if (principal == null)
            {
                context.HttpContext.Response.Cookies.Delete("jwt");
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Kiểm tra vai trò nếu có yêu cầu
            if (_vaiTro != null)
            {
                var vaiTroHienTai = principal
                    .FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (vaiTroHienTai != _vaiTro)
                {
                    context.Result = new RedirectToActionResult("Index", "Home", null);
                    return;
                }
            }
        }
    }
}
