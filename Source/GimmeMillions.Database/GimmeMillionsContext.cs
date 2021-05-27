using GimmeMillions.Domain.Authentication;
using GimmeMillions.Domain.Features;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace GimmeMillions.Database
{
    public class GimmeMillionsContext : DbContext
    {
        public DbSet<StockRecommendation> LastRecommendations { get; set; }
        public DbSet<StockRecommendationHistory> RecommendationHistories { get; set; }
        public DbSet<User> Users { get; set; }

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
            modelBuilder.Entity<StockRecommendationHistory>().Ignore(x => x.HistoricalData);

            modelBuilder.Entity<StockRecommendation>()
                .Ignore(x => x.PredictedPriceTarget)
                .Ignore(x => x.PredictedLowTarget);

        }



    }
}
