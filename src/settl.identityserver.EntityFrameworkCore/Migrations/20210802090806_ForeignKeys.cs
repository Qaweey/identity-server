using Microsoft.EntityFrameworkCore.Migrations;

namespace settl.identityserver.EntityFrameworkCore.Migrations
{
    public partial class ForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tbl_IdentityServer_TempUsers_UserTypeId",
                table: "tbl_IdentityServer_TempUsers",
                column: "UserTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_IdentityServer_TempUsers_TblUserTypes_UserTypeId",
                table: "tbl_IdentityServer_TempUsers",
                column: "UserTypeId",
                principalTable: "TblUserTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_IdentityServer_TempUsers_TblUserTypes_UserTypeId",
                table: "tbl_IdentityServer_TempUsers");

            migrationBuilder.DropIndex(
                name: "IX_tbl_IdentityServer_TempUsers_UserTypeId",
                table: "tbl_IdentityServer_TempUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "tbl_IdentityServer_TempUsers",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ConfirmPassword",
                table: "tbl_IdentityServer_TempUsers",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}