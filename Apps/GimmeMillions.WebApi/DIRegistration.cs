using GimmeMillions.Database;
using GimmeMillions.Domain.Stocks;
using Microsoft.Extensions.DependencyInjection;

namespace GimmeMillions.WebApi
{
    public static class DIRegistration
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IStockRecommendationRepository, SQLStockRecommendationRepository>();
        }
    }
}
