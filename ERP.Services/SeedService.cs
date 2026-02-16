using ERP.Data;
using ERP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ERP.Services
{
    public static class SeedService // <-- Asegúrate de que es static
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Lógica de creación de roles, empresa y usuario admin...
        }
    }
}