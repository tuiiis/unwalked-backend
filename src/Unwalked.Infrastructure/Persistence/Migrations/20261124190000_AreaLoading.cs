using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unwalked.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(UnwalkedDbContext))]
    [Migration("20261124190000_AreaLoading")]
    public partial class AreaLoading : Migration
    {
        private const string AppWrite = DatabaseRoleNames.AppWrite;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"GRANT INSERT ON region, place TO {AppWrite};");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"REVOKE INSERT ON region, place FROM {AppWrite};");
        }
    }
}
