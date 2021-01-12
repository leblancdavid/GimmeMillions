using GimmeMillions.DataAccess.Clients.TDAmeritrade;
using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Authentication;
using GimmeMillions.Domain.Stocks;
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
            //services.AddScoped<IStockRecommendationRepository, SQLStockRecommendationRepository>();
            //services.AddScoped<IStockHistoryRepository, SQLStockHistoryRepository>();
            services.AddScoped<IStockRepository, DefaultStockRepository>();
            services.AddScoped<IRecommendationSystemProvider, EgyptianMauRecommendationSystemProvider>();
            services.AddScoped<IUserService, SQLUserRepository>();
            services.AddSingleton<IStockSymbolsRepository>(new StockSymbolsFile("Resources/nasdaq_screener.csv"));

            var ameritradeClient = new TDAmeritradeApiClient(configuration["TdApiKey"]);
            services.AddSingleton(ameritradeClient);
            services.AddScoped<IStockAccessService, TDAmeritradeStockAccessService>();

        }
    }
}
