using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataToThirdPartyModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Update Role: Change 'doctor' to 'service_provider'
            migrationBuilder.Sql(@"
                UPDATE petcare.roles 
                SET 
                    role_name = 'service_provider',
                    description = 'Nhà cung cấp dịch vụ (Grooming, Pet Hotel, etc.)'
                WHERE role_name = 'doctor';
            ");

            // 2. Delete old medical service categories and their services
            migrationBuilder.Sql(@"
                DELETE FROM petcare.services 
                WHERE category_id IN (
                    SELECT id FROM petcare.service_categories 
                    WHERE category_name IN ('Khám bệnh', 'Tiêm phòng', 'Phẫu thuật')
                );
                
                DELETE FROM petcare.service_categories 
                WHERE category_name IN ('Khám bệnh', 'Tiêm phòng', 'Phẫu thuật');
            ");

            // 3. Insert new service categories
            migrationBuilder.Sql(@"
                INSERT INTO petcare.service_categories (id, category_name, description, icon_url, created_at)
                SELECT gen_random_uuid(), 'Huấn luyện', 'Dịch vụ huấn luyện thú cưng', '/icons/training.svg', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM petcare.service_categories WHERE category_name = 'Huấn luyện');
                
                INSERT INTO petcare.service_categories (id, category_name, description, icon_url, created_at)
                SELECT gen_random_uuid(), 'Tư vấn sức khỏe', 'Tư vấn và giới thiệu dịch vụ thú y đối tác', '/icons/consultation.svg', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM petcare.service_categories WHERE category_name = 'Tư vấn sức khỏe');
                
                INSERT INTO petcare.service_categories (id, category_name, description, icon_url, created_at)
                SELECT gen_random_uuid(), 'Dịch vụ tại nhà', 'Các dịch vụ chăm sóc tại nhà', '/icons/home-service.svg', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM petcare.service_categories WHERE category_name = 'Dịch vụ tại nhà');
            ");

            // 4. Insert new services
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    v_spa_id UUID;
                    v_hotel_id UUID;
                    v_consultation_id UUID;
                BEGIN
                    SELECT id INTO v_spa_id FROM petcare.service_categories WHERE category_name = 'Spa & Chăm sóc' LIMIT 1;
                    SELECT id INTO v_hotel_id FROM petcare.service_categories WHERE category_name = 'Khách sạn thú cưng' LIMIT 1;
                    SELECT id INTO v_consultation_id FROM petcare.service_categories WHERE category_name = 'Tư vấn sức khỏe' LIMIT 1;
                    
                    IF v_spa_id IS NOT NULL THEN
                        INSERT INTO petcare.services (id, category_id, service_name, description, duration_minutes, price, is_active, created_at)
                        SELECT gen_random_uuid(), v_spa_id, 'Spa thư giãn', 'Massage, chăm sóc da lông cao cấp', 60, 250000, true, NOW()
                        WHERE NOT EXISTS (SELECT 1 FROM petcare.services WHERE service_name = 'Spa thư giãn');
                    END IF;
                    
                    IF v_hotel_id IS NOT NULL THEN
                        INSERT INTO petcare.services (id, category_id, service_name, description, duration_minutes, price, is_active, created_at)
                        SELECT gen_random_uuid(), v_hotel_id, 'Lưu trú thú cưng', 'Dịch vụ lưu trú theo ngày', 1440, 200000, true, NOW()
                        WHERE NOT EXISTS (SELECT 1 FROM petcare.services WHERE service_name = 'Lưu trú thú cưng');
                    END IF;
                    
                    IF v_consultation_id IS NOT NULL THEN
                        INSERT INTO petcare.services (id, category_id, service_name, description, duration_minutes, price, is_active, created_at)
                        SELECT gen_random_uuid(), v_consultation_id, 'Tư vấn sức khỏe', 'Tư vấn và giới thiệu bác sĩ thú y uy tín', 30, 0, true, NOW()
                        WHERE NOT EXISTS (SELECT 1 FROM petcare.services WHERE service_name = 'Tư vấn sức khỏe');
                    END IF;
                END $$;
            ");

            // 5. Update FAQ items
            migrationBuilder.Sql(@"
                UPDATE petcare.faq_items 
                SET 
                    question = 'PetCare cung cấp những dịch vụ gì?',
                    answer = 'PetCare là nền tảng kết nối cung cấp dịch vụ grooming, spa, khách sạn thú cưng, huấn luyện và tư vấn sức khỏe. Chúng tôi không cung cấp dịch vụ khám chữa bệnh trực tiếp nhưng có thể giới thiệu các phòng khám thú y uy tín.',
                    category = 'Dịch vụ',
                    keywords = ARRAY['dịch vụ', 'grooming', 'spa', 'khách sạn']
                WHERE question LIKE '%tiêm phòng%';
                
                DELETE FROM petcare.faq_items 
                WHERE category = 'Sức khỏe' AND question LIKE '%bác sĩ%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Restore doctor role
            migrationBuilder.Sql(@"
                UPDATE petcare.roles 
                SET 
                    role_name = 'doctor',
                    description = 'Bác sĩ thú y'
                WHERE role_name = 'service_provider';
            ");
        }
    }
}
