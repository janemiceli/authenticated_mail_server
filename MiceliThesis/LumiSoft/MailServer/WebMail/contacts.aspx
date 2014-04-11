<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="Npgsql" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private WText m_WTxt = null;

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);

	if(Session["Name"] == null || Session["Name"].ToString().Length < 1){
		Response.Redirect("login.aspx");
	}

	m_WTxt = new WText(Request.PhysicalApplicationPath + "bin\\",Application["DefaultLanguage"].ToString());

	// Delete contact
	if(Request.Params["h1"] != null){
		DeleteContact();
	}
}

private void DeleteContact()
{
	string[] text = Request.Params["h3"].ToString().Split(';');
	string   line = "";
	
	DataSet ds = new DataSet();
	switch(Application["DatabaseType"].ToString().ToLower())
	{
		case "mssql":
			foreach(string ss in text){
				using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
					conn.Open();
		
					using(SqlCommand cmd = new SqlCommand("DeleteContact",conn)){
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add( "@ContactID" ,SqlDbType.NVarChar).Value = ss;

						using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
							sqlCmd.Fill(ds);
						}
					}
				}
			}
			
		break;
		
		case "pgsql":
		break;
		
		case "xml":
			ds.ReadXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml");
		
			foreach(string ss in text){
				DataView dv = ds.Tables["Contacts"].DefaultView;
				dv.RowFilter = "ContactID = '" + ss + "'";
		
				if(dv.Count > 0){
					dv[0].Row.Delete();
				}
			}
		
			ds.WriteXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml",XmlWriteMode.WriteSchema);
			
		break;
	}
}

private DataSet GetData()
{
	DataSet ds = new DataSet();
	switch(Application["DatabaseType"].ToString().ToLower())
	{
		case "mssql":
			using(SqlConnection conn = new SqlConnection(Application["connStr"].ToString())){
				conn.Open();
		
				using(SqlCommand cmd = new SqlCommand("GetContacts",conn)){
					cmd.Parameters.Add( "@UserName" ,SqlDbType.NVarChar).Value = Session["Name"];
					cmd.CommandType = CommandType.StoredProcedure;

					using(SqlDataAdapter sqlCmd = new SqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
				}
			}
		
		break;
		
		case "pgsql":
			using(NpgsqlConnection con = new NpgsqlConnection(Application["connStr"].ToString())){
				con.Open();
				
				string cmdText = "select * from lspr_GetContacts('" + Session["Name"] + "')";
				using(NpgsqlCommand cmd = new NpgsqlCommand(cmdText,con)){
					
					using(NpgsqlDataAdapter sqlCmd = new NpgsqlDataAdapter(cmd)){
						sqlCmd.Fill(ds);
					}
					
					// REMOVE ME - pgsql client doesn't handle empty tables ok
					if(ds.Tables.Count == 0){
						ds.Tables.Add("dummy");
					}
				}
			}

		break;
		
		case "xml":
			ds.ReadXml(Request.PhysicalApplicationPath + "data\\" + Session["Name"] + ".xml");
		
			ds.Tables.Remove("Settings");
			
		break;		
	}

	return ds;
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

function Mark_ButtonClick()
{
	for(var i = 0; i < form1.elements.length; i++){
		if(form1.elements[i].type == "checkbox"){
			form1.elements[i].checked = true
		}
	}
}

function UnMark_ButtonClick()
{
	for(var i = 0; i < form1.elements.length; i++){
		if(form1.elements[i].type == "checkbox"){
			form1.elements[i].checked = false
		}
	}
}

function Delete_ButtonClick()
{
	var value = "";
	var count = 0;
	for(var i = 0; i < form1.elements.length; i++){
		if(form1.elements[i].type == "checkbox"){
			if(form1.elements[i].checked){
				if(value == ""){
					value = form1.elements[i].value;
				}
				else{
					value += ";" + form1.elements[i].value;
				}
				count = count + 1;
			}
		}
	}

	if(count == 0){
		alert("<% Response.Write(m_WTxt["69"]); %>!"); // Ühtegi kontakti pole märgistatud
		return;
	}

	if(confirm("<% Response.Write(m_WTxt["70"]); %>?")){ // Kas Te olete kindel, et tahate märgistatud kontaktid kustutada
		document.form1.h1.value = "delete"
		document.form1.h2.value = count;
		document.form1.h3.value = value;
		document.form1.submit();
	}
}

function AddNew_ButtonClick()
{
	window.location.href= "contacts_add.aspx";
}

function ReadContact_ButtonClick(contactID)
{
	window.location.href= "contacts_read.aspx?ContactID=" + contactID;
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
          <td width="10" background="images/p_left_ud.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_up.gif" align="center">
          <a href="contacts.aspx"><b><font color="#000000"><% Response.Write(m_WTxt["4"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_right_ud.gif" align="center">&nbsp;</td>
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
      <input type="hidden" name="h2" value="">
      <input type="hidden" name="h3" value="">
        <div align="center">
          <center>
          <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="710" id="AutoNumber8">
            <tr>
              <td width="710" align="center">
              <p align="left">
              <input type="button" value="<% Response.Write(m_WTxt["10"]); %>" onClick="javascript:Mark_ButtonClick()" name="mark" style="width: 80; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["11"]); %>" onClick="javascript:UnMark_ButtonClick()" name="unmark" style="width: 160; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["31"]); %>" onClick="javascript:Delete_ButtonClick()" name="delete" style="width: 280; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["67"]); %>" onClick="javascript:AddNew_ButtonClick()" name="add" style="width: 140; border: 1px solid #808080"></td>
            </tr>
          </table>
          </center>
        </div>
        <p>&nbsp;</p>
        <div align="center">
          <center>
          <table border="1" cellspacing="0" style="border-collapse: collapse" bordercolor="#F2F2F2" width="730" id="AutoNumber7" cellpadding="0" bgcolor="#FFFFFF">
            <tr>
              <td width="30" height="25" align="center" bgcolor="#C0C0C0">
              &nbsp;</td>
              <td width="200" height="25" align="center" bgcolor="#C0C0C0">
              <font color="#FFFFFF"><b><% Response.Write(m_WTxt["32"]); %></b></font></td>
              <td width="280" height="25" align="center" bgcolor="#C0C0C0">
              <font color="#FFFFFF"><b><% Response.Write(m_WTxt["33"]); %></b></font></td>
              <td width="220" height="25" align="center" bgcolor="#C0C0C0">
              <font color="#FFFFFF"><b><% Response.Write(m_WTxt["34"]); %></b></font></td>
            </tr>
              <%
              DataSet ds = GetData();
              
              foreach(System.Data.DataRow dr in ds.Tables[0].Rows)
              {
 	             string aa = "";
 	             
            	 aa += "<tr>";
              	 aa += "<td width=\"30\" align=\"center\"><p style=\"margin-left: 2; margin-right: 2\"><input type=\"checkbox\" name=\"C2\" value=\"" + dr["ContactID"].ToString() + "\"></td>";
              	 aa += "<td width=\"200\"><p style=\"margin-left: 2; margin-right: 2\"><a href=\"javascript:ReadContact_ButtonClick('" + dr["ContactID"].ToString() + "')\">" + dr["ForName"].ToString() + " " + dr["SurName"].ToString() + "</a></td>";
              	 aa += "<td width=\"280\"><p style=\"margin-left: 2; margin-right: 2\">" + dr["Email"].ToString() + "</td>";

              	 if(dr["Phone1"].ToString().Length < 1 && dr["Phone2"].ToString().Length < 1){
	              	 aa += "<td width=\"220\"><p style=\"margin-left: 2; margin-right: 2\"></td>";
              	 }
              	 else if(dr["Phone1"].ToString().Length < 1){
	              	 aa += "<td width=\"220\"><p style=\"margin-left: 2; margin-right: 2\">" + dr["Phone2"].ToString() + "</td>";
              	 }
              	 else if(dr["Phone2"].ToString().Length < 1){
	              	 aa += "<td width=\"220\"><p style=\"margin-left: 2; margin-right: 2\">" + dr["Phone1"].ToString() + "</td>";
              	 }
              	 else{
	              	 aa += "<td width=\"220\"><p style=\"margin-left: 2; margin-right: 2\">" + dr["Phone1"].ToString() + "; " + dr["Phone2"].ToString() + "</td>";
              	 }

            	 aa += "</tr>";
	             
    	         Response.Write(aa);
              }

              if(ds.Tables[0].Rows.Count == 0){
 	            string aa = "";

            	aa += "<tr>";
              	aa += "<td width=\"730\" align=\"center\" colspan=\"5\" height=\"21\">-- " + m_WTxt["68"] + " --</td>";
            	aa += "</tr>";

    	        Response.Write(aa);
              }
              %>
          </table>
          </center>
        </div>
        <p>&nbsp;</p>
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