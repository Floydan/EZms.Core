using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EZms.Core
{
    public class EZmsContextFactory : IDesignTimeDbContextFactory<EZmsContext>
    {
        public EZmsContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<EZmsContext>();
            builder.UseSqlServer("Server=tcp:floydan-projects.database.windows.net,1433;Initial Catalog=rentals.ezms;Persist Security Info=False;User ID=floydan;Password=-^#AJ3}qE0LDzPzi;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new EZmsContext(builder.Options);
        }
    }
}
