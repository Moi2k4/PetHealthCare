using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToProductCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE petcare.product_categories
                ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone;
            ");

            migrationBuilder.Sql(@"
                UPDATE petcare.product_categories
                SET updated_at = created_at
                WHERE updated_at IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE petcare.product_categories
                DROP COLUMN IF EXISTS updated_at;
            ");
        }
    }
}
