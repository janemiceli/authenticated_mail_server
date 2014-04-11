

CREATE OR REPLACE FUNCTION lspr_AddAlias(varchar,varchar,varchar,varchar,varchar,boolean)
  RETURNS setof lsAliases
AS
'
DECLARE
        _aliasName   ALIAS FOR $1;
        _description ALIAS FOR $2;
        _members     ALIAS FOR $3;
        _domainID    ALIAS FOR $4;
        _aliasID     ALIAS FOR $5;
        _isPublic    ALIAS FOR $6;

        r lsAliases%rowtype;
BEGIN
	insert into lsAliases (AliasID,AliasName,Description,AliasMembers,DomainID,IsPublic) values (_aliasID,_aliasName,_description,_members,_domainID,_isPublic);

	for r in select * from lsAliases where (AliasID = _aliasID) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_AddDomain(varchar,varchar,varchar)
  RETURNS setof lsDomains
AS
'
DECLARE
        _domainName  ALIAS FOR $1;
        _description ALIAS FOR $2;
        _domainID    ALIAS FOR $3;

        r lsDomains%rowtype;
BEGIN
	insert into lsDomains (DomainID,DomainName,Description) values (_domainID,_domainName,_description);
        	
        for r in select * from lsDomains where (DomainID = _domainID) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_AddRoute(varchar,varchar,varchar,varchar,varchar)
  RETURNS setof lsRouting
AS
'
DECLARE
        _pattern     ALIAS FOR $1;
        _mailbox     ALIAS FOR $2;
        _description ALIAS FOR $3;
        _domainID    ALIAS FOR $4;
        _routeID     ALIAS FOR $5;

        r lsRouting%rowtype;
BEGIN
	DECLARE
		domainName varchar := (select DomainName from lsDomains where DomainID=_DomainID);

	insert into lsRouting (Pattern,Mailbox,Description,DomainID,RouteID,DomainName) values (_pattern,_mailbox,_description,_domainID,_routeID,_domainName);
        	
        for r in select * from lsRouting where (RouteID = _routeID) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_AddSecurityEntry(varchar,varchar,varchar,varchar,varchar,varchar,varchar,varchar)
  RETURNS setof lsSecurityList
AS
'
DECLARE
        _description ALIAS FOR $1;
        _protocol    ALIAS FOR $2;
        _type        ALIAS FOR $3;
        _action      ALIAS FOR $4;
        _content     ALIAS FOR $5;
        _startIP     ALIAS FOR $6;
        _endIP       ALIAS FOR $7;
        _securityID  ALIAS FOR $8;

        r lsSecurityList%rowtype;
BEGIN
	insert into lsSecurityList (SecurityID,Description,Protocol,Type,Action,Content,StartIP,EndIP,Enabled) values (_securityID,_description,_protocol,_type,_action,_content,_startIP,_endIP,1);
        	
        for r in select * from lsSecurityList where (SecurityID = _securityID) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_AddUser(varchar,varchar,varchar,varchar,varchar,varchar,int,boolean,boolean,varchar,bytea)
  RETURNS setof lsUsers
AS
'
DECLARE
        _fullName       ALIAS FOR $1;
        _loginName      ALIAS FOR $2;
        _password       ALIAS FOR $3;
        _description    ALIAS FOR $4;
        _emails         ALIAS FOR $5;
        _domainID       ALIAS FOR $6;
        _mailboxSize    ALIAS FOR $7;
        _enabled        ALIAS FOR $8;
        _allowRelay     ALIAS FOR $9;
        _userID         ALIAS FOR $10;
        _remPop3Servers ALIAS FOR $11;

        r lsUsers%rowtype;
BEGIN
	insert into lsUsers (UserID,FullName,UserName,Password,Description,Emails,Mailbox_Size,DomainID,Enabled,AllowRelay,RemotePop3Servers) values (_userID,_fullName,_userName,_password,_description,_emails,_mailboxSize,_domainID,_enabled,_allowRelay,_remPop3Servers);
        	
        for r in select * from lsUsers where (UserID = _userID) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_AuthUser(varchar)
  RETURNS setof lsUsers
AS
'
DECLARE
        _userName ALIAS FOR $1;

        r lsUsers%rowtype;
BEGIN   	
        for r in select * from lsUsers as Users where (UserName=_userName AND enabled=1) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_CheckConnection()
  RETURNS void
AS
'
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_CopyMessage(varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _messageID  ALIAS FOR $1;
        _mailbox    ALIAS FOR $2;
        _folder     ALIAS FOR $3;
        _destFolder ALIAS FOR $4;
BEGIN

        declare _UserID varchar
        select _UserID = (select UserID from lsUsers where UserName = _Mailbox)

        -- Check if destination folder exists
        if not exists(select * from  lsIMAPFolders where UserID = _UserID AND FolderName = _DestFolder) THEN
                select ''Destination Folder('' + _Folder  + '') doesn''t exists'' as ErrorText
	       return
        end if

        if exists(select * from  lsIMAPFolders where UserID = _UserID AND FolderName = _Folder) THEN
                insert into lsMailStore 
	        (MessageID,Mailbox,Folder,Data,Size,TopLines,MessageFlags,Date)  
        	select MessageID,Mailbox,_DestFolder as Folder,Data,Size,TopLines,MessageFlags,Date  
	        from lsMailStore 
        	where (MessageID = _MessageID AND Mailbox = _Mailbox AND Folder = _Folder)        
        else
	       select ''Source Folder('' + _Folder  + '') doesn''t exists'' as ErrorText	
        end if       

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_CreateFolder(varchar,varchar)
  RETURNS void
AS
'
BEGIN
DECLARE
        _mailbox    ALIAS FOR $1;
        _folder     ALIAS FOR $2;

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteAlias(varchar)
  RETURNS void
AS
'
DECLARE
        _aliasID ALIAS FOR $1;
BEGIN
	delete from lsAliases where (AliasID = _aliasID);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteDomain(varchar)
  RETURNS void
AS
'
DECLARE
        domainID ALIAS FOR $1;
BEGIN
	delete from lsRouting where (DomainID = domainID);
	delete from lsAliases where (DomainID = domainID);
	delete from lsUsers   where (DomainID = domainID);

	delete from lsDomains where (DomainID = domainID);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteFolder(varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _mailbox ALIAS FOR $1;
        _folder  ALIAS FOR $2;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteMessage(varchar)
  RETURNS void
AS
'
DECLARE
        messageID ALIAS FOR $1;
BEGIN
	delete from lsMailStore where (MessageID = messageID);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleRoute(varchar)
  RETURNS void
AS
'
DECLARE
        routeID ALIAS FOR $1;
BEGIN
	delete from lsRouting where (RouteID = routeID);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteSecurityEntry(varchar)
  RETURNS void
AS
'
DECLARE
        securityID ALIAS FOR $1;
BEGIN
	delete from lsSecurityList where (SecurityID = securityID);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteUser(varchar)
  RETURNS void
AS
'
DECLARE
        userID ALIAS FOR $1;
BEGIN
	delete from lsUsers where (UserID = userID);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DomainExists(varchar)
  RETURNS void
AS
'
DECLARE
        domainName ALIAS FOR $1;
BEGIN
	select * from lsDomains where (DomainName=domainName);
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_EmailAddressExists(varchar)
  RETURNS void
AS
'
DECLARE
        emailAddress ALIAS FOR $1;
BEGIN
	select * from lsUsers where (Emails like  '%<' + emailAddress + '>%');
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetAlias(varchar)
  RETURNS setof lsAliases
AS
'
DECLARE
        _aliasName ALIAS FOR $1;

        r lsAliases%rowtype;
BEGIN   	
        for r in select * from lsAliases as Aliases where (AliasName = _aliasName) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetAliasesList()
  RETURNS setof lsAliases
AS
'
DECLARE                        
        r lsAliases%rowtype;
BEGIN   	
        for r in select * from lsAliases loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetDomainList()
  RETURNS setof lsDomains
AS
'
DECLARE                        
        r lsDomains%rowtype;
BEGIN   	
        for r in select * from lsDomains loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetFolders(varchar)
  RETURNS void 
AS
'
DECLARE                        
        r lsDomains%rowtype;
DECLARE
        _userName ALIAS FOR $1;
BEGIN
	
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetMailboxFromPattern(varchar,varchar)
  RETURNS void
AS
'
DECLARE
        mailbox    ALIAS FOR $1;
        domainName ALIAS FOR $2;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetMessage(varchar)
  RETURNS setof lsMailStore
AS
'
DECLARE
        messageID ALIAS FOR $1;

        r lsMailStore%rowtype;
BEGIN   	
        for r in select Data from lsMailStore where (MessageID = messageID) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetMessageList(varchar)
  RETURNS void
AS
'
DECLARE
        mailbox ALIAS FOR $1;

        r lsMailStore%rowtype;
BEGIN
	select MessageID,Size from lsMailStore where MAILBOX = mailbox;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetMessageTopLines(varchar)
  RETURNS void
AS
'
DECLARE
        messageID ALIAS FOR $1;

        r lsMailStore%rowtype;
BEGIN
	select TopLines from lsMailStore where MessageID = messageID;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetRouteList()
  RETURNS void
AS
' 
DECLARE
        r lsRouting%rowtype;
BEGIN
        for r in select * from lsRouting loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetSecurityList()
  RETURNS void
AS
' 
DECLARE
        r lsSecurityList%rowtype;
BEGIN
        for r in select * from lsSecurityList loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetSubscribedFolders(varchar)
  RETURNS void
AS
'
DECLARE
        _userName ALIAS FOR $1;
BEGIN
	
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetUserList(varchar)
  RETURNS void
AS
'
DECLARE
        domainID ALIAS FOR $1;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetUserProperties(varchar)
  RETURNS setof lsUsers
AS
'
DECLARE
        userName ALIAS FOR $1;

        r lsUsers%rowtype;
BEGIN   	
        for r in select * from lsUsers where (UserName = userName) loop
                return next r;	
        end loop;
        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_IsPop3AccessAllowed(bigint)
  RETURNS void
AS
'
DECLARE
        IP ALIAS FOR $1;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_IsRelayAllowed(bigint)
  RETURNS void
AS
'
DECLARE
        IP ALIAS FOR $1;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_IsSmtpAccessAllowed(bigint)
  RETURNS void
AS
'
DECLARE
        IP ALIAS FOR $1;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_RenameFolder(varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _userName  ALIAS FOR $1;
        _folder    ALIAS FOR $1;
        _newFolder ALIAS FOR $1;
BEGIN

END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_StoreMessage(varchar,bytea,bigint,bytea)
  RETURNS void
AS
'
DECLARE
        mailBox  ALIAS FOR $1;
        data     ALIAS FOR $2;
        size     ALIAS FOR $3;
        topLines ALIAS FOR $4;
BEGIN
	insert into lsMailStore (MessageID,Mailbox,Data,Size,TopLines) values (newid(),mailbox,data,size,topLines)
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_StoreMessageFlags(varchar,varchar,varchar,int)
  RETURNS void
AS
'
DECLARE
        _messagrID    ALIAS FOR $1;
        _mailbox      ALIAS FOR $2;
        _folder       ALIAS FOR $3;
        _messageFlags ALIAS FOR $4;
BEGIN
	insert into lsMailStore (MessageID,Mailbox,Data,Size,TopLines) values (newid(),mailbox,data,size,topLines)
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_SubscribeFolder(varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _mailbox      ALIAS FOR $1;
        _folder       ALIAS FOR $2;
BEGIN
	
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_TruncateSettings()
  RETURNS void
AS
'
BEGIN
	truncate table lsDomains;
	truncate table lsUsers;
	truncate table lsAliases;
	truncate table lsRouting;
	truncate table lsSecurityList;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UnSubscribeFolder(varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _mailbox      ALIAS FOR $1;
        _folder       ALIAS FOR $2;
BEGIN
	
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UpdateAlias(varchar,varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        aliasID     ALIAS FOR $1;
        aliasName   ALIAS FOR $2;
        description ALIAS FOR $3;
        members     ALIAS FOR $4;
        domainID    ALIAS FOR $5;
BEGIN
	update lsAliases set AliasName=aliasName,Description=description,AliasMembers=members,DomainID=domainID 
           where AliasID = aliasID;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UpdateRoute(varchar,varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        routeID     ALIAS FOR $1;
        pattern     ALIAS FOR $2;
        mailbox     ALIAS FOR $3;
        description ALIAS FOR $4;
        domainID    ALIAS FOR $5;
BEGIN
	DECLARE
		domainName varchar := (select DomainName from lsDomains where DomainID=@DomainID);

	update lsRouting set Pattern=pattern,Mailbox=mailbox,Description=description,DomainID=domainID,DomainName=domainName
           where RouteID = routeID;
END; 
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UpdateSecurityEntry(varchar,varchar,varchar,varchar,varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        securityID  ALIAS FOR $1;
        description ALIAS FOR $2;
        protocol    ALIAS FOR $3;
        type        ALIAS FOR $4;
        action      ALIAS FOR $5;
        content     ALIAS FOR $6;
        startIP     ALIAS FOR $7;
        endIP       ALIAS FOR $8;
BEGIN
	update lsSecurityList set Description=description,Protocol=protocol,Type=type,Action=action,Content=content,StartIP=startIP,EndIP=endIP 
           where SecurityID = securityID;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UpdateUser(varchar,varchar,varchar,varchar,varchar,varchar,varchar,bigint,boolean,boolean,bytea)
  RETURNS void
AS
'
DECLARE
        userID         ALIAS FOR $1;
        fullName       ALIAS FOR $2;
        loginName      ALIAS FOR $3;
        password       ALIAS FOR $4;
        description    ALIAS FOR $5;
        emails         ALIAS FOR $6;
        domainID       ALIAS FOR $7;
        mailboxSize    ALIAS FOR $8;
        enabled        ALIAS FOR $9;
        allowRelay     ALIAS FOR $10;
        remPop3Servers ALIAS FOR $11;
BEGIN
	update lsUsers set FullName=fullName,Password=password,Description=description,Emails=emails,Mailbox_Size=mailboxSize,DomainID=domainID,Enabled=enabled,AllowRelay=allowRelay,RemotePop3Servers=remPop3Servers
           where UserID = userID;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UserExists(varchar)
  RETURNS void
AS
'
DECLARE
        userName ALIAS FOR $1;
BEGIN
	select * from lsUsers as Users where UserName=userName;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_ValidateMailboxSize(varchar)
  RETURNS void
AS
'
DECLARE
        mailbox ALIAS FOR $1;
BEGIN

END;
'
 LANGUAGE 'plpgsql';
