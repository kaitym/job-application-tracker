using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusHistoryAndTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "deadline",
                table: "jobs",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "needs_action",
                table: "jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "responded_at",
                table: "jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "job_status_changes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    job_id = table.Column<int>(type: "integer", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_status_changes", x => x.id);
                    table.ForeignKey(
                        name: "fk_job_status_changes_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_job_status_changes_job_id",
                table: "job_status_changes",
                column: "job_id");

            // Give jobs that already exist a starting history entry
            migrationBuilder.Sql(
                "INSERT INTO job_status_changes (job_id, from_status, to_status, changed_at) " +
                "SELECT id, NULL, status, now() FROM jobs;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_status_changes");

            migrationBuilder.DropColumn(
                name: "deadline",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "needs_action",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "responded_at",
                table: "jobs");
        }
    }
}
