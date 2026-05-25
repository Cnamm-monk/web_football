using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Web_Stadium.EFCore;
using Web_Stadium.Hubs;

namespace Web_Stadium
{
    public class Program
    {

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                System.IO.File.WriteAllText(
                    Path.Combine(Directory.GetCurrentDirectory(), "crash_log.txt"),
                    e.ExceptionObject?.ToString() ?? "unknown error"
                );
            };
            // Bắt lỗi trên Task/async (quan trọng!)
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                System.IO.File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "crash_log.txt"),
                    "UnobservedTaskException:\n" + e.Exception?.ToString());
                e.SetObserved();
            };

            var builder = WebApplication.CreateBuilder(args);

            // Thêm middleware bắt lỗi toàn bộ request
            builder.Services.AddExceptionHandler(options =>
            {
                options.ExceptionHandlingPath = "/error";
            });

            

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

            builder.Services.AddHttpClient();
            // Đăng ký IConfiguration để dùng được trong _Layout.cshtml
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            //SignalR - Real-Time cập nhật khung giờ
            builder.Services.AddSignalR();
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //builder.Services.AddHostedService<Web_Stadium.End.MatchmakingAutoCleanupService>();
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 52428800;
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 52428800;
            });
            // Thêm trước builder.Build()
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 52428800; // 50MB
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 52428800; // 50MB
            });
            var app = builder.Build();
            // Thêm ngay sau var app = builder.Build();
            app.Use(async (context, next) =>
            {
                try { await next(); }
                catch (Exception ex)
                {
                    await System.IO.File.WriteAllTextAsync(@"D:\crash_log.txt",
                        $"[{DateTime.Now}] {context.Request.Path}\n{ex}");
                    throw;
                }
            });
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
