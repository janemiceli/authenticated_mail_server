path %path%;C:\Program Files\Microsoft Visual Studio 8\Common7\IDE\

                                                            
VCSExpress.exe ..\ServerAPI\API\ServerAPI.csproj /rebuild "Release" 
VCSExpress.exe ..\ServerAPI\API_XML\xml_API.csproj /rebuild "Release"
VCSExpress.exe ..\ServerAPI\API_MSSQL\mssql_API.csproj /rebuild "Release"
VCSExpress.exe ..\ServerAPI\API_PGSQL\pgsql_API.csproj /rebuild "Release"
VCSExpress.exe ..\MailServer\lsMailServer.csproj /rebuild "Release"
VCSExpress.exe ..\MailServerService\MailServerService.csproj /rebuild "Release"

VCSExpress.exe ..\MailServerConfiguration\MailServerConfiguration.csproj /rebuild "Release"
VCSExpress.exe ..\ServerAPI\UserAPI\UserAPI\UserAPI.csproj /rebuild "Release"

VCSExpress.exe ..\MailServerManager\MailServerManager.csproj /rebuild "Release"

copy ..\version.txt Release\version.txt
del Release\*.pdb

