using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandDogBreedSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "petcare",
                table: "pet_breeds",
                columns: new[] { "id", "breed_name", "characteristics", "created_at", "species_id" },
                values: new object[,]
                {
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2013"), "Akita Inu", "Điềm tĩnh, trung thành, tự lập", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8580), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2014"), "Shiba Inu", "Lanh lợi, sạch sẽ, độc lập", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8582), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2015"), "Border Collie", "Rất thông minh, nhanh nhẹn, dễ huấn luyện", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8584), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2016"), "Rottweiler", "Mạnh mẽ, canh gác tốt, trung thành", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8588), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2017"), "Doberman Pinscher", "Nhanh, lanh lợi, bảo vệ tốt", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8590), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2018"), "Maltese", "Nhỏ, mềm mại, tình cảm", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8593), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2019"), "Bichon Frise", "Vui vẻ, lông xoăn, hợp nuôi trong nhà", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8595), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2020"), "Yorkshire Terrier", "Nhỏ gọn, lanh lợi, giàu năng lượng", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8597), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2021"), "Miniature Schnauzer", "Thông minh, cảnh giác, dễ dạy", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8600), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2022"), "Cavalier King Charles Spaniel", "Hiền, quấn chủ, thân thiện", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8603), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2023"), "English Cocker Spaniel", "Vui vẻ, hiền, thích vận động", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8605), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2024"), "Shetland Sheepdog", "Thông minh, ngoan, thích học lệnh", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8611), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2025"), "Chihuahua", "Rất nhỏ, lanh lợi, bám chủ", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8614), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2026"), "Samoyed", "Lông trắng, thân thiện, hoạt bát", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8616), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2027"), "Great Dane", "Rất lớn, hiền, điềm đạm", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8618), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2028"), "Boxer", "Năng động, mạnh mẽ, thân thiện", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8620), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2029"), "Dalmatian", "Nổi bật, năng động, thích vận động", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8623), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2030"), "Basset Hound", "Tai dài, hiền, thích ngửi tìm mùi", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8625), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2031"), "Basenji", "Sạch sẽ, yên tĩnh, độc lập", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8627), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") },
                    { new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2032"), "Jack Russell Terrier", "Rất hiếu động, thông minh, nhanh nhẹn", new DateTime(2026, 4, 16, 8, 37, 4, 375, DateTimeKind.Utc).AddTicks(8631), new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f1001") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2013"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2014"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2015"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2016"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2017"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2018"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2019"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2020"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2021"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2022"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2023"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2024"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2025"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2026"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2027"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2028"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2029"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2030"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2031"));

            migrationBuilder.DeleteData(
                schema: "petcare",
                table: "pet_breeds",
                keyColumn: "id",
                keyValue: new Guid("7d8f6c44-3f6f-4a8c-9d1a-5b8fb78f2032"));
        }
    }
}
