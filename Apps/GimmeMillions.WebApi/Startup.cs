using GimmeMillions.Database;
using GimmeMillions.Domain.Authentication;
using GimmeMillions.WebApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GimmeMillions.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();

            var connectionString = Configuration["DbConnectionString"];
            services.AddDbContext<GimmeMillionsContext>(opt =>
            {
                opt.UseSqlite(connectionString);
                var context = new GimmeMillionsContext(opt.Options);
                context.Database.Migrate();
            });

            DIRegistration.RegisterServices(services, Configuration);

            // configure basic authentication 
            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, 
                BasicAuthenticationHandler>("BasicAuthentication", null);

            var provider = services.BuildServiceProvider();
            //Setup default super user
            var userService = provider.GetService<IUserService>();
            if (!userService.UserExists("gm_superuser"))
            {
                userService.AddOrUpdateUser(new User(
                    "gm_superuser",
                    "gm_superuser", 
                    "gm_superuser", 
                    "gm_superuser",
                    UserRole.SuperUser));
            }


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseHttpsRedirection();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
