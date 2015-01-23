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
    public partial class cldChallenge : ChildWindow
    {
        public string challenger;
        public cldChallenge(string inputChallenger)
        {
            InitializeComponent();
            challenger = inputChallenger;
            textBlock1.Text = challenger + textBlock1.Text;
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        private void cmdAccept_Click(object sender, RoutedEventArgs e)
        {
            M.opponent = challenger;
            DialogResult = true;
        }

        private void cmdReject_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }


       
    }
}

