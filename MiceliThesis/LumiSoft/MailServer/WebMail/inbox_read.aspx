<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="LumiSoft.Net.IMAP.Client" %>
<%@ Import Namespace="LumiSoft.Net.Mime" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>

<script runat="server">

private MimeParser p           = null;
private WText      m_WTxt      = null;
private string     m_ErrorText = "";

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

		clnt.SelectFolder(Request.Params["Folder"].ToString());

		// Delete Message
		if(Request.Params["h1"] == "delete"){
			if(Session["DeleteFolder"] == ""){
				clnt.DeleteMessages(Convert.ToInt32(Request.Params["MessageID"]),Convert.ToInt32(Request.Params["MessageID"]),true);
			}
			else{
				clnt.MoveMessages(Convert.ToInt32(Request.Params["MessageID"]),Convert.ToInt32(Request.Params["MessageID"]),Session["DeleteFolder"].ToString(),true);
			}
			clnt.Disconnect();

			Response.Redirect("inbox.aspx?Folder=Inbox");
			return;
		}

		if(Request.Params["MessageID"] != null){
			IMAP_FetchItem[] f = clnt.FetchMessages(Convert.ToInt32(Request.Params["MessageID"]),Convert.ToInt32(Request.Params["MessageID"]),true,false,true);
		
			if(f.Length > 0){
				p = new MimeParser(f[0].Data);
			}
			else{
				// Siis pole kirja olemas ja viskab viga
			}
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
<meta http-equiv="Content-Language" content="et">
<meta name="GENERATOR" content="Microsoft FrontPage 5.0">
<meta name="ProgId" content="FrontPage.Editor.Document">
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252">
<title>LS Mail</title>
<link rel="stylesheet" type="text/css" href="style.css">
<script language="javascript">
<!--

function Reply_ButtonClick()
{
	window.location.href= "compose.aspx?MessageID=<% Response.Write(Request.Params["MessageID"]); %>&Folder=<% Response.Write(Request.Params["Folder"]); %>&Option=reply";
}

function ReplyAll_ButtonClick()
{
	window.location.href= "compose.aspx?MessageID=<% Response.Write(Request.Params["MessageID"]); %>&Folder=<% Response.Write(Request.Params["Folder"]); %>&Option=replyall";
}

function Forward_ButtonClick()
{
	window.location.href= "compose.aspx?MessageID=<% Response.Write(Request.Params["MessageID"]); %>&Folder=<% Response.Write(Request.Params["Folder"]); %>&Option=forward";
}

function Delete_ButtonClick()
{
	if(confirm("<% Response.Write(m_WTxt["55"]); %>?")){ // Kas Te olete kindel, et tahate seda kirja kustutada
		document.form1.h1.value = "delete";
		document.form1.submit();
	}
}

function Close_ButtonClick()
{
	window.location.href= "inbox.aspx?Folder=<% Response.Write(Request.Params["Folder"]); %>";
}

function OpenAttachment_ButtonClick(file_name)
{
	window.location.href = "attachment.aspx?File=" + file_name + "&Folder=<% Response.Write(Request.Params["Folder"]); %>&MessageID=<% Response.Write(Request.Params["MessageID"]); %>";
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
          <a href="inbox.aspx?Folder=Inbox"><b><font color="#000000"><% Response.Write(m_WTxt["1"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="compose.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["2"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_middle_d.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_down.gif" align="center">
          <a href="folders.aspx"><b><font color="#808080"><% Response.Write(m_WTxt["3"].ToUpper()); %></font></b></a></td>
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
              <td width="129" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["13"]); %>: </b></td>
              <td width="540" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(p.From); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="129" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["18"]); %>: </b></td>
              <td width="540" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% string str = ""; foreach(string t in p.To){str += t.Replace("<","").Replace(">","") + ";";} Response.Write(str); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <% if(p.Cc[0].Length > 1){ %>
            <tr>
              <td width="129" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["19"]); %>: </b></td>
              <td width="540" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% string str2 = ""; foreach(string t in p.Cc){str2 += t.Replace("<","").Replace(">","") + ";";} Response.Write(str2); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <% } %>
            <tr>
              <td width="129" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["35"]); %>: </b></td>
              <td width="540" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(Convert.ToDateTime(p.MessageDate).ToString("dd-MM-yyyy HH:mm")); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <tr>
              <td width="129" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["14"]); %>: </b></td>
              <td width="540" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2"><% Response.Write(p.Subject); %></td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <%
            bool att = false;
            
            foreach(LumiSoft.Net.Mime.MimeEntry entry in p.MimeEntries){
            	if(entry.ContentDisposition == LumiSoft.Net.Mime.Disposition.Attachment){
            		att = true;
            	}
            }
            
            if(att){
            %>
            <tr>
              <td width="129" height="20" align="right">
              <p style="margin-left: 2; margin-right: 2"><b><% Response.Write(m_WTxt["21"]); %>: </b>
              </td>
              <td width="540" height="20" style="border-bottom: 1px solid #808080; padding: 0">
              <p style="margin-left: 2; margin-right: 2">
              <%
              	foreach(LumiSoft.Net.Mime.MimeEntry entry in p.MimeEntries){
					if(entry.ContentDisposition == LumiSoft.Net.Mime.Disposition.Attachment){
						Response.Write("<a href=\"javascript:OpenAttachment_ButtonClick('" + entry.FileName + "')\">" + entry.FileName + "</a> (" + Convert.ToDecimal(Convert.ToDecimal(entry.Data.Length) / 1000).ToString("f1") + "k); ");
					}
				}
              %>
              </td>
              <td width="61" height="20">
              <p style="margin-left: 2; margin-right: 2">&nbsp;</td>
            </tr>
            <% } %>
          </table>
          </center>
        </div>
        <p>&nbsp;</p>
        <div align="center">
          <center>
          <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="710" id="AutoNumber11">
            <tr>
              <td width="642">
              <input type="button" value="<% Response.Write(m_WTxt["36"]); %>" onClick="javascript:Reply_ButtonClick()" name="reply" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["37"]); %>" onClick="javascript:ReplyAll_ButtonClick()" name="replyall" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["38"]); %>" onClick="javascript:Forward_ButtonClick()" name="forward" style="width: 100; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["39"]); %>" onClick="javascript:Delete_ButtonClick()" name="delete" style="width: 100; border: 1px solid #808080"></td>
              <td width="68">
              <p align="right">
              <input type="button" value="<% Response.Write(m_WTxt["40"]); %>" onClick="javascript:Close_ButtonClick()" name="close" style="width: 100; border: 1px solid #808080"></td>
            </tr>
          </table>
          </center>
        </div>
        <p>&nbsp;</p>
        <div align="center">
          <center>
          <table border="1" cellpadding="2" cellspacing="0" style="border-collapse: collapse" bordercolor="#808080" width="730" id="AutoNumber8" bgcolor="#FFFFFF">
            <tr>
              <% 
              string[] bodyTextLines = p.BodyText.Replace("\r","").Split('\n');
              
              string aa = "";
              
              aa += "<td width=\"100%\">";
              		 
              foreach(string line in bodyTextLines){
              	if(line == ""){
              		aa += "<p>&nbsp;</p>";
              	}
              	else{
              		aa += "<p>" + line + "</p>";
              	}
              }
              		 
              aa += "&nbsp;</td>";
              
              Response.Write(aa);
              %>
            </tr>
            </table>
          </center>
        </div>
        <p>&nbsp;</p>
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