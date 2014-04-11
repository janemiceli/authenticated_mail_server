if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_AddAlias]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_AddAlias]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_AddDomain]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_AddDomain]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_AddRoute]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_AddRoute]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_AddSecurityEntry]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_AddSecurityEntry]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_AddUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_AddUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_AuthUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_AuthUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_CheckConnection]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_CheckConnection]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_CopyMessage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_CopyMessage]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_CreateFolder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_CreateFolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteAlias]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteAlias]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteDomain]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteDomain]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteFolder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteFolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteMessage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteMessage]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteRoute]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteRoute]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteSecurityEntry]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteSecurityEntry]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DeleteUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DeleteUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_DomainExists]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_DomainExists]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_EmailAddressExists]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_EmailAddressExists]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetAlias]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetAlias]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetAliasesList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetAliasesList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetDomainList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetDomainList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetFolders]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetFolders]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetMailboxFromPattern]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetMailboxFromPattern]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetMessage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetMessage]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetMessageList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetMessageList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetMessageTopLines]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetMessageTopLines]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetRouteList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetRouteList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetSecurityList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetSecurityList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetSubscribedFolders]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetSubscribedFolders]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetUserList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetUserList]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_GetUserProperties]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_GetUserProperties]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_IsPop3AccessAllowed]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_IsPop3AccessAllowed]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_IsRelayAllowed]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_IsRelayAllowed]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_IsSmtpAccessAllowed]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_IsSmtpAccessAllowed]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_RenameFolder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_RenameFolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_StoreMessage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_StoreMessage]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_StoreMessageFlags]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_StoreMessageFlags]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_SubscribeFolder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_SubscribeFolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_TruncateSettings]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_TruncateSettings]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_UnSubscribeFolder]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_UnSubscribeFolder]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_UpdateAlias]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_UpdateAlias]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_UpdateRoute]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_UpdateRoute]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_UpdateSecurityEntry]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_UpdateSecurityEntry]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_UpdateUser]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_UpdateUser]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_UserExists]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_UserExists]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[lspr_ValidateMailboxSize]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure [dbo].[lspr_ValidateMailboxSize]
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_AddAlias 

@AliasName	varchar(100)	=NULL,
@Description	varchar(100)	=NULL,
@Members	varchar(2000)	=NULL,
@DomainID	uniqueidentifier	=NULL,
@AliasID	uniqueidentifier	=NULL,
@IsPublic	bit		=0

AS

set nocount on

-- If alias id isn't specified, assign new
if(@AliasID is NULL)
begin
      select   @AliasID = newid()
end

insert lsAliases (AliasID,AliasName,Description,AliasMembers,DomainID,IsPublic) values (@AliasID,@AliasName,@Description,@Members,@DomainID,@IsPublic)

select @AliasID as AliasID,@AliasName as AliasName,@Description as Description,@Members as AliasMembers,@DomainID as DomainID,@IsPublic as IsPublic

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_AddDomain 

@DomainName	nvarchar(30)	=NULL,
@Description	nvarchar(30)	=NULL,
@DomainID       uniqueidentifier   =NULL

AS

set nocount on

-- If domain id isn't specified, assign new
if(@DomainID is NULL)
begin
      select   @DomainID = newid()
end

insert lsDomains (DomainID,DomainName,Description) values (@DomainID,@DomainName,@Description)

select @DomainID as DomainID,@DomainName as DomainName,@Description as Description

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_AddRoute

@Pattern	varchar(100)	=NULL,
@Mailbox	varchar(50)	=NULL,
@Description	varchar(100)	=NULL,
@DomainID	uniqueidentifier	=NULL,
@RouteID	uniqueidentifier	=NULL

AS

set nocount on

-- If route id isn't specified, assign new
if(@RouteID is NULL)
begin
      select   @RouteID = newid()
end

-- Get domain name
declare @DomainName nvarchar(50)
select    @DomainName = (select DomainName from lsDomains where DomainID=@DomainID)

insert lsRouting (Pattern,Mailbox,Description,DomainID,RouteID,DomainName) values (@Pattern,@Mailbox,@Description,@DomainID,@RouteID,@DomainName)

select @Pattern as Pattern,@Mailbox as Mailbox,@Description as DESCRIPTION,@DomainID as DomainID,@RouteID as RouteID,@DomainName as DomainName

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_AddSecurityEntry

@Description	varchar(100)	=NULL,
@Protocol	varchar(50)	=NULL,
@Type	      	varchar(100)	=NULL,
@Action	varchar(50)	=NULL,
@Content	varchar(50)	=NULL,
@StartIP	varchar(50)	=NULL,
@EndIP	varchar(50)	=NULL,
@SecurityID	uniqueidentifier	=NULL

AS

set nocount on

-- If security id isn't specified, assign new
if(@SecurityID is NULL)
begin
      select   @SecurityID = newid()
end


insert lsSecurityList (SecurityID,Description,Protocol,Type,Action,Content,StartIP,EndIP,Enabled) values (@SecurityID,@Description,@Protocol,@Type,@Action,@Content,@StartIP,@EndIP,1)

select @SecurityID as SecurityID,@Description as Description,@Protocol as Protocol,@Type as Type,@Action as Action,@Content as Content,@StartIP as StartIP,@EndIP as EndIP

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_AddUser

@FullName	varchar(50)	=NULL,
@UserName	varchar(50)	=NULL,
@Password	varchar(50)	=NULL,
@Description	varchar(100)	=NULL,
@Emails	varchar(2000)	=NULL,
@DomainID	uniqueidentifier	=NULL,
@MailboxSize	int		=0,
@Enabled          bit                       = true,
@AllowRelay      bit                       = true,
@UserID	uniqueidentifier  =NULL,
@RemotePop3Servers image     = NULL

 AS

set nocount on

-- If security id isn't specified, assign new
if(@UserID is NULL)
begin
      select   @UserID = newid()
end


insert lsUsers (UserID,FullName,UserName,Password,Description,Emails,Mailbox_Size,DomainID,Enabled,AllowRelay,RemotePop3Servers) values (@UserID,@FullName,@UserName,@Password,@Description,@Emails,@MailboxSize,@DomainID,@Enabled,@AllowRelay,@RemotePop3Servers)

select @UserID as UserID,@FullName as FullName,@UserName as UserName ,@Password as Password,@Description as Description,@Emails as Emails,@MailboxSize as Mailbox_Size,@DomainID as DomainID,@Enabled as Enabled,@AllowRelay as AllowRelay,@RemotePop3Servers as RemotePop3Servers

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_AuthUser

@UserName	nvarchar(100)	=NULL

AS

select * from lsUsers  as Users where (UserName=@UserName AND Enabled=1)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_CheckConnection

 AS

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_CopyMessage
@MessageID	uniqueidentifier	 = NULL,
@Mailbox          as nvarchar(100),
@Folder             as nvarchar(100),
@DestFolder     as nvarchar(100)
AS


declare @UserID as uniqueidentifier
select @UserID = (select UserID from lsUsers where UserName = @Mailbox)

-- Check if destination folder exists
if not exists(select * from  lsIMAPFolders where UserID = @UserID AND FolderName = @DestFolder)
begin
	select 'Destination Folder(' + @Folder  + ') doesn''t exists' as ErrorText
	return
end

if exists(select * from  lsIMAPFolders where UserID = @UserID AND FolderName = @Folder)
begin
	insert into lsMailStore 
		(MessageID,Mailbox,Folder,Data,Size,TopLines,MessageFlags,Date)  
	select MessageID,Mailbox,@DestFolder as Folder,Data,Size,TopLines,MessageFlags,Date  
	from lsMailStore 
	where (MessageID = @MessageID AND Mailbox = @Mailbox AND Folder = @Folder)
end
else
begin
	select 'Source Folder(' + @Folder  + ') doesn''t exists' as ErrorText	
end

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_CreateFolder 
@UserName as nvarchar(100),
@Folder        as nvarchar(100)
AS


declare @UserID as uniqueidentifier
select @UserID = (select UserID from lsUsers where UserName = @UserName)

if exists(select * from  lsIMAPFolders where UserID = @UserID AND FolderName = @Folder)
begin
	select 'Folder(' + @Folder  + ') already exists' as ErrorText
end
else
begin
	insert into lsIMAPFolders (UserID,FolderName) values (@UserID,@Folder)
end

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteAlias

@AliasID	uniqueidentifier	=NULL

AS

delete from lsAliases where AliasID=@AliasID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteDomain 

@DomainID	uniqueidentifier	=NULL

AS

delete from lsRouting where DomainID=@DomainID
delete from lsAliases where DomainID=@DomainID
delete from lsUsers    where DomainID=@DomainID

delete from lsDomains where DomainID=@DomainID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteFolder 
@UserName as nvarchar(100),
@Folder        as nvarchar(100)
AS


declare @UserID as uniqueidentifier
select @UserID = (select UserID from lsUsers where UserName = @UserName)

if exists(select * from  lsIMAPFolders where UserID = @UserID AND FolderName = @Folder)
begin
	delete lsIMAPFolders where (UserID = @UserID AND FolderName = @Folder)
end
else
begin
	select 'Folder(' + @Folder  + ') doesn''t exist' as ErrorText	
end

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteMessage 

@MessageID	uniqueidentifier	= NULL,
@Mailbox	nvarchar(100)	= NULL,
@Folder	nvarchar(100)	= NULL

AS

delete from lsMailStore where MessageID = @MessageID AND Mailbox = @Mailbox AND Folder = @Folder

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteRoute

@RouteID	uniqueidentifier	=NULL

AS

delete from lsRouting where RouteID=@RouteID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteSecurityEntry

@SecurityID	uniqueidentifier	=NULL

AS

delete from lsSecurityList where SecurityID=@SecurityID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DeleteUser

@UserID	uniqueidentifier	=NULL

AS

delete from lsUsers where UserID=@UserID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_DomainExists 

@DomainName  nvarchar(100)	=NULL

AS

select * from lsDomains where DomainName=@DomainName

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_EmailAddressExists

@EmailAddress	nvarchar(100)	=NULL

 AS

select * from lsUsers where Emails like  '%<' + @EmailAddress + '>%'

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetAlias

@AliasName	nvarchar(100)	=NULL

AS

select * from lsAliases  as Aliases where AliasName=@AliasName

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetAliasesList

 AS

select * from lsAliases as Aliases

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetDomainList  AS

select * from lsDomains  as Domains

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetFolders 
@UserName as nvarchar(100)
AS

select * from lsIMAPFolders where UserID=(select UserID from lsUsers where UserName = @UserName)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetMailboxFromPattern

@LocalPart	  varchar(100)	=NULL,
@DomainName  nvarchar(100)	=NULL

AS

set nocount on

declare @DomainID uniqueidentifier
declare @Mailbox     nvarchar(30)

select @DomainID = (select DomainID from lsDomains where DomainName=@DomainName)

if(@DomainID != NULL)
begin
     declare rs cursor for select Pattern,Mailbox from lsRouting where DomainID=@DomainID order by Len(Pattern) desc,Pattern desc
     open rs

    declare @Pattern nvarchar(30)
    declare @Mailbx  nvarchar(30)
    fetch next from  rs into @Pattern,@Mailbx
    while(@@FETCH_STATUS = 0)
    begin

         -- Exact address routing
        if(charindex('*',@Pattern) = 0 AND lower(@LocalPart) = lower(@Pattern))
        begin
	select @Mailbox = @Mailbx
              break;
        end

         -- Route all messages
         if(@Pattern = '*')
         begin
                 select @Mailbox = @Mailbx
                 break;
         end

         -- Starts with *
         if(left(@Pattern,1) = '*')
         begin
               -- Ends with *
               if(right(@Pattern,1) = '*')
               begin
                      --  *pattern*
                      if(patindex('%' + upper(replace(@Pattern,'*','')) + '%', @LocalPart) > 0)
                     begin
                            select @Mailbox = @Mailbx
                            break;
                     end
               end
               -- *pattern
               else
               begin
                      if(upper(right(@LocalPart,len(replace(@Pattern,'*','')))) = upper(replace(@Pattern,'*','')))
                      begin
                           select @Mailbox = @Mailbx
                           break;
                      end
               end
         end
        -- pattern*
        else if(right(@Pattern,1) = '*')
        begin
              if(upper(left(@LocalPart,len(replace(@Pattern,'*','')))) = upper(replace(@Pattern,'*','')))
              begin
                         select @Mailbox = @Mailbx
                         break;
              end
        end

         -- Get next data row
         fetch next from  rs into @Pattern,@Mailbx
    end

     close rs
     deallocate rs
end


select @Mailbox as Mailbox
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetMessage

@MessageID	uniqueidentifier	= NULL,
@Mailbox	nvarchar(100)	= NULL,
@Folder	nvarchar(100)	= NULL

 AS

select Data from lsMailStore where MessageID = @MessageID AND Mailbox = @Mailbox AND Folder = @Folder

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetMessageList

@Mailbox	nvarchar(100)	=NULL,
@Folder	nvarchar(100)	=NULL
AS

select MessageID,Size,Date,MessageFlags,UID  from lsMailStore where MAILBOX = @Mailbox AND Folder = @Folder

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetMessageTopLines

@MessageID	uniqueidentifier	 =  NULL,
@Mailbox           nvarchar(100)      = NULL,
@Folder             nvarchar(100)      = NULL

AS

select TopLines from lsMailStore where MessageID = @MessageID AND  Mailbox = @Mailbox AND Folder = @Folder

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetRouteList

 AS

select * from lsRouting

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetSecurityList

 AS

select *  from lsSecurityList

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetSubscribedFolders 
@UserName as nvarchar(100)
AS

select * from lsIMAPSubscribedFolders where UserID=(select UserID from lsUsers where UserName = @UserName)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetUserList

@DomainID  uniqueidentifier	=NULL

 AS

if(@DomainID != null)
begin
      select * from lsUsers where DomainID=@DomainID
end
else
begin
       select * from lsUsers
end

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_GetUserProperties

@UserName	nvarchar(100)	=NULL

 AS

select * from lsUsers where UserName = @UserName

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_IsPop3AccessAllowed

@IP	bigint	=NULL

AS

declare @allowed as bit
select   @allowed = 0

--1) Check if denied  - if denied, don't check allowed
--2) Check if allowed

-- Check if access is denied
if(not exists(select * from lsSecurityList where Protocol='POP3' AND Action='Deny' AND StartIP <= @IP AND  EndIP >= @IP))
begin
        -- Check if access is allowed
        if(exists(select * from lsSecurityList where Protocol='POP3' AND Action='Allow' AND StartIP <= @IP AND  EndIP >= @IP))
       begin
            select @Allowed = 1
       end       
end

select @Allowed as Allowed

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_IsRelayAllowed

@IP	bigint	=NULL

AS

declare @allowed as bit
select   @allowed = 0

--1) Check if denied  - if denied, don't check allowed
--2) Check if allowed

-- Check if relay is denied
if(not exists(select * from lsSecurityList where Protocol='SMTP' AND Action='Deny Relay' AND StartIP <= @IP AND  EndIP >= @IP))
begin
        -- Check if relay is allowed
        if(exists(select * from lsSecurityList where Protocol='SMTP' AND Action='Allow Relay' AND StartIP <= @IP AND  EndIP >= @IP))
       begin
            select @Allowed = 1
       end       
end

select @Allowed as Allowed


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_IsSmtpAccessAllowed

@IP	bigint	=NULL

AS

declare @allowed as bit
select   @allowed = 0

--1) Check if denied  - if denied, don't check allowed
--2) Check if allowed

-- Check if access is denied
if(not exists(select * from lsSecurityList where Protocol='SMTP' AND Action='Deny' AND StartIP <= @IP AND  EndIP >= @IP))
begin
        -- Check if access is allowed
        if(exists(select * from lsSecurityList where Protocol='SMTP' AND Action='Allow' AND StartIP <= @IP AND  EndIP >= @IP))
       begin
            select @Allowed = 1
       end       
end

select @Allowed as Allowed

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_RenameFolder 
@UserName  as nvarchar(100),
@Folder         as nvarchar(100),
@NewFolder  as nvarchar(100)
AS


declare @UserID as uniqueidentifier
select @UserID = (select UserID from lsUsers where UserName = @UserName)

-- Check if destination folder exists
if exists(select * from  lsIMAPFolders where UserID = @UserID AND FolderName = @NewFolder)
begin
	select 'Destination Folder(' + @Folder  + ') already exists' as ErrorText
	return
end

if exists(select * from  lsIMAPFolders where UserID = @UserID AND FolderName = @Folder)
begin
	update lsIMAPFolders  set FolderName = @NewFolder where UserID = @UserID AND FolderName = @Folder
end
else
begin
	select 'Source Folder(' + @Folder  + ') doesn''t exists' as ErrorText	
end

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_StoreMessage

@Mailbox	 nvarchar(100)	= NULL,
@Folder	 nvarchar(100)	= NULL,
@Data		 image		= NULL,
@Size		 bigint		= 0,
@TopLines	 image		= NULL,
@Date		 DateTime	= NULL,
@MessageFlags int	             = 0

 AS

insert lsMailStore (MessageID,Mailbox,Folder,Data,Size,TopLines,Date,MessageFlags) values (newid(),@Mailbox,@Folder,@Data,@Size,@TopLines,@Date,@MessageFlags)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_StoreMessageFlags

@MessageID	uniqueidentifier	= NULL,
@Mailbox	 nvarchar(100)	= NULL,
@Folder	 nvarchar(100)	= NULL,
@MessageFalgs int		= NULL

 AS

Update lsMailStore set MessageFlags = @MessageFalgs where MessageID = @MessageID AND Mailbox = @Mailbox AND Folder = @Folder

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_SubscribeFolder 
@UserName as nvarchar(100),
@Folder        as nvarchar(100)
AS

-- ToDo: check if exist, delete or just skip ???

declare @UserID as uniqueidentifier
select @UserID = (select UserID from lsUsers where UserName = @UserName)

insert into lsIMAPSubscribedFolders (UserID,FolderName) values (@UserID,@Folder)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_TruncateSettings

 AS

truncate table lsDomains
truncate table lsUsers
truncate table lsAliases
truncate table lsRouting
truncate table lsSecurityList

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_UnSubscribeFolder 
@UserName as nvarchar(100),
@Folder        as nvarchar(100)
AS

declare @UserID as uniqueidentifier
select @UserID = (select UserID from lsUsers where UserName = @UserName)

delete  lsIMAPSubscribedFolders where UserID =  @UserID AND FolderName = @Folder

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_UpdateAlias
 
@AliasID	uniqueidentifier	=NULL,
@AliasName	varchar(100)	=NULL,
@Description	varchar(100)	=NULL,
@Members	varchar(2000)	=NULL,
@DomainID	uniqueidentifier	=NULL,
@IsPublic	bit		=0

AS

update lsAliases set AliasName=@AliasName,Description=@Description,AliasMembers=@Members,DomainID=@DomainID,IsPublic=@IsPublic
           where  AliasID=@AliasID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_UpdateRoute

@RouteID	uniqueidentifier	=NULL,
@Pattern	varchar(100)	=NULL,
@Mailbox	varchar(100)	=NULL,
@Description	varchar(100)	=NULL,
@DomainID	uniqueidentifier	=NULL

AS

-- Get domain name
declare @DomainName nvarchar(50)
select    @DomainName = (select DomainName from lsDomains where DomainID=@DomainID)

update lsRouting set Pattern=@Pattern,Mailbox=@Mailbox,Description=@Description,DomainID=@DomainID,DomainName=@DomainName
           where  RouteID=@RouteID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_UpdateSecurityEntry

@SecurityID	uniqueidentifier	=NULL,
@Description	varchar(100)	=NULL,
@Protocol	varchar(50)	=NULL,
@Type	      	varchar(100)	=NULL,
@Action	varchar(50)	=NULL,
@Content	varchar(50)	=NULL,
@StartIP	varchar(50)	=NULL,
@EndIP	varchar(50)	=NULL

AS

update lsSecurityList set Description=@Description,Protocol=@Protocol,Type=@Type,Action=@Action,Content=@Content,StartIP=@StartIP,EndIP=@EndIP 
           where  SecurityID=@SecurityID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_UpdateUser

@UserID	uniqueidentifier	=NULL,
@FullName	varchar(50)	=NULL,
@UserName	varchar(50)	=NULL,
@Password	varchar(50)	=NULL,
@Description	varchar(50)	=NULL,
@Emails	varchar(2000)	=NULL,
@DomainID	uniqueidentifier	=NULL,
@MailboxSize	int		=0,
@Enabled          bit                       = true,
@AllowRelay      bit                       = true,
@RemotePop3Servers image       = NULL

AS

update lsUsers set FullName=@FullName/*,UserName=@UserName*/,Password=@Password,Description=@Description,Emails=@Emails,Mailbox_Size=@MailboxSize,DomainID=@DomainID,Enabled=@Enabled,AllowRelay=@AllowRelay,RemotePop3Servers=@RemotePop3Servers
           where  UserID=@UserID

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_UserExists 

@UserName	nvarchar(100)	=NULL

AS

select * from lsUsers  as Users where UserName=@UserName

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS OFF 
GO


CREATE PROCEDURE lspr_ValidateMailboxSize

@Mailbox	nvarchar(100)	=NULL

AS

set nocount on

declare @Size int , @AllowedSize int
select   @Size = 0
select   @AllowedSize = -1

-- Get mailbox size
if(exists(select Mailbox_Size from lsUsers where UserName=@Mailbox))
begin
    select   @AllowedSize = (select Mailbox_Size from lsUsers where UserName=@Mailbox)
end


-- Count mailbox size
if(exists(select MailBox from lsMailStore where Mailbox=@Mailbox))
begin
    select   @Size = (select sum(Size) from lsMailStore where Mailbox=@Mailbox)
end


if(@Size < @AllowedSize*1000000)  -- Allowed size in mb, size is bytes
  select cast(1 as Bit) as Validated
else
  select cast(0 as Bit) as Validated

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

