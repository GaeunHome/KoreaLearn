using KoreanLearn.Data;
using KoreanLearn.Web.Infrastructure.Extensions;
using KoreanLearn.Web.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDataServices(builder.Configuration)
    .AddApplicationServices();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

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
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed data
await DbInitializer.InitializeAsync(app.Services);

app.Run();
