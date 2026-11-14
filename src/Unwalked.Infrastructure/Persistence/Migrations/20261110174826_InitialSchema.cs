using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Unwalked.Infrastructure.Persistence.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category", x => x.id);
                    table.CheckConstraint("ck_category_code_format", "code ~ '^[a-z0-9_]{1,64}$'");
                });

            migrationBuilder.CreateTable(
                name: "region",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bounds = table.Column<Polygon>(type: "geography (polygon, 4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_region", x => x.id);
                    table.CheckConstraint("ck_region_code_format", "code ~ '^[a-z0-9-]{2,32}$'");
                    table.CheckConstraint("ck_region_name_not_blank", "btrim(name) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "user_account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_account", x => x.id);
                    table.CheckConstraint("ck_user_account_email_lowercase", "email = lower(email)");
                });

            migrationBuilder.CreateTable(
                name: "feedback_daily_total",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    received_on = table.Column<DateOnly>(type: "date", nullable: false),
                    recommendation_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    action = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    event_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_feedback_daily_total", x => x.id);
                    table.CheckConstraint("ck_feedback_daily_total_action", "action IN ('accepted', 'dismissed', 'visited')");
                    table.CheckConstraint("ck_feedback_daily_total_count_positive", "event_count > 0");
                    table.CheckConstraint("ck_feedback_daily_total_type", "recommendation_type IN ('nearby_new_place', 'along_habitual_route', 'unvisited_cell', 'walking_route')");
                    table.ForeignKey(
                        name: "fk_feedback_daily_total_category",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "recommendation_feedback",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recommendation_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    action = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    received_on = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recommendation_feedback", x => x.id);
                    table.CheckConstraint("ck_recommendation_feedback_action", "action IN ('accepted', 'dismissed', 'visited')");
                    table.CheckConstraint("ck_recommendation_feedback_type", "recommendation_type IN ('nearby_new_place', 'along_habitual_route', 'unvisited_cell', 'walking_route')");
                    table.ForeignKey(
                        name: "fk_recommendation_feedback_category",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "place",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: false),
                    location = table.Column<Point>(type: "geography (point, 4326)", nullable: false),
                    h3_r11 = table.Column<long>(type: "bigint", nullable: false),
                    region_id = table.Column<int>(type: "integer", nullable: false),
                    primary_category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_place", x => x.id);
                    table.CheckConstraint("ck_place_confidence_range", "confidence BETWEEN 0 AND 1");
                    table.CheckConstraint("ck_place_name_not_blank", "btrim(name) <> ''");
                    table.ForeignKey(
                        name: "fk_place_primary_category",
                        column: x => x.primary_category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_place_region",
                        column: x => x.region_id,
                        principalTable: "region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "region_pack",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    region_id = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    built_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    object_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_region_pack", x => x.id);
                    table.CheckConstraint("ck_region_pack_sha256_hex", "sha256 ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_region_pack_size_positive", "size_bytes > 0");
                    table.CheckConstraint("ck_region_pack_version_positive", "version > 0");
                    table.ForeignKey(
                        name: "fk_region_pack_region",
                        column: x => x.region_id,
                        principalTable: "region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "achievement_share",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    achieved_on = table.Column<DateOnly>(type: "date", nullable: false),
                    image = table.Column<byte[]>(type: "bytea", nullable: false),
                    image_content_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_achievement_share", x => x.id);
                    table.CheckConstraint("ck_achievement_share_image_size", "octet_length(image) BETWEEN 1 AND 307200");
                    table.CheckConstraint("ck_achievement_share_image_type", "image_content_type IN ('image/png', 'image/jpeg', 'image/webp')");
                    table.CheckConstraint("ck_achievement_share_name_not_blank", "btrim(achievement_name) <> ''");
                    table.ForeignKey(
                        name: "fk_achievement_share_owner",
                        column: x => x.owner_id,
                        principalTable: "user_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "place_alternate_category",
                columns: table => new
                {
                    place_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_place_alternate_category", x => new { x.place_id, x.category_id });
                    table.ForeignKey(
                        name: "fk_place_alternate_category_category",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_place_alternate_category_place",
                        column: x => x.place_id,
                        principalTable: "place",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_achievement_share_owner_id",
                table: "achievement_share",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_code",
                table: "category",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_daily_total_category_id",
                table: "feedback_daily_total",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ux_feedback_daily_total_day_type_action_category",
                table: "feedback_daily_total",
                columns: new[] { "received_on", "recommendation_type", "action", "category_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_place_external_id",
                table: "place",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_place_h3_r11",
                table: "place",
                column: "h3_r11");

            migrationBuilder.CreateIndex(
                name: "ix_place_location",
                table: "place",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_place_primary_category_id",
                table: "place",
                column: "primary_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_place_region_id",
                table: "place",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "ix_place_alternate_category_category_id_place_id",
                table: "place_alternate_category",
                columns: new[] { "category_id", "place_id" });

            migrationBuilder.CreateIndex(
                name: "ix_recommendation_feedback_category_id",
                table: "recommendation_feedback",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_recommendation_feedback_event_id",
                table: "recommendation_feedback",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recommendation_feedback_received_on_recommendation_type",
                table: "recommendation_feedback",
                columns: new[] { "received_on", "recommendation_type" });

            migrationBuilder.CreateIndex(
                name: "ix_region_code",
                table: "region",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_region_pack_object_key",
                table: "region_pack",
                column: "object_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_region_pack_region_id_version",
                table: "region_pack",
                columns: new[] { "region_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_region_pack_current",
                table: "region_pack",
                column: "region_id",
                unique: true,
                filter: "is_current");

            migrationBuilder.CreateIndex(
                name: "ix_user_account_email",
                table: "user_account",
                column: "email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "achievement_share");

            migrationBuilder.DropTable(
                name: "feedback_daily_total");

            migrationBuilder.DropTable(
                name: "place_alternate_category");

            migrationBuilder.DropTable(
                name: "recommendation_feedback");

            migrationBuilder.DropTable(
                name: "region_pack");

            migrationBuilder.DropTable(
                name: "user_account");

            migrationBuilder.DropTable(
                name: "place");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "region");
        }
    }
}
