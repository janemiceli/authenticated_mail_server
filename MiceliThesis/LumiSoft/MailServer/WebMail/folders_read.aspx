<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="LumiSoft.Net.IMAP.Client" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private DataSet ds          = null;
private WText   m_WTxt      = null;
private string  m_ErrorText = "";

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

		// Delete Folder
		if(Request.Params["h1"] == "delete"){
			clnt.DeleteFolder(Request.Params["Folder"].ToString());
			clnt.Disconnect();
			Response.Redirect("folders.aspx");
			return;
		}
				
		// Delete messages in Folder
		if(Request.Params["h1"] == "empty"){
			clnt.SelectFolder(Request.Params["Folder"].ToString());

            if(Session["DeleteFolder"].ToString().ToLower() == Request.Params["Folder"].ToString().ToLower() || Session["DeleteFolder"] == ""){				    
				clnt.DeleteMessages(1, -1, false);
			}
			else{
				clnt.MoveMessages(1,-1,Session["DeleteFolder"].ToString(),false);
			}
		}

		clnt.SelectFolder(Request.Params["Folder"].ToString());

		ds = new DataSet();
		DataTable dt = ds.Tables.Add("Folder");
		dt.Columns.Add("Name",typeof(string));
		dt.Columns.Add("Messages",typeof(int));
		dt.Columns.Add("New",typeof(int));
		dt.Columns.Add("Size",typeof(int));
	
		DataRow dr = dt.NewRow();
		dr["Name"]     = Request.Params["Folder"].ToString();
		dr["Messages"] = clnt.MessagesCount;
		dr["New"]      = clnt.GetUnseenMessagesCount();
		dr["Size"]     = clnt.GetMessagesTotalSize();
		dt.Rows.Add(dr);
	}
	catch(Exception x){
		m_ErrorText = x.Message;
	}

	clnt.Disconnect();
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

function Open_ButtonClick()
{
	window.location.href= "inbox.aspx?Folder=<% Response.Write(Request.Params["Folder"].ToString()); %>";
}

function Edit_ButtonClick()
{
	window.location.href= "folders_add.aspx?Folder=<% Response.Write(Request.Params["Folder"].ToString()); %>";
}

function Delete_ButtonClick()
{
	if(confirm("<% Response.Write(m_WTxt["62"]); %>?")){ // Kas Te olete kindel, et tahate seda kausta kustutada
		document.form1.h1.value = "delete";
		document.form1.submit();
	}
}

function Empty_ButtonClick()
{
	if(confirm("<% Response.Write(m_WTxt["63"]); %>?")){ // Kas Te olete kindel, et tahate selle kausta kirjad kustutada
		document.form1.h1.value = "empty";
		document.form1.submit();
	}
}

function Close_ButtonClick()
{
	window.location.href= "folders.aspx";
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
          <a href="folders.aspx"><b><font color="#000000"><% Response.Write(m_WTxt["3"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
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
      <form method="POST" style="margin-top: 0; margin-bottom: 0" name="form1">
      <input type="hidden" name="h1" value="">
      <% if(m_ErrorText.Length == 0){ %>
        <div align="center">
          <center>
          <table border="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="730" id="AutoNumber9" cellpadding="2">
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["41"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(ds.Tables[0].Rows[0]["Name"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["42"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(ds.Tables[0].Rows[0]["Messages"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["43"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(ds.Tables[0].Rows[0]["New"].ToString()); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="163" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["16"]); %>: </b></td>
              <td width="506" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(Convert.ToDecimal(Convert.ToDecimal(ds.Tables[0].Rows[0]["Size"]) / 1000).ToString("f1")); %>k</td>
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
              <input type="button" value="<% Response.Write(m_WTxt["45"]); %>" onClick="javascript:Open_ButtonClick('')" name="open" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <% if(ds.Tables[0].Rows[0]["Name"].ToString() != "Inbox"){ %>
              <input type="button" value="<% Response.Write(m_WTxt["61"]); %>" onClick="javascript:Edit_ButtonClick('')" name="edit" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["39"]); %>" onClick="javascript:Delete_ButtonClick()" name="delete" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <% } %>
              <input type="button" value="<% Response.Write(m_WTxt["46"]); %>" onClick="javascript:Empty_ButtonClick('')" name="empty" style="width: 100; border: 1px solid #808080"></td>
              <td width="68">
              <p align="right">
              <input type="button" value="<% Response.Write(m_WTxt["40"]); %>" onClick="javascript:Close_ButtonClick()" name="close" style="width: 100; border: 1px solid #808080"></td>
            </tr>
          </table>
          </center>
        </div>
      <% } else{ %>
        <p align="center">&nbsp;</p>
        <p align="center"><font color="#FF0000"><% Response.Write(m_ErrorText); %></font></p>
        <p align="center">&nbsp;</p>
      <% } %>
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