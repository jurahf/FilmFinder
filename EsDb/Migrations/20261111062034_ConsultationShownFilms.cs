using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsDb.Migrations
{
    /// <inheritdoc />
    public partial class ConsultationShownFilms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultationShownFilms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    ShownDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationShownFilms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationShownFilms_Consultations_ConsultationId",
                        column: x => x.ConsultationId,
                        principalTable: "Consultations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationShownFilms_ConsultationId_FilmId",
                table: "ConsultationShownFilms",
                columns: new[] { "ConsultationId", "FilmId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationShownFilms_ConsultationId_Order",
                table: "ConsultationShownFilms",
                columns: new[] { "ConsultationId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationShownFilms");
        }
    }
}
