using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Unwalked.Infrastructure.Persistence.Migrations
{
    public partial class BuildingsAndOpeningHours : Migration
    {
        private const string AppRead = DatabaseRoleNames.AppRead;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "opening_hours",
                table: "place",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "building",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    footprint = table.Column<MultiPolygon>(type: "geography (multipolygon, 4326)", nullable: false),
                    h3_r11 = table.Column<long>(type: "bigint", nullable: false),
                    region_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_building", x => x.id);
                    table.CheckConstraint("ck_building_footprint_valid", "ST_IsValid(footprint::geometry)");
                    table.ForeignKey(
                        name: "fk_building_region",
                        column: x => x.region_id,
                        principalTable: "region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_place_opening_hours_not_blank",
                table: "place",
                sql: "opening_hours IS NULL OR btrim(opening_hours) <> ''");

            migrationBuilder.CreateIndex(
                name: "ix_building_external_id",
                table: "building",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_building_footprint",
                table: "building",
                column: "footprint")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_building_h3_r11",
                table: "building",
                column: "h3_r11");

            migrationBuilder.CreateIndex(
                name: "ix_building_region_id",
                table: "building",
                column: "region_id");

            migrationBuilder.Sql($"""
                CREATE OR REPLACE VIEW v_place_detail AS
                SELECT place.id,
                       place.external_id,
                       place.name,
                       place.confidence,
                       place.location,
                       place.h3_r11,
                       region.code AS region_code,
                       category.code AS primary_category,
                       place.opening_hours
                FROM place
                JOIN region ON region.id = place.region_id
                JOIN category ON category.id = place.primary_category_id;

                GRANT SELECT ON building TO {AppRead};
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                REVOKE ALL ON building FROM {AppRead};
                DROP VIEW v_place_detail;
                CREATE VIEW v_place_detail AS
                SELECT place.id,
                       place.external_id,
                       place.name,
                       place.confidence,
                       place.location,
                       place.h3_r11,
                       region.code AS region_code,
                       category.code AS primary_category
                FROM place
                JOIN region ON region.id = place.region_id
                JOIN category ON category.id = place.primary_category_id;

                GRANT SELECT ON v_place_detail TO {AppRead};
                """);

            migrationBuilder.DropTable(
                name: "building");

            migrationBuilder.DropCheckConstraint(
                name: "ck_place_opening_hours_not_blank",
                table: "place");

            migrationBuilder.DropColumn(
                name: "opening_hours",
                table: "place");
        }
    }
}
