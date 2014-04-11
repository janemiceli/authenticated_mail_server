<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="LumiSoft.Net.IMAP.Client" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private DataSet  ds          = null;
private string[] folders     = null;
private WText    m_WTxt      = null;
private string   m_ErrorText = "";

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);

	if(Session["Name"] == null || Session["Name"].ToString().Length < 1){
		Response.Redirect("login.aspx");
	}

	m_WTxt = new WText(Request.PhysicalApplicationPath + "bin\\",Application["DefaultLanguage"].ToString());
    IMAP_Client clnt = new IMAP_Client();

	try
	{       		
		clnt.Connect(Application["IMAPServerName"].ToString(),Convert.ToInt32(Application["IMAPServerPort"]));
		clnt.Authenticate(Session["Name"].ToString(),Session["Password"].ToString());

		folders = clnt.GetFolders();

		// Delete Messages in Folders
		if(Request.Params["h1"] == "empty"){
			string[] text = Request.Params["h3"].ToString().Split(';');
	
			foreach(string ss in text){
            	clnt.SelectFolder(ss);

                if(Session["DeleteFolder"].ToString().ToLower() == ss.ToString().ToLower() || Session["DeleteFolder"] == ""){				    
					clnt.DeleteMessages(1, -1, false);
				}
				else{
				    clnt.MoveMessages(1,-1,Session["DeleteFolder"].ToString(),false);
				}
			}
			
			Response.Redirect("folders.aspx");
		}

		ds = new DataSet();
		DataTable dt = ds.Tables.Add("FolderList");
		dt.Columns.Add("Name",typeof(string));
		dt.Columns.Add("Messages",typeof(int));
		dt.Columns.Add("New",typeof(int));
		dt.Columns.Add("Size",typeof(int));

		foreach(string folder in folders){
			clnt.SelectFolder(folder);
	
			DataRow dr = dt.NewRow();
			dr["Name"]     = folder;
			dr["Messages"] = clnt.MessagesCount;
			dr["New"]      = clnt.GetUnseenMessagesCount();
			dr["Size"]     = clnt.GetMessagesTotalSize();
			dt.Rows.Add(dr);
		}
	}
	catch(Exception x){
		m_ErrorText = x.Message;
	}

    clnt.Disconnect();
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

function ReadFolder_ButtonClick(folder)
{
	window.location.href= "folders_read.aspx?Folder=" + folder;
}

function Empty_ButtonClick()
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
		alert("<% Response.Write(m_WTxt["59"]); %>!"); // Ühtegi kausta pole märgistatud
		return;
	}

	if(confirm("<% Response.Write(m_WTxt["60"]); %>?")){ // Kas Te olete kindel, et tahate märgistatud kaustad tühjendada
		document.form1.h1.value = "empty"
		document.form1.h2.value = count;
		document.form1.h3.value = value;
		document.form1.submit();
	}
}

function Add_ButtonClick()
{
	window.location.href= "folders_add.aspx";
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
          <td width="10" background="images/p_left_ud.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_up.gif" align="center">
          <a href="folders.aspx"><b><font color="#000000"><% Response.Write(m_WTxt["3"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_right_ud.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="contacts.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["4"].ToUpper()); %></font></b></a></td>
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
      <form method="POST" style="margin-top: 0; margin-bottom: 0" enctype="multipart/form-data" name="form1">
      <input type="hidden" name="h1" value="">
      <input type="hidden" name="h2" value="">
      <input type="hidden" name="h3" value="">
      <% if(m_ErrorText.Length == 0){ %>
        <div align="center">
          <center>
          <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="710" id="AutoNumber8">
            <tr>
              <td width="710" align="center">
              <p align="left">
              <input type="button" value="<% Response.Write(m_WTxt["10"]); %>" onClick="javascript:Mark_ButtonClick()" name="mark" style="width: 80; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["11"]); %>" onClick="javascript:UnMark_ButtonClick()" name="unmark" style="width: 160; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["28"]); %>" onClick="javascript:Empty_ButtonClick()" name="empty" style="width: 280; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["29"]); %>" onClick="javascript:Add_ButtonClick()" name="add" style="width: 140; border: 1px solid #808080"></td>
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
              <td width="400" height="25" align="center" bgcolor="#C0C0C0">
              <font color="#FFFFFF"><b><% Response.Write(m_WTxt["41"]); %></b></font></td>
              <td width="100" height="25" align="center" bgcolor="#C0C0C0">
              <font color="#FFFFFF"><b><% Response.Write(m_WTxt["42"]); %></b></font></td>
              <td width="100" height="25" align="center" bgcolor="#C0C0C0">
              <font color="#FFFFFF"><b><% Response.Write(m_WTxt["43"]); %></b></font></td>
              <td width="100" height="25" align="center" bgcolor="#C0C0C0"><b>
              <font color="#FFFFFF"><% Response.Write(m_WTxt["16"]); %></font></b></td>
            </tr>
              <%
              DataView dv = ds.Tables[0].DefaultView;
              dv.Sort = "Name ASC";
              
              int total_msg    = 0;
              int total_newmsg = 0;
              decimal total_size   = 0;
             
              foreach(System.Data.DataRowView drv in dv)
              {
              	System.Data.DataRow dr = drv.Row;

 	            string aa = "";

	            aa += "<tr>";
    	        aa += "<td width=\"30\" align=\"center\">";
              	aa += "<p style=\"margin-left: 2; margin-right: 2\">";
              	aa += "<input type=\"checkbox\" name=\"C5\" value=\"" + dr["Name"].ToString() + "\"></td>";
              	aa += "<td width=\"400\">";
              	aa += "<p style=\"margin-left: 2; margin-right: 2\">";
              	aa += "<a href=\"javascript:ReadFolder_ButtonClick('" + dr["Name"].ToString() + "')\">" + dr["Name"].ToString() + "</a></td>";
              	aa += "<td width=\"100\" align=\"right\">";
              	aa += "<p style=\"margin-left: 2; margin-right: 2\">" + dr["Messages"].ToString() + "</td>";
              	aa += "<td width=\"100\" align=\"right\">";
              	aa += "<p style=\"margin-left: 2; margin-right: 2\">" + dr["New"].ToString() + "</td>";
              	aa += "<td width=\"100\" align=\"right\">";
              	aa += "<p style=\"margin-left: 2; margin-right: 2\">" + Convert.ToDecimal(Convert.ToDecimal(dr["Size"]) / 1000).ToString("f1") + "k</td>";
            	aa += "</tr>";

				total_msg    = total_msg + Convert.ToInt32(dr["Messages"]);
				total_newmsg = total_newmsg + Convert.ToInt32(dr["New"]);
				total_size   = total_size + Convert.ToDecimal(Convert.ToDecimal(dr["Size"]) / 1000);

    	        Response.Write(aa);
              }
             
 	          string bb = "";

	          bb += "<tr>";
              bb += "<td width=\"30\" align=\"center\" height=\"20\" bgcolor=\"#DADADA\">";
              bb += "<p style=\"margin: 0 2\">&nbsp;</td>";
              bb += "<td width=\"400\" height=\"20\" bgcolor=\"#DADADA\">";
              bb += "<p style=\"margin: 0 2\"><b>" + m_WTxt["30"].ToUpper() + "</b></td>";
              bb += "<td width=\"100\" align=\"right\" height=\"20\" bgcolor=\"#DADADA\">";
              bb += "<p style=\"margin: 0 2\"><b>" + total_msg + "</b></td>";
              bb += "<td width=\"100\" align=\"right\" height=\"20\" bgcolor=\"#DADADA\">";
              bb += "<p style=\"margin: 0 2\"><b>" + total_newmsg + "</b></td>";
              bb += "<td width=\"100\" align=\"right\" height=\"20\" bgcolor=\"#DADADA\">";
              bb += "<p style=\"margin: 0 2\"><b>" + total_size.ToString("f1") + "k</b></td>";
              bb += "</tr>";

    	      Response.Write(bb);
              %>
          </table>
          </center>
        </div>
        <p align="center">
        &nbsp;</p>
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