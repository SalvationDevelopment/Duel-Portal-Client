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
    public partial class NotVerified : ChildWindow
    {
        public event EventHandler<GenericEventArgs<SQLReference.LoginRegisterData>> OnVerificationAccepted;
        SQLReference.Service1ConsoleClient sqlCli = new SQLReference.Service1ConsoleClient();


        private SQLReference.LoginRegisterData unverifiedData;

        public NotVerified(SQLReference.LoginRegisterData _unverifiedData)
        {
            InitializeComponent();
            unverifiedData = _unverifiedData;
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            sqlCli.createOrVerifyNewUserCompleted += sqlCli_createOrVerifyNewUserCompleted;
            textBox1.Text = textBox1.Text.Trim();
            sqlCli.createOrVerifyNewUserAsync(unverifiedData.Username, "", textBox1.Text, "", false, true, unverifiedData.Pool,null);

        }
        private void sqlCli_createOrVerifyNewUserCompleted(object sender, SQLReference.createOrVerifyNewUserCompletedEventArgs e)
        {
            if (e.Result.Verified)
            {
                
                MessageBox.Show("You are now verified, " + unverifiedData.Username);
                unverifiedData.Verified = true;
                this.DialogResult = true;
                if (OnVerificationAccepted != null)
                    OnVerificationAccepted(this, new GenericEventArgs<SQLReference.LoginRegisterData>(unverifiedData));
            }
            else
            {
                MessageBox.Show(e.Result.FailMessage);


            }
        }

        private void ChildWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sqlCli != null)
            { sqlCli.createOrVerifyNewUserCompleted -= sqlCli_createOrVerifyNewUserCompleted; sqlCli.CloseAsync(); }
        }


        public void smtp_Send(object sender, SQLReference.ResendVerificationCompletedEventArgs e)
        {
            sqlCli.ResendVerificationCompleted -= smtp_Send;
            if (e.Result == "Success")
            {
                lblVerificationSent.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                cmdResendVerification.IsEnabled = true;
                MessageBox.Show("There was an error sending the email. Make sure the address is valid." + Environment.NewLine + Environment.NewLine + "Message: " + e.Result);

            }
        }

        private void cmdResendVerification_Click(object sender, RoutedEventArgs e)
        {
            sqlCli.ResendVerificationCompleted += smtp_Send;
            sqlCli.ResendVerificationAsync(unverifiedData.Username);
            cmdResendVerification.IsEnabled = false;
            lblVerificationSent.Visibility = System.Windows.Visibility.Collapsed;
   
        }
    
    }
}

