using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class CreateProductVariantDto
    {
        [Required(ErrorMessage = "Tên biến thể là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá vốn phải là số không âm.")]
        public decimal? CostPrice { get; set; }

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Cân nặng phải là số không âm.")]
        public decimal? Weight { get; set; }

        [StringLength(50, ErrorMessage = "SKU không được vượt quá 50 ký tự.")]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải là số không âm.")]
        public int StockQuantity { get; set; } = 0;

  
        [StringLength(100, ErrorMessage = "Kích thước không được vượt quá 100 ký tự.")]
        public string? Dimensions { get; set; }
        public bool IsDefault { get; set; } = false;

        // --- Ghi chú ---
        // - ProductId: Sẽ được lấy từ tham số URL (route parameter) trong Controller.
        // - Id: Sẽ được tự động tạo bởi cơ sở dữ liệu.
        // - IsActive: Sẽ được tự động set là 'true' ở tầng DAO/Service khi tạo mới.
        // - HoldQuantity: Sẽ được quản lý bởi hệ thống (logic nghiệp vụ) và mặc định là 0.
        // - ProductVariantOptions: Việc liên kết options/values là một nghiệp vụ
        //   phức tạp hơn, có thể cần một DTO riêng hoặc xử lý sau khi tạo variant.
    }
}
