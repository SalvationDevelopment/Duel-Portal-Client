﻿#pragma checksum "C:\Users\Eric.Eric-PC\documents\visual studio 2010\Projects\DuelPortalCS\DuelPortalCS\Views\frmViewDrag.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "75479932D47EA9D9D5ABD433D4DF15B6"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using SilverFlow.Controls;
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
    
    
    public partial class frmViewDrag : System.Windows.Controls.UserControl {
        
        internal SilverFlow.Controls.FloatingWindowHost Root;
        
        internal System.Windows.Controls.Button OKButton;
        
        internal System.Windows.Controls.ListBox lstCards;
        
        internal System.Windows.Controls.Button cmdToGrave;
        
        internal System.Windows.Controls.Button cmdBanish;
        
        internal System.Windows.Controls.Button cmdToHand;
        
        internal System.Windows.Controls.Button cmdToTop;
        
        internal System.Windows.Controls.Button cmdToBottom;
        
        internal System.Windows.Controls.Button cmdToField;
        
        internal System.Windows.Controls.TextBlock lblDuelName;
        
        internal System.Windows.Controls.Image imgDuelAttribute;
        
        internal System.Windows.Controls.TextBlock lblDuelType;
        
        internal System.Windows.Controls.Image imgSTIcon;
        
        internal System.Windows.Controls.TextBox lblDuelATK;
        
        internal System.Windows.Controls.TextBox lblDuelDEF;
        
        internal System.Windows.Controls.TextBlock lblATKplaceholder;
        
        internal System.Windows.Controls.TextBlock lblDEFPlaceholder;
        
        internal System.Windows.Controls.StackPanel stackPanel1;
        
        internal System.Windows.Controls.Image imgStars12;
        
        internal System.Windows.Controls.Image imgStars11;
        
        internal System.Windows.Controls.Image imgStars10;
        
        internal System.Windows.Controls.Image imgStars9;
        
        internal System.Windows.Controls.Image imgStars8;
        
        internal System.Windows.Controls.Image imgStars7;
        
        internal System.Windows.Controls.Image imgStars6;
        
        internal System.Windows.Controls.Image imgStars5;
        
        internal System.Windows.Controls.Image imgStars4;
        
        internal System.Windows.Controls.Image imgStars3;
        
        internal System.Windows.Controls.Image imgStars2;
        
        internal System.Windows.Controls.Image imgStars1;
        
        internal System.Windows.Controls.TextBox lblDuelLore;
        
        internal System.Windows.Controls.Label deckHelperUp;
        
        internal System.Windows.Controls.Label deckHelperDown;
        
        internal System.Windows.Controls.Label lblViewing;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/DuelPortalCS;component/Views/frmViewDrag.xaml", System.UriKind.Relative));
            this.Root = ((SilverFlow.Controls.FloatingWindowHost)(this.FindName("Root")));
            this.OKButton = ((System.Windows.Controls.Button)(this.FindName("OKButton")));
            this.lstCards = ((System.Windows.Controls.ListBox)(this.FindName("lstCards")));
            this.cmdToGrave = ((System.Windows.Controls.Button)(this.FindName("cmdToGrave")));
            this.cmdBanish = ((System.Windows.Controls.Button)(this.FindName("cmdBanish")));
            this.cmdToHand = ((System.Windows.Controls.Button)(this.FindName("cmdToHand")));
            this.cmdToTop = ((System.Windows.Controls.Button)(this.FindName("cmdToTop")));
            this.cmdToBottom = ((System.Windows.Controls.Button)(this.FindName("cmdToBottom")));
            this.cmdToField = ((System.Windows.Controls.Button)(this.FindName("cmdToField")));
            this.lblDuelName = ((System.Windows.Controls.TextBlock)(this.FindName("lblDuelName")));
            this.imgDuelAttribute = ((System.Windows.Controls.Image)(this.FindName("imgDuelAttribute")));
            this.lblDuelType = ((System.Windows.Controls.TextBlock)(this.FindName("lblDuelType")));
            this.imgSTIcon = ((System.Windows.Controls.Image)(this.FindName("imgSTIcon")));
            this.lblDuelATK = ((System.Windows.Controls.TextBox)(this.FindName("lblDuelATK")));
            this.lblDuelDEF = ((System.Windows.Controls.TextBox)(this.FindName("lblDuelDEF")));
            this.lblATKplaceholder = ((System.Windows.Controls.TextBlock)(this.FindName("lblATKplaceholder")));
            this.lblDEFPlaceholder = ((System.Windows.Controls.TextBlock)(this.FindName("lblDEFPlaceholder")));
            this.stackPanel1 = ((System.Windows.Controls.StackPanel)(this.FindName("stackPanel1")));
            this.imgStars12 = ((System.Windows.Controls.Image)(this.FindName("imgStars12")));
            this.imgStars11 = ((System.Windows.Controls.Image)(this.FindName("imgStars11")));
            this.imgStars10 = ((System.Windows.Controls.Image)(this.FindName("imgStars10")));
            this.imgStars9 = ((System.Windows.Controls.Image)(this.FindName("imgStars9")));
            this.imgStars8 = ((System.Windows.Controls.Image)(this.FindName("imgStars8")));
            this.imgStars7 = ((System.Windows.Controls.Image)(this.FindName("imgStars7")));
            this.imgStars6 = ((System.Windows.Controls.Image)(this.FindName("imgStars6")));
            this.imgStars5 = ((System.Windows.Controls.Image)(this.FindName("imgStars5")));
            this.imgStars4 = ((System.Windows.Controls.Image)(this.FindName("imgStars4")));
            this.imgStars3 = ((System.Windows.Controls.Image)(this.FindName("imgStars3")));
            this.imgStars2 = ((System.Windows.Controls.Image)(this.FindName("imgStars2")));
            this.imgStars1 = ((System.Windows.Controls.Image)(this.FindName("imgStars1")));
            this.lblDuelLore = ((System.Windows.Controls.TextBox)(this.FindName("lblDuelLore")));
            this.deckHelperUp = ((System.Windows.Controls.Label)(this.FindName("deckHelperUp")));
            this.deckHelperDown = ((System.Windows.Controls.Label)(this.FindName("deckHelperDown")));
            this.lblViewing = ((System.Windows.Controls.Label)(this.FindName("lblViewing")));
        }
    }
}

