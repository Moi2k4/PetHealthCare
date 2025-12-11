using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToThirdPartyServiceModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_health_analyses",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analysis_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    input_data = table.Column<string>(type: "text", nullable: false),
                    ai_response = table.Column<string>(type: "text", nullable: false),
                    recommendations = table.Column<string>(type: "text", nullable: true),
                    confidence_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    tokens_used = table.Column<int>(type: "integer", nullable: false),
                    ai_model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_reviewed = table.Column<bool>(type: "boolean", nullable: false),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_health_analyses", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_health_analyses_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "petcare",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ai_health_analyses_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ai_health_analyses_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_packages",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    billing_cycle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    has_ai_health_tracking = table.Column<bool>(type: "boolean", nullable: false),
                    has_vaccination_tracking = table.Column<bool>(type: "boolean", nullable: false),
                    has_health_reminders = table.Column<bool>(type: "boolean", nullable: false),
                    has_ai_recommendations = table.Column<bool>(type: "boolean", nullable: false),
                    has_nutritional_analysis = table.Column<bool>(type: "boolean", nullable: false),
                    has_early_disease_detection = table.Column<bool>(type: "boolean", nullable: false),
                    has_priority_support = table.Column<bool>(type: "boolean", nullable: false),
                    max_pets = table.Column<int>(type: "integer", nullable: true),
                    features = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_packages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_subscriptions",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    next_billing_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    amount_paid = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_subscription_packages_subscription_packa~",
                        column: x => x.subscription_package_id,
                        principalSchema: "petcare",
                        principalTable: "subscription_packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_health_analyses_pet_id",
                schema: "petcare",
                table: "ai_health_analyses",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_health_analyses_pet_id_analysis_type",
                schema: "petcare",
                table: "ai_health_analyses",
                columns: new[] { "pet_id", "analysis_type" });

            migrationBuilder.CreateIndex(
                name: "IX_ai_health_analyses_reviewed_by",
                schema: "petcare",
                table: "ai_health_analyses",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_ai_health_analyses_user_id",
                schema: "petcare",
                table: "ai_health_analyses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_subscription_package_id",
                schema: "petcare",
                table: "user_subscriptions",
                column: "subscription_package_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_user_id",
                schema: "petcare",
                table: "user_subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_user_id_is_active",
                schema: "petcare",
                table: "user_subscriptions",
                columns: new[] { "user_id", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_health_analyses",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "user_subscriptions",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "subscription_packages",
                schema: "petcare");
        }
    }
}
