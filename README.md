#ニコ生フィーダー

<h3>API</h3>

　アラートのAPIと放送中の番組を取得するRSSのURLを叩いてデータを取得しています。アラートの仕様は、
  １. 認証用URLにID／パスワードをクエリと渡してチケットを取得→
  ２．URL (getalertstatus) に送信してスレッドIDを取得→
  ３．websocketを通じて双方向通信を確立、スレッドIDを送信→
  ４．新着の放送データをニコニコのサーバーから受信
というような感じです。
  C#の場合websocketによる通信は簡単です。サーバーのTCPポートを調べてConnect。Node.jsとかはいりません。ねっ簡単でしょ？
    
    IPEndPoint ephost = new IPEndPoint (addresslist [0], int.Parse (port));
    System.Net.Sockets.Socket sock = new System.Net.Sockets.Socket (
      System.Net.Sockets.AddressFamily.InterNetwork,
		  System.Net.Sockets.SocketType.Stream,
		  System.Net.Sockets.ProtocolType.Tcp);
    sock.Connect (ephost);

  RSSの場合は認証なしでソート条件・ページ番号などをクエリとして指定して取得します。
<h3>GUIによる表示 (GTK) </h3>

  GTKはちょっとドキュメントが少ないところがありますが、手軽に出来て便利です。デザインもほとんどコーディングだけで出来ますが、XamarinStudioにはデザインツールが付いています。それを使うと自動的にコードとして生成します
　このソフトの構成は
  Window
    →Vbox
      →Notebook, Toolbar
        →ScrolledWindow
          →TreeView（リスト表示用Widget）
            →CustomCellRenderer
というような風になっています
  
<h3>ライセンス</h3>
  MITライセンスです。利用の際は商用非商用に関わらずご自由にお使いください。製作者名とライセンスの明記以外は特に必要ありません
