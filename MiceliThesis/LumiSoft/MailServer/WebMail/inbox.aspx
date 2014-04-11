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

		clnt.SelectFolder(Request.Params["Folder"]);

		// Move To Folder
		if(Request.Params["h1"] == "move"){
			string[] text = Request.Params["h3"].ToString().Split(';');
	
			foreach(string ss in text){
				clnt.MoveMessages(Convert.ToInt32(ss),Convert.ToInt32(ss),Request.Params["h4"].ToString(),true);
			}
		}

		// Delete Messages
		if(Request.Params["h1"] == "delete"){
			string[] text = Request.Params["h3"].ToString().Split(';');
	
			foreach(string ss in text){
				if(Session["DeleteFolder"].ToString().ToLower() == Request.Params["Folder"].ToString().ToLower() || Session["DeleteFolder"] == ""){
					clnt.DeleteMessages(Convert.ToInt32(ss),Convert.ToInt32(ss),true);
				}
				else{
					clnt.MoveMessages(Convert.ToInt32(ss),Convert.ToInt32(ss),Session["DeleteFolder"].ToString(),true);
				}				
			}
		}

		ds = new DataSet();
		DataTable dt = ds.Tables.Add("MsgList");
		dt.Columns.Add("Sender",typeof(string));
		dt.Columns.Add("Subject",typeof(string));
		dt.Columns.Add("Date",typeof(DateTime));
		dt.Columns.Add("Size",typeof(int));
		dt.Columns.Add("UID",typeof(int));
		dt.Columns.Add("IsNewMsg",typeof(bool));

		IMAP_FetchItem[] fetchItems = clnt.FetchMessages(1,-1,false,true,false);			
		foreach(IMAP_FetchItem fetchItem in fetchItems){
			LumiSoft.Net.Mime.MimeParser p = new LumiSoft.Net.Mime.MimeParser(fetchItem.Data);

			DataRow dr = dt.NewRow();
			dr["Sender"]   = p.From;
			dr["Subject"]  = p.Subject;
			dr["Date"]     = p.MessageDate;
			dr["Size"]     = fetchItem.Size;
			dr["UID"]      = fetchItem.UID;
			dr["IsNewMsg"] = fetchItem.IsNewMessage;
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
<meta http-equiv="Content-Language" content="et">
<meta name="GENERATOR" content="Microsoft FrontPage 5.0">
<meta name="ProgId" content="FrontPage.Editor.Document">
<meta http-equiv="Content-Type" content="text/html; charset=windows-1252">
<title>LS Mail</title>
<link rel="stylesheet" type="text/css" href="style.css">
<script language="javascript">
<!--

function WindowLoad()
{
}

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
		alert("<% Response.Write(m_WTxt["52"]); %>!"); // Ühtegi kirja pole märgistatud
		return;
	}

	if(confirm("<% Response.Write(m_WTxt["53"]); %>?")){ // Kas Te olete kindel, et tahate märgistatud kirjad kustutada
		document.form1.h1.value = "delete"
		document.form1.h2.value = count;
		document.form1.h3.value = value;
		document.form1.submit();
	}
}

function OpenFolder_ButtonClick()
{
	window.location.href= "inbox.aspx?Folder=" + document.form1.open_folder.options[document.form1.open_folder.selectedIndex].text;
}

function ReadMail_ButtonClick(messageID)
{
	window.location.href= "inbox_read.aspx?MessageID=" + messageID + "&Folder=" + document.form1.open_folder.options[document.form1.open_folder.selectedIndex].text;
}

function MoveToFolder_ButtonClick()
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
		alert("<% Response.Write(m_WTxt["52"]); %>!"); // Ühtegi kirja pole märgistatud
		document.form1.move.selectedIndex = 0;
		return;
	}

	if(confirm("<% Response.Write(m_WTxt["54"]); %> \"" + document.form1.move.options[document.form1.move.selectedIndex].text + "\"?")){ // Kas Te olete kindel, et tahate märgistatud kirjad asetada kausta
		document.form1.h1.value = "move";
		document.form1.h2.value = count;
		document.form1.h3.value = value;
		document.form1.h4.value = document.form1.move.options[document.form1.move.selectedIndex].text;
		document.form1.submit();
	}
	else{
		document.form1.move.selectedIndex = 0;
	}
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
          <td width="10" background="images/p_left_u.gif" align="center">&nbsp;</td>
          <td width="111" background="images/p_up.gif" align="center">
          <a href="inbox.aspx?Folder=Inbox"><b><font color="#000000"><% Response.Write(m_WTxt["1"].ToUpper()); %></font></b></a></td>
          <td width="10" background="images/p_right_ud.gif" align="center">&nbsp;</td>
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
      <input type="hidden" name="h2" value="">
      <input type="hidden" name="h3" value="">
      <input type="hidden" name="h4" value="">
      <% if(m_ErrorText.Length == 0){ %>
        <div align="center">
          <center>
          <table border="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="730" id="AutoNumber10" cellpadding="0">
            <tr>
              <td width="100%">
              <p align="center">
              <select size="1" name="open_folder" style="width: 200; border: 1px solid #808080">
              <%
              foreach(string folder in folders){
 	             string aa = "";

	             if(folder == Request.Params["Folder"]){
	             	aa += "<option selected>" + folder + "</option>";
	             }
	             else{
	             	aa += "<option>" + folder + "</option>";
	             }

    	         Response.Write(aa);
              }
              %>
              </select>&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["9"]); %>" onClick="javascript:OpenFolder_ButtonClick()" name="open" style="width: 140; border: 1px solid #808080"></td>
            </tr>
            <tr>
              <td width="100%" height="20" valign="bottom">
              <hr color="#C0C0C0" width="730" size="1"></td>
            </tr>
          </table>
          </center>
        </div>
        <p>&nbsp;</p>
        <div align="center">
          <center>
          <table border="0" cellpadding="0" cellspacing="0" style="border-collapse: collapse" bordercolor="#111111" width="710" id="AutoNumber8">
            <tr>
              <td width="710" align="center">
              <p align="left">
              <input type="button" value="<% Response.Write(m_WTxt["10"]); %>" onClick="javascript:Mark_ButtonClick()" name="mark" style="width: 80; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["11"]); %>" onClick="javascript:UnMark_ButtonClick()" name="unmark" style="width: 160; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <input type="button" value="<% Response.Write(m_WTxt["12"]); %>" onClick="javascript:Delete_ButtonClick()" name="delete" style="width: 190; border: 1px solid #808080">&nbsp;&nbsp;&nbsp;
              <select size="1" onChange="javascript:MoveToFolder_ButtonClick()" name="move" style="width: 230; border: 1px solid #808080">
              <option selected><% Response.Write(m_WTxt["51"]); %></option>
              <%
              foreach(string folder in folders){
 	             string aa = "";

             	 aa += "<option>" + folder + "</option>";

    	         Response.Write(aa);
              }
              %>
              </select></td>
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
              <td width="200" height="25" align="center" bgcolor="#C0C0C0"><b>
              <font color="#FFFFFF"><% Response.Write(m_WTxt["13"]); %></font></b></td>
              <td width="330" height="25" align="center" bgcolor="#C0C0C0"><b>
              <font color="#FFFFFF"><% Response.Write(m_WTxt["14"]); %></font></b></td>
              <td width="110" height="25" align="center" bgcolor="#C0C0C0"><b>
              <font color="#FFFFFF"><% Response.Write(m_WTxt["35"]); %></font></b></td>
              <td width="60" height="25" align="center" bgcolor="#C0C0C0"><b>
              <font color="#FFFFFF"><% Response.Write(m_WTxt["16"]); %></font></b></td>
            </tr>
              <%
              DataView dv = ds.Tables[0].DefaultView;
              dv.Sort = "Date DESC";
             
              foreach(System.Data.DataRowView drv in dv){
              	System.Data.DataRow dr = drv.Row;

 	            string aa = "";

				if(Convert.ToBoolean(dr["IsNewMsg"])){
            		aa += "<tr>";
              		aa += "<td width=\"30\" align=\"center\" bgcolor=\"#F2F9F2\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">";
              		aa += "<input type=\"checkbox\" name=\"C2\" value=\"" + dr["UID"].ToString() + "\"></td>";
              		aa += "<td width=\"200\" bgcolor=\"#F2F9F2\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">";
              		aa += "<a href=\"javascript:ReadMail_ButtonClick('" + dr["UID"].ToString() + "')\">" + dr["Sender"].ToString() + "</a></td>";
              		aa += "<td width=\"330\" bgcolor=\"#F2F9F2\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">" + dr["Subject"].ToString() + "</td>";
              		aa += "<td width=\"110\" align=\"center\" bgcolor=\"#F2F9F2\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">" + Convert.ToDateTime(dr["Date"]).ToString("dd-MM-yyyy HH:mm") + "</td>";
              		aa += "<td width=\"60\" align=\"right\" bgcolor=\"#F2F9F2\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">" + Convert.ToDecimal(Convert.ToDecimal(dr["Size"]) / 1000).ToString("f1") + "k</td>";
              		aa += "</tr>";
              	}
              	else{
            		aa += "<tr>";
              		aa += "<td width=\"30\" align=\"center\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">";
              		aa += "<input type=\"checkbox\" name=\"C2\" value=\"" + dr["UID"].ToString() + "\"></td>";
              		aa += "<td width=\"200\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">";
              		aa += "<a href=\"javascript:ReadMail_ButtonClick('" + dr["UID"].ToString() + "')\">" + dr["Sender"].ToString() + "</a></td>";
              		aa += "<td width=\"330\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">" + dr["Subject"].ToString() + "</td>";
              		aa += "<td width=\"110\" align=\"center\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">" + Convert.ToDateTime(dr["Date"]).ToString("dd-MM-yyyy HH:mm") + "</td>";
              		aa += "<td width=\"60\" align=\"right\">";
              		aa += "<p style=\"margin-left: 2; margin-right: 2\">" + Convert.ToDecimal(Convert.ToDecimal(dr["Size"]) / 1000).ToString("f1") + "k</td>";
              		aa += "</tr>";
              	}

    	        Response.Write(aa);
              }

              if(ds.Tables[0].Rows.Count == 0){
 	            string aa = "";

            	aa += "<tr>";
              	aa += "<td width=\"730\" align=\"center\" colspan=\"5\" height=\"21\">-- " + m_WTxt["17"] + " --</td>";
            	aa += "</tr>";

    	        Response.Write(aa);
              }
             
              %>
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