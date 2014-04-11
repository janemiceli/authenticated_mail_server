<%@ Page language="c#" AutoEventWireup="false" EnableSessionState="true"%>

<script runat="server">

protected override void OnLoad(EventArgs e)
{
	base.OnLoad(e);

	Response.Redirect("login.aspx");
}

</script>
