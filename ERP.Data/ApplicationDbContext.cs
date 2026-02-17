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
        public DbSet<FamiliaArticulo> FamiliaArticulo { get; set; }
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

            // --- 1. FILTROS GLOBALES (Soft Delete / Seguridad) ---
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => c.IsActivo);
            modelBuilder.Entity<Articulo>().HasQueryFilter(a => !a.IsDescatalogado);
            modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.FechaBaja == null || e.FechaBaja > DateTime.Now);
            modelBuilder.Entity<Proveedor>().HasQueryFilter(p => p.IsActivo);

            // --- 2. CONFIGURACIÓN DE PRECISIÓN DECIMAL INDUSTRIAL ---
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

            // --- 3. RELACIONES Y ELIMINACIÓN EN CASCADA (Evitando Ciclos) ---

            modelBuilder.Entity<Vencimiento>()
                .HasOne(v => v.Empresa)
                .WithMany()
                .HasForeignKey(v => v.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

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

            // --- 4. RELACIONES COMERCIALES (Referential Integrity) ---
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
            
            // Relación Líneas -> Documento (Cascada manual para asegurar limpieza)
            modelBuilder.Entity<DocumentoLinea>()
                .HasOne(l => l.Documento)
                .WithMany(d => d.Lineas)
                .HasForeignKey(l => l.DocumentoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}