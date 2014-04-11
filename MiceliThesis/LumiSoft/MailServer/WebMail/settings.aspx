<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Npgsql" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private DataRow dr = null;
private WText m_WTxt = null;
private string m_ErrorText = "";

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);

	if(Session["Name"] == null || Session["Name"].ToString().Length < 1){
		Response.Redirect("login.aspx");
	}

	m_WTxt = new WText(Request.PhysicalApplicationPath + "bin\\",Application["DefaultLanguage"].ToString());
	
	try
	{
		if(Request.Params["h1"] != null){
			UpdateSettings();
		}

		GetSettings();
	}
	catch(Exception x){
		m_ErrorText = x.Message;
	}
}

private void GetSettings()
{
	DataSet ds = new DataSet();
	switch(Application["DatabaseType"].ToString().ToLower())
	{
		case "mssql":			
			using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
				conn.Open();		
				
				using(SqlCommand cmd = new SqlCommand("GetSettings",conn)){
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add( "@UserName" ,SqlDbType.NVarChar).Value = Session["Name"];

					using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
				}
				
				if(ds.Tables[0].Rows.Count > 0){
					Session.Add("userEmail", ds.Tables[0].Rows[0]["Email"].ToString());
					Session.Add("deleteFolder", ds.Tables[0].Rows[0]["DeleteFolder"].ToString());
					Session.Add("sentFolder", ds.Tables[0].Rows[0]["SentFolder"].ToString());
				}
	
				dr = ds.Tables[0].Rows[0];
			}			
			
		break;
		
		case "pgsql":
			using(NpgsqlConnection con = new NpgsqlConnection(Application["connStr"].ToString())){
				con.Open();
				
				string cmdText = "select * from lspr_GetSettings('" + Session["Name"] + "')";
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
				
				dr = ds.Tables[0].Rows[0];
			}

		break;
		
		case "xml":			
			ds.ReadXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml");

			if(ds.Tables[0].Rows.Count > 0){
				Session.Add("userEmail", ds.Tables[0].Rows[0]["Email"].ToString());
				Session.Add("deleteFolder", ds.Tables[0].Rows[0]["DeleteFolder"].ToString());
				Session.Add("sentFolder", ds.Tables[0].Rows[0]["SentFolder"].ToString());
			}
		
			dr = ds.Tables["Settings"].Rows[0];
			
		break;
	}
}

private void UpdateSettings()
{
	DataSet ds = new DataSet();
	switch(Application["DatabaseType"].ToString().ToLower())
	{
		case "mssql":
			using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
				conn.Open();
		
				using(SqlCommand cmd = new SqlCommand("UpdateSettings",conn)){
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add( "@UserName"     ,SqlDbType.NVarChar).Value = Session["Name"];
					cmd.Parameters.Add( "@Email"        ,SqlDbType.NVarChar).Value = Request.Params["Email"];
					cmd.Parameters.Add( "@DeleteFolder" ,SqlDbType.NVarChar).Value = Request.Params["deleteFolder"];
					cmd.Parameters.Add( "@SentFolder"   ,SqlDbType.NVarChar).Value = Request.Params["sentFolder"];

					using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
				}
			}
		
		break;
		
		case "pgsql":
			using(NpgsqlConnection con = new NpgsqlConnection(Application["connStr"].ToString())){
				con.Open();
				
				string cmdText = "select * from lspr_UpdateSettings('" + Session["Name"] + "','" + Request.Params["Email"] + "','" + Request.Params["deleteFolder"] + "','" + Request.Params["sentFolder"] + "')";
				using(NpgsqlCommand cmd = new NpgsqlCommand(cmdText,con)){
					
					using(NpgsqlDataAdapter sqlCmd = new NpgsqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
				}
			}

		break;
		
		case "xml":
			ds.ReadXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml");
		
			ds.Tables["Settings"].Rows[0]["Email"] = Request.Params["Email"];
			ds.Tables["Settings"].Rows[0]["DeleteFolder"] = Request.Params["deleteFolder"];
			ds.Tables["Settings"].Rows[0]["SentFolder"] = Request.Params["sentFolder"];
		
			ds.WriteXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml",XmlWriteMode.WriteSchema);
		break;
	}
}

</script>

<html>

<head>
<meta name="GENERATOR" content="Microsoft FrontPage 5.0">
<meta name="ProgId" content="FrontPage.Editor.Document">
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252">
<title>LS Mail</title>
<link rel="stylesheet" type="text/css" href="style.css">
<script language="javascript">
<!--

function Save_ButtonClick()
{
	document.form1.h1.value="update"
	document.form1.submit();
}

//-->
</script>
</head>

<body topmargin="0">

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
          <a href="inbox.aspx?Folder=Inbox"><b><font color="#808080"><% Response.Write(m_WTxt["1"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="compose.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["2"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="folders.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["3"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="contacts.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["4"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_left_ud.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_up.gif" align="center">
          <a href="settings.aspx"><b><font color="#000000"><% Response.Write(m_WTxt["5"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_right_u.gif" align="center">&nbsp;</td>
          <td width="10" background="images/p_right.gif" align="center">&nbsp;</td>
        </tr>
      </table>
      </td>
    </tr>
  </table>
  <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#C0C0C0" width="750" id="AutoNumber3" bgcolor="#F2F2F2">
    <tr>
      <td width="100%" style="border-left-style: solid; border-left-width: 1; border-right-style: solid; border-right-width: 1; border-bottom-style: solid; border-bottom-width: 1">
      <div align="center">
        <center>
        <table border="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="98%" height="20">
          <tr>
            <td width="33%" valign="top" style="border-bottom: 1px solid #C0C0C0"><font color="#808080"><b><i><% Response.Write(Session["userEmail"]); %></i></b></td>
            <td width="33%" valign="top" style="border-bottom: 1px solid #C0C0C0">
            <p align="center">&nbsp;</td>
            <td width="34%" valign="top" style="border-bottom: 1px solid #C0C0C0">
            <p align="right"><a href="login.aspx"><% Response.Write(m_WTxt["8"]); %></a></td>
          </tr>
        </table>
        </center>
      </div>
      <p>&nbsp;</p>
      <form method="POST" style="margin-top: 0; margin-bottom: 0" name="form1">
      <input type="hidden" name="h1" value="">
      <% if(m_ErrorText.Length == 0){ %>
        <div align="center">
          <center>
          <table border="0" cellpadding="2" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber6">
            <tr>
              <td width="50%" align="right"><b><% Response.Write(m_WTxt["79"]); %>: </b> </td>
              <td width="50%">
              <input type="text" name="Email" size="20" style="border: 1px solid #808080; width:250" value="<% Response.Write(dr["Email"].ToString()); %>"></td>
            </tr>
            <tr>
              <td width="50%" align="right">&nbsp;</td>
              <td width="50%">&nbsp;</td>
            </tr>
            <tr>
              <td width="50%" align="right"><b><% Response.Write(m_WTxt["80"]); %>: </b> </td>
              <td width="50%">
              <input type="text" name="deleteFolder" size="20" style="border: 1px solid #808080; width:250" value="<% Response.Write(dr["DeleteFolder"].ToString()); %>"></td>
            </tr>
            <tr>
              <td width="50%" align="right"><b><% Response.Write(m_WTxt["81"]); %>: </b> </td>
              <td width="50%">
              <input type="text" name="sentFolder" size="20" style="border: 1px solid #808080; width:250" value="<% Response.Write(dr["SentFolder"].ToString()); %>"></td>
            </tr>
            <tr>
              <td width="50%" align="right">&nbsp;</td>
              <td width="50%">&nbsp;</td>
            </tr>
          </table>
          </center>
        </div>
        <p align="center">
        &nbsp;</p>
        <p align="center">
        <input type="button" value="<% Response.Write(m_WTxt["47"]); %>" name="save" onClick="javascript:Save_ButtonClick()" style="border:1px solid #808080; width: 100"></p>
      <% } else{ %>
        <p align="center">&nbsp;</p>
        <p align="center"><font color="#FF0000"><% Response.Write(m_ErrorText); %></font></p>
        <p align="center">&nbsp;</p>
      <% } %>
      </form>
      <p>&nbsp;</td>
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