﻿#pragma checksum "C:\Users\Eric.Eric-PC\Documents\Visual Studio 2010\Projects\DuelPortalCS\DuelPortalCS\Views\Lobby.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "63CFADB870DA33F067B9F2D1A17BEA20"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace DuelPortalCS.Views {
    
    
    public partial class Lobby : System.Windows.Controls.Page {
        
        internal System.Windows.Controls.Canvas LayoutRoot;
        
        internal System.Windows.Controls.ListBox lstTournamentHost;
        
        internal System.Windows.Controls.TextBlock Label1;
        
        internal System.Windows.Controls.Button cmdHost;
        
        internal System.Windows.Controls.ListBox lstTraditionalHost;
        
        internal System.Windows.Controls.TextBlock lblWaiting;
        
        internal System.Windows.Controls.Button cmdDuel;
        
        internal System.Windows.Controls.TextBlock TextBlock1;
        
        internal System.Windows.Shapes.Ellipse Ellipse1;
        
        internal System.Windows.Controls.TextBlock Label3;
        
        internal System.Windows.Controls.ListBox lstCurrentDuel;
        
        internal System.Windows.Controls.Button cmdWatch;
        
        internal System.Windows.Controls.ComboBox cmbChangeDeck;
        
        internal System.Windows.Controls.TextBlock Label4;
        
        internal System.Windows.Controls.RadioButton rdTournament;
        
        internal System.Windows.Controls.RadioButton rdTraditional;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/DuelPortalCS;component/Views/Lobby.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Canvas)(this.FindName("LayoutRoot")));
            this.lstTournamentHost = ((System.Windows.Controls.ListBox)(this.FindName("lstTournamentHost")));
            this.Label1 = ((System.Windows.Controls.TextBlock)(this.FindName("Label1")));
            this.cmdHost = ((System.Windows.Controls.Button)(this.FindName("cmdHost")));
            this.lstTraditionalHost = ((System.Windows.Controls.ListBox)(this.FindName("lstTraditionalHost")));
            this.lblWaiting = ((System.Windows.Controls.TextBlock)(this.FindName("lblWaiting")));
            this.cmdDuel = ((System.Windows.Controls.Button)(this.FindName("cmdDuel")));
            this.TextBlock1 = ((System.Windows.Controls.TextBlock)(this.FindName("TextBlock1")));
            this.Ellipse1 = ((System.Windows.Shapes.Ellipse)(this.FindName("Ellipse1")));
            this.Label3 = ((System.Windows.Controls.TextBlock)(this.FindName("Label3")));
            this.lstCurrentDuel = ((System.Windows.Controls.ListBox)(this.FindName("lstCurrentDuel")));
            this.cmdWatch = ((System.Windows.Controls.Button)(this.FindName("cmdWatch")));
            this.cmbChangeDeck = ((System.Windows.Controls.ComboBox)(this.FindName("cmbChangeDeck")));
            this.Label4 = ((System.Windows.Controls.TextBlock)(this.FindName("Label4")));
            this.rdTournament = ((System.Windows.Controls.RadioButton)(this.FindName("rdTournament")));
            this.rdTraditional = ((System.Windows.Controls.RadioButton)(this.FindName("rdTraditional")));
        }
    }
}

