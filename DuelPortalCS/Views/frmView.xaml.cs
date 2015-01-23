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
    public partial class frmView : ChildWindow
    {
        public DuelFieldNew.Area area;
        public bool isPlayer;

        public DuelFieldNew.Area oncloseFromArea;
        public DuelFieldNew.Area oncloseToArea;
        public int oncloseFromIndex;
        public int oncloseToIndex;
        public bool oncloseBanishFacedown;

        public bool AllMonsterZonesFull;
        public bool AllSTZonesFull;
        public string opponentSet;
        int selectedCardIndex = 0;
        private List<MouseEventHandler> MouseEnterHandlers = new List<MouseEventHandler>();
        public frmView()
        {
            InitializeComponent();
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            loaded();
        }

        protected override void OnClosed(EventArgs e)
        {
 	        base.OnClosed(e);
            closed();
        }
        public void closed()
        {
            staCards.Children.Clear();

        }

        public void loaded()
        {
            oncloseFromArea = DuelFieldNew.Area.None;
            oncloseToArea = DuelFieldNew.Area.None;
            oncloseFromIndex = -1;
            oncloseToIndex = -1;
            oncloseBanishFacedown = false;
            selectedCardIndex = 0;
            MouseEnterHandlers.Clear();

            switch (area)
            {
                case DuelFieldNew.Area.Deck:
                    this.Title = "Viewing Deck";
                    for (int n = 1; n <= M.PlayerDeck.CountNumCards(); n++)
                    {
                        CreateAndAddImage(M.PlayerDeck[n]);
                    }
                    break;
                case DuelFieldNew.Area.Extra:
                    this.Title = "Viewing Extra Deck";
                    for (int n = 1; n <= M.PlayerEDeck.CountNumCards(); n++)
                    {
                        CreateAndAddImage(M.PlayerEDeck[n]);
                    }
                    break;
                case DuelFieldNew.Area.RFG:
                    if (isPlayer)
                    {
                        this.Title = "Viewing RFG";
                        for (int n = 1; n <= M.PlayerRFG.CountNumCards(); n++)
                        {
                            CreateAndAddImage(M.PlayerRFG[n]);
                        }
                    }
                    else
                    {
                        this.Title = "Viewing Opponent RFG";
                        for (int n = 1; n <= M.OpponentRFG.CountNumCards(); n++)
                        {
                            CreateAndAddImage(M.OpponentRFG[n]);
                        }
                    }
                    break;
                case DuelFieldNew.Area.Grave:
                    if (isPlayer)
                    {
                        this.Title = "Viewing Grave";
                        for (int n = 1; n <= M.PlayerGrave.CountNumCards(); n++)
                        {
                            CreateAndAddImage(M.PlayerGrave[n]);
                        }
                    }
                    else
                    {
                        this.Title = "Viewing Opponent Grave";
                        for (int n = 1; n <= M.OpponentGrave.CountNumCards(); n++)
                        {
                            CreateAndAddImage(M.OpponentGrave[n]);
                        }
                    }
                    break;
            }

            ChangeButtons(false);
            ClearStatLabels();
           
        }
        public void EnableAppropriateButtons(SQLReference.CardDetails stats)
        {
            switch (area)
            {
                case DuelFieldNew.Area.Deck:
                    ChangeButtons(true);
                    break;
                case DuelFieldNew.Area.Extra:
                    ChangeButtons(true);
                    cmdToBottom.IsEnabled = false;
                    cmdToTop.IsEnabled = false;
                    cmdToHand.IsEnabled = false;
                    cmdToExtra.IsEnabled = false;
                    break;
                case DuelFieldNew.Area.RFG:
                    if (isPlayer)
                    {
                        ChangeButtons(true);
                        cmdBanish.IsEnabled = false;
                        chkBanishFacedown.IsEnabled = false;
                    }
                    else
                    {
                        ChangeButtons(false);
                    }
                    break;
                case DuelFieldNew.Area.Grave:
                    if (isPlayer)
                    {
                        ChangeButtons(true);
                        cmdToGrave.IsEnabled = false;
                    }
                    else
                    {
                        ChangeButtons(false);
                    }
                    break;
            }

            if (stats.IsMonster())
                cmdToField.IsEnabled = !AllMonsterZonesFull && isPlayer && !stats.Facedown && !M.IamWatching;
            else
                cmdToField.IsEnabled = !AllSTZonesFull && isPlayer && !stats.Facedown && !M.IamWatching;
        }
        public void ClearStatLabels()
        {
            for (int n = 1; n <= 12; n++)
            {
                ImgStars(n).Source = null;
            }
            lblSlash.Visibility = System.Windows.Visibility.Collapsed;
            BordATK.Visibility = System.Windows.Visibility.Collapsed;
            BordDEF.Visibility = System.Windows.Visibility.Collapsed;
            imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
            imgDuelAttribute.Source = null;
            lblDuelLore.Text = "";
            lblDuelType.Text = "";
            lblDuelName.Text = "";
        }
        public void ChangeButtons(bool enable)
        {
            if (M.IamWatching)
                enable = false;
            cmdToGrave.IsEnabled = enable;
            cmdToField.IsEnabled = enable;
            cmdBanish.IsEnabled = enable;
            chkBanishFacedown.IsEnabled = enable;
            cmdToBottom.IsEnabled = enable;
            cmdToTop.IsEnabled = enable;
            cmdToExtra.IsEnabled = enable;
            cmdToHand.IsEnabled = enable;
          
        }
        public void UpdatePictureBox(Image pBox, SQLReference.CardDetails stats)
        {
         
           
            try
            {
                if (stats.Name == null)
                {
                    M.setImage(pBox, null, UriKind.Relative);
                }
                else if (stats.Facedown)
                {
                    M.setImage(pBox, "back.jpg", UriKind.Relative);
                }
                else
                {
                    //int cardid = 0;

                    if (stats.ID == 0)
                        M.setImage(pBox, "token.jpg", UriKind.Relative);
                    else if (stats.Attribute == "Trap")
                        M.setImage(pBox, "trap.jpg", UriKind.Relative);
                    else if (stats.Attribute == "Spell")
                        M.setImage(pBox, "magic.jpg", UriKind.Relative);
                    else if (stats.Type.Contains("/Effect") && M.IsOrange(stats) == true)
                        M.setImage(pBox, "monstereffect.jpg", UriKind.Relative);
                    else if (stats.Type.Contains("/Ritual"))
                        M.setImage(pBox, "ritual.jpg", UriKind.Relative);
                    else if (stats.Type.Contains("/Synchro"))
                        M.setImage(pBox, "synchro.jpg", UriKind.Relative);
                    else if (stats.Type.Contains("/Fusion"))
                        M.setImage(pBox, "fusion.jpg", UriKind.Relative);
                    else if (stats.Type.Contains("/Xyz"))
                        M.setImage(pBox, "xyz.jpg", UriKind.Relative);
                    else
                        M.setImage(pBox, "monster.jpg", UriKind.Relative);

                    if (M.cardsWithImages.Contains(M.getRealImageName(stats.Name, stats.ID, isPlayer ? M.mySet : opponentSet)))
                    {
                        M.setImage(pBox, M.toPortalURL(stats.Name, stats.ID, isPlayer ? M.mySet : opponentSet), UriKind.Absolute);
                    }
                
                }
             

            }
            catch
            {

            }
        }
        public void CreateAndAddImage(SQLReference.CardDetails stats)
        {
            Border bord = new Border();
            bord.BorderThickness = new Thickness(4);
            bord.Width = 59;
            bord.Height = 85;
            bord.Margin = new Thickness(0, 0, -3, 0);
            Image image = new Image();
            bord.Child = image;
            int imageIndex = staCards.Children.Count + 1;
            bord.Name = "bord" + imageIndex;
            UpdatePictureBox(image, stats);
            MouseEventHandler enterHandler = new MouseEventHandler((s, e) => {
                genericShowStats(stats);
            });
            MouseEnterHandlers.Add(enterHandler);
            image.MouseEnter += enterHandler;

            image.MouseLeave += (s, e) =>
                {
                    if (selectedCardIndex > 0)
                        MouseEnterHandlers[selectedCardIndex - 1].Invoke(null, null);
                    else
                        ClearStatLabels();
                };

            image.MouseLeftButtonUp += (s, e) =>
                {
                    if (selectedCardIndex > 0)
                    {
                        
                        Border previousSelectedBorder = (Border)LayoutRoot.FindName("bord" + selectedCardIndex);
                        ((Storyboard)previousSelectedBorder.Tag).Stop();
                        previousSelectedBorder.BorderBrush = null;
                        if (selectedCardIndex == imageIndex) //Selecting a card which was already selected; cancel it
                        {
                            selectedCardIndex = 0;
                            ChangeButtons(false);
                            return;
                        }
                    }
                    selectedCardIndex = imageIndex;
                    bord.BorderBrush = new SolidColorBrush(Colors.Orange);
                    DoubleAnimation opacityAnimation = new DoubleAnimation();
                    opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
                    opacityAnimation.From = 1;
                    opacityAnimation.To = 0;
                    opacityAnimation.AutoReverse = true;
                    opacityAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                    Storyboard sb =  bord.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, opacityAnimation);
                    bord.Tag = sb;

                    ChangeButtons(false);
                    EnableAppropriateButtons(stats); //Reset which buttons should be enabled
                };
            staCards.Children.Add(bord);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cmdToGrave_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;

            oncloseFromArea = area;
            oncloseToArea = DuelFieldNew.Area.Grave;
            oncloseFromIndex = selectedCardIndex;
            DialogResult = true;
        }

        private void cmdBanish_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;
            oncloseFromArea = area;
            oncloseToArea = DuelFieldNew.Area.RFG;
            oncloseFromIndex = selectedCardIndex;
            oncloseBanishFacedown = (bool)chkBanishFacedown.IsChecked;
            DialogResult = true;
        }

        private void cmdToHand_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;
            oncloseFromArea = area;
            oncloseToArea = DuelFieldNew.Area.Hand;
            oncloseFromIndex = selectedCardIndex;
            DialogResult = true;
        }

        private void cmdToTop_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;
            oncloseFromArea = area;
            oncloseToArea = DuelFieldNew.Area.Deck;
            oncloseFromIndex = selectedCardIndex;
            DialogResult = true;
        }

        private void cmdToBottom_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;
            oncloseFromArea = area;
            oncloseToArea = DuelFieldNew.Area.Deck;
            oncloseFromIndex = selectedCardIndex;
            oncloseToIndex = 1;
            DialogResult = true;
        }

        private void cmdToExtra_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;
            oncloseFromArea = area;
            oncloseToArea = DuelFieldNew.Area.Extra;
            oncloseFromIndex = selectedCardIndex;
            oncloseToIndex = 1;
            DialogResult = true;
        }

        private void cmdToField_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (selectedCardIndex == 0)
                return;
           
            int id = M.findID(lblDuelName.Text);

            if (M.CardStats[id].IsMonster())
            {
                if (AllMonsterZonesFull) return;
                oncloseToArea = DuelFieldNew.Area.Monster;

            }
            else if (M.CardStats[id].Type == "Field")
            {
                oncloseToArea = DuelFieldNew.Area.FieldSpell;

            }
            else
            {
                if (AllSTZonesFull) return;
                oncloseToArea = DuelFieldNew.Area.ST;

            }

            oncloseFromArea = area;
            oncloseFromIndex = selectedCardIndex;
            DialogResult = true;
        }
        public Image ImgStars(int index)
        {
            Image img = (Image)LayoutRoot.FindName("imgStars" + index);
            return img;
        }
        private void genericShowStats(SQLReference.CardDetails stats)
        {
            int n = 0;
            if (stats.Facedown)
            {
                lblDuelName.Text = "{{Facedown Card}}";
                for (n = 1; n <= 12; n++)
                {
                    ImgStars(n).Source = null;
                }
                lblSlash.Visibility = System.Windows.Visibility.Collapsed;
                BordATK.Visibility = System.Windows.Visibility.Collapsed;
                BordDEF.Visibility = System.Windows.Visibility.Collapsed;
                imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
                M.setImage(imgDuelAttribute, "Unknown.png", UriKind.Relative);
                lblDuelLore.Text = "";
                lblDuelType.Text = "";
                return;
            }
            lblDuelName.Text = stats.Name;

            if (stats.Type == null)
            {
                for (n = 1; n <= 12 - stats.Level; n++)
                {
                    ImgStars(n).Source = null;
                }
            }
            else if (stats.Type.Contains("Xyz"))
            {
                for (n = 1; n <= 12 - stats.Level; n++)
                {
                    ImgStars(n).Source = null;
                }
                for (n = 12 - stats.Level + 1; n <= 12; n++)
                {
                    M.setImage(ImgStars(n), "RankStar.jpg", UriKind.Relative);
                }
            }
            else
            {
                for (n = 1; n <= stats.Level; n++) 
                {
                    M.setImage(ImgStars(n), "Star.jpg", UriKind.Relative);
                }
                for (n = stats.Level + 1; n <= 12; n++)
                {
                    ImgStars(n).Source = null;
                }
            }

            if (stats.IsMonster())
            {
                lblSlash.Visibility = System.Windows.Visibility.Visible;
                BordATK.Visibility = System.Windows.Visibility.Visible;
                lblATK.Text = stats.ATK.ToStringCountingQuestions();
                BordDEF.Visibility = System.Windows.Visibility.Visible;
                lblDEF.Text = stats.DEF.ToStringCountingQuestions();
            }
            else
            {
                lblSlash.Visibility = System.Windows.Visibility.Collapsed;
                BordATK.Visibility = System.Windows.Visibility.Collapsed;
                BordDEF.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!stats.IsMonster())
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Visible;
                M.setImage(imgSTIcon, M.TypeToImageName(stats.Type), UriKind.Relative);
            }
            else
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            lblDuelLore.Text = stats.Lore == null ? "" : stats.Lore;
            lblDuelType.Text = stats.Type == null ? null : stats.Type.NotDisplayEffect();

            if (stats.Attribute == null)
                M.setImage(imgDuelAttribute, DuelFieldNew.BLANK_IMAGE, UriKind.Relative);
            else
                M.setImage(imgDuelAttribute, M.AttributeToImageName(stats.Attribute), UriKind.Relative);



        }

        private void LayoutRoot_MouseEnter(object sender, MouseEventArgs e)
        {

        }


    }

}