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
    public partial class cldInput : ChildWindow
    {
        public string input = "";
        public string message
        {
            get { return Label1.Text; }
            set { Label1.Text = value; }
        }

        public cldInput(string myMessage)
        {
            Closed += ChildWindow_Closed;
            InitializeComponent();
            Label1.Text = myMessage;

            TextBox1.Text = "";
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            input = TextBox1.Text.Trim();
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            input = "";
            this.DialogResult = false;
        }


        private void ChildWindow_Closed(System.Object sender, System.EventArgs e)
        {
            TextBox1.Text = "";
            Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);

        }
    }
}

