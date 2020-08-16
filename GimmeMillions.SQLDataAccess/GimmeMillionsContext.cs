using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using Microsoft.EntityFrameworkCore;

namespace GimmeMillions.SQLDataAccess
{
    public class GimmeMillionsContext : DbContext
    {
        public DbSet<FeatureVector> FeatureVectors { get; set; }
        public DbSet<StockData> StockDatas { get; set; }

        public GimmeMillionsContext(DbContextOptions<GimmeMillionsContext> options)
            : base(options)
        { }

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

    }
}
