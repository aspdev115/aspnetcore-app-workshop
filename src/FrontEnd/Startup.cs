﻿using System;
using System.Net.Http;
using FrontEnd.Infrastructure;
using FrontEnd.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrontEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
                    {
                        options.Filters.AddService(typeof(RequireLoginFilter));
                    })
                    .AddRazorPagesOptions(options =>
                    {
                        options.AuthorizeFolder("/admin", "Admin");
                    });

            services.AddScoped<RequireLoginFilter>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = TwitterDefaults.AuthenticationScheme;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireAuthenticatedUser()
                          .RequireUserName(Configuration["admin"]);
                });
            });

            services.AddCookieAuthentication();
            services.AddTwitterAuthentication(options => Configuration.GetSection("twitter").Bind(options));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Configuration["serviceUrl"])
            };

            services.AddSingleton(httpClient);
            services.AddSingleton<IApiClient, ApiClient>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
