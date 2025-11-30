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
GO

CREATE TABLE [AspNetRoles] (
    [Id] varchar(50) NOT NULL,
    [Name] varchar(256) NULL,
    [NormalizedName] varchar(256) NULL,
    [ConcurrencyStamp] varchar(50) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] varchar(50) NOT NULL,
    [FirstName] varchar(50) NULL,
    [LastName] varchar(50) NULL,
    [UserName] varchar(256) NULL,
    [NormalizedUserName] varchar(256) NULL,
    [Email] varchar(256) NULL,
    [NormalizedEmail] varchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] varchar(256) NULL,
    [SecurityStamp] varchar(50) NULL,
    [ConcurrencyStamp] varchar(50) NULL,
    [PhoneNumber] varchar(20) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserEmail] varchar(50) NOT NULL,
    [UserName] varchar(50) NOT NULL,
    [Action] varchar(20) NOT NULL,
    [Module] varchar(20) NOT NULL,
    [Description] varchar(250) NOT NULL,
    [OldValues] varchar(max) NULL,
    [Timestamp] datetime2(0) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Envanterler] (
    [ID] int NOT NULL IDENTITY,
    [DeviceName] varchar(60) NOT NULL,
    [Tur] varchar(20) NOT NULL,
    [IpAddress] varchar(45) NULL,
    [IloIdracIp] varchar(45) NULL,
    [ServiceTagSerialNumber] varchar(30) NULL,
    [Model] varchar(60) NULL,
    [VcenterAddress] varchar(45) NULL,
    [ClusterName] varchar(50) NULL,
    [Location] varchar(30) NULL,
    [OperatingSystem] varchar(50) NULL,
    [Kabin] varchar(20) NULL,
    [RearFront] varchar(10) NULL,
    [KabinU] varchar(10) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Envanterler] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [Vips] (
    [ID] int NOT NULL IDENTITY,
    [Dns] varchar(100) NULL,
    [VipIp] varchar(50) NULL,
    [Port] varchar(20) NULL,
    [MakineIp] varchar(50) NULL,
    [MakineAdi] varchar(100) NULL,
    [Durumu] varchar(20) NULL,
    [Network] varchar(50) NULL,
    [Cluster] varchar(50) NULL,
    [Host] varchar(100) NULL,
    [OS] varchar(60) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Vips] PRIMARY KEY ([ID])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] varchar(50) NOT NULL,
    [ClaimType] varchar(max) NULL,
    [ClaimValue] varchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] varchar(50) NOT NULL,
    [ClaimType] varchar(max) NULL,
    [ClaimValue] varchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] varchar(128) NOT NULL,
    [ProviderKey] varchar(128) NOT NULL,
    [ProviderDisplayName] varchar(max) NULL,
    [UserId] varchar(50) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] varchar(50) NOT NULL,
    [RoleId] varchar(50) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] varchar(50) NOT NULL,
    [LoginProvider] varchar(128) NOT NULL,
    [Name] varchar(128) NOT NULL,
    [Value] varchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Baglantilar] (
    [ID] int NOT NULL IDENTITY,
    [SourceDeviceID] int NULL,
    [TargetDeviceID] int NULL,
    [Source_Port] varchar(50) NOT NULL,
    [Source_LinkStatus] varchar(20) NULL,
    [Source_LinkSpeed] varchar(20) NULL,
    [Source_NicID] varchar(50) NULL,
    [Source_FiberMAC] varchar(30) NULL,
    [Source_BakirMAC] varchar(30) NULL,
    [Source_WWPN] varchar(60) NULL,
    [Target_Port] varchar(50) NOT NULL,
    [Target_LinkStatus] varchar(20) NULL,
    [Target_LinkSpeed] varchar(20) NULL,
    [Target_FiberMAC] varchar(30) NULL,
    [Target_BakirMAC] varchar(30) NULL,
    [Target_WWPN] varchar(60) NULL,
    [ConnectionType] varchar(50) NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Baglantilar] PRIMARY KEY ([ID]),
    CONSTRAINT [FK_Baglantilar_Envanterler_SourceDeviceID] FOREIGN KEY ([SourceDeviceID]) REFERENCES [Envanterler] ([ID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Baglantilar_Envanterler_TargetDeviceID] FOREIGN KEY ([TargetDeviceID]) REFERENCES [Envanterler] ([ID]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Baglantilar_SourceDeviceID] ON [Baglantilar] ([SourceDeviceID]);
GO

CREATE INDEX [IX_Baglantilar_TargetDeviceID] ON [Baglantilar] ([TargetDeviceID]);
GO

CREATE INDEX [IX_Envanterler_DeviceName] ON [Envanterler] ([DeviceName]);
GO

CREATE INDEX [IX_Envanterler_IpAddress] ON [Envanterler] ([IpAddress]);
GO

CREATE INDEX [IX_Envanterler_ServiceTagSerialNumber] ON [Envanterler] ([ServiceTagSerialNumber]);
GO

CREATE INDEX [IX_Vips_Dns] ON [Vips] ([Dns]);
GO

CREATE INDEX [IX_Vips_VipIp] ON [Vips] ([VipIp]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251130211728_FinalStableSchema', N'8.0.22');
GO

COMMIT;
GO

