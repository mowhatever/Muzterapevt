﻿if not exists( select * from sys.tables where object_id = object_id('dbo.Files') ) 
	create table dbo.Files (
		Id int identity primary key,
		Domain nvarchar(500),
		FilePath nvarchar(500),
		CreatedOn datetime2 not null,
		OriginalFileName nvarchar(500),

		Data varbinary(max),
		ContentType nvarchar(50)
	)
GO

if not exists( select * from sys.tables where object_id = object_id('dbo.FileVersions') ) 
	create table dbo.FileVersions (
		Id int identity primary key,
		[File_Id] int references dbo.Files not null,
		[Key] nvarchar(500),
		CreatedOn datetime2 not null,
		Data varbinary(max),
		ContentType nvarchar(50)
	)
GO