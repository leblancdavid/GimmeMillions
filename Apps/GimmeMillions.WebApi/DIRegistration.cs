using GimmeMillions.DataAccess.Clients.TDAmeritrade;
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
            services.AddScoped<IStockRecommendationRepository, SQLStockRecommendationRepository>();
            services.AddScoped<IStockHistoryRepository, SQLStockHistoryRepository>();
            services.AddScoped<IStockRepository, DefaultStockRepository>();
            services.AddScoped<IRecommendationSystemProvider, DonskoyRecommendationSystemProvider>();
            services.AddScoped<IUserService, SQLUserRepository>();

            var ameritradeClient = new TDAmeritradeApiClient(configuration["TdAccessFile"]);
            if(!ameritradeClient.RefreshAuthentication().IsSuccessStatusCode)
            {
                throw new Exception("Could not authenticate into the TD Ameritrade API");
            }

        }
    }
}
