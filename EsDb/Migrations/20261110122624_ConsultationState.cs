using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsDb.Migrations
{
    /// <inheritdoc />
    public partial class ConsultationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultationAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ConsultationId = table.Column<int>(type: "int", nullable: false),
                    VariableId = table.Column<int>(type: "int", nullable: false),
                    DomainValueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationAnswers_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultationAnswers_DomainValues_DomainValueId",
                        column: x => x.DomainValueId,
                        principalTable: "DomainValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsultationAnswers_Variables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "Variables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConsultationUnknownVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultationId = table.Column<int>(type: "int", nullable: false),
                    VariableId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationUnknownVariables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationUnknownVariables_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultationUnknownVariables_Variables_VariableId",
                        column: x => x.VariableId,
                        principalTable: "Variables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAnswers_ConsultationId_Order",
                table: "ConsultationAnswers",
                columns: new[] { "ConsultationId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAnswers_DomainValueId",
                table: "ConsultationAnswers",
                column: "DomainValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationAnswers_VariableId",
                table: "ConsultationAnswers",
                column: "VariableId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationUnknownVariables_ConsultationId_VariableId",
                table: "ConsultationUnknownVariables",
                columns: new[] { "ConsultationId", "VariableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationUnknownVariables_VariableId",
                table: "ConsultationUnknownVariables",
                column: "VariableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationAnswers");

            migrationBuilder.DropTable(
                name: "ConsultationUnknownVariables");
        }
    }
}
