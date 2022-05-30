using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Migrations
{
    public partial class ShortCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorialGroupMemberEntity_Users_UserId",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropIndex(
                name: "IX_Users_TeamId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorialGroupMemberEntity",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropColumn(
                name: "DefaultTutorialGroupId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "HasMultipleTutorialGroups",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "QnAPairId",
                table: "Questions",
                newName: "InitialResponseMessageId");

            migrationBuilder.AlterColumn<string>(
                name: "TeamId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ShortCode",
                table: "TutorialGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "TutorialGroupMemberEntity",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimeStamp",
                table: "Questions",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeBaseId",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimeStamp",
                table: "AnswerEntity",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorialGroupMemberEntity",
                table: "TutorialGroupMemberEntity",
                columns: new[] { "CourseId", "TutorialGroupId", "UserId" });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeBases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeBases_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "AadId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId",
                unique: true,
                filter: "[TeamId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TutorialGroupMemberEntity_CourseId_UserId",
                table: "TutorialGroupMemberEntity",
                columns: new[] { "CourseId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TutorialGroupMemberEntity_TutorialGroupId",
                table: "TutorialGroupMemberEntity",
                column: "TutorialGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_KnowledgeBaseId",
                table: "Courses",
                column: "KnowledgeBaseId");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeBases_OwnerUserId",
                table: "KnowledgeBases",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_KnowledgeBases_KnowledgeBaseId",
                table: "Courses",
                column: "KnowledgeBaseId",
                principalTable: "KnowledgeBases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorialGroupMemberEntity_CourseMemberEntity_CourseId_UserId",
                table: "TutorialGroupMemberEntity",
                columns: new[] { "CourseId", "UserId" },
                principalTable: "CourseMemberEntity",
                principalColumns: new[] { "CourseId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TutorialGroupMemberEntity_Users_UserId",
                table: "TutorialGroupMemberEntity",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "AadId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_KnowledgeBases_KnowledgeBaseId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorialGroupMemberEntity_CourseMemberEntity_CourseId_UserId",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_TutorialGroupMemberEntity_Users_UserId",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "KnowledgeBases");

            migrationBuilder.DropIndex(
                name: "IX_Users_TeamId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TutorialGroupMemberEntity",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropIndex(
                name: "IX_TutorialGroupMemberEntity_CourseId_UserId",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropIndex(
                name: "IX_TutorialGroupMemberEntity_TutorialGroupId",
                table: "TutorialGroupMemberEntity");

            migrationBuilder.DropIndex(
                name: "IX_Courses_KnowledgeBaseId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ShortCode",
                table: "TutorialGroups");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "KnowledgeBaseId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "AnswerEntity");

            migrationBuilder.RenameColumn(
                name: "InitialResponseMessageId",
                table: "Questions",
                newName: "QnAPairId");

            migrationBuilder.AlterColumn<string>(
                name: "TeamId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "TutorialGroupMemberEntity",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DefaultTutorialGroupId",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasMultipleTutorialGroups",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TutorialGroupMemberEntity",
                table: "TutorialGroupMemberEntity",
                columns: new[] { "TutorialGroupId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TutorialGroupMemberEntity_Users_UserId",
                table: "TutorialGroupMemberEntity",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "AadId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
