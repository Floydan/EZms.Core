using System;
using System.Linq;
using System.Threading.Tasks;
using EZms.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EZms.Core
{
    public class EZmsContext : IdentityDbContext
    {
        private const string SchemaName = "EZms";
        public EZmsContext(DbContextOptions<EZmsContext> options) : base(options)
        {

        }

        public DbSet<Content> Content { get; set; }
        public DbSet<ContentVersion> ContentVersions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(schema: SchemaName);

            modelBuilder.Entity<Content>()
                .HasIndex("ParentId", "UrlSlug")
                .IsUnique();

            modelBuilder.Entity<ContentVersion>();

            base.OnModelCreating(modelBuilder);
        }

        public bool Exists<T>(T entity) where T : class
        {
            return Set<T>().Local.Any(e => e == entity);
        }

        public async Task<int> SaveChangesAsync()
        {
            Audit();
            return await base.SaveChangesAsync();
        }

        private void Audit()
        {
            var entries = ChangeTracker.Entries().Where(x => x.Entity is IContentData && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((IContentData)entry.Entity).CreatedAt = DateTime.UtcNow;
                }
                ((IContentData)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
