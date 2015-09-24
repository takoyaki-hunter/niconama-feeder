using System;
using Gtk;
using Gdk;
using GLib;
using Cairo;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Net.WebSockets;
using System.Web;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;


namespace t
{	
	public class CustomCellRenderer : Gtk.CellRenderer
	{
		[GLib.Property ("text1")]
		public string text1 { get; set; }
		[GLib.Property ("text2")]
		public string text2 { get; set; }
		[GLib.Property ("text3")]
		public string text3 { get; set; }

		public override void GetSize (Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			x_offset = 0;
			y_offset = 0;
			width = 0;
			height = 0;
		}
		protected override void Render (Drawable window, Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, Gdk.Rectangle expose_area, CellRendererState flags)
		{
			try{
				Gdk.Rectangle text_area1 = new Gdk.Rectangle();
			Gdk.Rectangle text_area2 = new Gdk.Rectangle();
			Gdk.Rectangle text_area3 = new Gdk.Rectangle();
				text_area1.Y= cell_area.Y;
				text_area2.Y= cell_area.Y+33;

				text_area3.X = cell_area.Width-20;
				text_area3.Y= cell_area.Y+33;
				text_area3.Width = 75;

				Pango.Layout text_l1 = new Pango.Layout(widget.PangoContext);
				text_l1.FontDescription = Pango.FontDescription.FromString ("Meiryo,Arial 10.5");
				text_l1.SetText(text1);

				Pango.Layout text_l2 = new Pango.Layout(widget.PangoContext);
				text_l2.FontDescription = Pango.FontDescription.FromString ("Meiryo,MS Gothic,Arial 8");
				text_l2.SetText(text2);
				text_l2.Alignment = Pango.Alignment.Right;


				Pango.Layout text_l3 = new Pango.Layout(widget.PangoContext);
				text_l3.Width = Pango.Units.FromPixels(text_area3.Width);
				text_l3.FontDescription = Pango.FontDescription.FromString ("Meiryo,MS Gothic,Arial 8");
				text_l3.Alignment = Pango.Alignment.Right;
				text_l3.SetText(text3);
				text_l2.Width = Pango.Units.FromPixels(cell_area.Width-text_l3.Text.Length*8-13);

				StateType state = flags.HasFlag(CellRendererState.Selected) ?
				widget.IsFocus ? StateType.Selected : StateType.Active : StateType.Normal;
				text_l3.SetMarkup("<span color=" + (char)34 + "grey" + (char)34 + ">" + text_l3.Text + "</span>");;
				window.DrawLayout(widget.Style.TextGC(state), 55, text_area1.Y, text_l1);
				window.DrawLayout(widget.Style.TextGC(state), 55, text_area2.Y, text_l2);
				window.DrawLayout(widget.Style.TextGC(state), text_area3.X, text_area3.Y, text_l3);

				text_l1.Dispose ();
				text_l2.Dispose ();
				text_l3.Dispose ();

			}catch(Exception e){
				Console.WriteLine (e);
			}

		}
	}
	public class Server {
		private WebClient wc = new WebClient();
		private ListStore ls1;
		private ListStore ls2;
		private TreeView tree1;
		private TreeView tree2;
		private static string commId;
		public Server ()
		{
			
			this.ls1 = new Gtk.ListStore (
				typeof(Gdk.Pixbuf),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(long),
				typeof(string)
			);
			this.ls2 = new Gtk.ListStore (
				typeof(Gdk.Pixbuf),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(long),
				typeof(string)
			);

			this.ls1.AppendValues(null,"","","","",(long)0,"");
			this.ls2.AppendValues (null,"","","","",(long)0,"");
			this.tree1 = CreateTree (this.ls1);
			this.tree2 = CreateTree (this.ls2);
		}
		public TreeView CreateTree(ListStore ls){

			TreeView tree = new TreeView ();
			CustomCellRenderer cr = new CustomCellRenderer ();
			Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ();
			col.PackStart (cr, true);
			col.AddAttribute(cr, "text1", 1);
			col.AddAttribute(cr, "text2", 2);
			col.AddAttribute(cr, "text3", 3);
			tree.AppendColumn ("", new Gtk.CellRendererPixbuf (), "pixbuf", 0);
			tree.AppendColumn (col);
			tree.Model = ls;
			col.Expand = true;
			tree.HeadersVisible = false;
			tree.ButtonPressEvent+=CellClicked;
			return tree;
		}
		public ScrolledWindow Page_mylist()
		{
			ScrolledWindow scroll = new ScrolledWindow ();
			scroll.HscrollbarPolicy = PolicyType.Automatic;
			scroll.VscrollbarPolicy = PolicyType.Automatic;
			scroll.Add (this.tree1);
			return scroll;
		}
		public ScrolledWindow Page_all()
		{
			ScrolledWindow scroll = new ScrolledWindow ();
			scroll.HscrollbarPolicy = PolicyType.Automatic;
			scroll.VscrollbarPolicy = PolicyType.Automatic;
			scroll.Add (this.tree2);
			return scroll;
		}

		[GLib.ConnectBeforeAttribute]
		static void CellClicked (object sender, ButtonPressEventArgs e)
		{
			TreeModel ls = (sender as TreeView).Model;
			TreePath path;
			int x = Convert.ToInt32(e.Event.X);
			int y = Convert.ToInt32(e.Event.Y);
			if (!(sender as TreeView).GetPathAtPos (x, y, out path)) 
				return;

			TreeIter iter;
			if (!ls.GetIter(out iter, path)) 
				return;

			if (e.Event.Button == 1) 
			{
					System.Diagnostics.Process.Start("http://live.nicovideo.jp/watch/lv"+ls.GetValue (iter, 4));
			}
			if (e.Event.Button == 3)
			{
					Menu m = new Menu ();
				MenuItem item = new MenuItem("お気に入りに登録する");
					commId = (string)ls.GetValue (iter, 6);
					item.ButtonPressEvent += AddToFavorite;
					m.Add (item);
					m.ShowAll ();
					m.Popup ();
			}
		}
		static void AddToFavorite (object sender, ButtonPressEventArgs e)
		{
			System.Diagnostics.Process.Start("http://com.nicovideo.jp/motion/"+commId);
		}

	public async void receiveFromServer(string addr, string port, string thread) {
			await Task.Run( () => {
				var httpListener = new HttpListener ();
				IPAddress[] addresslist = Dns.GetHostAddresses (addr);
				if (addresslist.Rank == 0)
					return;

				IPEndPoint ephost = new IPEndPoint (addresslist [0], int.Parse (port));
				System.Net.Sockets.Socket sock = new System.Net.Sockets.Socket (
					System.Net.Sockets.AddressFamily.InterNetwork,
					System.Net.Sockets.SocketType.Stream,
					System.Net.Sockets.ProtocolType.Tcp);
				sock.Connect (ephost);
				string param = String.Format ("<thread thread=\"{0}\" version=\"20061206\" scores=\"1\" res_from=\"-1\"/>\0", thread);
				byte[] data = Encoding.UTF8.GetBytes (param);
				sock.Send (data, data.Length, System.Net.Sockets.SocketFlags.None);

				const int MAX_RECEIVE_SIZE = 1024 * 100;
				string prev = "";
				this.setTimer();
				wc = new WebClient();
				for (;;) {
					byte[] resBytes = new byte[MAX_RECEIVE_SIZE];
					int resSize = sock.Receive (resBytes, resBytes.Length, System.Net.Sockets.SocketFlags.None);
					if (resSize == 0)
						break;
					string xml = prev + Encoding.UTF8.GetString (resBytes, 0, resSize).Replace ('\0', '\n');
					string[] lines = xml.Split ('\n');
					foreach (string line in lines) {
						if (line != "" && !line.EndsWith (">")) {
							prev = line;
							break;
						}
						if (line.StartsWith ("<chat")) {
							XmlDocument xdoc = new XmlDocument ();
							xdoc.LoadXml (line);
							string[] infos = xdoc.InnerText.Split (',');
							if (infos.Length != 3) {
								continue;
							}

							try{
								XmlNodeList nl = MainClass.Request (
									"http://live.nicovideo.jp/api/getstreaminfo/lv"+infos[0],
									@"//title|//default_community|//name|//thumbnail",
									new NameValueCollection());
								
								if(MainClass.communities.IndexOf(infos[1]) > -1)
									ShowLiveInfo(infos[0], ref nl, ref this.ls1, ref this.tree1);
								ShowLiveInfo(infos[0], ref nl, ref this.ls2, ref this.tree2);
							}catch(System.Exception e){
								Console.WriteLine(e);
							}
						}
					}
					wc.Dispose();
				}
				sock.Shutdown (System.Net.Sockets.SocketShutdown.Both);
				sock.Close ();
			} );
		}/*
		public void SendComment(string line,string addr, string port, string thread)
		{
			if (line.StartsWith("<thread"))
			{
				string ticket = "";
				string server_time = "";
				XmlDocument xdoc = new XmlDocument();
				xdoc.LoadXml(line);
				XmlElement root = xdoc.DocumentElement;
				foreach (XmlAttribute attrib in root.Attributes)
				{
					if (attrib.Name == "ticket")
					{
						ticket = attrib.Value;
					}
					if (attrib.Name == "server_time")
					{
						server_time = attrib.Value;
					}
				}
				if ((ticket == "") || (server_time == ""))
				{
					return false;
				}
				DateTime m_DateTimeStart = DateTime.Now;

				Int64 serverTimeSpan = Int64.Parse(server_time) - Int64.Parse(m_base_time);
				Int64 localTimeSpan = UnixTimestamp(DateTime.Now) - UnixTimestamp(m_DateTimeStart);
				string vpos = ((serverTimeSpan + localTimeSpan) * 100).ToString();
				string postkey = GetPostKey ();
				if (postkey == "")
				{
					return false;
				}
				param = String.Format("<chat thread=\"{0}\" ticket=\"{1}\" vpos=\"{2}\" postkey=\"{3}\" mail=\" 184\" user_id=\"{4}\" premium=\"1\">わこつ</chat>\0"
					, thread
					, ticket
					, vpos
					, postkey
					, m_userid);
				data = Encoding.UTF8.GetBytes(param);
				sock.Send(data, data.Length, System.Net.Sockets.SocketFlags.None);



			}

			if (line.StartsWith("<chat_result"))
			{
				Console.WriteLine("(投稿応答)");
				Console.WriteLine();
			}
			if (line.StartsWith("<chat "))
			{
				XmlDocument xdoc = new XmlDocument();
				xdoc.LoadXml(line);
				Console.WriteLine(xdoc.InnerText);
			}
		}*/
		public static double UnixTimestamp(ref DateTime dateTime)
		{
			return (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
		}
		static string GetPostKey(string thread)
		{
			try
			{
				string url = "http://live.nicovideo.jp/api/getpostkey?thread=" + thread + "&block_no=0";
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
				//req.CookieContainer = m_cc;//取得済みのクッキーコンテナ
				WebResponse res = req.GetResponse();
				Stream resStream = res.GetResponseStream();
				StreamReader sr = new StreamReader(resStream, Encoding.UTF8);
				string text = sr.ReadToEnd();
				sr.Close();
				resStream.Close();
				return text.Substring(8, text.Length - 8);
			}
			catch (Exception e)
			{
				Console.WriteLine("Message    :n" + e.Message);
				Console.WriteLine("Type       :n" + e.GetType().FullName);
				Console.WriteLine("StackTrace :n" + e.StackTrace.ToString());
			}
			return "";
		}


		private Timer timer;
		private void setTimer()
		{
			timer = new Timer();
			timer.Interval = 30000;
			timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
			timer.Start();
		}
		private void OnTimerEvent(object source, ElapsedEventArgs e)
		{
			updateTime (ref ls1);
			updateTime (ref ls2);
		}
		private void updateTime(ref ListStore ls)
		{
			ls.Foreach(new TreeModelForeachFunc(delegate(TreeModel model, TreePath path, TreeIter iter) {
				long ticks = DateTime.Now.Ticks - (long)model.GetValue(iter,5);
				TimeSpan t = TimeSpan.FromTicks( ticks );
				string pt = "";
				if(t.Hours < 1){
					pt = string.Format("{0:#0}分前", t.Minutes);
				}else{
					pt = string.Format("{0:#0}時間{1:#0}分前",
						t.Hours,
						t.Minutes);
				}
				model.SetValue(iter, 3, pt);
				return false;
			}));
		}


		public void ShowLiveInfo(string liveId, ref XmlNodeList nl, ref ListStore ls, ref TreeView tree){

			object [] vals = new object[7];
			try{
				byte[] imgData = wc.DownloadData(nl [3].InnerText);
				vals[0] = (Gdk.Pixbuf) new Gdk.Pixbuf (imgData,50,50);//サムネイル
			}catch{
				vals[0] = (Gdk.Pixbuf) new Gdk.Pixbuf("noimage.png",50,50);
			}

	//			Console.WriteLine (nl [2].InnerText);
			vals[1] = nl[0].InnerText;//放送タイトル
			vals[2] = nl[2].InnerText;//コミュニティ名
			vals[3] = "0分前";
			vals[4] = liveId;
			vals[5] = DateTime.Now.Ticks;
			vals[6] = nl[1].InnerText;//コミュニティID

			var iter = ls.InsertWithValues(1,vals);

		}
		byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}
	}
	public class SettingsMap
	{
		public string mail;
		public string password;
	}
	public static class Settings
	{

		public static void SetSettings(string m, string p){
			string fn = @"settings.Xml";
			XmlSerializer xs = new XmlSerializer(typeof(SettingsMap));
			try{
				SettingsMap n = new SettingsMap();
				n.mail = m;
				n.password = p;
				FileStream fs = new FileStream (fn, FileMode.Create);
				xs.Serialize(fs,n);
				fs.Close ();
			}catch(Exception e){
				Console.WriteLine (e);
			}

		}

		public static void GetSettings(){
			string fn = @"settings.Xml";
			XmlSerializer xs = new XmlSerializer(typeof(SettingsMap));
			try{
				FileStream fs = new FileStream (fn, FileMode.Open);
				SettingsMap sm = (SettingsMap)xs.Deserialize(fs);
				//				Console.WriteLine(sm.ticket);
				fs.Close ();
				if(sm.mail != null){
					MainClass.mail = sm.mail;
					MainClass.password = sm.password;
				}
			}catch(Exception e){
				Console.WriteLine (e);
			}

		}

	}
	public class MainClass
	{
		private static WebClient wc;
		public static string communities="";
		public static LoginFormUI form;
		public static string mail = "";
		public static string password = "";
		public static Gtk.Window start;
		private static NameValueCollection nv;

		//public static Gtk.TreeView tree;
		public static void Main (string[] args)
		{
			Gtk.Application.Init ();

			Settings.GetSettings ();
//			test ();
			if (mail != null && mail != "") {
				GetTicket ();
			} else {
				LoginRequest ();
			}
			Gtk.Application.Run ();

		}

		static void LoginRequest(){
			form = new LoginFormUI ();
			form.Build ();
			start = new Gtk.Window ("ログインフォーム");
			start.SetPosition(Gtk.WindowPosition.Center);
			start.Add (form.table1);
			start.DeleteEvent += new DeleteEventHandler (OnQuit);
			start.ShowAll ();
		}
		public static void GetTicket(){

			nv = new NameValueCollection();
			nv.Add("mail", MainClass.mail);
			nv.Add("password", MainClass.password);

			XmlNodeList nl = Request (
				"https://secure.nicovideo.jp/secure/login?site=nicolive_antenna",
				@"//ticket",
				nv
			);
			if (MainClass.start == null) {
				if (nl.Count > 0) {
					GetFeed (nl [0].InnerText);
				}else{
					Console.WriteLine ("保存済みのアカウントは無効です");
					LoginRequest ();
				}
			} else {
				if (nl.Count > 0) {
					MainClass.start.Destroy ();
					GetFeed (nl [0].InnerText);
				}else{
					form.label1.Text = "メールアドレスかパスワードが間違っています";
				}
			}
		}

		public static ScrolledWindow Page_tab(string tab)
		{
			Server sv = new Server ();
			ScrolledWindow sc = new ScrolledWindow();
			var ls = new Gtk.ListStore (
				typeof(Gdk.Pixbuf),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(long),
				typeof(string)
			);
			var tree = sv.CreateTree (ls);
			sc.HscrollbarPolicy = PolicyType.Automatic;
			sc.VscrollbarPolicy = PolicyType.Always;
//			sc.GrabBrokenEvent += ScHiddenEvent;
			sc.Add (tree);
			sc.Realized += (sender, e) => TreeRenderingEvent(sender, e, tab, tree, ls);
			return sc;
		}
		private async static void TreeRenderingEvent(object sender, EventArgs e, string tab, TreeView tree, ListStore ls){
			await Task.Run (() => {
				
					nv = new NameValueCollection ();
					nv.Add ("tab", tab);
					nv.Add ("sort", "view_counter");
				try{
					WebClient client = new WebClient ();
					client.Encoding = System.Text.Encoding.UTF8;
					client.QueryString = nv;
					string response = client.UploadString ("http://live.nicovideo.jp/recent/rss", "POST");
					XmlDocument doc = new XmlDocument ();
					doc.LoadXml (response);

					XmlNamespaceManager NsManager = new XmlNamespaceManager (doc.NameTable);
					NsManager.AddNamespace ("nicolive", "http://live.nicovideo.jp/");
					NsManager.AddNamespace ("media", "http://search.yahoo.com/mrss/");
					XmlNodeList items = doc.SelectNodes ("//channel/item", NsManager);
					XmlNodeList nl;
					wc = new WebClient();
					foreach (XmlNode item in items) {
							nl = item.SelectNodes ("title|guid|pubDate|nicolive:community_name|nicolive:community_id", NsManager);
							/*Console.WriteLine (nl [0].InnerText);//タイトル
							Console.WriteLine (nl [1].InnerText);//放送ID
							Console.WriteLine (nl [2].InnerText);//放送名
							Console.WriteLine (nl [3].InnerText.Replace ("co", ""));//コミュニティID
							Console.WriteLine ("http://icon.nimg.jp/community/s/" + nl [3].InnerText + ".jpg");*/
							ShowRssData (ref nl, ref ls, ref tree);
					}
					wc.Dispose();
					client.Dispose ();
				}catch(Exception ext){
					Console.Write(ext.StackTrace);
				}

			});
		}



		private static void test(){

			Gtk.Window window = new Gtk.Window ("ニコ生フィーダー v1.0");
			window.DeleteEvent += new DeleteEventHandler (OnQuit);
			window.Gravity = Gdk.Gravity.North;
			window.SetSizeRequest (300, 550);
			Notebook nb = new Notebook ();

			nb.AppendPage(Page_tab ("common"), new MultiTab("タブ（一般）",nb));
			nb.AppendPage(Page_tab ("live"), new MultiTab("タブ（ゲーム）",nb));

			//sc.Add (tree);
			window.Add (nb);
			window.ShowAll ();
		}
		public static void ShowRssData(ref XmlNodeList nl, ref ListStore ls, ref TreeView tree){
			object [] vals = new object[7];
			try{
				byte[] imgData = wc.DownloadData("http://icon.nimg.jp/community/s/"+nl[4].InnerText+".jpg");
				vals[0] = (Gdk.Pixbuf) new Gdk.Pixbuf (imgData,50,50);//サムネイル
			}catch{
				vals[0] = (Gdk.Pixbuf) new Gdk.Pixbuf("noimage.png",50,50);
			}
			DateTime time = DateTime.Parse(String.Format("{0:MM/dd/yyyy hh:mm tt}", DateTime.Parse(nl[2].InnerText.Remove(nl[2].InnerText.IndexOf(" +")))));

			TimeSpan t = TimeSpan.FromTicks( DateTime.Now.Ticks - time.Ticks );
			string pt = "";
			if(t.Hours < 1){
				pt = string.Format("{0:#0}分前", t.Minutes);
			}else{
				pt = string.Format("{0:#0}時間{1:#0}分前",t.Hours,t.Minutes);
			}
			vals[1] = nl[0].InnerText;//放送タイトル
			vals[2] = nl[3].InnerText;//コミュニティ名
			vals[3] = pt;
			vals[4] = nl[1].InnerText.Replace("lv","");
			vals[5] = time.Ticks;
			vals[6] = nl[4].InnerText;//コミュニティID
			ls.AppendValues (vals);
		}

		static void GetFeed(string ticket)
		{
			nv = new NameValueCollection ();
			nv.Add ("ticket", ticket);
			XmlNodeList nl = Request (
				"http://live.nicovideo.jp/api/getalertstatus",
				@"//communities|//addr|//port|//thread",
				nv);
			if (nl.Count == 0) {
				Console.WriteLine ("チケットの値が不正です");
				LoginRequest ();
				return;
			}
			Server sv = new Server ();
			Notebook nb = new Notebook ();
			nb.AppendPage(sv.Page_all (), new MultiTab("最新の放送",nb));
			nb.AppendPage(sv.Page_mylist (), new MultiTab("お気に入り",nb));
			nb.AppendPage(Page_tab ("common"), new MultiTab("タブ（一般）",nb));
			nb.AppendPage(Page_tab ("live"), new MultiTab("タブ（ゲーム）",nb));
			nb.AppendPage(Page_tab ("face"), new MultiTab("タブ（顔出し）",nb));
			nb.AppendPage(Page_tab ("totu"), new MultiTab("タブ（凸待ち）",nb));
			nb.AppendPage(Page_tab ("req"), new MultiTab("タブ（動画紹介）",nb));
			nb.AppendPage(Page_tab ("try"), new MultiTab("タブ（やってみた）",nb));
			nb.AppendPage(Page_tab ("r18"), new MultiTab("タブ（Ｒ―18）",nb));

			//nb.SwitchPage += (sender, e) => PageChangedEvent(sender, e);

			Gtk.Window window = new Gtk.Window ("ニコ生フィーダー v1.0");
			window.DeleteEvent += new DeleteEventHandler (OnQuit);
			window.SetSizeRequest (300, 550);
			window.KeepAbove = true;
			Toolbar bar = new Toolbar ();
			ToggleToolButton sendBackBtn = new ToggleToolButton ();

			sendBackBtn.Toggled += (sender, e) => SendBack(sender, e, ref window);
			sendBackBtn.TooltipText = "最前面での固定を解除";
			ToggleToolButton logoutBtn = new ToggleToolButton ();
			logoutBtn.TooltipText = "ログアウト";
			logoutBtn.Clicked += (sender, e) => Logout(sender, e, ref window);

			bar.HeightRequest = 25;

			Gtk.Image img = new Gtk.Image();
			img.Pixbuf = new Gdk.Pixbuf("go_back.png",12,12);
			sendBackBtn.IconWidget = img;

			Gtk.Image img2 = new Gtk.Image();
			img2.Pixbuf = new Gdk.Pixbuf("logout.png",16,16);
			logoutBtn.IconWidget = img2;
			logoutBtn.HeightRequest = 25;
			sendBackBtn.HeightRequest = 25;
			bar.Add (logoutBtn);
			bar.Add (sendBackBtn);
			bar.ShowAll ();

			VBox vbox = new VBox(false, 0);
			vbox.PackStart (bar, false, true, 0);
			vbox.PackStart (nb, true, true, 0);
			window.Add (vbox);
			window.ShowAll ();
			window.Focus = nb;
			MainClass.communities = nl [0].InnerText;
			sv.receiveFromServer (nl [1].InnerText, nl [2].InnerText, nl [3].InnerText);
		}
		static void SendBack (object sender, EventArgs e, ref Gtk.Window window)
		{
			if ((sender as ToggleToolButton).Active == false)
				window.KeepAbove = true;
			else
				window.KeepAbove = false;
		}
		static void Logout (object sender, EventArgs e, ref Gtk.Window window)
		{
			window.Destroy ();
			Settings.SetSettings ("", "");
			LoginRequest ();
		}
		public static XmlNodeList Request(string url, string xpath, NameValueCollection nv)
		{
			WebClient client = new WebClient ();
			var response = client.UploadValues(url, nv);
			string resPostData = System.Text.Encoding.UTF8.GetString(response);
			client.Dispose();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(resPostData);
			XmlNodeList nodeList = doc.SelectNodes(xpath);
			return nodeList;
		}
		static void OnQuit (object sender, DeleteEventArgs args)
		{
			Gtk.Application.Quit ();
		}
	}

	public class MultiTab : Gtk.Box
	{
		public Gtk.Label Caption;
		public Gtk.Notebook _parent;
		public MultiTab ( string name )
		{
			CreateUI(name);
		}
		public MultiTab(string name, Gtk.Notebook parent)
		{
			_parent = parent;
			CreateUI(name);
		}
		void CreateUI(string name)
		{
			Caption = new Gtk.Label(name);
			PackStart( Caption );
			ShowAll();
		}
		public bool Active;
	}    

	public class LoginFormUI
	{
		public global::Gtk.UIManager UIManager;
		public global::Gtk.Table table1;
		private global::Gtk.Button button1;
		private global::Gtk.Button button2;
		private global::Gtk.Entry entry1;
		private global::Gtk.Entry entry2;
		public global::Gtk.Label label1;
		private global::Gtk.Label label2;
		private global::Gtk.Label label3;
		public void Build(){
			this.table1 = new global::Gtk.Table (((uint)(4)), ((uint)(3)), true);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(5));
			this.table1.ColumnSpacing = ((uint)(5));
			this.button1 = new global::Gtk.Button ();
			this.button1.CanFocus = true;
			this.button1.Name = "button1";
			this.button1.UseUnderline = true;
			this.button1.Label = global::Mono.Unix.Catalog.GetString ("ログイン");
			this.button1.Clicked += Login;
			this.button2 = new global::Gtk.Button ();
			this.button2.CanFocus = true;
			this.button2.Name = "button2";
			this.button2.UseUnderline = true;
			this.button2.Label = global::Mono.Unix.Catalog.GetString ("キャンセル");
			this.button2.Clicked += Cancel;
			this.label1 = new global::Gtk.Label();
			this.label1.Text = "ニコニコアカウントにログインしてください";
			this.label2 = new global::Gtk.Label();
			this.label2.Text = "メールアドレス";
			this.label3 = new global::Gtk.Label();
			this.label3.Text = "パスワード";
			this.entry1 = new global::Gtk.Entry ();
			this.entry1.CanFocus = true;
			this.entry1.Name = "entry1";
			this.entry1.IsEditable = true;
			this.entry1.InvisibleChar = '?';
			this.entry1.KeyPressEvent += Keypressed;
			this.entry2 = new global::Gtk.Entry ();
			this.entry2.CanFocus = true;
			this.entry2.Name = "entry2";
			this.entry2.IsEditable = true;
			this.entry2.Visibility = false;
			this.entry2.InvisibleChar = '?';
			this.entry2.KeyPressEvent += Keypressed;
			this.table1.Attach (this.label1,0,3,0,1,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:2);
			this.table1.Attach (this.label2,0,1,1,2,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:2);
			this.table1.Attach (this.label3,0,1,2,3,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:2);
			this.table1.Attach (this.entry1,1,3,1,2,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:2);
			this.table1.Attach (this.entry2,1,3,2,3,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:2);
			this.table1.Attach (this.button1,2,3,3,4,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:7);
			this.table1.Attach (this.button2,0,1,3,4,xoptions:Gtk.AttachOptions.Fill,yoptions:Gtk.AttachOptions.Fill,xpadding:5,ypadding:7);
		}
		public void Login(object obj, EventArgs args){
			this.label1.Text = "リクエストを送信中です…";
			MainClass.mail = this.entry1.Text;
			MainClass.password = this.entry2.Text;
			Settings.SetSettings(MainClass.mail, MainClass.password);
			MainClass.GetTicket();
		}
		private void Cancel(object obj, EventArgs args){
			MainClass.start.Destroy ();
			Application.Quit ();
		}
		[GLib.ConnectBefore()]
		private void Keypressed(object obj, KeyPressEventArgs e){
			//			Console.WriteLine (e.Event.Key);
			if (e.Event.Key == Gdk.Key.Return) {
				Login (obj, e);
			}
		}
	}
}

