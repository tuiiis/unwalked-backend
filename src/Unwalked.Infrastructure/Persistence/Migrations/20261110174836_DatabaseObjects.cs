using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unwalked.Infrastructure.Persistence.Migrations
{
    public partial class DatabaseObjects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION mask_email(email text) RETURNS text
                LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
                RETURN left(email, 1) || '***@' || split_part(email, '@', 2);
                """);

            migrationBuilder.Sql("""
                CREATE FUNCTION region_pack_retire_previous() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    IF TG_OP = 'INSERT' AND EXISTS (
                        SELECT 1 FROM region_pack
                        WHERE region_id = NEW.region_id AND version = NEW.version
                    ) THEN
                        RETURN NEW;
                    END IF;

                    UPDATE region_pack
                    SET is_current = false
                    WHERE region_id = NEW.region_id AND id <> NEW.id AND is_current;
                    RETURN NEW;
                END
                $$;

                CREATE TRIGGER trg_region_pack_retire_previous
                BEFORE INSERT OR UPDATE OF is_current ON region_pack
                FOR EACH ROW WHEN (NEW.is_current)
                EXECUTE FUNCTION region_pack_retire_previous();
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE aggregate_feedback(cutoff date)
                LANGUAGE sql AS $$
                    WITH moved AS (
                        DELETE FROM recommendation_feedback
                        WHERE received_on < cutoff
                        RETURNING received_on, recommendation_type, action, category_id
                    )
                    INSERT INTO feedback_daily_total (received_on, recommendation_type, action, category_id, event_count)
                    SELECT received_on, recommendation_type, action, category_id, count(*)
                    FROM moved
                    GROUP BY received_on, recommendation_type, action, category_id
                    ON CONFLICT (received_on, recommendation_type, action, category_id)
                    DO UPDATE SET event_count = feedback_daily_total.event_count + EXCLUDED.event_count;
                $$;
                """);

            migrationBuilder.Sql("""
                CREATE VIEW v_region_pack_index AS
                SELECT region.code AS region_code,
                       region.name AS region_name,
                       region.bounds,
                       pack.version,
                       pack.built_at,
                       pack.size_bytes,
                       pack.sha256,
                       pack.object_key
                FROM region_pack pack
                JOIN region ON region.id = pack.region_id
                WHERE pack.is_current;

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

                CREATE VIEW v_user_account_masked WITH (security_barrier) AS
                SELECT account.id,
                       mask_email(account.email) AS email,
                       account.created_at,
                       count(share.id) AS share_count
                FROM user_account account
                LEFT JOIN achievement_share share ON share.owner_id = account.id
                GROUP BY account.id;

                CREATE VIEW v_feedback_summary AS
                SELECT recommendation_type, action, sum(event_count)::bigint AS event_count
                FROM (
                    SELECT recommendation_type, action, count(*) AS event_count
                    FROM recommendation_feedback
                    GROUP BY recommendation_type, action
                    UNION ALL
                    SELECT recommendation_type, action, sum(event_count)
                    FROM feedback_daily_total
                    GROUP BY recommendation_type, action
                ) counts
                GROUP BY recommendation_type, action;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP VIEW v_feedback_summary;
                DROP VIEW v_user_account_masked;
                DROP VIEW v_place_detail;
                DROP VIEW v_region_pack_index;
                DROP PROCEDURE aggregate_feedback(date);
                DROP TRIGGER trg_region_pack_retire_previous ON region_pack;
                DROP FUNCTION region_pack_retire_previous();
                DROP FUNCTION mask_email(text);
                """);
        }
    }
}
