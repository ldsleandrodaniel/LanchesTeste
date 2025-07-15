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

// ===============================================
// CONFIGURAÇÕES INICIAIS COM TRATAMENTO DE ERROS
// ===============================================

// Debug: Mostrar todas as configurações carregadas
Console.WriteLine("=== CONFIGURAÇÕES CARREGADAS ===");
Console.WriteLine(builder.Configuration.GetDebugView());

// Validação aprimorada da connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration["POSTGRES_CONNECTION_STRING"] 
        ?? throw new Exception("""
            ERRO: ConnectionString não configurada!
            Defina UMA dessas variáveis no Render:
            1. ConnectionStrings__DefaultConnection
            2. POSTGRES_CONNECTION_STRING
            Formato: Server=...;Port=5432;Database=...;User Id=...;Password=...;
            """);
}

// Configuração do PostgreSQL com resiliência
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, o => 
    {
        o.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Configuração de proxy para o Render
builder.Services.Configure<ForwardedHeadersOptions>(options => 
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configuração de Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configurações de cookies para o Render
builder.Services.ConfigureApplicationCookie(options => 
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
});

// Configurações de serviços
builder.Services.Configure<ConfigurationImagens>(builder.Configuration
    .GetSection("ConfigurationPastaImagens"));

builder.Services.AddTransient<ILancheRepository, LancheRepository>();
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();
builder.Services.AddScoped<RelatorioVendasService>();
builder.Services.AddScoped<GraficoVendasService>();

builder.Services.AddAuthorization(options => 
{
    options.AddPolicy("Admin", politica => politica.RequireRole("Admin"));
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));
builder.Services.AddControllersWithViews();

builder.Services.AddPaging(options => 
{
    options.ViewName = "Bootstrap4";
    options.PageParameterName = "pageindex";
});

builder.Services.AddMemoryCache();
builder.Services.AddSession(options => 
{
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// ===============================================
// MIDDLEWARE PIPELINE
// ===============================================

// Validação EXTRA da conexão com o banco
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        Console.WriteLine("Testando conexão com o banco...");
        if (!db.Database.CanConnect())
        {
            throw new Exception("Não foi possível conectar ao banco de dados");
        }
        Console.WriteLine("Conexão com o banco estabelecida com sucesso!");
    }
    catch (Exception ex)
    {
        throw new Exception("FALHA NA CONEXÃO COM O BANCO: " + ex.Message);
    }
}

// Middleware de proxy
app.UseForwardedHeaders();

// Middleware para corrigir esquema
app.Use((context, next) => 
{
    if (context.Request.Headers["X-Forwarded-Proto"] == "https") 
    {
        context.Request.Scheme = "https";
    }
    return next();
});

// Configuração do ambiente
if (app.Environment.IsDevelopment()) 
{
    app.UseDeveloperExceptionPage();
} 
else 
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middlewares - ORDEM CORRETA
app.UseStaticFiles();
app.UseRouting();

// Aplicar migrations automaticamente
if (!app.Environment.IsDevelopment()) 
{
    using (var scope = app.Services.CreateScope()) 
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRO NAS MIGRAÇÕES: {ex.Message}");
            throw;
        }
    }
}

// Rota de debug (remova em produção)
app.MapGet("/debug-config", () => 
    Results.Text(builder.Configuration.GetDebugView(), "text/plain"));

CriarPerfisUsuarios(app);
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

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

// ===============================================
// MÉTODOS AUXILIARES
// ===============================================

void CriarPerfisUsuarios(WebApplication app) 
{
    try
    {
        var scopedFactory = app.Services.GetService<IServiceScopeFactory>();
        using (var scope = scopedFactory.CreateScope()) 
        {
            var service = scope.ServiceProvider.GetService<ISeedUserRoleInitial>();
            service.SeedRoles();
            service.SeedUsers();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO AO CRIAR PERFIS: {ex.Message}");
    }
}
