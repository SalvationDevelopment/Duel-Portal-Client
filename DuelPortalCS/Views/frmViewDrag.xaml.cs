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

namespace DuelPortalCS.Views
{
    public partial class frmViewDrag : UserControl
    {
        public short numOfViewing;
        public string WhatImViewing;
        public event Action Closed;
        public frmViewDrag()
        {
            Loaded += loaded;
            InitializeComponent();
        
        }
        public void Close()
        {

            this.Visibility = System.Windows.Visibility.Collapsed;
            Closed();
        }
        public void Show()
        {
            this.Visibility = System.Windows.Visibility.Visible;
          
            loaded(null, null);
        }
        public void loaded(object sender, RoutedEventArgs e)
        {
            deckHelperUp.Visibility = Visibility.Visible;
            deckHelperDown.Visibility = Visibility.Visible;

            deckHelperUp.Content = "^" + Environment.NewLine +
                                   "|" + Environment.NewLine +
                                   "to" + Environment.NewLine + "bottom";

            deckHelperDown.Content = "to top" + Environment.NewLine +
                                      "|" + Environment.NewLine +
                                     "v";
            this.lblViewing.Content = "Viewing: " + WhatImViewing;
            lstCards.Items.Clear();
            switch (WhatImViewing)
            {
                case "Graveyard":
                    for (short n = 1; n <= Module1.PlayerCurrentGrave.CountNumCards(); n++)
                    {
                        lstCards.Items.Add(Module1.PlayerCurrentGrave[n].Name);
                    }

                    ChangeButtons(true);
                    cmdToGrave.IsEnabled = false;
                    break;
                case "Deck":
                    System.Diagnostics.Debug.Assert(Module1.PlayerCurrentDeck[0].Name == null);
                    for (short n = 1; n <= Module1.PlayerCurrentDeck.CountNumCards(); n++)
                    {
                        lstCards.Items.Add(Module1.PlayerCurrentDeck[n].Name);
                    }

                    ChangeButtons(true);


                    break;
                case "RFG":
                    for (short n = 1; n <= Module1.PlayerCurrentRFG.CountNumCards(); n++)
                    {
                        lstCards.Items.Add(Module1.PlayerCurrentRFG[n].Name);
                    }

                    ChangeButtons(true);
                    cmdBanish.IsEnabled = false;

                    break;
                case "Extra Deck":
                    for (short n = 1; n <= Module1.PlayerCurrentEDeck.CountNumCards(); n++)
                    {
                        lstCards.Items.Add(Module1.PlayerCurrentEDeck[n].Name);
                    }

                    ChangeButtons(true);
                    cmdToBottom.IsEnabled = false;
                    cmdToTop.IsEnabled = false;
                    cmdToHand.IsEnabled = false;

                    break;
                case "Opponent Graveyard":
                    for (short n = 1; n <= Module1.OpponentCurrentGrave.CountNumCards(); n++)
                    {
                        lstCards.Items.Add(Module1.OpponentCurrentGrave[n].Name);
                    }



                    ChangeButtons(false);
                    break;
                case "Opponent RFG":
                    for (short n = 1; n <= Module1.OpponentCurrentRFG.CountNumCards(); n++)
                    {
                        lstCards.Items.Add(Module1.OpponentCurrentRFG[n].Name);
                    }

                    ChangeButtons(false);

                    break;
            }
        }
        public void ChangeButtons(bool enable)
        {
            if (Module1.IamWatching)
                enable = false;
            cmdToGrave.IsEnabled = enable;
            cmdToField.IsEnabled = enable;
            cmdBanish.IsEnabled = enable;
            cmdToBottom.IsEnabled = enable;
            cmdToTop.IsEnabled = enable;
            cmdToHand.IsEnabled = enable;
        }
  

        private void cmdToGrave_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            Module1.NumberOnList = lstCards.SelectedIndex + 1;
            switch (WhatImViewing)
            {
                case "RFG":
                    Module1.RFGToGrave((short)Module1.NumberOnList);
                    break;
                case "Deck":
                    Module1.DeckToGrave((short)Module1.NumberOnList);
                    break;
                case "Extra Deck":
                    Module1.ExtraDeckToGrave((short)Module1.NumberOnList);

                    break;
            }
            this.Close();
           
        }

        private void cmdBanish_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            Module1.NumberOnList = lstCards.SelectedIndex + 1;
            switch (WhatImViewing)
            {
                case "Graveyard":
                    Module1.GraveToRFG((short)Module1.NumberOnList);
                    break;
                case "Deck":
                    Module1.DeckToRFG((short)Module1.NumberOnList);
                    break;
                case "Extra Deck":
                    Module1.ExtraDeckToRFG((short)Module1.NumberOnList);

                    break;
            }
            this.Close();
        }

        private void cmdToHand_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            Module1.NumberOnList = lstCards.SelectedIndex + 1;
            switch (WhatImViewing)
            {
                case "Graveyard":
                    Module1.GraveToHand((short)Module1.NumberOnList);
                    break;
                case "RFG":
                    Module1.RFGToHand((short)Module1.NumberOnList);
                    break;
                case "Deck":
                    Module1.DeckToHand((short)Module1.NumberOnList);

                    break;
            }
            this.Close();
        }

        private void cmdToTop_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            Module1.NumberOnList = lstCards.SelectedIndex + 1;
            switch (WhatImViewing)
            {
                case "Graveyard":
                    Module1.GraveToTop((short)Module1.NumberOnList);
                    break;
                case "RFG":
                    Module1.RFGToTop((short)Module1.NumberOnList);
                    break;
                case "Deck":
                    Module1.DeckToTop((short)Module1.NumberOnList);

                    break;
            }
            this.Close();
        }

        private void cmdToBottom_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            Module1.NumberOnList = lstCards.SelectedIndex + 1;
            switch (WhatImViewing)
            {
                case "Graveyard":
                    Module1.GraveToBottom((short)Module1.NumberOnList);
                    break;
                case "RFG":
                    Module1.RFGToBottom((short)Module1.NumberOnList);
                    break;
                case "Deck":
                    Module1.DeckToBottom((short)Module1.NumberOnList);

                    break;
            }
            this.Close();
        }

        private void cmdToField_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            Module1.NumberOnList = lstCards.SelectedIndex + 1;
            Module1.LogViewAction = "Place on Field from " + WhatImViewing;
            this.Close();
        }
        public Image imgStars(short index)
        {

            Image img = (Image)Root.FindName("imgStars" + index);
            return img;

        }
        private void lstCards_SelectionChanged(System.Object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            short index = (short)(lstCards.SelectedIndex + 1);
            SQLReference.CardDetails stats = default(SQLReference.CardDetails);
            switch (WhatImViewing)
            {
                case "Graveyard":
                    stats = Module1.PlayerCurrentGrave[index];
                    break;
                case "RFG":
                    stats = Module1.PlayerCurrentRFG[index];
                    break;
                case "Extra Deck":
                    stats = Module1.PlayerCurrentEDeck[index];
                    break;
                case "Deck":
                    stats = Module1.PlayerCurrentDeck[index];
                    break;
                case "Opponent Graveyard":
                    stats = Module1.OpponentCurrentGrave[index];
                    break;
                case "Opponent RFG":
                    stats = Module1.OpponentCurrentRFG[index];

                    break;
            }
            short n = 0;
            short StarsCounter = 0;
            lblDuelName.Text = stats.Name;

            for (n = 1; n <= 12; n++)
            {
                imgStars(n).Source = null;
            }

            StarsCounter = 1;
            if (!string.IsNullOrEmpty(stats.Level))
            {
                while (Convert.ToDouble(stats.Level) >= StarsCounter)
                {
                    Image refer = imgStars(StarsCounter); Module1.setImage(ref refer, "Star.jpg", UriKind.Relative);
                    StarsCounter = (short)(StarsCounter + 1);
                }
            }

            if (string.IsNullOrEmpty(stats.ATK))
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDuelATK.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDuelATK.Visibility = System.Windows.Visibility.Visible;
                lblDuelATK.Text = stats.ATK;
            }

            if (string.IsNullOrEmpty(stats.DEF))
            {
                lblDEFPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDuelDEF.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblDEFPlaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDuelDEF.Visibility = System.Windows.Visibility.Visible;
                lblDuelDEF.Text = stats.DEF;
            }

            if (stats.ATK == "" && !string.IsNullOrEmpty(stats.Type))
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Visible;
                if (stats.Type == "Continuous")
                {
                    Module1.setImage(ref imgSTIcon, "ContinuousIcon.jpg", UriKind.Relative);
                }
                else if (stats.Type == "Counter")
                {
                    Module1.setImage(ref imgSTIcon, "CounterIcon.jpg", UriKind.Relative);
                }
                else if (stats.Type == "Equip")
                {
                    Module1.setImage(ref imgSTIcon, "EquipIcon.jpg", UriKind.Relative);
                }
                else if (stats.Type == "Field")
                {
                    Module1.setImage(ref imgSTIcon, "FieldIcon.jpg", UriKind.Relative);
                }
                else if (stats.Type == "Quick-Play")
                {
                    Module1.setImage(ref imgSTIcon, "Quick-PlayIcon.jpg", UriKind.Relative);
                }
                else if (stats.Type == "Ritual")
                {
                    Module1.setImage(ref imgSTIcon, "RitualIcon.jpg", UriKind.Relative);
                }
            }
            else
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            lblDuelLore.Text = stats.Lore;
            lblDuelType.Text = stats.Type.NotDisplayEffect();


            switch (stats.Attribute.ToLower())
            {
                case "dark":
                Module1.setImage(ref imgDuelAttribute, "Dark.jpg", UriKind.Relative);
                break;

                case "light":
                Module1.setImage(ref imgDuelAttribute, "Light.jpg", UriKind.Relative);
                break;

                case "earth":       
                Module1.setImage(ref imgDuelAttribute, "Earth.jpg", UriKind.Relative);
                break;

                case "wind":
                Module1.setImage(ref imgDuelAttribute, "Wind.jpg", UriKind.Relative);
                break;

                case "fire":
                Module1.setImage(ref imgDuelAttribute, "Fire.jpg", UriKind.Relative);
                break;

                case "water":
                Module1.setImage(ref imgDuelAttribute, "Water.jpg", UriKind.Relative);
                break;

                case "spell":
                Module1.setImage(ref imgDuelAttribute, "SpellIcon.jpg", UriKind.Relative);
                break;

                case "trap":
            
                Module1.setImage(ref imgDuelAttribute, "TrapIcon.jpg", UriKind.Relative);
                break;

        }

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
        
    }
}
