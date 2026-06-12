using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.Controllers;
using Web_Stadium.EFCore;

namespace Web_Stadium.Filters
{
    /// <summary>
    /// Filter kiểm tra đăng nhập + role + IsActive mỗi request.
    /// Khi Admin khóa tài khoản, người dùng bị đá ra ngay lần request tiếp theo.
    /// </summary>
    public class YeuCauDangNhapAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _roles;

        // Không tham số — yêu cầu đăng nhập, chấp nhận mọi role
        public YeuCauDangNhapAttribute() => _roles = Array.Empty<string>();

        // Có tham số — yêu cầu role cụ thể (phân cách bằng dấu phẩy)
        // VD: [YeuCauDangNhap("Admin")] hoặc [YeuCauDangNhap("User,Owner")]
        public YeuCauDangNhapAttribute(string roles)
            => _roles = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            var dbContext = httpContext.RequestServices.GetRequiredService<SanBongContext>();

            // 1. Lấy JWT cookie
            var token = httpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                Redirect(context, httpContext);
                return;
            }

            // 2. Giải mã token
            var principal = TokenHelper.DocToken(token, config);
            if (principal == null)
            {
                Redirect(context, httpContext);
                return;
            }

            // 3. Lấy UserId từ claims
            var userIdStr = principal.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
            {
                Redirect(context, httpContext);
                return;
            }

            // 4. ✅ CHECK ISACTIVE MỖI REQUEST — khi Admin khóa TK, bị đá ra ngay
            var user = await dbContext.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.VaiTro, u.IsActive })
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || !user.IsActive)
            {
                // Xóa cookie JWT
                httpContext.Response.Cookies.Delete("jwt");
                Redirect(context, httpContext);
                return;
            }

            // 5. Kiểm tra role
            if (_roles.Length > 0 && !_roles.Contains(user.VaiTro))
            {
                // Có đăng nhập nhưng sai role → redirect về trang chủ role của mình
                var redirectUrl = user.VaiTro switch
                {
                    "Admin" => "/Admin",
                    "Owner" => "/Owner",
                    "Staff" => "/Staff",
                    _ => "/"
                };
                context.Result = new RedirectResult(redirectUrl);
                return;
            }

            await next();
        }

        private static void Redirect(ActionExecutingContext context, HttpContext httpContext)
        {
            var returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;
            context.Result = new RedirectResult($"/Auth/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
    }
}