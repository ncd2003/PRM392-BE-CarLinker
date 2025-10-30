using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    // Lớp này dùng để nhận tham số lọc và phân trang từ API
    public class ProductFilterParams
    {
        // --- Filtering ---
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        //public bool? IsFeatured { get; set; }

        // --- Sorting ---
        public string? SortBy { get; set; } // Vd: "price_asc", "price_desc", "name"

        // --- Paging ---
        //private int _pageSize = 12; // Kích thước trang mặc định
        //private const int MaxPageSize = 48; // Kích thước trang tối đa

        //public int PageNumber { get; set; } = 1; // Trang mặc định

        //public int PageSize
        //{
        //    get => _pageSize;
        //    set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        //}
    }
}