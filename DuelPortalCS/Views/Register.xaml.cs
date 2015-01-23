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
    public partial class Register : ChildWindow
    {

        public event EventHandler<GenericEventArgs<SQLReference.LoginRegisterData>> OnRegisterAccepted;
        SQLReference.Service1ConsoleClient cli;
        
        public Register()
        {
            InitializeComponent();
            chkAllowMessaging.Content += Environment.NewLine + "(Address will be hidden)";
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
            foreach (string pool in M.PoolOptions)
            {
                if (pool != M.DEFAULT_SET)
                    cmbCustom.Items.Add(pool);
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
           if (cli != null)  cli.CloseAsync();
            base.OnClosing(e);
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        void ChangeEditing(bool isEnabled)
        {
            txtUsername.IsEnabled = isEnabled;
            txtPassword.IsEnabled = isEnabled;
            txtEmail.IsEnabled = isEnabled;
            cmbCustom.IsEnabled = isEnabled;
            rdCustom.IsEnabled = isEnabled;
            rdDefault.IsEnabled = isEnabled;
        }

        private void cmdSendVerification_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (cmdSendVerification.Content.ToString() == "Resend Verification")
            {
                SendEmail(txtEmail.Text, ver);
                cmdSendVerification.IsEnabled = false;
                return;
            }
            txtUsername.Text = txtUsername.Text.Trim();
            txtPassword.Text = txtPassword.Text.Trim();
            txtEmail.Text = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                lblNeedUsername.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                if (txtUsername.Text.Contains("|") || txtPassword.Text.Contains("|") || txtEmail.Text.Contains("|"))
                {
                    MessageBox.Show("Usernames, passwords and emails cannot contain the Bar | character");
                    return;
                }
                if (txtUsername.Text.Contains("portalguest"))
                {
                    MessageBox.Show("The word portalguest is a special keyword in the program. Please choose something else.");
                    return;
                }
                if (txtUsername.Text == "All")
                {
                    MessageBox.Show("The word all is a special keyword in the program. Please choose something else.");
                    return;
                }
                if (txtUsername.Text == "PUB")
                {
                    MessageBox.Show("The word PUB is a special keyword in the program. Please choose something else.");
                    return;
                }
                if (txtUsername.Text.Contains("&&"))
                {
                    MessageBox.Show("Usernames cannot contain double ampersand && characters");
                    return;
                }
                if (txtUsername.Text.Contains("_"))
                {
                    MessageBox.Show("Usernames cannot contain underscore _ characters");
                    return;
                }
                lblNeedUsername.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                lblNeedPassword.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                lblNeedPassword.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(txtEmail.Text) || txtEmail.Text.Contains("@") == false)
            {
                lblNeedEmail.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                lblNeedEmail.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (txtUsername.Text.Length > 30)
            {
                MessageBox.Show("Usernames must be 30 characters or fewer.");
                return;
            }
            if (txtPassword.Text.Length > 30)
            {
                MessageBox.Show("Passwords must be 30 characters or fewer.");
                return;
            }
            if (txtEmail.Text.Length > 50)
            {
                MessageBox.Show("Emails must be 50 characters or fewer.");
                return;
            }

            if (rdCustom.IsChecked == true && cmbCustom.SelectedValue == null)
            {
                lblNeedCustom.Visibility = System.Windows.Visibility.Visible;
                return;
            }
            else
            {
                lblNeedCustom.Visibility = System.Windows.Visibility.Collapsed;
            }


            ver = GenerateVerification();
            string chosenPool = "";
            if (rdDefault.IsChecked == true)
            {
                chosenPool = "Default";
            }
            else if ((bool)rdCustom.IsChecked)
            {
                chosenPool = cmbCustom.SelectionBoxItem.ToString();
            }
            ChangeEditing(false);
           
            cli = new SQLReference.Service1ConsoleClient();
            cli.createOrVerifyNewUserCompleted += cli_createUserDone;
            cli.createOrVerifyNewUserAsync(txtUsername.Text, txtPassword.Text, ver, txtEmail.Text, (bool)chkAllowMessaging.IsChecked, false, chosenPool);


        }
        public string ver;

        public bool SendEmail(string address, string verification)
        {
            cli = new SQLReference.Service1ConsoleClient();
            cli.SendEmailCompleted += smtp_Send;
            cli.SendEmailAsync(txtUsername.Text, txtEmail.Text, "Verification Code", txtUsername.Text + ", your verification code is:" + Environment.NewLine + Environment.NewLine + verification, false);
            return true;
        }

        public void smtp_Send(object sender, SQLReference.SendEmailCompletedEventArgs e)
        {
            cli.SendEmailCompleted -= smtp_Send;
            if (e.Result != "Success")
            {
                cmdSendVerification.IsEnabled = true;
                MessageBox.Show("There was an error sending the email. Make sure the address is valid." + Environment.NewLine + Environment.NewLine + "Message: " + e.Result);
            }
            cmdSendVerification.IsEnabled = true;
        }
        public void cli_createUserDone(object sender, SQLReference.createOrVerifyNewUserCompletedEventArgs e)
        {
            cli.createOrVerifyNewUserCompleted -= cli_createUserDone;

            ChangeEditing(true);

            txtVerify.IsEnabled = true;
            
            

            cmdSendVerification.Content = "Resend Verification";

            if (e.Error != null)
            {
                MessageBox.Show("There was an error creating new user. The server may be down.");
            }
            else if (e.Result.Verified) //mail is down, blah 
            {
                if (!string.IsNullOrEmpty(e.Result.FailMessage))
                {
                    MessageBox.Show(e.Result.FailMessage);
                }
                e.Result.FailMessage = null;
                if (OnRegisterAccepted != null)
                    OnRegisterAccepted(this, new GenericEventArgs<SQLReference.LoginRegisterData>(e.Result));


                this.DialogResult = true;
            }
            else if (!string.IsNullOrEmpty(e.Result.FailMessage))
            {
                MessageBox.Show(e.Result.FailMessage);
            }
            else
            {
                lblSentVerification.Visibility = System.Windows.Visibility.Visible;
                lblVerification.Visibility = System.Windows.Visibility.Visible;
                cmdVerify.Visibility = System.Windows.Visibility.Visible;
                txtVerify.Visibility = System.Windows.Visibility.Visible;
                ChangeEditing(false);
            }
        }
        public string GenerateVerification()
        {
            string ver = "";
            Random rnd = new Random();
            for (int n = 0; n <= 9; n++)
            {
                ver += ((char)(M.Rand(97, 122, rnd))).ToString();
            }
            return ver.ToString();
        }

        private void cmdVerify_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            cli.createOrVerifyNewUserCompleted += cli_verify;


            string chosenPool = "";
            if (rdDefault.IsChecked == true)
            {
                chosenPool = "Default";
            }
            else if ((bool)rdCustom.IsChecked)
            {
                chosenPool = cmbCustom.SelectionBoxItem.ToString();
            }

            cli.createOrVerifyNewUserAsync(txtUsername.Text, txtPassword.Text, txtVerify.Text, txtEmail.Text, (bool)chkAllowMessaging.IsChecked, true, chosenPool, null);
        }

        private void cli_verify(object sender, SQLReference.createOrVerifyNewUserCompletedEventArgs e)
        {
            cli.createOrVerifyNewUserCompleted -= cli_verify;
            if (e.Result.Verified)
            {
                if (string.IsNullOrEmpty(e.Result.FailMessage))
                    MessageBox.Show("You are now verified, " + txtUsername.Text);
                else
                    MessageBox.Show(e.Result.FailMessage); //Shows 'email is down, blah blah blah'

                if (OnRegisterAccepted != null)
                    OnRegisterAccepted(this, new GenericEventArgs<SQLReference.LoginRegisterData>(e.Result));
                
               
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("The verification code was incorrect.");
            }
        }

    }
}

