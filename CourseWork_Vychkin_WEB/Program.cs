using CourseWork_Vychkin_WEB.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using CourseWork_Vychkin_WEB.Models.RouteConstraints;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<HousesDBContext>();

builder.Services.AddDbContext<HousesDBContext>(options =>
{
    //options.UseSqlServer(builder.Configuration.GetConnectionString("HousesDb"));
    options.UseSqlServer(builder.Configuration["HousesDB"]);
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["HouseContainerBlob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["HouseContainerQueue"], preferMsi: true);
});
builder.Services.AddAuthentication().AddGoogle(options=>
{
    //IConfigurationSection googlesection = configuration.GetSection("Authentication:Google");
    //options.ClientId = googlesection.GetValue<string>("ClientId");
    //options.ClientSecret = googlesection["ClientSecret"];
    options.ClientId = configuration.GetValue<string>("GoogleClientId");
    options.ClientSecret = configuration["GoogleClientSecret"];
    options.ClaimActions.MapJsonKey("picture", "picture", "url");
    options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/Signin"; // Путь к действию при отказе в доступе
    options.LoginPath = "/Account/Signin"; // Путь к странице входа
});
builder.Services.AddSession();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTSCONNECTIONSTRING"]);
//builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Profiles",
    pattern: "/Profile/{id?}",
    defaults: new { controller = "Users", action = "Details" },
    constraints: new { controller = new ControllerRouteConstraint("Users"), action = new ActionRouteConstraint("Details") }
);
app.MapControllerRoute(
    name: "EditHouse",
    pattern: "/House/Edit/{id?}",
    defaults: new { controller = "Houses", action = "Edit" },
    constraints: new { controller = new ControllerRouteConstraint("Houses"), action = new ActionRouteConstraint("Edit") }
);
app.MapControllerRoute(
    name: "DetailsHouse",
    pattern: "/House/{id?}",
    defaults: new { controller = "Houses", action = "Details" },
    constraints: new { controller = new ControllerRouteConstraint("Houses"),action=new ActionRouteConstraint("Details") }
);
app.MapControllerRoute(
    name: "Admin",
    pattern: "/Admin/{controller}/{action}",
    defaults: new { controller = "Home", action = "Index" },
    constraints: new { controller = new ForAdminPanelRouteConstraint() }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
