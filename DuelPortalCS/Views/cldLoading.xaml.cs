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
    public partial class cldLoading : ChildWindow
    {
        public cldLoading(string loadText)
        {
            InitializeComponent();
            TextBlock1.Text = loadText;
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

