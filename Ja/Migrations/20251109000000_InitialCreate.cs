using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ja.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Weight = table.Column<double>(type: "REAL", nullable: true),
                    Height = table.Column<double>(type: "REAL", nullable: true),
                    RestingHeartRate = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxHeartRate = table.Column<int>(type: "INTEGER", nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FTPHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    FtpValue = table.Column<double>(type: "REAL", nullable: false),
                    WeightAtTest = table.Column<double>(type: "REAL", nullable: true),
                    TestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FTPHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FTPHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeightHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeightHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MinPercent = table.Column<double>(type: "REAL", nullable: false),
                    MaxPercent = table.Column<double>(type: "REAL", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MinDurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerZones_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeartRateZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MinPercent = table.Column<double>(type: "REAL", nullable: false),
                    MaxPercent = table.Column<double>(type: "REAL", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeartRateZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeartRateZones_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SettingKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PMCData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CTL = table.Column<double>(type: "REAL", nullable: false),
                    ATL = table.Column<double>(type: "REAL", nullable: false),
                    TSB = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMCData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMCData_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TrainingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    DistanceMeters = table.Column<double>(type: "REAL", nullable: true),
                    AvgPower = table.Column<double>(type: "REAL", nullable: true),
                    NormalizedPower = table.Column<double>(type: "REAL", nullable: true),
                    MaxPower = table.Column<double>(type: "REAL", nullable: true),
                    AvgHeartRate = table.Column<double>(type: "REAL", nullable: true),
                    MaxHeartRate = table.Column<int>(type: "INTEGER", nullable: true),
                    AvgCadence = table.Column<double>(type: "REAL", nullable: true),
                    MaxCadence = table.Column<int>(type: "INTEGER", nullable: true),
                    AvgSpeed = table.Column<double>(type: "REAL", nullable: true),
                    ElevationGain = table.Column<double>(type: "REAL", nullable: true),
                    ElevationLoss = table.Column<double>(type: "REAL", nullable: true),
                    TSS = table.Column<double>(type: "REAL", nullable: true),
                    IntensityFactor = table.Column<double>(type: "REAL", nullable: true),
                    VariabilityIndex = table.Column<double>(type: "REAL", nullable: true),
                    WorkKJ = table.Column<double>(type: "REAL", nullable: true),
                    FtpUsed = table.Column<double>(type: "REAL", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trainings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrainingId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartSecond = table.Column<int>(type: "INTEGER", nullable: false),
                    EndSecond = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    AvgPower = table.Column<double>(type: "REAL", nullable: false),
                    AvgPowerWatts = table.Column<double>(type: "REAL", nullable: false),
                    Zone = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IntervalType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Slope = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingIntervals_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrainingId = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    PowerWatts = table.Column<double>(type: "REAL", nullable: false),
                    HeartRate = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRecords_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrainingId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false),
                    SecondaryValue = table.Column<double>(type: "REAL", nullable: true),
                    AchievedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalRecords_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "idx_trainings_user_date",
                table: "Trainings",
                columns: new[] { "UserId", "TrainingDate" });

            migrationBuilder.CreateIndex(
                name: "idx_trainings_date",
                table: "Trainings",
                column: "TrainingDate");

            migrationBuilder.CreateIndex(
                name: "idx_intervals_training",
                table: "TrainingIntervals",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "idx_records_training",
                table: "TrainingRecords",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "idx_records_duration",
                table: "TrainingRecords",
                column: "DurationSeconds");

            migrationBuilder.CreateIndex(
                name: "idx_pmc_user_date",
                table: "PMCData",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "idx_personal_records_type",
                table: "PersonalRecords",
                column: "RecordType");

            migrationBuilder.CreateIndex(
                name: "idx_settings_user_key",
                table: "Settings",
                columns: new[] { "UserId", "SettingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FTPHistory_UserId",
                table: "FTPHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightHistory_UserId",
                table: "WeightHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerZones_UserId",
                table: "PowerZones",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartRateZones_UserId",
                table: "HeartRateZones",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_UserId",
                table: "PersonalRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRecords_TrainingId",
                table: "PersonalRecords",
                column: "TrainingId");

            // Dodaj domyślnego użytkownika
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "Surname", "DateOfBirth", "Gender", "Weight", "Height", "RestingHeartRate", "MaxHeartRate", "ProfilePicturePath", "CreatedAt", "UpdatedAt" },
                values: new object[] { 1, "Jan", "Kowalski", null, "Male", 75.0, 180.0, 55, 185, null, DateTime.UtcNow, DateTime.UtcNow });

            // Dodaj domyślne strefy mocy dla użytkownika
            migrationBuilder.InsertData(
                table: "PowerZones",
                columns: new[] { "UserId", "ZoneNumber", "ZoneName", "MinPercent", "MaxPercent", "ColorHex", "MinDurationSeconds" },
                values: new object[,]
                {
                    { 1, 1, "Recovery", 0, 55, "#808080", 0 },
                    { 1, 2, "Endurance", 55, 75, "#4169E1", 0 },
                    { 1, 3, "Tempo", 75, 90, "#32CD32", 120 },
                    { 1, 4, "Threshold", 90, 105, "#FFD700", 60 },
                    { 1, 5, "VO2max", 105, 120, "#FF8C00", 30 },
                    { 1, 6, "Anaerobic", 120, 150, "#FF4500", 10 },
                    { 1, 7, "Neuromuscular", 150, 999, "#8B0000", 5 }
                });

            // Dodaj domyślne strefy tętna dla użytkownika
            migrationBuilder.InsertData(
                table: "HeartRateZones",
                columns: new[] { "UserId", "ZoneNumber", "ZoneName", "MinPercent", "MaxPercent", "ColorHex" },
                values: new object[,]
                {
                    { 1, 1, "Recovery", 0, 60, "#808080" },
                    { 1, 2, "Endurance", 60, 70, "#4169E1" },
                    { 1, 3, "Tempo", 70, 80, "#32CD32" },
                    { 1, 4, "Threshold", 80, 90, "#FFD700" },
                    { 1, 5, "VO2max", 90, 100, "#FF4500" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FTPHistory");
            migrationBuilder.DropTable(name: "WeightHistory");
            migrationBuilder.DropTable(name: "PowerZones");
            migrationBuilder.DropTable(name: "HeartRateZones");
            migrationBuilder.DropTable(name: "Settings");
            migrationBuilder.DropTable(name: "PMCData");
            migrationBuilder.DropTable(name: "TrainingIntervals");
            migrationBuilder.DropTable(name: "TrainingRecords");
            migrationBuilder.DropTable(name: "PersonalRecords");
            migrationBuilder.DropTable(name: "Trainings");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
