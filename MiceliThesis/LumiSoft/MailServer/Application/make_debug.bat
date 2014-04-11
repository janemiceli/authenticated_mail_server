path %path%;C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\IDE

                                                                                 
devenv.com ..\ServerAPI\ServerAPI.csproj /rebuild "Debug"
devenv.com ..\MailServer\lsMailServer.csproj /rebuild "Debug"

rem devenv.com ..\MailServerManager\MailServerManager.csproj /rebuild "Debug"

copy Debug\LumiSoft.MailServerAPI.dll Debug\RemoteAdmin\bin\LumiSoft.MailServerAPI.dll 


