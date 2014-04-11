
CREATE table lsAliases(
        AliasID varchar(40) UNIQUE,
        DomainID varchar(40), 
        AliasName varchar(500), 
        Description varchar(500),  
        AliasMembers varchar(8000),
        IsPublic boolean   
);


CREATE table lsDomains(
        DomainID varchar(40) UNIQUE,
        DomainName varchar(500),
        Description varchar(500)
);


CREATE table lsMailStore(
        MessageID varchar(40) UNIQUE,
        Mailbox varchar(500),
        Folder varchar(500),
        Data bytea,
        Size bigint,
        TopLines bytea,
        MessageFlags bigint,
        Date timestamp,
        UID int
);


CREATE table lsRouting(
        RouteID varchar(40) UNIQUE,
        DomainID varchar(40),        
        Pattern varchar(500),
        Mailbox varchar(500),
        Description varchar(500),
        DomainName varchar(500)
);


CREATE table lsSecurityList(
        SecurityID varchar(40) UNIQUE,
        Description varchar(500),
        Protocol varchar(500),
        Type varchar(500),
        Action varchar(500),
        Content varchar(500),
        StartIP bigint,
        EndIP bigint,
        Enabled boolean
);


CREATE table lsUsers(
        UserID varchar(40) UNIQUE,
        DomainID varchar(40),
        FullName varchar(500),
        UserName varchar(500),
        Password varchar(500),
        Description varchar(500),
        Emails varchar(8000),
        MailBox_Size bigint,
        Enabled boolean,
        AllowRelay boolean,
        RemotePop3Servers bytea
);

CREATE table lsIMAPFolders(
        UserID varchar(40) UNIQUE,
        FolderName varchar(500)
);

CREATE table lsIMAPSubscribedFolders(
        UserID varchar(40) UNIQUE,
        FolderName varchar(500)
);
