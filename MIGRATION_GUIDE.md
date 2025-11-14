# 🔄 Service Architecture Migration Guide

## Migration Date: November 12, 2025

## Summary

This migration fixes the service booking architecture to support **multi-garage operations** where:
- **Admin** creates global service catalog (categories and items)
- **Garages** enable specific services and optionally set custom pricing
- **Customers** book services from garage-enabled items
- **Staff** manages service records with proper assignment

---

## ✅ What Was Changed

### 1. **New Models Created**

#### `GarageServiceItem.cs`
Junction table linking garages to service items they offer.
```csharp
- GarageId (FK → Garage)
- ServiceItemId (FK → ServiceItem)
- CustomPrice (nullable decimal) - Garage's custom price
- IsEnabled (bool) - Whether garage offers this service
```

#### `ServiceRecordItem.cs`
Junction table for many-to-many relationship between bookings and services.
```csharp
- ServiceRecordId (FK → ServiceRecord)
- ServiceItemId (FK → ServiceItem)
- Quantity (int) - Number of times performed
- Price (decimal) - Actual charged price at time of booking
```

### 2. **Models Updated**

#### `ServiceCategory`
**Before:** Tied to specific garages
```csharp
❌ public int GarageId { get; set; }
❌ public virtual Garage Garage { get; set; }
```

**After:** Global Admin-defined catalog
```csharp
✅ public int? CreatedBy { get; set; } // Admin ID
✅ public string? Description { get; set; }
✅ Unique index on Name
```

#### `ServiceItem`
**Before:**
```csharp
❌ public decimal Price { get; set; }
❌ public int? ServiceCategoryId { get; set; }
❌ public int? ServiceRecordId { get; set; }
```

**After:**
```csharp
✅ public int CategoryId { get; set; } // Required FK
✅ public decimal DefaultPrice { get; set; } // Base price
✅ public string? Description { get; set; }
✅ public int DurationMinutes { get; set; }
✅ public virtual ICollection<GarageServiceItem> GarageServiceItems { get; set; }
✅ public virtual ICollection<ServiceRecordItem> ServiceRecordItems { get; set; }
```

#### `ServiceRecord`
**Added:**
```csharp
✅ public int? StaffId { get; set; } // Assigned staff member
✅ public virtual User? Staff { get; set; }
✅ public virtual ICollection<ServiceRecordItem> ServiceRecordItems { get; set; }
```

**Removed:**
```csharp
❌ public virtual ICollection<ServiceItem> ServiceItems { get; set; }
```

#### `Garage`
**Changed:**
```csharp
❌ public virtual ICollection<ServiceCategory> ServiceCategories { get; set; }
✅ public virtual ICollection<GarageServiceItem> GarageServiceItems { get; set; }
```

### 3. **Database Changes (Migration)**

**Migration Name:** `20251112082026_FixServiceArchitectureAndAddJunctionTables`

**New Tables:**
- `GarageServiceItem` (with unique index on GarageId + ServiceItemId)
- `ServiceRecordItem` (with index on ServiceRecordId + ServiceItemId)

**Modified Tables:**
- `ServiceCategory`: Dropped `GarageId` column and FK
- `ServiceItem`: Renamed `Price` → `DefaultPrice`, made `CategoryId` required, added `Description` and `DurationMinutes`
- `ServiceRecord`: Added `StaffId` column (nullable FK to User)

**Index Changes:**
- Added unique index `UX_ServiceCategory_Name` on ServiceCategory.Name
- Added unique index `UX_GarageServiceItem_GarageId_ServiceItemId`

---

## 📦 New Components Created

### DAOs
- ✅ `GarageServiceItemDAO.cs` - CRUD operations for garage service mappings

### Repositories
- ✅ `IGarageServiceItemRepository.cs` - Interface
- ✅ `GarageServiceItemRepository.cs` - Implementation

### DTOs
- ✅ `GarageServiceItemDto.cs` - Response DTO with calculated ActualPrice
- ✅ `EnableServiceItemDto.cs` - Request DTO for enabling services

---

## 🔧 Components Updated

### DAOs
- ✅ `GarageDAO.cs`:
  - `GetById()` - Now loads `GarageServiceItems` instead of `ServiceCategories`
  - `GetServiceItemsByGarageId()` - Uses `GarageServiceItem` junction table

### DbContext
- ✅ `MyDbContext.cs`:
  - Added `DbSet<GarageServiceItem>`
  - Added `DbSet<ServiceRecordItem>`
  - Fixed all entity relationships
  - Added indexes for junction tables

---

## 🚀 How to Apply This Migration

### Step 1: Apply the Migration to Database

```bash
cd BusinessObjects
dotnet ef database update
```

### Step 2: Update Dependency Injection (if needed)

Add to `Program.cs` in TheVehicleEcosystemAPI:

```csharp
builder.Services.AddScoped<IGarageServiceItemRepository, GarageServiceItemRepository>();
```

### Step 3: Migrate Existing Data (if any)

If you have existing data in the old schema, you'll need to:

1. **Backup your database** before applying migration
2. **Create Admin-level ServiceCategories** from unique garage categories
3. **Migrate ServiceItems** to use new `CategoryId` and `DefaultPrice`
4. **Create GarageServiceItem records** from existing garage-service relationships
5. **Update ServiceRecords** to use `ServiceRecordItem` junction table

**SQL Script Example** (run after migration):

```sql
-- Step 1: Create GarageServiceItem mappings from existing data
-- This assumes you had ServiceCategory.GarageId in old schema

INSERT INTO GarageServiceItem (GarageId, ServiceItemId, CustomPrice, IsEnabled, CreatedAt, UpdatedAt)
SELECT 
    sc.GarageId,
    si.Id as ServiceItemId,
    NULL as CustomPrice, -- No custom price initially
    1 as IsEnabled,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM ServiceItem si
INNER JOIN ServiceCategory sc ON si.ServiceCategoryId = sc.Id
WHERE si.IsActive = 1 AND sc.IsActive = 1;

-- Step 2: If you have existing ServiceRecords with direct ServiceItem links,
-- create ServiceRecordItem junction records
-- (This depends on your old schema - adjust as needed)
```

---

## ⚠️ Breaking Changes

### For Frontend/Mobile Apps

1. **Service Item Price Field Changed:**
   - Old: `ServiceItem.Price`
   - New: `ServiceItem.DefaultPrice`

2. **Garage Service Items Endpoint:**
   - Now returns `GarageServiceItemDto` with `ActualPrice` (custom or default)
   - New field: `ActualPrice = CustomPrice ?? DefaultPrice`

3. **Booking Creation:**
   - Service items must be enabled via `GarageServiceItem` for that garage
   - Price charged is the garage's `CustomPrice` if set, otherwise `DefaultPrice`

4. **ServiceRecord no longer has direct ServiceItems collection:**
   - Use `ServiceRecordItems` junction table instead
   - Each item includes `Quantity` and `Price` at time of booking

### For Controllers/APIs

1. **BookingController:**
   - Must query `GarageServiceItem` to get enabled services
   - Must create `ServiceRecordItem` records when booking
   - Must use `ActualPrice` from `GarageServiceItem`

2. **ServiceCategoryController:**
   - No longer garage-specific
   - Should be Admin-only for create/update/delete

3. **Need New Controller:**
   - `GarageServiceController` or similar for garage managers to enable/disable services

---

## 📋 Next Steps (TODO)

### Controllers to Update/Create

- [ ] Fix `BookingController.CreateBooking()` to use `GarageServiceItem` and `ServiceRecordItem`
- [ ] Fix `BookingController.GetGarageServiceItems()` to return `GarageServiceItemDto`
- [ ] Update `ServiceCategoryController` - make Admin-only
- [ ] Create `GarageServiceController` for garage managers:
  - `GET /api/garages/{id}/available-services` - View all global services
  - `POST /api/garages/{id}/services` - Enable a service
  - `PATCH /api/garages/{id}/services/{itemId}` - Update custom price
  - `DELETE /api/garages/{id}/services/{itemId}` - Disable service

### Staff Management Endpoints

- [ ] `GET /api/bookings/garage/{garageId}/pending` - Staff view pending bookings
- [ ] `PATCH /api/bookings/{id}/assign-staff` - Staff self-assign
- [ ] `PATCH /api/bookings/{id}/update-services` - Modify services during service
- [ ] `PATCH /api/bookings/{id}/complete` - Mark completed with final cost

### Documentation to Update

- [ ] API_DOCUMENTATION.md - Update all booking endpoints
- [ ] booking-service.instructions.md - Reflect new architecture
- [ ] create-booking.instructions.md - Update flow diagrams

---

## 🧪 Testing Checklist

After migration, test these scenarios:

### Admin Flow
- [ ] Create ServiceCategory (global, not tied to garage)
- [ ] Create ServiceItem under category with DefaultPrice
- [ ] Edit ServiceCategory/ServiceItem
- [ ] Delete ServiceCategory/ServiceItem (soft delete)

### Garage Manager Flow
- [ ] View all available global ServiceItems
- [ ] Enable specific ServiceItems for garage
- [ ] Set custom price for enabled service
- [ ] Disable a service

### Customer Flow
- [ ] Browse garages
- [ ] View services offered by a garage (only enabled ones)
- [ ] See correct pricing (custom or default)
- [ ] Create booking with multiple services
- [ ] View booking details with itemized services

### Staff Flow
- [ ] View pending bookings for their garage
- [ ] Accept booking (sets StaffId and status IN_PROGRESS)
- [ ] Update services during service execution
- [ ] Complete service (set final cost and status COMPLETED)

---

## 🔍 Business Rules Validation

Confirm these rules are enforced:

- ✅ **BR10**: Only Admin can create global ServiceCategory/ServiceItem
- ✅ **BR11**: Garages can only enable/disable and set custom price
- ✅ **BR12**: ServiceCategory names are unique system-wide
- ✅ **BR6**: ServiceRecord can contain multiple ServiceItems via junction
- ✅ **BR7**: StaffId must belong to same garage as GarageId
- ✅ **BR9**: TotalCost calculated from GarageServiceItem.ActualPrice

---

## 📞 Support

If you encounter issues after migration:

1. Check migration was applied: `dotnet ef migrations list`
2. Verify new tables exist in database
3. Check foreign key constraints are correct
4. Review this document for breaking changes
5. Contact backend team for assistance

---

**Migration Created By:** GitHub Copilot  
**Documentation Version:** 1.0  
**Last Updated:** November 12, 2025
