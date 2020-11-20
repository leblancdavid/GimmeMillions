using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using Microsoft.EntityFrameworkCore;

namespace GimmeMillions.Database
{
    public class GimmeMillionsContext : DbContext
    {
        public DbSet<FeatureVector> FeatureVectors { get; set; }
        public DbSet<StockHistory> StockHistories { get; set; }
        public DbSet<StockRecommendation> StockRecommendations { get; set; }

        public GimmeMillionsContext(DbContextOptions options)
            : base(options)
        { }

        public GimmeMillionsContext()
        { }

        //Uncomment this when you want to add a migration? Not sure why but it's apparently necessary
        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite("DataSource=default.db");

        //protected override void OnConfiguring(DbContextOptionsBuilder options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FeatureVector>()
                .Ignore(x => x.Data)
                .Property<string>("DataStr");

            modelBuilder.Entity<StockHistory>().Ignore(x => x.HistoricalData);

            modelBuilder.Entity<StockRecommendation>()
                .Ignore(x => x.PredictedPriceTarget)
                .Ignore(x => x.PredictedLowTarget);
        }



    }
}
