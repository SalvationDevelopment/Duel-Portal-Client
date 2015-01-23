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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    public partial class Home : Page
    {
        DuelPortalCS.Views.cldLoading loadingMenu;
        SQLReference.Service1ConsoleClient client = new SQLReference.Service1ConsoleClient();
        DuelPortalCS.Views.Register reg = new DuelPortalCS.Views.Register();
        DuelPortalCS.Views.cldInput inputForm = new DuelPortalCS.Views.cldInput("");
      
        public Home()
        {
            InitializeComponent();

            reg.OnRegisterAccepted += RegisteredOrVerified;
        }

        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            M.CenterCanvas(LayoutRoot, this);
            M.ScreenResized += delegate
            {
               M.CenterCanvas(LayoutRoot, this);
            };
            lblSuccess.Visibility = System.Windows.Visibility.Collapsed;
            gridFullProfile.Visibility = System.Windows.Visibility.Collapsed;
            cmdSeeMore.Visibility = System.Windows.Visibility.Collapsed;

            if (M.noInternet)
            {
                MessageBoxResult mbr = MessageBox.Show("", "", MessageBoxButton.OKCancel);
                if (mbr == MessageBoxResult.OK)
                {
                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(setfileNoInternet_Get);
                    Uri urii = new Uri("http://localhost:" + client.ChannelFactory.Endpoint.Address.Uri.Port + "/accg.txt", UriKind.Absolute);
                    
                    wc.DownloadStringAsync(urii, true);
                    
                    return;
                }
                else if (mbr == MessageBoxResult.Cancel)
                {
                    mbr  = MessageBox.Show("", "" , MessageBoxButton.OKCancel);
                    if (mbr == MessageBoxResult.Cancel)
                    {
                        WebClient wc = new WebClient();
                        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(setfileNoInternet_Get);
                        Uri urii = new Uri("http://localhost:" + client.ChannelFactory.Endpoint.Address.Uri.Port + "/accg.txt", UriKind.Absolute);
                        wc.DownloadStringAsync(urii, false);
                    }
                    else
                    {
                        WebClient wc = new WebClient();
                        wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(setfileNoInternet_Get);
                        Uri urii = new Uri("http://localhost:" + client.ChannelFactory.Endpoint.Address.Uri.Port + "/accg.txt", UriKind.Absolute);
                        wc.DownloadStringAsync(urii, null);

                    }
                    return;
                }
            }


            if (!string.IsNullOrEmpty(M.username) && M.username.Contains("portalguest") == false)
            {
                lblLogin.Text = "Logged in as " + M.username;
                cmdSeeMore.Visibility = System.Windows.Visibility.Visible;
               cnvLogin.Visibility = System.Windows.Visibility.Collapsed;
                gridButtons.Visibility = System.Windows.Visibility.Collapsed;

            }
            else
            {
                try
                {
                    string user = M.GetCookie("Username");
                    string pass = M.GetCookie("Password");
                    if (!string.IsNullOrEmpty(user))
                    {
                        txtUsername.Text = user;
                        txtPassword.Password = pass;
                        chkRemember.IsChecked = true;
                    }
                    string dontShowMessageAgain = M.GetCookie("Popup");
                    if (!string.IsNullOrEmpty(dontShowMessageAgain))
                    {
                        switch (dontShowMessageAgain)
                        {
                            case "F":
                                M.doNotShowPublicMessageAgain = true;
                                break;
                            default:
                                M.doNotShowPublicMessageAgain = false;
                                break;
                        }
                    }
                    else { M.doNotShowPublicMessageAgain = false; }
                }
                catch { }
                lblPool.Text = "";
            }
            if (M.cardsWithImages.Count == 0)
            {
                client.getListOfImagesCompleted += (s, e2) =>
                    {
                        if (e2.Error == null)
                        {
                            try
                            {
                                M.cardsWithImages = e2.Result.Split('|').ToList<string>();
                            }
                            catch { }
                        }

                    };
                client.getListOfImagesAsync(); 
            }

        }
        private void tournamentBanList_Get(object sender, SQLReference.InetConnectionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    M.ParseBanlist(M.ByteArrayToString(e.Result));
                }
            }
            else
            {
                    lblSuccess.Visibility = System.Windows.Visibility.Visible;
                    lblSuccess.Text = "Error: Failure to get banlist.";

            }
        }




        private void cmdLogin_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            client.userLoginCompleted += client_LoginResult;
            cmdLogin.IsEnabled = false;
            
       

            txtUsername.Text = txtUsername.Text.Trim();
            txtPassword.Password = txtPassword.Password.Trim();
            if (chkRemember.IsChecked == true)
            {
                M.SetCookie("Username", txtUsername.Text);
                M.SetCookie("Password", txtPassword.Password);
            }
            else
            {
                M.SetCookie("Username", "");
                M.SetCookie("Password", "");
            }
            
            txtUsername.IsEnabled = false;
            txtPassword.IsEnabled = false;
            cmdRegister.IsEnabled = false;
            MyMainPage.disableNavigation();

            if (MyMainPage.floatChatReal != null && M.sock.isConnected)
            {
                M.sock.ReceiveAbort();
                M.sock.Disconnect();
                if (MyMainPage.floatChatReal.IsOpen)
                {
                    MyMainPage.floatChatReal.Close();
                }
            }
            M.username = txtUsername.Text;
            client.userLoginAsync(txtUsername.Text, txtPassword.Password);
        }
        private MainPage MyMainPage
        {
            get
            {
                return ((MainPage)((Canvas)((Border)((Frame)Parent).Parent).Parent).Parent);
            }
        }
       private void client_LoginResult(object sender, SQLReference.userLoginCompletedEventArgs e)
        {
            client.userLoginCompleted -= client_LoginResult;
            
            txtUsername.IsEnabled = true;
            txtPassword.IsEnabled = true;
            cmdRegister.IsEnabled = true;
            MyMainPage.enableNavigation();
            
          

            if (e.Error != null)
            {
                MessageBox.Show("The service is temporarily unavailable.");
                cmdLogin.IsEnabled = true;
                M.username = "";
                return;
            }


            
           if (!e.Result.Verified && !string.IsNullOrEmpty(e.Result.FailMessage))
            {

                cmdLogin.IsEnabled = true;
                if (e.Result.FailMessage.Contains("Authentication"))
                    lblSuccess.Text = "Error: MySQL Authentication Failed";
                else
                    lblSuccess.Text = e.Result.FailMessage;
                lblSuccess.Visibility = System.Windows.Visibility.Visible;

                M.username = "";
            }
            else if (!e.Result.Verified) //Not verified
            {
                M.username = "";
               DuelPortalCS.Views.NotVerified nv = new Views.NotVerified(e.Result);
               nv.OnVerificationAccepted += RegisteredOrVerified;
               nv.Show();
               
               return;
            }
            else //Success
            {
                M.username = e.Result.Username;
                M.mySet = e.Result.Pool;
                M.myUsernameId = e.Result.Id;

                switch (M.mySet)
                {
                    case "Default":

                        client.populateSQLCompleted += sqlFile_Get;

                        client.populateSQLAsync(M.username, false);
                        loadingMenu = new DuelPortalCS.Views.cldLoading("Loading " + M.mySet + " cards and images...");
                        loadingMenu.Show();
                        break;
                    default: //CCG
                        client.InetConnectionCompleted += setfile_Get;
                        client.InetConnectionAsync("http://192.227.234.101/admin/" + M.mySet + ".txt");
                        loadingMenu = new DuelPortalCS.Views.cldLoading("Loading " + M.mySet + " cards and images...");
                        loadingMenu.Show();
                        break;

                        
                }
                client.getListOfMyDecksCompleted += (s, e2) => 
                    {
                        if (e2.Error == null) 
                            M.listOfMyDecks = e2.Result.ToList(); 
                    };
                client.getListOfMyDecksAsync(M.username);
            }
        }
        private void HyperlinkButton1_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://forum.yugiohcardmaker.net/topic/279701-duel-portal-program-available-now-topic-remade-2/"));
        }
       
        
        public void sqlFile_Get(object sender, SQLReference.populateSQLCompletedEventArgs e)
        {
            M.CardStats = e.Result.ToList<SQLReference.CardDetails>();
            SQLReference.Service1ConsoleClient anotherotherCli = new SQLReference.Service1ConsoleClient();
            anotherotherCli.InetConnectionCompleted += (s, e2) =>
                {
                if (e2.Result != null)
                {
                    M.ParseBanlist(M.ByteArrayToString(e2.Result));
                    showLogin();
                }
                else
                {
                    MessageBox.Show("Could not find banlist. Tournament mode will be disabled.");
                    for (int n = 1; n <= M.TotalCards; n++)
                        M.CardStats[n].Limit = 0; 
                    showLogin();
                }
                };
            anotherotherCli.InetConnectionAsync("http://192.227.234.101/admin/banlist/Tournament.txt");
        }
        public void setfileNoInternet_Get(object sender, DownloadStringCompletedEventArgs e)
        {
            string strdata =e.Result;

            M.ParseSETCards(strdata);

            if ((bool?)e.UserState == true)
            {
                M.username = "hccgguy";
                M.opponent = "ur enemy";
//M.sideDecking = true;
                M.mySet = "HCCG";
                M.PlayerDeck.ClearAndAdd();
                M.PlayerDeck.Add(M.CardStats[1].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[2].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[3].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[4].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[5].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[6].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[9].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[75].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[64].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[42].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[66].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[54].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[99].toTrueStats());
                //M.PlayerDeck.Add(M.CardStats[180].toTrueStats());

                M.realDeckIDs.Add(1);
                M.realDeckIDs.Add(2);
                M.realDeckIDs.Add(3);
                M.realDeckIDs.Add(4);
                M.realDeckIDs.Add(5);
                M.realDeckIDs.Add(6);
                M.realDeckIDs.Add(9);
                M.realDeckIDs.Add(75);
                M.realDeckIDs.Add(64);
                M.realDeckIDs.Add(66);
                M.realDeckIDs.Add(54);
                M.realDeckIDs.Add(99);
                //M.realDeckIDs.Add(180);
               

                M.realSideDeckIDs.Add(53);
                M.realSideDeckIDs.Add(74);
                M.realSideDeckIDs.Add(89);
            }
            else if ((bool?)e.UserState == false)
            {
                M.username = "b";
               
                M.mySet = "HCCG";
                M.PlayerDeck.ClearAndAdd();
                M.PlayerDeck.Add(M.CardStats[5].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[80].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[70].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[74].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[41].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[51].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[61].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[78].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[99].toTrueStats());
                //M.PlayerDeck.Add(M.CardStats[298].toTrueStats());
                //M.PlayerDeck.Add(M.CardStats[300].toTrueStats());
                //M.PlayerDeck.Add(M.CardStats[301].toTrueStats());

                M.realSideDeckIDs.Add(53);
                M.realSideDeckIDs.Add(74);
            }
            else if ((bool?)e.UserState == null)
            {
                M.username = "watchur";
                M.IamWatching = true;
                
                M.PlayerDeck.ClearAndAdd();
                M.PlayerDeck.Add(M.CardStats[5].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[80].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[70].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[74].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[75].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[78].toTrueStats());
                M.PlayerDeck.Add(M.CardStats[89].toTrueStats());
            }
            M.PlayerEDeck.ClearAndAdd();


            client.getListOfImagesCompleted += (s, e2) =>
            {
                if (e2.Error == null)
                {
                    try
                    {
                        M.cardsWithImages = e2.Result.Split('|').ToList<string>();
                    }
                    catch { }
                }

            };
            client.getListOfImagesAsync(); 
        }
        public void setfile_Get(object sender, SQLReference.InetConnectionCompletedEventArgs e)
        {
            client.InetConnectionCompleted -= setfile_Get;
            loadingMenu.ProgressBar1.Value = 50;
            string strdata = M.ByteArrayToString(e.Result);

            if (string.IsNullOrEmpty(strdata))
            {
                MessageBox.Show("Failed to load cards. ");
                showLogin();
                return;
            }

            M.ParseSETCards(strdata);

            client.InetConnectionCompleted += (s, e2) =>
                {
                    if (e2.Result != null)
                    {
                        M.ParseBanlist(M.ByteArrayToString(e2.Result));
                        showLogin();
                    }
                    else
                    {
                        MessageBox.Show("Could not find banlist. Tournament mode will be disabled.");
                        for (int n = 1; n <= M.TotalCards; n++)
                            M.CardStats[n].Limit = 0; 
                        showLogin();
                    }
                };
            client.InetConnectionAsync("http://192.227.234.101/admin/banlist/" + M.mySet + ".txt");

     
        }
     
        public void showLogin()
        {
            loadingMenu.Close();
            lblLogin.Text = "Logged in as " + M.username;
            cmdSeeMore.Visibility = System.Windows.Visibility.Visible;

            cnvLogin.Visibility = System.Windows.Visibility.Collapsed;
            gridButtons.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void cmdRegister_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
           
            reg.Show();

        }

        public void RegisteredOrVerified(object sender, GenericEventArgs<SQLReference.LoginRegisterData> e)
        {
            SQLReference.userLoginCompletedEventArgs eventArgs = new SQLReference.userLoginCompletedEventArgs(new object[] { e.Value }, null, false, null);
            client_LoginResult(sender, eventArgs);
        }
        
        #region "Profile"
        private void cmdSeeMore_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            cmdSeeMore.Visibility = System.Windows.Visibility.Collapsed;
            gridFullProfile.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetTop(gridFullProfile, gridButtons.CTop());
            client.getAvatarImageCompleted += (s, e2) =>
                {
                    if (e2.Error != null)
                    {
                        if (!string.IsNullOrEmpty(e2.Result))
                            M.setImage(imgAvatar, e2.Result, UriKind.Absolute);
                    }
                };
            client.getAvatarImageAsync(M.myUsernameId);
            client.GetUserDataCompleted += (s, e2) => { updateProfile(e2.Result, true); };
            client.GetUserDataAsync(M.username);
            
        }
        private void updateProfile(SQLReference.UserData uData, bool showRating)
        {

            if (!string.IsNullOrEmpty(M.mySet))
                lblPool.Text = "Pool: " + M.mySet;
            if (showRating)
            {
                lblRating.Text = "Rating: " + uData.Rating;
                imgRating.Width = uData.Rating * 10 + 1;
                if (uData.Rating < 4)
                {
                    imgRating.Fill = new System.Windows.Media.SolidColorBrush(Colors.Blue);
                    lblRatingDescription.Text = "Underpowered";
                }
                else if (uData.Rating < 6)
                {
                    imgRating.Fill = new System.Windows.Media.SolidColorBrush(Colors.Green);
                    lblRatingDescription.Text = "Balanced";
                }
                else
                {
                    imgRating.Fill = new System.Windows.Media.SolidColorBrush(Colors.Red);
                    lblRatingDescription.Text = "Overpowered";
                }

                lblRatingNumber.Text = "Number of Ratings: " + uData.Rating_Number;
            }
            MyPassword = uData.Password;
            System.Text.StringBuilder fakePassword = new System.Text.StringBuilder();
            for (short n = 1; n <= MyPassword.Length; n++)
            {
                fakePassword.Append("*");
            }
            lblProfilePassword.Text = "Password: " + fakePassword.ToString();
            lblProfileEmail.Text = "Email: " + uData.Email;
            lblProfileMessaging.Text = "Allow Messaging: " + (uData.Allow_Messaging ? "Yes" : "No");

        }

        SQLReference.UserData createUserdata()
        {
            SQLReference.UserData uData = new SQLReference.UserData();
            if (lblProfileMessaging.Text == "Allow Messaging: Yes")
            {
                uData.Allow_Messaging = true;
            }
            else
            {
                uData.Allow_Messaging = false;
            }

            uData.Password = MyPassword;
            uData.Email = lblProfileEmail.Text.Replace("Email: ", "");
            return uData;
        }

        private string MyPassword;
        private void cmdChangePassword_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            inputForm.message = "Enter in your old password.";
            inputForm.Closed += changePasswordStep1;
            inputForm.Show();
        }
        private void changePasswordStep1(object sender, EventArgs e)
        {
            inputForm.Closed -= changePasswordStep1;
            if (inputForm.input != MyPassword)
            {
                MessageBox.Show("That was not your old password.");
                return;
            }

            inputForm.message = "Enter your new password:";
            inputForm.Closed += changePasswordStep2;
            inputForm.Show();

            
        }

        private void changePasswordStep2(object sender, EventArgs e)
        {
            inputForm.Closed -= changePasswordStep2;
            if (string.IsNullOrEmpty(inputForm.input))
            {
                MessageBox.Show("You must enter a password!");
                return;
            }
            SQLReference.UserData uData = createUserdata();
            uData.Password  = inputForm.input;
            client.SetUserDataCompleted += endSetData;
            client.SetUserDataAsync(M.username, uData, uData);
        }
        private void endSetData(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            client.SetUserDataCompleted -= endSetData;
            if (e.Error == null)
            {
                MessageBox.Show("Successfully updated profile.");
                updateProfile((SQLReference.UserData)e.UserState, false);
            }
            else
            {
                MessageBox.Show("There was an error updating your profile.");
            }
        }

        private void cmdChangeEmail_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            inputForm.message = "Enter your new Email address:";
            inputForm.Closed += changeEmailStep1;
            inputForm.Show();
        }
        private void changeEmailStep1(object sender, EventArgs e)
        {
            inputForm.Closed -= changeEmailStep1;
            if (string.IsNullOrEmpty(inputForm.input))
            {
                MessageBox.Show("You must enter an email!");
                return;
            }
            SQLReference.UserData uData = createUserdata();
            uData.Email = inputForm.input;
            client.SetUserDataCompleted += endSetData;
            client.SetUserDataAsync(M.username, uData, uData);
        }

        private void cmdChangeMessaging_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            SQLReference.UserData uData = createUserdata();

            if (lblProfileMessaging.Text == "Allow Messaging: Yes")
                uData.Allow_Messaging = false;
            else
                uData.Allow_Messaging = true;
 
            client.SetUserDataCompleted += endSetData;
            client.SetUserDataAsync(M.username, uData, uData);

        }

   
        private void cmdChangePool_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("You have chosen to change pools." + Environment.NewLine + Environment.NewLine + "When you do, all of your current Cards and Decks under your account will be DELETED. Are you sure you want to continue?", "Change Pool", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;
            Views.cldDropDownDialogue ddd = new Views.cldDropDownDialogue("Choose your new pool");
            foreach (string pool in M.PoolOptions)
            {
                ddd.addChoice(pool);
            }

            ddd.Closed += ddd_Closed;
            ddd.Show();
        }

        void ddd_Closed(object sender, EventArgs e)
        {
            
            Views.cldDropDownDialogue ddd = sender as Views.cldDropDownDialogue;
            if (ddd.DialogResult == false || string.IsNullOrEmpty(ddd.chosen)) return;
            Cursor = Cursors.Wait;
            client.deleteAllDecksCompleted += client_deleteAllDecksCompleted;
            client.deleteAllDecksAsync(M.username, ddd.chosen);
        }
        
        void client_deleteAllDecksCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            

            client.deleteAllDecksCompleted -= client_deleteAllDecksCompleted;
            if (e.Error != null)
            {
                MessageBox.Show("There was an error deleting the decks. We will still try to change the pool.");
            }
            client.deleteAllCardsCompleted += client_deleteAllCardsCompleted;
            client.deleteAllCardsAsync(M.username, e.UserState);
        }

        void client_deleteAllCardsCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            client.deleteAllCardsCompleted -= client_deleteAllCardsCompleted;
            if (e.Error != null)
            {
                MessageBox.Show("There was an error deleting the cards. We will still try to change the pool.");;
            }
            client.SetPoolCompleted += client_SetPoolCompleted;
            client.SetPoolAsync(M.username, e.UserState.ToString(), e.UserState);
        }

        void client_SetPoolCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            client.SetPoolCompleted -= client_SetPoolCompleted;
            if (e.Error != null)
            {
                MessageBox.Show("There was an error changing the pool.");
            }
            else
            {
                MessageBox.Show("The pool has been changed to " + e.UserState.ToString() + "." + Environment.NewLine + Environment.NewLine + "The tab will now close. The next time you log in, the changes will be shown.");
                M.mySet = e.UserState.ToString();
                lblPool.Text = "Pool: " + e.UserState.ToString();
                System.Windows.Browser.HtmlPage.Window.Invoke("open", new object[] { "", "_self", "" });
                System.Windows.Browser.HtmlPage.Window.Invoke("close"); 
            }
        }

        private void cmdChangeAvatar_Click(object sender, RoutedEventArgs e)
        {
            inputForm.Closed += finishChangeAvatar;
            inputForm.message = "Enter the URL for your new avatar. Leave blank to delete your avatar.";
            inputForm.Show();

        }
        private void finishChangeAvatar(object sender, EventArgs e)
        {
            inputForm.Closed -= finishChangeAvatar;
            if (inputForm.DialogResult == true)
            {
                MyMainPage.disableNavigation();
               
                SQLReference.Service1ConsoleClient client = new SQLReference.Service1ConsoleClient();
                client.saveImageCompleted += (sender2, e2) =>
                    {
                        MyMainPage.enableNavigation();
                        if (e2.Error == null && !e2.Result.Contains("ERROR"))
                        {
                            if (e2.Result == "Delete")
                                M.setImage(imgAvatar, "blankavatar.jpg", UriKind.Relative);
                            else
                                M.setImage(imgAvatar, e2.Result, UriKind.Absolute);

                        }
                        else if (e2.Result.Contains("ERROR"))
                            MessageBox.Show(e2.Result);
                        else
                            MessageBox.Show("There was an error in changing your avatar. The server might be down.");


                    };
                client.saveImageAsync(M.myUsernameId.ToString(), true, inputForm.input == null ? "" : inputForm.input);
                
            }
        }
    }
        #endregion
}