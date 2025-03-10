using ApiGym.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ApiGym
{
    public class ApplicationDbContext:IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plan>()
               .Property(p => p.Precio)
               .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Pago>()
               .Property(p => p.monto)
               .HasColumnType("decimal(18,2)");


        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Plan> Plan { get; set; }
        public DbSet<Membresia> Membresias { get; set; }
        public DbSet<Pago> Pagos { get; set; }

    }
}
