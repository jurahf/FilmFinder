using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsDb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estimate = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SessionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationFacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Truly = table.Column<int>(type: "int", nullable: false),
                    ConsultationId = table.Column<int>(type: "int", nullable: false),
                    FactId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationFacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Work = table.Column<int>(type: "int", nullable: false),
                    ConsultationId = table.Column<int>(type: "int", nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ExpertSystemId = table.Column<int>(type: "int", nullable: false),
                    CurrentRuleId = table.Column<int>(type: "int", nullable: true),
                    GoalStackId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultations_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinalSolutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalSolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinalSolutions_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpertSystemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DomainId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainValues_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpertSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpertSystems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Variables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reasoning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DomainId = table.Column<int>(type: "int", nullable: true),
                    ExpertSystemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Variables_Domains_DomainId",
                        column: x => x.DomainId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Variables_ExpertSystems_ExpertSystemId",
                        column: x => x.ExpertSystemId,
                        principalTable: "ExpertSystems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Facts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariableId = table.Column<int>(type: "int", nullable: false),
                    DomainValueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facts_DomainValues_DomainValueId",
                        column: x => x.DomainValueId,
                        principalTable: "DomainValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Facts_Variables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "Variables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GoalStacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VariableId = table.Column<int>(type: "int", nullable: true),
                    NextGoalId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalStacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalStacks_GoalStacks_NextGoalId",
                        column: x => x.NextGoalId,
                        principalTable: "GoalStacks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoalStacks_Variables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "Variables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultId = table.Column<int>(type: "int", nullable: true),
                    ExpertSystemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rules_ExpertSystems_ExpertSystemId",
                        column: x => x.ExpertSystemId,
                        principalTable: "ExpertSystems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rules_Facts_ResultId",
                        column: x => x.ResultId,
                        principalTable: "Facts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleFacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: true),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    FactId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleFacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleFacts_Facts_FactId",
                        column: x => x.FactId,
                        principalTable: "Facts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RuleFacts_Rules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "Rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationFacts_ConsultationId",
                table: "ConsultationFacts",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationFacts_FactId",
                table: "ConsultationFacts",
                column: "FactId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRules_ConsultationId",
                table: "ConsultationRules",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRules_RuleId",
                table: "ConsultationRules",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_CurrentRuleId",
                table: "Consultations",
                column: "CurrentRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ExpertSystemId",
                table: "Consultations",
                column: "ExpertSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_GoalStackId",
                table: "Consultations",
                column: "GoalStackId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_SessionId",
                table: "Consultations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Domains_ExpertSystemId_Name",
                table: "Domains",
                columns: new[] { "ExpertSystemId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_DomainValues_DomainId_Value",
                table: "DomainValues",
                columns: new[] { "DomainId", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpertSystems_Name",
                table: "ExpertSystems",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ExpertSystems_TargetId",
                table: "ExpertSystems",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Facts_DomainValueId",
                table: "Facts",
                column: "DomainValueId");

            migrationBuilder.CreateIndex(
                name: "IX_Facts_VariableId_DomainValueId",
                table: "Facts",
                columns: new[] { "VariableId", "DomainValueId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinalSolutions_ConsultationId",
                table: "FinalSolutions",
                column: "ConsultationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalStacks_NextGoalId",
                table: "GoalStacks",
                column: "NextGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalStacks_VariableId",
                table: "GoalStacks",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SessionId",
                table: "Reviews",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleFacts_FactId",
                table: "RuleFacts",
                column: "FactId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleFacts_RuleId",
                table: "RuleFacts",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_ExpertSystemId_Name",
                table: "Rules",
                columns: new[] { "ExpertSystemId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Rules_ResultId",
                table: "Rules",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionId",
                table: "Sessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variables_DomainId",
                table: "Variables",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Variables_ExpertSystemId_Name",
                table: "Variables",
                columns: new[] { "ExpertSystemId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationFacts_Consultations_ConsultationId",
                table: "ConsultationFacts",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationFacts_Facts_FactId",
                table: "ConsultationFacts",
                column: "FactId",
                principalTable: "Facts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationRules_Consultations_ConsultationId",
                table: "ConsultationRules",
                column: "ConsultationId",
                principalTable: "Consultations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationRules_Rules_RuleId",
                table: "ConsultationRules",
                column: "RuleId",
                principalTable: "Rules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_ExpertSystems_ExpertSystemId",
                table: "Consultations",
                column: "ExpertSystemId",
                principalTable: "ExpertSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_GoalStacks_GoalStackId",
                table: "Consultations",
                column: "GoalStackId",
                principalTable: "GoalStacks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Rules_CurrentRuleId",
                table: "Consultations",
                column: "CurrentRuleId",
                principalTable: "Rules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Domains_ExpertSystems_ExpertSystemId",
                table: "Domains",
                column: "ExpertSystemId",
                principalTable: "ExpertSystems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpertSystems_Variables_TargetId",
                table: "ExpertSystems",
                column: "TargetId",
                principalTable: "Variables",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Domains_ExpertSystems_ExpertSystemId",
                table: "Domains");

            migrationBuilder.DropForeignKey(
                name: "FK_Variables_ExpertSystems_ExpertSystemId",
                table: "Variables");

            migrationBuilder.DropTable(
                name: "ConsultationFacts");

            migrationBuilder.DropTable(
                name: "ConsultationRules");

            migrationBuilder.DropTable(
                name: "FinalSolutions");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RuleFacts");

            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropTable(
                name: "GoalStacks");

            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Facts");

            migrationBuilder.DropTable(
                name: "DomainValues");

            migrationBuilder.DropTable(
                name: "ExpertSystems");

            migrationBuilder.DropTable(
                name: "Variables");

            migrationBuilder.DropTable(
                name: "Domains");
        }
    }
}
