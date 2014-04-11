if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[AddContact]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[AddContact]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DeleteContact]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[DeleteContact]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetContact]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetContact]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetContacts]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetContacts]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetSettings]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetSettings]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[GetUserID]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[GetUserID]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UpdateContact]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[UpdateContact]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[UpdateSettings]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[UpdateSettings]
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE AddContact
@UserName	nvarchar(50),
@ForName	nvarchar(50),
@SurName	nvarchar(50),
@Email		nvarchar(50),
@Phone1	nvarchar(50),
@Phone2	nvarchar(50)
AS

declare @ContactID as uniqueidentifier
set @ContactID = newid()

insert into Contacts (UserName, ContactID, ForName, SurName, Email, Phone1, Phone2) values(@UserName, @ContactID, @ForName, @SurName, @Email, @Phone1, @Phone2)
  
select * from Contacts where ContactID = @ContactID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE DeleteContact
@ContactID uniqueidentifier
 AS

delete from Contacts where ContactID = @ContactID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE GetContact
@ContactID	uniqueidentifier
 AS

select * from Contacts where ContactID = @ContactID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE GetContacts
@UserName	nvarchar(50)
 AS

select * from Contacts where UserName = @UserName order by SurName, ForName
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE GetSettings
@UserName                nvarchar(50)
 AS

select * from Users where LoginName = @UserName
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE GetUserID
@LoginName nvarchar(50)
 AS

if not exists (select * from Users where LoginName=@LoginName)
begin
insert into Users (LoginName) values (@LoginName)
end

select * from Users where LoginName=@LoginName
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateContact
@ContactID	uniqueidentifier,
@ForName	nvarchar(50),
@SurName	nvarchar(50),
@Email		nvarchar(50),
@Phone1	nvarchar(50),
@Phone2	nvarchar(50)
 AS

update Contacts set ForName = @ForName, SurName = @SurName, Email = @Email, Phone1 = @Phone1, Phone2 = @Phone2 where ContactID = @ContactID
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE PROCEDURE UpdateSettings
@UserName	nvarchar(50),
@Email		nvarchar(50),
@DeleteFolder	nvarchar(50),
@SentFolder	nvarchar(50)
 AS

update Users set Email = @Email, DeleteFolder = @DeleteFolder, SentFolder = @SentFolder where LoginName = @UserName
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

