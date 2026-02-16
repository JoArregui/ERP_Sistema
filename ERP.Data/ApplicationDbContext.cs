using Microsoft.EntityFrameworkCore;
using ERP.Domain.Entities;

namespace ERP.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<ControlHorario> ControlesHorarios { get; set; }
        public DbSet<DocumentoComercial> Documentos { get; set; }
        public DbSet<DocumentoLinea> DocumentoLineas { get; set; }
        public DbSet<Vencimiento> Vencimientos { get; set; }
        public DbSet<Nomina> Nominas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. FILTROS GLOBALES ---
            // Estos filtros ocultan registros "borrados" o "inactivos" automáticamente en todo el CMS
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.IsActivo);
            
            modelBuilder.Entity<Articulo>().HasQueryFilter(a => !a.IsDescatalogado);
            
            modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.FechaBaja == null || e.FechaBaja > DateTime.Now);
            
            modelBuilder.Entity<Proveedor>().HasQueryFilter(p => p.IsActivo);

            // --- 2. CONFIGURACIÓN DE PRECISIÓN DECIMAL ---
            // Aplica 18,4 a todos los campos decimales para evitar redondeos en contabilidad
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

                foreach (var property in properties)
                {
                    property.SetPrecision(18);
                    property.SetScale(4);
                }
            }

            // --- 3. RESOLUCIÓN DE WARNINGS DE RELACIÓN (ISREQUIRED = FALSE) ---
            // Al tener filtros globales, las relaciones deben ser opcionales para que EF Core
            // no falle si un registro hijo apunta a un padre que está oculto por el filtro.
            
            modelBuilder.Entity<ControlHorario>()
                .HasOne(c => c.Empleado)
                .WithMany()
                .HasForeignKey(c => c.EmpleadoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Nomina>()
                .HasOne(n => n.Empleado)
                .WithMany()
                .HasForeignKey(n => n.EmpleadoId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentoLinea>()
                .HasOne(l => l.Articulo)
                .WithMany()
                .HasForeignKey(l => l.ArticuloId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // --- 4. RELACIONES COMERCIALES ---
            modelBuilder.Entity<DocumentoComercial>()
                .HasOne(d => d.Cliente)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentoComercial>()
                .HasOne(d => d.Proveedor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}