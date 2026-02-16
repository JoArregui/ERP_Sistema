using Microsoft.AspNetCore.Identity;

namespace ERP.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;
        
        // Relación con la Empresa para el Multi-tenancy profesional
        public int EmpresaId { get; set; }
        
        // Es buena práctica marcar la navegación como virtual para Lazy Loading si se usa
        public virtual Empresa? Empresa { get; set; }
    }
}