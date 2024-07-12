using ASP_P15.Data;
using ASP_P15.Services.Hash;
using ASP_P15.Services.Kdf;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/* Місце для реєстрації служб - між створенням builder та його використанням (app) 
 * Реєстрація - співставлення інтерфейсу з класом за формулою 
 * "Буде запит на IHashService - видати об'єкт класу Md5HashService"
 */
// builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IHashService, ShaHashService>();
builder.Services.AddSingleton<IKdfService, Pbkdf1Service>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Реєструємо контекст даних
builder.Services.AddDbContext<DataContext>( options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LocalDb")
    )
);

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

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(   // маршрутизатор
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
