using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilmDb.Migrations
{
    /// <inheritdoc />
    public partial class Ies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdviceCustomPropertys_Advices_AdviceId",
                table: "AdviceCustomPropertys");

            migrationBuilder.DropForeignKey(
                name: "FK_AdviceCustomPropertys_CustomPropertys_CustomPropertyId",
                table: "AdviceCustomPropertys");

            migrationBuilder.DropForeignKey(
                name: "FK_CountryFilms_Countrys_CountryId",
                table: "CountryFilms");

            migrationBuilder.DropForeignKey(
                name: "FK_FilmCustomPropertys_CustomPropertys_CustomPropertyId",
                table: "FilmCustomPropertys");

            migrationBuilder.DropForeignKey(
                name: "FK_FilmCustomPropertys_Films_FilmId",
                table: "FilmCustomPropertys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FilmCustomPropertys",
                table: "FilmCustomPropertys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomPropertys",
                table: "CustomPropertys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countrys",
                table: "Countrys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdviceCustomPropertys",
                table: "AdviceCustomPropertys");

            migrationBuilder.RenameTable(
                name: "FilmCustomPropertys",
                newName: "FilmCustomProperties");

            migrationBuilder.RenameTable(
                name: "CustomPropertys",
                newName: "CustomProperties");

            migrationBuilder.RenameTable(
                name: "Countrys",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "AdviceCustomPropertys",
                newName: "AdviceCustomProperties");

            migrationBuilder.RenameIndex(
                name: "IX_FilmCustomPropertys_FilmId",
                table: "FilmCustomProperties",
                newName: "IX_FilmCustomProperties_FilmId");

            migrationBuilder.RenameIndex(
                name: "IX_FilmCustomPropertys_CustomPropertyId",
                table: "FilmCustomProperties",
                newName: "IX_FilmCustomProperties_CustomPropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_AdviceCustomPropertys_CustomPropertyId",
                table: "AdviceCustomProperties",
                newName: "IX_AdviceCustomProperties_CustomPropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_AdviceCustomPropertys_AdviceId",
                table: "AdviceCustomProperties",
                newName: "IX_AdviceCustomProperties_AdviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FilmCustomProperties",
                table: "FilmCustomProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomProperties",
                table: "CustomProperties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdviceCustomProperties",
                table: "AdviceCustomProperties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdviceCustomProperties_Advices_AdviceId",
                table: "AdviceCustomProperties",
                column: "AdviceId",
                principalTable: "Advices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdviceCustomProperties_CustomProperties_CustomPropertyId",
                table: "AdviceCustomProperties",
                column: "CustomPropertyId",
                principalTable: "CustomProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CountryFilms_Countries_CountryId",
                table: "CountryFilms",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilmCustomProperties_CustomProperties_CustomPropertyId",
                table: "FilmCustomProperties",
                column: "CustomPropertyId",
                principalTable: "CustomProperties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilmCustomProperties_Films_FilmId",
                table: "FilmCustomProperties",
                column: "FilmId",
                principalTable: "Films",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdviceCustomProperties_Advices_AdviceId",
                table: "AdviceCustomProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_AdviceCustomProperties_CustomProperties_CustomPropertyId",
                table: "AdviceCustomProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_CountryFilms_Countries_CountryId",
                table: "CountryFilms");

            migrationBuilder.DropForeignKey(
                name: "FK_FilmCustomProperties_CustomProperties_CustomPropertyId",
                table: "FilmCustomProperties");

            migrationBuilder.DropForeignKey(
                name: "FK_FilmCustomProperties_Films_FilmId",
                table: "FilmCustomProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FilmCustomProperties",
                table: "FilmCustomProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomProperties",
                table: "CustomProperties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AdviceCustomProperties",
                table: "AdviceCustomProperties");

            migrationBuilder.RenameTable(
                name: "FilmCustomProperties",
                newName: "FilmCustomPropertys");

            migrationBuilder.RenameTable(
                name: "CustomProperties",
                newName: "CustomPropertys");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Countrys");

            migrationBuilder.RenameTable(
                name: "AdviceCustomProperties",
                newName: "AdviceCustomPropertys");

            migrationBuilder.RenameIndex(
                name: "IX_FilmCustomProperties_FilmId",
                table: "FilmCustomPropertys",
                newName: "IX_FilmCustomPropertys_FilmId");

            migrationBuilder.RenameIndex(
                name: "IX_FilmCustomProperties_CustomPropertyId",
                table: "FilmCustomPropertys",
                newName: "IX_FilmCustomPropertys_CustomPropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_AdviceCustomProperties_CustomPropertyId",
                table: "AdviceCustomPropertys",
                newName: "IX_AdviceCustomPropertys_CustomPropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_AdviceCustomProperties_AdviceId",
                table: "AdviceCustomPropertys",
                newName: "IX_AdviceCustomPropertys_AdviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FilmCustomPropertys",
                table: "FilmCustomPropertys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomPropertys",
                table: "CustomPropertys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countrys",
                table: "Countrys",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdviceCustomPropertys",
                table: "AdviceCustomPropertys",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdviceCustomPropertys_Advices_AdviceId",
                table: "AdviceCustomPropertys",
                column: "AdviceId",
                principalTable: "Advices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdviceCustomPropertys_CustomPropertys_CustomPropertyId",
                table: "AdviceCustomPropertys",
                column: "CustomPropertyId",
                principalTable: "CustomPropertys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CountryFilms_Countrys_CountryId",
                table: "CountryFilms",
                column: "CountryId",
                principalTable: "Countrys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilmCustomPropertys_CustomPropertys_CustomPropertyId",
                table: "FilmCustomPropertys",
                column: "CustomPropertyId",
                principalTable: "CustomPropertys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilmCustomPropertys_Films_FilmId",
                table: "FilmCustomPropertys",
                column: "FilmId",
                principalTable: "Films",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
