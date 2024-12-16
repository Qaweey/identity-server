using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace settl.identityserver.EntityFrameworkCore.Migrations
{
    public partial class addedfieldtoregisteernewdevice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionPin",
                table: "tbl_IdentityServer_RegisterNewDevice",
                newName: "LoginCountry");

            migrationBuilder.RenameColumn(
                name: "OTP",
                table: "tbl_IdentityServer_RegisterNewDevice",
                newName: "LoginCity");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAndTimeOfLogin",
                table: "tbl_IdentityServer_RegisterNewDevice",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAndTimeOfLogin",
                table: "tbl_IdentityServer_RegisterNewDevice");

            migrationBuilder.RenameColumn(
                name: "LoginCountry",
                table: "tbl_IdentityServer_RegisterNewDevice",
                newName: "TransactionPin");

            migrationBuilder.RenameColumn(
                name: "LoginCity",
                table: "tbl_IdentityServer_RegisterNewDevice",
                newName: "OTP");
        }
    }
}
