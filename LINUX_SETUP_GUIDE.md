# 🐧 Complete Linux Development Guide for CarLinker Backend

This guide will help you set up and work with the CarLinker backend on Linux using SQL Server in Docker.

---

## 🎯 **What I've Set Up For You**

✅ **Docker Compose** with SQL Server 2022  
✅ **Updated connection strings** to use SQL authentication (works on Linux)  
✅ **Health checks** for SQL Server container  
✅ **Persistent volume** for database data  

---

## 📦 **Prerequisites**

1. **Docker & Docker Compose** installed
2. **.NET 8 SDK** installed
3. **EF Core Tools** installed globally

```bash
# Install EF Core tools globally (if not already installed)
dotnet tool install --global dotnet-ef
```

---

## 🚀 **Step-by-Step Setup**

### **1️⃣ Start SQL Server Container**

```bash
# From the project root directory
docker-compose up -d

# Check if container is running
docker ps

# Watch the logs to ensure SQL Server is ready
docker logs -f carlinker-mssql
```

**Wait for this message:**
```
SQL Server is now ready for client connections
```

Press `Ctrl+C` to stop watching logs.

---

### **2️⃣ Verify SQL Server Connection**

```bash
# Connect to SQL Server using sqlcmd (inside the container)
docker exec -it carlinker-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "CarLinker@2025" -C

# Once connected, you should see: 1>
# Type these commands to verify:
SELECT @@VERSION;
GO
EXIT
```

---

### **3️⃣ Apply Database Migrations**

```bash
# Navigate to the BusinessObjects project
cd BusinessObjects

# Apply existing migrations to create the database
dotnet ef database update

# You should see output like:
# "Applying migration '20251109194644_TenBanNhanDienMigration'"
# "Done."
```

---

### **4️⃣ Verify Database Creation**

```bash
# Connect to SQL Server
docker exec -it carlinker-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "CarLinker@2025" -C

# Check if CarLinker database exists
SELECT name FROM sys.databases;
GO

# Use the database
USE CarLinker;
GO

# List all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
GO

# Exit
EXIT
```

You should see tables like: `User`, `Vehicle`, `ServiceRecord`, `Product`, `Cart`, `Order`, etc.

---

### **5️⃣ Run the API**

```bash
# Navigate to API project
cd ../TheVehicleEcosystemAPI

# Restore packages
dotnet restore

# Run the application
dotnet run

# Or run with hot reload
dotnet watch run
```

The API will start at:
- **HTTPS**: `https://localhost:7151`
- **HTTP**: `http://localhost:5173` (configured for CORS)
- **Swagger**: `https://localhost:7151/swagger`

---

## 🔧 **Common EF Core Commands**

### **Create a New Migration**
```bash
cd BusinessObjects
dotnet ef migrations add YourMigrationName
```

### **Apply Migrations**
```bash
dotnet ef database update
```

### **Rollback to Previous Migration**
```bash
dotnet ef database update PreviousMigrationName
```

### **Remove Last Migration** (if not applied)
```bash
dotnet ef migrations remove
```

### **List All Migrations**
```bash
dotnet ef migrations list
```

### **Generate SQL Script** (without applying)
```bash
dotnet ef migrations script
```

### **Drop Database** (be careful!)
```bash
dotnet ef database drop
```

---

## 🛠️ **Docker Commands**

### **Start SQL Server**
```bash
docker-compose up -d
```

### **Stop SQL Server**
```bash
docker-compose down
```

### **Stop and Remove Data** (⚠️ destroys database!)
```bash
docker-compose down -v
```

### **View Logs**
```bash
docker logs -f carlinker-mssql
```

### **Restart Container**
```bash
docker-compose restart
```

### **Execute SQL Queries Directly**
```bash
docker exec -it carlinker-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "CarLinker@2025" -C \
  -Q "SELECT COUNT(*) FROM CarLinker.dbo.[User]"
```

---

## 🔐 **Connection String Details**

### **For Linux (SQL Authentication)**
```
Server=localhost,1433;Database=CarLinker;User Id=sa;Password=CarLinker@2025;TrustServerCertificate=true;MultipleActiveResultSets=true;
```

### **For Windows (Windows Authentication) - Your teammates**
```
Server=localhost;Database=CarLinker;Trusted_Connection=True;TrustServerCertificate=true;
```

**💡 Solution for Team Collaboration:**

Create `appsettings.Development.json` per developer:

**Your appsettings.Development.json (Linux):**
```json
{
  "ConnectionStrings": {
    "MyCnn": "Server=localhost,1433;Database=CarLinker;User Id=sa;Password=CarLinker@2025;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

**Your Windows teammates' appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "MyCnn": "Server=localhost;Database=CarLinker;Trusted_Connection=True;TrustServerCertificate=true;"
  }
}
```

**⚠️ Add to `.gitignore`:**
```gitignore
**/appsettings.Development.json
```

This way each developer uses their own connection string without conflicts.

---

## 📊 **Database Structure Overview**

Your database has these main modules:

### **🛒 E-Commerce Module**
- `Product` - Products/parts
- `ProductVariant` - SKU variants (size, color, etc.)
- `ProductOption` & `OptionValue` - Product options
- `Category` - Product categories
- `Brand` & `Manufacturer` - Product metadata
- `Cart` & `CartItem` - Shopping cart
- `Order` & `OrderItem` - Orders

### **🔧 Service Module**
- `Garage` - Garage/workshop info
- `ServiceCategory` - Service categories per garage
- `ServiceItem` - Available services
- `ServiceRecord` - Service bookings and history

### **👤 User Module**
- `User` - Users (CUSTOMER, STAFF, GARAGE_OWNER, ADMIN)
- `Vehicle` - User vehicles
- `Transaction` - Payment transactions

---

## 🧪 **Testing the Setup**

### **1. Check if API is running**
```bash
curl http://localhost:5173
# or
curl https://localhost:7151
```

### **2. Open Swagger UI**
Visit: `https://localhost:7151/swagger`

### **3. Test Database Connection**
```bash
cd TheVehicleEcosystemAPI
dotnet run
# Check the console output - no database errors means success!
```

---

## 🐛 **Troubleshooting**

### **Problem: Container won't start**
```bash
# Check logs
docker logs carlinker-mssql

# Common fix: remove old container and volume
docker-compose down -v
docker-compose up -d
```

### **Problem: Cannot connect to SQL Server**
```bash
# Check if port 1433 is open
netstat -tuln | grep 1433

# Test connection
docker exec -it carlinker-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "CarLinker@2025" -C -Q "SELECT 1"
```

### **Problem: Migration fails**
```bash
# Check connection string in appsettings.json
cat BusinessObjects/appsettings.json

# Try updating with verbose logging
dotnet ef database update --verbose
```

### **Problem: Port 1433 already in use**
```bash
# Check what's using the port
sudo lsof -i :1433

# Change port in docker-compose.yml:
# ports:
#   - "1434:1433"  # Use 1434 instead
# Then update connection string:
# Server=localhost,1434;...
```

---

## 🤝 **Working with Your Windows Teammates**

### **Option 1: Each Developer Uses Local DB**
- You: Docker SQL Server on Linux
- Teammates: Native SQL Server on Windows
- Migrations are committed to Git
- Everyone applies migrations to their local DB

### **Option 2: Shared Development Database**
- Set up a shared Azure SQL Database or remote SQL Server
- Everyone connects to the same database
- Update connection string to point to remote server

### **Option 3: Docker for Everyone**
- Ask teammates to also use Docker
- Same `docker-compose.yml` works on Windows and Linux
- Everyone has identical environment

**Recommended:** Option 1 for development, Option 2 for staging/testing

---

## 📝 **Quick Reference**

| Task | Command |
|------|---------|
| Start DB | `docker-compose up -d` |
| Stop DB | `docker-compose down` |
| View logs | `docker logs -f carlinker-mssql` |
| Apply migrations | `cd BusinessObjects && dotnet ef database update` |
| Create migration | `cd BusinessObjects && dotnet ef migrations add MigrationName` |
| Run API | `cd TheVehicleEcosystemAPI && dotnet run` |
| Connect to DB | `docker exec -it carlinker-mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CarLinker@2025" -C` |

---

## 🎓 **Next Steps**

1. ✅ Start SQL Server: `docker-compose up -d`
2. ✅ Apply migrations: `cd BusinessObjects && dotnet ef database update`
3. ✅ Run API: `cd TheVehicleEcosystemAPI && dotnet run`
4. ✅ Open Swagger: `https://localhost:7151/swagger`
5. ✅ Start developing!

---

## 💡 **Pro Tips**

1. **Use Azure Data Studio** - Great SQL Server GUI for Linux
   ```bash
   # Install on Ubuntu/Debian
   wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > packages.microsoft.gpg
   sudo install -o root -g root -m 644 packages.microsoft.gpg /etc/apt/trusted.gpg.d/
   sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/microsoft.list'
   sudo apt-get update
   sudo apt-get install azuredatastudio
   ```

2. **Use DBeaver** - Universal database tool
   ```bash
   sudo snap install dbeaver-ce
   ```

3. **Add this alias** to your `~/.bashrc`:
   ```bash
   alias sqlcmd='docker exec -it carlinker-mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CarLinker@2025" -C'
   ```
   Then you can just run: `sqlcmd`

4. **Keep Docker running** on startup:
   ```bash
   # Add to systemd or use Docker Desktop auto-start
   ```

---

## 📚 **Additional Resources**

- [SQL Server on Linux Docs](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-overview)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Docker Compose](https://docs.docker.com/compose/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)

---

**Happy Coding! 🚀**
