using System.Text;
using BoardCreate.DataContext;
using BoardCreate.Repositories;
using BoardCreate.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace BoardCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                });

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.AddServerHeader = false;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/User/Login";
                options.AccessDeniedPath = "/User/AccessDenied";
            });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder => builder.WithOrigins("http://localhost:5000")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader()
                                      .AllowCredentials());
            });

            builder.Services.AddControllersWithViews();

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<MemberDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDbContext<BoardSectionsDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDbContext<BoardDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDbContext<SectionTabsDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDbContext<CommentsDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDbContext<CommentsLikesDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddScoped<MessageService>();
            builder.Services.AddScoped<AdminService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<CookieService>();

            builder.Services.AddScoped<MemberRepository>(provider =>
                new MemberRepository(
                    provider.GetRequiredService<MemberDbContext>(),
                    provider.GetRequiredService<IHubContext<NotificationHub>>(),
                    provider.GetRequiredService<ILogger<MemberRepository>>(),
                    connectionString));

            builder.Services.AddScoped<AdminRepository>(provider =>
                new AdminRepository(
                    provider.GetRequiredService<BoardSectionsDbContext>(),
                    provider.GetRequiredService<SectionTabsDbContext>(),
                    provider.GetRequiredService<ILogger<AdminRepository>>(),
                    connectionString));

            builder.Services.AddScoped<BoardRepository>(provider =>
                 new BoardRepository(
                     provider.GetRequiredService<BoardDbContext>(),
                     provider.GetRequiredService<SectionTabsDbContext>(),
                     provider.GetRequiredService<IHubContext<NotificationHub>>(),
                     provider.GetRequiredService<ILogger<BoardRepository>>(),
                     connectionString));

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddLogging();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/User/Error");
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                if (!context.Response.Headers.ContainsKey("Content-Type"))
                {
                    context.Response.Headers.Add("Content-Type", "text/html; charset=utf-8");
                }
                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowSpecificOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
            app.UseCookiePolicy();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=User}/{action=Index}/{id?}");

            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();
        }
    }
}
