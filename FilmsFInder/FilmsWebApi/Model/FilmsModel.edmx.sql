
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/09/2019 09:30:32
-- Generated from EDMX file: D:\PROJECTS\CSharp\FilmsFInder\FilmsWebApi\Model\FilmsModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [FilmFinderBase];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CountryCountryFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CountryFilmSet] DROP CONSTRAINT [FK_CountryCountryFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmCountryFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CountryFilmSet] DROP CONSTRAINT [FK_FilmCountryFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmActorFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActorFilmSet] DROP CONSTRAINT [FK_FilmActorFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_ActorActorFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActorFilmSet] DROP CONSTRAINT [FK_ActorActorFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_ProducerFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FilmSet] DROP CONSTRAINT [FK_ProducerFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_GenreGenreFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GenreFilmSet] DROP CONSTRAINT [FK_GenreGenreFilm];
GO
IF OBJECT_ID(N'[dbo].[FK_FilmGenreFilm]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GenreFilmSet] DROP CONSTRAINT [FK_FilmGenreFilm];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FilmSet];
GO
IF OBJECT_ID(N'[dbo].[CountrySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CountrySet];
GO
IF OBJECT_ID(N'[dbo].[CountryFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CountryFilmSet];
GO
IF OBJECT_ID(N'[dbo].[ActorFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ActorFilmSet];
GO
IF OBJECT_ID(N'[dbo].[ActorSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ActorSet];
GO
IF OBJECT_ID(N'[dbo].[ProducerSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProducerSet];
GO
IF OBJECT_ID(N'[dbo].[GenreFilmSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GenreFilmSet];
GO
IF OBJECT_ID(N'[dbo].[GenreSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GenreSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FilmSet'
CREATE TABLE [dbo].[FilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Year] int  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [Rating] decimal(11,9)  NULL,
    [Poster] nvarchar(max)  NULL,
    [Slogan] nvarchar(max)  NULL,
    [Link] nvarchar(max)  NULL,
    [KinopoiskId] nvarchar(max)  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Producer_Id] int  NOT NULL
);
GO

-- Creating table 'CountrySet'
CREATE TABLE [dbo].[CountrySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'CountryFilmSet'
CREATE TABLE [dbo].[CountryFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Country_Id] int  NOT NULL,
    [Film_Id] int  NOT NULL
);
GO

-- Creating table 'ActorFilmSet'
CREATE TABLE [dbo].[ActorFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Film_Id] int  NOT NULL,
    [Actor_Id] int  NOT NULL
);
GO

-- Creating table 'ActorSet'
CREATE TABLE [dbo].[ActorSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'ProducerSet'
CREATE TABLE [dbo].[ProducerSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GenreFilmSet'
CREATE TABLE [dbo].[GenreFilmSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Genre_Id] int  NOT NULL,
    [Film_Id] int  NOT NULL
);
GO

-- Creating table 'GenreSet'
CREATE TABLE [dbo].[GenreSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'FilmSet'
ALTER TABLE [dbo].[FilmSet]
ADD CONSTRAINT [PK_FilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CountrySet'
ALTER TABLE [dbo].[CountrySet]
ADD CONSTRAINT [PK_CountrySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CountryFilmSet'
ALTER TABLE [dbo].[CountryFilmSet]
ADD CONSTRAINT [PK_CountryFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ActorFilmSet'
ALTER TABLE [dbo].[ActorFilmSet]
ADD CONSTRAINT [PK_ActorFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ActorSet'
ALTER TABLE [dbo].[ActorSet]
ADD CONSTRAINT [PK_ActorSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProducerSet'
ALTER TABLE [dbo].[ProducerSet]
ADD CONSTRAINT [PK_ProducerSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GenreFilmSet'
ALTER TABLE [dbo].[GenreFilmSet]
ADD CONSTRAINT [PK_GenreFilmSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GenreSet'
ALTER TABLE [dbo].[GenreSet]
ADD CONSTRAINT [PK_GenreSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Country_Id] in table 'CountryFilmSet'
ALTER TABLE [dbo].[CountryFilmSet]
ADD CONSTRAINT [FK_CountryCountryFilm]
    FOREIGN KEY ([Country_Id])
    REFERENCES [dbo].[CountrySet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CountryCountryFilm'
CREATE INDEX [IX_FK_CountryCountryFilm]
ON [dbo].[CountryFilmSet]
    ([Country_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'CountryFilmSet'
ALTER TABLE [dbo].[CountryFilmSet]
ADD CONSTRAINT [FK_FilmCountryFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmCountryFilm'
CREATE INDEX [IX_FK_FilmCountryFilm]
ON [dbo].[CountryFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'ActorFilmSet'
ALTER TABLE [dbo].[ActorFilmSet]
ADD CONSTRAINT [FK_FilmActorFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmActorFilm'
CREATE INDEX [IX_FK_FilmActorFilm]
ON [dbo].[ActorFilmSet]
    ([Film_Id]);
GO

-- Creating foreign key on [Actor_Id] in table 'ActorFilmSet'
ALTER TABLE [dbo].[ActorFilmSet]
ADD CONSTRAINT [FK_ActorActorFilm]
    FOREIGN KEY ([Actor_Id])
    REFERENCES [dbo].[ActorSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ActorActorFilm'
CREATE INDEX [IX_FK_ActorActorFilm]
ON [dbo].[ActorFilmSet]
    ([Actor_Id]);
GO

-- Creating foreign key on [Producer_Id] in table 'FilmSet'
ALTER TABLE [dbo].[FilmSet]
ADD CONSTRAINT [FK_ProducerFilm]
    FOREIGN KEY ([Producer_Id])
    REFERENCES [dbo].[ProducerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ProducerFilm'
CREATE INDEX [IX_FK_ProducerFilm]
ON [dbo].[FilmSet]
    ([Producer_Id]);
GO

-- Creating foreign key on [Genre_Id] in table 'GenreFilmSet'
ALTER TABLE [dbo].[GenreFilmSet]
ADD CONSTRAINT [FK_GenreGenreFilm]
    FOREIGN KEY ([Genre_Id])
    REFERENCES [dbo].[GenreSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_GenreGenreFilm'
CREATE INDEX [IX_FK_GenreGenreFilm]
ON [dbo].[GenreFilmSet]
    ([Genre_Id]);
GO

-- Creating foreign key on [Film_Id] in table 'GenreFilmSet'
ALTER TABLE [dbo].[GenreFilmSet]
ADD CONSTRAINT [FK_FilmGenreFilm]
    FOREIGN KEY ([Film_Id])
    REFERENCES [dbo].[FilmSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FilmGenreFilm'
CREATE INDEX [IX_FK_FilmGenreFilm]
ON [dbo].[GenreFilmSet]
    ([Film_Id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------