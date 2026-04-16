using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDogSpeciesAndBreeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "petcare",
                table: "pet_species",
                columns: new[] { "id", "created_at", "description", "species_name" },
                values: new object[] { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001"), new DateTime(2026, 4, 16, 7, 45, 51, 367, DateTimeKind.Utc).AddTicks(6921), "Loài chó", "Chó" });

            migrationBuilder.InsertData(
                schema: "petcare",
                table: "pet_breeds",
                columns: new[] { "id", "breed_name", "characteristics", "created_at", "species_id" },
                values: new object[,]
                {
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2001"), "Golden Retriever", "Hiền, thông minh, thân thiện", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(438), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2002"), "Labrador Retriever", "Ngoan, năng động, dễ huấn luyện", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(451), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2003"), "German Shepherd", "Trung thành, cảnh giác, mạnh mẽ", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(454), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2004"), "Poodle", "Thông minh, ít rụng lông, thân thiện", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(465), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2005"), "Beagle", "Hiếu động, tò mò, thân thiện", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(468), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2006"), "French Bulldog", "Nhỏ gọn, dễ nuôi, tình cảm", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(470), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2007"), "Shih Tzu", "Hiền, đáng yêu, hợp nuôi trong nhà", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(473), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2008"), "Pomeranian", "Nhỏ, lanh lợi, bộ lông dày", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(475), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2009"), "Corgi", "Thân thiện, vui vẻ, chân ngắn", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(478), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2010"), "Dachshund", "Dài người, nhanh nhẹn, bám chủ", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(480), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2011"), "Siberian Husky", "Năng động, đẹp, cần vận động nhiều", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(483), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2012"), "Pug", "Mặt xệ, thân thiện, thích ở gần người", new DateTime(2026, 4, 16, 7, 45, 51, 368, DateTimeKind.Utc).AddTicks(489), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2001"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2002"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2003"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2004"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2005"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2006"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2007"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2008"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2009"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2010"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2011"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2012"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_species",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001"));
        }
    }
}
