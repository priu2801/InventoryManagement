using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using ServiceLayer.Implementation;
using ServiceLayer.Interface;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementAPI
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
            services.AddControllers();
            // JWT Token Generation from Server Side.
            services.AddMvc();
            //services.AddApiVersioning();
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;


                config.ApiVersionReader = new QueryStringApiVersionReader("api-version");
                SwaggerConfig.UseQueryStringApiVersion("api-version");

                //using Custom Request Header allows URLs to stay clean
                config.ApiVersionReader = new HeaderApiVersionReader("X-Version");
                SwaggerConfig.UseCustomHeaderApiVersion("X-Version");


                //For Accept header versioning
                config.ApiVersionReader = new MediaTypeApiVersionReader("v");
                SwaggerConfig.UseAcceptHeaderApiVersion("v");

            });

            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IProductDetailsService, ProductDetailsService>();
            services.AddTransient<ILoggerService, LoggerService>();
            // Enable Swagger 
            services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "JWT Token Authentication API",
                    Description = "ASP.NET Core 3.1 Web API"
                });

                swagger.SwaggerDoc("v2",
                    new OpenApiInfo
                    {
                        Version = "v2",
                        Title = "v2 API",
                        Description = "v2 API Description"
                    });


      
                swagger.OperationFilter<SwaggerParameterFilters>();
                swagger.DocumentFilter<SwaggerVersionMapping>();

                swagger.DocInclusionPredicate((version, desc) =>
                {
                    if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
                    var versions = methodInfo.DeclaringType.GetCustomAttributes(true).OfType<ApiVersionAttribute>().SelectMany(attr => attr.Versions);
                    var maps = methodInfo.GetCustomAttributes(true).OfType<MapToApiVersionAttribute>().SelectMany(attr => attr.Versions).ToArray();
                    version = version.Replace("v", "");
                    return versions.Any(v => v.ToString() == version && maps.Any(v => v.ToString() == version));
                });
                // To Enable authorization using Swagger (JWT)
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
            });

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])) //Configuration["JwtToken:SecretKey"]
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddLog4Net();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

           

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseAuthentication();
            // Swagger Configuration in API
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {

                options.DocumentTitle = "Test Title";
                options.SwaggerEndpoint($"/swagger/v1/swagger.json", $"v1");
                options.SwaggerEndpoint($"/swagger/v2/swagger.json", $"v2");
            });


        }
    }
}
