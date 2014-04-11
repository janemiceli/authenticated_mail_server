<%@ Inherits="System.Web.HttpApplication" %>
<%@ Import Namespace="System.IO" %>

<script runat="server" language="C#">

private void Application_BeginRequest(Object sender, EventArgs e)
{
        // Valid values
        // mssql - Data is stored Microsoft Sql server. NOTE: Database must be created and "connStr" must be setted.
        // pgsql - Data is stored Postgre Sql server. NOTE: Database must be created and "connStr" must be setted.
        // xml   - Data is stored xml. NOTE: data dir must have write permission for ASP.NET account.
	Application.Add("DatabaseType","pgsql");
//	Application.Add("connStr","server=kaido_pc;uid=sa;pwd=;database=LS_www_mail");
        Application.Add("connStr","Server=127.0.0.1;uid=Ivar;Password=;database=ls_www_mail;");

	Application.Add("IMAPServerName","localhost");
	Application.Add("IMAPServerPort","143");

        Application.Add("SMTPServer","localhost");

        // Valid values
        // ENG - Engilsh
        // EST - Estonian
        // RUS - Russian
	Application.Add("DefaultLanguage","ENG");
}

</script>
