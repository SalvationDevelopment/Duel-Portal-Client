using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DuelPortalCS.SQLReference;
using DuelPortalCS;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Net;
using DuelPortalCS.Views;
using System.Windows.Media;
using System.Windows.Controls;
using System.Linq;
using System.Windows;


    public static class Module1{
        public delegate void ScreenResizedDelegate(double oldWidth, double oldHeight);
        public static event ScreenResizedDelegate ScreenResized;
        public static void RaiseScreenResized(double oldWidth, double oldHeight) { if (ScreenResized != null) ScreenResized(oldWidth, oldHeight); }
        public static bool noInternet = false;
    
   
        
        public static CardDetails statsFromId(int index)
        {
            CardDetails newDet = new CardDetails();
            copyCardDetails(ref newDet, CardStats[index]);
            return newDet;
        }
        public static CardDetails toTrueStats(this CardDetails det)
        {
            int index = findID(det.Name);
            if (index == 0) { return null; }
            CardDetails newDet = new CardDetails();
            copyCardDetails(ref newDet, CardStats[index]);
            return newDet;
        }
        public static bool IsMonster(this CardDetails det)
        {
            return det.Attribute != "Spell" && det.Attribute != "Trap";
        }
        public static void Defaultize(this CardDetails det)
        {
            if (det.OpponentOwned)
            {
                det.Facedown = false; det.IsItHorizontal = false; det.Counters = 0;
            }
            else
                copyCardDetails(ref det, CardStats[findID(det.Name)]);
        }
        public static List<DuelPortalCS.SQLReference.CardDetails> CardStats = new List<DuelPortalCS.SQLReference.CardDetails>();
       // public static List<DuelPortalCS.SQLReference.CardDetails> OpponentHand = new List<DuelPortalCS.SQLReference.CardDetails>();

       // public static DuelPortalCS.SQLReference.CardDetails[] OpponentHand = new CardDetails[21];

        public static int TotalCards
        {
            get
            {
                return CardStats.Count - 1;
            }
        }
        public static Dictionary<int, CardDetails> OpponentHand = new Dictionary<int, CardDetails>();

        public static List<DuelPortalCS.SQLReference.CardDetails> PlayerHand = new List<DuelPortalCS.SQLReference.CardDetails>();
        public static Dictionary<int, DuelPortalCS.SQLReference.CardDetails> WatcherHand;
        public static List<DuelPortalCS.SQLReference.CardDetails> PlayerDeck = new List<DuelPortalCS.SQLReference.CardDetails>();
        public static List<DuelPortalCS.SQLReference.CardDetails> PlayerEDeck = new List<DuelPortalCS.SQLReference.CardDetails>();
        public static DuelPortalCS.SQLReference.CardDetails[] PlayerMonsters = new DuelPortalCS.SQLReference.CardDetails[6];
        public static DuelPortalCS.SQLReference.CardDetails[] PlayerST = new DuelPortalCS.SQLReference.CardDetails[6];
        public static CardDetails PlayerFSpell = new CardDetails();
         public static List<DuelPortalCS.SQLReference.CardDetails> OpponentGrave =  new List<DuelPortalCS.SQLReference.CardDetails>();
        public static List<DuelPortalCS.SQLReference.CardDetails> OpponentRFG = new List<DuelPortalCS.SQLReference.CardDetails>();

        public static DuelPortalCS.SQLReference.CardDetails[] OpponentMonsters = new DuelPortalCS.SQLReference.CardDetails[6];
        public static DuelPortalCS.SQLReference.CardDetails[] OpponentST = new DuelPortalCS.SQLReference.CardDetails[6];
        public static CardDetails OpponentFSpell = new CardDetails();
         public static List<DuelPortalCS.SQLReference.CardDetails> PlayerRFG = new List<DuelPortalCS.SQLReference.CardDetails>();
         public static List<DuelPortalCS.SQLReference.CardDetails> PlayerGrave = new List<DuelPortalCS.SQLReference.CardDetails>();
         public static int NumCardsInopDeck;
         public static int NumCardsInopEDeck;
         public static int NumCardsInOpHand;
         public static int watcherNumCardsInHand;
         public static int watcherNumCardsInDeck;
         public static int watcherNumCardsInEDeck;

       
        public static List<CardDetails>[] PlayerOverlaid = new List<CardDetails>[6];
        public static List<CardDetails>[] OpponentOverlaid = new List<CardDetails>[6];

        public static int PlayerLP = 8000;

  
        public static List<int> realDeckIDs = new List<int>();
        public static List<int> realEDeckIDs = new List<int>();
        public static List<int> realSideDeckIDs =new List<int>();

        public static string username = string.Empty;
        public static string opponent;
        public static int myUsernameId = 0;

        public static string myRoomID;


        public static SocketClient sock;
        
        public static string mySet = string.Empty;
        public const string DEFAULT_SET = "Default";
        public static HashSet<string> PoolOptions = new HashSet<string>() { "Default", "LCCG", "HCCG", "ACG" };
        public static bool isDefaultSet { get { return mySet == DEFAULT_SET; } }
        public static bool isLoggedIn { get { return username != string.Empty && username.Contains("portalguest") == false; } }
        //public static bool loadedPublic = false;
        public static bool doNotShowPublicMessageAgain;

        public static int EditExistingCardID;
        public static bool IamWatching;
        public static string WatcherMySide;
        public static string WatcherOtherSide;
        public static bool sideDecking;
       
        private static string _warnOnExitMessage = string.Empty;
        public static string warnOnExitMessage {
            get { return _warnOnExitMessage; }
            set { _warnOnExitMessage = value; }
        }
        //public static string myLegacyDeck;
        public static List<string> cardsWithImages = new List<string>();
        public static List<string> listOfMyDecks = new List<string>();
       
        public static string defaultDeckName;
        public const char DOUBLE_BAR = (char)187;
        public const int STAT_QUESTION = -1; 
        public static int ToIntCountingQuestions(this string str) 
        {
            if (str == null) return 0;
            if (str == "?")
                return STAT_QUESTION;
            else if (str == "")
                return 0;
            else
                return str.ToInt();
        }
        public static string ToStringCountingQuestions(this int i) 
        {
            if (i == STAT_QUESTION)
                return "?";
            else
                return i.ToString();
        }
        public static int ToInt(this string str)
        { 
            return Convert.ToInt32(str); 
        }
 
        public static string toPortalURL(string name, int databaseID, string theSet)
        {
            string schemeName = System.Windows.Browser.HtmlPage.Document.DocumentUri.Scheme;
            if (!isDefaultSet)
                return schemeName + "://192.227.234.101/img/" + theSet + "_" + name.Replace(" ", "%20") + ".jpg";
            else
                return schemeName + "://192.227.234.101/img/" + databaseID + ".jpg";
         
        }
        public static string toProperImageNameNoExtension(int cardid)
        {
            if (isDefaultSet)
                return Module1.CardStats[cardid].ID.ToString();
            else
                return Module1.mySet + "_" + Module1.CardStats[cardid].Name;
        }
        public static string getRealImageName(string imgName, int id, string theSet)
        {
            if (string.IsNullOrEmpty(theSet) || theSet == DEFAULT_SET)
            {
                return id.ToString();
            }
            else { return theSet + "_" + imgName; }
        }
        public static Dictionary<Image, EventHandler<RoutedEventArgs>> PendingImages = new Dictionary<Image, EventHandler<RoutedEventArgs>>();
       // public static Dictionary<Image, Image> RealToDummyImage = new Dictionary<Image, Image>();
        public static void setImage(System.Windows.Controls.Image img, string nameWithExtension, UriKind uriKind)
        {
            
            if (string.IsNullOrEmpty(nameWithExtension))
            {
                img.Source = null; return;
            }

            if (uriKind == System.UriKind.Relative)
            {
                System.Windows.Media.Imaging.BitmapImage b = new System.Windows.Media.Imaging.BitmapImage(new Uri("/DuelPortalCS;component/Images/" + nameWithExtension, System.UriKind.Relative));
                b.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                img.Source = b;
            }
            else
            {
                object obj = new object();
                lock (obj)
                {
                    System.Windows.Media.Imaging.BitmapImage b = new System.Windows.Media.Imaging.BitmapImage();

                    PendingImages[img] = (s, e) => { img.Source = b; };

                    b.ImageOpened += PendingImages[img];
                    b.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                    b.UriSource = new Uri(
                        nameWithExtension.Replace("#", "%23"), System.UriKind.Absolute);
                }

                //System.Windows.Media.Imaging.BitmapImage b = new System.Windows.Media.Imaging.BitmapImage();
                //b.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                //System.Windows.Controls.Image dummyImage = new Image();
                 
                //dummyImage.LayoutUpdated += (s, e) => { 
                //    if (!double.IsNaN(dummyImage.ActualWidth) && dummyImage.ActualWidth > 0.0)
                //        img.Source = dummyImage.Source;
                //    System.Diagnostics.Debug.WriteLine("Layout Updated");
                //};
                //b.UriSource = new Uri(nameWithExtension.Replace("#", "%23"), System.UriKind.Absolute);
                //dummyImage.Source = b;
            }

        }
        public static void setImage(System.Windows.Controls.Border bord, string nameWithExtension, UriKind uriKind)
        {
            Image img = bord.Child as Image;
            if (img == null)
            {
                img = new Image();
                bord.Child = img;
            }
            setImage(img, nameWithExtension, uriKind);
        }

        public static string AttributeToImageName(string attributeName)
        {
            switch (attributeName)
            {
                case "Fire":
                case "FIRE":
                    return "Fire.jpg";
                case "Water":
                case "WATER":
                    return "Water.jpg";
                case "Earth":
                case "EARTH":
                    return "Earth.jpg";
                case "Wind":
                case "WIND":
                    return "Wind.jpg";
                case "Light":
                case "LIGHT":
                    return "Light.jpg";
                case "Dark":
                case "DARK":
                    return "Dark.jpg";
                case "Divine":
                case "DIVINE":
                    return "Divine.jpg";
                case "Spell":
                case "SPELL":
                    return "SpellIcon.jpg";
                case "Trap":
                case "TRAP":
                    return "TrapIcon.jpg";
                default:
                    return "Unknown.png";
            }
        }
        public static string TypeToImageName(string typeName)
        {
            switch (typeName)
            {
                case "Continuous":
                    return "ContinuousIcon.jpg";
                case "Counter":
                    return "CounterIcon.jpg";
                case "Quick-Play":
                case "Quickplay":
                    return "Quick-PlayIcon.jpg";
                case "Equip":
                    return "EquipIcon.jpg";
                case "Field":
                    return "FieldIcon.jpg";
                case "Ritual":
                    return "RitualIcon.jpg";
                default:
                    return "";
            }
        }

        public static void ClearAndAdd(this List<CardDetails> list, bool trimExcess = false)
        {
            list.Clear();
            list.Add(null);
            if (trimExcess)
                list.TrimExcess();
        }
        public static void ClearAndAdd(this List<int> list)
        {
            list.Clear();
            list.Add(0);
        }
        public static void ClearArray(this CardDetails[] array)
        {
            for (int n = 1; n <= array.Length - 1; n++)
            {
                if (array[n] == null) { continue; }
                array[n].Name = null;
                array[n].ID = 0;
                array[n].Level = 0;
                array[n].ATK = 0;
                array[n].DEF = 0;
                array[n].Type = null;
                array[n].Attribute = null;
                array[n].SpecialSet = null;
                array[n].Lore = null;
                array[n].Creator = null;
                array[n].IsItHorizontal = false;
                array[n].Facedown = false;
                array[n].Counters = 0;
                array[n].OpponentOwned = false;
      
            }
        }
        #region reflection
        //public static void ClearEventInvocations(this object obj, string eventName)
        //{
        //    var fi = obj.GetType().GetEventField(eventName);
        //    if (fi == null) return;
        //    fi.SetValue(obj, null);
        //}
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }
        public static void ModifyCardInList(this List<CardDetails> list, int index, string propName, object newValue)
        {
            
            CardDetails newDetails = new CardDetails();
            copyCardDetails(ref newDetails, list[index]);
           System.Reflection.PropertyInfo propInfo = newDetails.GetType().GetProperty(propName);

           propInfo.SetValue(newDetails, Convert.ChangeType(newValue, propInfo.PropertyType, null), null);
           list.RemoveAt(index);
           list.Insert(index, newDetails);
        }
        #endregion
        public static double CLeft(this System.Windows.UIElement u) { /*System.Diagnostics.Debug.WriteLine("Left: " + Canvas.GetLeft(u));*/ return Canvas.GetLeft(u); }
        public static double CTop(this System.Windows.UIElement u) { /*System.Diagnostics.Debug.WriteLine("Top: " + Canvas.GetTop(u));*/ return Canvas.GetTop(u); }
        public static int findID(string searchString)
        {
            int n = 0;
            for (n = 1; n <= TotalCards; n++)
            {
                if (searchString == CardStats[n].Name)
                {
                    return n;
                }
            }
            return 0;
        }
        public static int findTrueID(string searchString)
        {
            int n = 0;
            for (n = 1; n <= TotalCards; n++)
            {
                if (searchString == CardStats[n].Name)
                {
                    return CardStats[n].ID;
                }
            }
            return 0;
        }
        public static int findIndexFromTrueID(int trueID)
        {
            int n = 0;
            for (n = 1; n <= TotalCards; n++)
            {
                if (CardStats[n].ID == trueID)
                {
                    return n;
                }
            }
            return 0;
     
        }
        public static void copyCardDetails(ref CardDetails target, CardDetails source)
        {
            if (target == null) target = new CardDetails();

            if (source != null)
            {
           
                target.Name = source.Name;
                target.ID = source.ID;
                target.Type = source.Type;
                target.Attribute = source.Attribute;
                target.ATK = source.ATK;
                target.DEF = source.DEF;
                target.Level = source.Level;
                target.Lore = source.Lore;
                target.IsItHorizontal = source.IsItHorizontal;
                target.Facedown = source.Facedown;
                target.Counters = source.Counters;
                target.Creator = source.Creator;
                target.OpponentOwned = source.OpponentOwned;
            }
            else
            {
                target.Name = null;
                target.ID = 0;
                target.Type = null;
                target.Attribute = null;
                target.ATK = 0;
                target.DEF = 0;
                target.Level = 0;
                target.Lore = null;
                target.IsItHorizontal = false;
                target.Facedown = false;
                target.Counters = 0;
                target.Creator = null;
                target.OpponentOwned = false;
            }
            

        }
        public static void copyCardDetails(CardDetails target, CardDetails source)
        {
            
            if (source != null)
            {

                target.Name = source.Name;
                target.ID = source.ID;
                target.Type = source.Type;
                target.Attribute = source.Attribute;
                target.ATK = source.ATK;
                target.DEF = source.DEF;
                target.Level = source.Level;
                target.Lore = source.Lore;
                target.IsItHorizontal = source.IsItHorizontal;
                target.Facedown = source.Facedown;
                target.Counters = source.Counters;
                target.Creator = source.Creator;
                target.OpponentOwned = source.OpponentOwned;
            }
            else
            {
                target.Name = null;
                target.ID = 0;
                target.Type = null;
                target.Attribute = null;
                target.ATK = 0;
                target.DEF = 0;
                target.Level = 0;
                target.Lore = null;
                target.IsItHorizontal = false;
                target.Facedown = false;
                target.Counters = 0;
                target.Creator = null;
                target.OpponentOwned = false;
            }


        }
         [System.Diagnostics.DebuggerStepThrough()]
        public static int CountNumCards(this List<CardDetails> card)
        {
           
          return card.Count - 1;

        }
   

        public static string getridofascii(string input)
        { 
            string strBld = System.Windows.Browser.HttpUtility.UrlDecode(input);
            strBld = strBld.Replace("+", " ");
            return strBld;
        }
       
        public static string getRidOfHarmfulCharacters(string input)
        {
            input = input.Replace("|", "");
            input = input.Replace("{", "");
            input = input.Replace("}", "");
            input = input.Replace("&&", "");
            return input;
        }

        public static string capitalizeFirstLetter(this string str)
        {
            System.Text.StringBuilder strBld = new StringBuilder(str);
            strBld[0] = char.ToUpper(str[0]);
            return strBld.ToString();
        }

        public static bool IsOrange(CardDetails det)
        {
          if (det != null && !string.IsNullOrEmpty(det.Type))
          {
              try
              {
                  if (det.Type.Contains("Synchro"))
                      return false;
                  if (det.Type.Contains("Ritual"))
                      return false;
                  if (det.Type.Contains("Fusion"))
                      return false;
                  if (det.Type.Contains("Xyz"))
                      return false;
              }
              catch { }
            }
            return true;
        }
        public static bool BelongsInExtra(CardDetails det)
        {
            if (det != null && !string.IsNullOrEmpty(det.Type))
            {
                try
                {
                if (det.Type.Contains("Synchro"))
                    return true;

                if (det.Type.Contains("Fusion"))
                    return true;
                if (det.Type.Contains("Xyz"))
                    return true;
                }
                 catch { }
            }
          
            return false;
        }
        public static void SortByOccurrence(this List<string> list)
        {
            List<KeyValuePair<string, int>> occurrences = new List<KeyValuePair<string, int>>();
            
            foreach (string l in list)
            {
                bool found = false;
                foreach (KeyValuePair<string, int> checkKvp in occurrences)
                {
                    if (checkKvp.Key == l)
                    { found = true; break; }
                }
                if (!found)
                {
                   KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(l, list.Count(k => k == l));
                   occurrences.Add(kvp);
                }
                
            }

            occurrences= occurrences.OrderByDescending(kvp => kvp.Value).ToList();
            list.Clear();

            foreach (KeyValuePair<string, int> kvp in occurrences)
            {
                for (int n = 1; n <= kvp.Value; n++)
                    list.Add(kvp.Key);
            }

          //  list = (List<T>)list.OrderByDescending(t => list.Count(w => w.Equals(t)));

        }
        public static string getFirstType(string str)
        {
            if (string.IsNullOrEmpty(str)) { return ""; }
            int place = str.IndexOf("/");
            if (place == -1)
                return str;
            else
                return str.Substring(0, place);
 
        }
        public static string getSecondType(string str)
        {
            int place = str.IndexOf("/");
            int nextplace = str.IndexOf("/", place + 1);
            if (place == -1 || nextplace == -1)
                return "";
            else
                return str.Substring(place + 1, nextplace - place - 1);
        }
        public static string getThirdType(string str)
        {
            int place = str.IndexOf("/");
            int nextplace = str.IndexOf("/", place + 1);
         
            return str.Substring(nextplace + 1, str.Length - nextplace - 1);
        }
        public static string ByteArrayToString(byte[] Bytes)
        {
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            if (Bytes == null) { return ""; }
            try
            {
                return encoding.GetString(Bytes, 0, Bytes.Length);
            }
            catch
            {
                return "";
            }
            //
        }
        public static void SendErrorReport(string errorClass, string errorMessage, object result)
        {
            Service1ConsoleClient client = new Service1ConsoleClient();
            client.WriteErrorReportAsync(errorClass.Replace("DuelPortalCS.SQLReference.", ""), errorMessage, result, Module1.username);
        }
       
        public static int Rand(int Low, int High, Random rnd)
        {
            
            return rnd.Next(Low, High + 1);
   
        }
        public static bool isNumeric(string checkString)
        {
            if (string.IsNullOrEmpty(checkString))
                return false; 
      

            for (int n = 0; n <= checkString.Length - 1; n++)
            {
                switch (checkString[n])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':

                        break;
                    case '-':
                        if (n > 0 || checkString.Length == 1)
                            return false;
                        break;
                    default:
                        return false;
                }

            }
            return true;
        }
        public static string GetCookie(string key)
        {
            string[] cookies = System.Windows.Browser.HtmlPage.Document.Cookies.Split(';');

            key += '=';

            foreach (string cookie in cookies)
            {
                string cookieStr = cookie.Trim();
                if (cookieStr.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    string[] vals = cookieStr.Split('=');
                    if (vals.Length >= 2)
                    {
                        return vals[1];
                    }
                    return string.Empty;
                }
            }
            return null;
        }
        public static void SetCookie(string key, string value)
        {
            string oldCookie = System.Windows.Browser.HtmlPage.Document.GetProperty("cookie") as String;
            DateTime expiration = DateTime.UtcNow + TimeSpan.FromDays(2000);
            string cookie = String.Format("{0}={1};expires={2}", key, value, expiration.ToString("R"));
            System.Windows.Browser.HtmlPage.Document.SetProperty("cookie", cookie);
        }
        public static void ScaleCanvas(Canvas cnv, double oldWidth, double oldHeight)
        {
           foreach (UIElement ui in cnv.Children)
            {
                ScaleTransform scale;
                if (ui.RenderTransform.GetType() != typeof(ScaleTransform))
                    scale = new ScaleTransform();
                else
                    scale = ui.RenderTransform as ScaleTransform;
                scale.ScaleX *= ClassLibrary.BrowserScreenInformation.ClientWidth / oldWidth;
                scale.ScaleY *= (ClassLibrary.BrowserScreenInformation.ClientHeight - 42) / oldHeight; //42 is the height of the navigation grid

                ui.RenderTransform = scale;
            }
        }
        public static void ScaleCanvas(Canvas cnv, Page page)
        {
            if (double.IsNaN(page.Width) || page.Width == 0.0) return;/*
            if (cnv.RenderTransform == null || cnv.RenderTransform.GetType() == typeof(MatrixTransform))
                cnv.RenderTransform = new TransformGroup();
            TransformGroup group = cnv.RenderTransform as TransformGroup;
            ScaleTransform scale = (ScaleTransform)group.Children.FirstOrDefault(s => s.GetType() == typeof(ScaleTransform));
            if (scale == null)
            {
                scale = new ScaleTransform();
                group.Children.Add(scale);
            }*/
            
                ScaleTransform scale = new ScaleTransform();
                scale.ScaleX = ClassLibrary.BrowserScreenInformation.ClientWidth / page.Width;
                scale.ScaleY = (ClassLibrary.BrowserScreenInformation.ClientHeight - 42) / page.Height; //42 is the height of the navigation grid

                cnv.RenderTransform = scale;
                
           // if (group.Children.FirstOrDefault(s => s.GetType() == typeof(TranslateTransform)) != null)
            //    CenterCanvas(cnv, page);
        }
      
        public static void CenterCanvas(Canvas cnv, Page page)
        {
            if (double.IsNaN(page.Width) || page.Width == 0.0) return;/*
            if (cnv.RenderTransform == null || cnv.RenderTransform.GetType() == typeof(MatrixTransform))
                cnv.RenderTransform = new TransformGroup();
            TransformGroup group = cnv.RenderTransform as TransformGroup;
            TranslateTransform trans = (TranslateTransform)group.Children.FirstOrDefault(s => s.GetType() == typeof(TranslateTransform));
            if (trans == null)
            {
                trans = new TranslateTransform();
                group.Children.Add(trans);
            }*/
            TranslateTransform trans = new TranslateTransform();
            trans.X = (ClassLibrary.BrowserScreenInformation.ClientWidth / 2) - (page.Width / 2);
            trans.Y = (ClassLibrary.BrowserScreenInformation.ClientHeight / 2) - (page.Height / 2);
            if (trans.X < 0) trans.X = 0;
            if (trans.Y < 0) trans.Y = 0;
            cnv.RenderTransform = trans; 
        }
        public static void CenterGrid(Grid grd, Page page)
        {
            if (double.IsNaN(page.Width) || page.Width == 0.0) return;
            if (grd.RenderTransform == null || grd.RenderTransform.GetType() == typeof(MatrixTransform))
                grd.RenderTransform = new TransformGroup();
            TransformGroup group = grd.RenderTransform as TransformGroup;
            TranslateTransform trans = (TranslateTransform)group.Children.FirstOrDefault(s => s.GetType() == typeof(TranslateTransform));
            if (trans == null)
            {
                trans = new TranslateTransform();
                group.Children.Add(trans);
            }
            trans.X = (ClassLibrary.BrowserScreenInformation.ClientWidth / 2) - (page.Width / 2);
            trans.Y = (ClassLibrary.BrowserScreenInformation.ClientHeight / 2) - (page.Height / 2);
            grd.RenderTransform = group; 
        }
        public static void ParseSETCards(string data)
        {
            int lbar = 0;
            int rbar = 0;
            int linenum = 1;
            string immediatetext = null;
            string[] lines = data.Split('\n');
          

            CardStats.ClearAndAdd();
            DuelPortalCS.SQLReference.CardDetails statBuild; 
            do
            {
                statBuild =  new DuelPortalCS.SQLReference.CardDetails();
                for (int n = 1; n <= 11; n++)
                {
                    rbar = lines[linenum - 1].IndexOf("|", lbar + 1);
                    if (rbar <= 1)
                    { break; }
                  
                    try
                    {
                        immediatetext = lines[linenum - 1].Substring(lbar + 1, rbar - lbar - 1);
                        lbar = rbar;
                    }
                    catch
                    {
                        break; 
                    }
                  
                    switch (n)
                    {
                        case 1:
                            statBuild.Name = immediatetext;
                            break;
                        case 2:
                            statBuild.ID = linenum;
                            break;
                        case 3:
                            statBuild.SpecialSet = immediatetext;
                            break;
                        case 4:
                            statBuild.Type = immediatetext;
                            break;
                        case 5:
                            statBuild.Attribute = immediatetext;
                            break;
                        case 6:
                            statBuild.Level = immediatetext.ToIntCountingQuestions();
                            break;
                        case 7:
                            if (!statBuild.IsMonster())
                                statBuild.Type = immediatetext;
                            break;
                        case 8:
                            statBuild.ATK = immediatetext.ToIntCountingQuestions();
                            break;
                        case 9:
                            statBuild.DEF = immediatetext.ToIntCountingQuestions();
                            break;
                        case 10:
                            statBuild.Lore = immediatetext;
                            break;
                        case 11:
                            statBuild.Flavor = immediatetext;
                            break;
                    }




                }

                statBuild.Limit = 3; 
                CardStats.Add(statBuild);
               
                lbar = 0;



                linenum += 1;

                if (linenum > lines.Length - 1) //(lbar == -1)
                    break; 
                
            } while (true);
           

        }
        public static void ParseBanlist(string banlist)
        {
            int currentLimit = 0;
            string[] lines = banlist.Split('\r');
            for (int n = 0; n < lines.Length; n++)
            {
                switch (lines[n].Trim())
                {
                    case "~Banned~":
                        currentLimit = 0;
                        continue;
                    case "~Limited~":
                        currentLimit = 1;
                        continue;
                    case "~Semi-Limited~":
                        currentLimit = 2;
                        continue;
                    default:
                        int id = Module1.findID(lines[n].Trim());
                        if (id > 0)
                           Module1.CardStats[id].Limit = currentLimit; 
                        break;
                }
            }
        }

        public static string NotDisplayEffect(this string str)
        {
            string functionReturnValue = str;
            try
            {
                if (str.Contains("Tuner") || str.Contains("Spirit") || str.Contains("Union") || str.Contains("Toon") || str.Contains("Gemini"))
                {
                    int place = str.IndexOf("/Effect");
                    if (place != -1) functionReturnValue = str.Substring(0, place);
                }
            }
            catch { }
            return functionReturnValue;

        }
        public static bool ContainsPoint(this Border bord, Point point)
        {
            Point bordPoint = new Point(bord.CLeft(), bord.CTop());
            return point.X >= bordPoint.X && point.X <= bordPoint.X + bord.ActualWidth &&
                   point.Y >= bordPoint.Y && point.Y <= bordPoint.Y + bord.ActualHeight;
        }
        public static bool StrToBool(this string str)
        {
            return str != "0";
        }
        public static bool containsAny(string baseString, string[] searches)
        {
            if (searches == null || searches.Length == 0) return false;
            foreach (string s in searches)
            {
                if (baseString.Contains(s))
                    return true;
            }
            return false;
        }
        public static bool containsAll(string baseString, string[] searches)
        {
            if (searches == null || searches.Length == 0) return false;
            foreach (string s in searches)
            {
                if (!baseString.Contains(s))
                    return false;
            }
            return true;
        }

        public static bool isOwner(int id)
        {

            string cardCreatorString = Module1.CardStats[id].Creator.Replace("_", "");
            if (cardCreatorString == "" || cardCreatorString == "PUB") return false;
            string ownerString = ""; int currentCharIndex = 1; //start after first bar
            while (cardCreatorString[currentCharIndex] != '|')
            {
                ownerString +=  cardCreatorString[currentCharIndex];
                currentCharIndex++;
            }
            if (ownerString == Module1.username)
            {
                return true;
            }
            else
                return false;
        }
        public static void setAsNothing(CardDetails stats)
        {
            stats.Name = null;
            
            stats.Type = null;
            stats.Attribute = null;
            stats.Facedown = false;
            stats.IsItHorizontal = false;
            stats.Lore = null;
            stats.ATK = 0;
            stats.DEF = 0;
            stats.Level = 0;

            stats.ID = 0;
            stats.SpecialSet = null;
            //stats.STicon = null;
            stats.Creator = null;
            stats.Counters = 0;
            stats.OpponentOwned = false;
        }
       
        public static void Shuffle()
        {
            Random rnd = new Random();
           List<CardDetails> shuffledDeck = new List<CardDetails>();
           shuffledDeck.Add(new CardDetails());
            int NumCardsInDeck = PlayerDeck.CountNumCards();
            bool[] doneArray = new bool[NumCardsInDeck + 1];
            int n;
            for (n = 1; n <= NumCardsInDeck; n++)
            {
                do
                {
                    int ranPosition = Rand(1, NumCardsInDeck, rnd);
                    if (doneArray[ranPosition] == false)
                    {
                        shuffledDeck.Add(PlayerDeck[ranPosition]);
                        doneArray[ranPosition] = true;
                        break; 
                    }
                } while (true);
            }

            PlayerDeck.ClearAndAdd();
            for (n = 1; n <= NumCardsInDeck; n++)
            {
                PlayerDeck.Add(shuffledDeck[n]);
            }
           
        }
 
   
        public class SocketClient
        {
           public const int PolicyPort = 943;
            public bool isConnected = false;
            private bool aborted = false;
            private System.Net.Sockets.Socket socket;
            private System.Net.DnsEndPoint endPoint;

            private AutoResetEvent autoEvent = new AutoResetEvent(false);
            private AutoResetEvent sendWait = new AutoResetEvent(false);
            private AutoResetEvent receiveWait = new AutoResetEvent(false);

            #region actions
            internal SocketClient(string host, int port)
            {
                endPoint = new DnsEndPoint(host, port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                aborted = false;
            }
            internal void ReceiveAbort()
            {
               aborted= true;
                receiveWait.Set();
                
            }
            internal string Connect() //returns error
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SocketClientAccessPolicyProtocol = SocketClientAccessPolicyProtocol.Tcp;
                args.UserToken = socket;
                args.RemoteEndPoint = endPoint;

                args.Completed += OnConnect;


                autoEvent.Reset();
                socket.ConnectAsync(args);
                autoEvent.WaitOne();

                if (args.SocketError == SocketError.Success)
                {
                return "";
                }
                else
                {
                    Exception p = args.ConnectByNameError;
                    return p.Message;
                }

            }

            internal void Disconnect()
            {
                if (isConnected)
                {
                   this.SendMessage("DISCONNECT", true);
                   socket.Close();
                    isConnected = false;
                }
            }

            private void ProcessError(SocketAsyncEventArgs e)
            {
                Socket s = e.UserToken as Socket;
                if (s.Connected)
                {
                    try
                    {
                        s.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (s.Connected)
                        {
                            s.Close();
                        }
                    }
                }

                throw new SocketException(Convert.ToInt32(e.SocketError));
            }

            internal void SendMessage(string message, bool noWait = false)
            {
                if (isConnected)
                {
                    sendWait.Reset();
                    Byte[] bytes = Encoding.UTF8.GetBytes(message);

                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.SetBuffer(bytes, 0, bytes.Length);
                    args.RemoteEndPoint = endPoint;

                    if (noWait)
                    {
                        socket.SendAsync(args);
                    }
                    else
                    {
                        
                        args.Completed += OnSend;
                        socket.SendAsync(args);

                        sendWait.WaitOne();
                    }
                }
                else
                {
                    throw new SocketException(Convert.ToInt32(SocketError.NotConnected));
                }
            }
            internal string ReceiveMessage()
            {
                if (isConnected)
                {
                    aborted = false;
                    receiveWait.Reset();
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                    byte[] empBuf = new byte[8193];
                    args.SetBuffer(empBuf, 0, 8192);

                    //args.UserToken = socket;
                    args.RemoteEndPoint = endPoint;
                    args.Completed += OnReceive;

                    socket.ReceiveAsync(args);
                    receiveWait.WaitOne();

                    if (aborted == false)
                        return Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    else
                        return "ABORT";
                }
                else
                {
                   
                    throw new SocketException(Convert.ToInt32(SocketError.NotConnected));
                }
            }
            #endregion
            #region "Events"
            private void OnConnect(object sender, SocketAsyncEventArgs e)
            {
                e.Completed -= OnConnect;
                autoEvent.Set();

                if (e.SocketError == SocketError.Success)
                    isConnected = true;
                else
                    isConnected = false;
            }
            private void OnReceive(object sender, SocketAsyncEventArgs e)
            {
                e.Completed -= OnReceive;
                receiveWait.Set();
            }
            private void OnSend(object sender, SocketAsyncEventArgs e)
            {
                e.Completed -= OnSend;
                sendWait.Set();
            }
            #endregion

        }


        public static List<SocketMessage> ModifiedDeserialize(string serializedMessage)
        {
            List<SocketMessage> sockMesList = new List<SocketMessage>();
            string[] SocketMessageArgs = new string[4];
            int LBar = -1;
            int RBar = 0;
            bool exitLoop = false;
            do
            {
             
                for (int n = 0; n <= 3; n++)
                {
                    RBar = serializedMessage.IndexOf( DOUBLE_BAR, LBar + 1);
                    if (RBar == -1)
                    {
                        exitLoop = true; break;
                    }
                    SocketMessageArgs[n] = serializedMessage.Substring(LBar + 1, RBar - LBar - 1);
                    LBar = RBar;
                }
                if (!exitLoop)
                {
                    try
                    {
                        SocketMessage sockMes = new SocketMessage();
                        sockMes.To = SocketMessageArgs[0];
                        sockMes.From = SocketMessageArgs[1];
                        sockMes.data = SocketMessageArgs[2];
                        sockMes.mType = (MessageType)Convert.ToByte(SocketMessageArgs[3]);
                        sockMesList.Add(sockMes);
                    }
                    catch { exitLoop = true; }
                }
            } while (!exitLoop);
            return sockMesList;
        }
        public static string socketSerialize(string toString, string fromString, string data, MessageType mType)
        {
            string[] baseString = {
			toString,
			fromString,
			data,
			Convert.ToString((byte)mType)
		};
            return string.Join(Convert.ToString(DOUBLE_BAR), baseString) + DOUBLE_BAR;
         
        }

        public static string formatYCM(int id, string picture)
        {
            return formatYCM(Module1.CardStats[id], picture);
        }
        public static string formatYCM(DuelPortalCS.SQLReference.CardDetails stats, string picture)
        {
            System.Text.StringBuilder newUrl = new System.Text.StringBuilder("http://www.yugiohcardmaker.net/ycmaker/createcard.php?");
            DuelPortalCS.SQLReference.CardDetails theCard = stats;
            if (theCard.Type == null) return "";
            //if (theCard.Type.Contains("Xyz")) return "xyz";

            newUrl.Append("name=" + theCard.Name + "&");

            if (theCard.Attribute == "Spell")
                newUrl.Append("cardtype=Spell&");
            else if (theCard.Attribute == "Trap")
                newUrl.Append("cardtype=Trap&");
            else if (theCard.Type.Contains("Synchro"))
                newUrl.Append("cardtype=Synchro&");
            else if (theCard.Type.Contains("Ritual"))
                newUrl.Append("cardtype=Ritual&");
            else if (theCard.Type.Contains("Fusion"))
                newUrl.Append("cardtype=Fusion&");
            else if (theCard.Type.Contains("Xyz"))
                newUrl.Append("cardtype=XYZ&");
            else
                newUrl.Append("cardtype=Monster&");

            if (theCard.Type.Contains("Gemini"))
                newUrl.Append("subtype=Gemini&");
            else if (theCard.Type.Contains("Spirit"))
                newUrl.Append("subtype=Spirit&");
            else if (theCard.Type.Contains("Toon"))
                newUrl.Append("subtype=Toon&");
            else if (theCard.Type.Contains("Union"))
                newUrl.Append("subtype=Union&");
            else if (theCard.Type.Contains("Tuner") && theCard.Type.Contains("Effect"))
                newUrl.Append("subtype=Tuner&");
            else if (!theCard.Type.Contains("Effect"))
                newUrl.Append("subtype=Normal&");
            else if (theCard.Attribute != "Spell" && theCard.Attribute != "Trap")
                newUrl.Append("subtype=Effect&");

            newUrl.Append("attribute=" + theCard.Attribute + "&");
            newUrl.Append("level=" + theCard.Level + "&");
            newUrl.Append("trapmagictype=" + theCard.Type + "&");

            newUrl.Append("rarity=Common&");

            newUrl.Append("picture=" + picture + "&");
            if (theCard.Type.Contains("Tuner") && !theCard.Type.Contains("Effect"))
                newUrl.Append("type=" + Module1.getFirstType(theCard.Type) + " / Tuner&");
            else
                newUrl.Append("type=" + Module1.getFirstType(theCard.Type) + "&");
            newUrl.Append("carddescription=" + theCard.Lore + "&");
            newUrl.Append("atk=" + theCard.ATK + "&");
            newUrl.Append("def=" + theCard.DEF + "&");

            return newUrl.ToString();
        }
}

    public enum MessageType : byte
    {
        Normal,
        AddTournamentHost,
        AddTraditionalHost,
        RemoveTournamentHost,
        RemoveTraditionalHost,
        AddDuel,
        RemoveDuel,
        Challenge,
        Accept,
        Reject,
        Join,
        Leave,
        LobbyEnter,
        DuelEnter,
        DuelMessage,
        DuelLeave
    }
       
    public class SocketMessage
    {
        public string To;
        public string From;
        public string data;

        public MessageType mType;
        public SocketMessage(string toWho, string fromWho, string mData, MessageType typ)
        {
            To = toWho;
            From = fromWho;
            data = mData;
            mType = typ;

        }

        public SocketMessage()
        {
        }
    }
    /// <summary>
    /// Extension class to do some extra operation with Silverlight ChildWindow.
    /// </summary>
    public static class ChildWindowExtensions
    {
       
        public static void DragChildWindow(this ChildWindow childWindow, Point p)
        {
            var root = VisualTreeHelper.GetChild(childWindow, 0) as FrameworkElement;
            if (root == null) { return; }

            var contentRoot = root.FindName("ContentRoot") as FrameworkElement;
            if (contentRoot == null) { return; }

            var group = contentRoot.RenderTransform as TransformGroup;
            if (group == null) { return; }

            TranslateTransform translateTransform = null;
            foreach (var transform in group.Children.OfType<TranslateTransform>())
            {
                translateTransform = transform;
            }

            if (translateTransform == null) { return; }

            // reset transform
            translateTransform.X += p.X;
            translateTransform.Y += p.Y;
        }
    }