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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [Courses] (
        [Id] nvarchar(450) NOT NULL,
        [TeamId] nvarchar(450) NOT NULL,
        [TeamAadObjectId] nvarchar(450) NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [HasMultipleTutorialGroups] bit NOT NULL,
        [DefaultTutorialGroupId] nvarchar(max) NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [Users] (
        [AadId] nvarchar(450) NOT NULL,
        [TeamId] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Upn] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([AadId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [Channels] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [CourseId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Channels] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Channels_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [TutorialGroups] (
        [Id] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(100) NOT NULL,
        [CourseId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_TutorialGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TutorialGroups_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [CourseMemberEntity] (
        [CourseId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [MemberRole] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_CourseMemberEntity] PRIMARY KEY ([CourseId], [UserId]),
        CONSTRAINT [FK_CourseMemberEntity_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CourseMemberEntity_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([AadId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [Questions] (
        [Id] nvarchar(450) NOT NULL,
        [CourseId] nvarchar(max) NULL,
        [ChannelId] nvarchar(450) NOT NULL,
        [MessageId] nvarchar(450) NOT NULL,
        [AuthorId] nvarchar(max) NOT NULL,
        [AnswerId] nvarchar(max) NULL,
        [QnAPairId] nvarchar(max) NULL,
        CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Questions_Channels_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [Channels] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [TutorialGroupMemberEntity] (
        [TutorialGroupId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CourseId] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TutorialGroupMemberEntity] PRIMARY KEY ([TutorialGroupId], [UserId]),
        CONSTRAINT [FK_TutorialGroupMemberEntity_TutorialGroups_TutorialGroupId] FOREIGN KEY ([TutorialGroupId]) REFERENCES [TutorialGroups] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TutorialGroupMemberEntity_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([AadId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE TABLE [AnswerEntity] (
        [Id] nvarchar(450) NOT NULL,
        [QuestionId] nvarchar(450) NOT NULL,
        [CourseId] nvarchar(max) NOT NULL,
        [ChannelId] nvarchar(max) NOT NULL,
        [MessageId] nvarchar(max) NOT NULL,
        [AuthorId] nvarchar(max) NOT NULL,
        [AcceptedById] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AnswerEntity] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnswerEntity_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX [IX_AnswerEntity_QuestionId] ON [AnswerEntity] ([QuestionId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE INDEX [IX_Channels_CourseId] ON [Channels] ([CourseId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE INDEX [IX_CourseMemberEntity_UserId] ON [CourseMemberEntity] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX [IX_Courses_TeamAadObjectId] ON [Courses] ([TeamAadObjectId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX [IX_Courses_TeamId] ON [Courses] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE INDEX [IX_Questions_ChannelId] ON [Questions] ([ChannelId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX [IX_Questions_MessageId] ON [Questions] ([MessageId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE INDEX [IX_TutorialGroupMemberEntity_UserId] ON [TutorialGroupMemberEntity] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE INDEX [IX_TutorialGroups_CourseId] ON [TutorialGroups] ([CourseId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    CREATE UNIQUE INDEX [IX_Users_TeamId] ON [Users] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210520221239_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210520221239_InitialCreate', N'5.0.3');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [TutorialGroupMemberEntity] DROP CONSTRAINT [FK_TutorialGroupMemberEntity_Users_UserId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    DROP INDEX [IX_Users_TeamId] ON [Users];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [TutorialGroupMemberEntity] DROP CONSTRAINT [PK_TutorialGroupMemberEntity];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courses]') AND [c].[name] = N'DefaultTutorialGroupId');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Courses] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Courses] DROP COLUMN [DefaultTutorialGroupId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Courses]') AND [c].[name] = N'HasMultipleTutorialGroups');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Courses] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Courses] DROP COLUMN [HasMultipleTutorialGroups];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    EXEC sp_rename N'[Questions].[QnAPairId]', N'InitialResponseMessageId', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'TeamId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Users] ALTER COLUMN [TeamId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [TutorialGroups] ADD [ShortCode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TutorialGroupMemberEntity]') AND [c].[name] = N'CourseId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [TutorialGroupMemberEntity] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [TutorialGroupMemberEntity] ALTER COLUMN [CourseId] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [Questions] ADD [TimeStamp] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [Courses] ADD [KnowledgeBaseId] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [AnswerEntity] ADD [TimeStamp] datetimeoffset NOT NULL DEFAULT '0001-01-01T00:00:00.0000000+00:00';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [TutorialGroupMemberEntity] ADD CONSTRAINT [PK_TutorialGroupMemberEntity] PRIMARY KEY ([CourseId], [TutorialGroupId], [UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    CREATE TABLE [AppSettings] (
        [Key] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AppSettings] PRIMARY KEY ([Key])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    CREATE TABLE [KnowledgeBases] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [OwnerUserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_KnowledgeBases] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KnowledgeBases_Users_OwnerUserId] FOREIGN KEY ([OwnerUserId]) REFERENCES [Users] ([AadId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_TeamId] ON [Users] ([TeamId]) WHERE [TeamId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    CREATE INDEX [IX_TutorialGroupMemberEntity_CourseId_UserId] ON [TutorialGroupMemberEntity] ([CourseId], [UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    CREATE INDEX [IX_TutorialGroupMemberEntity_TutorialGroupId] ON [TutorialGroupMemberEntity] ([TutorialGroupId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    CREATE INDEX [IX_Courses_KnowledgeBaseId] ON [Courses] ([KnowledgeBaseId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    CREATE INDEX [IX_KnowledgeBases_OwnerUserId] ON [KnowledgeBases] ([OwnerUserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [Courses] ADD CONSTRAINT [FK_Courses_KnowledgeBases_KnowledgeBaseId] FOREIGN KEY ([KnowledgeBaseId]) REFERENCES [KnowledgeBases] ([Id]) ON DELETE SET NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [TutorialGroupMemberEntity] ADD CONSTRAINT [FK_TutorialGroupMemberEntity_CourseMemberEntity_CourseId_UserId] FOREIGN KEY ([CourseId], [UserId]) REFERENCES [CourseMemberEntity] ([CourseId], [UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    ALTER TABLE [TutorialGroupMemberEntity] ADD CONSTRAINT [FK_TutorialGroupMemberEntity_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([AadId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20210924213224_ShortCode')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20210924213224_ShortCode', N'5.0.3');
END;
GO

COMMIT;
GO

