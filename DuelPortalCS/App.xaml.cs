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

namespace DuelPortalCS
{
    public partial class App : Application
    {
        public App()
        {
            this.Startup += this.Application_Startup;
            this.UnhandledException += this.Application_UnhandledException;
            this.Exit += new EventHandler(App_Exit);
            this.Host.Content.Resized += Content_Resized;
            this.Host.Content.FullScreenChanged += Content_Resized;
            InitializeComponent();
        }
        private double oldWidth;
        private double oldHeight;
        void Content_Resized(object sender, EventArgs e)
        {
            M.RaiseScreenResized(oldWidth, oldHeight);
            oldWidth = ClassLibrary.BrowserScreenInformation.ClientWidth;
            oldHeight = ClassLibrary.BrowserScreenInformation.ClientHeight;
        }

        void App_Exit(object sender, EventArgs e)
        {
     
            if (M.sock != null && M.sock.isConnected)
                M.sock.Disconnect();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.RootVisual = new MainPage();
            M.CardStats.ClearAndAdd();
        }
  
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using
            // a ChildWindow control.
            if (!System.Diagnostics.Debugger.IsAttached) 
            { 
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                ChildWindow errorWin = new ErrorWindow(e.ExceptionObject);
                errorWin.Show();
            }
        }
    }
}