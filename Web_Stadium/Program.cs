using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Web_Stadium.EFCore;
using Web_Stadium.EFCore;
using Web_Stadium.Hubs;

namespace Web_Stadium
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Đăng ký DbContext - liên kết EFCore với SQL Server
            builder.Services.AddDbContext<SanBongContext>(options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("ConnectedDb")
            //Đọc chuỗi keets nối từ file appsettings.json -> ConnectionStrings -> ConnectedDb
                )
            );

            // Đăng ký Repository vào Dependency Injection Container
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Cấu hình JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // ĐOẠN QUAN TRỌNG: Ép Server đọc Token từ Cookie "jwt"
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt"];
                        return Task.CompletedTask;
                    }
                };
            });

            // Đăng ký IConfiguration để dùng được trong _Layout.cshtml
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            //SignalR - Real-Time cập nhật khung giờ
            builder.Services.AddSignalR();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHostedService<Web_Stadium.End.MatchmakingAutoCleanupService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //Map SignalR Hub
            app.MapHub<SanBongHub>("/sanBongHub");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
