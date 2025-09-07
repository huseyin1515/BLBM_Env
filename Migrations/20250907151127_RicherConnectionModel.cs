using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLBM_Env.Migrations
{
    /// <inheritdoc />
    public partial class RicherConnectionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Envanterler",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceTagSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VcenterAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClusterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IloIdracIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kabin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RearFront = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KabinU = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Dns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VipIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakineIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MakineAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Durumu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cluster = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OS = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vips", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Baglantilar",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceDeviceID = table.Column<int>(type: "int", nullable: true),
                    TargetDeviceID = table.Column<int>(type: "int", nullable: true),
                    Source_Port = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source_LinkStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_LinkSpeed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_NicID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_FiberMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_BakirMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_WWPN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_Port = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target_LinkStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_LinkSpeed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_FiberMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_BakirMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_WWPN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectionType = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "IX_Baglantilar_SourceDeviceID",
                table: "Baglantilar",
                column: "SourceDeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Baglantilar_TargetDeviceID",
                table: "Baglantilar",
                column: "TargetDeviceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Baglantilar");

            migrationBuilder.DropTable(
                name: "Vips");

            migrationBuilder.DropTable(
                name: "Envanterler");
        }
    }
}
