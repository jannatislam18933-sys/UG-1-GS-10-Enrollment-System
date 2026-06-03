using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnrollmentSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncEnrollmentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnrollmentRequestID",
                table: "FeeSlips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "FeeSlips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "FeeSlips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeeSlipID",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Enrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByClerkID",
                table: "Enrollments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Enrollments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_FeeSlipID",
                table: "Enrollments",
                column: "FeeSlipID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ReviewedByClerkID",
                table: "Enrollments",
                column: "ReviewedByClerkID");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_FeeSlips_FeeSlipID",
                table: "Enrollments",
                column: "FeeSlipID",
                principalTable: "FeeSlips",
                principalColumn: "FeeSlipID");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Users_ReviewedByClerkID",
                table: "Enrollments",
                column: "ReviewedByClerkID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_FeeSlips_FeeSlipID",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Users_ReviewedByClerkID",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_FeeSlipID",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_ReviewedByClerkID",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "EnrollmentRequestID",
                table: "FeeSlips");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "FeeSlips");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "FeeSlips");

            migrationBuilder.DropColumn(
                name: "FeeSlipID",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "ReviewedByClerkID",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Enrollments");
        }
    }
}
