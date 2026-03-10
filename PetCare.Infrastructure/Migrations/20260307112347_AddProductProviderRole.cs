using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductProviderRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                schema: "petcare",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_ProviderId",
                schema: "petcare",
                table: "products",
                column: "ProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_users_ProviderId",
                schema: "petcare",
                table: "products",
                column: "ProviderId",
                principalSchema: "petcare",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_users_ProviderId",
                schema: "petcare",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_ProviderId",
                schema: "petcare",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                schema: "petcare",
                table: "products");
        }
    }
}
