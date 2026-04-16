using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO petcare.product_categories (id, category_name, parent_category_id, description, image_url, display_order, is_active, created_at, updated_at)
                SELECT '13f6f900-4b7d-4db7-a843-39fbe4d31001'::uuid, 'Thức ăn', NULL, 'Thức ăn cho thú cưng', NULL, 1, true, NOW(), NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM petcare.product_categories WHERE lower(category_name) = lower('Thức ăn') AND parent_category_id IS NULL
                );

                INSERT INTO petcare.product_categories (id, category_name, parent_category_id, description, image_url, display_order, is_active, created_at, updated_at)
                SELECT '13f6f900-4b7d-4db7-a843-39fbe4d31002'::uuid, 'Phụ kiện', NULL, 'Phụ kiện chăm sóc thú cưng', NULL, 2, true, NOW(), NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM petcare.product_categories WHERE lower(category_name) = lower('Phụ kiện') AND parent_category_id IS NULL
                );

                INSERT INTO petcare.product_categories (id, category_name, parent_category_id, description, image_url, display_order, is_active, created_at, updated_at)
                SELECT '13f6f900-4b7d-4db7-a843-39fbe4d31003'::uuid, 'Đồ chơi', NULL, 'Đồ chơi cho thú cưng', NULL, 3, true, NOW(), NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM petcare.product_categories WHERE lower(category_name) = lower('Đồ chơi') AND parent_category_id IS NULL
                );

                INSERT INTO petcare.product_categories (id, category_name, parent_category_id, description, image_url, display_order, is_active, created_at, updated_at)
                SELECT '13f6f900-4b7d-4db7-a843-39fbe4d31004'::uuid, 'Thuốc & Vitamin', NULL, 'Thuốc và vitamin bổ sung', NULL, 4, true, NOW(), NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM petcare.product_categories WHERE lower(category_name) = lower('Thuốc & Vitamin') AND parent_category_id IS NULL
                );

                INSERT INTO petcare.product_categories (id, category_name, parent_category_id, description, image_url, display_order, is_active, created_at, updated_at)
                SELECT '13f6f900-4b7d-4db7-a843-39fbe4d31005'::uuid, 'Vệ sinh', NULL, 'Sản phẩm vệ sinh', NULL, 5, true, NOW(), NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM petcare.product_categories WHERE lower(category_name) = lower('Vệ sinh') AND parent_category_id IS NULL
                );

                INSERT INTO petcare.product_categories (id, category_name, parent_category_id, description, image_url, display_order, is_active, created_at, updated_at)
                SELECT '13f6f900-4b7d-4db7-a843-39fbe4d31006'::uuid, 'Quần áo', NULL, 'Quần áo cho thú cưng', NULL, 6, true, NOW(), NULL
                WHERE NOT EXISTS (
                    SELECT 1 FROM petcare.product_categories WHERE lower(category_name) = lower('Quần áo') AND parent_category_id IS NULL
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM petcare.product_categories
                WHERE id IN (
                    '13f6f900-4b7d-4db7-a843-39fbe4d31001'::uuid,
                    '13f6f900-4b7d-4db7-a843-39fbe4d31002'::uuid,
                    '13f6f900-4b7d-4db7-a843-39fbe4d31003'::uuid,
                    '13f6f900-4b7d-4db7-a843-39fbe4d31004'::uuid,
                    '13f6f900-4b7d-4db7-a843-39fbe4d31005'::uuid,
                    '13f6f900-4b7d-4db7-a843-39fbe4d31006'::uuid
                );
            ");
        }
    }
}
