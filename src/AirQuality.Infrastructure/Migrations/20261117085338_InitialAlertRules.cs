using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AirQuality.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAlertRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alert_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    pollutant = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    threshold = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_rules", x => x.id);
                    table.CheckConstraint("ck_alert_rules_threshold", "threshold > 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_rules");
        }
    }
}
