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
using PresentationLayer.Data;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

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
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "PLACEHOLDER";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "PLACEHOLDER";
    });

builder.Services.AddRazorPages();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DataSeeder.SeedRolesAndUsersAsync(services);
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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireAuthorization();
app.MapRazorPages();

app.Run();
