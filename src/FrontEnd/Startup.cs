﻿using System;
using System.Net.Http;
using FrontEnd.Infrastructure;
using FrontEnd.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
                options.Filters.AddService<RequireLoginFilter>();
            })
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AuthorizeFolder("/Admin", "Admin");
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddTransient<RequireLoginFilter>();

            var authBuilder =  services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/Denied";
                });

            var twitterConfig = Configuration.GetSection("twitter");
            if (twitterConfig["consumerKey"] != null)
            {
                authBuilder.AddTwitter(options => twitterConfig.Bind(options));
            }

            var googleConfig = Configuration.GetSection("google");
            if (googleConfig["clientID"] != null)
            {
                authBuilder.AddGoogle(options => googleConfig.Bind(options));
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireAuthenticatedUser()
                          .RequireUserName(Configuration["admin"]);
                });
            });

            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration["serviceUrl"]);
            });
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

            app.UseHsts();

            app.UseStatusCodePagesWithReExecute("/Status/{0}");

            app.UseHttpsRedirection();

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
