using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unwalked.Infrastructure.Persistence.Migrations
{
    public partial class DatabaseRoles : Migration
    {
        private const string AppRead = DatabaseRoleNames.AppRead;
        private const string AppWrite = DatabaseRoleNames.AppWrite;
        private const string AppAnalyst = DatabaseRoleNames.AppAnalyst;
        private const string Admin = DatabaseRoleNames.Admin;
        private const string AppReadable = "place, category, place_alternate_category, region, region_pack, user_account, achievement_share, recommendation_feedback, v_region_pack_index, v_place_detail";
        private const string AnalystReadable = "category, recommendation_feedback, feedback_daily_total, v_feedback_summary, v_user_account_masked";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{AppRead}') THEN
                        CREATE ROLE {AppRead} NOLOGIN;
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{AppWrite}') THEN
                        CREATE ROLE {AppWrite} NOLOGIN IN ROLE {AppRead};
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{AppAnalyst}') THEN
                        CREATE ROLE {AppAnalyst} NOLOGIN;
                    END IF;
                    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '{Admin}') THEN
                        CREATE ROLE {Admin} NOLOGIN;
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql($"""
                GRANT USAGE ON SCHEMA public TO {AppRead}, {AppAnalyst};
                GRANT SELECT ON {AppReadable} TO {AppRead};
                GRANT INSERT, DELETE ON user_account TO {AppWrite};
                GRANT INSERT, DELETE ON achievement_share TO {AppWrite};
                GRANT INSERT ON recommendation_feedback TO {AppWrite};
                GRANT SELECT ON {AnalystReadable} TO {AppAnalyst};
                REVOKE ALL ON FUNCTION mask_email(text), region_pack_retire_previous() FROM PUBLIC;
                REVOKE ALL ON PROCEDURE aggregate_feedback(date) FROM PUBLIC;
                GRANT EXECUTE ON FUNCTION mask_email(text) TO {AppAnalyst};
                GRANT ALL ON SCHEMA public TO {Admin};
                GRANT ALL ON ALL TABLES IN SCHEMA public TO {Admin};
                GRANT ALL ON ALL SEQUENCES IN SCHEMA public TO {Admin};
                GRANT ALL ON ALL ROUTINES IN SCHEMA public TO {Admin};
                ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO {Admin};
                ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO {Admin};
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON SEQUENCES FROM {Admin};
                ALTER DEFAULT PRIVILEGES IN SCHEMA public REVOKE ALL ON TABLES FROM {Admin};
                REVOKE ALL ON ALL ROUTINES IN SCHEMA public FROM {Admin};
                REVOKE ALL ON ALL SEQUENCES IN SCHEMA public FROM {Admin};
                REVOKE ALL ON ALL TABLES IN SCHEMA public FROM {Admin};
                REVOKE ALL ON SCHEMA public FROM {Admin};
                REVOKE EXECUTE ON FUNCTION mask_email(text) FROM {AppAnalyst};
                REVOKE ALL ON {AnalystReadable} FROM {AppAnalyst};
                REVOKE ALL ON {AppReadable} FROM {AppWrite};
                REVOKE ALL ON {AppReadable} FROM {AppRead};
                REVOKE USAGE ON SCHEMA public FROM {AppRead}, {AppAnalyst};
                """);
        }
    }
}
