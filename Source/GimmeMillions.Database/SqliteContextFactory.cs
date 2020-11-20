using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GimmeMillions.Database
{
    public class SqliteContextFactory : IDesignTimeDbContextFactory<GimmeMillionsContext>
    {
        public GimmeMillionsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GimmeMillionsContext>();
            optionsBuilder.UseSqlite("DataSource=default.db");
            return new GimmeMillionsContext(optionsBuilder.Options);
        }
    }
}
