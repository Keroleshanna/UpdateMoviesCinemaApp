using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MoviesDashboard.Interfaces;
using MoviesDashboard.Models.Identity;
using MoviesDashboard.Persistence.Context;
using MoviesDashboard.Repositories;
using MoviesDashboard.Repositories.IRepositories;
using MoviesDashboard.Services;
using MoviesDashboard.Utility;


namespace MoviesDashboard
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDIBusiness(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.AddScoped<IAccountService, AccountService>();
            //services.AddScoped<IEmailSender, EmailSender>();


            return services;
        }
    }
}
