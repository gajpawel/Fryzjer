using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Fryzjer.Data;
using Fryzjer.Middleware;
using Fryzjer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Dodaje kontrolery API
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Czas wyga�ni�cia sesji
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<FryzjerContext>(options =>
    options.UseSqlite("Data Source=fryzjer.db"));

builder.Services.AddHttpContextAccessor(); // Dodana nowa linia do obs�ugi IHttpContextAccessor

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

//dodawnanie permisji wg folderu w kt�rym znajduje si� strona
var routePermissions = new Dictionary<string, PermissionController.UserTypes[]>
{
    { "/Clients", [PermissionController.UserTypes.Client, PermissionController.UserTypes.Admin] },
    { "/Hairdressers", [PermissionController.UserTypes.Hairdresser, PermissionController.UserTypes.Admin] },
    { "/Admin", [PermissionController.UserTypes.Admin] },
    { "/Admin/Services/AddService", new [] { PermissionController.UserTypes.Admin } }
};

//rejestracja permisji wykorzystuj�c middleware
foreach (var routePermission in routePermissions)
{
    app.UseWhen(context => context.Request.Path.StartsWithSegments(routePermission.Key), appBuilder =>
    {
        appBuilder.UsePermission(routePermission.Value);
    });
}

// Dodaj obs�ug� kontroler�w API
app.MapControllers(); // To umo�liwia obs�ug� endpoint�w kontroler�w API
app.MapRazorPages();

app.Run();