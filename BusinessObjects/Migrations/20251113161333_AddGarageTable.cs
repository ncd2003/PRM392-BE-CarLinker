using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Manufacturer");

            migrationBuilder.DropIndex(
                name: "IX_GarageStaff_GarageId",
                table: "GarageStaff");

            migrationBuilder.RenameIndex(
                name: "IX_GarageStaff_Email",
                table: "GarageStaff",
                newName: "UX_GarageStaff_Email");

            migrationBuilder.AlterColumn<int>(
                name: "GarageId",
                table: "GarageStaff",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "GarageStaff",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GarageId1",
                table: "GarageStaff",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "GarageStaff",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GarageStaff_GarageId_IsActive",
                table: "GarageStaff",
                columns: new[] { "GarageId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_GarageStaff_GarageId1",
                table: "GarageStaff",
                column: "GarageId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GarageStaff_Garage_GarageId1",
                table: "GarageStaff",
                column: "GarageId1",
                principalTable: "Garage",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarageStaff_Garage_GarageId1",
                table: "GarageStaff");

            migrationBuilder.DropIndex(
                name: "IX_GarageStaff_GarageId_IsActive",
                table: "GarageStaff");

            migrationBuilder.DropIndex(
                name: "IX_GarageStaff_GarageId1",
                table: "GarageStaff");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GarageStaff");

            migrationBuilder.DropColumn(
                name: "GarageId1",
                table: "GarageStaff");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GarageStaff");

            migrationBuilder.RenameIndex(
                name: "UX_GarageStaff_Email",
                table: "GarageStaff",
                newName: "IX_GarageStaff_Email");

            migrationBuilder.AlterColumn<int>(
                name: "GarageId",
                table: "GarageStaff",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Website = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturer", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GarageStaff_GarageId",
                table: "GarageStaff",
                column: "GarageId");
        }
    }
}
