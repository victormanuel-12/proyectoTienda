using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Data;
using Microsoft.OpenApi.Models;
using proyectoTienda.Servicios;

using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Client.CardToken;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Customer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromSeconds(300);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

if (env == "Development")
{
  builder.WebHost.UseUrls("https://localhost:7027", "http://localhost:5017");
}
else
{
  var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
  builder.WebHost.UseUrls($"http://*:{port}");
}

// Mercado Pago
builder.Services.AddScoped<CardTokenClient>();
builder.Services.AddScoped<CustomerClient>();
builder.Services.AddScoped<PaymentClient>();

// Substack
builder.Services.AddHttpClient<SubstackService>();

// Base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Servicios
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "API",
    Version = "v1",
    Description = "Descripción de la API"
  });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseHttpsRedirection();
}
else
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseSession();
app.UseRouting();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
});

app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();