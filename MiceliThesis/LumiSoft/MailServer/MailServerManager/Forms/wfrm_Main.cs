using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using LumiSoft.MailServer;

namespace MailServerManager.Forms
{	
	/// <summary>
	/// Main UI form.
	/// </summary>
	public class wfrm_Main : System.Windows.Forms.Form
	{
		private LumiSoft.UI.Controls.WFrame wFrame1;
		private System.Windows.Forms.ImageList imgList_Tree;
		private System.ComponentModel.IContainer components;

		private TreeView m_pTreeView = null;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public wfrm_Main()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			InitTree();

			DataSet ds = new DataSet();
			ds.ReadXml(Application.StartupPath + "\\Settings\\ManagerSettings.xml");

			foreach(DataRow dr in ds.Tables["Servers"].Rows){
				AddMailServer(dr["Name"].ToString(),dr["WebServicesUrl"].ToString(),dr["WebServicesUser"].ToString(),dr["WebServicesPwd"].ToString());
			}
		}

		#region function Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region function wfrm_Main_Closing

		private void wfrm_Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(wFrame1.Frame_Form != null){
				Form frm = wFrame1.Frame_Form;
				frm.Dispose();
			}
		}

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(wfrm_Main));
			this.wFrame1 = new LumiSoft.UI.Controls.WFrame();
			this.imgList_Tree = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// wFrame1
			// 
			this.wFrame1.ControlPaneWidth = 150;
			this.wFrame1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wFrame1.FormFrameBorder = System.Windows.Forms.BorderStyle.Fixed3D;
			this.wFrame1.Location = new System.Drawing.Point(0, 0);
			this.wFrame1.Name = "wFrame1";
			this.wFrame1.Size = new System.Drawing.Size(632, 413);
			this.wFrame1.SplitterColor = System.Drawing.SystemColors.Control;
			this.wFrame1.SplitterMinExtra = 0;
			this.wFrame1.SplitterMinSize = 0;
			this.wFrame1.TabIndex = 0;
			this.wFrame1.TopPaneBkColor = System.Drawing.SystemColors.Control;
			this.wFrame1.TopPaneHeight = 25;
			// 
			// imgList_Tree
			// 
			this.imgList_Tree.ImageSize = new System.Drawing.Size(16, 16);
			this.imgList_Tree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList_Tree.ImageStream")));
			this.imgList_Tree.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// wfrm_Main
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 413);
			this.Controls.Add(this.wFrame1);
			this.Name = "wfrm_Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LumiSoft Mail Server Manager";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.wfrm_Main_Closing);
			this.ResumeLayout(false);

		}
		#endregion


		#region Events handling

		#region function treeView1_AfterSelect

		private void treeView1_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if(e.Node == null || e.Node.Tag == null){
				return;
			}

			this.Cursor = Cursors.WaitCursor;

			m_pTreeView.Refresh();

			wFrame1.Frame_ToolBar = null;
			wFrame1.Frame_Form   = null;
			wFrame1.FormFrameBorder = BorderStyle.Fixed3D;

			NodeData nData = (NodeData)e.Node.Tag;
			switch(nData.NodeType)
			{
				case NodeType.Servers:
					wFrame1.Frame_Form = new wfrm_Servers(this);
					break;

				case NodeType.Server:
					wFrame1.Frame_Form = new wfrm_Server(nData.ServerAPI);
					break;


				case NodeType.System:
					wFrame1.FormFrameBorder = BorderStyle.None;
					wFrame1.Frame_Form = new wfrm_System_Frame(nData.ServerAPI);
					break;

				case NodeType.Domains:
					wFrame1.Frame_Form = new wfrm_Domains(nData.ServerAPI,wFrame1);
					break;

				case NodeType.Users:
					wFrame1.Frame_Form = new wfrm_Users(nData.ServerAPI,wFrame1);
					break;

				case NodeType.Aliases:
					wFrame1.Frame_Form = new wfrm_Aliases(nData.ServerAPI,wFrame1);
					break;

				case NodeType.Routing:
					wFrame1.Frame_Form = new wfrm_Routing(nData.ServerAPI,wFrame1);
					break;

				case NodeType.Security:
					wFrame1.Frame_Form = new wfrm_Security(nData.ServerAPI,wFrame1);
					break;

				case NodeType.Filters:
					wFrame1.Frame_Form = new wfrm_Filters(nData.ServerAPI,wFrame1);
					break;

				default:					
					wFrame1.Frame_Form = new Form();
					break;
			}

			this.Cursor = Cursors.Default;
		}

		#endregion

		#region function treeView1_MouseUp

		private void treeView1_MouseUp(object sender,System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right && m_pTreeView.SelectedNode != null){
				TreeNode node = m_pTreeView.SelectedNode;
				
				NodeData nData = (NodeData)node.Tag;
				switch(nData.NodeType)
				{
					case NodeType.Server:
						ContextMenu men = new ContextMenu();
				//		men.
						break;
				}

			}
		}

		#endregion

		#endregion


		#region function AddMailServer

		/// <summary>
		/// Adds mailserver to treeview.
		/// </summary>
		internal void AddMailServer(string name,string webServicesUrl,string userName,string password)
		{
			TreeNode node_Server = new TreeNode(name,1,1);

			ServerAPI api = null;			
			// Local server connection
			if(node_Server.Text == "[local]"){
				api = new ServerAPI(Application.StartupPath + "\\Settings\\");
			}
			// Remote server connection.
			else{
				api = new ServerAPI(Application.StartupPath + "\\Settings\\",webServicesUrl,userName,password);
			}
			
			node_Server.Tag = new NodeData(NodeType.Server,api);
			m_pTreeView.Nodes[0].Nodes.Add(node_Server);				
				
				TreeNode node_Sys = new TreeNode("System",2,2);
				node_Sys.Tag = new NodeData(NodeType.System,api);
				node_Server.Nodes.Add(node_Sys);

				TreeNode node_Domains = new TreeNode("Domains",3,3);
				node_Domains.Tag = new NodeData(NodeType.Domains,api);
				node_Server.Nodes.Add(node_Domains);

				TreeNode node_Users = new TreeNode("Users",4,4);
				node_Users.Tag = new NodeData(NodeType.Users,api);
				node_Server.Nodes.Add(node_Users);

				TreeNode node_Aliases = new TreeNode("Aliases",5,5);
				node_Aliases.Tag = new NodeData(NodeType.Aliases,api);
				node_Server.Nodes.Add(node_Aliases);

				TreeNode node_Routing = new TreeNode("Routing",6,6);
				node_Routing.Tag = new NodeData(NodeType.Routing,api);
				node_Server.Nodes.Add(node_Routing);

				TreeNode node_Security = new TreeNode("Security",7,7);
				node_Security.Tag = new NodeData(NodeType.Security,api);
				node_Server.Nodes.Add(node_Security);

				TreeNode node_Filters = new TreeNode("Filters",8,8);
				node_Filters.Tag = new NodeData(NodeType.Filters,api);
				node_Server.Nodes.Add(node_Filters);
		}

		#endregion

		#region function InitTree

		/// <summary>
		/// Inits treeview.
		/// </summary>
		private void InitTree()
		{
			m_pTreeView = new TreeView();
			m_pTreeView.BorderStyle = BorderStyle.None;
			m_pTreeView.HideSelection = false;
			m_pTreeView.HotTracking = true;
			m_pTreeView.ImageList = imgList_Tree;
			m_pTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
			m_pTreeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseUp);
			wFrame1.Frame_BarControl = m_pTreeView;

			TreeNode node_MailServers = new TreeNode("Mail Servers",0,0);
			node_MailServers.Tag = new NodeData(NodeType.Servers);
			m_pTreeView.Nodes.Add(node_MailServers);				
		}

		#endregion

	}


	#region enum NodeType

	internal enum NodeType
	{
		Dummy,
		Servers,
		Server,
		System,
		Domains,
		Users,
		Aliases,
		Routing,
		Security,
		Filters,
	}

	#endregion

	#region class NodeData

	internal class NodeData
	{
		private NodeType  m_NodeType   = NodeType.Dummy;
		private ServerAPI m_pServerAPI = null;

		public NodeData(NodeType type) : this(type,null)
		{
		}

		public NodeData(NodeType type,ServerAPI api)
		{
			m_NodeType   = type;
			m_pServerAPI = api;
		}


		#region Properties Implementation

		/// <summary>
		/// Gets node type.
		/// </summary>
		public NodeType NodeType
		{
			get{ return m_NodeType; }
		}

		/// <summary>
		/// Gets referance server API.
		/// </summary>
		public ServerAPI ServerAPI
		{
			get{ return m_pServerAPI; }
		}

		#endregion

	}

	#endregion

}
