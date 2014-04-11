<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Npgsql" %>
<%@ Import Namespace="LumiSoft.Net.IMAP.Client" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private bool   m_errors    = false;
private string m_ErrorText = "";
private WText  m_WTxt      = null;

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);

	m_WTxt = new WText(Request.PhysicalApplicationPath + "bin\\",Application["DefaultLanguage"].ToString());

	if(Request.Params["Name"] != null && Request.Params["Password"] != null){

		IMAP_Client clnt = new IMAP_Client();
		bool isConnected = false;

		try{
			clnt.Connect(Application["IMAPServerName"].ToString(),Convert.ToInt32(Application["IMAPServerPort"]));
			isConnected = true;
			clnt.Authenticate(Request.Params["Name"],Request.Params["Password"]);

			Session.Add("Name", Request.Params["Name"]);
			Session.Add("Password", Request.Params["Password"]);

			switch(Application["DatabaseType"].ToString().ToLower())
			{
				case "mssql":					
					using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
						conn.Open();

						DataSet ds = new DataSet();
						using(SqlCommand cmd = new SqlCommand("GetUserID",conn)){
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.Parameters.Add( "@LoginName" ,SqlDbType.NVarChar).Value = Request.Params["Name"];

							using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
								sqlCmd.Fill(ds);
							}
						}
						
						if(ds.Tables[0].Rows.Count > 0){
							Session.Add("userEmail", ds.Tables[0].Rows[0]["Email"].ToString());
							Session.Add("deleteFolder", ds.Tables[0].Rows[0]["DeleteFolder"].ToString());
							Session.Add("sentFolder", ds.Tables[0].Rows[0]["SentFolder"].ToString());
						}
					}					

				break;
				
				case "pgsql":
					using(NpgsqlConnection con = new NpgsqlConnection(Application["connStr"].ToString())){
						con.Open();

						DataSet ds = new DataSet();
						string cmdText = "select * from lspr_login('" + Request.Params["Name"] + "')";
						using(NpgsqlCommand cmd = new NpgsqlCommand(cmdText,con)){
						
							using(NpgsqlDataAdapter sqlCmd = new NpgsqlDataAdapter(cmd)){
								sqlCmd.Fill(ds);
							}
						}
						
						if(ds.Tables[0].Rows.Count > 0){
							Session.Add("userEmail", ds.Tables[0].Rows[0]["Email"].ToString());
							Session.Add("deleteFolder", ds.Tables[0].Rows[0]["DeleteFolder"].ToString());
							Session.Add("sentFolder", ds.Tables[0].Rows[0]["SentFolder"].ToString());
						}
					}
				break;
				
				case "xml":
					if(!File.Exists(Request.PhysicalApplicationPath + "data\\" + Request.Params["Name"] + ".xml")){
						DataSet ds = new DataSet();

						ds.Tables.Add("Settings");
						ds.Tables["Settings"].Columns.Add("Email");
						ds.Tables["Settings"].Columns.Add("DeleteFolder");
						ds.Tables["Settings"].Columns.Add("SentFolder");
						ds.Tables["Settings"].Rows.Add(new object[]{"","",""});

						ds.Tables.Add("Contacts");
						ds.Tables["Contacts"].Columns.Add("ForName");
						ds.Tables["Contacts"].Columns.Add("SurName");
						ds.Tables["Contacts"].Columns.Add("Email");
						ds.Tables["Contacts"].Columns.Add("Phone1");
						ds.Tables["Contacts"].Columns.Add("Phone2");
						ds.Tables["Contacts"].Columns.Add("ContactID");
					
						try{
							ds.WriteXml(Request.PhysicalApplicationPath + "data\\" + Request.Params["Name"] + ".xml",XmlWriteMode.WriteSchema);
						}
						catch{
							m_ErrorText = "Folder " + Request.PhysicalApplicationPath + "data\\" + " has no write permission!";
						}
					}
				
					DataSet dss = new DataSet();
					dss.ReadXml(Request.PhysicalApplicationPath + "data\\" + Request.Params["Name"] + ".xml");
				
					Session["userEmail"] = dss.Tables["Settings"].Rows[0]["Email"];
					Session["deleteFolder"] = dss.Tables["Settings"].Rows[0]["DeleteFolder"];
					Session["sentFolder"] = dss.Tables["Settings"].Rows[0]["SentFolder"];

				break;
			}

			Response.Redirect("inbox.aspx?Folder=Inbox");
		}
		catch(Exception x){
			if(!isConnected){
				m_ErrorText = x.Message;
			}
			m_errors= true;
		}
		
		clnt.Disconnect();
	}
}

</script>

<html>

<head>
<meta http-equiv="Content-Language" content="et">
<meta name="GENERATOR" content="Microsoft FrontPage 5.0">
<meta name="ProgId" content="FrontPage.Editor.Document">
<meta http-equiv="Content-Type" content="text/html; charset=windows-1257">
<title>LS Mail</title>
<link rel="stylesheet" type="text/css" href="style.css">
<script language="javascript">
<!--

function WindowLoad()
{
	document.form1.Name.focus();
	document.form1.Name.value = "<% Response.Write(Request.Params["Name"]); %>";
}

function Login_ButtonClick()
{
	document.form1.submit();
}

//-->
</script>
</head>

<body topmargin="0" onLoad="javascript:WindowLoad()">

<div align="center">
  <center>
  <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="750" id="AutoNumber1" height="50">
    <tr>
      <td width="100%" valign="bottom">
      <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber2" height="38">
        <tr>
          <td width="10" background="images/p_left.gif" align="center">&nbsp;</td>
          <td width="135" background="images/logo.gif" align="center">&nbsp;</td>
          <td width="10" background="images/p_left_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <b><font color="#808080"><% Response.Write(m_WTxt["1"].ToUpper()); %></font></b></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <b><font color="#808080"><% Response.Write(m_WTxt["2"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <b><font color="#808080"><% Response.Write(m_WTxt["3"].ToUpper()); %></font></b></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <b><font color="#808080"><% Response.Write(m_WTxt["4"].ToUpper()); %></font></b></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <b><font color="#808080"><% Response.Write(m_WTxt["5"].ToUpper()); %></font></b></td>
          <td width="10" background="images/p_right_d.gif" align="center">&nbsp;</td>
          <td width="10" background="images/p_right.gif" align="center">&nbsp;</td>
        </tr>
      </table>
      </td>
    </tr>
  </table>
  <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#C0C0C0" width="750" id="AutoNumber3" height="150" bgcolor="#F2F2F2">
    <tr>
      <td width="100%" style="border-left-style: solid; border-left-width: 1; border-right-style: solid; border-right-width: 1; border-bottom-style: solid; border-bottom-width: 1">
      <form method="POST" style="margin-top: 0; margin-bottom: 0" name="form1">
      	<% if(m_errors){ %>      	
        <p align="center">&nbsp;</p>
        <p align="center"><font color="#FF0000"><% if(m_ErrorText.Length == 0){ Response.Write(m_WTxt["50"]); } else { Response.Write(m_ErrorText); } %></font></p>
        <p align="center">&nbsp;</p>
        <% } %>
        <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber7">
          <tr>
            <td width="50%" align="right" height="25"><b><% Response.Write(m_WTxt["6"]); %>:&nbsp;
            </b> </td>
            <td width="50%" height="25">
            <input type="text" name="Name" size="20" style="border: 1px solid #808080"></td>
          </tr>
          <tr>
            <td width="50%" align="right" height="25"><b><% Response.Write(m_WTxt["7"]); %>:&nbsp; </b> </td>
            <td width="50%" height="25">
            <input type="password" name="Password" size="20" style="border: 1px solid #808080"></td>
          </tr>
          <tr>
            <td width="100%" align="right" colspan="2" height="25">
            <p align="center">
            &nbsp;<p align="center">
            <b>
            <input type="button" value="<% Response.Write(m_WTxt["15"]); %>" onClick="javascript:Login_ButtonClick()" name="login" style="border:1px solid #808080; width: 100"></b></td>
          </tr>
        </table>
        <p>&nbsp;</p>
      </form>
      </td>
    </tr>
  </table>
  <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="750" id="AutoNumber4" height="25">
    <tr>
      <td width="100%" valign="bottom">
      <table border="1" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#C0C0C0" width="100%" id="AutoNumber5" bgcolor="#F2F2F2" height="16">
        <tr>
          <td width="100%" bgcolor="#FCFCFC" height="16">
          <p align="center"><font color="#808080">2003 by LumiSoft</font></td>
        </tr>
      </table>
      </td>
    </tr>
  </table>
  </center>
</div>

<p align="center">&nbsp;</p>

</body>

</html>