using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BackEnd.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace BackEnd
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

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlite("Data Source=conferences.db");
			});

			services.AddControllers();
			services.AddGrpc();

			services.AddHealthChecks()
				.AddDbContextCheck<ApplicationDbContext>();

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "Conference Planner API", Version = "v1" });
				options.DescribeAllEnumsAsStrings();

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);

			});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();


			app.UseSwagger();

			app.UseSwaggerUI(options =>
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "Conference Planner API v1")
			);

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", context => {
					context.Response.Redirect("/swagger/");
					return Task.CompletedTask;
				});
				endpoints.MapGrpcService<GrpcSessionController>();
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health");
			});
		}
	}
}
