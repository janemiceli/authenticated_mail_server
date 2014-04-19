path %path%;C:\Program Files (x86)\Microsoft Visual Studio 8\Common7\IDE

                                                                                 
devenv.exe ..\ServerAPI\API\ServerAPI.csproj /rebuild "release"
devenv.exe ..\ServerAPI\API_XML\xml_API.csproj /rebuild "release"
devenv.exe ..\ServerAPI\API_MSSQL\mssql_API.csproj /rebuild "release"
devenv.exe ..\ServerAPI\API_PGSQL\pgsql_API.csproj /rebuild "release"
devenv.exe ..\MailServer\lsMailServer.csproj /rebuild "release"
devenv.exe ..\MailServerService\MailServerService.csproj /rebuild "release"

devenv.exe ..\MailServerConfiguration\MailServerConfiguration.csproj /rebuild "release"
devenv.exe ..\ServerAPI\UserAPI\UserAPI\UserAPI.csproj /rebuild "release"

devenv.exe ..\MailServerManager\MailServerManager.csproj /rebuild "release"

copy ..\version.txt Release\version.txt
 


