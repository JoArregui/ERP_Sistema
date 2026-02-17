using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ERP.Data;
using ERP.Domain.Entities;
using ERP.Services;
using ERP.API.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE BASE DE DATOS ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 2. CONFIGURACIÓN DE IDENTITY ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- 3. CONFIGURACIÓN DE SEGURIDAD JWT ---
var jwtSecret = builder.Configuration["JWT:Secret"] ?? "Clave_Super_Secreta_De_Prueba_2026_ERP";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero 
    };
});

// --- 4. POLÍTICA DE CORS (CORREGIDA) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5053") // Puerto de tu ERP.Web
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // <--- ELIMINADO EL "Any", ahora es correcto
    });
});

// --- 5. REGISTRO DE SERVICIOS DE NEGOCIO ---
builder.Services.AddScoped<NominaService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<CicloFacturacionService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<ComprasService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- 6. SWAGGER PROFESIONAL ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ERP Profesional API", 
        Version = "v1",
        Description = "Backend Industrial para Gestión de Empresas, Facturación y Recursos Humanos."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingresa el token JWT: Bearer {tu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- 7. PROCESO DE SEMILLADO (SEEDING) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();
        await SeedService.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error crítico durante la migración o el semillado.");
    }
}

// --- 8. CONFIGURACIÓN DEL PIPELINE (MIDDLEWARE) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP API v1"));
}

app.UseHttpsRedirection();

// El CORS debe ir antes de Authentication y Authorization
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();