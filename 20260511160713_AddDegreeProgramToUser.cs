using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnrollmentSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDegreeProgramToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DegreeProgramID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DegreeProgramID",
                table: "Users",
                column: "DegreeProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_DegreePrograms_DegreeProgramID",
                table: "Users",
                column: "DegreeProgramID",
                principalTable: "DegreePrograms",
                principalColumn: "DegreeProgramID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_DegreePrograms_DegreeProgramID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DegreeProgramID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DegreeProgramID",
                table: "Users");
        }
    }
}
