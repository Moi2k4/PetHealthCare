using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "petcare");

            migrationBuilder.CreateTable(
                name: "blog_categories",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    opening_hours = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    brand_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faq_items",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    answer = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    keywords = table.Column<string[]>(type: "text[]", nullable: true),
                    usage_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faq_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pet_species",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pet_species", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_categories",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_categories_product_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalSchema: "petcare",
                        principalTable: "product_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_categories",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    icon_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pet_breeds",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<Guid>(type: "uuid", nullable: false),
                    breed_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    characteristics = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pet_breeds", x => x.id);
                    table.ForeignKey(
                        name: "FK_pet_breeds_pet_species_species_id",
                        column: x => x.species_id,
                        principalSchema: "petcare",
                        principalTable: "pet_species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    sale_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    weight = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    dimensions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_brands_brand_id",
                        column: x => x.brand_id,
                        principalSchema: "petcare",
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_products_product_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "petcare",
                        principalTable: "product_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "petcare",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    is_home_service = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_services", x => x.id);
                    table.ForeignKey(
                        name: "FK_services_service_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "petcare",
                        principalTable: "service_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_images_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "petcare",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_posts",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    featured_image_url = table.Column<string>(type: "text", nullable: true),
                    excerpt = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_blog_posts_blog_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "petcare",
                        principalTable: "blog_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_blog_posts_users_author_id",
                        column: x => x.author_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cart_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_cart_items_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "petcare",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cart_items_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_sessions",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    session_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    link_url = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    final_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    shipping_address = table.Column<string>(type: "text", nullable: false),
                    shipping_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shipping_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    ordered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pets",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<Guid>(type: "uuid", nullable: true),
                    breed_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pet_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    color = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    microchip_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    special_notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pets", x => x.id);
                    table.ForeignKey(
                        name: "FK_pets_pet_breeds_breed_id",
                        column: x => x.breed_id,
                        principalSchema: "petcare",
                        principalTable: "pet_breeds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pets_pet_species_species_id",
                        column: x => x.species_id,
                        principalSchema: "petcare",
                        principalTable: "pet_species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pets_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff_schedules",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    work_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_schedules_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "petcare",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_schedules_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff_services",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_services", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_services_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "petcare",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_staff_services_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_comments",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comment_text = table.Column<string>(type: "text", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_blog_comments_blog_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalSchema: "petcare",
                        principalTable: "blog_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_blog_comments_blog_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "petcare",
                        principalTable: "blog_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_blog_comments_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "blog_likes",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_likes", x => x.id);
                    table.ForeignKey(
                        name: "FK_blog_likes_blog_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "petcare",
                        principalTable: "blog_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_blog_likes_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_post_tags",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blog_post_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_blog_post_tags_blog_posts_post_id",
                        column: x => x.post_id,
                        principalSchema: "petcare",
                        principalTable: "blog_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_blog_post_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "petcare",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message_text = table.Column<string>(type: "text", nullable: false),
                    message_metadata = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_chat_messages_chat_sessions_session_id",
                        column: x => x.session_id,
                        principalSchema: "petcare",
                        principalTable: "chat_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "petcare",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_items_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "petcare",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_status_history",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_status_history_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "petcare",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_status_history_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_reviews",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    review_text = table.Column<string>(type: "text", nullable: true),
                    images = table.Column<string[]>(type: "text[]", nullable: true),
                    is_verified_purchase = table.Column<bool>(type: "boolean", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_reviews_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "petcare",
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_reviews_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "petcare",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_reviews_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    appointment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    appointment_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_staff_id = table.Column<Guid>(type: "uuid", nullable: true),
                    appointment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    service_address = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.id);
                    table.ForeignKey(
                        name: "FK_appointments_branches_branch_id",
                        column: x => x.branch_id,
                        principalSchema: "petcare",
                        principalTable: "branches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "petcare",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "petcare",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_assigned_staff_id",
                        column: x => x.assigned_staff_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "health_records",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    record_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    height = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    temperature = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    heart_rate = table.Column<int>(type: "integer", nullable: true),
                    diagnosis = table.Column<string>(type: "text", nullable: true),
                    treatment = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    recorded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_health_records_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "petcare",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_health_records_users_recorded_by",
                        column: x => x.recorded_by,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "health_reminders",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reminder_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reminder_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    reminder_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_reminders", x => x.id);
                    table.ForeignKey(
                        name: "FK_health_reminders_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "petcare",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vaccinations",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vaccine_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    vaccination_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    administered_by = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vaccinations", x => x.id);
                    table.ForeignKey(
                        name: "FK_vaccinations_pets_pet_id",
                        column: x => x.pet_id,
                        principalSchema: "petcare",
                        principalTable: "pets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vaccinations_users_administered_by",
                        column: x => x.administered_by,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointment_status_history",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_appointment_status_history_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "petcare",
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_appointment_status_history_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "service_reviews",
                schema: "petcare",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    review_text = table.Column<string>(type: "text", nullable: true),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_service_reviews_appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalSchema: "petcare",
                        principalTable: "appointments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_service_reviews_services_service_id",
                        column: x => x.service_id,
                        principalSchema: "petcare",
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_reviews_users_staff_id",
                        column: x => x.staff_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_service_reviews_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "petcare",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_status_history_appointment_id",
                schema: "petcare",
                table: "appointment_status_history",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_status_history_updated_by",
                schema: "petcare",
                table: "appointment_status_history",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_appointment_date",
                schema: "petcare",
                table: "appointments",
                column: "appointment_date");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_assigned_staff_id",
                schema: "petcare",
                table: "appointments",
                column: "assigned_staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_branch_id",
                schema: "petcare",
                table: "appointments",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_pet_id",
                schema: "petcare",
                table: "appointments",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_service_id",
                schema: "petcare",
                table: "appointments",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_user_id",
                schema: "petcare",
                table: "appointments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_categories_slug",
                schema: "petcare",
                table: "blog_categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blog_comments_parent_comment_id",
                schema: "petcare",
                table: "blog_comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_comments_post_id",
                schema: "petcare",
                table: "blog_comments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_comments_user_id",
                schema: "petcare",
                table: "blog_comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_likes_post_id_user_id",
                schema: "petcare",
                table: "blog_likes",
                columns: new[] { "post_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blog_likes_user_id",
                schema: "petcare",
                table: "blog_likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_post_tags_post_id_tag_id",
                schema: "petcare",
                table: "blog_post_tags",
                columns: new[] { "post_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blog_post_tags_tag_id",
                schema: "petcare",
                table: "blog_post_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_author_id",
                schema: "petcare",
                table: "blog_posts",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_category_id",
                schema: "petcare",
                table: "blog_posts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_slug",
                schema: "petcare",
                table: "blog_posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_blog_posts_status",
                schema: "petcare",
                table: "blog_posts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_product_id",
                schema: "petcare",
                table: "cart_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_cart_items_user_id_product_id",
                schema: "petcare",
                table: "cart_items",
                columns: new[] { "user_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_messages_session_id",
                schema: "petcare",
                table: "chat_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_user_id",
                schema: "petcare",
                table: "chat_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_records_pet_id",
                schema: "petcare",
                table: "health_records",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_records_recorded_by",
                schema: "petcare",
                table: "health_records",
                column: "recorded_by");

            migrationBuilder.CreateIndex(
                name: "IX_health_reminders_pet_id",
                schema: "petcare",
                table: "health_reminders",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                schema: "petcare",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_read",
                schema: "petcare",
                table: "notifications",
                columns: new[] { "user_id", "is_read" },
                filter: "is_read = false");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                schema: "petcare",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                schema: "petcare",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_history_order_id",
                schema: "petcare",
                table: "order_status_history",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_history_updated_by",
                schema: "petcare",
                table: "order_status_history",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_number",
                schema: "petcare",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                schema: "petcare",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pet_breeds_species_id",
                schema: "petcare",
                table: "pet_breeds",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_breed_id",
                schema: "petcare",
                table: "pets",
                column: "breed_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_species_id",
                schema: "petcare",
                table: "pets",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "IX_pets_user_id",
                schema: "petcare",
                table: "pets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_categories_parent_category_id",
                schema: "petcare",
                table: "product_categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_product_id",
                schema: "petcare",
                table: "product_images",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_reviews_order_id",
                schema: "petcare",
                table: "product_reviews",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_reviews_product_id",
                schema: "petcare",
                table: "product_reviews",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_reviews_user_id",
                schema: "petcare",
                table: "product_reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_brand_id",
                schema: "petcare",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                schema: "petcare",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_sku",
                schema: "petcare",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_name",
                schema: "petcare",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_reviews_appointment_id",
                schema: "petcare",
                table: "service_reviews",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_reviews_service_id",
                schema: "petcare",
                table: "service_reviews",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_reviews_staff_id",
                schema: "petcare",
                table: "service_reviews",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_service_reviews_user_id",
                schema: "petcare",
                table: "service_reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_services_category_id",
                schema: "petcare",
                table: "services",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_schedules_branch_id",
                schema: "petcare",
                table: "staff_schedules",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_schedules_user_id",
                schema: "petcare",
                table: "staff_schedules",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_services_service_id",
                schema: "petcare",
                table: "staff_services",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_services_user_id_service_id",
                schema: "petcare",
                table: "staff_services",
                columns: new[] { "user_id", "service_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_slug",
                schema: "petcare",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_tag_name",
                schema: "petcare",
                table: "tags",
                column: "tag_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "petcare",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                schema: "petcare",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_vaccinations_administered_by",
                schema: "petcare",
                table: "vaccinations",
                column: "administered_by");

            migrationBuilder.CreateIndex(
                name: "IX_vaccinations_pet_id",
                schema: "petcare",
                table: "vaccinations",
                column: "pet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_status_history",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "blog_comments",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "blog_likes",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "blog_post_tags",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "cart_items",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "chat_messages",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "faq_items",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "health_records",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "health_reminders",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "order_items",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "order_status_history",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "product_images",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "product_reviews",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "service_reviews",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "staff_schedules",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "staff_services",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "vaccinations",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "blog_posts",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "chat_sessions",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "orders",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "products",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "appointments",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "blog_categories",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "brands",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "product_categories",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "branches",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "pets",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "services",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "pet_breeds",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "users",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "service_categories",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "pet_species",
                schema: "petcare");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "petcare");
        }
    }
}
