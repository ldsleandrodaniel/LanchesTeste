using Lanches.Areas.Admin.Services;
using Lanches.Context;
using Lanches.Models;
using Lanches.Repositories.Interfaces;
using Lanches.Repositories;
using Lanches.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    throw new Exception("❌ ConnectionString não configurada! Defina 'ConnectionStrings__DefaultConnection' no Render.");
}

// Configuração para o Render (REMOVA a configuração de UseUrls)
// if (!OperatingSystem.IsLinux()) // REMOVER ESTE BLOCO
// {
//     builder.WebHost.UseUrls("http://*:5000", "https://*:5001");
// }

// Configuração do PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connection, o => o.EnableRetryOnFailure()));

// Configuração de proxy para o Render
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configuração de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configurações de cookies para o Render
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Alterado para Always
    options.Cookie.HttpOnly = true;
});

// Configurações de serviços (mantenha os existentes)
builder.Services.Configure<ConfigurationImagens>(builder.Configuration
    .GetSection("ConfigurationPastaImagens"));

builder.Services.AddTransient<ILancheRepository, LancheRepository>();
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();
builder.Services.AddScoped<RelatorioVendasService>();
builder.Services.AddScoped<GraficoVendasService>();

builder.Services.AddAuthorization(options => {
    options.AddPolicy("Admin", politica => politica.RequireRole("Admin"));
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));
builder.Services.AddControllersWithViews();

builder.Services.AddPaging(options => {
    options.ViewName = "Bootstrap4";
    options.PageParameterName = "pageindex";
});

builder.Services.AddMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// REMOVA a configuração de HTTPS Redirection do builder
// builder.Services.AddHttpsRedirection(options => {
//     options.HttpsPort = 443;
// });

var app = builder.Build();

// Middleware de proxy DEVE vir primeiro
app.UseForwardedHeaders();

// Middleware para corrigir esquema
app.Use((context, next) => {
    if (context.Request.Headers["X-Forwarded-Proto"] == "https") {
        context.Request.Scheme = "https";
    }
    return next();
});

// Configuração do ambiente
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middlewares - ORDEM CORRETA
app.UseStaticFiles();
app.UseRouting();

// Aplicar migrations automaticamente
if (!app.Environment.IsDevelopment()) {
    using (var scope = app.Services.CreateScope()) {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}

CriarPerfisUsuarios(app);
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// COMENTE temporariamente o HTTPS Redirection
// app.UseHttpsRedirection();

// Configuração de rotas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "categoriaFiltro",
    pattern: "Lanche/{action}/{categoria?}",
    defaults: new { Controller = "Lanche", action = "List" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void CriarPerfisUsuarios(WebApplication app) {
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
    using (var scope = scopedFactory.CreateScope()) {
        var service = scope.ServiceProvider.GetService<ISeedUserRoleInitial>();
        service.SeedRoles();
        service.SeedUsers();
    }
}
