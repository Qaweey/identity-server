using Microsoft.EntityFrameworkCore.Migrations;

namespace settl.identityserver.EntityFrameworkCore.Migrations
{
    public partial class addedIsVerifiedProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "tbl_IdentityServer_RegisterNewDevice",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "tbl_IdentityServer_RegisterNewDevice");
        }
    }
}
