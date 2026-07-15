using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.Interfaces;
using ServiceLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using PresentationLayer.Seeders;
using PresentationLayer.Hubs;
using DotNetEnv;

Env.Load();
var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages ─────────────────────────────────────────
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

// ── DbContext ─────────────────────────────────────────
builder.Services.AddDbContext<ChatbotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity & Auth ───────────────────────────────────
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ChatbotDbContext>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var clientId = builder.Configuration["Authentication:Google:ClientId"];
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        
        options.ClientId = string.IsNullOrEmpty(clientId) ? "PLACEHOLDER" : clientId;
        options.ClientSecret = string.IsNullOrEmpty(clientSecret) ? "PLACEHOLDER" : clientSecret;
    });

builder.Services.AddRazorPages(options =>
{
    // Chặn trang đăng ký — chỉ Admin mới vào được (mọi người khác bị redirect)
    options.Conventions.AuthorizePage("/Account/Register", "Admin");
});

// ── Repositories (DAL) ───────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();

// ── Services (BLL) ───────────────────────────────────
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();
builder.Services.AddScoped<ITextExtractorService, TextExtractorService>();
builder.Services.AddScoped<IChunkingService, ChunkingService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeeder.SeedRolesAndUsersAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
// ── Pipeline ─────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages().RequireAuthorization();
app.MapHub<ChatHub>("/chatHub");

app.Run();
