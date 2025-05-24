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
    // En desarrollo, usa URLs fijos con HTTPS y HTTP
    builder.WebHost.UseUrls("https://localhost:7027", "http://localhost:5017");
}
else
{
    // En producción (Render) solo HTTP en el puerto asignado por la variable de entorno PORT
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    builder.WebHost.UseUrls($"http://*:{port}");
}



// Configurar servicios de Mercado Pago
builder.Services.AddScoped<CardTokenClient>();
builder.Services.AddScoped<CustomerClient>();
builder.Services.AddScoped<PaymentClient>();

//Substack
builder.Services.AddScoped<SubstackService>();


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
.AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICarritoService, CarritoService>();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient(); // <- Aquí


// API Documentation
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
app.UseSession();


app.UseRouting();
app.UseHttpsRedirection();
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
