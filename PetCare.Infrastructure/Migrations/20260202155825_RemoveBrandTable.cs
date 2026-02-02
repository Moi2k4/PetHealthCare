using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrandTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_products_brands_brand_id",
            //    schema: "petcare",
            //    table: "products");

            migrationBuilder.Sql("DROP TABLE IF EXISTS petcare.brands CASCADE;");

            //migrationBuilder.DropIndex(
            //    name: "IX_products_brand_id",
            //    schema: "petcare",
            //    table: "products");

            migrationBuilder.DropColumn(
                name: "brand_id",
                schema: "petcare",
                table: "products");

            migrationBuilder.AddColumn<string>(
                name: "VoucherCode",
                schema: "petcare",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VoucherId",
                schema: "petcare",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payments",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    payment_gateway_response = table.Column<string>(type: "text", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refund_reason = table.Column<string>(type: "text", nullable: true),
                    refunded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "petcare",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vouchers",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    minimum_order_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    maximum_discount_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    usage_limit = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    applicable_product_categories = table.Column<string>(type: "text", nullable: true),
                    applicable_services = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vouchers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_usages",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    discount_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher_usages", x => x.id);
                    table.ForeignKey(
                        name: "FK_voucher_usages_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "petcare",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_voucher_usages_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_voucher_usages_vouchers_voucher_id",
                        column: x => x.voucher_id,
                        principalSchema: "petcare",
                        principalTable: "vouchers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_VoucherId",
                schema: "petcare",
                table: "orders",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_order_id",
                schema: "petcare",
                table: "payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_transaction_id",
                schema: "petcare",
                table: "payments",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_user_id",
                schema: "petcare",
                table: "payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_usages_order_id",
                schema: "petcare",
                table: "voucher_usages",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_usages_user_id",
                schema: "petcare",
                table: "voucher_usages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_usages_voucher_id",
                schema: "petcare",
                table: "voucher_usages",
                column: "voucher_id");

            migrationBuilder.CreateIndex(
                name: "IX_vouchers_code",
                schema: "petcare",
                table: "vouchers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vouchers_is_active_valid_from_valid_to",
                schema: "petcare",
                table: "vouchers",
                columns: new[] { "is_active", "valid_from", "valid_to" });

            migrationBuilder.AddForeignKey(
                name: "FK_orders_vouchers_VoucherId",
                schema: "petcare",
                table: "orders",
                column: "VoucherId",
                principalSchema: "petcare",
                principalTable: "vouchers",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_vouchers_VoucherId",
                schema: "petcare",
                table: "orders");

            migrationBuilder.DropTable(
                name: "payments",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "voucher_usages",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "vouchers",
                schema: "petcare");

            migrationBuilder.DropIndex(
                name: "IX_orders_VoucherId",
                schema: "petcare",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "VoucherCode",
                schema: "petcare",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                schema: "petcare",
                table: "orders");

            migrationBuilder.AddColumn<Guid>(
                name: "brand_id",
                schema: "petcare",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "brands",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_brand_id",
                schema: "petcare",
                table: "products",
                column: "brand_id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_brands_brand_id",
                schema: "petcare",
                table: "products",
                column: "brand_id",
                principalSchema: "petcare",
                principalTable: "brands",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
