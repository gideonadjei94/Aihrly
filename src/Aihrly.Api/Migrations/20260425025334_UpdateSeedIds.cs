using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Aihrly.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TeamMembers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "TeamMembers",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "TeamMembers",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.InsertData(
                table: "TeamMembers",
                columns: new[] { "Id", "Email", "Name", "Role" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "alice@aihrly.com", "Alice Mensah", "Recruiter" },
                    { new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "kwame@aihrly.com", "Kwame Boateng", "HiringManager" },
                    { new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"), "sara@aihrly.com", "Sara Osei", "Recruiter" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TeamMembers",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"));

            migrationBuilder.DeleteData(
                table: "TeamMembers",
                keyColumn: "Id",
                keyValue: new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"));

            migrationBuilder.DeleteData(
                table: "TeamMembers",
                keyColumn: "Id",
                keyValue: new Guid("c3d4e5f6-a7b8-9012-cdef-123456789012"));

            migrationBuilder.InsertData(
                table: "TeamMembers",
                columns: new[] { "Id", "Email", "Name", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "alice@aihrly.com", "Alice Mensah", "Recruiter" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "kwame@aihrly.com", "Kwame Boateng", "HiringManager" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "sara@aihrly.com", "Sara Osei", "Recruiter" }
                });
        }
    }
}
