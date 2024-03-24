using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TareasMVC.Migrations
{
    /// <inheritdoc />
    public partial class AdminRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF NOT EXISTS(Select Id from AspNetRoles where Id = '117654df-07f6-4d45-84cb-76252d7091e9')
BEGIN
	INSERT AspNetRoles (Id, [Name], [NormalizedName])
	VALUES ('117654df-07f6-4d45-84cb-76252d7091e9', 'admin', 'ADMIN')
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE AspNetRoles Where Id = '117654df-07f6-4d45-84cb-76252d7091e9'");
        }
    }
}
