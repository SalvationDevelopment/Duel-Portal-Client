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
    public partial class frmEdit : ChildWindow
    {

        public frmEdit()
        {
            Loaded += frmEdit_Loaded;
            InitializeComponent();
            this.Closed += delegate
            {
                Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            };

        }
        private void minipopulate()
        {
            bool first = false;
            int n = 0;
            lstCards.Items.Clear();
            List<string> items = new List<string>();
            if (cmbFilter.Items.Count == 0)
            {
                cmbFilter.Items.Clear();
                cmbFilter.Items.Add("All");
                cmbFilter.Items.Add("--PUBLIC--");
                cmbFilter.Items.Add("--PRIVATE--");
                first = true;
            }
            for (n = 1; n <= M.TotalCards; n++)
            {
                if (M.CardStats[n].Name != null)
                {
                    items.Add(M.CardStats[n].Name);
                    if (!string.IsNullOrEmpty(M.CardStats[n].SpecialSet) && cmbFilter.Items.Count(k => k.ToString() == M.CardStats[n].SpecialSet) == 0)
                        items.Add(M.CardStats[n].SpecialSet);
                }
            }

            items.Sort();
            for (n = 0; n < items.Count; n++)
                lstCards.Items.Add(items[n]);
            
            if (first)
               cmbFilter.SelectedIndex = 0;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cmdChoose_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstCards.SelectedIndex == -1)
                return;
            int id = M.findID(lstCards.SelectedItem.ToString());

            //if (M.CardStats[id].Creator == "PUB")
            //{MessageBox.Show("You cannot edit Public cards.");return; }
            if (!string.IsNullOrEmpty(M.CardStats[id].Creator) && M.CardStats[id].Creator.Contains("_"))
            {
                if (!M.isOwner(id))
                {
                    MessageBox.Show("This card is read-only, and you are not the original owner.");
                    return;
                }

            }
            M.EditExistingCardID = id;


            this.DialogResult = true;
        }

   
        private void frmEdit_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            M.EditExistingCardID = 0;

            minipopulate();
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            lstCards.Items.Clear();
            List<string> items = new List<string>();
            for (int n = 1; n <= M.TotalCards; n++)
            {
                if (M.CardStats[n].Name.IndexOf(TextBox1.Text, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    if (cmbFilter.SelectedItem.ToString() == "All")
                        items.Add(M.CardStats[n].Name);
                    else if (cmbFilter.SelectedItem.ToString() == "--PUBLIC--")
                    { if (M.CardStats[n].Creator == "PUB") items.Add(M.CardStats[n].Name); }
                    else if (cmbFilter.SelectedItem.ToString() == "--PRIVATE--")
                    { if (M.CardStats[n].Creator != "PUB") items.Add(M.CardStats[n].Name); }
                    else if (M.CardStats[n].SpecialSet == cmbFilter.SelectedItem.ToString())
                        items.Add(M.CardStats[n].Name);
                }
            }
            items.Sort();
            for (int n = 0; n < items.Count; n++)
                lstCards.Items.Add(items[n]);
        }

        private void TextBox1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TextBox1.Text == "Search here")
                TextBox1.Text = "";
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFilter != null && cmbFilter.SelectedItem != null && cmbFilter.SelectedItem.ToString() != "System.Windows.Controls.ComboBoxItem")
            {
                lstCards.Items.Clear();
                List<string> items = new List<string>();
                for (int n = 1; n <= M.TotalCards; n++)
                {
                    if (/*M.CardStats[n].Creator != "PUB" &&*/ M.CardStats[n].Name.IndexOf(TextBox1.Text, StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        if (cmbFilter.SelectedItem.ToString() == "All")
                            items.Add(M.CardStats[n].Name);
                        else if (cmbFilter.SelectedItem.ToString() == "--PUBLIC--")
                        { if (M.CardStats[n].Creator == "PUB") items.Add(M.CardStats[n].Name); }
                        else if (cmbFilter.SelectedItem.ToString() == "--PRIVATE--")
                        { if (M.CardStats[n].Creator != "PUB") items.Add(M.CardStats[n].Name); }
                        else if (M.CardStats[n].SpecialSet == cmbFilter.SelectedItem.ToString())
                            items.Add(M.CardStats[n].Name);
                    }
                }
                items.Sort();
                for (int n = 0; n < items.Count; n++)
                    lstCards.Items.Add(items[n]);
            }
        }
    }
}

