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
        public DbSet<Familia> Familias { get; set; } // Corregido: Unificado con el controlador
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<ControlHorario> ControlesHorarios { get; set; }
        public DbSet<DocumentoComercial> Documentos { get; set; }
        public DbSet<DocumentoLinea> DocumentoLineas { get; set; }
        public DbSet<Vencimiento> Vencimientos { get; set; }
        public DbSet<Nomina> Nominas { get; set; }
        public DbSet<CierreCaja> CierresCaja { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- 1. FILTROS GLOBALES ---
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.IsActivo);
            modelBuilder.Entity<Articulo>().HasQueryFilter(a => !a.IsDescatalogado);
            modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.FechaBaja == null || e.FechaBaja > DateTime.Now);
            modelBuilder.Entity<Proveedor>().HasQueryFilter(p => p.IsActivo);
            modelBuilder.Entity<Familia>().HasQueryFilter(f => f.IsActiva); // Filtro para familias

            // --- 2. CONFIGURACIÓN DE PRECISIÓN DECIMAL ---
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

            // --- 3. RELACIONES Y CASCADA ---
            modelBuilder.Entity<Articulo>()
                .HasOne(a => a.Familia)
                .WithMany(f => f.Articulos)
                .HasForeignKey(a => a.FamiliaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vencimiento>()
                .HasOne(v => v.Empresa)
                .WithMany()
                .HasForeignKey(v => v.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DocumentoLinea>()
                .HasOne(l => l.Articulo)
                .WithMany()
                .HasForeignKey(l => l.ArticuloId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentoComercial>()
                .HasOne(d => d.Cliente)
                .WithMany()
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentoComercial>()
                .HasOne(d => d.Proveedor)
                .WithMany()
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<DocumentoLinea>()
                .HasOne(l => l.Documento)
                .WithMany(d => d.Lineas)
                .HasForeignKey(l => l.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}