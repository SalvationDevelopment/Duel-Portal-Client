﻿#pragma checksum "C:\Users\Eric.Eric-PC\Documents\Visual Studio 2010\Projects\DuelPortalCS\DuelPortalCS\Views\Register.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8B4318DE7DA1C600E77E4691BFA1632C"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
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
    
    
    public partial class Register : System.Windows.Controls.ChildWindow {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Button CancelButton;
        
        internal System.Windows.Controls.TextBlock Label1;
        
        internal System.Windows.Controls.TextBox txtUsername;
        
        internal System.Windows.Controls.TextBlock Label2;
        
        internal System.Windows.Controls.TextBox txtPassword;
        
        internal System.Windows.Controls.TextBlock Label3;
        
        internal System.Windows.Controls.TextBox txtEmail;
        
        internal System.Windows.Controls.Button cmdSendVerification;
        
        internal System.Windows.Controls.TextBlock lblVerification;
        
        internal System.Windows.Controls.TextBox txtVerify;
        
        internal System.Windows.Controls.TextBlock lblSentVerification;
        
        internal System.Windows.Controls.TextBlock lblNeedUsername;
        
        internal System.Windows.Controls.TextBlock lblNeedPassword;
        
        internal System.Windows.Controls.TextBlock lblNeedEmail;
        
        internal System.Windows.Controls.CheckBox chkAllowMessaging;
        
        internal System.Windows.Controls.RadioButton rdDefault;
        
        internal System.Windows.Controls.RadioButton rdCustom;
        
        internal System.Windows.Controls.ComboBox cmbCustom;
        
        internal System.Windows.Controls.TextBlock lblNeedCustom;
        
        internal System.Windows.Controls.Button cmdVerify;
        
        internal System.Windows.Controls.TextBlock Label4;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/DuelPortalCS;component/Views/Register.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.CancelButton = ((System.Windows.Controls.Button)(this.FindName("CancelButton")));
            this.Label1 = ((System.Windows.Controls.TextBlock)(this.FindName("Label1")));
            this.txtUsername = ((System.Windows.Controls.TextBox)(this.FindName("txtUsername")));
            this.Label2 = ((System.Windows.Controls.TextBlock)(this.FindName("Label2")));
            this.txtPassword = ((System.Windows.Controls.TextBox)(this.FindName("txtPassword")));
            this.Label3 = ((System.Windows.Controls.TextBlock)(this.FindName("Label3")));
            this.txtEmail = ((System.Windows.Controls.TextBox)(this.FindName("txtEmail")));
            this.cmdSendVerification = ((System.Windows.Controls.Button)(this.FindName("cmdSendVerification")));
            this.lblVerification = ((System.Windows.Controls.TextBlock)(this.FindName("lblVerification")));
            this.txtVerify = ((System.Windows.Controls.TextBox)(this.FindName("txtVerify")));
            this.lblSentVerification = ((System.Windows.Controls.TextBlock)(this.FindName("lblSentVerification")));
            this.lblNeedUsername = ((System.Windows.Controls.TextBlock)(this.FindName("lblNeedUsername")));
            this.lblNeedPassword = ((System.Windows.Controls.TextBlock)(this.FindName("lblNeedPassword")));
            this.lblNeedEmail = ((System.Windows.Controls.TextBlock)(this.FindName("lblNeedEmail")));
            this.chkAllowMessaging = ((System.Windows.Controls.CheckBox)(this.FindName("chkAllowMessaging")));
            this.rdDefault = ((System.Windows.Controls.RadioButton)(this.FindName("rdDefault")));
            this.rdCustom = ((System.Windows.Controls.RadioButton)(this.FindName("rdCustom")));
            this.cmbCustom = ((System.Windows.Controls.ComboBox)(this.FindName("cmbCustom")));
            this.lblNeedCustom = ((System.Windows.Controls.TextBlock)(this.FindName("lblNeedCustom")));
            this.cmdVerify = ((System.Windows.Controls.Button)(this.FindName("cmdVerify")));
            this.Label4 = ((System.Windows.Controls.TextBlock)(this.FindName("Label4")));
        }
    }
}
