namespace TheVehicleEcosystemAPI.Response.DTOs
{
    /// <summary>
    /// Paginated response data wrapper
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class PaginatedData<T>
    {
        public int Size { get; set; }
        public int Page { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public PaginatedData()
        {
        }

        public PaginatedData(IEnumerable<T> items, int total, int page, int pageSize)
        {
            Items = items;
            Total = total;
            Page = page;
            Size = pageSize;
            TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        }

        /// <summary>
        /// Create paginated data from a full list
        /// </summary>
        public static PaginatedData<T> Create(IEnumerable<T> allItems, int page, int pageSize)
        {
            var itemsList = allItems.ToList();
            var total = itemsList.Count;
            var items = itemsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedData<T>(items, total, page, pageSize);
        }
    }
}
