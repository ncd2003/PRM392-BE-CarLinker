using BusinessObjects.Models.DTOs.ServiceCategory;
using BusinessObjects.Models.DTOs.ServiceItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.DTOs.Garage
{
    public class GarageDetailDto
    {
        public GarageDto? GarageDto { get; set; }
        
        /// <summary>
        /// Danh sách dịch vụ được nhóm theo category
        /// </summary>
        public List<ServiceCategoryDto> ServiceCategories { get; set; } = new List<ServiceCategoryDto>();
        
        /// <summary>
        /// Tổng số dịch vụ của garage
        /// </summary>
        public int TotalServiceItems { get; set; }
    }
}
