using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GimmeMillions.Database
{
    public class SqliteGimmeMillionsContextFactory : IDesignTimeDbContextFactory<GimmeMillionsContext>
    {
        private string _connectionString = "DataSource=default.db";
        public SqliteGimmeMillionsContextFactory() { }
        public SqliteGimmeMillionsContextFactory(string connectionString) 
        {
            _connectionString = connectionString;
        }

        public GimmeMillionsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite("DataSource=default.db");
            return new GimmeMillionsContext(optionsBuilder.Options);
        }
    }
}
