using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DuelPortalCS
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        { 
            InitializeComponent();
                
            
          
            soundPlayer.MediaOpened += new RoutedEventHandler(soundPlayer_MediaOpened);
           // LayoutRoot.Width = ClassLibrary.BrowserScreenInformation.ClientWidth;
            //LayoutRoot.Height = ClassLibrary.BrowserScreenInformation.ClientHeight;// -NavigationGrid.Height;
            M.ScreenResized += (oldWidth, oldHeight) =>
                {
                    NavigationGrid.Width = ClassLibrary.BrowserScreenInformation.ClientWidth;

                    if (floatChatReal != null)
                    {
                        if (ClassLibrary.BrowserScreenInformation.ClientWidth < oldWidth)
                        {
                            if (floatChatReal.Width + (ClassLibrary.BrowserScreenInformation.ClientWidth - oldWidth) / 2 >= floatChatReal.MinWidth)
                                floatChatReal.Width +=(ClassLibrary.BrowserScreenInformation.ClientWidth - oldWidth) / 2;
                            else
                                floatChatReal.Width = floatChatReal.MinWidth;


                            if (floatChatReal.Position.X + (ClassLibrary.BrowserScreenInformation.ClientWidth - oldWidth) / 2 >= 0)
                                floatChatReal.Position = new Point(floatChatReal.Position.X + (ClassLibrary.BrowserScreenInformation.ClientWidth - oldWidth) / 2, floatChatReal.Position.Y);

                        }
                        if (ClassLibrary.BrowserScreenInformation.ClientHeight < oldHeight)
                        {
                            if (floatChatReal.Height + (ClassLibrary.BrowserScreenInformation.ClientHeight - oldHeight) / 2 >= floatChatReal.MinHeight)
                                floatChatReal.Height +=(ClassLibrary.BrowserScreenInformation.ClientHeight - oldHeight) / 2;
                            else
                                floatChatReal.Height = floatChatReal.MinHeight;

                            if (floatChatReal.Position.Y + (ClassLibrary.BrowserScreenInformation.ClientHeight - oldHeight) / 2 >= 0)
                               floatChatReal.Position = new Point(floatChatReal.Position.X, floatChatReal.Position.Y + (ClassLibrary.BrowserScreenInformation.ClientHeight - oldHeight) / 2);

                        }
                        /*
                        ScaleTransform scale;
                        if (floatChatReal.RenderTransform.GetType() == typeof(ScaleTransform))
                            scale = floatChatReal.RenderTransform as ScaleTransform;
                        else
                            scale = new ScaleTransform();
                        scale.ScaleX *= ClassLibrary.BrowserScreenInformation.ClientWidth / oldWidth;
                        scale.ScaleY *= ClassLibrary.BrowserScreenInformation.ClientHeight / oldHeight;
                        floatChatReal.RenderTransform = scale;*/
                        
                    }
                };
        }

        void soundPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            soundPlayer.IsMuted = false;
        }

   
        
        // After the Frame navigates, ensure the HyperlinkButton representing the current page is selected
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        { 
            foreach (UIElement child in LinksStackPanel.Children)
            {
                HyperlinkButton hb = child as HyperlinkButton;
                if (hb != null && hb.NavigateUri != null)
                {
                    if (hb.NavigateUri.ToString().Equals(e.Uri.ToString()))
                    {
                        System.Windows.Browser.HtmlPage.Document.SetProperty("title", "Duel Portal Online");
                        if (hb.Content.ToString() == "Duel") //lobby
                        {
                            if (floatChatReal == null || !LayoutRoot.Children.Contains(floatChatReal))
                            {
                                Link6_Click(null, null);
                            }
                            else
                            {
                                noResizeRecursion = true;
                            }
                   
                        }
                        VisualStateManager.GoToState(hb, "ActiveLink", true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(hb, "InactiveLink", true);
                    }
                }
            }
        }

        // If an error occurs during navigation, show an error window
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            e.Handled = true;
            ChildWindow errorWin = new ErrorWindow(e.Uri);
            errorWin.Show();
        }

        private void ContentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(M.warnOnExitMessage))
            {
                MessageBoxResult msgboxresult = MessageBox.Show(M.warnOnExitMessage,  "Leaving Page", MessageBoxButton.OKCancel);
                if (msgboxresult != MessageBoxResult.OK)
                    e.Cancel = true;
                else
                {
                    M.warnOnExitMessage = "";
                    M.sideDecking = false;
                }
            }
        }
        public void disableNavigation()
        {
            foreach (UIElement child in LinksStackPanel.Children)
            {
                if (child.GetType() == typeof(HyperlinkButton))
                {
                    HyperlinkButton hb = child as HyperlinkButton;
                    hb.IsEnabled = false;
                }
            }
        }
        public void enableNavigation()
        {
            foreach (UIElement child in LinksStackPanel.Children)
            {
                if (child.GetType() == typeof(HyperlinkButton))
                {
                    HyperlinkButton hb = child as HyperlinkButton;
                    hb.IsEnabled = true;
                }
            }
        }
        public SilverFlow.Controls.FloatingWindow floatChatReal;
        Canvas cnvChat;
        ListBox lstFloatMessages;
        ListBox lstOnlineUsers;
        TextBox txtFloatSend;
        TextBlock lblSeeOnlineUsers;
        //CheckBox chkMute;
        public bool noResizeRecursion = false;
        public void floatChatReal_SizeChanged(Size PreviousSize, Size NewSize)
        {
             if (noResizeRecursion == false && PreviousSize.Width > 0.0 && PreviousSize.Height > 0.0 
                                            && Math.Abs(floatChatReal.Width - NewSize.Width) < 3 
                                            && Math.Abs(floatChatReal.Height - NewSize.Height) < 3)
                    {
                        
                        if (lstFloatMessages.Width + NewSize.Width - PreviousSize.Width < 50)
                        {
                            noResizeRecursion = true;
                            floatChatReal.Width = PreviousSize.Width;
                        }
                        else
                        {
                             lstFloatMessages.Width += NewSize.Width - PreviousSize.Width;
                             lstOnlineUsers.Width += NewSize.Width - PreviousSize.Width;
                             txtFloatSend.Width += NewSize.Width - PreviousSize.Width;
                        }

                        if (lstFloatMessages.Height + NewSize.Height - PreviousSize.Height < 50)
                        {
                            noResizeRecursion = true;
                            floatChatReal.Height = PreviousSize.Height;
                        }
                        else
                        {
                            lstFloatMessages.Height += NewSize.Height - PreviousSize.Height;
                            lstOnlineUsers.Height += NewSize.Height - PreviousSize.Height;
                            Canvas.SetTop(txtFloatSend, Canvas.GetTop(txtFloatSend) + NewSize.Height - PreviousSize.Height);
                        }
                    }
                    else
                        noResizeRecursion = false;
        }
        public void initializeFloatChat()
        {
            floatChatReal = new SilverFlow.Controls.FloatingWindow();
            floatChatReal.Title = "Chat!";
            floatChatReal.ShowMaximizeButton = false;
            floatChatReal.ShowMinimizeButton = false;
            floatChatReal.Width = 200;
            floatChatReal.Height = 300;
            floatChatReal.SizeChanged += (s, e) =>
                {
                    floatChatReal_SizeChanged(e.PreviousSize, e.NewSize);

                };
            floatChatReal.MinHeight = 50;
            floatChatReal.MinWidth = 40;
            //chkMute = new CheckBox();
            //chkMute.Name = "chkMute";
            //chkMute.Width = 50;
            //chkMute.Height = 15;
            //chkMute.Content = "Mute";

            lstFloatMessages = new ListBox();
            lstFloatMessages.Name = "lstFloatChatMessages";
            lstFloatMessages.Width = 150;
            lstFloatMessages.Height = 200;
          
            lstOnlineUsers = new ListBox();
            lstOnlineUsers.Name = "lstOnlineUsers";
            lstOnlineUsers.Visibility = System.Windows.Visibility.Collapsed;
            lstOnlineUsers.Width = 150;
            lstOnlineUsers.Height = 200;

            lblSeeOnlineUsers = new TextBlock();
            lblSeeOnlineUsers.Text = "Roll over here to see who's online";
            lblSeeOnlineUsers.Height = 15;
            lblSeeOnlineUsers.Width = 150;
            lblSeeOnlineUsers.MouseEnter += (s, e) => 
            { lstOnlineUsers.Visibility = System.Windows.Visibility.Visible; lstFloatMessages.Visibility = System.Windows.Visibility.Collapsed; };
            lblSeeOnlineUsers.MouseLeave += (s, e) => 
            { lstOnlineUsers.Visibility = System.Windows.Visibility.Collapsed; lstFloatMessages.Visibility = System.Windows.Visibility.Visible; };
            
            txtFloatSend = new TextBox();
            txtFloatSend.Name = "txtFloatSend";
            txtFloatSend.Width = lstFloatMessages.Width;
            txtFloatSend.Height = 25;
            txtFloatSend.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter && M.sock.isConnected)
                    {
                        M.sock.SendMessage(
                 M.socketSerialize("Server", M.username, txtFloatSend.Text, MessageType.Normal));
                        txtFloatSend.Text = "";
                    }
                };

            cnvChat = new Canvas();
            cnvChat.Children.Add(lstFloatMessages);
            cnvChat.Children.Add(lstOnlineUsers);
            cnvChat.Children.Add(lblSeeOnlineUsers);
            cnvChat.Children.Add(txtFloatSend);
            //cnvChat.Children.Add(chkMute);
            floatChatReal.Content = cnvChat;

            Canvas.SetTop(lstFloatMessages, lblSeeOnlineUsers.Height);
            Canvas.SetTop(lstOnlineUsers, lblSeeOnlineUsers.Height);
            Canvas.SetTop(txtFloatSend, lblSeeOnlineUsers.Height + lstFloatMessages.Height);

            startConnections();

        }
        public void refreshFloatChat(double width, double height)
        {
            noResizeRecursion = true;
            floatChatReal.Width = width;
            floatChatReal.Height = height;

            lstFloatMessages.Width = floatChatReal.Width - 50;
            lstFloatMessages.Height = floatChatReal.Height - 100;
    
            lstOnlineUsers.Width = floatChatReal.Width - 50;
            lstOnlineUsers.Height = floatChatReal.Height - 100;

            txtFloatSend.Width = lstFloatMessages.Width;
            
            txtFloatSend.Height = 25;
            Canvas.SetTop(txtFloatSend, lblSeeOnlineUsers.Height + lstFloatMessages.Height);
        }
        private void startConnections()
        {
            if (mesDelegate == null)
            {
                mesDelegate = new Action<string>(client_MessageReceived);
                ttlDelegate = new Action<string>(changeTitle);
                sendDelegate = new Action<string>(sendMessageInvoke);
            }

            if (string.IsNullOrEmpty(M.username))
                M.username = "portalguest" + M.Rand(0, 999, new Random()).ToString();

            if (M.sock != null && M.sock.isConnected)
            {
                tReceive = new System.Threading.Thread(receiveThread);
                tReceive.Start();
            }
            else
            {

                try
                {
                    string dnsHost = Application.Current.Host.Source.DnsSafeHost;
                   // M.sock = new M.SocketClient("192.227.234.101", 4530);
                    M.sock = new M.SocketClient(dnsHost, 4530);
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }


                changeTitle("Connecting...");
                connectThread();
            }
        }
        private void Link6_Click(object sender, RoutedEventArgs e)
        {
            if (floatChatReal == null)
                initializeFloatChat();
            else if (M.sock.isConnected == false)
            { lstOnlineUsers.Items.Clear(); startConnections(); }
            else
                floatChatReal.Position = new Point(200, 200);


            LayoutRoot.Add(floatChatReal);
            floatChatReal.Show();
            imgBubble.Visibility = System.Windows.Visibility.Collapsed;
        }

        public Action<string> mesDelegate;// = new client_MessageReceivedDelegate(client);
        public Action<string> ttlDelegate;// = new changeTitleDelegate( );
        public Action<string> sendDelegate;// = new sendMessageInvokeDelegate();s
        public System.Threading.Thread tReceive;
        #region "Sockets"
        private void ContentFrame_Loaded(object sender, RoutedEventArgs e)
        {

            NavigationGrid.Width = Application.Current.Host.Content.ActualWidth;

           
        }
        public void connectThread()
        {

            try
            {
                string errormess = M.sock.Connect();
                if (M.sock.isConnected || errormess == "")
                {
                    tReceive = new System.Threading.Thread(receiveThread);
                    tReceive.Start();
                }
                else
                {
                    addMessage("Couldn't connect to the server for some reason.");
                    addMessage(errormess);
                    changeTitle("Not connected");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.InnerException.ToString());
                }
                else
                {
                    //              MessageBox.Show(ex.Message)
                }

            }
        }

        public void sendMessageInvoke(string serializedMessage)
        {
            M.sock.SendMessage(serializedMessage);
        }
        public void receiveThread()
        {
            string serializedJoin = M.socketSerialize("Server", M.username, "", MessageType.Join);
           
            this.Dispatcher.BeginInvoke(sendDelegate, serializedJoin);
            this.Dispatcher.BeginInvoke(ttlDelegate, "Connected as " + M.username);

            while (M.sock.isConnected)
            {
                try
                {
                    string serializedMessage = M.sock.ReceiveMessage();

                    if (serializedMessage == "ABORT")
                        return;
                    if (serializedMessage == "")
                    {

                        M.sock.Disconnect();
                        this.Dispatcher.BeginInvoke(ttlDelegate, new object[1] { "Disconnected due to server failure or inactivity." });
                        break;
                    }

                    if (serializedMessage != null)
                    {
                        this.Dispatcher.BeginInvoke(mesDelegate, new object[1] { serializedMessage });

                    }

                }
                catch
                {
                }
            }
        }
        private Views.Lobby MyLobby {get
        { return ((Views.Lobby)ContentFrame.Content);} }
        private Views.DuelFieldNew MyDuelField { get { return (Views.DuelFieldNew)ContentFrame.Content; } }
       
        public void client_MessageReceived(string serializedMessage)
        {
            if (string.IsNullOrEmpty(serializedMessage))
                return;

            try
            {
                List<SocketMessage> msges = M.ModifiedDeserialize(serializedMessage);

                Type formType = ContentFrame.Content.GetType();
                

                if (formType == typeof(Views.Lobby))
                #region "Lobby Sockets"
                {
                    foreach (SocketMessage msg in msges)
                    {
                        MyLobby.HandleSocketMessage(msg);
                    }

                }
#endregion
                else if (formType == typeof(Views.DuelFieldNew))
                #region "Duel Sockets"
                {
                     foreach (SocketMessage msg in msges)
                     {
                         MyDuelField.HandleSocketMessage(msg);
                     }

                }
                #endregion
                
                #region "Other Sockets"
                {
                     foreach (SocketMessage msg in msges)
                    {

                        switch (msg.mType)
                        {
                            case MessageType.Normal:
                                System.Windows.Browser.HtmlPage.Document.SetProperty("title", "Message received");

                                if (lstOnlineUsers.Items.Contains(msg.From))
                                {
                                   lstOnlineUsers.Items.Remove(msg.From);
                                   lstOnlineUsers.Items.Insert(0, msg.From);
                                }
                                chatNotification();
                                addMessage(msg.From + ": " + msg.data);
                                break;
                            case MessageType.Join:
                                if (lstOnlineUsers.Items.Contains(msg.From) == false)
                                {
                                    lstOnlineUsers.Items.Add(msg.From);
                                }
                                break;
                            case MessageType.Leave:
                                lstOnlineUsers.Items.Remove(msg.data);
                                break;
                        }
                     }
                }
                #endregion

            }
            catch (Exception)
            {
            }

        }
        /// <summary>
        /// Plays the sound.
        /// </summary>
        private void chatNotification()
        {
            if (floatChatReal != null && !floatChatReal.IsOpen)
            {
                if (chkMute.IsChecked == false)
                {
                   soundPlayer.Source = new Uri("/Sounds/ChatMessage.mp3", UriKind.Relative);
                   // soundPlayer.Play();
                }
                imgBubble.Visibility = System.Windows.Visibility.Visible;
            }

        }
        private void addMessage(string message)
        {
            string[] splitm;
            splitm = new string[10];
            System.Text.StringBuilder newmessage = new System.Text.StringBuilder();
            int element = 0;
            // int prevSpacePos = 0;
            int nextSpacePos = 0;
            while (message.Length > 40 && element < 10)
            {

                nextSpacePos = message.IndexOf(" ", 40);
                if (nextSpacePos == -1) { break; }
                splitm[element] = message.Substring(0, nextSpacePos).TrimStart();
                message = message.Substring(nextSpacePos, message.Length - nextSpacePos);
                element++;
                //prevSpacePos = nextSpacePos;
            }


            for (int n = 0; n <= element - 1; n++)
                newmessage.Append(splitm[n] + Environment.NewLine);

            newmessage.Append(message.TrimStart());


            lstFloatMessages.Items.Add(newmessage);
            lstFloatMessages.UpdateLayout();
            lstFloatMessages.ScrollIntoView(lstFloatMessages.Items[lstFloatMessages.Items.Count - 1]);
        }
        public void changeTitle(string title)
        {
            floatChatReal.Title = title;
        }
         private void floatChat_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (noResizeRecursion) { noResizeRecursion = false; return; }
            //if (floatChat == null || lstFloatMessages == null || txtFloatSend == null) return;
            //if (e.PreviousSize.Width == 0 || e.PreviousSize.Height == 0)
            //    return;
            //noResizeRecursion = true;

            //if (e.NewSize.Width < 254)
            //{ floatChat.Width = 254; lstFloatMessages.Width = 229; txtFloatSend.Width = 230; }
            //else
            //{ 
            //    if (lstFloatMessages.Width + e.NewSize.Width - e.PreviousSize.Width >= 229)
            //        lstFloatMessages.Width += e.NewSize.Width - e.PreviousSize.Width;

            //    if (txtFloatSend.Width + e.NewSize.Width - e.PreviousSize.Width >= 230)
            //        txtFloatSend.Width += e.NewSize.Width - e.PreviousSize.Width;

            //}

            //if (e.NewSize.Height < 450)
            //{ floatChat.Height = 450; lstFloatMessages.Height = 350; Canvas.SetTop(txtFloatSend, 144); }
            //else
            //{
            //    if (lstFloatMessages.Height + e.NewSize.Height - e.PreviousSize.Height >= 350)
            //        lstFloatMessages.Height += e.NewSize.Height - e.PreviousSize.Height;
            //    Canvas.SetTop(txtFloatSend, Canvas.GetTop(txtFloatSend) + e.NewSize.Height - e.PreviousSize.Height);
            //}
           

          // Canvas.SetTop(txtFloatSend, Canvas.GetTop(txtFloatSend) + e.NewSize.Height - e.PreviousSize.Height);
        }

        #endregion



        private void lblWhosOnline_MouseEnter(object sender, MouseEventArgs e)
        {
            lstOnlineUsers.Visibility = Visibility.Visible;
            lstFloatMessages.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void lblWhosOnline_MouseLeave(object sender, MouseEventArgs e)
        {
            lstOnlineUsers.Visibility = System.Windows.Visibility.Collapsed;
            lstFloatMessages.Visibility = Visibility.Visible;
        }

       


    }
}