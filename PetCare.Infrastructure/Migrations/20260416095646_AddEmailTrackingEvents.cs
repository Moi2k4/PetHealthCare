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
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_tracking_events", x => x.id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_tracking_events",
                schema: "petcare");
        }
    }
}
