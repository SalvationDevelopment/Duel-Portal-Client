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
 public partial class DeckEditorNew : Page
    {
        public SQLReference.Service1ConsoleClient SQLcli;
        public List<string> DeckBuffer = new List<string>();// = new M.ShallowCardDetails[10001];
        public int NumberOfPages;
        public int BufPageNum;
        const int CARDS_PER_PAGE = 16;
        const int LBLCARDLORE_MAXHEIGHT = 200;
        const int LBLCARDFLAVOR_MAXHEIGHT = 50;  
        public cldInput inputForm = new cldInput("");
        public int startingCombinedDeckNumber;
        public bool automaticDoubleClick = false;
       

        readonly double lstCurrentDeckListOriginalHeight;
        readonly double lstExtraDeckListOriginalHeight;
        public DeckEditorNew()
        { 
            MouseLeftButtonUp += DeckEditor_MouseLeftButtonUp;
            InitializeComponent();
            lstCurrentDeckListOriginalHeight = lstCurrentDeckList.Height;
            lstExtraDeckListOriginalHeight = lstExtraDeckList.Height;
            lblCardLore.MaxHeight = LBLCARDLORE_MAXHEIGHT;
            lblCardFlavor.MaxHeight = LBLCARDFLAVOR_MAXHEIGHT;
        }

        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
           
            M.ScaleCanvas(Root, this);
            M.ScreenResized += (oldW, oldH) =>
                {
                    M.ScaleCanvas(Root, this);
                };
            if (!M.isLoggedIn)
            {
                
                MessageBox.Show("You cannot use this function until you are Logged In.");
                this.NavigationService.Navigate(new System.Uri("/Home", UriKind.Relative));
                return;
            }
            this.NavigationCacheMode = System.Windows.Navigation.NavigationCacheMode.Disabled;
            cmdSearch.Focus();
            
            ctxtSideDeck.myArea = ContextMenu.Area.Side_Deck;
            ctxtSideDeck.onLoaded();
            ctxtSideDeck.RenderTransform = new TranslateTransform();
            ctxtSideDeck.Visibility = System.Windows.Visibility.Collapsed;
            ctxtSideDeck.Item_Clicked += SideDeckItem_Clicked;
         
            SQLcli = new SQLReference.Service1ConsoleClient();
            SQLcli.NewDeckCompleted += createdNewDeck;
            SQLcli.SaveDeckCompleted += client_SaveComplete;
            SQLcli.loadDeckCompleted += doneLoadingDeck;
            SQLcli.deleteDeckCompleted += SQLcli_deleteDeckAsyncCompleted;

            List<string> listOfSets = new List<string>();
            for (int n = 1; n <= M.TotalCards; n++)
            {
                if (M.CardStats[n].SpecialSet != null && !listOfSets.Contains(M.CardStats[n].SpecialSet))
                    listOfSets.Add(M.CardStats[n].SpecialSet);
            }
            listOfSets.Sort();
            foreach (string setName in listOfSets)
            { cmbSet.Items.Add(setName); }

          

             if (M.sideDecking)
            {

                M.warnOnExitMessage = "You're currently sidedecking for another duel. If you exit now, you cannot return to your match. Exit anyway?";
                cmdSearch.IsEnabled = false;
                lstLoadedCards.Items.Clear();
                cmdSave.IsEnabled = false;
                cmdSaveAs.IsEnabled = false;
                cmbLoadDeck.IsEnabled = false;
                cmdPrevPage.IsEnabled = false;
                cmdNextPage.IsEnabled = false;
               cmdDeleteDeck.IsEnabled = false;
                chkTournament.IsEnabled = false;
                chkTraditional.IsEnabled = false;
                cmdDoneSideDecking.Visibility = System.Windows.Visibility.Visible;
                cmdClear.IsEnabled = false;
                 int n;
                for (n = 1; n < M.realDeckIDs.Count; n++)
                {
                    addToMainOrExtraDeck(M.realDeckIDs[n]);
                }
                for (n = 1; n < M.realEDeckIDs.Count; n++)
                {
                    addToMainOrExtraDeck(M.realEDeckIDs[n]); 
                }
                startingCombinedDeckNumber = M.realDeckIDs.Count + M.realEDeckIDs.Count - 2; //Because both their [0] is empty
                for (n = 1; n < M.realSideDeckIDs.Count; n++)
                {
                    addToSideDeck(M.realSideDeckIDs[n]);
                }
            }
            else
            {
                cmdDoneSideDecking.Visibility = System.Windows.Visibility.Collapsed;
                foreach (string deck in M.listOfMyDecks)
                {
                    cmbLoadDeck.Items.Add(deck);
                }
                M.defaultDeckName = M.GetCookie("default");
               
            }

        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            M.sideDecking = false;
            base.OnNavigatedFrom(e);
        }
        #region "Properties and Misc"
        private int ActualMainDeckCount
        {
            get
            {
                int total = 0;
                foreach (string item in lstCurrentDeckList.Items)
                {
                    total += GetNumOfCopies(item);
                }
                return total;
            }
        }
        private int ActualExtraDeckCount
        {
            get
            {
                int total = 0;
                foreach (string item in lstExtraDeckList.Items)
                {
                    total += GetNumOfCopies(item);
                }
                return total;
            }
        }
        private int ActualSideDeckCount
        {
            get
            {
                int total = 0;
                foreach (string item in lstSideDeckList.Items)
                {
                    total += GetNumOfCopies(item);
                }
                return total;
            }
        }
        private int cardsOverSidedeckLimit
        {
            get
            {
                return ActualMainDeckCount + ActualExtraDeckCount - startingCombinedDeckNumber;
            }
        }
        public Image GetExtraDeckPictureBox(int index)
        {
            return (Image)Root.FindName("imgExtra" + index);
        }
        public Image GetMainDeckPictureBox(int index)
        {
            return (Image)Root.FindName("imgMain" + index);
        }
        public Image GetSideDeckPictureBox(int index)
        {
            return (Image)Root.FindName("imgSide" + index);
        }
        public Image GetBanIcon(Image hostImage)
        {
            string banName = "ban" + hostImage.Name.Substring(3, hostImage.Name.Length - 3);
            if (hostImage.Parent as Canvas == null) return null;
            return (hostImage.Parent as Canvas).FindName(banName) as Image;
        }
        public Image GetBanIcon(int imageIndex, Canvas hostCanvas)
        {
            if (hostCanvas.Name == "cnvMainDeck")
                return hostCanvas.FindName("banMain" + imageIndex) as Image;
            if (hostCanvas.Name == "cnvExtraDeck")
                return hostCanvas.FindName("banExtra" + imageIndex) as Image;
            if (hostCanvas.Name == "cnvSideDeck")
                return hostCanvas.FindName("banSide" + imageIndex) as Image;
            return null;

        }
        public string GetFileNameFromLimit(int limit)
        {
            switch (limit)
            {
                case 2:
                    return "SemiLimited.png";
                case 1:
                    return "Limited.png";
                case 0:
                    return "Banned.png";
                default:
                    return "";
            }
        }
        private void AddToNumberedList(ListBox list, string item)
        {
            for (int n = 0; n < list.Items.Count; n++)
            {
                string iContentToString = list.Items[n].ToString();
                if (iContentToString.Substring(2, iContentToString.Length - 2) == item)
                {
                    list.Items[n] = (GetNumOfCopies(iContentToString) + 1) + " " +
                                 GetActualName(iContentToString);
                    return;
                }
            }

            list.Items.Add("1 " + item);
        }
        private int RemoveFromNumberedList(ListBox list, int index)
        {
            string iContentToString = list.Items[index].ToString();
            int numCopies = GetNumOfCopies(iContentToString);
            if (numCopies == 1)
                list.Items.RemoveAt(index);
            else
                list.Items[index] = (numCopies - 1) + " " + GetActualName(list.Items[index].ToString());
            return index;
        }
        private int GetNumOfCopies(ListBox list, string searchString)
        {
            foreach (string item in list.Items)
            {
                if (GetActualName(item) == searchString)
                {
                    return GetNumOfCopies(item);
                }
            }
            return 0;
        }
        private int GetNumOfCopies(string item)
        {
            if (item == null) return 0;
            return Convert.ToInt32(item.Substring(0, 1));
        }
        private string GetActualName(string item)
        {
            return item.Substring(2, item.Length - 2);
        }
        private bool ContainsActualName(ListBox list, string actualname)
        {
            foreach (string item in list.Items)
            {
                if (GetActualName(item) == actualname)
                    return true;
            }
            return false;
        }
        private int ListboxToImageIndex(ListBox list, string listItem)
        {
            int index = 0;
            foreach (string i in list.Items)
            {
                if (GetActualName(i) == listItem)
                    return index;
                else
                {
                    int num = GetNumOfCopies(i);
                    index += num;
                }
            }
            return -1;
        }
        private int ListboxToImageIndex(ListBox list, int listboxIndex)
        {
            int total = 0;
            for (int n = 0; n < listboxIndex; n++)
            {
                total += GetNumOfCopies(list.Items[n].ToString());
            }
            return total + 1;
        }
        private int ImageToListboxIndex(ListBox list, int ImageIndex)
        {
             int index = 0;
            for (int n = 0; n < list.Items.Count; n++)
            {
                index += GetNumOfCopies(list.Items[n].ToString());
                if (ImageIndex <= index)
                    return n;
            }
            return -1;
        }
        private int ImageToListboxIndex(ListBox list, string actualName)
        {
            int index = 0;
            for (int n = 0; n < list.Items.Count; n++)
            {
                if (GetActualName(list.Items[n].ToString()) == actualName)
                    return index + 1;
                else
                    index += GetNumOfCopies(list.Items[n].ToString());
            }
            return -1;
        }
        private int ImageIndex(Image image)
        {
            switch (image.Name.Substring(0, 7))
            {
                case "imgMain":
                case "imgSide":
                    return Convert.ToInt32(image.Name.Substring(7, image.Name.Length - 7));
                case "imgExtr":
                    return Convert.ToInt32(image.Name.Substring(8, image.Name.Length - 8));
                default:
                    return -1;
            }
        }
        private double GetMainDeckOverlap(Image sampleImage, int maxToRow = 10)
        {
            return ((sampleImage.Width * maxToRow) - brdMainDeck.Width) / (maxToRow - 1); 
        }
        private double GetExtraOrSideDeckOverlap(Image sampleImage, int maxToRow = 15)
        {
            return ((sampleImage.Width * maxToRow) - brdExtraDeck.Width) / (maxToRow - 1);
        }
        public void MsgBox(string text, System.Windows.MessageBoxButton msgtype = MessageBoxButton.OK)
        {
            MessageBox.Show(text, "", msgtype);
        }
        #endregion

        public void CountUpDeckStatistics()
        {
            int MainDeckIntCount = 0;
            int SpellIntCount = 0;
            int MonsterIntCount = 0;
            int TrapIntCount = 0;
           
            int TheID = 0;
            int copies;
            int n = 0;


            if (lstCurrentDeckList.Items.Count != 0)
            {
                for (n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
                {
                    TheID = M.findID(GetActualName(lstCurrentDeckList.Items[n].ToString()));
                    copies = GetNumOfCopies(lstCurrentDeckList.Items[n].ToString());
                    if (M.CardStats[TheID].IsMonster())
                        MonsterIntCount += copies;
                    else if (M.CardStats[TheID].Attribute == "Spell")
                        SpellIntCount += copies;
                    else if (M.CardStats[TheID].Attribute == "Trap")
                        TrapIntCount += copies;
                    MainDeckIntCount += copies;
                }

            }



            lblDeckStats.Text = "Monsters: " + Convert.ToString(MonsterIntCount) + " | Spells : " + Convert.ToString(SpellIntCount) + " | Traps: " + Convert.ToString(TrapIntCount) + " | Total: " + Convert.ToString(MainDeckIntCount)
                              + " | Extra: " + Convert.ToString(ActualExtraDeckCount) + " | Side: " + Convert.ToString(ActualSideDeckCount);



        }

        private void SideDeck_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image senderImage = sender as Image;
            int n = 1;
            Image img = null;
            while (GetSideDeckPictureBox(n) != null)
            {
                img = GetSideDeckPictureBox(n);
                if (object.ReferenceEquals(senderImage, img))
                    Canvas.SetZIndex(img, 1);
                else
                    Canvas.SetZIndex(img, 0);

                n++;
            }
        }
        private void ExtraDeck_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image senderImage = sender as Image;
            int n = 1;
            Image img = null;
            while (GetExtraDeckPictureBox(n) != null)
            {
                img = GetExtraDeckPictureBox(n);
                if (object.ReferenceEquals(senderImage, img))
                    Canvas.SetZIndex(img, 1);
                else
                    Canvas.SetZIndex(img, 0);

                n++;
            }
        }
        private void MainDeck_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image senderImage = sender as Image;
            int n = 1;
            Image img = null;
            while (GetMainDeckPictureBox(n) != null)
            {
                img = GetMainDeckPictureBox(n);
                if (object.ReferenceEquals(senderImage, img))
                    Canvas.SetZIndex(img, 1);
                else
                    Canvas.SetZIndex(img, 0);

                n++;
            }

        }

        public void MainDeck_MouseClick(System.Object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            int indx = ImageToListboxIndex(lstCurrentDeckList, ImageIndex(img));

            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = true;
                lstCurrentDeckList_MouseLeftButtonUp(sender, e);
            }
            else
                lstCurrentDeckList.SelectedIndex = indx;

        }
        private void ExtraDeck_MouseClick(System.Object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            int indx = ImageToListboxIndex(lstExtraDeckList, ImageIndex(img));

            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = true;
                lstExtraDeckList_MouseLeftButtonUp(sender, e);
            }
            else
                lstExtraDeckList.SelectedIndex = indx;
        }
        private void SideDeck_MouseClick(System.Object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;
            int indx = ImageToListboxIndex(lstSideDeckList, ImageIndex(img));

            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = true;
                lstSideDeckList_MouseLeftButtonUp(sender, e);
            }
            else
                lstSideDeckList.SelectedIndex = indx;
        }





        private void cmdClear_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult input = MessageBox.Show("Are you sure you want to clear your deck?", "", MessageBoxButton.OKCancel);
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
            cnvMainDeck.Children.Clear();
            cnvExtraDeck.Children.Clear();
            cnvSideDeck.Children.Clear();
            CountUpDeckStatistics();

            M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?";
        }

        #region "Banlist"
        private string BanlistProblems()
        {
            Dictionary<int, int> idToCopies = new Dictionary<int, int>();
            foreach (string item in lstCurrentDeckList.Items)
            {
                int id = M.findID(GetActualName(item));
                int copies = GetNumOfCopies(item);
                idToCopies.Add(id, copies);
            }
            foreach (string item in lstExtraDeckList.Items)
            {
                int id = M.findID(GetActualName(item));
                int copies = GetNumOfCopies(item);
                idToCopies.Add(id, copies);
            }
            foreach (string item in lstSideDeckList.Items)
            {
                int id = M.findID(GetActualName(item));
                int copies = GetNumOfCopies(item);
                if (idToCopies.ContainsKey(id))
                    idToCopies[id] += copies;
                else
                    idToCopies.Add(id, copies);
            }

            System.Text.StringBuilder problemBuilder = new System.Text.StringBuilder();
            
            foreach (KeyValuePair<int, int> kv in idToCopies)
            {
                if (kv.Value > M.CardStats[kv.Key].Limit)
                {
                    if (problemBuilder.ToString() == "")
                        problemBuilder.Append("The following cards violate the tournament banlist: " + 
                            M.CardStats[kv.Key].Name + " (@" + M.CardStats[kv.Key].Limit + ")");
                    else
                        problemBuilder.Append(", " + M.CardStats[kv.Key].Name + " (@" + M.CardStats[kv.Key].Limit + ")");

                }
            }
            return problemBuilder.ToString();
        }
        private void changedBanlist()
        {
            if ((bool)chkTournament.IsChecked)
            {
                string problems = BanlistProblems();
                if (problems != "")
                {
                    MessageBox.Show(problems);
                    chkTraditional.IsChecked = true;
                    return;
                }
                /*
                Image img;
                Image ban;
                int iterator = 1;
                while ((img = GetMainDeckPictureBox(iterator)) != null)
                {
                    ban = GetBanIcon(img);
                    if (ban != null)
                    {
                        int listIndex = ImageToListboxIndex(lstCurrentDeckList, iterator);
                        int id = M.findID(GetActualName(lstCurrentDeckList.Items[listIndex].ToString()));
                        M.setImage(ban, GetFileNameFromLimit(M.CardStats[id].Limit), UriKind.Relative);
                    }
                    iterator++;
                }
                iterator = 1;
                while ((img = GetExtraDeckPictureBox(iterator)) != null)
                {
                    ban = GetBanIcon(img);
                    if (ban != null)
                    {
                        int listIndex = ImageToListboxIndex(lstExtraDeckList, iterator);
                        int id = M.findID(GetActualName(lstExtraDeckList.Items[listIndex].ToString()));
                        M.setImage(ban, GetFileNameFromLimit(M.CardStats[id].Limit), UriKind.Relative);
                    }
                    iterator++;
                }
                iterator = 1;
                while ((img = GetSideDeckPictureBox(iterator)) != null)
                {
                    ban = GetBanIcon(img);
                    if (ban != null)
                    {
                        int listIndex = ImageToListboxIndex(lstSideDeckList, iterator);
                        int id = M.findID(GetActualName(lstSideDeckList.Items[listIndex].ToString()));
                        M.setImage(ban, GetFileNameFromLimit(M.CardStats[id].Limit), UriKind.Relative);
                    }
                    iterator++;
                }

            }
            else
            {
                Image img;
                Image ban;
                int iterator = 1;
                while ((img = GetMainDeckPictureBox(iterator)) != null)
                {
                    ban = GetBanIcon(img);
                    if (ban != null)
                    {
                        M.setImage(ban, "", UriKind.Relative);
                    }
                    iterator++;
                }
                iterator = 1;
                while ((img = GetExtraDeckPictureBox(iterator)) != null)
                {
                    ban = GetBanIcon(img);
                    if (ban != null)
                    {
                        M.setImage(ban, "", UriKind.Relative);
                    }
                    iterator++;
                }
                iterator = 1;
                while ((img = GetSideDeckPictureBox(iterator)) != null)
                {
                    ban = GetBanIcon(img);
                    if (ban != null)
                    {
                        M.setImage(ban, "", UriKind.Relative);
                    }
                    iterator++;
                }*/
            }
        }
        private void chkTraditional_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkTraditional.IsChecked == false)
                return;


            chkTraditional.IsEnabled = false;
            chkTournament.IsChecked = false;
            chkTournament.IsEnabled = true;
            changedBanlist();
        }
        private void chkTournament_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (chkTournament == null)
                return;
           if (chkTournament.IsChecked == false)
                return;


           chkTournament.IsEnabled = false;
           chkTraditional.IsChecked = false;
           chkTraditional.IsEnabled = true;
           changedBanlist();
        }
        #endregion



        private void cmdSearch_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {

             if (radPublic.IsChecked == true)
            {
                searchCards(true);
            }
            else
                searchCards(false);

        }



        #region "Searching"
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
        private void txtNameAndDesc_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cmdSearch != null)
            {
                cmdSearch_Click(null, new RoutedEventArgs());
                txtNameAndDesc.Focus();
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
        private void cmdResetSearch_Click(object sender, RoutedEventArgs e)
        {
            txtNameAndDesc.Text = "";
            cmbSet.SelectedIndex = 0;
            chkMonsters.IsChecked = false;
            chkSpells.IsChecked = false;
            chkTraps.IsChecked = false;
            chkMonsters.IsChecked = true;
            chkSpells.IsChecked = true;
            chkTraps.IsChecked = true;
            cmdSearch.Focus();
        }
        private void searchCards(bool isPublic)
        {
            int n = 0;
            DeckBuffer.Clear();
            DeckBuffer.Add("");

            lstLoadedCards.Focus();
            lstLoadedCards.Items.Clear();

             

            int bufnum = 1;

            if (!string.IsNullOrEmpty(txtNameAndDesc.Text))
                txtNameAndDesc.Text = txtNameAndDesc.Text.Trim();
            for (n = 1; n <= M.TotalCards; n++)
            {
                if (M.CardStats[n].Name == null)
                    continue;
                if (string.IsNullOrEmpty(M.CardStats[n].Name))
                    continue;


                if (!string.IsNullOrEmpty(txtNameAndDesc.Text))
                {
                    if (M.CardStats[n].Name.IndexOf(txtNameAndDesc.Text, StringComparison.OrdinalIgnoreCase) == -1
                        && M.CardStats[n].Lore.IndexOf(txtNameAndDesc.Text, StringComparison.OrdinalIgnoreCase) == -1)
                        continue;
                }

                if (isPublic && M.CardStats[n].Creator != "PUB" ||
                    !isPublic && M.CardStats[n].Creator == "PUB")
                    continue;

                if (cmbSet.SelectionBoxItem.ToString() != "All")
                {
                    if (M.CardStats[n].SpecialSet != cmbSet.SelectionBoxItem.ToString())
                    {
                        continue;
                    }

                }


                //Monsters is checked
                if (chkMonsters.IsChecked == true)
                {
                    if (cmbAttribute.SelectionBoxItem.ToString() != "All Attributes")
                    {
                        if (M.CardStats[n].Attribute != cmbAttribute.SelectionBoxItem.ToString())
                        {
                            continue;
                        }
                    }

                    if (cmbType.SelectionBoxItem.ToString() != "All Types")
                    {
                        if (M.CardStats[n].Type.IndexOf(cmbType.SelectionBoxItem.ToString(), StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            continue;
                        }
                        else if (cmbType.SelectionBoxItem.ToString() == "Beast" || cmbType.SelectionBoxItem.ToString() == "Warrior")
                        {
                            if (M.CardStats[n].Type.IndexOf("Beast-Warrior", StringComparison.OrdinalIgnoreCase) != -1 || M.CardStats[n].Type.IndexOf("Winged Beast", StringComparison.OrdinalIgnoreCase) != -1)
                                continue;
                        }

                    }

                    if (cmbEffect.SelectionBoxItem.ToString() != "All Subtypes" && cmbEffect.SelectionBoxItem.ToString() != "Normal")
                    {
                        if (!M.CardStats[n].Type.Contains(cmbEffect.SelectionBoxItem.ToString()))
                        {
                            continue;
                        }
                    }

                    if (cmbEffect.SelectionBoxItem.ToString() == "Normal")
                    {
                        if (M.CardStats[n].Type.Contains("Effect"))
                        {
                            continue;
                        }
                    }

                    if (cmbLevel.SelectionBoxItem != null && !string.IsNullOrEmpty(txtLevel.Text))
                    {
                        if (cmbLevel.SelectionBoxItem.ToString() == "<=")
                        {
                            if (txtLevel.Text == "?" && M.CardStats[n].Level != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtLevel.Text) && M.CardStats[n].Level > Convert.ToInt32(txtLevel.Text))
                                continue;
                        }

                        if (cmbLevel.SelectionBoxItem.ToString() == ">=")
                        {
                            if (txtLevel.Text == "?" && M.CardStats[n].Level != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtLevel.Text) && M.CardStats[n].Level < Convert.ToInt32(txtLevel.Text))
                                continue;
                        }

                        if (cmbLevel.SelectionBoxItem.ToString() == "=")
                        {
                            if (txtLevel.Text == "?" && M.CardStats[n].Level != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtLevel.Text) && M.CardStats[n].Level != Convert.ToInt32(txtLevel.Text))
                                continue;
                        }
                    }

                    if (cmbATK.SelectionBoxItem != null && !string.IsNullOrEmpty(txtATK.Text))
                    {
                        if (cmbATK.SelectionBoxItem.ToString() == "<=")
                        {
                            if (txtATK.Text == "?" && M.CardStats[n].ATK != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtATK.Text) && M.CardStats[n].ATK > Convert.ToInt32(txtATK.Text))
                                continue;
                        }

                        if (cmbATK.SelectionBoxItem.ToString() == ">=")
                        {
                            if (txtATK.Text == "?" && M.CardStats[n].ATK != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtATK.Text) && M.CardStats[n].ATK < Convert.ToInt32(txtATK.Text))
                                continue;
                        }

                        if (cmbATK.SelectionBoxItem.ToString() == "=")
                        {
                            if (txtATK.Text == "?" && M.CardStats[n].ATK != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtATK.Text) && M.CardStats[n].ATK != Convert.ToInt32(txtATK.Text))
                                continue;
                        }
                    }

                    if (cmbDEF.SelectionBoxItem != null && !string.IsNullOrEmpty(txtDEF.Text))
                    {
                        if (cmbDEF.SelectionBoxItem.ToString() == "<=")
                        {
                            if (txtDEF.Text == "?" && M.CardStats[n].DEF != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtDEF.Text) && M.CardStats[n].DEF > Convert.ToInt32(txtDEF.Text))
                                continue;
                        }

                        if (cmbDEF.SelectionBoxItem.ToString() == ">=")
                        {
                            if (txtDEF.Text == "?" && M.CardStats[n].DEF != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtDEF.Text) && M.CardStats[n].DEF < Convert.ToInt32(txtDEF.Text))
                                continue;
                        }

                        if (cmbDEF.SelectionBoxItem.ToString() == "=")
                        {
                            if (txtDEF.Text == "?" && M.CardStats[n].DEF != M.STAT_QUESTION)
                                continue;
                            if (M.isNumeric(txtDEF.Text) && M.CardStats[n].DEF != Convert.ToInt32(txtDEF.Text))
                                continue;
                        }
                    }

                }

                //monsters is UNchecked
                if (chkMonsters.IsChecked == false)
                {
                    if (M.CardStats[n].Attribute != "Spell" && M.CardStats[n].Attribute != "Trap")
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
                            if (M.CardStats[n].Type != cmbSpells.SelectionBoxItem.ToString() || M.CardStats[n].Attribute != "Spell")
                            {
                                continue;
                            }


                            if (cmbSpells.SelectionBoxItem.ToString() == "Normal")
                            {
                                if (!string.IsNullOrEmpty(M.CardStats[n].Type))
                                {
                                    continue;
                                }

                            }
                        }
                    }
                }

                if (chkSpells.IsChecked == false)
                {
                    if (M.CardStats[n].Attribute == "Spell")
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
                            if (M.CardStats[n].Type != cmbTraps.SelectionBoxItem.ToString() || M.CardStats[n].Attribute != "Trap")
                            {
                                continue;
                            }


                            if (cmbTraps.SelectionBoxItem.ToString() == "Normal")
                            {
                                if (M.CardStats[n].Type == "Continuous" || M.CardStats[n].Type == "Quick-Play" || M.CardStats[n].Type == "Counter" || M.CardStats[n].Type == "Field" || M.CardStats[n].Type == "Ritual" || M.CardStats[n].Type == "Equip")
                                {
                                    continue;
                                }

                            }
                        }
                    }

                }

                if (chkTraps.IsChecked == false)
                {
                    if (M.CardStats[n].Attribute == "Trap")
                    {
                        continue;
                    }

                }



                if (lstLoadedCards.Items.Count < CARDS_PER_PAGE)
                {
                    lstLoadedCards.Items.Add(M.CardStats[n].Name);
                }

                //    DeckBuffer[bufnum].Name = M.CardStats[n].Name;
                //   DeckBuffer[bufnum].ID = n;
                DeckBuffer.Add(M.CardStats[n].Name);
                bufnum += 1;
            }

            NumberOfPages = Convert.ToInt32((bufnum - 1) / CARDS_PER_PAGE) + 1;
            BufPageNum = 1;
            lblPageNum.Text = "Page " + BufPageNum;
            cmdPrevPage.IsEnabled = false;
            if (NumberOfPages > 1)
                cmdNextPage.IsEnabled = true;
            else
                cmdNextPage.IsEnabled = false;
        }
        private void cmdNextPage_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            BufPageNum += 1;
            lblPageNum.Text = "Page " + BufPageNum;
            lstLoadedCards.Items.Clear();
            for (int n = (BufPageNum * CARDS_PER_PAGE) - CARDS_PER_PAGE + 1; n <= (BufPageNum * CARDS_PER_PAGE); n++)
            {
                try
                {
                    lstLoadedCards.Items.Add(DeckBuffer[n]);
                }
                catch
                {
                    break;
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
            lblPageNum.Text = "Page " + BufPageNum;
            lstLoadedCards.Items.Clear();
            for (int n = (BufPageNum * CARDS_PER_PAGE) - CARDS_PER_PAGE + 1; n <= (BufPageNum * CARDS_PER_PAGE); n++)
            {
                try
                {
                    lstLoadedCards.Items.Add(DeckBuffer[n]);
                }
                catch
                {
                    break;
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
        #endregion

        #region "Card Displaying"
        private void lstLoadedCards_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                if (lstLoadedCards.SelectedItem == null)
                    return;
                int id = M.findID(lstLoadedCards.SelectedItem.ToString());
                addToMainOrExtraDeck(id);
                if (M.sideDecking == false) { M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; }
                if (M.sideDecking)
                {
                    lstLoadedCards.Items.RemoveAt(lstLoadedCards.SelectedIndex);

                }
            }
        }
        private void lstLoadedCards_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int id = 0;
            if (lstLoadedCards.SelectedIndex == -1)
                return;
            id = M.findID(lstLoadedCards.SelectedItem.ToString());
            selectionChanged(id);
        }
       
        private void lstCurrentDeckList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstCurrentDeckList.Height = lstCurrentDeckListOriginalHeight;
            lstExtraDeckList.Visibility = System.Windows.Visibility.Visible;
            lstSideDeckList.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetZIndex(lstCurrentDeckList, 0);
        }
        private void lstCurrentDeckList_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstCurrentDeckList.Height = Canvas.GetTop(lstSideDeckList) + lstSideDeckList.Height - Canvas.GetTop(lstCurrentDeckList);
            lstExtraDeckList.Visibility = System.Windows.Visibility.Collapsed;
            lstSideDeckList.Visibility = System.Windows.Visibility.Collapsed;
            Canvas.SetZIndex(lstCurrentDeckList, 1);
        }

        private void lstExtraDeckList_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstExtraDeckList.Height = Canvas.GetTop(lstSideDeckList) + lstSideDeckList.Height - Canvas.GetTop(lstExtraDeckList);
            lstSideDeckList.Visibility = System.Windows.Visibility.Collapsed;
            Canvas.SetZIndex(lstExtraDeckList, 1);
        }
        private void lstExtraDeckList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lstExtraDeckList.Height = lstExtraDeckListOriginalHeight;
            lstSideDeckList.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetZIndex(lstExtraDeckList, 0);
        }

        private void selectionChanged(int id)
        {
            try
            {
                lblCardName.Text = M.CardStats[id].Name;
                M.setImage(imgCardAttribute, M.AttributeToImageName(M.CardStats[id].Attribute), UriKind.Relative);
                if (!M.CardStats[id].IsMonster())
                {
                    staLevel.Children.Clear();
                    lblCardLevel.Text = "";
                    lblCardATK.Text = "";
                    lblCardDEF.Text = "";
                    lblCardType.Text = "";

                    bordCardATK.Visibility = System.Windows.Visibility.Collapsed;
                    bordCardDEF.Visibility = System.Windows.Visibility.Collapsed;
                    lblSlash.Visibility = System.Windows.Visibility.Collapsed;

                    M.setImage(imgSTIcon, M.TypeToImageName(M.CardStats[id].Type), UriKind.Relative);
                }
                else
                {
                    bordCardATK.Visibility = System.Windows.Visibility.Visible;
                    bordCardDEF.Visibility = System.Windows.Visibility.Visible;
                    lblSlash.Visibility = System.Windows.Visibility.Visible;

                    bool isXyz = M.CardStats[id].Type.Contains("Xyz");
                    lblCardLevel.Text = isXyz ? "Rank " : "Level " + M.CardStats[id].Level;

                    staLevel.Children.Clear();
                    staLevel.FlowDirection = isXyz ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
                    for (int n = 1; n <= M.CardStats[id].Level; n++)
                    {
                        Image star = new Image();
                        star.Width = 17;
                        star.Height = 17;
                        M.setImage(star, isXyz ? "RankStar.jpg" : "Star.jpg", UriKind.Relative);
                        staLevel.Children.Add(star);
                    }
                    
                    lblCardATK.Text = M.CardStats[id].ATK.ToStringCountingQuestions();
                    lblCardDEF.Text = M.CardStats[id].DEF.ToStringCountingQuestions();
                    M.setImage(imgSTIcon, "", UriKind.Relative);
                    lblCardType.Text = M.CardStats[id].Type;
                }
                if (M.CardStats[id].Flavor != null)
                {
                    lblCardLore.MaxHeight = LBLCARDLORE_MAXHEIGHT - LBLCARDFLAVOR_MAXHEIGHT;
                    lblCardFlavor.Visibility = System.Windows.Visibility.Visible;
                    lblCardFlavor.Text = M.CardStats[id].Flavor;
                }
                else
                {
                    lblCardLore.MaxHeight = LBLCARDLORE_MAXHEIGHT;
                    lblCardFlavor.Visibility = System.Windows.Visibility.Collapsed;
                }
                lblCardLore.Text = M.CardStats[id].Lore;
             
                lblSetName.Text = M.CardStats[id].SpecialSet;
                UpdatePictureBox(ref imgChosenCard, M.CardStats[id]);
            }
            catch (ArgumentException)
            {
                M.setImage(imgChosenCard, "token.jpg", UriKind.Relative);
                lblCardName.Text = "???";
                lblCardType.Text = "???";
                M.setImage(imgCardAttribute, "Unknown.png", UriKind.Relative);
                lblCardLevel.Text = "???";
                lblCardType.Text = "???";
                lblCardATK.Text = "???";
                lblCardDEF.Text = "???";
                lblCardLore.Text = "Card not found";
            }
        }

        private void lstCurrentDeckList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstCurrentDeckList.SelectedIndex == -1)
                return;
            cardID = M.findID( GetActualName( lstCurrentDeckList.SelectedItem.ToString()));
            if (cardID == 0)
                throw new ArgumentException();
            selectionChanged(cardID);
        }
        private void lstExtraDeckList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstExtraDeckList.SelectedIndex == -1)
                return;
            cardID = M.findID(GetActualName(lstExtraDeckList.SelectedItem.ToString()));
            if (cardID == 0)
                throw new ArgumentException();
            selectionChanged(cardID);

        }
        private void lstSideDeckList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int cardID = 0;
            if (lstSideDeckList.SelectedIndex == -1)
                return;
            cardID = M.findID(GetActualName(lstSideDeckList.SelectedItem.ToString()));
            if (cardID == 0)
                throw new ArgumentException();
            selectionChanged(cardID);
        }
        private void lstCurrentDeckList_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (automaticDoubleClick || ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = false;
                if (lstCurrentDeckList.SelectedIndex == -1 || lstCurrentDeckList.SelectedItem == null) return;
                
                int index =  ListboxToImageIndex(lstCurrentDeckList, lstCurrentDeckList.SelectedIndex);
                Image image = GetMainDeckPictureBox(index);
                 if (M.sideDecking)
                    addToSideDeck(M.findID(GetActualName(lstCurrentDeckList.SelectedItem.ToString())));
                 else
                    M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; 
                if (GetBanIcon(image) != null) cnvMainDeck.Children.Remove(GetBanIcon(image));
                cnvMainDeck.Children.Remove(image);
                RemoveFromNumberedList(lstCurrentDeckList, lstCurrentDeckList.SelectedIndex);
                ShiftMainDeckLeft(index);

                CountUpDeckStatistics();
            }
        }
        private void lstExtraDeckList_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (automaticDoubleClick || ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = false;
                if (lstExtraDeckList.SelectedIndex == -1 || lstExtraDeckList.SelectedItem == null) return;

                int index = ListboxToImageIndex(lstExtraDeckList, lstExtraDeckList.SelectedIndex);
                Image image = GetExtraDeckPictureBox(index);
                if (M.sideDecking)
                    addToSideDeck(M.findID(GetActualName(lstExtraDeckList.SelectedItem.ToString())));
                else
                    M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; 
                if (GetBanIcon(image) != null) cnvExtraDeck.Children.Remove(GetBanIcon(image));
                cnvExtraDeck.Children.Remove(image);
                RemoveFromNumberedList(lstExtraDeckList, lstExtraDeckList.SelectedIndex);
                ShiftExtraDeckLeft(index);
            }
            CountUpDeckStatistics();
        }
        private void lstSideDeckList_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (automaticDoubleClick || ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                automaticDoubleClick = false;
                if (lstSideDeckList.SelectedIndex == -1 || lstSideDeckList.SelectedItem == null) return;

                int index = ListboxToImageIndex(lstSideDeckList, lstSideDeckList.SelectedIndex);
                Image image = GetSideDeckPictureBox(index);
                if (M.sideDecking)
                    addToMainOrExtraDeck(M.findID(GetActualName(lstSideDeckList.SelectedItem.ToString())));
                else
                    M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?"; 
                if (GetBanIcon(image) != null) cnvSideDeck.Children.Remove(GetBanIcon(image));
                cnvSideDeck.Children.Remove(image);
                RemoveFromNumberedList(lstSideDeckList, lstSideDeckList.SelectedIndex);
                ShiftSideDeckLeft(index);
            }
            CountUpDeckStatistics();
        }
        public Image CreateExtraDeckPictureBox(int index, int cardID)
        {
            Image image = new Image();
            image.Width = 55;
            image.Height = 81;
            cnvExtraDeck.Children.Add(image);
            int lstIndex = ImageToListboxIndex(lstExtraDeckList, M.CardStats[cardID].Name);
            if (lstIndex == -1)
            {
                image.Name = "imgExtra" + index;
                AdjustExtraDeckImage(image, index, false, ActualExtraDeckCount + 1);
            }
            else
            {
                ShiftExtraDeckRight(lstIndex);
                image.Name = "imgExtra" + lstIndex;
                AdjustExtraDeckImage(image, lstIndex, false, ActualExtraDeckCount + 1);
            }
            return image;
        }
        public Image CreateMainDeckPictureBox(int index, int cardID)
        {
            Image image = new Image();
            image.Width = 55;
            image.Height = 81;
            //image.Opacity = 0.8;
            cnvMainDeck.Children.Add(image);
            int lstIndex = ImageToListboxIndex(lstCurrentDeckList, M.CardStats[cardID].Name);
            if (lstIndex == -1)
            {
                image.Name = "imgMain" + index;
                AdjustMainDeckImage(image, index, false, ActualMainDeckCount + 1);
            }
            else
            {
                ShiftMainDeckRight(lstIndex);
                image.Name = "imgMain" + lstIndex;
                AdjustMainDeckImage(image, lstIndex, false, ActualMainDeckCount + 1);
            }
            AddBanImage(image, cardID);
            if (image.Parent as Canvas == null) throw new Exception("Parent as canvas is null");
            return image;
        }
        public Image CreateSideDeckPictureBox(int index, int cardID)
        {
            Image image = new Image();
            image.Width = 55;
            image.Height = 81;
            cnvSideDeck.Children.Add(image);
            int lstIndex = ImageToListboxIndex(lstSideDeckList, M.CardStats[cardID].Name);
            if (lstIndex == -1)
            {
                image.Name = "imgSide" + index;
                AdjustSideDeckImage(image, index, false, ActualSideDeckCount + 1);
            }
            else
            {
                ShiftSideDeckRight(lstIndex);
                image.Name = "imgSide" + lstIndex;
                AdjustSideDeckImage(image, lstIndex, false, ActualSideDeckCount + 1);
            }
            return image;
        }
        public void AddBanImage(Image image, int id)
        {
            if (M.CardStats[id].Limit >= 3) return;
            Image ban = new Image();
            ban.Name = "ban" + image.Name.Substring(3, image.Name.Length - 3);
            ban.Width = 22;
            ban.Height = 22;
            
            switch (M.CardStats[id].Limit)
            {
                case 0:
                    M.setImage(ban, "Banned.png", UriKind.Relative);
                    break;
                case 1:
                    M.setImage(ban, "Limited.png", UriKind.Relative);
                    break;
                case 2:
                    M.setImage(ban, "SemiLimited.png", UriKind.Relative);
                    break;
            } 

            try
            {
                (image.Parent as Canvas).Children.Add(ban);
                AdjustBanIcon(image, ban);
            } catch { }
        }
        private void ShiftMainDeckLeft(int removedImageIndex)
        {
            Image image;
            int iterator = removedImageIndex + 1;
            while ( (image = GetMainDeckPictureBox(iterator)) != null)
            {
                image.Name = "imgMain" + (iterator - 1).ToString();
                AdjustMainDeckImage(image, iterator - 1, true, ActualMainDeckCount);
                Image banIcon = GetBanIcon(iterator, cnvMainDeck);
                if (banIcon != null)
                {
                    banIcon.Name = "banMain" + (iterator - 1).ToString();
                    AdjustBanIcon(image, banIcon);
                }
                iterator++;
            }
            if (ActualMainDeckCount >= 40)
            {
                for (int n = 1; n <= removedImageIndex - 1; n++)
                {
                    AdjustMainDeckImage(GetMainDeckPictureBox(n), n, true, ActualMainDeckCount);
                    Image banIcon = GetBanIcon(n, cnvMainDeck);
                    if (banIcon != null)
                    {
                        AdjustBanIcon(GetMainDeckPictureBox(n), banIcon);
                    }
                }

            }
        }
        private void ShiftExtraDeckLeft(int removedImageIndex)
        {
            Image image;
            int iterator = removedImageIndex + 1;
            while ((image = GetExtraDeckPictureBox(iterator)) != null)
            {
                image.Name = "imgExtra" + (iterator - 1).ToString();
                AdjustExtraDeckImage(image, iterator - 1, true, ActualExtraDeckCount);
                Image banIcon = GetBanIcon(iterator, cnvExtraDeck);
                if (banIcon != null)
                {
                    banIcon.Name = "banExtra" + (iterator - 1).ToString();
                    AdjustBanIcon(image, banIcon);
                }
                iterator++;
            }
            if (ActualExtraDeckCount >= 15)
            {
                for (int n = 1; n <= removedImageIndex - 1; n++)
                {
                    AdjustExtraDeckImage(GetExtraDeckPictureBox(n), n, true, ActualExtraDeckCount);
                    Image banIcon = GetBanIcon(n, cnvExtraDeck);
                    if (banIcon != null)
                    {
                        AdjustBanIcon(GetMainDeckPictureBox(n), banIcon);
                    }
                }

            }
        }
        private void ShiftSideDeckLeft(int removedImageIndex)
        {
            Image image;
            int iterator = removedImageIndex + 1;
            while ((image = GetSideDeckPictureBox(iterator)) != null)
            {
                image.Name = "imgSide" + (iterator - 1).ToString();
                AdjustSideDeckImage(image, iterator - 1, true, ActualSideDeckCount);
                Image banIcon = GetBanIcon(iterator, cnvSideDeck);
                if (banIcon != null)
                {
                    banIcon.Name = "banSide" + (iterator - 1).ToString();
                    AdjustBanIcon(image, banIcon);
                }
                iterator++;
            }
            if (ActualSideDeckCount >= 15)
            {
                for (int n = 1; n <= removedImageIndex - 1; n++)
                {
                    AdjustExtraDeckImage(GetSideDeckPictureBox(n), n, true, ActualSideDeckCount);
                    Image banIcon = GetBanIcon(n, cnvSideDeck);
                    if (banIcon != null)
                    {
                        AdjustBanIcon(GetSideDeckPictureBox(n), banIcon);
                    }
                }

            }
        }
        private void ShiftMainDeckRight(int displacedImageIndex)
        {
            Image image;
            int iterator = ActualMainDeckCount;
            while (iterator >= displacedImageIndex)
            {
                image = GetMainDeckPictureBox(iterator);
                image.Name = "imgMain" + (iterator + 1).ToString();
                AdjustMainDeckImage(image, iterator + 1, true, ActualMainDeckCount);
                Image banIcon = GetBanIcon(iterator, cnvMainDeck);
                if (banIcon != null)
                {
                    banIcon.Name = "banMain" + (iterator + 1).ToString();
                    AdjustBanIcon(image, banIcon);
                }
                iterator--;
            }
            if (ActualMainDeckCount >= 40)
            {
                for (int n = displacedImageIndex - 1; n >= 1; n--)
                {
                    AdjustMainDeckImage(GetMainDeckPictureBox(n), n, true, ActualMainDeckCount);
                    Image banIcon = GetBanIcon(n, cnvMainDeck);
                    if (banIcon != null)
                    {
                        AdjustBanIcon(GetMainDeckPictureBox(n), banIcon);
                    }
                }

            }
        }
        private void ShiftExtraDeckRight(int displacedImageIndex)
        {
            Image image;
            int iterator = ActualExtraDeckCount;
            while (iterator >= displacedImageIndex)
            {
                image = GetExtraDeckPictureBox(iterator);
                image.Name = "imgExtra" + (iterator + 1).ToString();
                AdjustExtraDeckImage(image, iterator + 1, true, ActualExtraDeckCount);
                Image banIcon = GetBanIcon(iterator, cnvExtraDeck);
                if (banIcon != null)
                {
                    banIcon.Name = "banExtra" + (iterator + 1).ToString();
                    AdjustBanIcon(image, banIcon);
                }
                iterator--;
            }
            if (ActualExtraDeckCount >= 15)
            {
                for (int n = displacedImageIndex - 1; n >= 1; n--)
                {
                    AdjustExtraDeckImage(GetExtraDeckPictureBox(n), n, true, ActualExtraDeckCount);
                    Image banIcon = GetBanIcon(n, cnvExtraDeck);
                    if (banIcon != null)
                    {
                        AdjustBanIcon(GetMainDeckPictureBox(n), banIcon);
                    }
                }

            }
        }
        private void ShiftSideDeckRight(int displacedImageIndex)
        {
            Image image;
            int iterator = ActualSideDeckCount;
            while (iterator >= displacedImageIndex)
            {
                image = GetSideDeckPictureBox(iterator);
                image.Name = "imgSide" + (iterator + 1).ToString();
                AdjustSideDeckImage(image, iterator + 1, true, ActualSideDeckCount);
                Image banIcon = GetBanIcon(iterator, cnvSideDeck);
                if (banIcon != null)
                {
                    banIcon.Name = "banSide" + (iterator + 1).ToString();
                    AdjustBanIcon(image, banIcon);
                }
                iterator--;
            }
            if (ActualSideDeckCount >= 15)
            {
                for (int n = displacedImageIndex - 1; n >= 1; n--)
                {
                    AdjustSideDeckImage(GetSideDeckPictureBox(n), n, true, ActualSideDeckCount);
                    Image banIcon = GetBanIcon(n, cnvSideDeck);
                    if (banIcon != null)
                    {
                        AdjustBanIcon(GetSideDeckPictureBox(n), banIcon);
                    }
                }

            }
        }
        private void AdjustMainDeckImage(Image image, int index, bool skipAdjustment, int mainDeckCount)
        {

            int maxToRow = ((mainDeckCount - 1) / 4) + 1;
            if (ActualMainDeckCount < 40)
            {
                Canvas.SetLeft(image, (image.Width * ((index - 1) % 10)) - (GetMainDeckOverlap(image, 10) * ((index - 1) % 10)));
                Canvas.SetTop(image, ((index - 1) / 10) * image.Height);
            }
            else
            {
                if (!skipAdjustment)
                {
                    for (int n = 1; n <= mainDeckCount; n++)
                    {
                        AdjustMainDeckImage(GetMainDeckPictureBox(n), n, true, mainDeckCount);
                        Image banIcon = GetBanIcon(n, cnvMainDeck);
                        if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(GetMainDeckPictureBox(n)) + 1);
                    }
                }
                Canvas.SetLeft(image, (image.Width * ((index - 1) % maxToRow)) - (GetMainDeckOverlap(image, maxToRow) * ((index - 1) % maxToRow)));
                Canvas.SetTop(image, ((index - 1) / maxToRow) * image.Height);
                
            }
            Canvas.SetZIndex(image, index);
        }
        private void AdjustExtraDeckImage(Image image, int index, bool skipAdjustment, int extraDeckCount)
        {
           
            if (ActualExtraDeckCount >= 15 && !skipAdjustment)
            {
                for (int n = 1; n <= extraDeckCount; n++)
                {
                    AdjustExtraDeckImage(GetExtraDeckPictureBox(n), n, true, extraDeckCount);
                    Image banIcon = GetBanIcon(n, cnvExtraDeck);
                    if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(GetExtraDeckPictureBox(n)) + 1);
                }
            }
            Canvas.SetLeft(image, ((index - 1) * image.Width) - (GetExtraOrSideDeckOverlap(image, extraDeckCount <= 15 ? 15 : extraDeckCount) * (index - 1)));
            Canvas.SetZIndex(image, index);
        }
        private void AdjustSideDeckImage(Image image, int index, bool skipAdjustment, int sideDeckCount)
        {
            if (ActualSideDeckCount >= 15 && !skipAdjustment)
            {
                for (int n = 1; n <= sideDeckCount; n++)
                {
                    AdjustExtraDeckImage(GetSideDeckPictureBox(n), n, true, sideDeckCount);
                    Image banIcon = GetBanIcon(n, cnvSideDeck);
                    if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(GetSideDeckPictureBox(n)) + 1);
                }
            }
            Canvas.SetLeft(image, ((index - 1) * image.Width) - (GetExtraOrSideDeckOverlap(image, sideDeckCount <= 15 ? 15 : sideDeckCount) * (index - 1)));
            Canvas.SetZIndex(image, index);
        }
        private void AdjustBanIcon(Image image, Image banIcon = null)
        {
            if (banIcon == null)
            {
                banIcon = GetBanIcon(image);
                if (banIcon == null) return;
            }
            Canvas.SetLeft(banIcon, image.CLeft());
            Canvas.SetTop(banIcon, image.CTop());
            Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
        }
        private void addToMainOrExtraDeck(int id)
        {
            if (M.BelongsInExtra(M.CardStats[id]))
            {
                if (ActualExtraDeckCount == 15 && !M.sideDecking)
                {
                    MsgBox("You can only have 15 cards in your extra deck.");
                   return;
                }
                if ((bool)chkTournament.IsChecked && !M.sideDecking 
                    && GetNumOfCopies(lstExtraDeckList, M.CardStats[id].Name) + GetNumOfCopies(lstSideDeckList, M.CardStats[id].Name) >= M.CardStats[id].Limit) 
                {
                    
                    if (M.CardStats[id].Limit == 0)
                        MsgBox("You cannot have any copies of " + M.CardStats[id].Name + " in your deck.");
                    else if (M.CardStats[id].Limit == 1)
                        MsgBox("You can only have " + M.CardStats[id].Limit + " copy of " + M.CardStats[id].Name + " in your deck.");
                    else
                        MsgBox("You can only have " + M.CardStats[id].Limit + " copies of " + M.CardStats[id].Name + " in your deck.");
                    return;
                     
                }
               Image image = CreateExtraDeckPictureBox(ActualExtraDeckCount + 1, id);
               UpdatePictureBox(ref image, M.CardStats[id]);
               image.MouseLeftButtonUp += ExtraDeck_MouseClick;
               image.MouseEnter += delegate
               {
                   Canvas.SetZIndex(image, Canvas.GetZIndex(image) + 2);
                   Image banIcon = GetBanIcon(image);
                   if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
               };
               image.MouseLeave += delegate
               {
                   Canvas.SetZIndex(image, ImageIndex(image));
                   Image banIcon = GetBanIcon(image);
                   if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
               };
               //Adding to list must  be AFTER adding image, not BEFORE
               AddToNumberedList(lstExtraDeckList, M.CardStats[id].Name);
            }
            else
            {
                if ((bool)chkTournament.IsChecked && !M.sideDecking 
                    && GetNumOfCopies(lstCurrentDeckList, M.CardStats[id].Name) + GetNumOfCopies(lstSideDeckList, M.CardStats[id].Name) >= M.CardStats[id].Limit)

                {
                   
                    if (M.CardStats[id].Limit == 0)
                        MsgBox("You cannot have any copies of " + M.CardStats[id].Name + " in your deck.");
                    else if (M.CardStats[id].Limit == 1)
                        MsgBox("You can only have " + M.CardStats[id].Limit + " copy of " + M.CardStats[id].Name + " in your deck.");
                    else
                        MsgBox("You can only have " + M.CardStats[id].Limit + " copies of " + M.CardStats[id].Name + " in your deck.");
                    return;
                    
                }
               Image image = CreateMainDeckPictureBox(ActualMainDeckCount + 1, id);
               image.ImageFailed += (s, e) =>
                   {
                       Image i = s as Image;
                       M.setImage(i, "Token.jpg", UriKind.Relative);
                   };
               UpdatePictureBox(ref image, M.CardStats[id]);
               image.MouseLeftButtonUp += MainDeck_MouseClick;
               image.MouseEnter += delegate 
                    { 
                        Canvas.SetZIndex(image, Canvas.GetZIndex(image) + 2);
                        Image banIcon = GetBanIcon(image);
                        if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
                    };
               image.MouseLeave += delegate 
                    { 
                        Canvas.SetZIndex(image, ImageIndex(image));
                        Image banIcon = GetBanIcon(image);
                        if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
                    };
               
                //Adding to list must  be AFTER adding image, not BEFORE
               AddToNumberedList(lstCurrentDeckList, M.CardStats[id].Name);
            }
            CountUpDeckStatistics();
            if (!M.sideDecking) M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?";
        }
        private void addToSideDeck(int id)
        {
            if (lstSideDeckList.Items.Count == 15 && !M.sideDecking)
            {
                MsgBox("You can only have 15 cards in your side deck.");
                return;
            }
            if ((bool)chkTournament.IsChecked && !M.sideDecking
                    && GetNumOfCopies(lstCurrentDeckList, M.CardStats[id].Name) 
                     + GetNumOfCopies(lstExtraDeckList, M.CardStats[id].Name)
                     + GetNumOfCopies(lstSideDeckList, M.CardStats[id].Name) >= M.CardStats[id].Limit)
            {

                if (M.CardStats[id].Limit == 0)
                    MsgBox("You cannot have any copies of " + M.CardStats[id].Name + " in your deck.");
                else if (M.CardStats[id].Limit == 1)
                    MsgBox("You can only have " + M.CardStats[id].Limit + " copy of " + M.CardStats[id].Name + " in your deck.");
                else
                    MsgBox("You can only have " + M.CardStats[id].Limit + " copies of " + M.CardStats[id].Name + " in your deck.");
                return;

            }
            Image image = CreateSideDeckPictureBox(ActualSideDeckCount + 1, id);
            UpdatePictureBox(ref image, M.CardStats[id]);
            image.MouseLeftButtonUp += SideDeck_MouseClick;
            image.MouseEnter += delegate
            {
                Canvas.SetZIndex(image, Canvas.GetZIndex(image) + 2);
                Image banIcon = GetBanIcon(image);
                if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
            };
            image.MouseLeave += delegate
            {
                Canvas.SetZIndex(image, ImageIndex(image));
                Image banIcon = GetBanIcon(image);
                if (banIcon != null) Canvas.SetZIndex(banIcon, Canvas.GetZIndex(image) + 1);
            };

            AddToNumberedList(lstSideDeckList, M.CardStats[id].Name);
            CountUpDeckStatistics();
            if (!M.sideDecking)  M.warnOnExitMessage = "You have made unsaved changes. Are you sure you wish to exit?";
        }
        #endregion
      
        private void SideDeckItem_Clicked(string itemText, int contextIndex)
        {
            switch (itemText)
            {
                case "Add to Side":
                    if (lstLoadedCards.SelectedIndex == -1)
                        return;
                    int id = M.findID(lstLoadedCards.Items[lstLoadedCards.SelectedIndex].ToString());
                    if (id == 0)
                        return;
                    addToSideDeck(id);
                    break;
                case "Share Card":
                    ShareCard();
                    break;
                case "Share Set":
                    if (lstLoadedCards.SelectedIndex == -1) return;
                    int id2 = M.findID(lstLoadedCards.Items[lstLoadedCards.SelectedIndex].ToString());
                    if (id2 == 0 || string.IsNullOrEmpty(M.CardStats[id2].SpecialSet))
                        return;

                    ShareSet(M.CardStats[id2].SpecialSet);
                    break;
        }
      }

        private void lstLoadedCards_MouseRightButtonDown(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstLoadedCards.SelectedIndex == -1) return;
            if (M.sideDecking) return;
            
            e.Handled = true;
            ctxtSideDeck.Visibility = System.Windows.Visibility.Visible;

            Point point = e.GetPosition(Root);
            TranslateTransform transRefer = ctxtSideDeck.RenderTransform as TranslateTransform;
            if (transRefer == null) throw new Exception("TranslateTransform cannot be null");
            FollowMouse(ref transRefer, ctxtSideDeck, point, false);
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


        public void UpdatePictureBox(ref Image pBox, SQLReference.CardDetails stats)
        {
            string cardname = stats.Name;
           // int suppliedid = M.findID(stats.Name);
            try
            {
                if (stats.Name == null)
                {
                    M.setImage(pBox, "LightBG.png", UriKind.Relative);
                    return;
                }
                else
                {
                    //int cardid = 0;
                 
                    if (stats.ID == 0 || cardname == null)
                        M.setImage(pBox, "token.jpg", UriKind.Relative);
                    else if (stats.Attribute=="Trap")
                    {
                        M.setImage(pBox, "trap.jpg", UriKind.Relative);
                    }
                    else if (stats.Attribute == "Spell")
                    {
                        M.setImage(pBox, "magic.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Effect") && M.IsOrange(stats) == true)
                    {
                        M.setImage(pBox, "monstereffect.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Ritual"))
                    {
                        M.setImage(pBox, "ritual.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Synchro"))
                    {
                        M.setImage(pBox, "synchro.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Fusion"))
                    {
                        M.setImage(pBox, "fusion.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Xyz"))
                    {
                        M.setImage(pBox, "xyz.jpg", UriKind.Relative);
                    }
                    else 
                        M.setImage(pBox, "monster.jpg", UriKind.Relative);

                    if (M.cardsWithImages.Contains(M.getRealImageName(cardname, stats.ID, M.mySet)))
                    {
                        M.setImage(pBox, M.toPortalURL(cardname, stats.ID, M.mySet), UriKind.Absolute);
                    }
                }
                pBox.Opacity = 1;

            }
            catch
            {

            }
        }


        
       



        #region "Sharing"
        private void ShareCard()
        {
            if (lstLoadedCards.SelectedIndex == -1)
                return;
            if (M.mySet != "Default")
            {
                MessageBox.Show("You cannot share cards under a non-default account, because others have no way of accessing the cards.");
                return;
            }
            inputForm.message = "Enter the name of the person to send " + lstLoadedCards.SelectedItem.ToString();
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
            card = M.CardStats[M.findID(lstLoadedCards.SelectedItem.ToString())].toTrueStats();
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
            SQLcli.shareCardAsync(M.username, inputForm.input, cardList);


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
                    int index = M.findID(card.Name);
                    M.CardStats[index] = card;
                }
            }
          
        }
        private void ShareSet(string setName = null)
        {
            if (M.mySet != "Default")
            {
                MessageBox.Show("You cannot share cards under a non-default account, because others have no way of accessing the cards.");
                return;
            }
            if (setName == null)
            {
                inputForm.message = "Enter in the Set Name:";
                inputForm.Closed += shareSetStep1;
                inputForm.Show();
            }
            else
            {
                inputForm.input = setName;
                shareSetStep1(null, null);
            }
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
            for (int n = 1; n <= M.TotalCards; n++)
            {
                if (!string.IsNullOrEmpty(M.CardStats[n].Name))
            {
                    if (M.CardStats[n].SpecialSet == inputForm.input && M.CardStats[n].Creator != "PUB")
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

            for (short n = 1; n <= M.TotalCards; n++)
            {
                if (!string.IsNullOrEmpty(M.CardStats[n].Name) && M.CardStats[n].SpecialSet == setToShare)
                {
                    SQLReference.CardDetails card = default(SQLReference.CardDetails);
                    card = M.CardStats[n].toTrueStats();
                    if (card.Creator != null && card.Creator.Contains("|" + inputForm.input + "|") == false)
                        card.Creator = card.Creator + inputForm.input + "|";
                    setOfCards.Add(card);

                }
            }
            SQLcli.shareCardCompleted += endShare;
            SQLcli.shareCardAsync(M.username, inputForm.input, setOfCards);

        }
        #endregion
        #region "Sorting"
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
            int n; int copies;
            for (n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
            {
                int id = M.findID(GetActualName(lstCurrentDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstCurrentDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (M.CardStats[id].Attribute == "Spell")
                {
                    for (int m = 0; m < copies;m++)
                    spellList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Attribute == "Trap")
                {
                    for (int m = 0; m < copies; m++)
                    trapList.Add(M.CardStats[id].Name);
                }
                else
                {
                    for (int m = 0; m < copies; m++)
                    monsterList.Add(M.CardStats[id].Name);
                }
            }
            for (n = 0; n <= lstExtraDeckList.Items.Count - 1; n++)
            {
                int id = M.findID(GetActualName(lstExtraDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstExtraDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (M.CardStats[id].Type.Contains("Fusion"))
                {
                    for (int m = 0; m < copies; m++)
                    fusionList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Xyz"))
                {
                    for (int m = 0; m < copies; m++)
                    xyzList.Add(M.CardStats[id].Name);
                }
                else
                {
                    for (int m = 0; m < copies; m++)
                    synchroList.Add(M.CardStats[id].Name);
                }
            }
            for (n = 0; n <= lstSideDeckList.Items.Count - 1; n++)
            {
                int id = M.findID(GetActualName(lstSideDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstSideDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (M.CardStats[id].Attribute == "Spell")
                {
                    for (int m = 0; m < copies; m++)
                    sideSpellList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Attribute == "Trap")
                {
                    for (int m = 0; m < copies; m++)
                    sideTrapList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Fusion"))
                {
                    for (int m = 0; m < copies; m++)
                    sideFusionList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Xyz"))
                {
                    for (int m = 0; m < copies; m++)
                    sideXyzList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Synchro"))
                {
                    for (int m = 0; m < copies; m++)
                    sideSynchroList.Add(M.CardStats[id].Name);
                }
                else
                {
                    for (int m = 0; m < copies; m++)
                    sideMonsterList.Add(M.CardStats[id].Name);
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
                addToMainOrExtraDeck(M.findID(monster));
            }
            foreach (string spell in spellList)
            {
                addToMainOrExtraDeck(M.findID(spell));
            }
            foreach (string trap in trapList)
            {
                addToMainOrExtraDeck(M.findID(trap));
            }
            foreach (string fusion in fusionList)
            {
                addToMainOrExtraDeck(M.findID(fusion));
            }
            foreach (string xyz in xyzList)
            {
                addToMainOrExtraDeck(M.findID(xyz));
            }
            foreach (string synchro in synchroList)
            {
                addToMainOrExtraDeck(M.findID(synchro));
            }


            foreach (string monster in sideMonsterList)
            {
                addToSideDeck(M.findID(monster));
            }
            foreach (string spell in sideSpellList)
            {
                addToSideDeck(M.findID(spell));
            }
            foreach (string trap in sideTrapList)
            {
                addToSideDeck(M.findID(trap));
            }
            foreach (string fusion in sideFusionList)
            {
                addToSideDeck(M.findID(fusion));
            }
            foreach (string xyz in sideXyzList)
            {
                addToSideDeck(M.findID(xyz));
            }
            foreach (string synchro in sideSynchroList)
            {
                addToSideDeck(M.findID(synchro));
            }
            CountUpDeckStatistics();
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
            int copies;
            for (int n = 0; n <= lstCurrentDeckList.Items.Count - 1; n++)
            {
                int id = M.findID(GetActualName(lstCurrentDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstCurrentDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (M.CardStats[id].Attribute == "Spell")
                {
                    for (int m = 0; m < copies; m++)
                    spellList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Attribute == "Trap")
                {
                    for (int m = 0; m < copies; m++)
                    trapList.Add(M.CardStats[id].Name);
                }
                else
                {
                    for (int m = 0; m < copies; m++)
                    monsterList.Add(M.CardStats[id].Name);
                }
            }
            for (int n = 0; n <= lstExtraDeckList.Items.Count - 1; n++)
            {
                int id = M.findID(GetActualName(lstExtraDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstExtraDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (M.CardStats[id].Type.Contains("Fusion"))
                {
                    for (int m = 0; m < copies; m++)
                    fusionList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Xyz"))
                {
                    for (int m = 0; m < copies; m++)
                    xyzList.Add(M.CardStats[id].Name);
                }
                else
                {
                    for (int m = 0; m < copies; m++)
                    synchroList.Add(M.CardStats[id].Name);
                }
            }
            for (int n = 0; n <= lstSideDeckList.Items.Count - 1; n++)
            {
                int id = M.findID(GetActualName(lstSideDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstSideDeckList.Items[n].ToString());
                if (id == 0)
                    continue;
                if (M.CardStats[id].Attribute == "Spell")
                {
                    for (int m = 0; m < copies; m++)
                    sideSpellList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Attribute == "Trap")
                {
                    for (int m = 0; m < copies; m++)
                    sideTrapList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Fusion"))
                {
                    for (int m = 0; m < copies; m++)
                    sideFusionList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Xyz"))
                {
                    for (int m = 0; m < copies; m++)
                    sideXyzList.Add(M.CardStats[id].Name);
                }
                else if (M.CardStats[id].Type.Contains("Synchro"))
                {
                    for (int m = 0; m < copies; m++)
                    sideSynchroList.Add(M.CardStats[id].Name);
                }
                else
                {
                    for (int m = 0; m < copies; m++)
                    sideMonsterList.Add(M.CardStats[id].Name);
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
                addToMainOrExtraDeck(M.findID(monster));
            }
            foreach (string spell in spellList)
            {
                addToMainOrExtraDeck(M.findID(spell));
            }
            foreach (string trap in trapList)
            {
                addToMainOrExtraDeck(M.findID(trap));
            }
            foreach (string fusion in fusionList)
            {
                addToMainOrExtraDeck(M.findID(fusion));
            }
            foreach (string xyz in xyzList)
            {
                addToMainOrExtraDeck(M.findID(xyz));
            }
            foreach (string synchro in synchroList)
            {
                addToMainOrExtraDeck(M.findID(synchro));
            }


            foreach (string monster in sideMonsterList)
            {
                addToSideDeck(M.findID(monster));
            }
            foreach (string spell in sideSpellList)
            {
                addToSideDeck(M.findID(spell));
            }
            foreach (string trap in sideTrapList)
            {
                addToSideDeck(M.findID(trap));
            }
            foreach (string fusion in sideFusionList)
            {
                addToSideDeck(M.findID(fusion));
            }
            foreach (string xyz in sideXyzList)
            {
                addToSideDeck(M.findID(xyz));
            }
            foreach (string synchro in sideSynchroList)
            {
                addToSideDeck(M.findID(synchro));
            }
            CountUpDeckStatistics();
        }
        #endregion
        #region "Deck Operations"
        public void makeDeckFromSiding()
        {

            M.PlayerDeck.ClearAndAdd();
            M.PlayerEDeck.ClearAndAdd();
            int id = 0, n;
            for (n = 0; n < lstCurrentDeckList.Items.Count; n++)
            {
                id = M.findID(GetActualName( lstCurrentDeckList.Items[n].ToString()));
                int copies = GetNumOfCopies(lstCurrentDeckList.Items[n].ToString());
                for (int m = 1; m <= copies; m++)
                     M.PlayerDeck.Add(M.CardStats[id]);
            }
          
            for (n = 0; n < lstExtraDeckList.Items.Count; n++)
            {
                id = M.findID(GetActualName(lstExtraDeckList.Items[n].ToString()));
                int copies = GetNumOfCopies(lstExtraDeckList.Items[n].ToString());
                for (int m = 1; m <= copies; m++)
                    M.PlayerEDeck.Add(M.CardStats[id]);
            }

        }
        public void makeSideDeck()
        {
            int id = 0;
            for (int n = 0; n < lstSideDeckList.Items.Count; n++)
            {
                id = M.findID(lstSideDeckList.Items[n].ToString());
                M.realSideDeckIDs[n] = id;
            }

        }
        private void cmdDoneSideDecking_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            int cardsOverSide = cardsOverSidedeckLimit;
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
                M.warnOnExitMessage = "";
                makeDeckFromSiding();
                this.NavigationService.Navigate(new System.Uri("/DuelFieldNew", UriKind.Relative));
            }
        }
        private void cmbLoadDeck_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbLoadDeck != null)
            {
                if (cmbLoadDeck.SelectedItem != null && cmbLoadDeck.SelectedItem.ToString() != "System.Windows.Controls.ComboBoxItem")
                {
                    cmbLoadDeck.IsEnabled = false;
                    SQLcli.loadDeckAsync(M.username, cmbLoadDeck.SelectedItem.ToString());
                    M.defaultDeckName = cmbLoadDeck.SelectedItem.ToString();
                    M.SetCookie("default", M.defaultDeckName);
                }
                else
                    cmdSave.Content = "Save [Deck]";
            }
        }
        private void doneLoadingDeck(object sender, SQLReference.loadDeckCompletedEventArgs e)
        {
            cmbLoadDeck.IsEnabled = true;
            if (e.Error != null)
            {
                M.SendErrorReport(e.GetType().ToString(), e.Error != null ? e.Error.Message : null, e.Error == null ? e.Result : null);
                return;
            }
            
            if (!string.IsNullOrEmpty(e.Result))
            {
                deserializeAndLoad(e.Result);
                cmdSave.Content = "Save " + cmbLoadDeck.SelectedItem.ToString();
            }
            else
            {
                MsgBox("There was an error loading your deck.");
                M.SendErrorReport(e.GetType().ToString(), e.Error != null ? e.Error.Message : null, e.Error == null ? e.Result : null);
            }
        }
        private void cmdDeleteDeck_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLoadDeck.SelectionBoxItem == null || cmbLoadDeck.SelectionBoxItem.ToString() == "") { return; }
            if (cmbLoadDeck.SelectionBoxItem.ToString() == "Load Deck") { return; }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete deck " + cmbLoadDeck.SelectionBoxItem.ToString() + "? There is no undoing this action.", "Delete Deck", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) { return; }
            SQLcli.deleteDeckAsync(M.username, cmbLoadDeck.SelectionBoxItem.ToString(), cmbLoadDeck.SelectionBoxItem.ToString());
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
                SQLcli.SaveDeckAsync(M.username, cmbLoadDeck.SelectionBoxItem.ToString(), serializeDeck());
            }

        }
        private void client_SaveComplete(object sender, SQLReference.SaveDeckCompletedEventArgs e)
        {
            cmdSave.IsEnabled = true;
            cmdSaveAs.IsEnabled = true;
            if (e.Error == null)
            {
                MessageBox.Show("Deck was saved.");
                M.warnOnExitMessage = "";
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Error.Message))
                    MessageBox.Show("There was an error saving your deck." + Environment.NewLine + Environment.NewLine +
                                    e.Error.Message);
                else
                    MessageBox.Show("There was an error saving your deck. The error message is not available.");

                M.SendErrorReport(e.GetType().ToString(), e.Error != null ? e.Error.Message : null, e.Error == null ? e.Result : null);
                
            }
        }
        private string serializeDeck()
        {
            List<string> serializedList = new List<string>();
            int n; int m; int copies; int trueID;
            for (n = 0; n < lstCurrentDeckList.Items.Count; n++)
            {
                trueID = M.findTrueID(GetActualName(lstCurrentDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstCurrentDeckList.Items[n].ToString());
                for (m = 0; m < copies; m++)
                    serializedList.Add(trueID.ToString());
            }
            for (n = 0; n < lstExtraDeckList.Items.Count; n++)
            {
                trueID = M.findTrueID(GetActualName(lstExtraDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstExtraDeckList.Items[n].ToString());
                for (m = 0; m < copies; m++)
                    serializedList.Add(trueID.ToString());
            }
            serializedList.Add("&");
            for (n = 0; n < lstSideDeckList.Items.Count; n++)
            {
                trueID = M.findTrueID(GetActualName(lstSideDeckList.Items[n].ToString()));
                copies = GetNumOfCopies(lstSideDeckList.Items[n].ToString());
                for (m = 0; m < copies; m++)
                    serializedList.Add(trueID.ToString());
            }
            string serializedString = string.Join("|", serializedList.ToArray());
            return serializedString;
        }
        private void deserializeAndLoad(string serializedString)
        {
            string[] deserializedArray = serializedString.Split('|');
            int count = deserializedArray.Count();
            bool sideDeckSwitch = false;

            clearDeck();

            for (int n = 0; n <= count - 1; n++)
            {
                if (deserializedArray[n] == "&")
                {
                    sideDeckSwitch = true;
                    continue;
                }
                int id = M.findIndexFromTrueID(Convert.ToInt32(deserializedArray[n]));
                if (id == 0) { continue; }
                if (sideDeckSwitch == false)
                {
                    addToMainOrExtraDeck(id);
                }
                else
                {
                    addToSideDeck(id);
                }
            }
            CountUpDeckStatistics();
            if (BanlistProblems() == "")
                chkTournament.IsChecked = true;
            else
                chkTraditional.IsChecked = true;
            M.warnOnExitMessage = "";
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

            SQLcli.NewDeckAsync(M.username, txtSaveAs.Text, serializeDeck());
        }
        private void createdNewDeck(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            cmdSave.IsEnabled = true;
            cmdSaveAs.IsEnabled = true;
            if (e.Error == null)
            {
                cmbLoadDeck.Items.Add(txtSaveAs.Text);
                cmbLoadDeck.SelectedIndex = cmbLoadDeck.Items.Count - 1;
                M.defaultDeckName = txtSaveAs.Text;
                M.listOfMyDecks.Add(txtSaveAs.Text);
                M.SetCookie("default", txtSaveAs.Text);
                MessageBox.Show("New Deck made.");
                M.warnOnExitMessage = "";
               
            }
            else
                MessageBox.Show("There was an error creating your deck. The service might be down. Press save again or try later.");

        }
        #endregion
        private void SQLcli_deleteDeckAsyncCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show((string)e.UserState + " was successfully deleted.");
               // cmbLoadDeck.SelectedIndex = 0;
                cmbLoadDeck.Items.Remove((string)e.UserState);
                M.listOfMyDecks.Remove((string)e.UserState);
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
                int id = M.findID(GetActualName(lstCurrentDeckList.Items[i].ToString()));
                int copies = GetNumOfCopies(lstCurrentDeckList.Items[i].ToString());
                idToIndex.Add(id, copies);
            }

            strBld.Append("Monsters:" + Environment.NewLine);
            foreach (int id in idToIndex.Keys)
            {
                if (M.CardStats[id].IsMonster())
                {
                    monCount += idToIndex[id];
                    strBld.Append(idToIndex[id].ToString() + " " + M.CardStats[id].Name + Environment.NewLine);
                }
            }
            strBld.Replace("Monsters:", "Monsters: " + monCount.ToString());

            strBld.Append(Environment.NewLine + "Spells:" + Environment.NewLine);
            foreach (int id in idToIndex.Keys)
            {
                if (M.CardStats[id].Attribute == "Spell")
                {
                    sCount += idToIndex[id];
                    strBld.Append(idToIndex[id].ToString() + " " + M.CardStats[id].Name + Environment.NewLine);
                }
            }
            strBld.Replace("Spells:", "Spells: " + sCount.ToString());

            strBld.Append(Environment.NewLine + "Traps:" + Environment.NewLine);
            foreach (int id in idToIndex.Keys)
            {
                if (M.CardStats[id].Attribute == "Trap")
                {
                    tCount += idToIndex[id];
                    strBld.Append(idToIndex[id].ToString() + " " + M.CardStats[id].Name + Environment.NewLine);
                }
            }
            strBld.Replace("Traps:", "Traps: " + tCount.ToString());


            if (lstExtraDeckList.Items.Count > 0)
            {
                idToIndex.Clear();
                for (int i = 0; i < lstExtraDeckList.Items.Count; i++)
                {
                    int id = M.findID(GetActualName(lstExtraDeckList.Items[i].ToString()));
                    int copies = GetNumOfCopies(lstExtraDeckList.Items[i].ToString());
                    idToIndex.Add(id, copies);
                }

                strBld.Append(Environment.NewLine + "Extra:" + Environment.NewLine);

                foreach (int id in idToIndex.Keys)
                {
                    strBld.Append(idToIndex[id].ToString() + " " + M.CardStats[id].Name + Environment.NewLine);
                }
            }

            if (lstSideDeckList.Items.Count > 0)
            {
                idToIndex.Clear();
                for (int i = 0; i < lstSideDeckList.Items.Count; i++)
                {
                    int id = M.findID(GetActualName(lstSideDeckList.Items[i].ToString()));
                    int copies = GetNumOfCopies(lstSideDeckList.Items[i].ToString());
                    idToIndex.Add(id, copies);
                }
                
                strBld.Append(Environment.NewLine + "Side:" + Environment.NewLine);
                
                foreach (int id in idToIndex.Keys)
                {
                    strBld.Append(idToIndex[id].ToString() + " " + M.CardStats[id].Name + Environment.NewLine);
                }
            }
            try
            {
                Clipboard.SetText(strBld.ToString());
                MessageBox.Show("Text version of deck copied to clipboard. Paste it in a forum or something.");
            }
            catch { }
        }

        private void Root_Loaded_1(object sender, RoutedEventArgs e)
        {
            cmdSearch_Click(null, null);
        }

        private void cmdLoadFromText_Click_1(object sender, RoutedEventArgs e)
        {
            inputForm.message = "Enter the text provided by the 'Copy to Clipboard' button:";
            inputForm.Closed += LoadFromText_InputFormClosed;
            inputForm.Show();
        }

        private void LoadFromText_InputFormClosed(object sender, EventArgs e)
        {
            inputForm.Closed -= LoadFromText_InputFormClosed;
            if (string.IsNullOrEmpty(inputForm.input))
                return;

            clearDeck();

            try
            {
                string[] lines = inputForm.input.Split(new char[]{'\r', '\n'});

                for (int n = 0; n < lines.Length; n++)
                {
                    string line = lines[n].Trim();
                    if (line == string.Empty) 
                        continue;
                    int copies = 0;
                    bool success = int.TryParse(line[0].ToString(), out copies);
                    if (!success)
                        continue;
                    int falseID = M.findID(line.Substring(2, line.Length - 2));
                    if (falseID > 0)
                    {
                        for (int m = 1; m <= copies; m++)
                            addToMainOrExtraDeck(falseID);

                    }
                }


            }
            catch
            {
                clearDeck();
                MessageBox.Show("The text was badly formatted. Please check to see each card is in this format:" + Environment.NewLine + Environment.NewLine + "3 Cardname" + Environment.NewLine + "2 Anothercardname");
            }
        }



    }

    }

