using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class TaskUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserCreateId",
                table: "Tareas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_UserCreateId",
                table: "Tareas",
                column: "UserCreateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tareas_AspNetUsers_UserCreateId",
                table: "Tareas",
                column: "UserCreateId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tareas_AspNetUsers_UserCreateId",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Tareas_UserCreateId",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "UserCreateId",
                table: "Tareas");
        }
    }
}
