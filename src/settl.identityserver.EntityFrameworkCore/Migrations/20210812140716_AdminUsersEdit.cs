using Microsoft.EntityFrameworkCore.Migrations;

namespace settl.identityserver.EntityFrameworkCore.Migrations
{
    public partial class AdminUsersEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "tbl_IdenitityServer_AdminUsers",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "00000000",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "tbl_IdenitityServer_AdminUsers",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "tbl_IdenitityServer_AdminUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "tbl_IdenitityServer_AdminUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);
        }
    }
}