path %path%;C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\IDE

                                                            
devenv.com ..\ServerAPI\ServerAPI.csproj /rebuild "Release"
devenv.com ..\MailServer\lsMailServer.csproj /rebuild "Release"

rem devenv.com ..\MailServerManager\MailServerManager.csproj /rebuild "Release"

copy Release\LumiSoft.MailServerAPI.dll Release\RemoteAdmin\bin\LumiSoft.MailServerAPI.dll
