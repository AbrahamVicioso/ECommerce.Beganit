using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Beganit.AdminPanel.Migrations
{
    /// <inheritdoc />
    public partial class CreateIsActiveProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ProductImages",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ProductImages");
        }
    }
}
