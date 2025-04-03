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
using Cinema_System.Areas.Request;
using Microsoft.AspNetCore.SignalR;

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
builder.Services.AddSignalR();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//builder.Services.AddDbContext<ApplicationDbContext>(u => u.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        b => b.MigrationsAssembly("Cinema.DataAccess") // ✅ Chỉ định dự án chứa Migration
//    ));


//-------------------- Configure database context ---------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging()  // Hiển thị giá trị tham số trong query
    .EnableDetailedErrors()       // Hiển thị thông báo lỗi chi tiết từ SQL Server
);

//-------------------------------------- SIGNAL IR   -------------------------------------------------
builder.Services.AddSignalR();


//------------------------------ Configure Identity ---------------------------
// Configure Identity with ApplicationUser
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireDigit = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireLowercase = false;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();


// Configure authentication cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
      // Cấu hình cookie
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie tồn tại 30 p
    options.SlidingExpiration = true; // Tự động gia hạn khi user active
});

// Configure Identity lockout policy
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
});


//// Add Facebook authentication
//builder.Services.AddAuthentication().AddFacebook(options =>
//{
//    options.AppId = "572726168935390";
//    options.AppSecret = "ef269c0c3efbd79bfae81afdcba26300";
//});
// Add authentication & authorization
builder.Services.AddAuthentication();
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy =>
//        policy.RequireRole(SD.Role_Admin));
//});
// Add Google authentication
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "1090292520927-n8hcmp4v0f4u1peg91j9mdadadjdl72u.apps.googleusercontent.com";
    options.ClientSecret = "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";
});

//------------------------------ Configure Session --------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure token lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromSeconds(30);
});

// Add scoped services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<UserManager<ApplicationUser>>();
//builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

//----------------------------------------------------------------------------------------
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Seed database
SeedDatabase();

app.MapRazorPages();
app.MapStaticAssets();
app.MapHub<CountdownHub>("/countdownHub");
//----------------------------------------- Class using SIGNAL IR( IN Utility) ---------------------------------------------

// Map SignalR hubs
app.MapHub<ChatHub>("/chatHub");
app.MapHub<SeatBookingHub>("/seatBookingHub");


// Configure routing for areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area=Guest}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Add middleware to handle role-based redirects
app.Use(async (context, next) =>
{
    // 1. Check if context.User and context.User.Identity are not null, and user is authenticated
    if (context.User?.Identity != null && context.User.Identity.IsAuthenticated)
    {
        var isAdmin = context.User.IsInRole(SD.Role_Admin);
        var path = context.Request.Path.ToString().ToLower();

        // 2. Skip redirect for static files, API calls, and Identity pages
        if (!path.StartsWith("/lib/") &&
            !path.StartsWith("/api/") &&
            !path.StartsWith("/identity/"))
        {
            // 3. Redirect Admin users to admin area if not already in admin path
            if (isAdmin && !path.StartsWith("/admin"))
            {
                context.Response.Redirect("/Admin/Users/Index");
                return;
            }
            // 4. Redirect non-Admin users out of admin path
            else if (!isAdmin && path.StartsWith("/admin"))
            {
                context.Response.Redirect("/Guest/Home/Index");
                return;
            }
        }
    }

    // 5. Continue to the next middleware
    await next();
});


// Add middleware to handle admin redirects
//app.Use(async (context, next) =>
//{
//    if (context.User.Identity.IsAuthenticated && 
//        context.User.IsInRole(SD.Role_Admin) && 
//        !context.Request.Path.StartsWithSegments("/Admin") &&
//        !context.Request.Path.StartsWithSegments("/Identity"))
//    {
//        context.Response.Redirect("/Admin/Home/Index");
//        return;
//    }
//    await next();
//});
app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var DbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        DbInitializer.Initialize();
    }
}
