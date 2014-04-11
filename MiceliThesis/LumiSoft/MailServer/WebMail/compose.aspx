<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="LumiSoft.Net.IMAP.Client" %>
<%@ Import Namespace="LumiSoft.Net.Mime" %>
<%@ Import Namespace="LumiSoft.Wisk.Text" %>
<%@ Import Namespace="LumiSoft.Net.SMTP.Client" %>

<script runat="server">
private MimeParser p                 = null;
private WText      m_WTxt            = null;
private string     m_ErrorText       = "";
private bool       m_IsLoadedMessage = false;

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
		// Add File
		if(Request.Params["h1"] == "addfile" && Request.Files.Count > 0){
			if(Session["Attachments"] == null){
				Session["Attachments"] = new Hashtable();
			}
		
			byte[] data = new byte[Request.Files[0].ContentLength];
			Request.Files[0].InputStream.Read(data,0,data.Length);
		
			Hashtable filelist = (Hashtable)Session["Attachments"];
		
			if(!filelist.Contains(Path.GetFileName(Request.Files[0].FileName))){
				filelist.Add(Path.GetFileName(Request.Files[0].FileName),data);
			}
		
			m_IsLoadedMessage = false;
			return;
		}

		// Remove File
		if(Request.Params["h1"] == "removefile" && Session["Attachments"] != null){
			Hashtable filelist = (Hashtable)Session["Attachments"];
		
			if(!filelist.Contains(Path.GetFileName(Request.Files[0].FileName))){
				filelist.Remove(Request.Params["h2"].ToString());
			}

			m_IsLoadedMessage = false;
			return;
		}

		// Send Mail
		if(Request.Params["h1"] == "send"){
			SendMail();
		}

		// Load Message
		if(Request.Params["MessageID"] != null){
			clnt.Connect(Application["IMAPServerName"].ToString(),Convert.ToInt32(Application["IMAPServerPort"]));
			clnt.Authenticate(Session["Name"].ToString(),Session["Password"].ToString());

			clnt.SelectFolder(Request.Params["Folder"].ToString());

			if(Request.Params["MessageID"] != null){
				IMAP_FetchItem[] f = clnt.FetchMessages(Convert.ToInt32(Request.Params["MessageID"]),Convert.ToInt32(Request.Params["MessageID"]),true,false,true);
		
				if(f.Length > 0){
					p = new MimeParser(f[0].Data);

					// Clear attachments
					Session["Attachments"] = null;
				
					if(Request.Params["Option"] == "forward"){
						Session["Attachments"] = new Hashtable();
						Hashtable filelist = (Hashtable)Session["Attachments"];

						foreach(LumiSoft.Net.Mime.MimeEntry entry in p.MimeEntries){
							if(entry.ContentDisposition == LumiSoft.Net.Mime.Disposition.Attachment){
								filelist.Add(entry.FileName,entry.Data);
							}
						}
					}
				
					m_IsLoadedMessage = true;
				}
				else{
					// Siis pole kirja olemas ja viskab viga
				}
			}
		}
	}
	catch(Exception x){
		m_ErrorText = x.Message;		
	}
	
	clnt.Disconnect();
}

private void SendMail()
{
	string[] to  = Request.Params["mail_to"].ToString().Split(';');
	string[] cc  = Request.Params["mail_cc"].ToString().Split(';');
	string[] bcc = Request.Params["mail_bcc"].ToString().Split(';');
	
	MimeConstructor m = new MimeConstructor();
	m.To      = to;
	m.Cc      = cc;
	m.Bcc     = bcc;
	m.From    = Session["userEmail"].ToString();
	m.Subject = Request.Params["mail_subject"].ToString();
	m.Body    = Request.Params["mail_text"].ToString();
	
	if(Session["Attachments"] != null){
		Hashtable filelist = (Hashtable)Session["Attachments"];
		
		foreach(string filename in filelist.Keys){
			m.Attachments.Add(new Attachment(filename,(byte[])filelist[filename]));
		}
	}

	SMTP_Client smtpClnt  = new SMTP_Client();
	smtpClnt.UseSmartHost = true;
	smtpClnt.SmartHost    = Application["SMTPServer"].ToString();

	MemoryStream msg = m.ConstructBinaryMime();
	bool noErrors    = smtpClnt.Send(to,Session["userEmail"].ToString(),msg);
	if(!noErrors){
		m_ErrorText = smtpClnt.Errors[0].ErrorText;
	}
	else{
    	if(Session["SentFolder"].ToString().Length > 0){
    		using(IMAP_Client clnt = new IMAP_Client()){

	       		clnt.Connect(Application["IMAPServerName"].ToString(),Convert.ToInt32(Application["IMAPServerPort"]));
		    	clnt.Authenticate(Session["Name"].ToString(),Session["Password"].ToString());

       			clnt.StoreMessage(Session["SentFolder"].ToString(),msg.ToArray());
       		}
   		}
		
		Session["Attachments"] = null;
		Response.Redirect("compose.aspx");
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

function WindowLoad()
{
	<% if(Request.Params["h1"] == null && Request.Params["MessageID"] == null){ Session["Attachments"] = null; } %>
}

function WindowUnload()
{
}

function Open_AddFromContacts()
{
	info = window.open("contacts_select.aspx","contacts","height=300,width=640,status=no,toolbar=no,menubar=no,location=no,scrollbars=yes");
	info.focus();
}

function AddFile()
{
	if(document.form1.upload.value.length == 0){
		alert("<% Response.Write(m_WTxt["56"]); %>!") // Vali fail, mida lisada nimekirja
	}
	else{
		document.form1.h1.value = "addfile";
		document.form1.submit();
	}
}

function RemoveFile()
{
	if(document.form1.filelist.length < 1){
		alert("<% Response.Write(m_WTxt["57"]); %>!") // Lisatud failide nimekiri on tühi
		return;
	}

	if(document.form1.filelist.selectedIndex != -1){
		document.form1.h1.value = "removefile";
		document.form1.h2.value = document.form1.filelist.options[document.form1.filelist.selectedIndex].text;
		document.form1.submit();
	}
	else{
		alert("<% Response.Write(m_WTxt["58"]); %>!") // Vali fail, mida eemaldada nimekirjast
	}
}

function SendMessage()
{
	if(document.form1.mail_to.value.length < 1){
		alert("<% Response.Write(m_WTxt["78"]); %>!"); // Sisesta kirja saaja aadress
	}
	else{
		document.form1.h1.value = "send";
		document.form1.submit();
	}
}

//-->
</script>
</head>

<body topmargin="0" onLoad="javascript:WindowLoad()" onUnload="javascript:WindowUnload()">

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
          <td width="10" background="images/p_left_ud.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_up.gif" align="center">
          <a href="compose.aspx"><b><font color="#000000"><% Response.Write(m_WTxt["2"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_right_ud.gif" align="center">&nbsp;</td>
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
      <form method="POST" style="margin-top: 0; margin-bottom: 0" enctype="multipart/form-data" name="form1">
      <input type="hidden" name="h1" value="">
      <input type="hidden" name="h2" value="">
      <% if(m_ErrorText.Length == 0){ %>
        <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber7">
          <tr>
            <td width="17%" align="right" height="22"><b><% Response.Write(m_WTxt["18"]); %>:&nbsp; </b> </td>
            <td width="60%" height="22">
            <input type="text" name="mail_to" size="69" style="border: 1px solid #808080" value="<% if(Request.Params["mail_to"] != null){ Response.Write(Request.Params["mail_to"]); } if(m_IsLoadedMessage){ if(Request.Params["Option"] == "replyall"){string str = ""; foreach(string t in p.To){str += t.Replace("<","").Replace(">","") + ";";} Response.Write(p.From + ";" + str);} if(Request.Params["Option"] == "reply"){Response.Write(p.From);}} %>"></td>
            <td width="73%" height="22">
            <a href="javascript:Open_AddFromContacts()">&lt; <% Response.Write(m_WTxt["27"]); %></a>
            </td>
          </tr>
          <tr>
            <td width="17%" align="right" height="22"><b><% Response.Write(m_WTxt["19"]); %>:&nbsp; </b> </td>
            <td width="60%" height="22">
            <input type="text" name="mail_cc" size="69" style="border: 1px solid #808080" value="<% if(Request.Params["mail_cc"] != null){ Response.Write(Request.Params["mail_cc"]); } %>"></td>
            <td width="73%" height="22">&nbsp;</td>
          </tr>
          <tr>
            <td width="17%" align="right" height="22"><b><% Response.Write(m_WTxt["20"]); %>:&nbsp; </b> </td>
            <td width="60%" height="22">
            <input type="text" name="mail_bcc" size="69" style="border: 1px solid #808080" value="<% if(Request.Params["mail_bcc"] != null){ Response.Write(Request.Params["mail_bcc"]); } %>"></td>
            <td width="73%" height="22">&nbsp;</td>
          </tr>
        </table>
        <p>&nbsp;</p>
        <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber8">
          <tr>
            <td width="12%">
            <p align="right"><b><% Response.Write(m_WTxt["14"]); %>:&nbsp; </b> </td>
            <td width="88%">
            <input type="text" name="mail_subject" size="101" style="border: 1px solid #808080" value="<% if(Request.Params["mail_subject"] != null){ Response.Write(Request.Params["mail_subject"]); } if(m_IsLoadedMessage){ if(Request.Params["Option"] == "reply" || Request.Params["Option"] == "replyall"){Response.Write("Re: " + p.Subject);} if(Request.Params["Option"] == "forward"){Response.Write("Fwd: " + p.Subject);}} %>"></td>
          </tr>
        </table>
        <p align="center">&nbsp;</p>
        <p align="center">
        <textarea rows="15" name="mail_text" cols="116" style="border: 1px solid #808080">
<%
if(Request.Params["mail_text"] != null){
	Response.Write(Request.Params["mail_text"]);
}

if(m_IsLoadedMessage){
	if(Request.Params["Option"] == "reply" || Request.Params["Option"] == "replyall"){
		string[] bodyTextLines = p.BodyText.Replace("\r","").Split('\n');
		
		string to = ""; foreach(string t in p.To){to += t.Replace("<","").Replace(">","") + ";";}
		string cc = ""; foreach(string t in p.Cc){cc += t.Replace("<","").Replace(">","") + ";";}
		
		string aa = "";
		aa += "\n\n>>\n";
		aa += ">>--- Original Message ---\n";
		aa += ">>From: " + p.From + "\n";
		aa += ">>Date: " + Convert.ToDateTime(p.MessageDate).ToString("dd-MM-yyyy HH:mm") + "\n";
		aa += ">>To: " + to + "\n";
		
		if(p.Cc[0].Length > 1){
			aa += ">>Cc: " + cc + "\n";
		}

		aa += ">>Subject: " + p.Subject + "\n";
		aa += ">>\n";
		foreach(string line in bodyTextLines){
			aa += ">>" + line + "\n";
		}
		aa += ">>";

		Response.Write(aa);
	}
	if(Request.Params["Option"] == "forward"){
		string[] bodyTextLines = p.BodyText.Replace("\r","").Split('\n');
		
		string to = ""; foreach(string t in p.To){to += t.Replace("<","").Replace(">","") + ";";}
		string cc = ""; foreach(string t in p.Cc){cc += t.Replace("<","").Replace(">","") + ";";}
		
		string aa = "";
		aa += "\n\n";
		aa += "--- Original Message ---\n";
		aa += "From: " + p.From + "\n";
		aa += "Date: " + Convert.ToDateTime(p.MessageDate).ToString("dd-MM-yyyy HH:mm") + "\n";
		aa += "To: " + to + "\n";
		
		if(p.Cc[0].Length > 1){
			aa += "Cc: " + cc + "\n";
		}

		aa += "Subject: " + p.Subject + "\n";
		aa += "\n";
		foreach(string line in bodyTextLines){
			aa += "" + line + "\n";
		}
		aa += "";

		Response.Write(aa);
	}
}
%></textarea></p>
        <p align="center">&nbsp;</p>
        <div align="center">
          <center>
          <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="673" id="AutoNumber10">
            <tr>
              <td width="287"><hr color="#808080" size="1"></td>
              <td width="100">
              <p align="center"><% Response.Write(m_WTxt["21"]); %></td>
              <td width="286"><hr color="#808080" size="1"></td>
            </tr>
          </table>
          </center>
        </div>
        <table border="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="100%" id="AutoNumber9" cellpadding="2">
          <tr>
            <td width="20%" align="right"><b><% Response.Write(m_WTxt["22"]); %>: </b></td>
            <td width="61%">
            <input type="file" name="upload" size="28" style="width: 430; border: 1px solid #808080"></td>
            <td width="19%">
            <input type="button" value="<% Response.Write(m_WTxt["24"]); %>" name="addfile" onClick="AddFile()" style="width: 80; border: 1px solid #808080"></td>
          </tr>
          <tr>
            <td width="20%" align="right" valign="top">
            <p style="margin-top: 3"><b><% Response.Write(m_WTxt["23"]); %>: </b></td>
            <td width="61%"><select size="4" name="filelist" style="width: 430">
			<%
			if(Session["Attachments"] != null){
				Hashtable filelist = (Hashtable)Session["Attachments"];
		
				foreach(string filename in filelist.Keys){
					Response.Write("<option>" + filename + "</option>");
				}
			}
			%>
            </select></td>
            <td width="19%" valign="bottom">
            <input type="button" value="<% Response.Write(m_WTxt["25"]); %>" name="removefile" onClick="RemoveFile()" style="width: 80; border: 1px solid #808080"></td>
          </tr>
        </table>
         <hr color="#808080" width="90%" size="1">
        <p>&nbsp;</p>
        <p align="center">
        <input type="button" value="<% Response.Write(m_WTxt["26"]); %>" name="send" onClick="SendMessage()" style="width: 100; border: 1px solid #808080"></p>
      </form>
      <% } else{ %>
        <p align="center">&nbsp;</p>
        <p align="center"><font color="#FF0000"><% Response.Write(m_ErrorText); %></font></p>
        <p align="center">&nbsp;</p>
      <% } %>
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