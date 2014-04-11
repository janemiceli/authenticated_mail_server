<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Npgsql" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private DataRow dr = null;
private WText m_WTxt = null;

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);

	if(Session["Name"] == null || Session["Name"].ToString().Length < 1){
		Response.Redirect("login.aspx");
	}

	m_WTxt = new WText(Request.PhysicalApplicationPath + "bin\\",Application["DefaultLanguage"].ToString());

	if(Request.Params["ContactID"] != null){
		GetContact();
	}

	if(Request.Params["h1"] != null){
		DeleteContact();
	}
}

private void GetContact()
{
	DataSet ds = new DataSet();
	switch(Application["DatabaseType"].ToString().ToLower())
	{
		case "mssql":
			using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
				conn.Open();
		
				using(SqlCommand cmd = new SqlCommand("GetContact",conn)){
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add( "@ContactID" ,SqlDbType.NVarChar).Value = Request.Params["ContactID"];

					using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
				}
			}
	
			dr = ds.Tables[0].Rows[0];
		
		break;
		
		case "pgsql":
			using(NpgsqlConnection con = new NpgsqlConnection(Application["connStr"].ToString())){
				con.Open();
				
				string cmdText = "select * from lspr_GetContact('" + Request.Params["ContactID"] + "')";
				using(NpgsqlCommand cmd = new NpgsqlCommand(cmdText,con)){
					
					using(NpgsqlDataAdapter sqlCmd = new NpgsqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
					
					// REMOVE ME - pgsql client doesn't handle empty tables ok
					if(ds.Tables.Count == 0){
						ds.Tables.Add("dummy");
					}
					else{
						dr = ds.Tables[0].Rows[0];
					}
				}
			}

		break;
		
		case "xml":
			DataSet ds = new DataSet();
			ds.ReadXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml");
		
			DataView dv = ds.Tables["Contacts"].DefaultView;
			dv.RowFilter = "ContactID = '" + Request.Params["ContactID"] + "'";
		
			if(dv.Count > 0){
				dr = dv[0].Row;
			}
			
		break;
	}
}

private void DeleteContact()
{
	DataSet ds = new DataSet();
	switch(Application["DatabaseType"].ToString().ToLower())
	{
		case "mssql":
			using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
				conn.Open();
		
				using(SqlCommand cmd = new SqlCommand("DeleteContact",conn)){
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add( "@ContactID" ,SqlDbType.NVarChar).Value = Request.Params["ContactID"];

					using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
				}
			}
			
		break;
		
		case "pgsql":
		break;
		
		case "xml":
			ds.ReadXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml");
		
			DataView dv = ds.Tables["Contacts"].DefaultView;
			dv.RowFilter = "ContactID = '" + Request.Params["ContactID"] + "'";
		
			if(dv.Count > 0){
				dv[0].Row.Delete();
		
				ds.WriteXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml",XmlWriteMode.WriteSchema);
			}
			
		break;
	}
	
	Response.Redirect("contacts.aspx");
}

</script>

<html>

<head>
<meta http-equiv="Content-Language" content="et">
<meta name="GENERATOR" content="Microsoft FrontPage 5.0">
<meta name="ProgId" content="FrontPage.Editor.Document">
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252">
<title>LS Mail</title>
<link rel="stylesheet" type="text/css" href="style.css">
<script language="javascript">
<!--

function Edit_ButtonClick(contactID)
{
	window.location.href= "contacts_add.aspx?ContactID=" + contactID;
}

function Delete_ButtonClick()
{
	if(confirm("<% Response.Write(m_WTxt["74"]); %>?")){ // Kas Te olete kindel, et tahate seda kontakti kustutada
		document.form1.h1.value="delete"
		document.form1.submit();
	}
}

function Close_ButtonClick()
{
	window.location.href= "contacts.aspx";
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
          <a href="contacts.aspx"><b><font color="#000000"><% Response.Write(m_WTxt["4"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="settings.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["5"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_right_d.gif" align="center">&nbsp;</td>
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
        <div align="center">
          <center>
          <table border="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="730" id="AutoNumber9" cellpadding="2">
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["48"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(dr["ForName"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["49"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(dr["SurName"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["33"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(dr["Email"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["34"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(dr["Phone1"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["71"]); %>: </b>
              </td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(dr["Phone2"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
          </table>
          </center>
        </div>
        <p>&nbsp;</p>
        <div align="center">
          <center>
          <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="710" id="AutoNumber11">
            <tr>
              <td width="642">
              <input type="button" value="<% Response.Write(m_WTxt["61"]); %>" onClick="javascript:Edit_ButtonClick('<% Response.Write(dr["ContactID"].ToString()); %>')" name="add" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["39"]); %>" onClick="javascript:Delete_ButtonClick()" name="delete" style="width: 100; border: 1px solid #808080"></td>
              <td width="68">
              <p align="right">
              <input type="button" value="<% Response.Write(m_WTxt["40"]); %>" onClick="javascript:Close_ButtonClick()" name="close" style="width: 100; border: 1px solid #808080"></td>
            </tr>
          </table>
          </center>
        </div>
      </form>
      <p>&nbsp;</td>
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