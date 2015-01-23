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
    public partial class cldEditImage : ChildWindow
    {
        public int cardID;
        public string imageURL = "";
        public bool useYCMTemplate;
        public cldEditImage()
        {
            InitializeComponent();
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        private void ChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
       
        }

        void sqlCli_saveImageCompleted(object sender, SQLReference.saveImageCompletedEventArgs e) 
        {
         
        }
        
        private void cmdAddImage_Click(object sender, RoutedEventArgs e)
        {
            useYCMTemplate = (bool)chkYCMTemplate.IsChecked;
            imageURL = txtAddExistingImage.Text;
            DialogResult = true;

        }
        

        private void cmdDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            imageURL = "Delete";
            DialogResult = true;
        }

        private void imgTheImage_ImageOpened(object sender, RoutedEventArgs e)
        {
          
        }

        private void ChildWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void txtAddExistingImage_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void chkYCMTemplate_Checked(object sender, RoutedEventArgs e)
        {
            if (cardID <= 0)
                lblYCMInfo.Visibility = System.Windows.Visibility.Visible;
        }

        private void chkYCMTemplate_Unchecked(object sender, RoutedEventArgs e)
        {
            lblYCMInfo.Visibility = System.Windows.Visibility.Collapsed;
        }
        
    }
}

