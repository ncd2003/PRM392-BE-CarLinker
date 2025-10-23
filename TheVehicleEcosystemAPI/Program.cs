using BusinessObjects;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using System.Security.Claims;
using System.Text;
using TheVehicleEcosystemAPI.Utils;
using TheVehicleEcosystemAPI.Security;
using TheVehicleEcosystemAPI.Middleware;

namespace TheVehicleEcosystemAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Register DbContext
            builder.Services.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
            );

            // Register DAOs
            builder.Services.AddScoped<VehicleDAO>();
            builder.Services.AddScoped<UserDAO>();
            builder.Services.AddScoped<CartDAO>();
            builder.Services.AddScoped<OrderDAO>();
            builder.Services.AddScoped<ProductDAO>();

            // Register Repositories
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            // Register JWT Configuration
            builder.Services.AddScoped<JWTConfiguration>();
            
            // Mapster
            ConfigureMapster.ConfigureMappings();

            // 1. Thêm dịch vụ Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // 2. Cấu hình JWT Bearer
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            // Configure Swagger with JWT Bearer authentication
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "The Vehicle Ecosystem API",
                    Version = "v1",
                    Description = "API for Vehicle Ecosystem Management System with JWT Authentication",
                    Contact = new OpenApiContact
                    {
                        Name = "Vehicle Ecosystem Team",
                        Email = "support@vehicleecosystem.com"
                    }
                });

                // Define the Bearer security scheme
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = @"JWT Authorization header using the Bearer scheme.
                    
Enter your token in the text input below.
Example: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'

**Note:** You don't need to add 'Bearer' prefix, it will be added automatically."
                });

                // Make sure swagger UI requires Bearer token specified above
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "The Vehicle Ecosystem API v1");
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    options.DefaultModelsExpandDepth(-1); // Hide schemas section by default
                });
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            // 4. Sử dụng Middleware (thứ tự quan trọng!)
            app.UseAuthentication(); // Phải đặt trước UseAuthorization
            app.UseAuthorization();
            
            // 5. Sử dụng custom middleware để validate role
            app.UseRoleValidation();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}