using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            builder.Services.AddScoped<Web_Stadium.Services.EmailService>();

            var app = builder.Build();

            // Seed tài khoản demo (Admin/Owner/Staff/User) + sân mẫu nếu chưa có
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SanBongContext>();
                if (!db.Users.Any(u => u.VaiTro == "Admin"))
                {
                    string Hash(string p) => BCrypt.Net.BCrypt.HashPassword(p);
                    var now = DateTime.Now;
                    var admin = new Web_Stadium.EFCore.User { HoTen = "Admin", Email = "admin@demo.com", MatKhau = Hash("123456"), VaiTro = "Admin", IsActive = true, NgayTao = now };
                    var owner = new Web_Stadium.EFCore.User { HoTen = "Owner Demo", Email = "owner@demo.com", MatKhau = Hash("123456"), VaiTro = "Owner", IsActive = true, NgayTao = now };
                    var staff = new Web_Stadium.EFCore.User { HoTen = "Staff Demo", Email = "staff@demo.com", MatKhau = Hash("123456"), VaiTro = "Staff", IsActive = true, NgayTao = now };
                    var user = new Web_Stadium.EFCore.User { HoTen = "Người dùng", Email = "user@demo.com", MatKhau = Hash("123456"), VaiTro = "User", IsActive = true, NgayTao = now };
                    db.Users.AddRange(admin, owner, staff, user);
                    db.SaveChanges();

                    staff.OwnerIdCuaStaff = owner.Id;
                    db.SaveChanges();

                    var san = new Web_Stadium.EFCore.SanBong
                    {
                        TenSan = "Sân Bóng Demo", DiaChi = "123 Đường Demo", Quan = "Cầu Giấy", ThanhPho = "Hà Nội",
                        LoaiSan = "5", LoaiCo = "CO_NHAN_TAO", MoTa = "Sân demo cho kiểm thử",
                        TrangThaiDuyet = "DaDuyet", OwnerId = owner.Id, DaKyHopDong = true, TyLeCoc = 0.3m,
                        IsHidden = false, DanhGiaTrungBinh = 4.5, Latitude = 21.0285, Longitude = 105.8542
                    };
                    db.SanBongs.Add(san);
                    db.SaveChanges();

                    db.StaffSanPhanCongs.Add(new Web_Stadium.EFCore.StaffSanPhanCong { StaffId = staff.Id, SanBongId = san.Id, NgayGan = now });
                    db.KhungGios.Add(new Web_Stadium.EFCore.KhungGio
                    {
                        SanBongId = san.Id, GioBatDau = new TimeOnly(6, 0), GioKetThuc = new TimeOnly(8, 0),
                        Gia = 200000, GiaGioVang = 250000, GiaCuoiTuan = 220000, LoaiNgay = "Thuong", TrangThai = "Trong"
                    });
                    db.SaveChanges();
                }
            }

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
