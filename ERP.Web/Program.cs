using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using ERP.Web;
using ERP.Web.Services;
using ERP.Domain.Constants;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- 1. CONFIGURACIÓN DE COMUNICACIÓN CON EL API ---
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5109/") 
});

// --- 2. SISTEMA DE AUTORIZACIÓN Y POLÍTICAS DINÁMICAS ---
builder.Services.AddAuthorizationCore(options =>
{
    foreach (var permission in AppPermissions.All)
    {
        options.AddPolicy(permission, policy => 
            policy.RequireClaim("Permission", permission));
    }
});

// Registro del Proveedor de Estado de Autenticación
builder.Services.AddScoped<CustomAuthenticationProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthenticationProvider>());

// --- 3. REGISTRO DE SERVICIOS WEB ---
builder.Services.AddScoped<AuthService>(); // <-- NUEVO SERVICIO REGISTRADO
builder.Services.AddScoped<ComprasWebService>();
builder.Services.AddScoped<NotificationService>();

// --- 4. SERVICIO DE TESORERÍA ---
builder.Services.AddScoped<TesoreriaWebService>();

// --- 5. SERVICIO DE EMPLEADOS ---
builder.Services.AddScoped<EmpleadoService>();

await builder.Build().RunAsync();