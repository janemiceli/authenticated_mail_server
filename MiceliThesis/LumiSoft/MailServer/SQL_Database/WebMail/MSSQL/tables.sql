

Create table Contacts(
        ContactID uniqueidentifier Default (''),
        ForName   varchar(100)     Default (''),
        SurName   varchar(100)     Default (''),
        Email     varchar(100)     Default (''),
        Phone1    varchar(100)     Default (''),
        Phone2    varchar(100)     Default (''),
	UserName  varchar(100)     Default ('')
)


Create table Users(        
        LoginName    varchar(100) Default (''),
        Email        varchar(100) Default (''),
        DeleteFolder varchar(100) Default (''),
        SentFolder   varchar(100) Default ('')
)
