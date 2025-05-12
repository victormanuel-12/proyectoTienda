using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using proyectoTienda.Data;
using proyectoTienda.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromSeconds(300);
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});
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
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
