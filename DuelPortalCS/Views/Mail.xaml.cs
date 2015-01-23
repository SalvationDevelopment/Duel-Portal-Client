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
    public partial class Mail : Page
    {

        SQLReference.Service1ConsoleClient mailClient;
        public Mail()
        {
            InitializeComponent();
        }

        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!M.isLoggedIn)
            {
                MessageBox.Show("You cannot use this function until you are Logged In.");
                this.NavigationService.Navigate(new System.Uri("/Home", UriKind.Relative));
                return;
            }
            mailClient = new SQLReference.Service1ConsoleClient();
            mailClient.getAllUsersCompleted += client_getUsersCompleted;
            mailClient.getAllUsersAsync();

            M.CenterCanvas(LayoutRoot, this);
            M.ScreenResized += delegate
            {
                M.CenterCanvas(LayoutRoot, this);
            };
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            
            if (mailClient != null) mailClient.CloseAsync();
            base.OnNavigatingFrom(e);
        }
        private void client_getUsersCompleted(object sender, SQLReference.getAllUsersCompletedEventArgs e)
        {
            mailClient.getAllUsersCompleted -= client_getUsersCompleted;
            if (e.Result.Count == 0)
            {
                MessageBox.Show("Unable to connect to server.");
                return;
            }
            cmbAddress.IsEnabled = true;
            // cmbAdditional1.IsEnabled = True
            // cmbAdditional2.IsEnabled = True
            // cmbAdditional3.IsEnabled = True
            //cmbAdditional4.IsEnabled = True

            List<string> userList = e.Result.ToList();
            userList.Sort(); ;
            cmbAddress.Items.Add("Admin");
            cmbAdditional1.Items.Add("Admin");
            cmbAdditional2.Items.Add("Admin");
            cmbAdditional3.Items.Add("Admin");
            cmbAdditional4.Items.Add("Admin");
            foreach (string user in userList)
            {
                    cmbAddress.Items.Add(user);
                    cmbAdditional1.Items.Add(user);
                    cmbAdditional2.Items.Add(user);
                    cmbAdditional3.Items.Add(user);
                    cmbAdditional4.Items.Add(user);
            }
           

        }
        private void cmdSend_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (cmbAddress.SelectionBoxItem == null)
            {
                lblNoUser.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                lblNoUser.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(txtSubject.Text.Trim()))
            {
                lblNoSubject.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                lblNoSubject.Visibility = System.Windows.Visibility.Collapsed;
            }

            ComboBox cmbBox = default(ComboBox);
            short numOfRecipients = 0; 
            for (short n = 1; n <= 4; n++)
            {
                cmbBox = (ComboBox)LayoutRoot.FindName("cmbAdditional" + n);
                if (cmbBox.SelectionBoxItem != null)
                {
                    numOfRecipients += 1;
                }
            }

            for (short n = 1; n <= numOfRecipients; n++)
            {
                cmbBox = (ComboBox)LayoutRoot.FindName("cmbAdditional" + n);

                if (cmbBox.SelectionBoxItem == cmbAddress.SelectionBoxItem)
                {
                    lblNoDupes.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

            }

            for (short n = 2; n <= numOfRecipients; n++)
            {
                cmbBox = (ComboBox)LayoutRoot.FindName("cmbAdditional" + n);

                if (cmbBox.SelectionBoxItem == cmbAdditional1.SelectionBoxItem)
                {
                    lblNoDupes.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

            }

            for (int n = 3; n <= numOfRecipients; n++)
            {
                cmbBox = (ComboBox)LayoutRoot.FindName("cmbAdditional" + n);
                if (cmbBox.SelectionBoxItem == cmbAdditional2.SelectionBoxItem)
                {
                    lblNoDupes.Visibility = System.Windows.Visibility.Visible;
                    return;
                }
            }

            for (int n = 4; n <= numOfRecipients; n++)
            {
                cmbBox = (ComboBox)LayoutRoot.FindName("cmbAdditional" + n);
                if (cmbBox.SelectionBoxItem == cmbAdditional3.SelectionBoxItem)
                {
                    lblNoDupes.Visibility = System.Windows.Visibility.Visible;
                    return; 
                }
            }

  
                 
            string addresses = cmbAddress.SelectionBoxItem.ToString();
            for (int n = 1; n <= numOfRecipients; n++)
            {
                cmbBox = (ComboBox)LayoutRoot.FindName("cmbAdditional" + n);
                addresses += "," + cmbBox.SelectionBoxItem.ToString();
            }
            //addresses = addresses.Replace("Admin", "Seattleite");
            
            mailClient = new SQLReference.Service1ConsoleClient();
            mailClient.SendEmailCompleted += mailClient_smtpSent;
            mailClient.SendEmailAsync(M.username, addresses, txtSubject.Text, txtBody.Text, true);

        }
        public void mailClient_smtpSent(object sender, SQLReference.SendEmailCompletedEventArgs e)
        {
            mailClient.SendEmailCompleted -= mailClient_smtpSent;
            if (e.Result == "Success")
            {
                MessageBox.Show("Email has been sent.");
                txtBody.Text = "";
                txtSubject.Text = "";

            }
            else
            {
                MessageBox.Show("There was an error sending the email." + Environment.NewLine + Environment.NewLine + "Message: " + e.Result);
            }
        }

        private void cmbAddress_SelectionChanged(System.Object sender, System.EventArgs e)
        {
            if (cmbAddress.SelectionBoxItem != null && cmbAddress.SelectedIndex > 0)
            {
                cmbAdditional1.IsEnabled = true;
            }
            else
            {
                cmbAdditional1.IsEnabled = false;
                cmbAdditional1.SelectedIndex = 0;
                cmbAdditional2.IsEnabled = false;
                cmbAdditional2.SelectedIndex = 0;
                cmbAdditional3.IsEnabled = false;
                cmbAdditional3.SelectedIndex = 0;
                cmbAdditional4.IsEnabled = false;
                cmbAdditional4.SelectedIndex = 0;
            }
        }

        private void cmbAdditional1_SelectionChanged(System.Object sender, System.EventArgs e)
        {
            if (cmbAdditional1.SelectionBoxItem != null && cmbAdditional1.SelectedIndex > 0)
            {
                cmbAdditional2.IsEnabled = true;
            }
            else
            {
                cmbAdditional2.IsEnabled = false;
                cmbAdditional2.SelectedIndex = 0;
                cmbAdditional3.IsEnabled = false;
                cmbAdditional3.SelectedIndex = 0;
                cmbAdditional4.IsEnabled = false;
                cmbAdditional4.SelectedIndex = 0;
            }
        }

        private void cmbAdditional2_SelectionChanged(System.Object sender, System.EventArgs e)
        {
            if (cmbAdditional2.SelectionBoxItem != null && cmbAdditional2.SelectedIndex > 0)
            {
                cmbAdditional3.IsEnabled = true;
            }
            else
            {
                cmbAdditional3.IsEnabled = false;
                cmbAdditional3.SelectedIndex = 0;
                cmbAdditional4.IsEnabled = false;
                cmbAdditional4.SelectedIndex = 0;
            }
        }

        private void cmbAdditional3_SelectionChanged(System.Object sender, System.EventArgs e)
        {
            if (cmbAdditional3.SelectionBoxItem != null && cmbAdditional3.SelectedIndex > 0)
            {
                cmbAdditional4.IsEnabled = true;
            }
            else
            {
                cmbAdditional4.IsEnabled = false;
                cmbAdditional4.SelectedIndex = 0;
            }
        }
    }
}
