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
    public partial class RateUser : ChildWindow
    {
        SQLReference.Service1ConsoleClient sqlCli = new SQLReference.Service1ConsoleClient();
        public RateUser()
        {
            InitializeComponent();
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (sqlCli != null) sqlCli.CloseAsync();
            base.OnClosing(e);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button1_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            Button1.IsEnabled = false;
            RadioButton radio = default(RadioButton);
            int rating = 0;
            for (int n = 0; n <= 10; n++)
            {
                radio = (RadioButton)LayoutRoot.FindName("rd" + n);
                if ((bool)radio.IsChecked)
                {
                    rating = n;
                    break; // TODO: might not be correct. Was : Exit For
                }
            }


            sqlCli.rateCompleted += rating_Result;
            sqlCli.rateAsync(M.username, M.opponent, rating);
        }
        private void rating_Result(object sender, SQLReference.rateCompletedEventArgs e)
        {
            sqlCli.rateCompleted -= rating_Result;
            if (e.Result == true)
            {
                MessageBox.Show("Rating received.");
            }
            else
            {
                MessageBox.Show("There was an error rating the opponent.");
            }
            DialogResult = true;
        }

        private void Button2_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

