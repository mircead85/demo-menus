
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 12/15/2016 21:57:18
-- Generated from EDMX file: C:\MIRCEA_C\TopTal\GitRepo\02 - Menus\TopTalMenus\MenusAPIlib\Model\MenusEntityModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [TopTalMenus];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_UserUserRole_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserUserRole] DROP CONSTRAINT [FK_UserUserRole_User];
GO
IF OBJECT_ID(N'[dbo].[FK_UserUserRole_UserRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserUserRole] DROP CONSTRAINT [FK_UserUserRole_UserRole];
GO
IF OBJECT_ID(N'[dbo].[FK_MenuEntryUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[MenuEntries] DROP CONSTRAINT [FK_MenuEntryUser];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[UserRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserRoles];
GO
IF OBJECT_ID(N'[dbo].[MenuEntries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MenuEntries];
GO
IF OBJECT_ID(N'[dbo].[LogEntries]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LogEntries];
GO
IF OBJECT_ID(N'[dbo].[UserUserRole]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserUserRole];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(30)  NOT NULL,
    [Password] nvarchar(3000)  NOT NULL,
    [DisplayName] nvarchar(100)  NOT NULL,
    [ExpectedNumCalories] int  NULL
);
GO

-- Creating table 'UserRoles'
CREATE TABLE [dbo].[UserRoles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CanCRUDOthersEntries] bit  NOT NULL,
    [CanCRUDUsers] bit  NOT NULL,
    [IsAdmin] bit  NOT NULL,
    [RoleName] nvarchar(100)  NOT NULL
);
GO

-- Creating table 'MenuEntries'
CREATE TABLE [dbo].[MenuEntries] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(100)  NOT NULL,
    [Moment] datetime  NOT NULL,
    [NumCalories] int  NOT NULL,
    [User_Id] int  NOT NULL
);
GO

-- Creating table 'LogEntries'
CREATE TABLE [dbo].[LogEntries] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Message] nvarchar(max)  NOT NULL,
    [Details] nvarchar(max)  NULL,
    [CredentialsSummary] nvarchar(max)  NULL,
    [Moment] datetime  NOT NULL
);
GO

-- Creating table 'UserUserRole'
CREATE TABLE [dbo].[UserUserRole] (
    [Users_Id] int  NOT NULL,
    [UserRoles_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserRoles'
ALTER TABLE [dbo].[UserRoles]
ADD CONSTRAINT [PK_UserRoles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MenuEntries'
ALTER TABLE [dbo].[MenuEntries]
ADD CONSTRAINT [PK_MenuEntries]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'LogEntries'
ALTER TABLE [dbo].[LogEntries]
ADD CONSTRAINT [PK_LogEntries]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Users_Id], [UserRoles_Id] in table 'UserUserRole'
ALTER TABLE [dbo].[UserUserRole]
ADD CONSTRAINT [PK_UserUserRole]
    PRIMARY KEY CLUSTERED ([Users_Id], [UserRoles_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Users_Id] in table 'UserUserRole'
ALTER TABLE [dbo].[UserUserRole]
ADD CONSTRAINT [FK_UserUserRole_User]
    FOREIGN KEY ([Users_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [UserRoles_Id] in table 'UserUserRole'
ALTER TABLE [dbo].[UserUserRole]
ADD CONSTRAINT [FK_UserUserRole_UserRole]
    FOREIGN KEY ([UserRoles_Id])
    REFERENCES [dbo].[UserRoles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserUserRole_UserRole'
CREATE INDEX [IX_FK_UserUserRole_UserRole]
ON [dbo].[UserUserRole]
    ([UserRoles_Id]);
GO

-- Creating foreign key on [User_Id] in table 'MenuEntries'
ALTER TABLE [dbo].[MenuEntries]
ADD CONSTRAINT [FK_MenuEntryUser]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_MenuEntryUser'
CREATE INDEX [IX_FK_MenuEntryUser]
ON [dbo].[MenuEntries]
    ([User_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------