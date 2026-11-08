using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _10xWorkoutManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "muscle_groups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muscle_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_plans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "exercises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MuscleGroupId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercises_muscle_groups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "muscle_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "training_days",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<short>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_days", x => x.Id);
                    table.CheckConstraint("CK_training_days_order", "[Order] >= 0");
                    table.ForeignKey(
                        name: "FK_training_days_workout_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "workout_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_day_exercises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainingDayId = table.Column<long>(type: "bigint", nullable: false),
                    ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                    Order = table.Column<short>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_day_exercises", x => x.Id);
                    table.CheckConstraint("CK_plan_day_exercises_order", "[Order] >= 0");
                    table.ForeignKey(
                        name: "FK_plan_day_exercises_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plan_day_exercises_training_days_TrainingDayId",
                        column: x => x.TrainingDayId,
                        principalTable: "training_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<long>(type: "bigint", nullable: true),
                    TrainingDayId = table.Column<long>(type: "bigint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_training_days_TrainingDayId",
                        column: x => x.TrainingDayId,
                        principalTable: "training_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sessions_workout_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "workout_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "session_exercises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<long>(type: "bigint", nullable: false),
                    ExerciseId = table.Column<long>(type: "bigint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Skipped = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_exercises", x => x.Id);
                    table.CheckConstraint("CK_session_exercises_order", "[Order] >= 0");
                    table.ForeignKey(
                        name: "FK_session_exercises_exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_session_exercises_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_sets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionExerciseId = table.Column<long>(type: "bigint", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Reps = table.Column<short>(type: "smallint", nullable: false),
                    IsFailure = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<short>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_sets", x => x.Id);
                    table.CheckConstraint("CK_exercise_sets_order", "[Order] >= 0");
                    table.CheckConstraint("CK_exercise_sets_reps", "[Reps] >= 0");
                    table.CheckConstraint("CK_exercise_sets_weight", "[Weight] >= 0");
                    table.ForeignKey(
                        name: "FK_exercise_sets_session_exercises_SessionExerciseId",
                        column: x => x.SessionExerciseId,
                        principalTable: "session_exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_sets_SessionExerciseId",
                table: "exercise_sets",
                column: "SessionExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_MuscleGroupId",
                table: "exercises",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_UserId_Name",
                table: "exercises",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_muscle_groups_Name",
                table: "muscle_groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_day_exercises_ExerciseId",
                table: "plan_day_exercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_plan_day_exercises_TrainingDayId_ExerciseId",
                table: "plan_day_exercises",
                columns: new[] { "TrainingDayId", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_exercises_ExerciseId",
                table: "session_exercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_session_exercises_SessionId",
                table: "session_exercises",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_PlanId",
                table: "sessions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_TrainingDayId",
                table: "sessions",
                column: "TrainingDayId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                table: "sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_training_days_PlanId",
                table: "training_days",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_plans_UserId",
                table: "workout_plans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_sets");

            migrationBuilder.DropTable(
                name: "plan_day_exercises");

            migrationBuilder.DropTable(
                name: "session_exercises");

            migrationBuilder.DropTable(
                name: "exercises");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "muscle_groups");

            migrationBuilder.DropTable(
                name: "training_days");

            migrationBuilder.DropTable(
                name: "workout_plans");
        }
    }
}
