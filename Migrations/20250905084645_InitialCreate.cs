using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BLBM_Env.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Source_DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_Tur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_ServiceTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source_Port = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target_DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_Tur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_ServiceTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target_Port = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConnectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkSpeed = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "EnvanterDetails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvanterID = table.Column<int>(type: "int", nullable: false),
                    Turu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceServiceTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lok = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkSpeed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NicID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiberMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BakirMAC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WWPN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwPort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwModel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwServiceTag = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvanterDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnvanterDetails_Envanterler_EnvanterID",
                        column: x => x.EnvanterID,
                        principalTable: "Envanterler",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Baglantilar_SourceDeviceID",
                table: "Baglantilar",
                column: "SourceDeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Baglantilar_TargetDeviceID",
                table: "Baglantilar",
                column: "TargetDeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_EnvanterDetails_EnvanterID",
                table: "EnvanterDetails",
                column: "EnvanterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Baglantilar");

            migrationBuilder.DropTable(
                name: "EnvanterDetails");

            migrationBuilder.DropTable(
                name: "Vips");

            migrationBuilder.DropTable(
                name: "Envanterler");
        }
    }
}
