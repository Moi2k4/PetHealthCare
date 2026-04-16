using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTrackingEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_tracking_events",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    recipient = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    clicked_url = table.Column<string>(type: "text", nullable: true),
                    event_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)

            migrationBuilder.CreateIndex(
                name: "IX_email_tracking_events_email_id",
                schema: "petcare",
                table: "email_tracking_events",
                column: "email_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_tracking_events_event_type",
                schema: "petcare",
                table: "email_tracking_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_email_tracking_events_recipient",
                schema: "petcare",
                table: "email_tracking_events",
                column: "recipient");
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2028"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 9, 56, 46, 253, DateTimeKind.Utc).AddTicks(342));

            migrationBuilder.UpdateData(

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2021"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9565));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2022"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9567));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2023"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9569));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2024"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9573));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2025"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9576));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2026"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9578));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2027"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9580));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2028"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9583));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2029"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9585));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2030"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9588));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2031"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9590));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2032"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(9594));

            migrationBuilder.UpdateData(
                schema: "petcare",
                table: "pet_species",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001"),
                column: "created_at",
                value: new DateTime(2026, 4, 16, 8, 47, 38, 755, DateTimeKind.Utc).AddTicks(8130));
        }
    }
}
