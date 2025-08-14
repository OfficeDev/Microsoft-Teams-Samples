using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.Teams.Apps.QBot.Infrastructure.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamAadObjectId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HasMultipleTutorialGroups = table.Column<bool>(type: "bit", nullable: false),
                    DefaultTutorialGroupId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    AadId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeamId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Upn = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.AadId);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channels_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorialGroups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorialGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorialGroups_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseMemberEntity",
                columns: table => new
                {
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MemberRole = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseMemberEntity", x => new { x.CourseId, x.UserId });
                    table.ForeignKey(
                        name: "FK_CourseMemberEntity_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseMemberEntity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "AadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChannelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QnAPairId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorialGroupMemberEntity",
                columns: table => new
                {
                    TutorialGroupId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorialGroupMemberEntity", x => new { x.TutorialGroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TutorialGroupMemberEntity_TutorialGroups_TutorialGroupId",
                        column: x => x.TutorialGroupId,
                        principalTable: "TutorialGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TutorialGroupMemberEntity_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "AadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuestionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChannelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcceptedById = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerEntity_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerEntity_QuestionId",
                table: "AnswerEntity",
                column: "QuestionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_CourseId",
                table: "Channels",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseMemberEntity_UserId",
                table: "CourseMemberEntity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeamAadObjectId",
                table: "Courses",
                column: "TeamAadObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeamId",
                table: "Courses",
                column: "TeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ChannelId",
                table: "Questions",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_MessageId",
                table: "Questions",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TutorialGroupMemberEntity_UserId",
                table: "TutorialGroupMemberEntity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorialGroups_CourseId",
                table: "TutorialGroups",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerEntity");

            migrationBuilder.DropTable(
                name: "CourseMemberEntity");

            migrationBuilder.DropTable(
                name: "TutorialGroupMemberEntity");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TutorialGroups");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Courses");
        }
    }
}
