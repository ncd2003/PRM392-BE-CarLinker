using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Product
{
    public class ProductVariantDto
    {
        // ID để xác định duy nhất biến thể này khi thêm vào giỏ hàng
        public int Id { get; set; }

        // Tên được tạo sẵn (nếu có), vd: "Đỏ / Size M"
        public string Name { get; set; }

        // Mã định danh sản phẩm, hữu ích cho việc quản lý
        public string? SKU { get; set; }

        // Giá bán mà khách hàng sẽ thấy
        public decimal Price { get; set; }

        // Số lượng tồn kho có sẵn để bán
        public int Stock { get; set; }

        // Cho frontend biết nên chọn sẵn biến thể này khi tải trang hay không
        public bool IsDefault { get; set; }

        // Gợi ý: Thêm thuộc tính này nếu mỗi biến thể có ảnh riêng
        // public string? Image { get; set; }

        // Đây là phần quan trọng nhất để liên kết với các lựa chọn của người dùng
        // Chứa danh sách các ID của OptionValue, vd: [1 (cho màu Đỏ), 5 (cho size M)]
        public List<int> SelectedOptionValueIds { get; set; }
    }
}
