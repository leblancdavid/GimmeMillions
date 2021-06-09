using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Authentication;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.Domain.Stocks.Recommendations;
using GimmeMillions.WebApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GimmeMillions.WebApi
{
    public static class DIRegistration
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IStockRecommendationHistoryRepository, SQLStockRecommendationHistoryRepository>();
            services.AddScoped<IRecommendationSystemProvider, RecommendationSystemProvider>();
            services.AddScoped<IUserService, SQLUserRepository>();
            services.AddSingleton<IStockSymbolsRepository>(new StockSymbolsFile("Resources/nasdaq_screener.csv"));

            var ameritradeClient = new TDAmeritradeApiClient(configuration["TdApiKey"]);
            services.AddSingleton(ameritradeClient);
            services.AddScoped<IStockAccessService, TDAmeritradeStockAccessService>();

        }
    }
}
