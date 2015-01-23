using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Browser;
using System.Windows.Media.Animation;



namespace ClassLibrary
{

    internal sealed class MouseButtonHelper
    {
        private MouseButtonHelper()
        {
        }
        private const long k_DoubleClickSpeed = 500;

        private const double k_MaxMoveDistance = 10;
        private static long _LastClickTicks = 0;
        private static Point _LastPosition;

        private static WeakReference _LastSender;
        static internal bool IsDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(null);
            long clickTicks = DateTime.Now.Ticks;
            long elapsedTicks = clickTicks - _LastClickTicks;
            long elapsedTime = elapsedTicks / TimeSpan.TicksPerMillisecond;
            bool quickClick = (elapsedTime <= k_DoubleClickSpeed);
            bool senderMatch = (_LastSender != null && sender.Equals(_LastSender.Target));

            if (senderMatch && quickClick && Distance(position, _LastPosition) <= k_MaxMoveDistance)
            {
                // Double click!
                _LastClickTicks = 0;
                _LastSender = null;
                return true;
            }


            // Not a double click
            _LastClickTicks = clickTicks;
            _LastPosition = position;
            if (!quickClick)
            {
                _LastSender = new WeakReference(sender);
            }
            return false;
        }


        private static double Distance(Point pointA, Point pointB)
        {
            double x = pointA.X - pointB.X;
            double y = pointA.Y - pointB.Y;
            return Math.Sqrt(x * x + y * y);
        }
    }
    


public class BrowserScreenInformation
{
/// <summary>
/// During static instantiation, only the Netscape flag is checked
/// </summary>
static BrowserScreenInformation()

{
_isNavigator = HtmlPage.BrowserInformation.Name.Contains("Netscape");

}

/// <summary>

/// Flag indicating Navigator/Firefox/Safari or Internet Explorer

/// </summary>
private static bool _isNavigator;

/// <summary>

/// Provides quick access to the window.screen ScriptObject

/// </summary>
private static ScriptObject Screen

{

get

{
ScriptObject screen = (ScriptObject)HtmlPage.Window.GetProperty("screen");if (screen == null)

{
throw new InvalidOperationException();

}
return screen;

}

}

/// <summary>
/// Gets the window object's client width
/// </summary>
public static double ClientWidth

{

get

{
return _isNavigator ? (double)HtmlPage.Window.GetProperty("innerWidth"): (double)HtmlPage.Document.Body.GetProperty("clientWidth");

}

}

/// <summary>
/// Gets the window object's client height
/// </summary>
public static double ClientHeight

{

get

{
return _isNavigator ? (double)HtmlPage.Window.GetProperty("innerHeight"): (double)HtmlPage.Document.Body.GetProperty("clientHeight");

}

}

/// <summary>

/// Gets the current horizontal scrolling offset

/// </summary>
public static double ScrollLeft

{

get

{
return _isNavigator ? (double)HtmlPage.Window.GetProperty("pageXOffset"): (double)HtmlPage.Document.Body.GetProperty("scrollLeft");

}

}

/// <summary>

/// Gets the current vertical scrolling offset

/// </summary>
public static double ScrollTop

{

get

{
return _isNavigator ? (double)HtmlPage.Window.GetProperty("pageYOffset"): (double)HtmlPage.Document.Body.GetProperty("scrollHeight");

}

}

/// <summary>
/// Gets the width of the entire display
/// </summary>
public static double ScreenWidth

{

get

{
return (double)Screen.GetProperty("width");

}

}

/// <summary>
/// Gets the height of the entire display
/// </summary>
public static double ScreenHeight

{

get

{
return (double)Screen.GetProperty("height");

}

}

/// <summary>

/// Gets the width of the available screen real estate, excluding the dock

/// or task bar

/// </summary>
public static double AvailableScreenWidth

{

get

{
return (double)Screen.GetProperty("availWidth");

}

}

/// <summary>

/// Gets the height of the available screen real estate, excluding the dock

/// or task bar

/// </summary>
public static double AvailableScreenHeight

{

get

{
return (double)Screen.GetProperty("availHeight");

}

}

/// <summary>

/// Gets the absolute left pixel position of the window in display coordinates

/// </summary>
public static double ScreenPositionLeft

{

get

{
return _isNavigator ? (double)HtmlPage.Window.GetProperty("screenX"): (double)HtmlPage.Window.GetProperty("screenLeft");

}

}

/// <summary>

/// Gets the absolute top pixel position of the window in display coordinates

/// </summary>
public static double ScreenPositionTop

{

get

{
return _isNavigator ? (double)HtmlPage.Window.GetProperty("screenY"): (double)HtmlPage.Window.GetProperty("screenTop");

}

}

}


}
