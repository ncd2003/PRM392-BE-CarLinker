using BusinessObjects.Models.DTOs.Product.ProductOption;
using BusinessObjects.Models.DTOs.Product.ProductVariant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int WarrantyPeriod { get; set; }

        public decimal Price { get; set; }

        // Dữ liệu đã được "làm phẳng"
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string BrandName { get; set; }

        // Danh sách các tùy chọn của sản phẩm (vd: Màu sắc, Kích thước)
        public List<ProductOptionDto> ProductOptions { get; set; }

        // Danh sách các biến thể của sản phẩm (vd: Áo Đỏ-S, Áo Xanh-M)
        public List<ProductVariantDto> ProductVariants { get; set; }

        public virtual ICollection<ProductImageDto> ProductImages { get; set; }


    }
}
