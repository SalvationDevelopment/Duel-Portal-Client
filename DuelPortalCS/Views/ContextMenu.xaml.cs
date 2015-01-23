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

namespace DuelPortalCS
{
    public partial class ContextMenu : UserControl
    {
        public Area myArea;
        public int _contextIndex = -1;
        public int _xyzIndex = -1;
        public event Item_ClickedEventHandler Item_Clicked;
        public delegate void Item_ClickedEventHandler(string itemText, int contextIndex);
        public event Item_ClickedXyzEventHandler Item_ClickedXyz;
        public delegate void Item_ClickedXyzEventHandler(string itemText, int contextIndex, int xyzIndex);
        // Public buttons As New List(Of Button)

        public ContextMenu()
        {
            InitializeComponent();

        }
        public void onLoaded()
        {
            switch (myArea)
            {
                case Area.Deck:
                    addItem("View");
                    addItem("Shuffle");
                    addItem("Opponent Draw");
                    addItem("Mill");
                    addItem("Banish Top");
                    addItem("Offer Rematch");
                    break;
                case Area.Hand:
                    addItem("Summon / Activate");
                    addItem("Set (Monster)");
                    addItem("Set (S/T)");
                    addItem("Discard");
                    addItem("Discard at Random");
                    addItem("Banish");
                    addItem("Banish Facedown");
                    addItem("Reveal Card");
                    addItem("Reveal All");
                    addItem("To Top of Deck");
                    addItem("To Bottom of Deck");
                    break;
                case Area.Monster_Full:
                    addItem("Send to Grave");
                    addItem("Return to Hand");
                    addItem("Banish");
                    addItem("Switch Position");
                    addItem("Flip");
                    addItem("To Top of Deck");
                    addItem("To Bottom of Deck");
                    addItem("To Extra Deck");
                    addItem("Change Control");
                    addItem("Attack...");
                    addItem("Move to Zone...");
                    break;
                case Area.Monster_Empty:
                    addItem("Summon Token");
                    break;
                case Area.ST:
                    addItem("Activate");
                    addItem("Send to Grave");
                    addItem("Return to Hand");
                    addItem("Banish");
                    addItem("Flip Face-Down");
                    addItem("To Top of Deck");
                    addItem("To Bottom of Deck");
                    addItem("Change Control");
                    addItem("Move to Zone...");
                    break;
                case Area.Side_Deck:
                    addItem("Add to Side");
 
                    break;
                case Area.Xyz:
                    addItem("Detach");
                    break;
            }

        }
        public void addItem(string Caption)
        {
            Button newButton = new Button();
            newButton.Content = Caption;
            newButton.Height = 16;
            newButton.Width = ContentStackPanel.Width;
            newButton.Opacity = 0.8;
            newButton.FontSize = 9.0;
            newButton.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            newButton.Click += ItemClickedSub;

            ContentStackPanel.Height += newButton.Height;
            ContentStackPanel.Children.Add(newButton);
        }

        public enum Area
        {
            None,
            Deck,
            Hand,
            Monster_Full,
            Monster_Empty,
            ST,
            Side_Deck,
            Xyz
        }
        public void ItemClickedSub(object sender, System.Windows.RoutedEventArgs e)
        {
            Button item = (Button)sender;

                if (_xyzIndex == -1 && Item_Clicked != null)
                    Item_Clicked(item.Content.ToString(), _contextIndex);
                else if (Item_ClickedXyz != null)
                    Item_ClickedXyz(item.Content.ToString(), _contextIndex, _xyzIndex);

        }

    }
}
