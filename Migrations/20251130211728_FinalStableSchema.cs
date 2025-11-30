using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLBM_Env.Migrations
{
    /// <inheritdoc />
    public partial class FinalStableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Module = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    OldValues = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Envanterler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: false),
                    Tur = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    IloIdracIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    ServiceTagSerialNumber = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Model = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    VcenterAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    ClusterName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    OperatingSystem = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Kabin = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RearFront = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    KabinU = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envanterler", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Vips",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dns = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    VipIp = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Port = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    MakineIp = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    MakineAdi = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Durumu = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Network = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Cluster = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Host = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    OS = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vips", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ClaimValue = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ClaimType = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ClaimValue = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    UserId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RoleId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Baglantilar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceDeviceID = table.Column<int>(type: "int", nullable: true),
                    TargetDeviceID = table.Column<int>(type: "int", nullable: true),
                    Source_Port = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Source_LinkStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Source_LinkSpeed = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Source_NicID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Source_FiberMAC = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Source_BakirMAC = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Source_WWPN = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    Target_Port = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Target_LinkStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Target_LinkSpeed = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Target_FiberMAC = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Target_BakirMAC = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Target_WWPN = table.Column<string>(type: "varchar(60)", unicode: false, maxLength: 60, nullable: true),
                    ConnectionType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baglantilar", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Baglantilar_Envanterler_SourceDeviceID",
                        column: x => x.SourceDeviceID,
                        principalTable: "Envanterler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Baglantilar_Envanterler_TargetDeviceID",
                        column: x => x.TargetDeviceID,
                        principalTable: "Envanterler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Baglantilar_SourceDeviceID",
                table: "Baglantilar",
                column: "SourceDeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Baglantilar_TargetDeviceID",
                table: "Baglantilar",
                column: "TargetDeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Envanterler_DeviceName",
                table: "Envanterler",
                column: "DeviceName");

            migrationBuilder.CreateIndex(
                name: "IX_Envanterler_IpAddress",
                table: "Envanterler",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Envanterler_ServiceTagSerialNumber",
                table: "Envanterler",
                column: "ServiceTagSerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Vips_Dns",
                table: "Vips",
                column: "Dns");

            migrationBuilder.CreateIndex(
                name: "IX_Vips_VipIp",
                table: "Vips",
                column: "VipIp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Baglantilar");

            migrationBuilder.DropTable(
                name: "Vips");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Envanterler");
        }
    }
}
