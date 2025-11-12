using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace BusinessObjects
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() { }
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        // --- 1. DbSet chính ---
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Manufacturer> Manufacturer { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductOption> ProductOption { get; set; }
        public DbSet<OptionValue> OptionValue { get; set; }
        public DbSet<ProductVariant> ProductVariant { get; set; }
        public DbSet<ProductVariantOption> ProductVariantOption { get; set; }
        public DbSet<Garage> Garage { get; set; }
        public DbSet<GarageStaff> GarageStaff { get; set; }
        public DbSet<ServiceCategory> ServiceCategory { get; set; }
        public DbSet<ServiceItem> ServiceItem { get; set; }
        public DbSet<ServiceRecord> ServiceRecord { get; set; }

        // --- 2. Các bảng người dùng và giao dịch ---
        public DbSet<User> User { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Transaction> Transaction { get; set; }

        // --- 3. Giỏ hàng và đơn hàng ---
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }

        public DbSet<ProductImage> ProductImage { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("MyCnn"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================================
            // PRODUCT - VARIANT - OPTION CONFIGURATION
            // ==========================================================

            modelBuilder.Entity<ProductVariantOption>()
                .HasKey(pvo => new { pvo.VariantId, pvo.OptionValueId });

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.SKU)
                .IsUnique()
                .HasDatabaseName("UX_ProductVariant_SKU");

            // PostgreSQL doesn't support filtered indexes in EF Core the same way as SQL Server
            // We'll create a unique index without the filter
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => new { pv.ProductId, pv.IsDefault })
                .HasDatabaseName("UX_ProductVariant_ProductId_IsDefault");

            modelBuilder.Entity<ProductVariantOption>()
                .HasIndex(pvo => new { pvo.VariantId, pvo.OptionValueId })
                .IsUnique()
                .HasDatabaseName("UX_ProductVariantOption_VariantId_OptionValueId");

            modelBuilder.Entity<ProductVariantOption>()
                .HasOne(pvo => pvo.ProductVariant)
                .WithMany(pv => pv.ProductVariantOptions)
                .HasForeignKey(pvo => pvo.VariantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProductVariantOption>()
                .HasOne(pvo => pvo.OptionValue)
                .WithMany(ov => ov.ProductVariantOptions)
                .HasForeignKey(pvo => pvo.OptionValueId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductOptions)
                .WithOne(po => po.Product)
                .HasForeignKey(po => po.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductOption>()
                .HasIndex(po => new { po.ProductId, po.Name })
                .IsUnique()
                .HasDatabaseName("UX_ProductOption_ProductId_Name");

            modelBuilder.Entity<ProductOption>()
                .HasMany(po => po.OptionValues)
                .WithOne(ov => ov.ProductOption)
                .HasForeignKey(ov => ov.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Manufacturer>()
                .HasMany(m => m.Products)
                .WithOne(p => p.Manufacturer)
                .HasForeignKey(p => p.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Brand>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Brand)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductVariants)
                .WithOne(pv => pv.Product)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.Property(pv => pv.Price).HasPrecision(18, 2);
                entity.Property(pv => pv.CostPrice).HasPrecision(18, 2);
                entity.Property(pv => pv.Weight).HasPrecision(8, 2);
            });

            // ==========================================================
            // USER CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<User>().ToTable("User");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            // ==========================================================
            // VEHICLE CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<Vehicle>().ToTable("Vehicle");

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================================
            // TRANSACTION CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<Transaction>().ToTable("Transaction");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)");

            // PostgreSQL uses NOW() or CURRENT_TIMESTAMP instead of GETDATE()
            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================================
            // CART CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<Cart>().ToTable("Cart");

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================================
            // CART ITEM CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<CartItem>().ToTable("CartItem");

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany(pv => pv.CartItems)
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Quantity)
                .HasDefaultValue(1)
                .IsRequired();

            // ==========================================================
            // ORDER CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<Order>().ToTable("Order");

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // PostgreSQL uses NOW() or CURRENT_TIMESTAMP 대신 GETDATE()
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("GETDATE()");

            // ==========================================================
            // ORDER ITEM CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<OrderItem>().ToTable("OrderItem");

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany(pv => pv.OrderItems)
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product) // Một ProductImage thuộc về một Product
                .WithMany(p => p.ProductImages) // Một Product có nhiều ProductImage
                .HasForeignKey(pi => pi.ProductId) // Khóa ngoại là ProductId
                .OnDelete(DeleteBehavior.Cascade); // <-- Quan trọng: Khi xóa Product, tất cả ProductImage liên quan sẽ tự động bị xóa.

            // ==========================================================
            // GARAGE CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<Garage>()
                .HasOne(g => g.User)
                .WithOne()
                .HasForeignKey<Garage>(g => g.UserId);

            modelBuilder.Entity<Garage>()
                .HasMany(g => g.ServiceRecords)
                .WithOne(sr => sr.Garage)
                .HasForeignKey(sr => sr.GarageId);

            modelBuilder.Entity<Garage>()
                .HasMany(g => g.GarageStaffs)
                .WithOne(gs => gs.Garage)
                .HasForeignKey(gs => gs.GarageId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================================
            // GARAGE STAFF CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<GarageStaff>().ToTable("GarageStaff");

            modelBuilder.Entity<GarageStaff>()
                .HasIndex(gs => gs.Email)
                .IsUnique()
                .HasDatabaseName("UX_GarageStaff_Email");

            modelBuilder.Entity<GarageStaff>()
                .HasIndex(gs => gs.PhoneNumber)
                .HasDatabaseName("IX_GarageStaff_PhoneNumber");

            modelBuilder.Entity<GarageStaff>()
                .HasIndex(gs => new { gs.GarageId, gs.IsActive })
                .HasDatabaseName("IX_GarageStaff_GarageId_IsActive");

            // ==========================================================
            // SERVICE CATEGORY CONFIGURATION
            // ==========================================================

            modelBuilder.Entity<ServiceCategory>()
                .HasMany(sc => sc.ServiceItems)
                .WithOne(si => si.ServiceCategory)
                .HasForeignKey(si => si.ServiceCategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);


            // ==========================================================
            // GARAGE CONFIGURATION
            // ==========================================================
            // 1. Định nghĩa Khóa chính kép cho bảng trung gian
            modelBuilder.Entity<GarageServiceItem>()
                .HasKey(gsi => new { gsi.GarageId, gsi.ServiceItemId });

            // 2. Cấu hình mối quan hệ giữa Garage và GarageServiceItem
            modelBuilder.Entity<GarageServiceItem>()
                .HasOne(gsi => gsi.Garage) // Mỗi bản ghi trung gian có 1 Garage
                .WithMany(g => g.GarageServiceItems) // Một Garage có nhiều bản ghi trung gian
                .HasForeignKey(gsi => gsi.GarageId);

            // 3. Cấu hình mối quan hệ giữa ServiceItem và GarageServiceItem
            modelBuilder.Entity<GarageServiceItem>()
                .HasOne(gsi => gsi.ServiceItem) // Mỗi bản ghi trung gian có 1 ServiceItem
                .WithMany(si => si.GarageServiceItems) // Một ServiceItem có nhiều bản ghi trung gian
                .HasForeignKey(gsi => gsi.ServiceItemId);

            // ==========================================================
            // SERVICE RECORD CONFIGURATION
            // ==========================================================
            modelBuilder.Entity<ServiceRecord>().ToTable("ServiceRecord");

            modelBuilder.Entity<ServiceRecord>()
                .Property(sr => sr.TotalCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRecord>()
                .Property(sr => sr.StartTime)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.ServiceRecords)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.Vehicle)
                .WithMany(v => v.ServiceRecords)
                .HasForeignKey(sr => sr.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRecord>()
                .HasMany(sr => sr.ServiceItems)
                .WithOne()
                .HasForeignKey(si => si.ServiceRecordId)
                .IsRequired(false);

            modelBuilder.Entity<ServiceRecord>()
                .HasOne(sr => sr.Garage)
                .WithMany(g => g.ServiceRecords);
        }

        // ==========================================================
        // AUTO AUDIT FIELDS
        // ==========================================================
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseModel && (e.State == EntityState.Added || e.State == EntityState.Modified));

            var now = DateTimeOffset.UtcNow;

            foreach (var entry in entries)
            {
                var entity = (BaseModel)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                }

                entity.UpdatedAt = now;
            }
        }
    }
}