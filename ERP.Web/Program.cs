using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ERP.Web;
using ERP.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// --- 1. CONFIGURACIÓN DE COMUNICACIÓN CON EL API ---
// Apuntamos al puerto 5109 donde el API está escuchando
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5109/") 
});

// --- 2. REGISTRO DE SERVICIOS WEB ---
// Registramos el servicio de Compras
builder.Services.AddScoped<ComprasWebService>();

// REGISTRO CRÍTICO: Este es el que falta y causa el Spinner infinito
// Permite que MainLayout y otros componentes muestren alertas y notificaciones
builder.Services.AddScoped<NotificationService>();

// --- 3. INICIO DE LA APLICACIÓN ---
await builder.Build().RunAsync();