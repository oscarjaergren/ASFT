
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/06/2015 12:47:22
-- Generated from EDMX file: C:\Users\Fredrik\Documents\Git\asft.issuemanager\Backend\IssueManagerAPI\Database\IssueManager.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [IssueManager];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_IssueIssueImage]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[IssueImages] DROP CONSTRAINT [FK_IssueIssueImage];
GO
IF OBJECT_ID(N'[dbo].[FK_LocationIssue]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Issues] DROP CONSTRAINT [FK_LocationIssue];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Locations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Locations];
GO
IF OBJECT_ID(N'[dbo].[Issues]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Issues];
GO
IF OBJECT_ID(N'[dbo].[IssueImages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[IssueImages];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Locations'
CREATE TABLE [dbo].[Locations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [FullName] nvarchar(max)  NOT NULL,
    [Longitude] float  NOT NULL,
    [Latitude] float  NOT NULL,
    [TimeZone] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Issues'
CREATE TABLE [dbo].[Issues] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [Longitude] float  NOT NULL,
    [Latitude] float  NOT NULL,
    [Status] smallint  NOT NULL,
    [Severity] smallint  NOT NULL,
    [LocationId] int  NOT NULL,
    [Created] datetime  NOT NULL,
    [Edited] datetime  NOT NULL
);
GO

-- Creating table 'IssueImages'
CREATE TABLE [dbo].[IssueImages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FileName] nvarchar(max)  NOT NULL,
    [IssueId] int  NOT NULL,
    [Created] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Locations'
ALTER TABLE [dbo].[Locations]
ADD CONSTRAINT [PK_Locations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Issues'
ALTER TABLE [dbo].[Issues]
ADD CONSTRAINT [PK_Issues]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'IssueImages'
ALTER TABLE [dbo].[IssueImages]
ADD CONSTRAINT [PK_IssueImages]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [IssueId] in table 'IssueImages'
ALTER TABLE [dbo].[IssueImages]
ADD CONSTRAINT [FK_IssueIssueImage]
    FOREIGN KEY ([IssueId])
    REFERENCES [dbo].[Issues]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IssueIssueImage'
CREATE INDEX [IX_FK_IssueIssueImage]
ON [dbo].[IssueImages]
    ([IssueId]);
GO

-- Creating foreign key on [LocationId] in table 'Issues'
ALTER TABLE [dbo].[Issues]
ADD CONSTRAINT [FK_LocationIssue]
    FOREIGN KEY ([LocationId])
    REFERENCES [dbo].[Locations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LocationIssue'
CREATE INDEX [IX_FK_LocationIssue]
ON [dbo].[Issues]
    ([LocationId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------