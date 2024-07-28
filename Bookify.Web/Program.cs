using Bookify.Web.Core.Mappings;
using Bookify.Web.Core.Settings;
using Bookify.Web.Core.Tasks;
using Bookify.Web.Data;
using Bookify.Web.Helpers;
using Bookify.Web.Helpers.Services;
using Bookify.Web.Seeds;
using Bookify.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Plugins;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container. 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString!));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews();
builder.Services.AddExpressiveAnnotations();

builder.Services.AddDataProtection().SetApplicationName(nameof(Bookify));

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));



builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequiredLength = 8;

    // Default SignIn settings.
    options.SignIn.RequireConfirmedEmail = true;

    // Default User settings.
    options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    options.User.RequireUniqueEmail = true;

    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
    options.Lockout.MaxFailedAccessAttempts = 3;
});



builder.Services.AddHangfire(c => c.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();


builder.Services.Configure<AuthorizationOptions>(option => option.AddPolicy("AdminOnly", p =>
{
    p.RequireAuthenticatedUser();
}));


builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    DashboardTitle = "Bookify Dashboard",
   // IsReadOnlyFunc = _ => true,
    Authorization = new[]{
        new DashboardAuthorizationFilter("AdminOnly")
   }
});

var objectFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
var serviceScope = objectFactory.CreateScope();

var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedAsync(roleManager);
await DefaultUsers.SeedAsync(userManager);


var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var emailSender = serviceScope.ServiceProvider.GetRequiredService<IEmailSender>();

var tasksHangfire = new TasksHangfire(dbContext, emailSender);

RecurringJob.AddOrUpdate("NotifyToRenewal", () => tasksHangfire.NotifayRenewAsync(), "0 13 * * *");
RecurringJob.AddOrUpdate("NotifyToReturnRental", () => tasksHangfire.ReturnRental(), "0 13 * * *");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();