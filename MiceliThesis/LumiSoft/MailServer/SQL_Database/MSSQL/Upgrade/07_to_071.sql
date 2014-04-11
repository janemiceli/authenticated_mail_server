

DROP TABLE lsMailStore
CREATE TABLE [dbo].[lsMailStore] (
	[MessageID] [uniqueidentifier] NOT NULL ,
	[Mailbox] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Folder] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
	[Data] [image] NULL ,
	[Size] [bigint] NOT NULL ,
	[TopLines] [image] NULL ,
	[MessageFlags] [bigint] NOT NULL ,
	[Date] [datetime] NOT NULL ,
	[UID] [int] IDENTITY (1, 1) NOT NULL 
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
GO

CREATE TABLE [dbo].[lsIMAPFolders] (
	[UserID] [uniqueidentifier] NOT NULL ,
	[FolderName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[lsIMAPSubscribedFolders] (
	[UserID] [uniqueidentifier] NOT NULL ,
	[FolderName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL 
) ON [PRIMARY]
GO