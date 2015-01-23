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
    public partial class cldSimpleDialog : ChildWindow
    {
       // bool doNotShowAgain = false;

        

        public cldSimpleDialog(string dialogue, bool doNotShowAgainVisible)
        {
       
            InitializeComponent();
            textBlock1.Text = dialogue;
            if (doNotShowAgainVisible)
                chkDoNotShowAgain.Visibility = System.Windows.Visibility.Visible;
            else
                chkDoNotShowAgain.Visibility = System.Windows.Visibility.Collapsed;

            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        { 
            if (chkDoNotShowAgain.IsChecked == true)
            { M.doNotShowPublicMessageAgain = true; M.SetCookie("Popup", "F"); }
            else
            { M.doNotShowPublicMessageAgain = false; M.SetCookie("Popup", "T"); }
            this.DialogResult = true;
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        public void attemptShow()
        {
            if (M.doNotShowPublicMessageAgain)
            {
                this.DialogResult = true;
                return;
            }

            this.Show();
        }

        private void ChildWindow_Loaded_1(object sender, RoutedEventArgs e)
        {
         
        }
    }
}

