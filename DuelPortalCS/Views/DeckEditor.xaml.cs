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
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace DuelPortalCS.Views
{
    public partial class DeckEditor : Page
    {
        public SQLReference.Service1ConsoleClient SQLcli;
        public List<string> DeckBuffer = new List<string>();// = new Module1.ShallowCardDetails[10001];
        public int NumberOfPages;
        public int BufPageNum;
        public LegacyDeck LegacyForm = new LegacyDeck();
        public cldInput inputForm = new cldInput("");
        public short startingCombinedDeckNumber;
        public bool automaticDoubleClick = false;
        public TranslateTransform SideDeckTransform;
        public System.Threading.ManualResetEvent waitForPublicCards;
        public DeckEditor()
        {
            MouseLeftButtonUp += DeckEditor_MouseLeftButtonUp;
            InitializeComponent();
        }

        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (string.IsNullOrEmpty(Module1.username) || Module1.username.Contains("portalguest"))
            {
                MessageBox.Show("You cannot use this function until you are Logged In.");
                this.NavigationService.Navigate(new System.Uri("/Home", UriKind.Relative));
                return;
            }
            this.NavigationCacheMode = System.Windows.Navigation.NavigationCacheMode.Disabled;
            cmdSearch.Focus();
            getNewsBegin();

            ctxtSideDeck.myArea = ContextMenu.Area.Side_Deck;
            ctxtSideDeck.onLoaded();
            SideDeckTransform = new TranslateTransform();
            ctxtSideDeck.RenderTransform = SideDeckTransform;
            ctxtSideDeck.Visibility = System.Windows.Visibility.Collapsed;
            ctxtSideDeck.Item_Clicked += SideDeckItem_Clicked;
            LegacyForm.Closed += Legacy_Closed;
            SQLcli.NewDeckCompleted += createdNewDeck;
            SQLcli.SaveDeckCompleted += client_SaveComplete;
            SQLcli.loadDeckCompleted += doneLoadingDeck;
            //SQLcli.getDefaultDeckNameOperationCompleted += defaultDeckName_Get;
            SQLcli.deleteDeckCompleted += SQLcli_deleteDeckAsyncCompleted;

            List<string> listOfSets = new List<string>();
            for (int n = 1; n <= Module1.TotalCards; n++)
            {
                if (Module1.CardStats[n].SpecialSet != null && !listOfSets.Contains(Module1.CardStats[n].SpecialSet))
                    listOfSets.Add(Module1.CardStats[n].SpecialSet);
            }
            listOfSets.Sort();
            foreach (string setName in listOfSets)
            { cmbSet.Items.Add(setName); }

          

            if (Module1.mySet == "LCCG") { lblRollOver.Visibility = System.Windows.Visibility.Visible; }
            // AddHandler SQLcli.loadDefaultCompleted, AddressOf doneLoadingDefaultDeck
            if (Module1.sideDecking == true)
            {
                Module1.warnOnExitMessage = "You're currently sidedecking for another duel. If you exit now, you cannot return to your match. Exit anyway?";
                cmdSearch.IsEnabled = false;
                lstBySetList.Items.Clear();
                cmdSave.IsEnabled = false;
                cmdSaveAs.IsEnabled = false;
                cmbLoadDeck.IsEnabled = false;
                cmdPrevPage.IsEnabled = false;
                cmdNextPage.IsEnabled = false;
                cmdLegacyLoad.IsEnabled = false;
                cmdDeleteDeck.IsEnabled = false;
                cmdShareSpecific.IsEnabled = false;
                cmdShareSet.IsEnabled = false;
                changeBanlist(Module1.traditionalMode);
                chkTournament.IsEnabled = false;
                chkTraditional.IsEnabled = false;
                cmdDoneSideDecking.Visibility = System.Windows.Visibility.Visible;
                cmdClear.IsEnabled = false;
                for (short n = 1; n <= Module1.realDeckLength; n++)
                {
                    addToExtraOrMainDeck(Module1.realDeckIDs[n], true);
                }
                for (short n = 1; n <= Module1.realEDeckLength; n++)
                {
                    addToExtraOrMainDeck(Module1.realEDeckIDs[n], true);
                }
                startingCombinedDeckNumber = (short)(Module1.realDeckLength + Module1.realEDeckLength);
                for (short n = 1; n <= Module1.realSideDeckLength; n++)
                {
                    lstSideDeckList.Items.Add(Module1.CardStats[Module1.realSideDeckIDs[n]].Name);
                    Image refer = GetSideDeckPictureBox(n);
                    UpdatePictureBox(ref refer, Module1.CardStats[Module1.realSideDeckIDs[n]]);
                }
            }
            else
            {
                foreach (string deck in Module1.listOfMyDecks)
                {
                    cmbLoadDeck.Items.Add(deck);
                }
                changeBanlist(false);
                Module1.defaultDeckName = Module1.GetCookie("default");
               
                //if (string.IsNullOrEmpty(Module1.defaultDeckName) || !Module1.listOfMyDecks.Contains(Module1.defaultDeckName))
                //{
                //    if (Module1.listOfMyDecks.Count > 0)
                //       SQLcli.loadDeckAsync(Module1.username, Module1.listOfMyDecks[0]);

                //}
                //else
                //{
                //    SQLcli.loadDeckAsync(Module1.username, Module1.defaultDeckName);
                //}
            }

        }
        private short cardsOverSidedeckLimit()
        {
            return (short)(lstCurrentDeckList.Items.Count + lstExtraDeckList.Items.Count - startingCombinedDeckNumber);
        }
        public Image GetExtraDeckPictureBox(short index)
        {
            return (Image)Root.FindName("imgExtra" + index);
        }
        public Image GetMainDeckPictureBox(short index)
        {
            return (Image)Root.FindName("Image" + index);
        }
        public Image GetSideDeckPictureBox(short index)
        {
            return (Image)Root.FindName("imgSide" + index);
        }
        public void MsgBox(string text, System.Windows.MessageBoxButton msgtype = MessageBoxButton.OK)
        {
            MessageBox.Show(text, "", msgtype);
        }
        public void clearSet()
        {
            lstBySetList.Items.Clear();
        }


        private bool addToExtraOrMainDeck(int id, bool noMessage)
        {
            short match = 0;
            short n = 0;
            if (Module1.CardStats[id].Name == null)
            { return false; }
            for (n = 0; n <= lstBanned.Items.Count - 1; n++)
            {
                if (Module1.CardStats[id].Name == lstBanned.Items[n].ToString())
                {
                    if (noMessage == false)
                        MsgBox(lstBanned.Items[n] + " is banned in this list. No cards are allowed in your deck.");
                    return false;
                }

            }

            for (n = 0; n <= lstLimited.Items.Count - 1; n++)
            {
                for (int m = 0; m <= lstCurrentDeckList.Items.Count - 1; m++)
                {
                    if (lstCurrentDeckList.Items[m] == lstLimited.Items[n])
                    {
                        match += 1;
                    }
                }
                for (int m = 0; m <= lstExtraDeckList.Items.Count - 1; m++)
                {
                    if (lstExtraDeckList.Items[m] == lstLimited.Items[n])
                    {
                        match += 1;
                    }
                }

                if (match >= 2)
                {
                    if (noMessage == false)
                        MsgBox(lstLimited.Items[n] + " is limited in this list. Only 1 copy is allowed in your deck.");
                    return false;
                }
                match = 0;
            }
            match = 0;

            for (n = 0; n <= lstSemi.Items.Count - 1; n++)
            {
                for (int m = 0; m <= lstCurrentDeckList.Items.Count - 1; m++)
                {
                    if (lstCurrentDeckList.Items[m] == lstSemi.Items[n])
                    {
                        match += 1;
                    }
                }
                for (int m = 0; m <= lstExtraDeckList.Items.Count - 1; m++)
                {
                    if (lstExtraDeckList.Items[m] == lstSemi.Items[n])
                    {
                        match += 1;
                    }
                }
                if (match >= 3)
                {
                    if (noMessage == false)
                        MsgBox(lstSemi.Items[n] + " is semi-limited in this list. Only 2 copies are allowed in your deck.");
                    return false;
                }
                match = 0;
            }
            match = 0;
            //    If lstBySetList.SelectedIndex = -1 And sideDecking = False Then Exit Sub
            for (n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
            {
                if (lstCurrentDeckList.Items[n].ToString() == Module1.CardStats[id].Name)
                    match += 1;
            }
            for (n = 0; n <= lstExtraDeckList.Items.Count - 1; n++)
            {
                if (lstExtraDeckList.Items[n].ToString() == Module1.CardStats[id].Name)
                    match += 1;
            }
            if (match >= 3)
            {
                if (noMessage == false)
                    MsgBox("You already have 3 of that card in your deck/extra deck.");
                return false;
            }
            Image img = default(Image);
            if (Module1.CardStats[id].Type.Contains("Xyz") || Module1.CardStats[id].Type.Contains("Synchro") || Module1.CardStats[id].Type.Contains("Fusion"))
            {
                if (lstExtraDeckList.Items.Count == 15)
                {
                    if (noMessage == false)
                        MsgBox("You cannot have more than 15 cards in your Extra Deck.");
                    return false;
                }
                lstExtraDeckList.Items.Add(Module1.CardStats[id].Name);
                img = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count);
                UpdatePictureBox(ref img, Module1.CardStats[id]);
            }
            else
            {
                if (lstCurrentDeckList.Items.Count == 99)
                {
                    if (noMessage == false)
                        MsgBox("You cannot have more than 99 cards in your Main Deck.");
                    return false;
                }
                lstCurrentDeckList.Items.Add(Module1.CardStats[id].Name);
                if (lstCurrentDeckList.Items.Count > 45)
                {
                    CountUpDeckStatistics();
                    return true;
                }
                img = GetMainDeckPictureBox((short)(lstCurrentDeckList.Items.Count));
                UpdatePictureBox(ref img, Module1.CardStats[id]);
            }
            CountUpDeckStatistics();
            return true;
        }


        public void CountUpDeckStatistics()
        {
            int MainDeckIntCount = 0;
            int SpellIntCount = 0;
            int MonsterIntCount = 0;
            int TrapIntCount = 0;
           
            int TheID = 0;
            short n = 0;


            if (lstCurrentDeckList.Items.Count != 0)
            {
                for (n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
                {
                    TheID = Module1.findID(lstCurrentDeckList.Items[n].ToString());
                    if (!string.IsNullOrEmpty(Module1.CardStats[TheID].ATK))
                        MonsterIntCount = MonsterIntCount + 1;
                    if (Module1.CardStats[TheID].Attribute == "Spell")
                        SpellIntCount = SpellIntCount + 1;
                    if (Module1.CardStats[TheID].Attribute == "Trap")
                        TrapIntCount = TrapIntCount + 1;
                    MainDeckIntCount = MainDeckIntCount + 1;
                }

            }



            frameMainDeck.Text = "Monsters: " + Convert.ToString(MonsterIntCount) + " Spells : " + Convert.ToString(SpellIntCount) + " Traps: " + Convert.ToString(TrapIntCount) + " Total: " + Convert.ToString(MainDeckIntCount)
                + Environment.NewLine + "Extra: " + Convert.ToString(lstExtraDeckList.Items.Count) + " Side: " + Convert.ToString(lstSideDeckList.Items.Count);



        }

        private void SideDeck_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image theImg = (Image)sender;
            Image img = default(Image);
            for (short n = 1; n <= 15; n++)
            {
                img = (Image)Root.FindName("imgSide" + n);
                if (!object.ReferenceEquals(img, theImg))
                {
                    Canvas.SetZIndex(img, 0);
                }
                else
                {
                    Canvas.SetZIndex(theImg, 1);
                }
            }
        }

        private void ExtraDeck_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image theImg = (Image)sender;
            Image img = default(Image);
            for (short n = 1; n <= 15; n++)
            {
                img = (Image)Root.FindName("imgExtra" + n);
                if (!object.ReferenceEquals(img, theImg))
                {
                    Canvas.SetZIndex(img, 0);
                }
                else
                {
                    Canvas.SetZIndex(theImg, 1);
                }
            }
        }
        private void MainDeck_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image theImg = (Image)sender;
            short theImgIndex = Convert.ToInt16(theImg.Name.Substring(5, theImg.Name.Length - 5));
            Image img = default(Image);
           if (theImgIndex >= 1 && theImgIndex <= 15)
           {
                    for (short n = 1; n <= 15; n++)
                    {
                        img = (Image)Root.FindName("Image" + n);
                        if (!object.ReferenceEquals(img, theImg))
                        {
                            Canvas.SetZIndex(img, 0);
                        }
                        else
                        {
                            Canvas.SetZIndex(theImg, 1);
                        }
                    }
           }
           else if (theImgIndex >= 16 && theImgIndex <= 30){
                    for (short n = 16; n <= 30; n++)
                    {
                        img = (Image)Root.FindName("Image" + n);
                        if (!object.ReferenceEquals(img, theImg))
                        {
                            Canvas.SetZIndex(img, 0);
                        }
                        else
                        {
                            Canvas.SetZIndex(theImg, 1);
                        }
                    }
           }
           else if (theImgIndex >= 31 && theImgIndex <= 45){
                    for (short n = 31; n <= 45; n++)
                    {
                        img = (Image)Root.FindName("Image" + n);
                        if (!object.ReferenceEquals(img, theImg))
                        {
                            Canvas.SetZIndex(img, 0);
                        }
                        else
                        {
                            Canvas.SetZIndex(theImg, 1);
                        }
                    }

               }
          

        }


        public void MainDeck_MouseClick(System.Object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            short indx = Convert.ToInt16(img.Name.Substring(5, img.Name.Length - 5));
            if (indx > lstCurrentDeckList.Items.Count)
                return;
            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = true;
                lstCurrentDeckList_MouseLeftButtonUp(sender, e);
            }
            else
                lstCurrentDeckList.SelectedIndex = indx - 1;

        }

        private void ExtraDeck_MouseClick(System.Object sender, MouseButtonEventArgs e)
        {

            Image img = (Image)sender;
            short indx = Convert.ToInt16(img.Name.Substring(8, img.Name.Length - 8));
            if (indx > lstExtraDeckList.Items.Count)
                return;
            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = true;
                lstExtraDeckList_MouseLeftButtonUp(sender, e);
            }
            else
                lstExtraDeckList.SelectedIndex = indx - 1;
        }

        private void SideDeck_MouseClick(System.Object sender, MouseButtonEventArgs e)
        {

            Image img = (Image)sender;
            short indx = Convert.ToInt16(img.Name.Substring(7, img.Name.Length - 7));
            if (indx > lstSideDeckList.Items.Count)
                return;
            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = true;
                lstSideDeckList_MouseLeftButtonUp(sender, e);
            }
            else
                lstSideDeckList.SelectedIndex = indx - 1;
        }





        private void cmdClear_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult input = MessageBox.Show("Are you sure you want to clear your deck?", "", MessageBoxButton.OKCancel);
            //inputBox("Are you sure you want to clear your deck?")
            if (input == MessageBoxResult.OK)
            {
                clearDeck();
            }

        }
        private void clearDeck()
        {
            lstCurrentDeckList.Items.Clear();
            lstExtraDeckList.Items.Clear();
            lstSideDeckList.Items.Clear();
            CountUpDeckStatistics();
            for (short n = 1; n <= 45; n++)
            {
                Image img = GetMainDeckPictureBox(n);
                img.Source = null;
            }
            for (short n = 1; n <= 15; n++)
            {
                Image img = GetExtraDeckPictureBox(n);
                img.Source = null;
            }
            for (short n = 1; n <= 15; n++)
            {
                Image img = GetSideDeckPictureBox(n);
                img.Source = null;
            }
            Module1.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?";
        }
        private string inputBox(string prompt)
        {
            string input = System.Windows.Browser.HtmlPage.Window.Invoke("prompt", new string[] {
			prompt,
			""
		}) as string;
            return input;
        }
        public void changeBanlist(bool toTraditional)
        {
            lstBanned.Items.Clear();
            lstSemi.Items.Clear();
            lstLimited.Items.Clear();
            if (toTraditional == false)
            {
                if (Module1.tournamentBanList != null)
                {
                    banlist_Changed(Module1.tournamentBanList);
                }
                else
                {
                    SQLcli = new SQLReference.Service1ConsoleClient();
                    SQLcli.InetConnectionCompleted += SQLcli_BanlistChange;
                    SQLcli.InetConnectionAsync("http://dl.dropbox.com/u/15387447/Tournament.txt");

                }
            }
        }
        public void SQLcli_BanlistChange(object sender, SQLReference.InetConnectionCompletedEventArgs e)
        {
            SQLcli.InetConnectionCompleted -= SQLcli_BanlistChange;
            //  SQLcli.CloseAsync()

            if (e.Error != null || e.Result == null)
                return;
            string thestring = Module1.ByteArrayToString(e.Result);
            banlist_Changed(thestring);
        }
        public void banlist_Changed(string thestring)
        {
            Module1.tournamentBanList = thestring;
            int temp = thestring.IndexOf("<!");
            try
            {
                thestring = thestring.Substring(0, temp) + Environment.NewLine;
            }
            catch
            {
            }

            int leftposition = -1;
            int rightposition = 0;
            string currentline = null;
            short listswitch = 0;
            //Banned = 0, Limited = 1, Semi = 2
            do
            {
                try
                {
                    rightposition = thestring.IndexOf(Environment.NewLine, leftposition + 1);
                    currentline = thestring.Substring(leftposition + 1, rightposition - leftposition - 1);
                    leftposition = rightposition;
                    currentline = currentline.Trim();
                    if (currentline == "~Banned~")
                        listswitch = 0;
                    if (currentline == "~Limited~")
                        listswitch = 1;
                    if (currentline == "~Semi-Limited~")
                        listswitch = 2;
                    switch (listswitch)
                    {

                        case 0:
                            if (!string.IsNullOrEmpty(currentline))
                                lstBanned.Items.Add(currentline);
                            break;
                        case 1:
                            if (!string.IsNullOrEmpty(currentline))
                                lstLimited.Items.Add(currentline);
                            break;
                        case 2:
                            if (!string.IsNullOrEmpty(currentline))
                                lstSemi.Items.Add(currentline);

                            break;
                    }
                }
                catch (Exception )
                {
                    break; // TODO: might not be correct. Was : Exit Do
                }
            } while (true);
        }
        private void chkTraditional_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkTraditional.IsChecked == false)
                return;
            changeBanlist(true);
            chkTraditional.IsEnabled = false;
            chkTournament.IsChecked = false;
            chkTournament.IsEnabled = true;
        }

        private void chkTournament_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkTournament == null)
                return;
           if (chkTournament.IsChecked == false)
                return;
           changeBanlist(false);
       chkTournament.IsEnabled = false;
      chkTraditional.IsChecked = false;
       chkTraditional.IsEnabled = true;
         
        }
        public void getNewsBegin()
        {
            SQLcli = new SQLReference.Service1ConsoleClient();
            SQLcli.InetConnectionCompleted += getNewsEnd;
            SQLcli.InetConnectionAsync("http://dl.dropbox.com/u/15387447/FreeNews.txt");

        }
        public void getNewsEnd(object sender, SQLReference.InetConnectionCompletedEventArgs e)
        {
            //  SQLcli.CloseAsync()
            SQLcli.InetConnectionCompleted -= getNewsEnd;

            if (e.Result != null)
            {
                txtNews.Text += Module1.ByteArrayToString(e.Result);

            }
            else
            {
                txtNews.Text = "Error getting news.";
            }
        }

        private void imgExtra1_ImageFailed(System.Object sender, System.Windows.ExceptionRoutedEventArgs e)
        {
        }

        private void lstBySetList_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                if (lstBySetList.SelectedItem == null)
                    return;
                int id = Module1.findID(lstBySetList.SelectedItem.ToString());
                addToExtraOrMainDeck(id, false);
                if (Module1.sideDecking == false) { Module1.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; }
                if (Module1.sideDecking)
                {
                    lstBySetList.Items.RemoveAt(lstBySetList.SelectedIndex);

                }
            }
        }

        private void lstBySetList_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstBySetList.SelectedIndex == -1)
                return;
            

            try
            {
                cardID = Module1.findID(lstBySetList.SelectedItem.ToString());
                if (cardID == 0)
                    throw new ArgumentException();
                lblCardName.Text = Module1.CardStats[cardID].Name;

                //get field 4 card type string
                lblCardType.Text = Module1.CardStats[cardID].Type.NotDisplayEffect();

                //get field 5 attribute name string
                lblCardAttribute.Text = Module1.CardStats[cardID].Attribute;


                //get field 6 card level string
                lblCardLevel.Text = Module1.CardStats[cardID].Level;

                //skip field 7
                if (lblCardAttribute.Text == "Spell" || lblCardAttribute.Text == "Trap")
                {
                    lblCardType.Text = Module1.CardStats[cardID].Type;
                }

                //get field 8 attack string
                lblCardATK.Text = Module1.CardStats[cardID].ATK;

                //get field 9 card defense string
                lblCardDEF.Text = Module1.CardStats[cardID].DEF;

                //get field 10 card lore string
                lblCardLore.Text = Module1.CardStats[cardID].Lore;
                lblSetName.Text = Module1.CardStats[cardID].SpecialSet;

                //get Picture
                UpdatePictureBox(ref imgDeckEditorImg, Module1.CardStats[cardID]);
                //d.Invoke(imgDeckEditorImg, Module1.CardStats[cardID])
            }
            catch (ArgumentException)
            {
                Module1.setImage(ref imgDeckEditorImg, "token.jpg", UriKind.Relative);
                lblCardName.Text = "???";
                lblCardType.Text = "???";
                lblCardAttribute.Text = "???";
                lblCardLevel.Text = "???";
                lblCardType.Text = "???";
                lblCardATK.Text = "???";
                lblCardDEF.Text = "???";
                lblCardLore.Text = "Card not found";
            }
        }

        private void cmdSearch_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            //if (radPublic.IsChecked == true && Module1.loadedPublic == false)
            //{
            //    loadingWindow = new cldLoading("Loading Public Cards...");
            //    loadingWindow.Show();
            //   // System.Threading.Thread t = new System.Threading.Thread(loadPublic);
            //   // t.Start();
            //    loadPublic();
            //}
             if (radPublic.IsChecked == true)
            {
                searchCards(true);
            }
            else
                searchCards(false);

        }
       
     

        private void searchCards(bool isPublic)
        {
            int n = 0;
           // string[] multipleNamesAND = txtName.Text.Split(new char[]{'A', 'N', 'D'});
            //string[] multipleNamesOR = txtName.Text.Split(new char[]{'A', 'N', 'D'});
           // string[] multipleLoresAND = txtDesc.Text.Split('+');

            DeckBuffer.Clear();
            DeckBuffer.Add("");

            lstBySetList.Focus();
            clearSet();


            if (isPublic)
            {
                cmdShareSpecific.IsEnabled = false;
                cmdShareSet.IsEnabled = false;
            }
            else
            {
                cmdShareSet.IsEnabled = true;
                cmdShareSpecific.IsEnabled = true;
            }


            int bufnum = 1;
            
            if (!string.IsNullOrEmpty(txtName.Text))
                txtName.Text = txtName.Text.Trim().ToLower();
            for (n = 1; n <= Module1.TotalCards; n++)
            {
                if (Module1.CardStats[n].Name == null)
                    continue;
                if (string.IsNullOrEmpty(Module1.CardStats[n].Name))
                    continue;


                if (!string.IsNullOrEmpty(txtName.Text))
                {
                   // if (multipleNames.Length > 1)
                   // {
                   //     if (

                 //   }
                   // else
                   // {
                        if (Module1.CardStats[n].Name.IndexOf(txtName.Text, StringComparison.OrdinalIgnoreCase) == -1)
                            continue;
                   // }
                }

                if (isPublic && Module1.CardStats[n].Creator != "PUB" ||
                    !isPublic && Module1.CardStats[n].Creator == "PUB")
                    continue;
         
                if (cmbSet.SelectionBoxItem.ToString() != "All")
                {
                    if (Module1.CardStats[n].SpecialSet != cmbSet.SelectionBoxItem.ToString())
                    {
                        continue;
                    }

                }


                //Monsters is checked
                if (chkMonsters.IsChecked == true)
                {
                    if (cmbAttribute.SelectionBoxItem.ToString() != "All Attributes")
                    {
                        if (Module1.CardStats[n].Attribute != cmbAttribute.SelectionBoxItem.ToString())
                        {
                            continue;
                        }
                    }

                    if (cmbType.SelectionBoxItem.ToString() != "All Types")
                    {
                        if (Module1.CardStats[n].Type.IndexOf(cmbType.SelectionBoxItem.ToString(), StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }
                        else if (cmbType.SelectionBoxItem.ToString() == "Beast")
                        {
                            if (Module1.CardStats[n].Type.Contains("Beast-Warrior") || Module1.CardStats[n].Type.Contains("Winged Beast"))
                                continue;
                        }
                    }

                    if (cmbEffect.SelectionBoxItem.ToString() != "All Subtypes" && cmbEffect.SelectionBoxItem.ToString() != "Normal")
                    {
                        if (!Module1.CardStats[n].Type.Contains(cmbEffect.SelectionBoxItem.ToString()))
                        {
                            continue;
                        }
                    }

                    if (cmbEffect.SelectionBoxItem.ToString() == "Normal")
                    {
                        if (Module1.CardStats[n].Type.Contains("Effect"))
                        {
                            continue;
                        }
                    }

                    if (cmbLevel.SelectionBoxItem != null && !string.IsNullOrEmpty(txtLevel.Text))
                    {
                        if (cmbLevel.SelectionBoxItem.ToString() == "<=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].Level))
                            {
                                if (Convert.ToInt32(Module1.CardStats[n].Level) > Convert.ToInt32(txtLevel.Text))
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }

                        if (cmbLevel.SelectionBoxItem.ToString() == ">=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].Level))
                            {
                                if (Convert.ToInt32(Module1.CardStats[n].Level) < Convert.ToInt32(txtLevel.Text))
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }

                        if (cmbLevel.SelectionBoxItem.ToString() == "=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].Level))
                            {
                                if (Module1.CardStats[n].Level != txtLevel.Text)
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }
                    }

                    if (cmbATK.SelectionBoxItem != null && !string.IsNullOrEmpty(txtATK.Text))
                    {
                        if (cmbATK.SelectionBoxItem.ToString() == "<=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].ATK))
                            {
                                if (Convert.ToInt32(Module1.CardStats[n].ATK) > Convert.ToInt32(txtATK.Text))
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }

                        if (cmbATK.SelectionBoxItem.ToString() == ">=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].ATK))
                            {
                                if (Convert.ToInt32(Module1.CardStats[n].ATK) < Convert.ToInt32(txtATK.Text))
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }

                        if (cmbATK.SelectionBoxItem.ToString() == "=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].ATK))
                            {
                                if (Module1.CardStats[n].ATK != txtATK.Text)
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                           
                        }
                    }

                    if (cmbDEF.SelectionBoxItem != null && !string.IsNullOrEmpty(txtDEF.Text))
                    {
                        if (cmbDEF.SelectionBoxItem.ToString() == "<=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].DEF))
                            {
                                if (Convert.ToInt32(Module1.CardStats[n].DEF) > Convert.ToInt32(txtDEF.Text))
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }

                        if (cmbDEF.SelectionBoxItem.ToString() == ">=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].DEF))
                            {
                                if (Convert.ToInt32(Module1.CardStats[n].DEF) < Convert.ToInt32(txtDEF.Text))
                                {
                                    continue;
                                }
                            }
                            else { continue; }
                        }

                        if (cmbDEF.SelectionBoxItem.ToString() == "=")
                        {
                            if (Module1.isNumeric(Module1.CardStats[n].DEF))
                            {
                                if (Module1.CardStats[n].DEF != txtDEF.Text)
                                {
                                    continue;
                                }
                            }
                            else { continue; }

                        }
                    }

                }

                //monsters is UNchecked
                if (chkMonsters.IsChecked == false)
                {
                    if (Module1.CardStats[n].Attribute != "Spell" && Module1.CardStats[n].Attribute != "Trap")
                    {
                        continue;
                    }

                }

                if (chkSpells.IsChecked == true)
                {
                    if (cmbSpells.SelectionBoxItem != null)
                    {
                        if (cmbSpells.SelectionBoxItem.ToString() != "Normal")
                        {
                            if (Module1.CardStats[n].Type != cmbSpells.SelectionBoxItem.ToString() || Module1.CardStats[n].Attribute != "Spell")
                            {
                                continue;
                            }


                            if (cmbSpells.SelectionBoxItem.ToString() == "Normal")
                            {
                                if (!string.IsNullOrEmpty(Module1.CardStats[n].Type))
                                {
                                    continue;
                                }

                            }
                        }
                    }
                }

                if (chkSpells.IsChecked == false)
                {
                    if (Module1.CardStats[n].Attribute == "Spell")
                    {
                        continue;
                    }

                }

                if (chkTraps.IsChecked == true)
                {
                    if (cmbTraps.SelectionBoxItem != null)
                    {
                        if (cmbTraps.SelectionBoxItem.ToString() != "Normal")
                        {
                            if (Module1.CardStats[n].Type != cmbTraps.SelectionBoxItem.ToString() || Module1.CardStats[n].Attribute != "Trap")
                            {
                                continue;
                            }


                            if (cmbTraps.SelectionBoxItem.ToString() == "Normal")
                            {
                                if (Module1.CardStats[n].Type == "Continuous" || Module1.CardStats[n].Type == "Quick-Play" || Module1.CardStats[n].Type == "Counter" || Module1.CardStats[n].Type == "Field" || Module1.CardStats[n].Type == "Ritual" || Module1.CardStats[n].Type == "Equip")
                                {
                                    continue;
                                }

                            }
                        }
                    }

                }

                if (chkTraps.IsChecked == false)
                {
                    if (Module1.CardStats[n].Attribute == "Trap")
                    {
                        continue;
                    }

                }


                if (!string.IsNullOrEmpty(txtDesc.Text))
                {
                    if (Module1.CardStats[n].Lore.IndexOf(txtDesc.Text, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        continue;

                    }


                }


                if (lstBySetList.Items.Count < 21)
                {
                    lstBySetList.Items.Add(Module1.CardStats[n].Name);
                }

            //    DeckBuffer[bufnum].Name = Module1.CardStats[n].Name;
             //   DeckBuffer[bufnum].ID = n;
                DeckBuffer.Add(Module1.CardStats[n].Name);
                bufnum += 1;
            }
            double dbl = bufnum / 21;

            NumberOfPages = Convert.ToInt16(dbl) + 1;
            BufPageNum = 1;
            cmdPrevPage.IsEnabled = false;
            if (NumberOfPages > 1)
                cmdNextPage.IsEnabled = true;
            else
                cmdNextPage.IsEnabled = false;
        }

        private void cmdNextPage_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            BufPageNum += 1;
            clearSet();
            for (int n = (BufPageNum * 21) - 20; n <= (BufPageNum * 21); n++)
            {
                try
                {
                    lstBySetList.Items.Add(DeckBuffer[n]);
                }
                catch (Exception)
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            if (BufPageNum > 1)
            {
                cmdPrevPage.IsEnabled = true;
            }
            else
            {
                cmdPrevPage.IsEnabled = false;
            }
            if (BufPageNum < NumberOfPages)
            {
                cmdNextPage.IsEnabled = true;
            }
            else
            {
                cmdNextPage.IsEnabled = false;
            }
        }

        private void cmdPrevPage_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            BufPageNum -= 1;
            clearSet();
            for (int n = (BufPageNum * 21) - 20; n <= (BufPageNum * 21); n++)
            {
                try
                {
                    lstBySetList.Items.Add(DeckBuffer[n]);
                }
                catch (Exception )
                {
                    break; // TODO: might not be correct. Was : Exit For
                }
            }
            if (BufPageNum > 1)
            {
                cmdPrevPage.IsEnabled = true;
            }
            else
            {
                cmdPrevPage.IsEnabled = false;
            }
            if (BufPageNum < NumberOfPages)
            {
                cmdNextPage.IsEnabled = true;
            }
            else
            {
                cmdNextPage.IsEnabled = false;
            }
        }

        private void cmdSave_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (cmbLoadDeck.SelectionBoxItem.ToString() == "Load Deck")
            {
                MsgBox("You have to have a deck already loaded.");
                return;
            }
            else
            {
                cmdSave.IsEnabled = false;
                cmdSaveAs.IsEnabled = false;
                SQLcli.SaveDeckAsync(Module1.username, cmbLoadDeck.SelectionBoxItem.ToString(), serializeDeck());
            }

        }
        private void client_SaveComplete(object sender, SQLReference.SaveDeckCompletedEventArgs e)
        {
            cmdSave.IsEnabled = true;
            cmdSaveAs.IsEnabled = true;
            if (e.Error == null)
            {
                MessageBox.Show("Deck was saved.");
                Module1.warnOnExitMessage = "";
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Error.Message))
                    MessageBox.Show("There was an error saving your deck." + Environment.NewLine + Environment.NewLine +
                                    e.Error.Message);
                else
                    MessageBox.Show("There was an error saving your deck. The error message is not available.");
            }
        }
        private string serializeDeck()
        {
            List<string> serializedList = new List<string>();
            for (short n = 1; n <= lstCurrentDeckList.Items.Count; n++)
            {
                int id = Module1.findTrueID(lstCurrentDeckList.Items[n - 1].ToString());
                if (id == 0)
                    continue;
                serializedList.Add(Convert.ToString(id));
            }
            for (short n = 1; n <= lstExtraDeckList.Items.Count; n++)
            {
                int id = Module1.findTrueID(lstExtraDeckList.Items[n - 1].ToString());
                if (id == 0)
                    continue;
                serializedList.Add(Convert.ToString(id));
            }
            serializedList.Add("&");
            for (short n = 1; n <= lstSideDeckList.Items.Count; n++)
            {
                int id = Module1.findTrueID(lstSideDeckList.Items[n - 1].ToString());
                if (id == 0)
                    continue;
                serializedList.Add(Convert.ToString(id));
            }
            string serializedString = string.Join("|", serializedList.ToArray());
            return serializedString;
        }
        private void deserializeAndLoad(string serializedString)
        {
            string[] deserializedArray = serializedString.Split('|');
            short count = (short)deserializedArray.Count();
            bool sideDeckSwitch = false;

            clearDeck();

            for (short n = 0; n <= count - 1; n++)
            {
                if (deserializedArray[n] == "&")
                {
                    sideDeckSwitch = true;
                    continue;
                }
                int id = Module1.findIndexFromTrueID(Convert.ToInt32(deserializedArray[n]));
                if (id == 0) { continue; }
                if (sideDeckSwitch == false)
                {
                   

                        //Belongs in Main Deck
                        if (Module1.BelongsInExtra(Module1.CardStats[id]) == false)
                        {
                            if (lstCurrentDeckList.Items.Count < 45)
                            {
                               Image refer = GetMainDeckPictureBox((short)(lstCurrentDeckList.Items.Count + 1));
                               UpdatePictureBox(ref refer, Module1.CardStats[id]);
                               lstCurrentDeckList.Items.Add(Module1.CardStats[id].Name);
                            }
                        }
                        else
                        {
                           
                            Image refer = GetExtraDeckPictureBox((short)(lstExtraDeckList.Items.Count + 1));
                            UpdatePictureBox(ref refer, Module1.CardStats[id]);
                            lstExtraDeckList.Items.Add(Module1.CardStats[id].Name);
                        }
                    
                }
                else
                {
                    Image refer = GetSideDeckPictureBox((short)(lstSideDeckList.Items.Count + 1));
                    UpdatePictureBox(ref refer, Module1.CardStats[id]);
                    lstSideDeckList.Items.Add(Module1.CardStats[id].Name);
                }
            }
            CountUpDeckStatistics();
            Module1.warnOnExitMessage = "";
        }
        private void lstCurrentDeckList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstCurrentDeckList.Height = 243;
            lstExtraDeckList.Visibility = System.Windows.Visibility.Visible;
            lstSideDeckList.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetZIndex(lstCurrentDeckList, 0);
        }


        private void lstCurrentDeckList_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstCurrentDeckList.Height = 423;
            lstExtraDeckList.Visibility = System.Windows.Visibility.Collapsed;
            lstSideDeckList.Visibility = System.Windows.Visibility.Collapsed;
            Canvas.SetZIndex(lstCurrentDeckList, 1);
        }

        private void lstExtraDeckList_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstExtraDeckList.Height *= 2;
            lstSideDeckList.Visibility = System.Windows.Visibility.Collapsed;
            Canvas.SetZIndex(lstExtraDeckList, 1);
        }

        private void lstExtraDeckList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstExtraDeckList.Height /= 2;
            lstSideDeckList.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetZIndex(lstExtraDeckList, 0);
        }

        private void lstCurrentDeckList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstCurrentDeckList.SelectedIndex == -1)
                return;


            try
            {
                cardID = Module1.findID(lstCurrentDeckList.SelectedItem.ToString());
                if (cardID == 0)
                    throw new ArgumentException();
                lblCardName.Text = Module1.CardStats[cardID].Name;

                //get field 4 card type string
                lblCardType.Text = Module1.CardStats[cardID].Type.NotDisplayEffect();
                //get field 5 attribute name string
                lblCardAttribute.Text = Module1.CardStats[cardID].Attribute;
                lblCardLevel.Text = Module1.CardStats[cardID].Level;
                if (lblCardAttribute.Text == "Spell" || lblCardAttribute.Text == "Trap")
                {
                    lblCardType.Text = Module1.CardStats[cardID].Type;
                }
                lblCardATK.Text = Module1.CardStats[cardID].ATK;

                //get field 9 card defense string
                lblCardDEF.Text = Module1.CardStats[cardID].DEF;

                //get field 10 card lore string
                lblCardLore.Text = Module1.CardStats[cardID].Lore;
                lblSetName.Text = Module1.CardStats[cardID].SpecialSet;

                //get Picture
                UpdatePictureBox(ref imgDeckEditorImg, Module1.CardStats[cardID]);
                //d.Invoke(imgDeckEditorImg, Module1.CardStats[cardID])
            }
            catch (ArgumentException )
            {
                Module1.setImage(ref imgDeckEditorImg, "token.jpg", UriKind.Relative);
                lblCardName.Text = "???";
                lblCardType.Text = "???";
                lblCardAttribute.Text = "???";
                lblCardLevel.Text = "???";
                lblCardType.Text = "???";
                lblCardATK.Text = "???";
                lblCardDEF.Text = "???";
                lblCardLore.Text = "Card not found";
            }
        }

        private void lstExtraDeckList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstExtraDeckList.SelectedIndex == -1)
                return;
            try
            {
                cardID = Module1.findID(lstExtraDeckList.SelectedItem.ToString());
                if (cardID == 0)
                    throw new ArgumentException();
                lblCardName.Text = Module1.CardStats[cardID].Name;

                //get field 4 card type string
                lblCardType.Text = Module1.CardStats[cardID].Type.NotDisplayEffect();

                //get field 5 attribute name string
                lblCardAttribute.Text = Module1.CardStats[cardID].Attribute;

                //get field 6 card level string
                lblCardLevel.Text = Module1.CardStats[cardID].Level;

                //skip field 7
                if (lblCardAttribute.Text == "Spell" || lblCardAttribute.Text == "Trap")
                {
                    lblCardType.Text = Module1.CardStats[cardID].Type;
                }

                //get field 8 attack string
                lblCardATK.Text = Module1.CardStats[cardID].ATK;

                //get field 9 card defense string
                lblCardDEF.Text = Module1.CardStats[cardID].DEF;

                //get field 10 card lore string
                lblCardLore.Text = Module1.CardStats[cardID].Lore;

    
                lblSetName.Text = Module1.CardStats[cardID].SpecialSet;

                //get Picture
                UpdatePictureBox(ref imgDeckEditorImg, Module1.CardStats[cardID]);
                //d.Invoke(imgDeckEditorImg, Module1.CardStats[cardID])
            }
            catch (ArgumentException )
            {
                Module1.setImage(ref imgDeckEditorImg, "token.jpg", UriKind.Relative);
                lblCardName.Text = "???";
                lblCardType.Text = "???";
                lblCardAttribute.Text = "???";
                lblCardLevel.Text = "???";
                lblCardType.Text = "???";
                lblCardATK.Text = "???";
                lblCardDEF.Text = "???";
                lblCardLore.Text = "Card not found";
            }
        }
        private void lstSideDeckList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstSideDeckList.SelectedIndex == -1)
                return;
            try
            {
                cardID = Module1.findID(lstSideDeckList.SelectedItem.ToString());
                if (cardID == 0)
                    throw new ArgumentException();
                lblCardName.Text = Module1.CardStats[cardID].Name;

                //get field 4 card type string
                lblCardType.Text = Module1.CardStats[cardID].Type.NotDisplayEffect();

                //get field 5 attribute name string
                lblCardAttribute.Text = Module1.CardStats[cardID].Attribute;

                //get field 6 card level string
                lblCardLevel.Text = Module1.CardStats[cardID].Level;

                //skip field 7
                if (lblCardAttribute.Text == "Spell" || lblCardAttribute.Text == "Trap")
                {
                    lblCardType.Text = Module1.CardStats[cardID].Type;
                }

                //get field 8 attack string
                lblCardATK.Text = Module1.CardStats[cardID].ATK;

                //get field 9 card defense string
                lblCardDEF.Text = Module1.CardStats[cardID].DEF;

                //get field 10 card lore string
                lblCardLore.Text = Module1.CardStats[cardID].Lore;
                lblSetName.Text = Module1.CardStats[cardID].SpecialSet;

                //get Picture
                UpdatePictureBox(ref imgDeckEditorImg, Module1.CardStats[cardID]);
                //d.Invoke(imgDeckEditorImg, Module1.CardStats[cardID])
            }
            catch (ArgumentException )
            {
                Module1.setImage(ref imgDeckEditorImg, "token.jpg", UriKind.Relative);
                lblCardName.Text = "???";
                lblCardType.Text = "???";
                lblCardAttribute.Text = "???";
                lblCardLevel.Text = "???";
                lblCardType.Text = "???";
                lblCardATK.Text = "???";
                lblCardDEF.Text = "???";
                lblCardLore.Text = "Card not found";
            }
        }
        private void lstCurrentDeckList_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstCurrentDeckList.SelectedIndex == -1)
            { automaticDoubleClick = false; return; }

            if (automaticDoubleClick || ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = false;
                short picindex = (short)(lstCurrentDeckList.SelectedIndex + 1);

                for (short n = picindex; n <= lstCurrentDeckList.Items.Count; n++)
                {
                    if (n > 45)
                        break; // TODO: might not be correct. Was : Exit For
                    if (n == 45 && lstCurrentDeckList.Items.Count > 45)
                    {
                        Module1.setImage(ref Image45, Module1.toPortalURL(lstCurrentDeckList.Items[45].ToString(), Module1.findTrueID(lstCurrentDeckList.Items[45].ToString()), Module1.mySet), UriKind.Absolute);
                    }
                    else if (n == 45 && lstCurrentDeckList.Items.Count == 45)
                    {
                        Image45.Source = null;
                    }
                    else
                    {
                        Image img = GetMainDeckPictureBox(n);
                        img.Source = GetMainDeckPictureBox((short)(n + 1)).Source;
                    }
                }

                if (Module1.sideDecking == true)
                {
                    if (lstSideDeckList.Items.Count == 15)
                    {
                        lstBySetList.Items.Add(lstCurrentDeckList.SelectedItem);
                    }
                    else
                    {
                        lstSideDeckList.Items.Add(lstCurrentDeckList.SelectedItem);
                        Image refer = GetSideDeckPictureBox((short)(lstSideDeckList.Items.Count));
                        UpdatePictureBox(ref refer, Module1.CardStats[Module1.findID(lstSideDeckList.Items[lstSideDeckList.Items.Count - 1].ToString())]);
                    }
                }


                lstCurrentDeckList.Items.RemoveAt(lstCurrentDeckList.SelectedIndex);
                if (Module1.sideDecking == false) { Module1.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; }
            }
           
            CountUpDeckStatistics();
        }

        private void lstExtraDeckList_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstExtraDeckList.SelectedIndex == -1)
            { automaticDoubleClick = false; return; }

            if (automaticDoubleClick || ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = false;
                short picindex = (short)(lstExtraDeckList.SelectedIndex + 1);

                for (short n = picindex; n <= lstExtraDeckList.Items.Count; n++)
                {
                    if (n == 15)
                    {
                        imgExtra15.Source = null;
                    }
                    else
                    {
                        Image img = GetExtraDeckPictureBox(n);
                        img.Source = GetExtraDeckPictureBox((short)(n + 1)).Source;
                    }
                }

                if (Module1.sideDecking == true)
                {
                    if (cardsOverSidedeckLimit() >= 0)
                    {
                        lstBySetList.Items.Add(lstExtraDeckList.SelectedItem);
                    }
                    else
                    {
                        lstSideDeckList.Items.Add(lstExtraDeckList.SelectedItem);
                        Image refer = GetSideDeckPictureBox((short)(lstSideDeckList.Items.Count));
                        UpdatePictureBox(ref refer, Module1.CardStats[Module1.findID(lstSideDeckList.Items[lstSideDeckList.Items.Count - 1].ToString())]);
                    }
                }

                lstExtraDeckList.Items.RemoveAt(lstExtraDeckList.SelectedIndex);
                if (Module1.sideDecking == false) { Module1.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; }
            }
            CountUpDeckStatistics();
        }
        private void lstSideDeckList_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstSideDeckList.SelectedIndex == -1)
            { automaticDoubleClick = false; return; }

            if (automaticDoubleClick || ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = false;
                short picindex = (short)(lstSideDeckList.SelectedIndex + 1);

                for (short n = picindex; n <= lstSideDeckList.Items.Count; n++)
                {
                    if (n == 15)
                    {
                        imgSide15.Source = null;
                    }
                    else
                    {
                        Image img = GetSideDeckPictureBox(n);
                        img.Source = GetSideDeckPictureBox((short)(n + 1)).Source;
                    }
                }

                if (Module1.sideDecking == true)
                {
                    bool success = addToExtraOrMainDeck(Module1.findID(lstSideDeckList.SelectedItem.ToString()), false);
                    if (success)
                        lstSideDeckList.Items.RemoveAt(lstSideDeckList.SelectedIndex);
                }
                else
                {
                    Module1.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?";

                    lstSideDeckList.Items.RemoveAt(lstSideDeckList.SelectedIndex);
                }
            }
        }

        private void cmdSaveAs_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            txtSaveAs.Text = txtSaveAs.Text.Trim();
            if (txtSaveAs.Text == "Load Deck")
            {
                MsgBox("Sorry, that name is reserved.");
                return;
            }
            if (string.IsNullOrEmpty(txtSaveAs.Text))
            {
                MsgBox("Enter a name for your deck.");
                return;
            }
            if (cmbLoadDeck.Items.Contains(txtSaveAs.Text))
            {
                MsgBox("You already have a deck under the name, " + "\"" + txtSaveAs.Text + "\"");
                return;
            }
            cmdSave.IsEnabled = false;
            cmdSaveAs.IsEnabled = false;
            
            SQLcli.NewDeckAsync(Module1.username, txtSaveAs.Text, serializeDeck());
        }
        private void createdNewDeck(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            cmdSave.IsEnabled = true;
            cmdSaveAs.IsEnabled = true;
            if (e.Error == null)
            {
                cmbLoadDeck.Items.Add(txtSaveAs.Text);
                Module1.defaultDeckName = txtSaveAs.Text;
                Module1.listOfMyDecks.Add(txtSaveAs.Text);
                Module1.SetCookie("default", txtSaveAs.Text);
                MessageBox.Show("New Deck made.");
                Module1.warnOnExitMessage = "";
            }
            else
                MessageBox.Show("There was an error creating your deck. The service might be down. Press save again or try later.");

        }
        private void SideDeckItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
        {
            if (lstBySetList.SelectedIndex == -1)
                return;
            if (lstSideDeckList.Items.Count == 15)
            {
                MsgBox("You can only have 15 cards in your side deck.");
                return;
            }
            lstSideDeckList.Items.Add(lstBySetList.Items[lstBySetList.SelectedIndex]);
            Image img = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count);
            UpdatePictureBox(ref img, Module1.CardStats[Module1.findID(lstSideDeckList.Items[lstSideDeckList.Items.Count - 1].ToString())]);
            Module1.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?";
        }

        private void lstBySetList_MouseRightButtonDown(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstBySetList.SelectedIndex == -1) return;
            
            e.Handled = true;
            ctxtSideDeck.Visibility = System.Windows.Visibility.Visible;

            Point point = e.GetPosition(this);
            FollowMouse(ref SideDeckTransform, ctxtSideDeck, point, false);
        }

        private void DeckEditor_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ctxtSideDeck.Visibility = System.Windows.Visibility.Collapsed;
        }
        public void FollowMouse(ref TranslateTransform transform, ContextMenu context, System.Windows.Point point, bool displayAbove)
        {


            double moveRight = (point.X - context.CLeft());
            //experimental
            double moveDown = (point.Y - context.CTop());
            if (displayAbove)
            {
                if (!double.IsNaN(context.Height))
                {
                    moveDown -= context.Height;
                }
            }
            transform.X = moveRight;
            transform.Y = moveDown;
        }

 
        public void makeDeckFromSiding()
        {

            Module1.PlayerCurrentDeck.ClearAndAdd();
            Module1.PlayerCurrentEDeck.ClearAndAdd();
            int id = 0;
            for (short n = 1; n <= lstCurrentDeckList.Items.Count; n++)
            {
                id = Module1.findID(lstCurrentDeckList.Items[n - 1].ToString());
                
                Module1.PlayerCurrentDeck.Add(Module1.CardStats[id]);
            }
            Module1.PlayerCurrentDeck.ClearAllAbove(lstCurrentDeckList.Items.Count + 1);
            //for (short n = (short)(lstCurrentDeckList.Items.Count + 1); n <= 100; n++)
            //{
            //   setAsNothing(Module1.PlayerCurrentDeck[n]);
            //}
          //  Module1.NumCardsInDeck = (short)lstCurrentDeckList.Items.Count;
           
            for (short n = 1; n <= lstExtraDeckList.Items.Count; n++)
            {
                id = Module1.findID(lstExtraDeckList.Items[n - 1].ToString());
               Module1.PlayerCurrentEDeck.Add( Module1.CardStats[id]);
            }

            Module1.PlayerCurrentEDeck.ClearAllAbove(lstExtraDeckList.Items.Count + 1);
            //for (short n = (short)(lstExtraDeckList.Items.Count + 1); n <= 15; n++)
            //{
            //     setAsNothing(Module1.PlayerCurrentEDeck[n]);
            //}
           // Module1.NumCardsInEDeck = (short)lstExtraDeckList.Items.Count;
            //  realEDeckLength = lstExtraDeckList.Items.Count



        }
        public void makeSideDeck()
        {
            int id = 0;
            for (short n = 1; n <= lstSideDeckList.Items.Count; n++)
            {
                id = Module1.findID(lstSideDeckList.Items[n - 1].ToString());
                Module1.realSideDeckIDs[n] = id;
            }

        }
        private void setAsNothing(SQLReference.CardDetails stats)
        {
            if (stats == null)
            { return; }
            stats.Name = null;
            stats.Level = null;
            stats.Type = null;
            stats.Attribute = null;
            stats.Facedown = false;
            stats.IsItHorizontal = false;
            stats.Lore = null;
            stats.ATK = null;
            stats.DEF = null;
            stats.ID = 0;
            stats.SpecialSet = null;
           // stats.STicon = null;
            stats.Creator = null;
            stats.Counters = 0;
            stats.OpponentOwned = false;
        }
     
        public void UpdatePictureBox(ref Image pBox, SQLReference.CardDetails stats)
        {
            string cardname = stats.Name;
           // int suppliedid = Module1.findID(stats.Name);
            try
            {
                if (stats.Name == null)
                {
                    Module1.setImage(ref pBox, "LightBG.png", UriKind.Relative);
                    return;
                }
                if (Module1.cardsWithImages.Contains(Module1.getRealImageName(cardname, stats.ID, Module1.mySet)))
                {
                    Module1.setImage(ref pBox, Module1.toPortalURL(cardname, stats.ID, Module1.mySet), UriKind.Absolute);
                }
                else
                {
                    //int cardid = 0;
                 
                    if (stats.ID == 0 || cardname == null)
                        Module1.setImage(ref pBox, "token.jpg", UriKind.Relative);


               
                    if (stats.Attribute=="Trap")
                    {
                        Module1.setImage(ref pBox, "trap.jpg", UriKind.Relative);
                        return;
                    }

                    if (stats.Attribute == "Spell")
                    {
                        Module1.setImage(ref pBox, "magic.jpg", UriKind.Relative);
                        return;
                    }

                    if (stats.Type.Contains("/Effect") && Module1.IsOrange(stats) == true)
                    {
                        Module1.setImage(ref pBox, "monstereffect.jpg", UriKind.Relative);
                        return;
                    }

                    if (stats.Type.Contains("/Ritual"))
                    {
                        Module1.setImage(ref pBox, "ritual.jpg", UriKind.Relative);
                        return;
                    }

                    if (stats.Type.Contains("/Synchro"))
                    {
                        Module1.setImage(ref pBox, "synchro.jpg", UriKind.Relative);
                        return;
                    }

                    if (stats.Type.Contains("/Fusion"))
                    {
                        Module1.setImage(ref pBox, "fusion.jpg", UriKind.Relative);
                        return;
                    }

                    if (stats.Type.Contains("/Xyz"))
                    {
                        Module1.setImage(ref pBox, "xyz.jpg", UriKind.Relative);
                        return;
                    }

                    Module1.setImage(ref pBox, "monster.jpg", UriKind.Relative);
                }
                pBox.Opacity = 1;

            }
            catch
            {

            }
        }

        private void cmdLegacyLoad_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            LegacyForm.Show();

        }
        public void Legacy_Closed(object sender, System.EventArgs e)
        {
            
            if (!string.IsNullOrEmpty(LegacyForm.myLegacyDeck))
            {
                clearDeck();
                try
                {
                    if (LegacyForm.myLegacyDeck.Contains("|")) //YVD
                    {
                        legacyLoadYVD(LegacyForm.myLegacyDeck);
                    }
                    else
                    {
                        string[] cards = LegacyForm.myLegacyDeck.Split('\r');
                        foreach (string card in cards)
                        {
                            int id = Module1.findID(card);
                            if (id > 0)
                            {
                                addToExtraOrMainDeck(id, true);
                            }

                        }
                    }

                }
                catch 
                {
                }
                LegacyForm.myLegacyDeck = "";
                //Module1.myLegacyDeck = "";
            }
        }
        private void legacyLoadYVD(string yvdCode)
        {
            string[] cards = yvdCode.Split('\r');
            bool sideSwitch = false;
            foreach (string cardGrouping in cards)
            {
                if (cardGrouping == "-SIDE DECK-")
                { sideSwitch = true; continue ; }
                else
                {
                    int num = Convert.ToInt32(cardGrouping.Substring(0, 1));
                    int lastBar = cardGrouping.LastIndexOf("|", cardGrouping.Length);
                    string cardname = cardGrouping.Substring(lastBar + 1, cardGrouping.Length - lastBar - 1);
                    int id = Module1.findID(cardname);
                    if (id > 0)
                    {

                        for (int n = 1; n <= num; n++)
                        {
                            if (sideSwitch)
                            {
                                Image refer = GetSideDeckPictureBox((short)(lstSideDeckList.Items.Count + 1));
                                if (refer != null)
                                {
                                    UpdatePictureBox(ref refer, Module1.CardStats[id]);
                                    lstSideDeckList.Items.Add(Module1.CardStats[id].Name);
                                }
                            }
                            else
                               addToExtraOrMainDeck(id, true);

                        }

                    }
                }
            }

        }
        private void cmdDoneSideDecking_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            short cardsOverSide = cardsOverSidedeckLimit();
            if (cardsOverSide > 0)
            {
                MsgBox("You have " + cardsOverSide + " too many cards in your Main/Extra deck.");
                return;
            }
            else if (cardsOverSide < 0)
            {
                MsgBox("You need to add " + (cardsOverSide * -1) + " more cards to your Main/Extra deck.");
                return;
            }
            else
            {
                Module1.warnOnExitMessage = "";
                Module1.sideDecking = false;
                makeDeckFromSiding();
                this.NavigationService.Navigate(new System.Uri("/DuelField", UriKind.Relative));
            }
        }


        private void cmbLoadDeck_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbLoadDeck != null)
            {
                if (cmbLoadDeck.SelectedItem != null && cmbLoadDeck.SelectedItem.ToString() != "System.Windows.Controls.ComboBoxItem")
                {
                    SQLcli.loadDeckAsync(Module1.username, cmbLoadDeck.SelectedItem.ToString());
                    Module1.defaultDeckName = cmbLoadDeck.SelectedItem.ToString();
                    Module1.SetCookie("default", Module1.defaultDeckName);
                }
            }
        }
        private void doneLoadingDeck(object sender, SQLReference.loadDeckCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Result))
            {
                deserializeAndLoad(e.Result);
                
            }
            else
            {
                MsgBox("There was an error loading your deck.");
            }
        }
        
       

        private void fraSearch_Navigated(System.Object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
        }

        private void cmdShareSpecific_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstBySetList.SelectedIndex == -1)
               return;
            if (Module1.CardStats[Module1.findID(lstBySetList.SelectedItem.ToString())].Creator == "TCG")
            {
                MessageBox.Show("You cannot share TCG Staples. The user must select 'Change TCG' in their profile on the main page.");
                return;
            }
            if (Module1.mySet != "Default")
            {
                MessageBox.Show("You cannot share cards under a non-default account, because others have no way of accessing the cards.");
                return;
            }
            inputForm.message = "Enter the name of the person to send " + lstBySetList.SelectedItem.ToString();
            inputForm.Closed += shareSpecificStep1;
            inputForm.Show();
        }
        private void shareSpecificStep1(object sender, EventArgs e)
        {
            inputForm.Closed -= shareSpecificStep1;
            if (string.IsNullOrEmpty(inputForm.input))
            {
                MessageBox.Show("You must enter a name!");
                return;
            }
            System.Collections.ObjectModel.ObservableCollection<SQLReference.CardDetails> cardList = new System.Collections.ObjectModel.ObservableCollection<SQLReference.CardDetails>();
            SQLReference.CardDetails card = default(SQLReference.CardDetails);
            card = Module1.CardStats[Module1.findID(lstBySetList.SelectedItem.ToString())].toTrueStats();
            if (card.Creator.Contains("|" + inputForm.input + "|"))
            {
                MessageBox.Show("You are already sharing " + card.Name + " with " + inputForm.input+ ", or you have already tried this session.");
                return;
            }
            if ((card.Creator + inputForm.input + "|").Length >= 200)
            {
                MessageBox.Show("The number of shared users exceeds maximum column length. Perhaps you should consider getting a CCG File for your group?");
                return;

            }

            card.Creator += inputForm.input + "|"; 
            cardList.Add(card);
            SQLcli.shareCardCompleted += endShare;
            SQLcli.shareCardAsync(Module1.username, inputForm.input, cardList);


        }
        private void endShare(object sender, SQLReference.shareCardCompletedEventArgs e)
        {
            SQLcli.shareCardCompleted -= endShare;
            MessageBox.Show(e.Result);
            System.Collections.ObjectModel.ObservableCollection<SQLReference.CardDetails> CardList = (System.Collections.ObjectModel.ObservableCollection<SQLReference.CardDetails>)e.UserState;
            if (CardList != null && e.Result.Contains("Success"))
            {
                foreach (SQLReference.CardDetails card in (CardList))
                {
                    int index = Module1.findID(card.Name);
                    Module1.CardStats[index] = card;
                }
            }
          
        }

        private void cmdShareSet_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (Module1.mySet != "Default")
            {
                MessageBox.Show("You cannot share cards under a non-default account, because others have no way of accessing the cards.");
                return;
            }
            inputForm.message = "Enter in the Set Name:";
            inputForm.Closed += shareSetStep1;
            inputForm.Show();
        }
        private void shareSetStep1(object sender, EventArgs e)
        {
            inputForm.Closed -= shareSetStep1;
            if (string.IsNullOrEmpty(inputForm.input))
            {
                MessageBox.Show("You must enter a Set Name!");
                return;
            }
            bool setFound = false;
            for (int n = 1; n <= Module1.TotalCards; n++)
            {
                if (!string.IsNullOrEmpty(Module1.CardStats[n].Name))
            {
                    if (Module1.CardStats[n].SpecialSet == inputForm.input && Module1.CardStats[n].Creator != "PUB")
                    {
                            setFound = true;
                            break;
                    
                    }

                }
            }

            if (setFound == false)
            {
                MessageBox.Show("The set " + inputForm.input + " was not found.");
                return;
            }

            inputForm.message = "Enter the name of the person to send set " + inputForm.input;
            setToShare = inputForm.input;
            inputForm.Closed += shareSetStep2;
            inputForm.Show();
        }
        private string setToShare;
        private void shareSetStep2(object sender, EventArgs e)
        {
            inputForm.Closed -= shareSetStep2;
            if (string.IsNullOrEmpty(inputForm.input))
            {
                MessageBox.Show("You must enter a name!");
                return;
            }
            System.Collections.ObjectModel.ObservableCollection<SQLReference.CardDetails> setOfCards = new System.Collections.ObjectModel.ObservableCollection<SQLReference.CardDetails>();

            for (short n = 1; n <= Module1.TotalCards; n++)
            {
                if (!string.IsNullOrEmpty(Module1.CardStats[n].Name) && Module1.CardStats[n].SpecialSet == setToShare)
                {
                    SQLReference.CardDetails card = default(SQLReference.CardDetails);
                    card = Module1.CardStats[n].toTrueStats();
                    if (card.Creator != null && card.Creator.Contains("|" + inputForm.input + "|") == false)
                        card.Creator = card.Creator + inputForm.input + "|";
                    setOfCards.Add(card);

                }
            }
            SQLcli.shareCardCompleted += endShare;
            SQLcli.shareCardAsync(Module1.username, inputForm.input, setOfCards);

        }

        private void cmdSort_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            List<string> monsterList = new List<string>();
            List<string> spellList = new List<string>();
            List<string> trapList = new List<string>();
            List<string> fusionList = new List<string>();
            List<string> xyzList = new List<string>();
            List<string> synchroList = new List<string>();
            List<string> sideMonsterList = new List<string>();
            List<string> sideSpellList = new List<string>();
            List<string> sideTrapList = new List<string>();
            List<string> sideFusionList = new List<string>();
            List<string> sideXyzList = new List<string>();
            List<string> sideSynchroList = new List<string>();
            for (short n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
            {
                int id = Module1.findID(lstCurrentDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (Module1.CardStats[id].Attribute == "Spell")
                {
                    spellList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Attribute == "Trap")
                {
                    trapList.Add(Module1.CardStats[id].Name);
                }
                else
                {
                    monsterList.Add(Module1.CardStats[id].Name);
                }
            }
            for (short n = 0; n <= lstExtraDeckList.Items.Count - 1; n++)
            {
                int id = Module1.findID(lstExtraDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (Module1.CardStats[id].Type.Contains("Fusion"))
                {
                    fusionList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Xyz"))
                {
                    xyzList.Add(Module1.CardStats[id].Name);
                }
                else
                {
                    synchroList.Add(Module1.CardStats[id].Name);
                }
            }
            for (short n = 0; n <= lstSideDeckList.Items.Count - 1; n++)
            {
                int id = Module1.findID(lstSideDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (Module1.CardStats[id].Attribute == "Spell")
                {
                    sideSpellList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Attribute == "Trap")
                {
                    sideTrapList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Fusion"))
                {
                    sideFusionList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Xyz"))
                {
                    sideXyzList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Synchro"))
                {
                    sideSynchroList.Add(Module1.CardStats[id].Name);
                }
                else
                {
                    sideMonsterList.Add(Module1.CardStats[id].Name);
                }
            }
            monsterList.Sort();
            spellList.Sort();
            trapList.Sort();
            fusionList.Sort();
            xyzList.Sort();
            synchroList.Sort();

            sideMonsterList.Sort();
            sideSpellList.Sort();
            sideTrapList.Sort();
            sideFusionList.Sort();
            sideXyzList.Sort();
            sideSynchroList.Sort();

            clearDeck();

            foreach (string monster in monsterList)
            {
                lstCurrentDeckList.Items.Add(monster);
                int id = Module1.findID(monster);
                if (lstCurrentDeckList.Items.Count < 46)
                {
                    Image refer = GetMainDeckPictureBox((short)(lstCurrentDeckList.Items.Count));
                    UpdatePictureBox(ref refer, Module1.CardStats[id]);
                }
            }
            foreach (string spell in spellList)
            {
                lstCurrentDeckList.Items.Add(spell);
                int id = Module1.findID(spell);
                if (lstCurrentDeckList.Items.Count < 46)
                {
                    Image refer = GetMainDeckPictureBox((short)lstCurrentDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
                }
            }
            foreach (string trap in trapList)
            {
                lstCurrentDeckList.Items.Add(trap);
                int id = Module1.findID(trap);
                if (lstCurrentDeckList.Items.Count < 46)
                {
                    Image refer = GetMainDeckPictureBox((short)lstCurrentDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
                }
            }
            foreach (string fusion in fusionList)
            {
                lstExtraDeckList.Items.Add(fusion);
                int id = Module1.findID(fusion);
                Image refer = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string xyz in xyzList)
            {
                lstExtraDeckList.Items.Add(xyz);
                int id = Module1.findID(xyz);
                Image refer = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string synchro in synchroList)
            {
                lstExtraDeckList.Items.Add(synchro);
                int id = Module1.findID(synchro);
                Image refer = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }


            foreach (string monster in sideMonsterList)
            {
                lstSideDeckList.Items.Add(monster);
                int id = Module1.findID(monster);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string spell in sideSpellList)
            {
                lstSideDeckList.Items.Add(spell);
                int id = Module1.findID(spell);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string trap in sideTrapList)
            {
                lstSideDeckList.Items.Add(trap);
                int id = Module1.findID(trap);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string fusion in sideFusionList)
            {
                lstSideDeckList.Items.Add(fusion);
                int id = Module1.findID(fusion);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string xyz in sideXyzList)
            {
                lstSideDeckList.Items.Add(xyz);
                int id = Module1.findID(xyz);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string synchro in sideSynchroList)
            {
                lstSideDeckList.Items.Add(synchro);
                int id = Module1.findID(synchro);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            CountUpDeckStatistics();
        }

        private void imgDeckEditorImg_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Module1.mySet == "LCCG" && Module1.cardsWithImages.Contains(Module1.getRealImageName(lblCardName.Text, 0, Module1.mySet)))
            {
                Canvas.SetZIndex(imgDeckEditorImg, 5);
                imgFullView.Visibility = System.Windows.Visibility.Visible;
                Module1.setImage(ref imgFullView, "GettingImage.jpg", UriKind.Relative);
                SQLcli.getFullImageURLCompleted += new EventHandler<SQLReference.getFullImageURLCompletedEventArgs>(SQLcli_getFullImageURLCompleted);
                SQLcli.getFullImageURLAsync("http://yugiohccg.wikia.com/wiki/" + lblCardName.Text.Replace(" ", "_"));
            }
        }
        private void SQLcli_getFullImageURLCompleted(object sender, SQLReference.getFullImageURLCompletedEventArgs e)
        {
            if (e.Result != "")
            {

                SQLcli.getFullImageURLCompleted -= SQLcli_getFullImageURLCompleted;
                Module1.setImage(ref imgFullView, e.Result, UriKind.Absolute);
                Canvas.SetZIndex(imgFullView, 10);
            }
        }
   
        private void imgFullView_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Module1.setImage(ref imgFullView, "NoImage.jpg", UriKind.Relative);
        }

        private void imgFullView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imgFullView.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imgFullView.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void chkMonsters_Checked(object sender, RoutedEventArgs e)
        {


            if (chkMonsters == null) { return; }

            if (cmdSearch != null)
                cmdSearch.Focus();


            if (chkMonsters.IsChecked == true)
            {
                cmbAttribute.SelectedIndex = 0;
                cmbType.SelectedIndex = 0;
                cmbEffect.SelectedIndex = 0;
                cmbATK.SelectedIndex = 0;
                cmbDEF.SelectedIndex = 0;
                cmbLevel.SelectedIndex = 0;
                txtATK.Text = "";
                txtDEF.Text = "";
                txtLevel.Text = "";

            }
        }

        private void chkSpells_Checked(object sender, RoutedEventArgs e)
        {
            if (chkSpells == null) { return; }

            if (cmdSearch != null)
                cmdSearch.Focus();


            if (chkSpells.IsChecked == true)
            { cmbSpells.SelectedIndex = 0; }
        }

        private void chkTraps_Checked(object sender, RoutedEventArgs e)
        {
            if (chkTraps == null) { return; }

            if (cmdSearch != null)
                cmdSearch.Focus();


            if (chkTraps.IsChecked == true)
            { cmbTraps.SelectedIndex = 0; }
        }

        private void cmdDeleteDeck_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoadDeck.SelectionBoxItem == null || cmbLoadDeck.SelectionBoxItem.ToString() == "") { return; }
            if (cmbLoadDeck.SelectionBoxItem.ToString() == "Load Deck") { return; }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete deck " + cmbLoadDeck.SelectionBoxItem.ToString() + "? There is no undoing this action.", "Delete Deck", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) { return; }
            SQLcli.deleteDeckAsync(Module1.username, cmbLoadDeck.SelectionBoxItem.ToString(), cmbLoadDeck.SelectionBoxItem.ToString());
        }
        private void SQLcli_deleteDeckAsyncCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show((string)e.UserState + " was successfully deleted.");
               // cmbLoadDeck.SelectedIndex = 0;
                cmbLoadDeck.Items.Remove((string)e.UserState);
            }
            else
            {MessageBox.Show("There was an error deleting the deck " + (string)e.UserState + ".");}
        }

        private void cmdClipboard_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<int, int> idToIndex = new Dictionary<int, int>();
            System.Text.StringBuilder strBld = new System.Text.StringBuilder();
            int monCount = 0, sCount = 0, tCount = 0;
            for (int i = 0; i < lstCurrentDeckList.Items.Count; i++)
            {
                int id = Module1.findID(lstCurrentDeckList.Items[i].ToString());
                if (!idToIndex.ContainsKey(id))
                    idToIndex.Add(id, 1);
                else
                    idToIndex[Module1.findID(lstCurrentDeckList.Items[i].ToString())]++;
            }

            strBld.Append("Monsters:" + Environment.NewLine);
            foreach (int id in idToIndex.Keys)
            {
                if (Module1.CardStats[id].ATK != "")
                {
                    monCount += idToIndex[id];
                    if (Module1.mySet == "LCCG")
                        strBld.Append(idToIndex[id].ToString() + " " + "[url=http://yugiohccg.wikia.com/wiki/" + Module1.CardStats[id].Name.Replace(" ", "_") + "]" + Module1.CardStats[id].Name + "[/url]" + Environment.NewLine);
                    else
                        strBld.Append(idToIndex[id].ToString() + " " + Module1.CardStats[id].Name + Environment.NewLine);
                }
            }
            strBld.Replace("Monsters:", "Monsters: " + monCount.ToString());

            strBld.Append(Environment.NewLine + "Spells:" + Environment.NewLine);
            foreach (int id in idToIndex.Keys)
            {
                if (Module1.CardStats[id].Attribute == "Spell")
                {
                    sCount += idToIndex[id];
                    if (Module1.mySet == "LCCG")
                        strBld.Append(idToIndex[id].ToString() + " " + "[url=http://yugiohccg.wikia.com/wiki/" + Module1.CardStats[id].Name.Replace(" ", "_") + "]" + Module1.CardStats[id].Name + "[/url]" + Environment.NewLine);
                    else
                        strBld.Append(idToIndex[id].ToString() + " " + Module1.CardStats[id].Name + Environment.NewLine);
                }
            }
            strBld.Replace("Spells:", "Spells: " + sCount.ToString());

            strBld.Append(Environment.NewLine + "Traps:" + Environment.NewLine);
            foreach (int id in idToIndex.Keys)
            {
                if (Module1.CardStats[id].Attribute == "Trap")
                {
                    tCount += idToIndex[id];
                    if (Module1.mySet == "LCCG")
                        strBld.Append(idToIndex[id].ToString() + " " + "[url=http://yugiohccg.wikia.com/wiki/" + Module1.CardStats[id].Name.Replace(" ", "_") + "]" + Module1.CardStats[id].Name + "[/url]" + Environment.NewLine);
                    else
                        strBld.Append(idToIndex[id].ToString() + " " + Module1.CardStats[id].Name + Environment.NewLine);
                }
            }
            strBld.Replace("Traps:", "Traps: " + tCount.ToString());


            if (lstExtraDeckList.Items.Count > 0)
            {
                idToIndex.Clear();
                for (int i = 0; i < lstExtraDeckList.Items.Count; i++)
                {
                    int id = Module1.findID(lstExtraDeckList.Items[i].ToString());
                    if (!idToIndex.ContainsKey(id))
                        idToIndex.Add(id, 1);
                    else
                        idToIndex[Module1.findID(lstExtraDeckList.Items[i].ToString())]++;
                }

                strBld.Append(Environment.NewLine + "Extra:" + Environment.NewLine);

                foreach (int id in idToIndex.Keys)
                {
                        if (Module1.mySet == "LCCG")
                            strBld.Append(idToIndex[id].ToString() + " " + "[url=http://yugiohccg.wikia.com/wiki/" + Module1.CardStats[id].Name.Replace(" ", "_") + "]" + Module1.CardStats[id].Name + "[/url]" + Environment.NewLine);
                        else
                            strBld.Append(idToIndex[id].ToString() + " " + Module1.CardStats[id].Name + Environment.NewLine);
                }
            }

            if (lstSideDeckList.Items.Count > 0)
            {
                idToIndex.Clear();
                for (int i = 0; i < lstSideDeckList.Items.Count; i++)
                {
                    int id = Module1.findID(lstSideDeckList.Items[i].ToString());
                    if (!idToIndex.ContainsKey(id))
                        idToIndex.Add(id, 1);
                    else
                        idToIndex[Module1.findID(lstSideDeckList.Items[i].ToString())]++;
                }
                
                strBld.Append(Environment.NewLine + "Side:" + Environment.NewLine);
                
                foreach (int id in idToIndex.Keys)
                {
                    if (Module1.mySet == "LCCG")
                        strBld.Append(idToIndex[id].ToString() + " " + "[url=http://yugiohccg.wikia.com/wiki/" + Module1.CardStats[id].Name.Replace(" ", "_") + "]" +  Module1.CardStats[id].Name + "[/url]" +  Environment.NewLine);
                    else
                        strBld.Append(idToIndex[id].ToString() + " " + Module1.CardStats[id].Name + Environment.NewLine);
                }
            }
            try
            {
                Clipboard.SetText(strBld.ToString());
                MessageBox.Show("Text version of deck copied to clipboard. Paste it in a forum or something.");
            }
            catch { }
        }

        private void cmbAttribute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
            cmdSearch.Focus();
        }

        private void cmbSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbEffect_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbATK_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbDEF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbSpells_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void cmbTraps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmdSearch != null)
                cmdSearch.Focus();
        }

        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdSearch != null)
            {
                cmdSearch_Click(null, new RoutedEventArgs());
                txtName.Focus();
            }
        }

        private void txtDesc_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdSearch != null)
            {
                cmdSearch_Click(null, new RoutedEventArgs());
                txtDesc.Focus();
            }
        }



        private void txtDEF_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdSearch != null)
            {
                cmdSearch_Click(null, new RoutedEventArgs());
                txtDEF.Focus();
            }
        }

        private void txtATK_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdSearch != null)
            {
                cmdSearch_Click(null, new RoutedEventArgs());
                txtATK.Focus();
            }
        }

        private void txtLevel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdSearch != null)
            {
                cmdSearch_Click(null, new RoutedEventArgs());
                txtLevel.Focus();
            }
        }

        private void cmdSortNum_Click(object sender, RoutedEventArgs e)
        {
            List<string> monsterList = new List<string>();
            List<string> spellList = new List<string>();
            List<string> trapList = new List<string>();
            List<string> fusionList = new List<string>();
            List<string> xyzList = new List<string>();
            List<string> synchroList = new List<string>();
            List<string> sideMonsterList = new List<string>();
            List<string> sideSpellList = new List<string>();
            List<string> sideTrapList = new List<string>();
            List<string> sideFusionList = new List<string>();
            List<string> sideXyzList = new List<string>();
            List<string> sideSynchroList = new List<string>();
            for (short n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
            {
                int id = Module1.findID(lstCurrentDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (Module1.CardStats[id].Attribute == "Spell")
                {
                    spellList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Attribute == "Trap")
                {
                    trapList.Add(Module1.CardStats[id].Name);
                }
                else
                {
                    monsterList.Add(Module1.CardStats[id].Name);
                }
            }
            for (short n = 0; n <= lstExtraDeckList.Items.Count - 1; n++)
            {
                int id = Module1.findID(lstExtraDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (Module1.CardStats[id].Type.Contains("Fusion"))
                {
                    fusionList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Xyz"))
                {
                    xyzList.Add(Module1.CardStats[id].Name);
                }
                else
                {
                    synchroList.Add(Module1.CardStats[id].Name);
                }
            }
            for (short n = 0; n <= lstSideDeckList.Items.Count - 1; n++)
            {
                int id = Module1.findID(lstSideDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (Module1.CardStats[id].Attribute == "Spell")
                {
                    sideSpellList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Attribute == "Trap")
                {
                    sideTrapList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Fusion"))
                {
                    sideFusionList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Xyz"))
                {
                    sideXyzList.Add(Module1.CardStats[id].Name);
                }
                else if (Module1.CardStats[id].Type.Contains("Synchro"))
                {
                    sideSynchroList.Add(Module1.CardStats[id].Name);
                }
                else
                {
                    sideMonsterList.Add(Module1.CardStats[id].Name);
                }
            }
            monsterList.SortByOccurrence();
            spellList.SortByOccurrence();
            trapList.SortByOccurrence();
            fusionList.SortByOccurrence();
            xyzList.SortByOccurrence();
            synchroList.SortByOccurrence();

            sideMonsterList.SortByOccurrence();
            sideSpellList.SortByOccurrence();
            sideTrapList.SortByOccurrence();
            sideFusionList.SortByOccurrence();
            sideXyzList.SortByOccurrence();
            sideSynchroList.SortByOccurrence();

            clearDeck();

            foreach (string monster in monsterList)
            {
                lstCurrentDeckList.Items.Add(monster);
                int id = Module1.findID(monster);
                if (lstCurrentDeckList.Items.Count < 46)
                {
                    Image refer = GetMainDeckPictureBox((short)(lstCurrentDeckList.Items.Count));
                    UpdatePictureBox(ref refer, Module1.CardStats[id]);
                }
            }
            foreach (string spell in spellList)
            {
                lstCurrentDeckList.Items.Add(spell);
                int id = Module1.findID(spell);
                if (lstCurrentDeckList.Items.Count < 46)
                {
                    Image refer = GetMainDeckPictureBox((short)lstCurrentDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
                }
            }
            foreach (string trap in trapList)
            {
                lstCurrentDeckList.Items.Add(trap);
                int id = Module1.findID(trap);
                if (lstCurrentDeckList.Items.Count < 46)
                {
                    Image refer = GetMainDeckPictureBox((short)lstCurrentDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
                }
            }
            foreach (string fusion in fusionList)
            {
                lstExtraDeckList.Items.Add(fusion);
                int id = Module1.findID(fusion);
                Image refer = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string xyz in xyzList)
            {
                lstExtraDeckList.Items.Add(xyz);
                int id = Module1.findID(xyz);
                Image refer = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string synchro in synchroList)
            {
                lstExtraDeckList.Items.Add(synchro);
                int id = Module1.findID(synchro);
                Image refer = GetExtraDeckPictureBox((short)lstExtraDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }


            foreach (string monster in sideMonsterList)
            {
                lstSideDeckList.Items.Add(monster);
                int id = Module1.findID(monster);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string spell in sideSpellList)
            {
                lstSideDeckList.Items.Add(spell);
                int id = Module1.findID(spell);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string trap in sideTrapList)
            {
                lstSideDeckList.Items.Add(trap);
                int id = Module1.findID(trap);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string fusion in sideFusionList)
            {
                lstSideDeckList.Items.Add(fusion);
                int id = Module1.findID(fusion);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string xyz in sideXyzList)
            {
                lstSideDeckList.Items.Add(xyz);
                int id = Module1.findID(xyz);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            foreach (string synchro in sideSynchroList)
            {
                lstSideDeckList.Items.Add(synchro);
                int id = Module1.findID(synchro);
                Image refer = GetSideDeckPictureBox((short)lstSideDeckList.Items.Count); UpdatePictureBox(ref refer, Module1.CardStats[id]);
            }
            CountUpDeckStatistics();
        }

        private void cmdResetSearch_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = "";
            txtDesc.Text = "";
            cmbSet.SelectedIndex = 0;
            chkMonsters.IsChecked = false;
            chkSpells.IsChecked = false;
            chkTraps.IsChecked = false;
            chkMonsters.IsChecked = true;
            chkSpells.IsChecked = true;
            chkTraps.IsChecked = true;
            cmdSearch.Focus();
        }

        private void fraSearch_Navigated_1(object sender, NavigationEventArgs e)
        {

        }

        private void Root_Loaded_1(object sender, RoutedEventArgs e)
        {
            cmdSearch_Click(null, null);
        }




    }
}
