using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using DuelPortalCS;

namespace DuelPortalCS.Views
{
    public partial class Lobby : Page
    {
        public SQLReference.Service1ConsoleClient sqlCli;
        public bool readyToDuel = false;
        public Action<string> mesDelegate;// = new client_MessageReceivedDelegate(client);
        public Action<string> sendDelegate;// = new sendMessageInvokeDelegate();
        public List<cldChallenge> challengeStack;
        public Lobby()
        {
           
             InitializeComponent();
        }

        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            M.ScreenResized += delegate
            {
                M.ScaleCanvas(LayoutRoot, this);
                
            };


            M.opponent = string.Empty;
            readyToDuel = false;
            challengeStack = new List<cldChallenge>();
            sqlCli = new SQLReference.Service1ConsoleClient();
            sqlCli.loadDeckCompleted += loaded_deck;
            foreach (string deck in M.listOfMyDecks)
            {
                cmbChangeDeck.Items.Add(deck);
            }


            if (M.isLoggedIn)
            {
                M.defaultDeckName = M.GetCookie("default");
                if (!string.IsNullOrEmpty(M.defaultDeckName) && M.listOfMyDecks.Contains(M.defaultDeckName))
                {
                    cmbChangeDeck.SelectedItem = M.defaultDeckName;
                }
                else
                {
                    if (M.noInternet) goto Skip;
                    if (M.listOfMyDecks.Count > 0)
                    {
                        M.defaultDeckName = M.listOfMyDecks[0];
                        cmbChangeDeck.SelectedItem = M.defaultDeckName;
                    }
                    else
                    {
                        lstTournamentHost.SelectedIndex = -1;
                        lstTraditionalHost.SelectedIndex = -1;
                        lstTournamentHost.IsEnabled = false;
                        lstTraditionalHost.IsEnabled = false;
                        cmdHost.IsEnabled = false;
                        cmdDuel.IsEnabled = false;
                    }
                Skip: ;
                }

            }
            else
            {
                lstTournamentHost.IsEnabled = false;
                lstTraditionalHost.IsEnabled = false;
                cmdHost.IsEnabled = false;
                cmdDuel.IsEnabled = false;
            }

            if (M.sock.isConnected)
            M.sock.SendMessage(M.socketSerialize("Server", M.username, "", MessageType.LobbyEnter));
        
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            sqlCli.loadDeckCompleted -= loaded_deck;
            sqlCli.CloseAsync();
            if (M.sock.isConnected)
            {
                if (lstTournamentHost.Items.Contains(M.username))
                {
                    M.sock.SendMessage(M.socketSerialize("Server", M.username, M.username, MessageType.RemoveTournamentHost), true);
                }
                else if (lstTraditionalHost.Items.Contains(M.username))
                {
                    M.sock.SendMessage(M.socketSerialize("Server", M.username, M.username, MessageType.RemoveTraditionalHost), true);
                }
            }
            base.OnNavigatingFrom(e);
        }

        #region "Commands"
        private void cmdHost_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (!M.sock.isConnected)
            { MessageBox.Show("You have been disconnected. Please re-open the chat window."); return; }
            if (cmdHost.Content.ToString() == "Host a Duel")
            {
                if (M.PlayerDeck.CountNumCards() < 40 && !M.noInternet)
                {
                    MessageBox.Show("You have less than 40 cards in your deck. Add " + Convert.ToString(40 - M.PlayerDeck.CountNumCards()) + " more cards to your deck.");
                    return;
                }
                MessageType typeOfHost = default(MessageType);
                if ((bool)rdTraditional.IsChecked)
                {
                    typeOfHost = MessageType.AddTraditionalHost;
                }
                else
                {
                    typeOfHost = MessageType.AddTournamentHost;
                }


                M.sock.SendMessage(M.socketSerialize("Server", M.username, M.username, typeOfHost));
                cmdHost.Content = "Cancel Host";
                cmdDuel.IsEnabled = false;
                cmdWatch.IsEnabled = false;
                cmbChangeDeck.IsEnabled = false;
                rdTournament.IsEnabled = false;
                rdTraditional.IsEnabled = false;
            }
            else
            {
                MessageType typeOfHost = default(MessageType);
                if ((bool)rdTraditional.IsChecked)
                {
                    typeOfHost = MessageType.RemoveTraditionalHost;
                }
                else
                {
                    typeOfHost = MessageType.RemoveTournamentHost;
                }

               M.sock.SendMessage(M.socketSerialize("Server", M.username, M.username, typeOfHost));
                cmdHost.Content = "Host a Duel";
                cmdDuel.IsEnabled = true;
                cmdWatch.IsEnabled = true;
                cmbChangeDeck.IsEnabled = true;
                rdTournament.IsEnabled = true;
                rdTraditional.IsEnabled = true;
            }

        }
        private void cmdDuel_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (!M.sock.isConnected)
            { MessageBox.Show("You have been disconnected. Please re-open the chat window."); return; }
            if (lstTournamentHost.SelectedIndex == -1 && lstTraditionalHost.SelectedIndex == -1)
                return;
            if (M.PlayerDeck.CountNumCards() < 40 && !M.noInternet)
            {
                MessageBox.Show("You have less than 40 cards in your deck. Add " + Convert.ToString(40 - M.PlayerDeck.CountNumCards()) + " more cards to your deck.");
                return;
            }
            M.opponent = cmdDuel.Content.ToString().Replace("Enter Duel with ", "");
            M.sock.SendMessage(M.socketSerialize(M.opponent, M.username, M.username, MessageType.Challenge));
            cmdDuel.IsEnabled = false;
            cmdHost.IsEnabled = false;
            cmdWatch.IsEnabled = false;
            cmbChangeDeck.IsEnabled = false;
            rdTournament.IsEnabled = false;
            rdTraditional.IsEnabled = false;
        }
        private void cmdWatch_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (!M.sock.isConnected)
            { MessageBox.Show("You have been disconnected. Please re-open the chat window."); return; }
            if (lstCurrentDuel.SelectedIndex > -1)
            {
                M.myRoomID = lstCurrentDuel.SelectedItem.ToString();
            }
            else
            {
                return;
            }
            string[] players = getTwoPlayerToRoom(M.myRoomID);
            M.WatcherMySide = players[0];
            M.WatcherOtherSide = players[1];
            M.IamWatching = true;
         //   M.sock.SendMessage(M.socketSerialize("Server", M.username, M.myRoomID, MessageType.Leave));

            this.NavigationService.Navigate(new System.Uri("/DuelFieldNew", UriKind.Relative));

        }
        #endregion

        #region "Form Maintenance"
        private void lstTournamentHost_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstTournamentHost.SelectedIndex == -1)
                return;
            cmdDuel.Content = "Enter Duel with " + lstTournamentHost.SelectedItem.ToString();

        }
        private void lstTraditionalHost_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstTraditionalHost.SelectedIndex == -1)
                return;
            cmdDuel.Content = "Enter Duel with " + lstTraditionalHost.SelectedItem.ToString();

        }
        private void lstCurrentDuel_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstCurrentDuel.SelectedIndex == -1)
                return;
            cmdWatch.Content = "Watch " + decodeRoomID(lstCurrentDuel.SelectedItem.ToString());
        }
        public string decodeRoomID(string roomid)
        {
            int bar = roomid.IndexOf("&&");
            string firstpart = roomid.Substring(0, bar);
            string secondpart = roomid.Substring(bar + 2, roomid.Length - bar - 2);
            return firstpart + " VS " + secondpart;
        }
        public string[] getTwoPlayerToRoom(string roomid)
        {
            int bar = roomid.IndexOf("&&");
            string firstpart = roomid.Substring(0, bar);
            string secondpart = roomid.Substring(bar + 2, roomid.Length - bar - 2);
            string[] parts = {
			    firstpart,
			    secondpart
		        };
            return parts;
        }
        private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Document.SetProperty("title", "Duel Portal Online");

        }
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (MyMainPage.floatChatReal == null)
                MyMainPage.initializeFloatChat();

            System.Windows.Media.GeneralTransform gt = lstTraditionalHost.TransformToVisual(Application.Current.RootVisual);
            Point offset = gt.Transform(new Point(lstTraditionalHost.Width + 30, 0));
            MyMainPage.floatChatReal.Position = offset;

            MyMainPage.refreshFloatChat(lstTraditionalHost.ActualHeight / 1.5, lstTraditionalHost.ActualHeight);

        }
        private MainPage MyMainPage
        {
            get
            {
                return ((MainPage)((Canvas)((Border)((Frame)Parent).Parent).Parent).Parent);
            }
        }
        #endregion

        #region "Decks"
        public void CheckDeck(List<int> currentDeckIDs, List<int> extraDeckIDs, List<int> sideDeckIDs)
        {
            bool failedBanlist = false;
            Dictionary<int, int> idToCopies = new Dictionary<int, int>();
            foreach (int id in currentDeckIDs)
            {
                if (idToCopies.ContainsKey(id))
                    idToCopies[id]++;
                else
                    idToCopies.Add(id, 1);
            }
            foreach (int id in extraDeckIDs)
            {
                if (idToCopies.ContainsKey(id))
                    idToCopies[id]++;
                else
                    idToCopies.Add(id, 1);
            }
            foreach (int id in sideDeckIDs)
            {
                if (idToCopies.ContainsKey(id))
                    idToCopies[id]++;
                else
                    idToCopies.Add(id, 1);
            }
            
            foreach (KeyValuePair<int, int> kv in idToCopies)
            {
                if (kv.Value > M.CardStats[kv.Key].Limit)
                    failedBanlist = true;
            }

            cmdHost.IsEnabled = true;
            cmdDuel.IsEnabled = true;
            cmbChangeDeck.IsEnabled = true;
            if (failedBanlist)
            {
                /*
                if (checkingBanlist)
                {
                    MessageBox.Show("Your deck does not follow the Tournament banlist. You must host or join a Traditional duel.");
                }*/
             
                lstTraditionalHost.IsEnabled = true;
                lstTournamentHost.SelectedIndex = -1;
                lstTournamentHost.IsEnabled = false;
                noFiringRadios = true;
                rdTraditional.IsChecked = true;
                noFiringRadios = false;
            }
            else
            {
                lstTraditionalHost.SelectedIndex = -1;
                lstTraditionalHost.IsEnabled = false;
                lstTournamentHost.IsEnabled = true;
                noFiringRadios = true;
                rdTournament.IsChecked = true;
                noFiringRadios = false;
            }
        }
        bool noFiringRadios = false;
        public void makeDeck(string serializedString, bool checkingBanlist)
        {
            List<int> currentDeckIDs = new List<int>();
            List<int> extraDeckIDs = new List<int>();
            List<int> sideDeckIDs = new List<int>();

            M.PlayerDeck.ClearAndAdd();
            M.PlayerEDeck.ClearAndAdd();

            M.realDeckIDs.ClearAndAdd();
            M.realEDeckIDs.ClearAndAdd();
            M.realSideDeckIDs.ClearAndAdd();

            string[] deserializedArray = serializedString.Split('|');
            bool sideDeckSwitch = false;
            int id = 0;


            for (int n = 0; n < deserializedArray.Length; n++)
            {
                if (deserializedArray[n] == "&")
                {
                    sideDeckSwitch = true;
                    continue;
                }
                id = M.findIndexFromTrueID(Convert.ToInt32(deserializedArray[n]));
                if (id == 0) { continue; }
                if (sideDeckSwitch == false)
                {
                    //Belongs in Main Deck
                    if (M.BelongsInExtra(M.CardStats[id]) == false)
                    {

                        currentDeckIDs.Add(id);
                    }
                    else
                    {
                        extraDeckIDs.Add(id);
                    }
                }
                else
                {
                    sideDeckIDs.Add(id);
                }
            }



            for (int n = 1; n <= currentDeckIDs.Count; n++)
            {
                M.realDeckIDs.Add(currentDeckIDs[n - 1]);
                M.PlayerDeck.Add(M.CardStats[currentDeckIDs[n - 1]].toTrueStats());
            }


            for (int n = 1; n <= extraDeckIDs.Count; n++)
            {
                M.realEDeckIDs.Add(extraDeckIDs[n - 1]);
                M.PlayerEDeck.Add(M.CardStats[extraDeckIDs[n - 1]].toTrueStats());
            }


            for (int n = 1; n <= sideDeckIDs.Count; n++)
            {
                M.realSideDeckIDs.Add(sideDeckIDs[n - 1]);
            }

            CheckDeck(currentDeckIDs, extraDeckIDs, sideDeckIDs);
        }
        private void cmbChangeDeck_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (cmbChangeDeck.SelectedItem != null && cmbChangeDeck.SelectedItem.ToString() != "System.Windows.Controls.ComboBoxItem")
            {
                cmbChangeDeck.IsEnabled = false;
                cmdHost.IsEnabled = false;
                cmdDuel.IsEnabled = false;
                sqlCli.loadDeckAsync(M.username, cmbChangeDeck.SelectedItem.ToString(), false);
            }
        }
        private void loaded_deck(object sender, SQLReference.loadDeckCompletedEventArgs e)
        {
           
           if (e.Error == null && !string.IsNullOrEmpty(e.Result))
           {
               bool checkBanlist = Convert.ToBoolean(e.UserState);

               makeDeck(e.Result, checkBanlist); //data from deck
               M.SetCookie("default", cmbChangeDeck.SelectedItem.ToString());
           }
           else
           {
               MessageBox.Show("There was a problem loading this deck (or the server is down).");
           }
        }
        private void rdTraditional_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (noFiringRadios)
                return;
            if (rdTraditional.IsChecked == true)
            {
                lstTournamentHost.SelectedIndex = -1;
                lstTournamentHost.IsEnabled = false;

                lstTraditionalHost.IsEnabled = true;

            }

        }
        private void rdTournament_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
          
            if (rdTournament == null)
                return;
            if (noFiringRadios)
                return;
            cmdHost.IsEnabled = false;
            cmdDuel.IsEnabled = false;
            if (rdTournament.IsChecked == true && cmbChangeDeck.SelectedItem != null)
            {
                sqlCli.loadDeckAsync(M.username, cmbChangeDeck.SelectedItem.ToString(), true);

            }

        }
        #endregion



        #region "Communications"
        public void cldChallenge_Closed(object sender, EventArgs e)
        {
            cldChallenge challengeForm = (cldChallenge)sender;
            challengeStack.Remove(challengeForm);
            if (challengeForm.challenger == M.opponent)
            {
                for (int n = 0; n < challengeStack.Count; n++)
                {
                    challengeStack[n].DialogResult = false;

                }
                readyToDuel = true;
               List<string> twousers = new List<string>();
                twousers.Add(M.username);
                twousers.Add(M.opponent);
                twousers.Sort();
                M.myRoomID = twousers[0] + "&&" + twousers[1];
                M.sock.SendMessage(M.socketSerialize(challengeForm.challenger, M.username, M.username, MessageType.Accept));
            
            }
            else
            {
                M.sock.SendMessage(M.socketSerialize(challengeForm.challenger, M.username, M.username, MessageType.Reject));
            }
        }
        public void HandleSocketMessage(SocketMessage msg)
        {
            switch (msg.mType)
            {
                case MessageType.AddTournamentHost:
                    if (lstTournamentHost.Items.Contains(msg.data) == false) lstTournamentHost.Items.Add(msg.data);
                    break;
                case MessageType.AddTraditionalHost:
                    if (lstTraditionalHost.Items.Contains(msg.data) == false) lstTraditionalHost.Items.Add(msg.data);
                    break;
                case MessageType.RemoveTournamentHost:
                    lstTournamentHost.Items.Remove(msg.data);
                    break;
                case MessageType.RemoveTraditionalHost:
                    lstTraditionalHost.Items.Remove(msg.data);
                    break;
                case MessageType.AddDuel:
                    if (lstCurrentDuel.Items.Contains(msg.data) == false) { lstCurrentDuel.Items.Add(msg.data); }
                    break;
                case MessageType.RemoveDuel:
                    lstCurrentDuel.Items.Remove(msg.data);
                    break;
                case MessageType.Challenge:
                    if (readyToDuel) //Already accepted a duel, instant rejection
                    {
                        M.sock.SendMessage(M.socketSerialize(msg.From, M.username, M.username, MessageType.Reject));
                    }
                    else
                    {
                        System.Windows.Browser.HtmlPage.Document.SetProperty("title", "Message received");
                        Views.cldChallenge newChallengeForm = new Views.cldChallenge(msg.From);

                        newChallengeForm.Closed += new EventHandler(cldChallenge_Closed);
                        challengeStack.Add(newChallengeForm);
                        newChallengeForm.Show();
                    }
                    break;
                case MessageType.Accept:
                    readyToDuel = true;
                    List<string> twousers2 = new List<string>();
                    twousers2.Add(M.username);
                    twousers2.Add(M.opponent);
                    twousers2.Sort();
                    M.myRoomID = twousers2[0] + "&&" + twousers2[1];
                    M.IamWatching = false;
                    NavigationService.Navigate(new System.Uri("/DuelFieldNew", UriKind.Relative));
                    break;

                case MessageType.Reject:
                    MessageBox.Show(msg.data + " has rejected your challenge.");
                    cmdDuel.IsEnabled = true;
                    cmdHost.IsEnabled = true;
                    cmdWatch.IsEnabled = true;
                    cmbChangeDeck.IsEnabled = true;
                    rdTournament.IsEnabled = true;
                    rdTraditional.IsEnabled = true;
                    break;

                case MessageType.DuelLeave:
                    RemoveFromCurrentDuels(msg.data);
                    break;
                case MessageType.Leave:
                    lstTraditionalHost.Items.Remove(msg.data);
                    lstTournamentHost.Items.Remove(msg.data);
                    RemoveFromCurrentDuels(msg.data);
                    break;
            }
        }
        void RemoveFromCurrentDuels(string user)
        {
            for (int n = 0; n <= lstCurrentDuel.Items.Count; n++)
            {
                string[] twoPlayers = getTwoPlayerToRoom(lstCurrentDuel.Items[n].ToString());
                if (twoPlayers != null && twoPlayers.Length == 2 &&
                    (twoPlayers[0] == user || twoPlayers[1] == user))
                {
                    lstCurrentDuel.Items.RemoveAt(n);
                    return;

                }
            }
        }

        #endregion



    }

   

   
}