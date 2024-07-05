using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TarefasAPI.Migrations
{
    /// <inheritdoc />
    public partial class TarefaSincronizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tarefas",
                table: "Tarefas");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tarefas",
                newName: "IdTarefaApp");

            migrationBuilder.AlterColumn<int>(
                name: "IdTarefaApp",
                table: "Tarefas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "IdTarefaAPI",
                table: "Tarefas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<bool>(
                name: "Excluido",
                table: "Tarefas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tarefas",
                table: "Tarefas",
                column: "IdTarefaAPI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tarefas",
                table: "Tarefas");

            migrationBuilder.DropColumn(
                name: "IdTarefaAPI",
                table: "Tarefas");

            migrationBuilder.DropColumn(
                name: "Excluido",
                table: "Tarefas");

            migrationBuilder.RenameColumn(
                name: "IdTarefaApp",
                table: "Tarefas",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Tarefas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tarefas",
                table: "Tarefas",
                column: "Id");
        }
    }
}
