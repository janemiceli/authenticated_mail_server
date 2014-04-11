<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.IO" %>
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
<meta http-equiv="Content-Language" content="et">
<meta name="GENERATOR" content="Microsoft FrontPage 5.0">
<meta name="ProgId" content="FrontPage.Editor.Document">
<meta http-equiv="Content-Type" content="text/html; charset=windows-1257">
<title>LS Mail - Kontaktid</title>
<link rel="stylesheet" type="text/css" href="style.css">
<script language="javascript">
<!--

function WindowLoad()
{
	var mail_to  = window.opener.document.form1.mail_to.value;
	var mail_cc  = window.opener.document.form1.mail_cc.value;
	var mail_bcc = window.opener.document.form1.mail_bcc.value;

	var to_split  = mail_to.split(";");
	var cc_split  = mail_cc.split(";");
	var bcc_split = mail_bcc.split(";");

	if(mail_to != ""){
		for(var i = 0; i < form1.elements.length; i++){
			if(form1.elements[i].type == "checkbox"){
				if(form1.elements[i].name == "TO"){
					for(var j in to_split){
						if(to_split[j] == form1.elements[i].value){
							form1.elements[i].checked = true;
							to_split.splice(j, 1);
						}
					}
				}
				if(form1.elements[i].name == "CC"){
					for(var j in cc_split){
						if(cc_split[j] == form1.elements[i].value){
							form1.elements[i].checked = true;
							cc_split.splice(j, 1);
						}
					}
				}
				if(form1.elements[i].name == "BCC"){
					for(var j in bcc_split){
						if(bcc_split[j] == form1.elements[i].value){
							form1.elements[i].checked = true;
							bcc_split.splice(j, 1);
						}
					}
				}
			}
		}
		
		form1.h1.value = to_split
		form1.h2.value = cc_split
		form1.h3.value = bcc_split
	}
}

function OK_ButtonClick()
{
	var value_to  = form1.h1.value;
	var value_cc  = form1.h2.value;
	var value_bcc = form1.h3.value;

	for(var i = 0; i < form1.elements.length; i++){
		if(form1.elements[i].type == "checkbox"){
			if(form1.elements[i].checked){
				if(form1.elements[i].name == "TO"){
					if(value_to == ""){
						value_to = form1.elements[i].value;
					}
					else{
						value_to += ";" + form1.elements[i].value;
					}
				}
				if(form1.elements[i].name == "CC"){
					if(value_cc == ""){
						value_cc = form1.elements[i].value;
					}
					else{
						value_cc += ";" + form1.elements[i].value;
					}
				}
				if(form1.elements[i].name == "BCC"){
					if(value_bcc == ""){
						value_bcc = form1.elements[i].value;
					}
					else{
						value_bcc += ";" + form1.elements[i].value;
					}
				}
			}
		}
	}

	window.opener.document.form1.mail_to.value = value_to;
	window.opener.document.form1.mail_cc.value = value_cc;
	window.opener.document.form1.mail_bcc.value = value_bcc;

	window.close();
}

//-->
</script>
</head>

<body topmargin="2" leftmargin="2" onLoad="javascript:WindowLoad()">

<table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber3" height="100%">
  <tr>
    <td width="100%" valign="top">
    <p align="center">&nbsp;</p>
    <p align="center"><u><b>!!!</b> <% Response.Write(m_WTxt["75"]); %> <b>!!!</b></u></p>
    <p align="center">&nbsp;</p>
    <table border="1" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#808080" width="100%" id="AutoNumber1" bgcolor="#F2F2F2">
      <tr>
        <td width="100%">&nbsp;
        <form method="POST" style="margin-top: 0; margin-bottom: 0" name="form1">
        <input type="hidden" name="h1" value="">
        <input type="hidden" name="h2" value="">
        <input type="hidden" name="h3" value="">
          <div align="center">
            <center>
            <table border="1" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#F2F2F2" width="600" id="AutoNumber2" bgcolor="#FFFFFF">
              <tr>
                <td width="55" align="center" height="30" bgcolor="#C0C0C0" bordercolor="#F2F2F2">
                <font color="#FFFFFF"><b><% Response.Write(m_WTxt["18"]); %></b></font></td>
                <td width="55" align="center" height="30" bgcolor="#C0C0C0" bordercolor="#F2F2F2">
                <font color="#FFFFFF"><b><% Response.Write(m_WTxt["19"]); %></b></font></td>
                <td width="55" align="center" height="30" bgcolor="#C0C0C0" bordercolor="#F2F2F2">
                <font color="#FFFFFF"><b><% Response.Write(m_WTxt["20"]); %></b></font></td>
                <td width="200" align="center" height="30" bgcolor="#C0C0C0" bordercolor="#F2F2F2">
                <font color="#FFFFFF"><b><% Response.Write(m_WTxt["32"]); %></b></font></td>
                <td width="235" align="center" height="30" bgcolor="#C0C0C0" bordercolor="#F2F2F2">
                <font color="#FFFFFF"><b><% Response.Write(m_WTxt["33"]); %></b></font></td>
              </tr>
              <%
              DataSet ds = GetData();
              
              foreach(System.Data.DataRow dr in ds.Tables[0].Rows)
              {
 	             string aa = "";

              	 aa += "<tr>";
                 aa += "<td width=\"55\" align=\"center\"><p style=\"margin-left: 2; margin-right: 2\">";
                 aa += "<input type=\"checkbox\" name=\"TO\" value=\"" + dr["Email"].ToString() + "\"></td>";
                 aa += "<td width=\"55\" align=\"center\"><p style=\"margin-left: 2; margin-right: 2\">";
                 aa += "<input type=\"checkbox\" name=\"CC\" value=\"" + dr["Email"].ToString() + "\"></td>";
                 aa += "<td width=\"55\" align=\"center\"><p style=\"margin-left: 2; margin-right: 2\">";
                 aa += "<input type=\"checkbox\" name=\"BCC\" value=\"" + dr["Email"].ToString() + "\"></td>";
                 aa += "<td width=\"200\"><p style=\"margin-left: 2; margin-right: 2\">" + dr["ForName"].ToString() + " " + dr["SurName"].ToString() + "</td>";
                 aa += "<td width=\"235\"><p style=\"margin-left: 2; margin-right: 2\">" + dr["Email"].ToString() + "</td>";
              	 aa += "</tr>";
	             
    	         Response.Write(aa);
              }

              if(ds.Tables[0].Rows.Count == 0){
 	            string aa = "";

            	aa += "<tr>";
              	aa += "<td width=\"600\" align=\"center\" colspan=\"5\" height=\"21\">-- " + m_WTxt["68"] + " --</td>";
            	aa += "</tr>";

    	        Response.Write(aa);
              }
              %>
            </table>
            <p>&nbsp;</p>
            <p>
            <input type="button" value="OK" onClick="javascript:OK_ButtonClick()" name="B1" style="width: 100; border: 1px solid #808080"></p>
            </center>
          </div>
        </form>
        <p>&nbsp;</td>
      </tr>
    </table>
    </td>
  </tr>
  <tr>
    <td width="100%" valign="bottom">
    <p align="center">&nbsp;</p>
    <p align="center"><a href="javascript:window.close()"><b>
    <font color="#000000"><% Response.Write(m_WTxt["76"].ToUpper()); %></font></b></a></p>
    <p align="center">&nbsp;</td>
  </tr>
</table>

</body>

</html>