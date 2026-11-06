using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EsDb
{
    public class EsDbContextFactory : IDesignTimeDbContextFactory<EsDbContext>
    {
        public EsDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<EsDbContext>()
                .UseSqlServer("data source=(local);initial catalog=EsDb;TrustServerCertificate=true;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
                .Options;

            return new EsDbContext(options);
        }
    }
}
