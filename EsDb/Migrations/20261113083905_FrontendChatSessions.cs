using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsDb.Migrations
{
    /// <inheritdoc />
    public partial class FrontendChatSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Sessions_SessionId",
                table: "Sessions",
                column: "SessionId");

            migrationBuilder.CreateTable(
                name: "FrontendChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Frontend = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrontendChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrontendChatSessions_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FrontendChatSessions_Frontend_ChatId",
                table: "FrontendChatSessions",
                columns: new[] { "Frontend", "ChatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrontendChatSessions_SessionId",
                table: "FrontendChatSessions",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FrontendChatSessions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Sessions_SessionId",
                table: "Sessions");
        }
    }
}
