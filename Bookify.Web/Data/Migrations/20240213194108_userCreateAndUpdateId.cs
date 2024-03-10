using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Web.Migrations
{
    public partial class userCreateAndUpdateId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                schema: "Auth",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "Auth",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                schema: "Library",
                table: "Categories",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "Library",
                table: "Categories",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                schema: "Library",
                table: "Books",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "Library",
                table: "Books",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                schema: "Library",
                table: "BookCopies",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "Library",
                table: "BookCopies",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                schema: "Library",
                table: "Authors",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "Library",
                table: "Authors",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedById",
                schema: "Library",
                table: "Categories",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedById",
                schema: "Library",
                table: "Categories",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CreatedById",
                schema: "Library",
                table: "Books",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UpdatedById",
                schema: "Library",
                table: "Books",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_CreatedById",
                schema: "Library",
                table: "BookCopies",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_UpdatedById",
                schema: "Library",
                table: "BookCopies",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_CreatedById",
                schema: "Library",
                table: "Authors",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_UpdatedById",
                schema: "Library",
                table: "Authors",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_Users_CreatedById",
                schema: "Library",
                table: "Authors",
                column: "CreatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Authors_Users_UpdatedById",
                schema: "Library",
                table: "Authors",
                column: "UpdatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_Users_CreatedById",
                schema: "Library",
                table: "BookCopies",
                column: "CreatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCopies_Users_UpdatedById",
                schema: "Library",
                table: "BookCopies",
                column: "UpdatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_CreatedById",
                schema: "Library",
                table: "Books",
                column: "CreatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_UpdatedById",
                schema: "Library",
                table: "Books",
                column: "UpdatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_CreatedById",
                schema: "Library",
                table: "Categories",
                column: "CreatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UpdatedById",
                schema: "Library",
                table: "Categories",
                column: "UpdatedById",
                principalSchema: "Auth",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Authors_Users_CreatedById",
                schema: "Library",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_Authors_Users_UpdatedById",
                schema: "Library",
                table: "Authors");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_Users_CreatedById",
                schema: "Library",
                table: "BookCopies");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCopies_Users_UpdatedById",
                schema: "Library",
                table: "BookCopies");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_CreatedById",
                schema: "Library",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Books_Users_UpdatedById",
                schema: "Library",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_CreatedById",
                schema: "Library",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UpdatedById",
                schema: "Library",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedById",
                schema: "Library",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UpdatedById",
                schema: "Library",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Books_CreatedById",
                schema: "Library",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_UpdatedById",
                schema: "Library",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_BookCopies_CreatedById",
                schema: "Library",
                table: "BookCopies");

            migrationBuilder.DropIndex(
                name: "IX_BookCopies_UpdatedById",
                schema: "Library",
                table: "BookCopies");

            migrationBuilder.DropIndex(
                name: "IX_Authors_CreatedById",
                schema: "Library",
                table: "Authors");

            migrationBuilder.DropIndex(
                name: "IX_Authors_UpdatedById",
                schema: "Library",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                schema: "Auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "Auth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                schema: "Library",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "Library",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                schema: "Library",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "Library",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                schema: "Library",
                table: "BookCopies");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "Library",
                table: "BookCopies");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                schema: "Library",
                table: "Authors");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "Library",
                table: "Authors");
        }
    }
}
