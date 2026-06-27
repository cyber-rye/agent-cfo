using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgentCfo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StripeCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StripeAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MonthlyBudget_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MonthlyBudget_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RunwayThresholdDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorType = table.Column<int>(type: "integer", nullable: false),
                    ActorId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeforeState = table.Column<string>(type: "text", nullable: true),
                    AfterState = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEntries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    MonthlyLimit_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MonthlyLimit_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrentSpend_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentSpend_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Period = table.Column<int>(type: "integer", nullable: false),
                    AlertThresholdPercent = table.Column<int>(type: "integer", nullable: false),
                    HardLimit = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedBy = table.Column<int>(type: "integer", nullable: false),
                    CurrentCashBalance_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentCashBalance_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MonthlyBurnRate_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MonthlyBurnRate_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MonthlyRevenue_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MonthlyRevenue_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RunwayDays = table.Column<int>(type: "integer", nullable: false),
                    RunwayEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Scenario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Confidence = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forecasts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StripeEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Amount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StripeInvoiceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectionPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ProjectedBalance_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProjectedBalance_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ProjectedRevenue_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProjectedRevenue_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ProjectedExpenses_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProjectedExpenses_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ForecastId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectionPoints_Forecasts_ForecastId",
                        column: x => x.ForecastId,
                        principalTable: "Forecasts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgentDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Reasoning = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    RelatedTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedBudgetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OverriddenBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentDecisions_Budgets_RelatedBudgetId",
                        column: x => x.RelatedBudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AgentDecisions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgentDecisions_Transactions_RelatedTransactionId",
                        column: x => x.RelatedTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentDecisions_OrganizationId",
                table: "AgentDecisions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentDecisions_RelatedBudgetId",
                table: "AgentDecisions",
                column: "RelatedBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentDecisions_RelatedTransactionId",
                table: "AgentDecisions",
                column: "RelatedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_CorrelationId",
                table: "AuditEntries",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_OrganizationId_CreatedAt",
                table: "AuditEntries",
                columns: new[] { "OrganizationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_OrganizationId_Category",
                table: "Budgets",
                columns: new[] { "OrganizationId", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_OrganizationId",
                table: "Forecasts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_StripeCustomerId",
                table: "Organizations",
                column: "StripeCustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectionPoints_ForecastId",
                table: "ProjectionPoints",
                column: "ForecastId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrganizationId_OccurredAt",
                table: "Transactions",
                columns: new[] { "OrganizationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StripeEventId",
                table: "Transactions",
                column: "StripeEventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentDecisions");

            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "ProjectionPoints");

            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
