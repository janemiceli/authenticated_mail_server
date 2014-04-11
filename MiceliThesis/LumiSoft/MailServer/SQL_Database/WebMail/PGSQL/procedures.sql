

CREATE OR REPLACE FUNCTION lspr_AddContact(varchar,varchar,varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _ContactID ALIAS FOR $1;
        _UserName  ALIAS FOR $2;
        _ForName   ALIAS FOR $3;
        _SurName   ALIAS FOR $4;
        _Email     ALIAS FOR $5;
        _Phone1    ALIAS FOR $6;
        _Phone2    ALIAS FOR $7;
BEGIN
	insert into Contects (ContactID,UserName,ForName,SurName,Email,Phone1,Phone2) 
        values (_ContactID,_UserName,_ForName,_SurName,_Email,_Phone1,_Phone2);

        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_DeleteContact(varchar)
  RETURNS void
AS
'
DECLARE
        _ContactID ALIAS FOR $1;
BEGIN
        delete from Users where (ContactID = _ContactID);

        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetContact(varchar)
  RETURNS setof Contacts
AS
'
DECLARE
        _ContactID ALIAS FOR $1;

        r Contacts%rowtype;
BEGIN
	for r in select * from Contacts where (ContactID = _ContactID) loop
                return next r;	
        end loop;

        return;
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetContacts(varchar)
  RETURNS setof Contacts
AS
'
DECLARE
         r Contacts%rowtype;         
BEGIN
        for r in select * from Contacts loop
                return next r;	
        end loop;

        return;	
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_GetSettings(varchar)
  RETURNS setof Users
AS
'
DECLARE
        _UserName ALIAS FOR $1;

         r Users%rowtype;
BEGIN
        for r in select * from Users where (LoginName = _UserName) loop
                return next r;	
        end loop;

        return;		
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_LogIn(varchar)
  RETURNS setof Users
AS
'
DECLARE
        _UserName ALIAS FOR $1;

         r Users%rowtype;
BEGIN
        if not exists(select * from Users where LoginName = _UserName) THEN
                insert into Users (LoginName,Email,DeleteFolder,SentFolder) 
                values (_UserName,'''','''','''');
        end if;

        for r in select * from Users where (LoginName = _UserName) loop
                return next r;	
        end loop;

        return;		
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UpdateContact(varchar,varchar,varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _ContactID ALIAS FOR $1;
        _UserName  ALIAS FOR $2;
        _ForName   ALIAS FOR $3;
        _SurName   ALIAS FOR $4;
        _Email     ALIAS FOR $5;
        _Phone1    ALIAS FOR $6;
        _Phone2    ALIAS FOR $7;
BEGIN
        update Contacts set UserName=_UserName,ForName=_ForName,SurName=_SurName,Email=_Email,Phone1=_Phone1,Phone2=_Phone2
           where ContactID = _ContactID;

        return;	
END;
'
 LANGUAGE 'plpgsql';


CREATE OR REPLACE FUNCTION lspr_UpdateSettings(varchar,varchar,varchar,varchar)
  RETURNS void
AS
'
DECLARE
        _UserName     ALIAS FOR $1;
        _Email        ALIAS FOR $1;
        _DeleteFolder ALIAS FOR $1;
        _SentFolder   ALIAS FOR $1;
BEGIN
        update Users set Email=_Emaile,DeleteFolder=_DeleteFolder,SentFolder=_SentFolder
           where UserName = _UserName;

        return;	
END;
'
 LANGUAGE 'plpgsql';

