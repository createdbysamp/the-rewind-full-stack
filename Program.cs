using Microsoft.EntityFrameworkCore;
using TheRewind.Models;
using TheRewind.Services;

var builder = WebApplication.CreateBuilder(args);

// var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
var dbPassword = builder.Configuration["DbPassword"];
var connectionString =
    $"Server=localhost;port=3306;userid=root;password={dbPassword};database=the_rewind_movies_db;";

// Add services to the container.
builder.Services.AddControllersWithViews();

// service to get session to work
builder.Services.AddDistributedMemoryCache();

// session services
builder.Services.AddSession();

// password hashing services
builder.Services.AddScoped<IPasswordService, BcryptPasswordService>();

// db context services
builder.Services.AddDbContext<MovieContext>(
    (options) =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    app.UseStatusCodePagesWithReExecute("/error/{0}");
    app.UseHsts();
}
app.UseHttpsRedirection();

// app calls
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
