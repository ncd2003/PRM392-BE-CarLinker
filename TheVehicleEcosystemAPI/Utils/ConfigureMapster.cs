using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Cart;
using BusinessObjects.Models.DTOs.Order;
using BusinessObjects.Models.DTOs.Product;
using BusinessObjects.Models.DTOs.Vehicle;
using Mapster;
using TheVehicleEcosystemAPI.Response.DTOs;

namespace TheVehicleEcosystemAPI.Utils
{
    public class ConfigureMapster
    {
        public static void ConfigureMappings()
        {
            TypeAdapterConfig<Vehicle, VehicleDto>.NewConfig().TwoWays();

            TypeAdapterConfig<OrderItem, OrderItemDto>.NewConfig().TwoWays(); ;

            TypeAdapterConfig<Order, OrderResponseDto>.NewConfig().TwoWays(); ;

            TypeAdapterConfig<CartItem, CartItemDto>.NewConfig().TwoWays();

            TypeAdapterConfig<Product, ProductDetailDto>.NewConfig().TwoWays(); ;

            TypeAdapterConfig<ProductOption, ProductOptionDto>.NewConfig().TwoWays();

            TypeAdapterConfig<OptionValue, OptionValueDto>.NewConfig().TwoWays();

            TypeAdapterConfig<ProductVariant, ProductVariantDto>.NewConfig().TwoWays();
        }
    }
}