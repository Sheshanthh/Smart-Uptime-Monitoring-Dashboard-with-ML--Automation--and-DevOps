using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartUptime.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddScriptRunnerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmergencyScripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ScriptType = table.Column<string>(type: "text", nullable: false),
                    ScriptPath = table.Column<string>(type: "text", nullable: false),
                    DefaultArguments = table.Column<string>(type: "text", nullable: true),
                    TriggerCondition = table.Column<string>(type: "text", nullable: false),
                    AnomalyThreshold = table.Column<int>(type: "integer", nullable: false),
                    DowntimeThreshold = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastExecuted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    FailureCount = table.Column<int>(type: "integer", nullable: false),
                    AverageExecutionTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyScripts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScriptExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScriptName = table.Column<string>(type: "text", nullable: false),
                    ScriptPath = table.Column<string>(type: "text", nullable: false),
                    Arguments = table.Column<string>(type: "text", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TriggerType = table.Column<string>(type: "text", nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    PingResultId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExitCode = table.Column<int>(type: "integer", nullable: false),
                    Output = table.Column<string>(type: "text", nullable: true),
                    ErrorOutput = table.Column<string>(type: "text", nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScriptExecutions_PingResults_PingResultId",
                        column: x => x.PingResultId,
                        principalTable: "PingResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScriptExecutions_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScriptExecutions_PingResultId",
                table: "ScriptExecutions",
                column: "PingResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ScriptExecutions_SiteId",
                table: "ScriptExecutions",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyScripts");

            migrationBuilder.DropTable(
                name: "ScriptExecutions");
        }
    }
}
