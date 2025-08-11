using FoxholeMap.Controllers;
using FoxholeMap.DataBase;
using Microsoft.EntityFrameworkCore;

using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlite("Data Source=DataBase/Users.db"));

builder.Services.AddDbContext<ChatsDbContext>(options =>
    options.UseSqlite("Data Source=DataBase/Chats.db"));

// Добавляем Quartz
builder.Services.AddQuartz(q =>
{
    // Регистрируем задание
    q.ScheduleJob<ChatReEncryptionJob>(trigger => trigger
        .WithIdentity("ChatReEncryptionTrigger")
        .WithCronSchedule("0 * * * * ?")); // каждый час
});

// Добавляем Quartz HostedService (фон)
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
