using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
namespace BusinessObjects
{
    public class MyDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        public MyDbContext() { }

        public MyDbContext(DbContextOptions<MyDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // --- 1. DbSet chính ---
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Manufacturer> Manufacturer { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductOption> ProductOption { get; set; }
        public DbSet<OptionValue> OptionValue { get; set; }
        public DbSet<ProductVariant> ProductVariant { get; set; }
        public DbSet<ProductVariantOption> ProductVariantOption { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Garage> Garage { get; set; }

        // --- 2. Các bảng người dùng và giao dịch ---
        public DbSet<User> User { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Transaction> Transaction { get; set; }

        // --- 3. Giỏ hàng và đơn hàng ---
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }


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

            // ============================================================
            // ✅ SET COLLATION CHO DATABASE & CÁC CỘT TIẾNG VIỆT
            // ============================================================

            if (Database.IsSqlServer())
            {
                modelBuilder.UseCollation("Vietnamese_CI_AS");
            }

            // PRODUCT
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .UseCollation("Vietnamese_CI_AS");

            modelBuilder.Entity<Product>()
                .Property(p => p.Description)
                .UseCollation("Vietnamese_CI_AS");

            // CATEGORY
            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .UseCollation("Vietnamese_CI_AS");

            modelBuilder.Entity<Category>()
                .Property(c => c.Description)
                .UseCollation("Vietnamese_CI_AS");

            // BRAND
            modelBuilder.Entity<Brand>()
                .Property(b => b.Name)
                .UseCollation("Vietnamese_CI_AS");

            // MANUFACTURER
            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.Name)
                .UseCollation("Vietnamese_CI_AS");

            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.Description)
                .UseCollation("Vietnamese_CI_AS");

            // PRODUCT OPTION
            modelBuilder.Entity<ProductOption>()
                .Property(po => po.Name)
                .UseCollation("Vietnamese_CI_AS");

            // OPTION VALUE
            modelBuilder.Entity<OptionValue>()
                .Property(ov => ov.Value)
                .UseCollation("Vietnamese_CI_AS");

            // PRODUCT VARIANT
            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.Name)
                .UseCollation("Vietnamese_CI_AS");

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.Dimensions)
                .UseCollation("Vietnamese_CI_AS");

            // ORDER
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingAddress)
                .UseCollation("Vietnamese_CI_AS");


            // USER
            modelBuilder.Entity<User>()
                .Property(u => u.FullName)
                .UseCollation("Vietnamese_CI_AS");

            //Service
            modelBuilder.Entity<Service>()
                .Property(v => v.Name)
                .UseCollation("Vietnamese_CI_AS");

            //Garage
            modelBuilder.Entity<Garage>()
                .Property (g => g.Name)
                .UseCollation("Vietnamese_CI_AS");

            // VEHICLE
            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Brand)
                .UseCollation("Vietnamese_CI_AS");

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Model)
                .UseCollation("Vietnamese_CI_AS");

            // ==========================================================
            // PRODUCT - VARIANT - OPTION CONFIGURATION
            // ==========================================================

            modelBuilder.Entity<ProductVariantOption>()
                .HasKey(pvo => new { pvo.VariantId, pvo.OptionValueId });

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.SKU)
                .IsUnique()
                .HasName("UX_ProductVariant_SKU");

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => new { pv.ProductId, pv.IsDefault })
                .IsUnique()
                .HasFilter("[IsDefault] = 1")
                .HasName("UX_ProductVariant_ProductId_IsDefault");

            modelBuilder.Entity<ProductVariantOption>()
                .HasIndex(pvo => new { pvo.VariantId, pvo.OptionValueId })
                .IsUnique()
                .HasName("UX_ProductVariantOption_VariantId_OptionValueId");

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
                .HasName("UX_ProductOption_ProductId_Name");

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

        //private string? GetCurrentUsername()
        //{
        //    try
        //    {
        //        // Lấy ClaimsPrincipal từ HttpContext
        //        var user = _httpContextAccessor?.HttpContext?.User;
                
        //        if (user?.Identity?.IsAuthenticated == true)
        //        {
        //            // Lấy thông tin từ claims theo thứ tự ưu tiên
        //            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        //            var name = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        //            var userId = user.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier)?.Value;
                    
        //            // Trả về theo thứ tự ưu tiên: email > name > userId
        //            return email ?? name ?? (userId != null ? $"User_{userId}" : null);
        //        }
                
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}