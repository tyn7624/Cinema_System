using Cinema.DataAccess.Data;
using Cinema.DataAccess.Repository.IRepository;
using Cinema.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Cinema.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Cinema.Utility;
using Cinema.DbInitializer;
using Cinema.DataAccess.DbInitializer;
using Net.payOS;
using Cinema_System.Areas.Service;


var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

PayOS payOs = new PayOS(
    configuration["PayOs:ClientId"] ?? throw new Exception("Cannot find environment"),
    configuration["PayOs:ApiKey"] ?? throw new Exception("Cannot find environment"),
    configuration["PayOs:CheckSumKey"] ?? throw new Exception("Cannot find environment"));

builder.Services.AddSingleton(payOs);

// Đăng ký PayOSService vào DI container
builder.Services.AddScoped<PayOSService>();  // Hoặc AddSingleton nếu bạn muốn dùng singleton

// Đăng ký các dịch vụ khác
builder.Services.AddControllersWithViews();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(u => u.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        b => b.MigrationsAssembly("Cinema.DataAccess") // ✅ Chỉ định dự án chứa Migration
//    ));

builder.Services.AddDbContext<ApplicationDbContext>(u => u.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
//options => options.SignIn.RequireConfirmedAccount = true
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

SeedDatabase();

app.MapRazorPages();
app.MapStaticAssets();



// ------------------- ROUTING CHO AREAS ------------------- //
// 1) Route cho Admin
//    Khi URL bắt đầu bằng "/Admin/...",
//    MVC sẽ tìm controller trong Area = "Admin".
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Guest}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{area=Admin}/{controller=Users}/{action=Index}/{id?}")
//    .WithStaticAssets();

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var DbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        DbInitializer.Initialize();
    }
}


