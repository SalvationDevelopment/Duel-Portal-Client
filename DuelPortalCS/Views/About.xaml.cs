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
    public partial class About : Page
    {
       // Storyboard stb = new Storyboard();
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        public About()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ContentText.Text = "Duel Portal is a program where you can create YuGiOh cards and duel with them. If you don't have an account, please register one. Many features are disabled with a guest account." + Environment.NewLine + Environment.NewLine +
            "The 'Submit' button will let you start making and submitting cards and images." + Environment.NewLine + Environment.NewLine +
 "The 'Deck Editor' button lets you build visual decks with yours and others' cards." + Environment.NewLine + Environment.NewLine +
 "The 'Duel' button is where you chat or meet your opponent. Confirming a duel here will take you to the Duel Field." + Environment.NewLine + Environment.NewLine +
 "The 'Message' button lets you email other Duel Portal users. Don't worry, your address will not be displayed." + Environment.NewLine + Environment.NewLine +
 "Duel Portal was created by Seattleite to stimulate the growth of the Yugioh Custom Card Community. Credit for templates goes to YVD.";
           
            M.CenterCanvas(LayoutRoot, this);
            M.ScreenResized += delegate
            {
                M.CenterCanvas(LayoutRoot, this);
            };
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Environment.ProcessorCount.ToString());

        }

        private void HyperlinkButton1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://dl.dropbox.com/u/15387447/PortalFAQ.txt"));
        }
       

   

        private void button2_Click(object sender, RoutedEventArgs e)
         {
           
           //  client.AddOrChangeCardCCGAsync("ACG", -1, "|Cyber Power|0|TST1|Trap Card|Trap||Continuous|||Once per turn, when a Machine-Type Union monster activates its effect: You can draw 1 card then shuffle 1 card from your hand into your Deck.|");
             //client.SendEmailCompleted += client_SendEmailCompleted;
             //client.SendEmailAsync("hccgguy", "soreric94@gmail.com", "Hi", "test", false);
            //return;

             this.NavigationService.Navigate(new Uri("/DuelFieldNew", UriKind.Relative));


        
        }

        void client_SendEmailCompleted(object sender, SQLReference.SendEmailCompletedEventArgs e)
        {
            MessageBox.Show(e.Result);
        }

       
      

        void sqlcli_getListOfImagesCompleted(object sender, SQLReference.getListOfImagesCompletedEventArgs e)
        {
           
            string j = e.Result;
        }

       
    }
}