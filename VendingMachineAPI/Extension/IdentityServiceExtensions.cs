using Microsoft.AspNetCore.Identity;
using VendingMachineAPI.Data;

namespace VendingMachineAPI.Extension
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config)
        {
            var builder = services.AddIdentityCore<IdentityUser>();

            builder = new IdentityBuilder(builder.UserType, builder.Services);
            builder.AddSignInManager<SignInManager<IdentityUser>>();
            builder.AddRoles<IdentityRole>();
            builder.AddEntityFrameworkStores<ApiContext>();

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("BUYER", policy => policy.RequireRole("BUYER"));
                opts.AddPolicy("SELLER", policy => policy.RequireRole("SELLER"));
            });

            return services;
        }
    }
}
