IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Envanterler] (
    [ID] int NOT NULL IDENTITY,
    [DeviceName] nvarchar(max) NOT NULL,
    [Tur] nvarchar(max) NOT NULL,
    [ServiceTagSerialNumber] nvarchar(max) NULL,
    [Model] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [VcenterAddress] nvarchar(max) NULL,
    [ClusterName] nvarchar(max) NULL,
    [Location] nvarchar(max) NULL,
    [OperatingSystem] nvarchar(max) NULL,
    [IloIdracIp] nvarchar(max) NULL,
    [Kabin] nvarchar(max) NULL,
    [RearFront] nvarchar(max) NULL,
    [KabinU] nvarchar(max) NULL,
    CONSTRAINT [PK_Envanterler] PRIMARY KEY ([ID])
);

CREATE TABLE [Vips] (
    [ID] int NOT NULL IDENTITY,
    [Dns] nvarchar(max) NULL,
    [VipIp] nvarchar(max) NULL,
    [Port] nvarchar(max) NULL,
    [MakineIp] nvarchar(max) NULL,
    [MakineAdi] nvarchar(max) NULL,
    [Durumu] nvarchar(max) NULL,
    [Network] nvarchar(max) NULL,
    [Cluster] nvarchar(max) NULL,
    [Host] nvarchar(max) NULL,
    [OS] nvarchar(max) NULL,
    CONSTRAINT [PK_Vips] PRIMARY KEY ([ID])
);

CREATE TABLE [Baglantilar] (
    [ID] int NOT NULL IDENTITY,
    [SourceDeviceID] int NULL,
    [TargetDeviceID] int NULL,
    [Source_Port] nvarchar(max) NOT NULL,
    [Source_LinkStatus] nvarchar(max) NULL,
    [Source_LinkSpeed] nvarchar(max) NULL,
    [Source_NicID] nvarchar(max) NULL,
    [Source_FiberMAC] nvarchar(max) NULL,
    [Source_BakirMAC] nvarchar(max) NULL,
    [Source_WWPN] nvarchar(max) NULL,
    [Target_Port] nvarchar(max) NOT NULL,
    [Target_LinkStatus] nvarchar(max) NULL,
    [Target_LinkSpeed] nvarchar(max) NULL,
    [Target_FiberMAC] nvarchar(max) NULL,
    [Target_BakirMAC] nvarchar(max) NULL,
    [Target_WWPN] nvarchar(max) NULL,
    [ConnectionType] nvarchar(max) NULL,
    CONSTRAINT [PK_Baglantilar] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_Baglantilar_Envanterler_SourceDeviceID] FOREIGN KEY ([SourceDeviceID]) REFERENCES [Envanterler] ([ID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Baglantilar_Envanterler_TargetDeviceID] FOREIGN KEY ([TargetDeviceID]) REFERENCES [Envanterler] ([ID]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Baglantilar_SourceDeviceID] ON [Baglantilar] ([SourceDeviceID]);

CREATE INDEX [IX_Baglantilar_TargetDeviceID] ON [Baglantilar] ([TargetDeviceID]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250907151127_RicherConnectionModel', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250908060128_laptop8eyll', N'9.0.8');

COMMIT;
GO

