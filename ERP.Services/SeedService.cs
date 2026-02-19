using ERP.Data;
using ERP.Domain.Entities;
using ERP.Domain.Constants; // Importante para acceder a AppPermissions
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Necesario para los permisos
using System.Threading.Tasks;

namespace ERP.Services
{
    public static class SeedService
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. ASEGURAR ROLES DEL SISTEMA
            string[] roles = { "Admin", "Usuario", "Almacen", "Contabilidad" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. ASEGURAR EXISTENCIA DE EMPRESA (Requisito para ApplicationUser)
            var empresaPrincipal = await context.Empresas.FirstOrDefaultAsync();
            if (empresaPrincipal == null)
            {
                empresaPrincipal = new Empresa 
                { 
                    NombreComercial = "SISTEMA ERP GENERICO",
                    CIF = "B00000000",
                    FechaAlta = DateTime.Now,
                    IsActiva = true
                };
                context.Empresas.Add(empresaPrincipal);
                await context.SaveChangesAsync();
            }

            // 3. ASEGURAR USUARIO ADMINISTRADOR
            var adminEmail = "admin@erp.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrador del Sistema",
                    IsActivo = true,
                    EmailConfirmed = true,
                    EmpresaId = empresaPrincipal.Id,
                    UltimoAcceso = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    // Asignar Rol
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // --- ASIGNACIÓN DE PERMISOS (CLAIMS) ---
                    // Esto es lo que hace que el NavMenu se llene de opciones
                    var existingClaims = await userManager.GetClaimsAsync(adminUser);
                    foreach (var permission in AppPermissions.All)
                    {
                        if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                        {
                            await userManager.AddClaimAsync(adminUser, new Claim("Permission", permission));
                        }
                    }
                }
            }

            // 4. CONFIGURACIÓN INICIAL DE PARÁMETROS GENERALES
            if (!await context.ConfiguracionesGenerales.AnyAsync())
            {
                var configs = new List<ConfiguracionGeneral>
                {
                    new ConfiguracionGeneral { Clave = "SMTP_Server", Valor = "smtp.gmail.com", Descripcion = "Servidor de correo outgoing", UltimaModificacion = DateTime.Now },
                    new ConfiguracionGeneral { Clave = "SMTP_Port", Valor = "587", Descripcion = "Puerto SMTP TLS/SSL", UltimaModificacion = DateTime.Now },
                    new ConfiguracionGeneral { Clave = "SMTP_User", Valor = "tu-email@gmail.com", Descripcion = "Usuario para autenticación SMTP", UltimaModificacion = DateTime.Now },
                    new ConfiguracionGeneral { Clave = "SMTP_Pass", Valor = "tu-password", Descripcion = "Contraseña cifrada o de aplicación", UltimaModificacion = DateTime.Now },
                    new ConfiguracionGeneral { Clave = "Empresa_Logo", Valor = "/img/logo.png", Descripcion = "Ruta virtual del logo de la empresa", UltimaModificacion = DateTime.Now }
                };

                await context.ConfiguracionesGenerales.AddRangeAsync(configs);
                await context.SaveChangesAsync();
            }
        }
    }
}