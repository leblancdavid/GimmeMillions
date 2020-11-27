using GimmeMillions.DataAccess.Stocks;
using GimmeMillions.Database;
using GimmeMillions.Domain.Authentication;
using GimmeMillions.Domain.Stocks;
using GimmeMillions.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GimmeMillions.WebApi
{
    public static class DIRegistration
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IStockRecommendationRepository, SQLStockRecommendationRepository>();
            services.AddScoped<IStockHistoryRepository, SQLStockHistoryRepository>();
            services.AddScoped<IStockRepository, DefaultStockRepository>();
            services.AddScoped<IRecommendationSystemProvider, DonskoyRecommendationSystemProvider>();
            services.AddScoped<IUserService, SQLUserRepository>();

        }
    }
}
