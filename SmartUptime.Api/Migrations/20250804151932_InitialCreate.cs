using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartUptime.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmergencyScripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ScriptType = table.Column<string>(type: "TEXT", nullable: false),
                    ScriptPath = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultArguments = table.Column<string>(type: "TEXT", nullable: true),
                    TriggerCondition = table.Column<string>(type: "TEXT", nullable: false),
                    AnomalyThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    DowntimeThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastExecuted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExecutionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FailureCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageExecutionTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyScripts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PingResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LatencyMs = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAnomaly = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PingResults_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScriptExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScriptName = table.Column<string>(type: "TEXT", nullable: false),
                    ScriptPath = table.Column<string>(type: "TEXT", nullable: false),
                    Arguments = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TriggerType = table.Column<string>(type: "TEXT", nullable: false),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: true),
                    PingResultId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ExitCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Output = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorOutput = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "IX_PingResults_SiteId",
                table: "PingResults",
                column: "SiteId");

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

            migrationBuilder.DropTable(
                name: "PingResults");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
