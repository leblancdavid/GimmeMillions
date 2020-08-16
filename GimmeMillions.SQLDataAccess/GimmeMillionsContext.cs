using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GimmeMillions.SQLDataAccess
{
    public class GimmeMillionsContext : DbContext
    {
        private readonly string _connectionString;
        public GimmeMillionsContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<FeatureVector> FeatureVectors { get; set; }
        public DbSet<StockData> StockDatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeatureVector>()
                .Ignore(x => x.Data)
                .Property<string>("DataStr");

            modelBuilder.Entity<StockData>()
                .Ignore(x => x.BottomWickPercent)
                .Ignore(x => x.CMF)
                .Ignore(x => x.PercentChangeFromPreviousClose)
                .Ignore(x => x.PercentChangeHighToOpen)
                .Ignore(x => x.PercentChangeHighToPreviousClose)
                .Ignore(x => x.PercentChangeLowToPreviousClose)
                .Ignore(x => x.PercentChangeOpenToPreviousClose)
                .Ignore(x => x.PercentDayChange)
                .Ignore(x => x.PercentHighToLow)
                .Ignore(x => x.TopWickPercent);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(_connectionString);
    }
}
