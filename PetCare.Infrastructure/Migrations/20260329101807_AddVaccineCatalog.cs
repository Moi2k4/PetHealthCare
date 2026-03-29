using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccineCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "vaccine_code",
                schema: "petcare",
                table: "vaccinations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "vaccine_catalog",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    aliases = table.Column<string>(type: "text", nullable: true),
                    default_interval_days = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccine_catalog", x => x.id);
                });

            migrationBuilder.InsertData(
                schema: "petcare",
                table: "vaccine_catalog",
                columns: new[] { "id", "aliases", "code", "created_at", "default_interval_days", "display_name", "is_active" },
                values: new object[,]
                {
                    { new Guid("9ef90f99-4b40-4e09-9f4b-1c39b44ea001"), "rabies;dai;dại;tiem dai;tiêm dại", "RABIES", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 365, "Rabies (Dai)", true },
                    { new Guid("9ef90f99-4b40-4e09-9f4b-1c39b44ea002"), "dhpp;dhlpp;5 in 1;5in1;7 in 1;7in1;distemper;parvo;care;ho cũi", "DHPP", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 365, "DHPP Core Vaccine", true },
                    { new Guid("9ef90f99-4b40-4e09-9f4b-1c39b44ea003"), "bordetella;kennel cough;ho cun cho;ho cũi chó", "BORDETELLA", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 365, "Bordetella (Kennel Cough)", true },
                    { new Guid("9ef90f99-4b40-4e09-9f4b-1c39b44ea004"), "lepto;leptospirosis", "LEPTO", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 365, "Leptospirosis", true },
                    { new Guid("9ef90f99-4b40-4e09-9f4b-1c39b44ea005"), "parvo;parvovirus", "PARVO_ONLY", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), 365, "Parvovirus", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_vaccine_catalog_code",
                schema: "petcare",
                table: "vaccine_catalog",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vaccine_catalog",
                schema: "petcare");

            migrationBuilder.DropColumn(
                name: "vaccine_code",
                schema: "petcare",
                table: "vaccinations");
        }
    }
}
