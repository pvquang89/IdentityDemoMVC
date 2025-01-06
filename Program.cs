using IdentityDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



//Configuration Identity Services
//AddIdentity() : phương thức dùng để cấu hình và thiết lập identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    //cấu hình tuỳ chỉnh yêu cầu password
    options.Password.RequireNonAlphanumeric = false;     // Không cần ký tự đặc biệt
    options.Password.RequireUppercase = false;           // Không cần chữ hoa
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure the Application Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    //Mặc định nếu ko đặt giá trị LoginPath là action Login trong Account controller
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

//Configure Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("SQLServerIdentityConnection") ?? throw new InvalidOperationException("Connection string 'SQLServerIdentityConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));



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

//Configuring Authentication Middleware to the Request Pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
