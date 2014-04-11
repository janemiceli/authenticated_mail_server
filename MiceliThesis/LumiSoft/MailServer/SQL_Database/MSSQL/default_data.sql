
TRUNCATE table lsSecurityList
INSERT into lsSecurityList (SecurityID,Description,Protocol,Type,Action,Content,StartIP,EndIP,Enabled) VALUES('6136e6a1-2069-4264-8bcf-bfa721b4c83e','Allow SMTP access to everyone','SMTP','IP Range','Allow','0.0.0.0-255.255.255.255',0,255255255255,1)
INSERT into lsSecurityList (SecurityID,Description,Protocol,Type,Action,Content,StartIP,EndIP,Enabled) VALUES('6136e6a1-2069-4264-8bcf-bfa721b4c83a','Allow POP3 access to everyone','POP3','IP Range','Allow','0.0.0.0-255.255.255.255',0,255255255255,1)
INSERT into lsSecurityList (SecurityID,Description,Protocol,Type,Action,Content,StartIP,EndIP,Enabled) VALUES('6136e6a1-2069-4264-8bcf-bfa721b4c83b','Allow relay to this IP','SMTP','IP Range','Allow','192.168.1.1-192.168.1.100',192168001001,192168001100,1)


