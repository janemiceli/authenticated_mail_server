if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsAliases]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsAliases]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsDomains]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsDomains]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsIMAPFolders]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsIMAPFolders]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsIMAPSubscribedFolders]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsIMAPSubscribedFolders]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsMailStore]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsMailStore]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsRouting]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsRouting]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsSecurityList]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsSecurityList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lsUsers]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
drop table [dbo].[lsUsers]
GO

CREATE TABLE [dbo].[lsAliases] (
	[AliasID] [uniqueidentifier] NULL ,
	[DomainID] [uniqueidentifier] NULL ,
	[AliasName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Description] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[AliasMembers] [nvarchar] (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[IsPublic] [bit] NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[lsDomains] (
	[DomainID] [uniqueidentifier] NULL ,
	[DomainName] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Description] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) ON [PRIMARY]
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

CREATE TABLE [dbo].[lsRouting] (
	[DomainID] [uniqueidentifier] NULL ,
	[RouteID] [uniqueidentifier] NOT NULL ,
	[Pattern] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Mailbox] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Description] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[DomainName] [nvarchar] (500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[lsSecurityList] (
	[SecurityID] [uniqueidentifier] NULL ,
	[Description] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Protocol] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Type] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Action] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Content] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[StartIP] [bigint] NOT NULL ,
	[EndIP] [bigint] NOT NULL ,
	[Enabled] [bit] NOT NULL 
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[lsUsers] (
	[UserID] [uniqueidentifier] NULL ,
	[DomainID] [uniqueidentifier] NULL ,
	[FullName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[UserName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Password] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Description] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[Emails] [nvarchar] (2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
	[MailBox_Size] [int] NULL ,
	[Enabled] [bit] NOT NULL ,
	[AllowRelay] [bit] NOT NULL ,
	[RemotePop3Servers] [image] NULL 
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

