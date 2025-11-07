using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Cart;
using BusinessObjects.Models.DTOs.Garage;
using BusinessObjects.Models.DTOs.Order;
using BusinessObjects.Models.DTOs.Product;
using BusinessObjects.Models.DTOs.ServiceCategory;
using BusinessObjects.Models.DTOs.ServiceItem;
using BusinessObjects.Models.DTOs.ServiceRecord;
using BusinessObjects.Models.DTOs.User;
using BusinessObjects.Models.DTOs.Vehicle;
using BusinessObjects.Models.Type;
using Mapster;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Utils
{
    public class ConfigureMapster
    {
        public static void ConfigureMappings()
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Default.IgnoreNullValues(true);

            TypeAdapterConfig<Vehicle, VehicleDto>.NewConfig().TwoWays();

            TypeAdapterConfig<OrderItem, OrderItemDto>.NewConfig().TwoWays();

            TypeAdapterConfig<Order, OrderResponseDto>.NewConfig().TwoWays();

            TypeAdapterConfig<CartItem, CartItemDto>.NewConfig().TwoWays();

            TypeAdapterConfig<Product, ProductDetailDto>.NewConfig().TwoWays();

            TypeAdapterConfig<ProductOption, ProductOptionDto>.NewConfig().TwoWays();

            TypeAdapterConfig<OptionValue, OptionValueDto>.NewConfig().TwoWays();

            TypeAdapterConfig<ProductVariant, ProductVariantDto>.NewConfig().TwoWays();

            TypeAdapterConfig<Garage, GarageDto>.NewConfig().TwoWays();

            // User Create mapping with password hashing
            TypeAdapterConfig<UserCreateDto, User>
                .NewConfig()
                .Map(dest => dest.PasswordHash, src => BCrypt.Net.BCrypt.HashPassword(src.Password))
                .Map(dest => dest.UserStatus, src => UserStatus.ACTIVE)
                .Map(dest => dest.IsActive, src => true)
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.RefreshToken)
                .Ignore(dest => dest.RefreshTokenExpiryTime)
                .Ignore(dest => dest.Image)
                .Ignore(dest => dest.Vehicles)
                .Ignore(dest => dest.Transactions)
                .Ignore(dest => dest.Cart)
                .Ignore(dest => dest.Orders)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt);

            // User Update mapping - ignore password and sensitive fields
            TypeAdapterConfig<UserUpdateDto, User>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.PasswordHash)
                .Ignore(dest => dest.RefreshToken)
                .Ignore(dest => dest.RefreshTokenExpiryTime)
                .Ignore(dest => dest.Image)
                .Ignore(dest => dest.Vehicles)
                .Ignore(dest => dest.Transactions)
                .Ignore(dest => dest.Cart)
                .Ignore(dest => dest.Orders)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.IsActive);

            // ServiceItem and ServiceCategory mappings
            TypeAdapterConfig<ServiceItem, ServiceItemDto>.NewConfig();
            TypeAdapterConfig<ServiceCategory, ServiceCategoryDto>.NewConfig();

            // ServiceRecord mapping with navigation properties
            TypeAdapterConfig<ServiceRecord, ServiceRecordDto>
                .NewConfig()
                .Map(dest => dest.User, src => src.User)
                .Map(dest => dest.Vehicle, src => src.Vehicle);
            //.Map(dest => dest.ServiceItem, src => src.ServiceItem);
        }
    }
}