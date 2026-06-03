using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnrollmentSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubjectDegreeProgramRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Departments_DepartmentID",
                table: "Subjects");

            migrationBuilder.RenameColumn(
                name: "DepartmentID",
                table: "Subjects",
                newName: "DegreeProgramID");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_DepartmentID",
                table: "Subjects",
                newName: "IX_Subjects_DegreeProgramID");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_DegreePrograms_DegreeProgramID",
                table: "Subjects",
                column: "DegreeProgramID",
                principalTable: "DegreePrograms",
                principalColumn: "DegreeProgramID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_DegreePrograms_DegreeProgramID",
                table: "Subjects");

            migrationBuilder.RenameColumn(
                name: "DegreeProgramID",
                table: "Subjects",
                newName: "DepartmentID");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_DegreeProgramID",
                table: "Subjects",
                newName: "IX_Subjects_DepartmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Departments_DepartmentID",
                table: "Subjects",
                column: "DepartmentID",
                principalTable: "Departments",
                principalColumn: "DepartmentID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
