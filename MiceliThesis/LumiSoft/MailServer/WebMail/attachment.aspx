<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="LumiSoft.Net.IMAP.Client" %>
<%@ Import Namespace="LumiSoft.Net.Mime" %>

<script runat="server">

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);
	
	IMAP_Client clnt = new IMAP_Client();
	MimeParser  p    = null;

	try
	{
		clnt.Connect(Application["IMAPServerName"].ToString(),Convert.ToInt32(Application["IMAPServerPort"]));
		clnt.Authenticate(Session["Name"].ToString(),Session["Password"].ToString());

		clnt.SelectFolder(Request.Params["Folder"].ToString());

		if(Request.Params["MessageID"] != null){
			IMAP_FetchItem[] f = clnt.FetchMessages(Convert.ToInt32(Request.Params["MessageID"]),Convert.ToInt32(Request.Params["MessageID"]),true,false,true);
		
			if(f.Length > 0){
				p = new MimeParser(f[0].Data);

				string fileName = Request.Params["File"].ToString();
				foreach(LumiSoft.Net.Mime.MimeEntry entry in p.MimeEntries){
					if(entry.ContentDisposition == LumiSoft.Net.Mime.Disposition.Attachment && entry.FileName == fileName){
						byte[] data = entry.Data;
					
						Response.ClearHeaders();
						Response.ClearContent();
						Response.ContentType = "Application/Octet-stream";
						Response.AppendHeader("Content-Disposition","Attachment;\tFileName=\"" + Request.Params["File"].ToString() + "\"");
					
						Response.OutputStream.Write(data,0,data.Length);
					}						
				}
			}
			else{
				// Siis pole kirja olemas ja viskab viga
			}
		}
	}
	catch{
	}
	
	clnt.Disconnect();
}

</script>