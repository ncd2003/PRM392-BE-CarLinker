using BusinessObjects;
using BusinessObjects.Models.DTOs.Garage;
using DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using System.Security.Claims;
using System.Text;
using TheVehicleEcosystemAPI.Security;
using TheVehicleEcosystemAPI.Utils;
using VNPAY.NET;

namespace TheVehicleEcosystemAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // ✨ Register IHttpContextAccessor for accessing HttpContext in DbContext
            builder.Services.AddHttpContextAccessor();

            // Register DbContext with IHttpContextAccessor
            builder.Services.AddDbContext<MyDbContext>((serviceProvider, options) =>
            {
                var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
                var connectionString = builder.Configuration.GetConnectionString("MyCnn");

                options.UseSqlServer(connectionString);
            });

            // Register DAOs
            builder.Services.AddScoped<VehicleDAO>();
            builder.Services.AddScoped<UserDAO>();
            builder.Services.AddScoped<CartDAO>();
            builder.Services.AddScoped<OrderDAO>();
            builder.Services.AddScoped<ProductDAO>();
            builder.Services.AddScoped<CategoryDAO>();
            builder.Services.AddScoped<BrandDAO>();
            builder.Services.AddScoped<ServiceCategoryDAO>();
            builder.Services.AddScoped<GarageDAO>();
            builder.Services.AddScoped<ServiceItemDAO>();
            builder.Services.AddScoped<ChatRoomDAO>();
            builder.Services.AddScoped<ChatMessageDAO>();
            builder.Services.AddScoped<ChatRoomMemberDAO>();

            // Register Repositories
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            builder.Services.AddScoped<IGarageRepository, GarageRepository>();
            builder.Services.AddScoped<IServiceItemRepository, ServiceItemRepository>();
            builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            builder.Services.AddScoped<IChatRoomMemberRepository, ChatRoomMemberRepository>();

            // Register Cloudflare R2 Storage
            builder.Services.AddSingleton<CloudflareR2Storage>();

            // Register JWT Configuration
            builder.Services.AddScoped<JWTConfiguration>();

            // Mapster
            ConfigureMapster.ConfigureMappings();

            //Vnpay
            builder.Services.AddSingleton<IVnpay>(sp =>
            {
                // Lấy IConfiguration từ Service Provider
                var config = sp.GetRequiredService<IConfiguration>();

                // Đọc các giá trị từ appsettings.json
                string tmnCode = config["Vnpay:TmnCode"];
                string hashSecret = config["Vnpay:HashSecret"];
                string baseUrl = config["Vnpay:BaseUrl"];
                string returnUrl = config["Vnpay:ReturnUrl"];

                // Tạo và khởi tạo đối tượng Vnpay
                var vnpay = new Vnpay();
                vnpay.Initialize(tmnCode, hashSecret, baseUrl, returnUrl);

                return vnpay; // Trả về đối tượng đã được khởi tạo
            });

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
                        policy.WithOrigins(
                      "http://localhost:5173",           // React app
                      "http://10.0.2.2:5291",            // Android emulator
                      "http://192.168.1.100:5291"        // Thiết bị thật (thay IP của bạn)
                  )
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
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

            //app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            // 4. Sử dụng Middleware (thứ tự quan trọng!)
            app.UseAuthentication(); // Phải đặt trước UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}