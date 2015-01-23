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
    public partial class cldDropDownDialogue : ChildWindow
    {
        public string chosen;
        public cldDropDownDialogue(string dialogue)
        {

            InitializeComponent();
            lblDialogue.Text = dialogue;
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };
        }

        public void addChoice(string choice)
        {
            cmbChoices.Items.Add(choice);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cmdChoose_Click(object sender, RoutedEventArgs e)
        {
            chosen = (string)cmbChoices.SelectedItem;
            this.DialogResult = true;
        }

        private void cmbChoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmdChoose.IsEnabled = true;
        }
    }
}

