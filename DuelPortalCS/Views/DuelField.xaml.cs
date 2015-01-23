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
using System.Windows.Navigation;
using System.Runtime.InteropServices;
namespace DuelPortalCS.Views
{
    public partial class DuelField : Page
    {
        public const string BLANK_IMAGE = "LightBG.png";
        public System.Windows.Threading.DispatcherTimer testTimer;
         public frmView viewForm;
         public bool isDraggingViewForm = false;
         public Point dragStartPoint;
        public int SelectedZone;
         public int stSelectedZone;
         public int handContextIndex;
        public int monContextIndex;
        public int stContextIndex;
        public int MonSelectedZone;
        public int xyzContextIndex;
        public bool DontFireChangeEventATK;
        public bool DontFireChangeEventDEF;
        public bool DontFireChangeEventCounters;
        public bool DontFireTurnCount;
        public bool ToChangeATK;
        public bool ToChangeDEF;
        public int ZoneofSwitchmonster = -1;
        public int ZoneofSwitchST = -1;
        public int TurnCount;
        public string WatcherCurrentlyPinging = Module1.WatcherMySide;
        public string watcherMySideSet;
        public string opponentSet;
        public System.Windows.Threading.DispatcherTimer PingTimer;
        //The Zone of the attacking monster
        public int goingToAttackZone = 0;
        //The Zone of the card to move. global.
        public int goingToMoveZone = 0;
        // Public ctxtDeck As New ContextMenu
        public TranslateTransform DeckTransform;
        // Public ctxtHand As New ContextMenu
        public TranslateTransform HandTransform;
        //Public ctxtMonster As New ContextMenu

        public TranslateTransform MonsterTransform;
        public TranslateTransform MonsterEmptyTransform;
        //Public ctxtSpellTrap As New ContextMenu
        public TranslateTransform SpellTrapTransform;
        public TranslateTransform XyzTransform;
        //public int messageReceiveNumber;
        public int pingTimerNumber = 0;
        private addMessageDelegate addMessageInvoke;
        public bool allowReconnect = true;
        //public List<SQLReference.CardDetails> RevealList = new List<SQLReference.CardDetails>();
        private const double imgHandTop = 495;
     
        //public List<Image> imgHand = new List<Image>();
        private RateUser withEventsField_RateForm = new RateUser();
        public RateUser RateForm
        {
            get { return withEventsField_RateForm; }
            set
            {
                if (withEventsField_RateForm != null)
                {
                    withEventsField_RateForm.Closed -= rateForm_Closed;
                }
                withEventsField_RateForm = value;
                if (withEventsField_RateForm != null)
                {
                    withEventsField_RateForm.Closed += rateForm_Closed;
                }
            }
        }
     
        // Public myCursorTransform As New TranslateTransform


public DuelField()
        {
           
            MouseLeftButtonUp += DuelField_MouseLeftButtonUp;
            KeyUp += DuelField_KeyUp;
 
            InitializeComponent();

         
        }


//Executes when the user navigates to this page.
protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Module1.CenterCanvas(LayoutRoot, this);
            Module1.ScreenResized += delegate
            {
                Module1.ScaleCanvas(LayoutRoot, this);
            };


            Module1.PlayerCurrentGrave.ClearAndAdd();
            Module1.PlayerCurrentRFG.ClearAndAdd();
            Module1.OpponentCurrentGrave.ClearAndAdd();
            Module1.OpponentCurrentRFG.ClearAndAdd();

            Module1.PlayerCurrentHand.ClearAndAdd();
            Module1.OpponentLP = 8000;
            Module1.PlayerLP = 8000;
            lblPlayerLP.Text = "LP: 8000";
            lblOpponentLP.Text = "LP: 8000";

            this.Cursor = Cursors.Arrow;
            Module1.currentForm = "DuelField";
            cmdShuffleHand.Content = "Shuffle" + Environment.NewLine + "Hand";
            cmdEndTurn.Content = "End" + Environment.NewLine + "Turn";
            cmdRefresh.Content = "Refresh" + Environment.NewLine + "Graphics";
            System.Windows.Browser.HtmlPage.Document.SetProperty("title", "Duel Portal Online");
            Module1.warnOnExitMessage = "Are you sure you want to quit this duel?";


          
              mesDelegate = new client_MessageReceivedDelegate(client_MessageReceived);
              sendDelegate = new sendMessageInvokeDelegate(sendMessageInvoke);
              timerDelegate = new startPingTimerDelegate(startPingTimer);
              addMessageInvoke = new addMessageDelegate(addMessage);

              Module1.animationQueue = new Queue<string>();
              Canvas.SetZIndex(imgMove, 10);
              Canvas.SetZIndex(imgMoveOrigin, 10);
            

            DeckTransform = new TranslateTransform();
            ctxtDeck.myArea = ContextMenu.Area.Deck;
            ctxtDeck.onLoaded();
            ctxtDeck.RenderTransform = DeckTransform;
            ctxtDeck.Visibility = System.Windows.Visibility.Collapsed;
            ctxtDeck.Item_Clicked += DeckContextItem_Clicked;

            HandTransform = new TranslateTransform();
            ctxtHand.myArea = ContextMenu.Area.Hand;
            ctxtHand.onLoaded();
            ctxtHand.RenderTransform = HandTransform;
            ctxtHand.Visibility = System.Windows.Visibility.Collapsed;
            ctxtHand.Item_Clicked += HandContextItem_Clicked;

            MonsterTransform = new TranslateTransform();
            ctxtMonster.myArea = ContextMenu.Area.Monster_Full;
            ctxtMonster.onLoaded();
            ctxtMonster.RenderTransform = MonsterTransform;
            ctxtMonster.Visibility = System.Windows.Visibility.Collapsed;
            ctxtMonster.Item_Clicked += MonsterContextItem_Clicked;
            Canvas.SetZIndex(ctxtMonster, 10);

            MonsterEmptyTransform = new TranslateTransform();
            ctxtMonsterEmpty.myArea = ContextMenu.Area.Monster_Empty;
            ctxtMonsterEmpty.onLoaded();
            ctxtMonsterEmpty.RenderTransform = MonsterEmptyTransform;
            ctxtMonsterEmpty.Visibility = System.Windows.Visibility.Collapsed;
            ctxtMonsterEmpty.Item_Clicked += MonsterEmptyContextItem_Clicked;
            Canvas.SetZIndex(ctxtMonsterEmpty, 10);

            SpellTrapTransform = new TranslateTransform();
            ctxtSpellTrap.myArea = ContextMenu.Area.ST;
            ctxtSpellTrap.onLoaded();
            ctxtSpellTrap.RenderTransform = SpellTrapTransform;
            ctxtSpellTrap.Visibility = System.Windows.Visibility.Collapsed;
            ctxtSpellTrap.Item_Clicked += SpellTrapContextItem_Clicked;

            XyzTransform = new TranslateTransform();
            ctxtXyz.myArea = ContextMenu.Area.Xyz;
            ctxtXyz.onLoaded();
            ctxtXyz.RenderTransform = XyzTransform;
            ctxtXyz.Visibility = System.Windows.Visibility.Collapsed;
            ctxtXyz.Item_Clicked += XyzContextItem_Clicked;
            Canvas.SetZIndex(ctxtXyz, 10);
            
            for (short n = 1; n <= 5; n++)
            {
         
                MonZone(n).RenderTransform = new CompositeTransform();
                MonZone(n).RenderTransform.SetValue(CompositeTransform.CenterXProperty, MonZone(n).Width / 2);
                MonZone(n).RenderTransform.SetValue(CompositeTransform.CenterYProperty, MonZone(n).Height / 2);
                MonZone(n).RenderTransform.SetValue(CompositeTransform.RotationProperty, 0.0);
                opMonZone(n).RenderTransform = new CompositeTransform();
                opMonZone(n).RenderTransform.SetValue(CompositeTransform.CenterXProperty, opMonZone(n).Width / 2);
                opMonZone(n).RenderTransform.SetValue(CompositeTransform.CenterYProperty, opMonZone(n).Height / 2);
                opMonZone(n).RenderTransform.SetValue(CompositeTransform.RotationProperty, 0.0);

                stZone(n).RenderTransform = new TranslateTransform();
                opstZone(n).RenderTransform = new TranslateTransform();

                MonZone(n).baseImage_Failed += new ExceptionRoutedEventHandler(anyImage_Failed);
                opMonZone(n).baseImage_Failed += new ExceptionRoutedEventHandler(anyImage_Failed);
                stZone(n).baseImage_Failed += new ExceptionRoutedEventHandler(anyImage_Failed);
                opstZone(n).baseImage_Failed += new ExceptionRoutedEventHandler(anyImage_Failed);

                MonZone(n).baseImage.Name = "BMonZone" + n.ToString();
                opMonZone(n).baseImage.Name = "BopMonZone" + n.ToString();
                stZone(n).baseImage.Name = "BstZone" + n.ToString();
                opstZone(n).baseImage.Name = "BopstZone" + n.ToString();

                Module1.PlayerCurrentMonsters[n] = new SQLReference.CardDetails();
                Module1.PlayerCurrentST[n] = new SQLReference.CardDetails();
                Module1.OpponentCurrentMonsters[n] = new SQLReference.CardDetails();
                Module1.OpponentCurrentST[n] = new SQLReference.CardDetails();
                Module1.PlayerOverlaid[n] = new List<SQLReference.CardDetails>(); Module1.PlayerOverlaid[n].ClearAndAdd();
                Module1.OpponentOverlaid[n] = new List<SQLReference.CardDetails>(); Module1.OpponentOverlaid[n].ClearAndAdd();
            }

            MonZone(1).animationTimer_Tick += (s, args) => { animationTimer_Tick(MonZone(1)); };
            MonZone(2).animationTimer_Tick += (s, args) => { animationTimer_Tick(MonZone(2)); };
            MonZone(3).animationTimer_Tick += (s, args) => { animationTimer_Tick(MonZone(3)); };
            MonZone(4).animationTimer_Tick += (s, args) => { animationTimer_Tick(MonZone(4)); };
            MonZone(5).animationTimer_Tick += (s, args) => { animationTimer_Tick(MonZone(5)); };
            opMonZone(1).animationTimer_Tick += (s, args) => { animationTimer_Tick(opMonZone(1)); };
            opMonZone(2).animationTimer_Tick += (s, args) => { animationTimer_Tick(opMonZone(2)); };
            opMonZone(3).animationTimer_Tick += (s, args) => { animationTimer_Tick(opMonZone(3)); };
            opMonZone(4).animationTimer_Tick += (s, args) => { animationTimer_Tick(opMonZone(4)); };
            opMonZone(5).animationTimer_Tick += (s, args) => { animationTimer_Tick(opMonZone(5)); };
            stZone(1).animationTimer_Tick += (s, args) => { animationTimer_Tick(stZone(1)); };
            stZone(2).animationTimer_Tick += (s, args) => { animationTimer_Tick(stZone(2)); };
            stZone(3).animationTimer_Tick += (s, args) => { animationTimer_Tick(stZone(3)); };
            stZone(4).animationTimer_Tick += (s, args) => { animationTimer_Tick(stZone(4)); };
            stZone(5).animationTimer_Tick += (s, args) => { animationTimer_Tick(stZone(5)); };
            opstZone(1).animationTimer_Tick += (s, args) => { animationTimer_Tick(opstZone(1)); };
            opstZone(2).animationTimer_Tick += (s, args) => { animationTimer_Tick(opstZone(2)); };
            opstZone(3).animationTimer_Tick += (s, args) => { animationTimer_Tick(opstZone(3)); };
            opstZone(4).animationTimer_Tick += (s, args) => { animationTimer_Tick(opstZone(4)); };
            opstZone(5).animationTimer_Tick += (s, args) => { animationTimer_Tick(opstZone(5)); };

            
            FieldSpellZone.baseImage_Failed += new ExceptionRoutedEventHandler(anyImage_Failed);
            opFieldSpellZone.baseImage_Failed += new ExceptionRoutedEventHandler(anyImage_Failed);
            FieldSpellZone.animationTimer_Tick += (s, args) => { animationTimer_Tick(FieldSpellZone); };
            opFieldSpellZone.animationTimer_Tick += (s, args) => { animationTimer_Tick(opFieldSpellZone); };

            FieldSpellZone.RenderTransform = new TranslateTransform();
            opFieldSpellZone.RenderTransform = new TranslateTransform();

            FieldSpellZone.baseImage.Name = "BFieldSpellZone";
            opFieldSpellZone.baseImage.Name = "BopFieldSpellZone";
 
            resetStats();
            updateOtherSideHovers();

            Module1.Shuffle();
        
            viewForm = new frmView();
            viewForm.Closed += viewForm_Closed;
            viewForm.MouseLeftButtonDown += viewForm_MouseLeftButtonDown;
            viewForm.MouseLeftButtonUp += viewForm_MouseLeftButtonUp;
            viewForm.MouseMove += viewForm_MouseMove;
           
          //  string dnsHost = Application.Current.Host.Source.DnsSafeHost;

                if (Module1.IamWatching == false && Module1.TagDuel == false)
                {
                    Module1.IamActive = true;
                }
                else if (Module1.IamWatching)
                {
                    lstMyHand.Visibility = System.Windows.Visibility.Collapsed;
                    cmdDrawFive.IsEnabled = false;
                    cmdDrawPhase.IsEnabled = false;
                    cmdStandbyPhase.IsEnabled = false;
                    cmdMainPhase1.IsEnabled = false;
                    cmdBattlePhase.IsEnabled = false;
                    cmdMainPhase2.IsEnabled = false;
                    cmdEndPhase.IsEnabled = false;
                    cmdEndTurn.IsEnabled = false;
                    cmdCoin.IsEnabled = false;
                    cmdDie.IsEnabled = false;
                    cmdGainLP.IsEnabled = false;
                    cmdLoseLP.IsEnabled = false;
                    cmdShuffleHand.IsEnabled = false;
                    //Module1.sock.SendMessage(Module1.socketSerialize("Server", Module1.username, myRoomID, MessageType.DuelEnter))

                }

                if (Module1.TagDuel == false)
                {
                    lstMyHand.Width = 240;
                    lblAlly.Visibility = System.Windows.Visibility.Collapsed;
                    lstAllyHand.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (Module1.noInternet) return;


                Module1.sock.SendMessage(Module1.socketSerialize("Server", Module1.username, Module1.myRoomID, MessageType.DuelEnter));
                startPingTimer();
        }

void viewForm_MouseMove(object sender, MouseEventArgs e)
{   
    if (isDraggingViewForm)
      dragViewForm(e.GetPosition(LayoutRoot));
}

void dragViewForm(Point p)
{
    Point toMove = new Point();
    toMove.X = p.X - dragStartPoint.X;
    toMove.Y = p.Y - dragStartPoint.Y;
    viewForm.DragChildWindow(toMove);
    dragStartPoint = p;
}

void viewForm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    isDraggingViewForm = false;
}

void viewForm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    dragStartPoint = e.GetPosition(LayoutRoot);
    isDraggingViewForm = true;
}


protected override void OnNavigatedFrom(NavigationEventArgs e)
{
    //if (Module1.sock != null)
    //{
    //    Module1.Module1.sock.Disconnect();
    //}
    Module1.sock.SendMessage(Module1.socketSerialize("Server", Module1.username, Module1.myRoomID, MessageType.DuelLeave));
    base.OnNavigatedFrom(e);
}
        private void rateForm_Closed(object sender, EventArgs e)
        {
            Module1.warnOnExitMessage = "";
            this.NavigationService.Navigate(new System.Uri("/Lobby", UriKind.Relative));
        }

        private static void MsgBox(string text){ MessageBox.Show(text);}

#region DataTransfer
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Area1">Hand, Monster, Over, ST, FSpell, Deck, Grave, RFG, Reveal</param>
        /// <param name="Index1"></param>
        /// <param name="Element1"></param>
        /// <param name="Area2"></param>
        /// <param name="Index2"></param>
        /// <param name="Element2"></param>
        /// <param name="Stat1">NHand, NDeck, NGrave, NRFG, LP</param>
        /// <param name="Stat2"></param>
        /// <param name="TheEvent"></param>
        /// <param name="Text"></param>
        /// <param name="TagSendToSingle"></param>
        /// <remarks></remarks>
public string SummarizeJabber( string Area1 = "", int Index1 = 0,  string Element1 = "",  
                               string Area2 = "", int Index2 = 0,  string Element2 = "",  
                               string Stat1 = "",  string Stat2 = "",  string TheEvent = "",  string Text = "",
                               bool TagSendToSingle = false,  bool returnOnly = false, Module1.animationBundle? bundle = null)
        {
            updateMySideHovers();
            string functionReturnValue = null;

            //string writeStr = "";
            System.Text.StringBuilder strBld = new System.Text.StringBuilder();
            int n = 0;

            //Areas: Hand, Monster, ST, Grave, Deck, EDeck, RFG, FSpell, Over

            #region Area1
            //Wants to summarize entire area
            if (!string.IsNullOrEmpty(Area1) && Index1 == 0)
            {
                strBld.Append("{" + Area1 + "}");
                switch (Area1)
                {
                    case "Hand":
                        for (n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentHand[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentHand[n].ID) + "|");
                            //strBld.Append(Module1.PlayerCurrentHand[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentHand[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");
                        }

                        break;
                    case "Monster":
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentMonsters[n].ID) + "|");
                            //strBld.Append(Module1.PlayerCurrentMonsters[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Level + "|");
                           // strBld.Append(Module1.PlayerCurrentMonsters[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].Facedown) + "|");
                        }

                        break;
                    case "FSpell":
                        strBld.Append(Module1.PlayerCurrentFSpell.Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentFSpell.ID) + "|");
                        //strBld.Append(Module1.PlayerCurrentFSpell.SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Type + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Level + "|");
                       // strBld.Append(Module1.PlayerCurrentFSpell.STicon + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.ATK + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.DEF + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Lore + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.Facedown) + "|");
                        break;
                    case "Over":
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(returnXyzSummarize(n));
                            //for (short m = 1; m <= 5; m++)
                            //{
                            //    strBld.Append(Module1.PlayerOverlaid[n, m] + "|");
                            //}
                        }

                        break;

               
                    case "ST":
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(Module1.PlayerCurrentST[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentST[n].ID) + "|");
                           // strBld.Append(Module1.PlayerCurrentST[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentST[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].Facedown) + "|");
                        }

                        break;
                    case "Deck":
                        for (n = 1; n <= Module1.PlayerCurrentDeck.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentDeck[n].ID) + "|");
                            //strBld.Append(Module1.PlayerCurrentDeck[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentDeck[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].Facedown) + "|");
                        }

                        break;
                    case "Grave":
                        for (n = 1; n <= Module1.PlayerCurrentGrave.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentGrave[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentGrave[n].ID) + "|");
                            //strBld.Append(Module1.PlayerCurrentGrave[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Level + "|");
                           // strBld.Append(Module1.PlayerCurrentGrave[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].Facedown) + "|");
                        }

                        break;
                    case "RFG":
                        for (n = 1; n <= Module1.PlayerCurrentRFG.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentRFG[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentRFG[n].ID) + "|");
                           // strBld.Append(Module1.PlayerCurrentRFG[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Level + "|");
                           // strBld.Append(Module1.PlayerCurrentRFG[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].Facedown) + "|");
                        }

                        break;
                    case "EDeck":
                        for (n = 1; n <= Module1.PlayerCurrentEDeck.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentEDeck[n].ID) + "|");
                            //strBld.Append(Module1.PlayerCurrentEDeck[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentEDeck[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].Facedown) + "|");
                        }

                        break;
                }
            }

            //Wants to summarize a specific card
            if (Index1 > 0 && string.IsNullOrEmpty(Element1))
            {
                strBld.Append( "{" + Area1 + "|" + Index1.ToString() + "}");
                n = Index1;
                switch (Area1)
                {
                    case "Hand":
                    case "Reveal":
                        strBld.Append(Module1.PlayerCurrentHand[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentHand[n].ID) + "|");
                        //strBld.Append(Module1.PlayerCurrentHand[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentHand[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");

                        break;
                    case "Monster":

                        strBld.Append(Module1.PlayerCurrentMonsters[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentMonsters[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentMonsters[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Level + "|");
                       // strBld.Append(Module1.PlayerCurrentMonsters[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].Facedown) + "|");

                        break;
                    case "Over":
                        strBld.Append(returnXyzSummarize(n));
                        //for (short m = 1; m <= 5; m++)
                        //{
                        //    strBld.Append( Module1.PlayerOverlaid[n, m] + "|");
                        //}


                        break;


                    case "FSpell":
                        strBld.Append(Module1.PlayerCurrentFSpell.Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentFSpell.ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentFSpell.SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Type + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Level + "|");
                      //  strBld.Append(Module1.PlayerCurrentFSpell.STicon + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.ATK + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.DEF + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Lore + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.Facedown) + "|");

                        break;
                    case "ST":

                        strBld.Append(Module1.PlayerCurrentST[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentST[n].ID) + "|");
                        //strBld.Append(Module1.PlayerCurrentST[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentST[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].Facedown) + "|");

                        break;
                    case "Deck":

                        strBld.Append(Module1.PlayerCurrentDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentDeck[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentDeck[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Level + "|");
                       // strBld.Append(Module1.PlayerCurrentDeck[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].Facedown) + "|");

                        break;
                    case "Grave":

                        strBld.Append(Module1.PlayerCurrentGrave[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentGrave[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentGrave[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentGrave[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].Facedown) + "|");

                        break;
                    case "RFG":

                        strBld.Append(Module1.PlayerCurrentRFG[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentRFG[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentRFG[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentRFG[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].Facedown) + "|");

                        break;
                    case "rmGrave": break;
                    case "rmRFG": break;
                    case "EDeck":

                        strBld.Append(Module1.PlayerCurrentEDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentEDeck[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentEDeck[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentEDeck[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].Facedown) + "|");

                        break;
                }

            }

            //Wants to summarize an element of a specific card
            if (Index1 > 0 && !string.IsNullOrEmpty(Element1))
            {
                strBld.Append("{" + Area1 + "|" + Index1.ToString() + "|" + Element1 + "}");
                n = Index1;
                switch (Area1)
                {
                    case "Hand":
                    case "Reveal":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "Monster":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentMonsters[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentMonsters[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentMonsters[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].Facedown) + "|");

                                break;
                        }

                        break;
                    case "ST":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentST[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentST[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentST[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "FSpell":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentFSpell.ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentFSpell.DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentFSpell.Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.Facedown) + "|");
                                break;
                        }
                        break;
                    case "Grave":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentGrave[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentGrave[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentGrave[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "RFG":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentRFG[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentRFG[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentRFG[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "Deck":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentDeck[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentDeck[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentDeck[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "EDeck":
                        switch (Element1)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentEDeck[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentEDeck[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentEDeck[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                }
            }

            #endregion
            #region Area2
            ///'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //Wants to summarize entire area
            if (!string.IsNullOrEmpty(Area2) && Index2 == 0)
            {
                 strBld.Append("{" + Area2 + "}");
                switch (Area2)
                {
                    case "Hand":
                        for (n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentHand[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentHand[n].ID) + "|");
                           // strBld.Append(Module1.PlayerCurrentHand[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentHand[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");
                        }

                        break;
                    case "Monster":
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentMonsters[n].ID) + "|");
                           // strBld.Append(Module1.PlayerCurrentMonsters[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentMonsters[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentMonsters[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].Facedown) + "|");
                        }

                        break;
                    case "FSpell":
                        strBld.Append(Module1.PlayerCurrentFSpell.Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentFSpell.ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentFSpell.SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Type + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Level + "|");
                        //strBld.Append(Module1.PlayerCurrentFSpell.STicon + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.ATK + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.DEF + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Lore + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.Facedown) + "|");

                        break;
                    case "Over":
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(returnXyzSummarize(n));
                            //for (short m = 1; m <= 5; m++)
                            //{
                            //     strBld.Append(Module1.PlayerOverlaid[n, m] + "|");
                            //}
                        }

                        break;
                    case "ST":
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(Module1.PlayerCurrentST[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentST[n].ID) + "|");
                          //  strBld.Append(Module1.PlayerCurrentST[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentST[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentST[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].Facedown) + "|");
                        }

                        break;
                    case "Deck":
                        for (n = 1; n <= Module1.PlayerCurrentDeck.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentDeck[n].ID) + "|");
                          //  strBld.Append(Module1.PlayerCurrentDeck[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentDeck[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].Facedown) + "|");
                        }

                        break;
                    case "Grave":
                        for (n = 1; n <= Module1.PlayerCurrentGrave.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentGrave[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentGrave[n].ID) + "|");
                          //  strBld.Append(Module1.PlayerCurrentGrave[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentGrave[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentGrave[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].Facedown) + "|");
                        }

                        break;
                    case "RFG":
                        for (n = 1; n <= Module1.PlayerCurrentRFG.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentRFG[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentRFG[n].ID) + "|");
                           // strBld.Append(Module1.PlayerCurrentRFG[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentRFG[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentRFG[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].Facedown) + "|");
                        }

                        break;
                    case "EDeck":
                        for (n = 1; n <= Module1.PlayerCurrentEDeck.CountNumCards(); n++)
                        {
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(Module1.PlayerCurrentEDeck[n].ID) + "|");
                           // strBld.Append(Module1.PlayerCurrentEDeck[n].SpecialSet + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Type + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Attribute + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Level + "|");
                            //strBld.Append(Module1.PlayerCurrentEDeck[n].STicon + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].ATK + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].DEF + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Lore + "|");
                            strBld.Append(Module1.PlayerCurrentEDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].Facedown) + "|");
                        }

                        break;
                }
            }

            //Wants to summarize a specific card
            if (Index2 > 0 && string.IsNullOrEmpty(Element2))
            {
                strBld.Append("{" + Area2 + "|" + Index2.ToString() + "}");
                n = Index2;
                switch (Area2)
                {
                    case "Hand":
                    case "Reveal":

                        strBld.Append(Module1.PlayerCurrentHand[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentHand[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentHand[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Level + "|");
                       // strBld.Append(Module1.PlayerCurrentHand[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");

                        break;
                    case "Monster":

                        strBld.Append(Module1.PlayerCurrentMonsters[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentMonsters[n].ID) + "|");
                      //  strBld.Append(Module1.PlayerCurrentMonsters[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Level + "|");
                       // strBld.Append(Module1.PlayerCurrentMonsters[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentMonsters[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].Facedown) + "|");

                        break;
                    case "Over":
                        strBld.Append(returnXyzSummarize(n));
                        //for (short m = 1; m <= 5; m++)
                        //{
                        //    strBld.Append(Module1.PlayerOverlaid[n, m] + "|");
                        //}


                        break;
                    case "FSpell":
                        strBld.Append(Module1.PlayerCurrentFSpell.Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentFSpell.ID) + "|");
                      //  strBld.Append(Module1.PlayerCurrentFSpell.SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Type + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Level + "|");
                        //strBld.Append(Module1.PlayerCurrentFSpell.STicon + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.ATK + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.DEF + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Lore + "|");
                        strBld.Append(Module1.PlayerCurrentFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.Facedown) + "|");

                        break;

                    case "ST":

                        strBld.Append(Module1.PlayerCurrentST[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentST[n].ID) + "|");
                      //  strBld.Append(Module1.PlayerCurrentST[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Level + "|");
                       // strBld.Append(Module1.PlayerCurrentST[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentST[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].Facedown) + "|");

                        break;
                    case "Deck":

                        strBld.Append(Module1.PlayerCurrentDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentDeck[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentDeck[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentDeck[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].Facedown) + "|");

                        break;
                    case "Grave":

                        strBld.Append(Module1.PlayerCurrentGrave[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentGrave[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentGrave[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentGrave[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentGrave[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].Facedown) + "|");

                        break;
                    case "RFG":

                        strBld.Append(Module1.PlayerCurrentRFG[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentRFG[n].ID) + "|");
                       // strBld.Append(Module1.PlayerCurrentRFG[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Level + "|");
                        //strBld.Append(Module1.PlayerCurrentRFG[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentRFG[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].Facedown) + "|");

                        break;
                    case "EDeck":

                        strBld.Append(Module1.PlayerCurrentEDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(Module1.PlayerCurrentEDeck[n].ID) + "|");
                      //  strBld.Append(Module1.PlayerCurrentEDeck[n].SpecialSet + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Type + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Attribute + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Level + "|");
                       // strBld.Append(Module1.PlayerCurrentEDeck[n].STicon + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].ATK + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].DEF + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Lore + "|");
                        strBld.Append(Module1.PlayerCurrentEDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].Facedown) + "|");

                        break;
                }

            }

            //Wants to summarize an element of a specific card
            if (Index2 > 0 && !string.IsNullOrEmpty(Element2))
            {
                 strBld.Append("{" + Area2 + "|" + Index2.ToString() + "|" + Element2 + "}");
                n = Index2;
                switch (Area2)
                {
                    case "Hand":
                    case "Reveal":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "Monster":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentMonsters[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentMonsters[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentMonsters[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentMonsters[n].Facedown) + "|");

                                break;
                        }
                        break;
                    case "ST":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentST[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentST[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentST[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentST[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "FSpell":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentFSpell.ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentFSpell.DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentFSpell.Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentFSpell.Facedown) + "|");
                                break;
                        }
                        break;
                    case "Grave":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentGrave[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentGrave[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentGrave[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentGrave[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "RFG":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentRFG[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentRFG[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentRFG[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentRFG[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "Deck":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentDeck[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentDeck[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentDeck[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case "EDeck":
                        switch (Element2)
                        {
                            case "ATK":
                                strBld.Append(Module1.PlayerCurrentEDeck[n].ATK + "|");
                                break;
                            case "DEF":
                                strBld.Append(Module1.PlayerCurrentEDeck[n].DEF + "|");
                                break;
                            case "Counters":
                                strBld.Append(Module1.PlayerCurrentEDeck[n].Counters.ToString() + "|");
                                break;
                            case "IsItHorizontal":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].IsItHorizontal) + "|");
                                break;
                            case "Facedown":
                                strBld.Append(Convert.ToByte(Module1.PlayerCurrentEDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                }
            }
            #endregion
            #region Stats
            ///'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //Stats: NHand, NDeck, NEDeck, NGrave, NRFG, LP (Stands for "Number")
            
            if (!string.IsNullOrEmpty(Stat1))
            {
                 strBld.Append("{" + Stat1 + "}");
                switch (Stat1)
                {
                    case "NHand":
                        strBld.Append(Module1.PlayerCurrentHand.CountNumCards() + "|");
                        break;
                    case "NDeck":
                        strBld.Append(Module1.PlayerCurrentDeck.CountNumCards() + "|");
                        break;
                    case "NEDeck":
                        strBld.Append(Module1.PlayerCurrentEDeck.CountNumCards() + "|");
                        break;
                    case "NGrave":
                        strBld.Append(Module1.PlayerCurrentGrave.CountNumCards() + "|");
                        break;
                    case "NRFG":
                        strBld.Append(Module1.PlayerCurrentRFG.CountNumCards() + "|");
                        break;
                    case "LP":
                        strBld.Append(Module1.PlayerLP + "|");
                        break;
                }
            }
            if (!string.IsNullOrEmpty(Stat2))
            {
                strBld.Append("{" + Stat2 + "}");
                switch (Stat2)
                {
                    case "NHand":
                        strBld.Append(Module1.PlayerCurrentHand.CountNumCards() + "|");
                        break;
                    case "NDeck":
                        strBld.Append(Module1.PlayerCurrentDeck.CountNumCards() + "|");
                        break;
                    case "NEDeck":
                        strBld.Append(Module1.PlayerCurrentEDeck.CountNumCards() + "|");
                        break;
                    case "NGrave":
                        strBld.Append(Module1.PlayerCurrentGrave.CountNumCards() + "|");
                        break;
                    case "NRFG":
                        strBld.Append(Module1.PlayerCurrentRFG.CountNumCards() + "|");
                        break;
                    case "LP":
                        strBld.Append(Module1.PlayerLP + "|");
                        break;
                }
            }

            ///'''
            if (Module1.TagDuel && Module1.IamActive)
            {
                if (Stat1 == "NHand" || Stat2 == "NHand")
                {
                     strBld.Append(AppendTagDuelData());
                }
            }
            ///'''

            #endregion

            #region EventSummary

            string miscstring = null;
            if (!string.IsNullOrEmpty(TheEvent))
            {
                miscstring = "";

                bool nowayjose = false;
                switch (TheEvent)
                {
                    case "Opponent Surrendered":
                        miscstring = "Surrendered";
                        break;
                    case "tst-cnnctn":
                        miscstring = TheEvent;
                        break;
                    case "trgt_crd":
                        miscstring = "trgt_crd";
                        break;
                    case "png-sccssfl":
                        miscstring = TheEvent;
                        break;
                    case "hnd-shffl":
                        miscstring = TheEvent;
                        break;
                    case "Opponent Pool":
                        miscstring = "Opponent Pool~" + Text;
                        break;
                    case "Flipped a Coin":
                        miscstring = "Flipped a Coin, the result was " + Text;
                        break;
                    case "Rolled a Die":
                        miscstring = "Rolled a Die, the result was " + Text;
                        break;
                    case "Draw Card":
                        miscstring = "drew a card";
                        break;
                    case "Activated Effect":
                        miscstring = "Activated the effect of " + "\"" + Text + "\"" + ".";
                        break;
                    case "Randomly sent card from Extra Deck to Grave":
                        miscstring = "randomly sent " + Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name + " from their Extra Deck to the Grave";
                        break;
                    case "Message":
                        miscstring = " : " + Text;
                        break;
                    case "Turn Count Changed":
                        miscstring = "Turn Count Changed~" + Text;
                        break;
                    case "Lost Lifepoints":
                        miscstring = "Lost " + Text + " Life Points.";
                        break;
                    case "Gained Lifepoints":
                        miscstring = "Gained " + Text + " Life Points.";
                        break;
                    case "Discarded":
                        miscstring = "Discarded " + "\"" + Text + "\"" + " from their hand.";
                        break;
                    case "Discarded at Random":
                        miscstring = "Discarded " + "\"" + Text + "\"" + " from their hand at random.";
                        break;
                    case "Discarded all":
                        miscstring = "Discarded all cards from their hand.";
                        break;
                    case "Shuffle":
                        miscstring = "Shuffled their deck.";
                        break;
                    case "Reset":
                        miscstring = "Reset.";
                        break;
                    case "Mill Top Card":
                        miscstring = "Sent " + "\"" + Text + "\"" + " from the top of their deck to the Graveyard.";
                        break;
                    case "Banish Top Card":
                        miscstring = "Banished " + "\"" + Text + "\"" + " from the top of their deck.";
                        break;
                    case "Set":
                        miscstring = "set a card from their hand.";
                        break;
                    case "Played":
                        miscstring = "Played " + "\"" + Text + "\"" + " from their hand.";
                        break;
                    case "Returned to Top of Deck":
                        miscstring = "Returned a card in their hand to the top of their deck.";
                        break;
                    case "Returned to Bottom of Deck":
                        miscstring = "Returned a card in their hand to the bottom of their deck.";
                        break;
                    case "Banished Card in Hand":
                        miscstring = "Banished " + "\"" + Text + "\"" + " from their hand.";
                        break;
                    case "Banished Card in Hand Facedown":
                        miscstring = "Banished a card Facedown from their hand.";
                        break;
                   case "Show Top Card":
                        miscstring = "Showed their top card on their deck: " + Text;
                        break;
                    case "Viewing Deck":
                        miscstring = "Is Viewing their deck";
                        break;
                    case "Viewing Top Cards":
                        miscstring = "Is Viewing the top " + Text + " cards of their deck.";
                        break;
                    case "Reveal":
                        miscstring = "Revealed a card in their hand : " + Text;
                        break;
                    case "Reveal All":
                        string p = "";
                        for (n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
                        {
                            p += Module1.PlayerCurrentHand[n].Name + ", ";
                        }

                        miscstring = "Revealed their entire hand :" + p;
                        break;
                    case "Moved Monster to ST Zone":
                        miscstring = "Moved " + Text + " to a S/T Zone.";
                        if (string.IsNullOrEmpty(Text))
                            miscstring = "Moved a Monster to a S/T Zone.";
                        break;
                    case "Moved ST to Monster Zone":
                        miscstring = "Moved " + Text + " to a Monster Zone.";
                        if (string.IsNullOrEmpty(Text))
                            miscstring = "Moved a S/T Card to a Monster Zone.";
                        break;
                    case "Moved Monster Zone":
                        miscstring = "Moved a monster to zone " + Text;
                        break;
                    case "Moved ST Zone":
                        miscstring = "Moved a Spell/Trap card to zone " + Text;
                        break;
                    case "Moved Monster to Field Spell Zone":
                        miscstring = "Moved " + Text + " to the Field Spell Zone.";
                        if (string.IsNullOrEmpty(Text))
                            miscstring = "Moved a Monster to the Field Spell Zone.";
                        break;
                    case "Moved ST to Field Spell Zone":
                        miscstring = "Moved " + Text + " to the Field Spell Zone.";
                        if (string.IsNullOrEmpty(Text))
                            miscstring = "Moved a Spell/Trap to the Field Spell Zone.";
                        break;
                    case "Moved Field to ST Zone":
                        miscstring = "Moved " + Text + " to a Spell/Trap Zone.";
                        if (string.IsNullOrEmpty(Text))
                            miscstring = "Moved a Field Spell to the Spell/Trap Zone.";
                        break;
                    case "Flipped Monster Face Down":
                        miscstring = "Flipped a monster face-down.";
                        break;
                    case "Flipped ST Face Down":
                        miscstring = "Flipped a Spell/Trap card they controlled Face-Down";
                        break;
                    case "Flipped Monster Face Up":
                        miscstring = "Flipped a monster face-up.";
                        break;
                    case "Flipped ST Face Up":
                        miscstring = "Flipped a Spell/Trap card they controlled Face-Up";
                        break;
                    case "Changed ATK":
                        miscstring = "Changed ATK of their monster " + "\"" + Text + "\"";
                        break;
                    case "Changed DEF":
                        miscstring = "Changed DEF of their monster " + "\"" + Text + "\"";
                        break;
                    case "Activated ST":
                        miscstring = "Activated their Spell/Trap card " + "\"" + Text + "\"";
                        break;
                    case "Sent ST to Grave":
                        miscstring = "Sent their Spell/Trap card " + "\"" + Text + "\"" + " to the Graveyard.";
                        break;
                    case "Banished Monster":
                        miscstring = "Banished their monster " + "\"" + Text + "\"" + " from the field.";
                        break;
                    case "Banished ST":
                        miscstring = "Banished their Spell/Trap card " + "\"" + Text + "\"" + " from the field.";
                        break;
                    case "Banished Field Spell":
                        miscstring = "Banished their Field Spell card " + "\"" + Text + "\"" + " from the field.";
                        break;
                    case "Returned":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from the field to their hand.";
                        break;
                    case "Spin":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from the field to the top of their deck.";
                        break;
                    case "To Bottom":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from the field to the bottom of their deck.";
                        break;
                    case "Switched Position":
                        miscstring = "Switched the Battle position of " + "\"" + Text + "\"";
                        break;
                    case "Summon Token":
                        miscstring = "Summoned a token.";
                        break;
                    case "Added Counter":
                        miscstring = "Added a counter to their card " + PlayerCurrentField(Convert.ToInt16(Text)).Name + " ( " + PlayerCurrentField(Convert.ToInt16(Text)).Counters.ToString() + " counters)";
                        break;
                    case "Removed Counter":
                        miscstring = "Removed a counter from their card " + PlayerCurrentField(Convert.ToInt16(Text)).Name + " ( " + PlayerCurrentField(Convert.ToInt16(Text)).Counters.ToString() + " counters)";
                        break;
                    case "Changed Counters":
                        miscstring = "Changed the number of counters on their card " + PlayerCurrentField(Convert.ToInt16(Text)).Name + " ( " + PlayerCurrentField(Convert.ToInt16(Text)).Counters.ToString() + " counters)";
                        break;
                    case "Give Control":
                        miscstring = "Give Control~" + Text;
                        break;
                    case "Give Control ST":
                        miscstring = "Give Control ST~" + Text;
                        break;
                    case "Give Back Control":
                        miscstring = "Give Back Control~" + Text;
                        break;
                    case "Give Back Control ST":
                        miscstring = "Give Back Control ST~" + Text;
                        break;
                    case "Confirm Control":
                        miscstring = "Confirm Control~" + Text;
                        break;
                    case "Confirm Control ST":
                        miscstring = "Confirm Control ST~" + Text;
                        break;
                    case "Confirm Give Back Control":
                        miscstring = "Confirm Give Back Control~" + Text;
                        break;
                    case "Confirm Give Back Control ST":
                        miscstring = "Confirm Give Back Control ST~" + Text;

                        break;
                    // ZoneofSwitchST = CShort(Text)
                    //Case "Agree to Control Change"
                  
                    //Case "Agree to ST Control Change"
                  
                    //Case "Disagree to Control Change"
                   
                    case "Returned to Extra Deck from Field":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from the Field to their Extra Deck.";
                        break;
                    case "Returned to Extra Deck from Graveyard":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their Graveyard to their Extra Deck.";
                        break;
                    case "Returned to Extra Deck from RFG":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their RFG to their Extra Deck.";
                        break;
                    case "Returned to Extra Deck from RFG Facedown":
                        miscstring = "Returned a facedown card from their RFG to their Extra Deck.";
                        break;
                    case "Attacking":
                        n = (short)Text.IndexOf( "|");
                        string mystr = Text.Substring( n + 1, 1);
                        string str = Text.Substring( 0, Text.Length - 2);
                        if (Module1.OpponentCurrentMonsters[Convert.ToInt16(str)].Facedown == false)
                        {
                            miscstring = "Is attacking " + Module1.OpponentCurrentMonsters[Convert.ToInt16(str)].Name + " (zone " + str + ") with " + Module1.PlayerCurrentMonsters[Convert.ToInt16(mystr)].Name + " (zone " + mystr + ")";
                        }
                        else
                        {
                            miscstring = "Is attacking a Face-Down monster (zone " + str + ") with " + Module1.PlayerCurrentMonsters[Convert.ToInt16(mystr)].Name + " (zone " + MonSelectedZone.ToString() + ")";
                        }
                        break;
                    case "Attacking Directly":
                        miscstring = "Is attacking directly with " + "\"" + Module1.PlayerCurrentMonsters[Convert.ToInt16(Text)].Name + "\"";

                        break;


                    case "Destroyed Monster":
                        miscstring = "Destroyed their Monster, " + "\"" + Text + "\"";
                        break;
                    case "Destroyed ST":
                        miscstring = "Destroyed their Spell/Trap Card, " + "\"" + Text + "\"";
                        break;
                    case "Destroyed FSpell":
                        miscstring = "Destroyed their Field Spell Card, " + "\"" + Text + "\"";

                        break;
                    case "Attach Material":
                        miscstring = "Attached Xyz material to their monster " + Text;
                        break;
                    case "Detatch Material":
                        miscstring = "Detatched Xyz material from their monster " + Text;
                        break;
                    ///'''''''''''''''''''''''''
                    case "Banished from Graveyard":
                        miscstring = "Banished " + "\"" + Text + "\"" + " from their Graveyard";
                        break;
                    case "Banished from Deck":
                        miscstring = "Banished " + "\"" + Text + "\"" + " from their Deck";
                        break;
                    case "Banished from Extra Deck":
                        miscstring = "Banished " + "\"" + Text + "\"" + " from their Extra Deck";
                        break;
                    case "Banished from Graveyard Facedown":
                        miscstring = "Banished a card Facedown from their Graveyard";
                        break;
                    case "Banished from Deck Facedown":
                        miscstring = "Banished a card Facedown from their Deck";
                        break;
                    case "Banished from Extra Deck Facedown":
                        miscstring = "Banished a card Facedown from their Extra Deck";
                        break;
                    case "Sent to Graveyard from RFG":
                        miscstring = "Sent " + "\"" + Text + "\"" + " to their Graveyard from their RFG.";
                        break;
                    case "Sent to Graveyard from Deck":
                        miscstring = "Sent " + "\"" + Text + "\"" + " to their Graveyard from their Deck.";
                        break;
                    case "Sent to Graveyard from Extra Deck":
                        miscstring = "Sent " + "\"" + Text + "\"" + " to their Graveyard from their Extra Deck.";
                        break;
                    case "Returned to Top of Deck from Deck":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their Deck to the top of their Deck.";
                        break;
                    case "Returned to Top of Deck from RFG":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their RFG to the top of their Deck.";
                        break;
                    case "Returned to Top of Deck from RFG Facedown":
                        miscstring = "Returned a facedown card from their RFG to the top of their Deck.";
                        break;
                    case "Returned to Top of Deck from Graveyard":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their Graveyard to the top of their Deck.";
                        break;
                    case "Returned to Bottom of Deck from Deck":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their Deck to the Bottom of their Deck.";
                        break;
                    case "Returned to Bottom of Deck from RFG":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their RFG to the Bottom of their Deck.";
                        break;
                    case "Returned to Bottom of Deck from RFG Facedown":
                        miscstring = "Returned a facedown card from their RFG to the Bottom of their Deck.";
                        break;
                    case "Returned to Bottom of Deck from Graveyard":
                        miscstring = "Returned " + "\"" + Text + "\"" + " from their Graveyard to the Bottom of their Deck.";
                        break;
                    case "Added to Hand from Deck":
                        AddFromDeckToHand((short)Module1.NumberOnList);
                        miscstring = "Added " + "\"" + Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name + "\"" + " from their Deck to their Hand.";
                        break;
                    case "Added to Hand from Graveyard":
                        miscstring = AddFromGraveToHand((short)Module1.NumberOnList);
                        break;
                    case "Added to Hand from RFG":
                        miscstring = AddFromRemovedFromPlayToHand((short)Module1.NumberOnList, false);
                        break;
                    case "Added to Hand from RFG Facedown":
                        miscstring = AddFromRemovedFromPlayToHand((short)Module1.NumberOnList, true);
                        break;
                    ///'''
                    case "Place on Field from RFG":
                        miscstring = "Placed " + "\"" + Module1.PlayerCurrentRFG[Module1.NumberOnList].Name + "\"" + " on the Field from their RFG.";
                        if (Module1.PlayerCurrentRFG[Module1.NumberOnList].IsMonster())
                        {
                            nowayjose = SSfromRFG((short)Module1.NumberOnList);
                        }
                        else
                        {
                            nowayjose = PlaceSTOnFieldFromRemovedFromPlay((short)Module1.NumberOnList);
                        }
                        if (nowayjose)
                            return functionReturnValue;
                        break;
                        
                    case "Place on Field from Graveyard":
                        miscstring = "Placed " + "\"" + Module1.PlayerCurrentGrave[Module1.NumberOnList].Name + "\"" + " on the Field from their Graveyard.";
                        if (Module1.PlayerCurrentGrave[Module1.NumberOnList].IsMonster())
                        {
                            nowayjose = SSfromgrave((short)Module1.NumberOnList);
                        }
                        else
                        {
                            nowayjose = PlaceSTOnFieldFromGraveyard((short)Module1.NumberOnList);
                        }
                        if (nowayjose)
                            return functionReturnValue;
                        break;
                    case "Place on Field from Deck":
                        miscstring = "Placed " + "\"" + Module1.PlayerCurrentDeck[Module1.NumberOnList].Name + "\"" + " on the Field from their Deck.";
                        if (Module1.PlayerCurrentDeck[Module1.NumberOnList].IsMonster())
                        {
                            nowayjose = SSfromDeck((short)Module1.NumberOnList);
                        }
                        else
                        {
                            nowayjose = PlaceSTOnFieldFromDeck((short)Module1.NumberOnList);
                        }
                        if (nowayjose)
                            return functionReturnValue;
                        break;
                    case "Place on Field from Extra Deck":
                        miscstring = "Placed " + "\"" + Module1.PlayerCurrentEDeck[Module1.NumberOnList].Name + "\"" + " on the Field from their Extra Deck.";
                        nowayjose = NormalSynchroSummon((short)Module1.NumberOnList);
                        if (nowayjose)
                            return functionReturnValue;
                        break;
                    ///''''''''''''''''''''''''''''''''''''''
                    case "Draw Phase":
                        miscstring = "Entered their Draw Phase";
                        break;
                    case "Standby Phase":
                        miscstring = "Entered their Standby Phase";
                        break;
                    case "Main Phase 1":
                        miscstring = "Entered their Main Phase 1";
                        break;
                    case "Battle Phase":
                        miscstring = "Entered their Battle Phase";
                        break;
                    case "Main Phase 2":
                        miscstring = "Entered their Main Phase 2";
                        break;
                    case "End Phase":
                        miscstring = "Entered their End Phase";
                        break;
                    case "End Turn":
                        miscstring = "Ended their turn. It is now the other player's turn.";
                        TurnCount += 1;
                        DontFireTurnCount = true;
                        txtTurnCount.Text = TurnCount.ToString();
                       // DontFireTurnCount = false;
                        break;
                    case "Tag Out":
                        miscstring = "Tagged Out!";
                        break;
                    case "Active":
                        miscstring = "is the active player now.";
                        break;
                    case "I Am":
                        miscstring = "'s name is " + Text;
                        break;
                }

            #endregion

                string Mestring = "You " + miscstring;
                if (Module1.TagDuel && Mestring.Contains("~") )
                {
                    Mestring = miscstring;
                }
                Mestring = Mestring.Replace("Is attacking", "Are attacking");
                Mestring = Mestring.Replace("Is Viewing", "Are Viewing");
                string OppString = "";
                if (Module1.TagDuel && miscstring.Contains("~"))
                {
                    OppString = miscstring;
                }
                else if (miscstring == "tst-cnnctn" || miscstring == "png-sccssfl" || miscstring == "Opponent Pool" || miscstring == "hnd-shffl")
                {
                    OppString = miscstring;
                }
                else if (!string.IsNullOrEmpty(miscstring) && miscstring.Contains("~") == false)
                {
                    addMessage(Mestring.Replace("their", "your"));
                    OppString = "Opponent " + miscstring;
                }
                else if (!string.IsNullOrEmpty(miscstring) && miscstring.Contains("~") && miscstring.Contains("Turn Count Changed"))
                {
                    addMessage("The turn count was changed to " + TurnCount.ToString() + ".");
                    OppString = miscstring;
                }
                else
                {
                    OppString = miscstring;
                }

                strBld.Append("{" + "msg" + "}");
                strBld.Append(OppString);
            }

            #region animation

            if (bundle != null)
            {
                strBld.Append("{ani}" +
                   ((int)bundle.Value.target).ToString() + "|" +
                   ((int)bundle.Value.fromArea).ToString() + "|" +
                   ((int)bundle.Value.fromIndex).ToString() + "|" +
                   ((int)bundle.Value.fromIndexXyz).ToString() + "|" +
                   ((int)bundle.Value.toArea).ToString() + "|" +
                   ((int)bundle.Value.toIndex).ToString() + "|" +
                   ((int)bundle.Value.toIndexXyz).ToString() + "|" +
                   ((int)bundle.Value.targetIndex).ToString() + "|" +
                   ((int)bundle.Value.targetIndexXyz).ToString());

            }

            #endregion
            //   Dim t As New Thread(AddressOf SendJabber)
            //    t.Start(writeStr)
            if (returnOnly)
            {
                return strBld.ToString();
            }
            else
            {
                SendJabber(strBld.ToString(), TagSendToSingle);
                return "";
            }
            


        }
private static string combineMultipleJabbers(List<string> messages)
        {
            while (messages.Contains(""))
            {
                messages.Remove("");
            }



            return string.Join("_", messages.ToArray());

        }
public static string AppendTagDuelData()
        {
            short n = 0;
            System.Text.StringBuilder strBld = new System.Text.StringBuilder();
            strBld.Append("{" + "Hand" + "}");

            for (n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
            {
           
                 strBld.Append(Module1.PlayerCurrentHand[n].Name + "|");
                strBld.Append(Convert.ToString(Module1.PlayerCurrentHand[n].ID) + "|");
               //  strBld.Append(Module1.PlayerCurrentHand[n].SpecialSet + "|");
                strBld.Append(Module1.PlayerCurrentHand[n].Type + "|");
                strBld.Append(Module1.PlayerCurrentHand[n].Attribute + "|");
                strBld.Append(Module1.PlayerCurrentHand[n].Level + "|");
                 //strBld.Append(Module1.PlayerCurrentHand[n].STicon + "|");
                strBld.Append(Module1.PlayerCurrentHand[n].ATK + "|");
                 strBld.Append(Module1.PlayerCurrentHand[n].DEF + "|");
                 strBld.Append(Module1.PlayerCurrentHand[n].Lore + "|");
                strBld.Append(Module1.PlayerCurrentHand[n].Counters.ToString() + "|");
                 strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].IsItHorizontal) + "|");
                 strBld.Append(Convert.ToByte(Module1.PlayerCurrentHand[n].Facedown) + "|");
            }
            return strBld.ToString();
        }
private static string returnXyzSummarize(int n)
{
    System.Text.StringBuilder strBld = new System.Text.StringBuilder();

    for (short m = 1; m <= Module1.PlayerOverlaid[n].CountNumCards(); m++)
    {
        strBld.Append(Module1.PlayerOverlaid[n][m].Name + "|");
        strBld.Append(Convert.ToString(Module1.PlayerOverlaid[n][m].ID) + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].Type + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].Attribute + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].Level + "|");
        //strBld.Append(Module1.PlayerOverlaid[n][m].STicon + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].ATK + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].DEF + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].Lore + "|");
        strBld.Append(Module1.PlayerOverlaid[n][m].Counters.ToString() + "|");
        strBld.Append(Convert.ToByte(Module1.PlayerOverlaid[n][m].IsItHorizontal) + "|");
        strBld.Append(Convert.ToByte(Module1.PlayerOverlaid[n][m].Facedown) + "|");
    }
    strBld.Append("-|");
    return strBld.ToString();

}
//public void connectThread()
//{

//    try
//    {
//        Module1.sock.Connect();

//        if (Module1.sock.isConnected)
//        {


//            initializingReceive = false;
//            string serializedDuelJoin = Module1.socketSerialize("Server", Module1.username, Module1.myRoomID, MessageType.DuelEnterNewClient);
//            Module1.sock.SendMessage(serializedDuelJoin);
//            receiveThread();
//        }
//    }
//    catch (Exception ex)
//    {
//        if (ex.InnerException != null)
//        {
//            MessageBox.Show(ex.Message + Environment.NewLine + ex.InnerException.ToString());
//        }
//        else
//        {
//            //     MessageBox.Show(ex.Message)
//        }
//    }
//}
//public void receiveThread()
//{

//    this.Dispatcher.BeginInvoke(timerDelegate);

//    while (Module1.sock.isConnected)
//    {

//        try
//        {
//            string serializedMessage = Module1.sock.ReceiveMessage();

//            if (serializedMessage == "" && allowReconnect == false && Module1.currentForm == "DuelField")
//            {
//                //string dnsHost = Application.Current.Host.Source.DnsSafeHost;

//                Module1.sock.Disconnect();
//                this.Dispatcher.BeginInvoke(addMessageInvoke, new object[1] { "Disconnected, attempting reconnect..." });
//               // sock = new Module1.SocketClient("192.227.234.101", 4530);
//                //sock = new Module1.SocketClient("localhost", 4530);
//                Module1.sock.Connect();
//                if (Module1.sock.isConnected)
//                {
//                    allowReconnect = true;
//                    this.Dispatcher.BeginInvoke(addMessageInvoke, new object[1] { "Reconnected, connecting to opponent..." });
//                    string serializedDuelJoin = Module1.socketSerialize("Server", Module1.username, Module1.myRoomID, MessageType.DuelEnterNewClient);
//                    Module1.sock.SendMessage(serializedDuelJoin);
//                    this.Dispatcher.BeginInvoke(timerDelegate);
//                }
//                else
//                    break;
//            }

//            if (serializedMessage != null)
//            {

//                this.Dispatcher.BeginInvoke(mesDelegate, new object[1] { serializedMessage });

//            }

//        }
//        catch (Exception)
//        {
//        }
//    }
//}

private void startPingTimer()
{
    if (PingTimer == null)
    {
        PingTimer = new System.Windows.Threading.DispatcherTimer();
        PingTimer.Interval = new TimeSpan(0, 0, 3);
        PingTimer.Tick += pingTimer_Tick;
        PingTimer.Start();
    }
    else { PingTimer.Stop(); PingTimer.Start(); }
}
public startPingTimerDelegate timerDelegate;
public delegate void startPingTimerDelegate();
private void pingTimer_Tick(object sender, EventArgs e)
{
    if (Module1.IamWatching)
    {
        if (!string.IsNullOrEmpty(WatcherCurrentlyPinging))
        {
            Module1.sock.SendMessage(Module1.socketSerialize(WatcherCurrentlyPinging, Module1.username, "tst-cnnctn", MessageType.DuelMessage));
        }
    }
    else
    {
        SummarizeJabber(TheEvent: "tst-cnnctn");
    }
    pingTimerNumber += 1;
    if (pingTimerNumber == 50)
    {
        PingTimer.Stop();
        PingTimer.Tick -= pingTimer_Tick;
        this.Dispatcher.BeginInvoke(addMessageInvoke, new object[1] { "We can't connect to opponent. They may have left the duel." });
    }
    // PingTimer.Stop();
}

public sendMessageInvokeDelegate sendDelegate;// = new sendMessageInvokeDelegate(sendMessageInvoke);
public delegate void sendMessageInvokeDelegate(string serializedMessage);
public void sendMessageInvoke(string serializedMessage)
{
    Module1.sock.SendMessage(serializedMessage);

}

public bool initializingReceive;

public client_MessageReceivedDelegate mesDelegate;// = new client_MessageReceivedDelegate(client_MessageReceived);
private delegate void addMessageDelegate(string message);
public void addMessage(string message)
{

    string[] splitm;
    splitm = new string[10];
    string newmessage = "";
    int element = 0;
    // int prevSpacePos = 0;
    int nextSpacePos = 0;
    while (message.Length > 75 && element < 10)
    {

        nextSpacePos = message.IndexOf(" ", 75);
        if (nextSpacePos == -1) { break; }
        splitm[element] = message.Substring(0, nextSpacePos).TrimStart();
        message = message.Substring(nextSpacePos, message.Length - nextSpacePos);
        element++;
        //prevSpacePos = nextSpacePos;
    }


    for (int n = 0; n <= element - 1; n++)
        newmessage += splitm[n] + Environment.NewLine;

    newmessage += message.TrimStart();

    while (lstMessages.Items.Count > 50)
        lstMessages.Items.RemoveAt(0);

    lstMessages.Items[lstMessages.Items.Count - 1] = newmessage;
    lstMessages.Items.Add("");
    //lstMessages.Items.Add(newmessage);
    lstMessages.UpdateLayout();
    lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]); //to blank


    //lstMessages.Items[lstMessages.Items.Count - 1] = message;
    //lstMessages.Items.Add("");
    //lstMessages.UpdateLayout();
    //lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);

}

private void SendJabber(string data, bool TagSendToSingle = false)
        {

            try
            {
                if (Module1.TagDuel)
                {
                    if (TagSendToSingle)
                    {
                        //JabberClient1.DuelMessageAsync(Opponent, Module1.username, data, myRoomID, "")
                        Module1.sock.SendMessage(Module1.socketSerialize(Module1.opponent, Module1.username, data, MessageType.DuelMessage));
                        //Send to Everyone
                    }
                    else
                    {
                        //JabberClient1.DuelMessageAsync("All", Module1.username, data, myRoomID, "")
                        Module1.sock.SendMessage(Module1.socketSerialize("All", Module1.username, data, MessageType.DuelMessage));
                    }
                }
                else if (Module1.IamWatching)
                {
                    //  JabberClient1.DuelMessageAsync("All", Module1.username, data, myRoomID, "")
                    Module1.sock.SendMessage(Module1.socketSerialize("All", Module1.username, data, MessageType.DuelMessage));
                }
                else
                {
                    //JabberClient1.DuelMessageAsync("All", Module1.username, data, myRoomID, "")
                    Module1.sock.SendMessage(Module1.socketSerialize("All", Module1.username, data, MessageType.DuelMessage));

                    //if (data.Contains("png-sccssfl") == false & data.Contains("tst-cnnctn") == false)
                    //    messagesStoredForWatcher.Add(data);
                }


            }
            catch
            {
            }

        }
public void DataArrivalJabber(string data, string FromWhom = "")
	{
   
        ZoneControl refer = null;
		short n = 0;
		string pieceOfData = null;
		short multipleJabberNumber = 0;
		short multipleJabberCount = 0;
		do {
			if (data.Contains("_")) {
				string[] dataArray = data.Split('_');
				multipleJabberCount = (short)dataArray.Count();
				pieceOfData = dataArray[multipleJabberNumber];
				multipleJabberNumber += 1;
			} else {
				pieceOfData = data;
			}
			Module1.ZoneInvolvement Involvement = Module1.ParceDataJabber(pieceOfData);
			//This takes care of ALOT
			string MiscString = "";
			int MiscCounter = 0;

           

			// OpponentBackgroundChange(0)
			for (n = 1; n <= 5; n++) {
				if ( (Involvement.Monsters[n] == true && Involvement.target == Module1.animationArea.None) ||
                     (Involvement.Xyz[n] == true            && Involvement.target == Module1.animationArea.None) ||// no animation
                     (Involvement.Monsters[n] == true && Involvement.toArea == Module1.animationArea.Hand)  )//exception for Bouncing
                {
					if (!string.IsNullOrEmpty(Module1.OpponentCurrentMonsters[n].Name)) {
                        
						refer = opMonZone(n); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.OpponentCurrentMonsters[n], opponentSet);
                        for (int k = Module1.OpponentOverlaid[n].CountNumCards(); k >= 1; k--)
                        {
                            ZoneControl opzImg = opimgXyz((short)n, (short)k);
                            if (opzImg == null)
                                addopXyzImg(n, (short)k);
                        }
                    }
                    else
                    { 
                         refer = opMonZone(n); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                         for (int k = 5; k >= 1; k--) //removes residue from move()
                         {
                             ZoneControl opzImg = opimgXyz((short)n, (short)k);
                             if (opzImg != null)
                                 removezImg(opzImg);
                         }

                    }
				}
                 


				if ((Involvement.ST[n] == true && Involvement.target == Module1.animationArea.None) ||  // no animation
                    (Involvement.ST[n] == true && Involvement.toArea == Module1.animationArea.Hand))     //exception for bouncing
                {
					//  And OpponentCurrentST[n].Facedown = False Then
					if (!string.IsNullOrEmpty(Module1.OpponentCurrentST[n].Name)) {
                        
                        refer = opstZone(n); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.OpponentCurrentST[n], opponentSet);
					} else {
						refer = opstZone(n); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
					}
				}
			}


			if ((Involvement.FSpell && Involvement.target == Module1.animationArea.None) || // no animation
                (Involvement.FSpell && Involvement.toArea == Module1.animationArea.Hand))     //exception for bouncing
            {
				if (!string.IsNullOrEmpty(Module1.OpponentCurrentFSpell.Name)) {
                  
                      UpdatePictureBoxDuelField(ref opFieldSpellZone.baseImage, opFieldSpellZone.Name, Module1.OpponentCurrentFSpell, opponentSet);
				} else {
					Module1.setImage(ref opFieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
				}
			}

            

            if (Involvement.Deck) {
                if (Module1.NumCardsInopDeck > 0)
                    Module1.setImage(ref opDeckZone, "back.jpg", UriKind.Relative);
                else
                    Module1.setImage(ref opDeckZone, BLANK_IMAGE, UriKind.Relative);
            }

           if (Involvement.Hand > 0)
            {
                resetopHandReveal();
                #region specialStuff
                short maxOpImgHand = 0;
                do{
                    ZoneControl zimg = opimgHand((short)(maxOpImgHand + 1));
                    if (zimg == null) { break; }
                    maxOpImgHand++;
                } while (true);

                short targetNumCards = (short)Module1.NumCardsInOpHand;
                bool moreThanTwoDiff = Math.Abs(targetNumCards - maxOpImgHand) > 1;

                if (maxOpImgHand != targetNumCards)
                {

                    while (maxOpImgHand < targetNumCards)
                    {
                        if (moreThanTwoDiff)
                            addopImgHand(true, maxOpImgHand + 1);
                        else
                            addopImgHand(true);

                       maxOpImgHand++; 
                    }
                }

                #endregion
            }
           if (Involvement.Reveal > 0)
           {
               ZoneControl zOpHand = opimgHand((short)Involvement.Reveal);
               UpdatePictureBoxDuelField(ref zOpHand.baseImage, zOpHand.Name, Module1.OpponentCurrentHand[Involvement.Reveal], opponentSet);
           }
           for (n = 1; n <= 5; n++)
           {
               if (Involvement.Xyz[n] == true)
               {
                   #region otherspecialstuff
                   short maxOpXyz = 0;
                   do
                   {
                       ZoneControl zimg = opimgXyz(n, (short)(maxOpXyz + 1));
                       if (zimg == null) { break; }
                       maxOpXyz++;
                   } while (true);

                   short targetNumCards = (short)Module1.OpponentOverlaid[n].CountNumCards();

                   if (maxOpXyz != targetNumCards)
                   {

                       while (maxOpXyz < targetNumCards)
                       {
                           maxOpXyz++;
                           addopXyzImg(n, maxOpXyz);

                       }
                   }
                   #endregion
               }
           }
            if (Involvement.target > 0)
            {
                SQLReference.CardDetails stat = null;
                switch (Involvement.toArea)
                {
                    case Module1.animationArea.Monster:
                        stat = Module1.OpponentCurrentMonsters[Involvement.toIndex];
                        break;
                    case Module1.animationArea.ST:
                        stat = Module1.OpponentCurrentST[Involvement.toIndex];
                        break;
                    case Module1.animationArea.FieldSpell:
                        stat = Module1.OpponentCurrentFSpell;
                        break;
                    case Module1.animationArea.Hand:
                       // stat = Module1.OpponentCurrentHand[Involvement.toIndex];
                        break;
                    case Module1.animationArea.Xyz:
                        //SOON
                        break;
                    case Module1.animationArea.Deck:

                        break;
                    case Module1.animationArea.Extra:

                        break;
                    case Module1.animationArea.Grave:
                        stat = Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()];
                        break;
                    case Module1.animationArea.RFG:
                        stat = Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()];
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false, "Bad target", Convert.ToString(Enum.GetName(typeof(Module1.animationArea), Involvement.target)));
                        break;
                }
                //handles removing of imgHand at endbehavior
                opponentAnimation(Involvement.target, Involvement.targetIndex, Involvement.targetIndexXyz,  stat,
                                  Involvement.fromArea, Involvement.fromIndex,
                                  Involvement.toArea, Involvement.toIndex,
                                  Involvement.fromIndexXyz, Involvement.toIndexXyz);

            }

            if (Involvement.Graveyard)
            {
                if (Module1.OpponentCurrentGrave.CountNumCards() > 0)
                {
                    UpdatePictureBoxDuelField(ref opGraveZone, opGraveZone.Name, Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()], opponentSet);
                }
                else
                {
                    Module1.setImage(ref opGraveZone, BLANK_IMAGE, UriKind.Relative);
                }
            }
           

            if (Involvement.RFG)
            {
                if (Module1.OpponentCurrentRFG.CountNumCards() > 0)
                {
                    UpdatePictureBoxDuelField(ref oprfgZone, oprfgZone.Name, Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()], opponentSet);
                }
                else
                {
                    Module1.setImage(ref oprfgZone, BLANK_IMAGE, UriKind.Relative);
                }
            }

            updateOtherSideHovers();

			lblOpponentLP.Text = "LP: " + Convert.ToString(Module1.OpponentLP);


			try {
				MiscCounter = Module1.OpRequestAction.IndexOf( "~");
                if (MiscCounter != -1)
                {
                    MiscString = Module1.OpRequestAction.Substring(MiscCounter + 1, Module1.OpRequestAction.Length - MiscCounter - 1);
                    Module1.OpRequestAction = Module1.OpRequestAction.Substring(0, MiscCounter);
                }

			} catch (Exception) {
			}
			if (data.Contains("Ended their turn. It is now the other player's turn.")) {
				TurnCount += 1;
				DontFireTurnCount = true;
				txtTurnCount.Text = TurnCount.ToString();
				//DontFireTurnCount = false;
			}

			//----------------------------------' 'Non - Numerical
            short zone = 0;
            short newZone = 0;
            short oppZone = 0;
            short newZoneST =0;
            short oppZoneST = 0;
			switch (Module1.OpRequestAction) {
                    
				//----------------------------------------------------------------------------------
                case "trgt_crd": 
				 zone = Convert.ToInt16(MiscString);
					break;
				//     PlayerBackgroundChange(zone)
				case "tst-cnnctn":
					SummarizeJabber(TheEvent:"png-sccssfl");
                   SummarizeJabber(TheEvent: "Opponent Pool", Text: Module1.mySet);
					break;
				case "png-sccssfl":
					if (Module1.TagDuel == false) {
						if (lstMessages.Items.Contains("Connection to Opponent OK") == false || allowReconnect) {
                            allowReconnect = false;
							addMessage("Connection to Opponent OK");
							PingTimer.Stop();
                            sendFieldToWatcher(Module1.opponent);
						}


					} else {
					}
					break;
                case "hnd-shffl":
                    opShuffleHand();
                    break;
                case "Opponent Pool":
                    opponentSet = MiscString;
                    break;
				case "Opponent Surrendered":

					MessageBoxResult answer = MessageBox.Show("Your Opponent has surrendered. You win!" + Environment.NewLine + Environment.NewLine + "Play again with this Opponent? (side deck will open)", "Play Again", MessageBoxButton.OKCancel);
					resetStats();
					//JabberClient1.LeaveDuelAsync(username, myRoomID)
					//messageTimer.Stop();
					if (answer == MessageBoxResult.OK) {
                     
                       // Module1.sock.Disconnect();
						Module1.sideDecking = true;
						Module1.warnOnExitMessage = "";
						this.NavigationService.Navigate(new System.Uri("/DeckEditor", UriKind.Relative));


					} else {
						RateForm.Show();
						// Me.NavigationService.Navigate(New System.Uri("/Lobby", UriKind.Relative))
					}

					return;


					
				case "Give Control":
					if (Module1.IamActive) {
						 newZone = FindEmptyMonZone();
						  oppZone = Convert.ToInt16(MiscString);
                            if (oppZone < 6){
                                Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[newZone], Module1.OpponentCurrentMonsters[oppZone]);
                            refer = MonZone(newZone);
                            UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[newZone], opponentSet);
                            addMessage(Module1.opponent + " gave control of their monster " + Module1.PlayerCurrentMonsters[newZone].Name + " to you");
							SummarizeJabber(Area1:"Monster", Index1:newZone, TheEvent:"Confirm Control", Text:MiscString);
                            Module1.PlayerCurrentMonsters[newZone].OpponentOwned = true;
						}
                         
                    }
                    break;

                      case "Give Control ST":
                   
							 newZoneST = FindEmptySTZone();
                             oppZoneST = Convert.ToInt16(MiscString);
                            if (oppZoneST < 6)
                            {
                                Module1.copyCardDetails(ref Module1.PlayerCurrentST[newZoneST] , Module1.OpponentCurrentST[oppZoneST]);
                                refer = stZone(newZoneST); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[newZoneST], opponentSet);
                                addMessage(Module1.opponent + " gave control of their Spell/Trap " + Module1.PlayerCurrentST[newZoneST].Name + " to you");
                                SummarizeJabber(Area1: "ST", Index1: newZoneST, TheEvent: "Confirm Control ST", Text: MiscString);
                                Module1.PlayerCurrentST[newZoneST].OpponentOwned = true;
                            }
                            else
                            {
                                Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell , Module1.OpponentCurrentFSpell);
                                UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, opponentSet);
                                addMessage(Module1.opponent + " gave control of their Field Spell " + Module1.PlayerCurrentFSpell.Name + " to you");
                                SummarizeJabber(Area1: "FSpell", TheEvent: "Confirm Control ST", Text: MiscString);
                                Module1.PlayerCurrentFSpell.OpponentOwned = true;
                            }
						
					break;

                      case "Give Back Control":
                    if (Module1.IamActive)
                    {
                         newZone = FindEmptyMonZone();
                         oppZone = Convert.ToInt16(MiscString);
                        if (oppZone < 6)
                        {
                           Module1.copyCardDetails(ref  Module1.PlayerCurrentMonsters[newZone] , Module1.OpponentCurrentMonsters[oppZone]);
                            refer = MonZone(newZone);
                            UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[newZone], opponentSet);
                            addMessage(Module1.opponent + " gave control of their monster " + Module1.PlayerCurrentMonsters[newZone].Name + " to you");
                            SummarizeJabber(Area1: "Monster", Index1: newZone, TheEvent: "Confirm Give Back Control", Text: MiscString);
                         
                        }
                    }
                    break;

                   case "Give Back Control ST":

                     newZoneST = FindEmptySTZone();
                     oppZoneST = Convert.ToInt16(MiscString);
                    if (oppZoneST < 6)
                    {
                        Module1.copyCardDetails(ref Module1.PlayerCurrentST[newZoneST] , Module1.OpponentCurrentST[oppZoneST]);
                        refer = stZone(newZoneST); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[newZoneST], opponentSet);
                        addMessage(Module1.opponent + " gave control of their Spell/Trap " + Module1.PlayerCurrentST[newZoneST].Name + " to you");
                        SummarizeJabber(Area1: "ST", Index1: newZoneST, TheEvent: "Confirm Give Back Control ST", Text: MiscString);

                    }
                    else
                    {
                        Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell , Module1.OpponentCurrentFSpell);
                        UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, opponentSet);
                        addMessage(Module1.opponent + " gave control of their Field Spell " + Module1.PlayerCurrentFSpell.Name + " to you");
                        SummarizeJabber(Area1: "FSpell", TheEvent: "Confirm Control ST", Text: MiscString);
                    }

                    break;


				case "Confirm Control":
					if (Module1.IamActive) {
						 zone = Convert.ToInt16(MiscString);
                         setAsNothing(Module1.PlayerCurrentMonsters[ZoneofSwitchmonster]);
                         refer = MonZone(ZoneofSwitchmonster); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                         SummarizeJabber(Area1: "Monster", Index1: ZoneofSwitchmonster);
                         Module1.OpponentCurrentMonsters[zone].OpponentOwned = true;
                         ZoneofSwitchmonster = -1;
					}
					break;
				case "Confirm Control ST":
					if (Module1.IamActive) {
					 zone = Convert.ToInt16(MiscString);
						//field spell
                     if (ZoneofSwitchST == 0)
                     {
							setAsNothing(Module1.PlayerCurrentFSpell);
							Module1.setImage(ref FieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
							SummarizeJabber(Area1:"FSpell");
                            Module1.OpponentCurrentFSpell.OpponentOwned = true;
                     }
                     else if (ZoneofSwitchST < 6)
                     {
                           setAsNothing(Module1.PlayerCurrentST[ZoneofSwitchST]);
						    refer = stZone(ZoneofSwitchST); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                            SummarizeJabber(Area1: "ST", Index1: ZoneofSwitchST);
                            Module1.OpponentCurrentST[zone].OpponentOwned = true;
						}
					}
                    ZoneofSwitchST = -1;
					break;

                case "Confirm Give Back Control":
                    if (Module1.IamActive)
                    {
                        zone = Convert.ToInt16(MiscString);
                        setAsNothing(Module1.PlayerCurrentMonsters[ZoneofSwitchmonster]);
                        refer = MonZone(ZoneofSwitchmonster); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                        SummarizeJabber(Area1: "Monster", Index1: ZoneofSwitchmonster);
                        
                    }
                    break;
                case "Confirm Give Back Control ST":
                    if (Module1.IamActive)
                    {
                        zone = Convert.ToInt16(MiscString);
                        //field spell
                        if (ZoneofSwitchST == 0)
                        {
                            setAsNothing(Module1.PlayerCurrentFSpell);
                            Module1.setImage(ref FieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
                            SummarizeJabber(Area1: "FSpell");
                        }
                        else if (ZoneofSwitchST < 6)
                        {
                           setAsNothing(Module1.PlayerCurrentST[ZoneofSwitchST]);
                            refer = stZone(ZoneofSwitchST); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                            SummarizeJabber(Area1: "ST", Index1: ZoneofSwitchST);
                        }
                    }

                    break;

				case "Turn Count Changed":
					if (Module1.IamActive) {
						TurnCount = Convert.ToInt16(MiscString);
                        DontFireTurnCount = true;
						txtTurnCount.Text = MiscString;
                       // DontFireTurnCount = false;
						addMessage("The turn count was changed to " + MiscString + ".");
					}
					break;
				default:
					if (Module1.TagDuel  && FromWhom != null)
						Module1.OpRequestAction = Module1.OpRequestAction.Replace("Opponent", FromWhom);

					if (!string.IsNullOrEmpty(Module1.OpRequestAction))
						addMessage(Module1.OpRequestAction);
					break;
			}


		} while (multipleJabberNumber < multipleJabberCount);
		//------------------------------------------------------------

		//If WarningsAnalyzed = False Then
		//WarningsAnalyzed = True
		//AnalyzeWarnings()
		//End If



	}   
public void WatchersArrival(string user, string data)
        {
        
            short n = 0;
            string pieceOfData = null;
            short multipleJabberNumber = 0;
            short multipleJabberCount = 0;
            ZoneControl refer = null;
            do
            {
                if (data.Contains("_"))
                {
                    string[] dataArray = data.Split('_');
                    multipleJabberCount = (short)dataArray.Count();
                    pieceOfData = dataArray[multipleJabberNumber];
                    multipleJabberNumber += 1;
                }
                else
                {
                    pieceOfData = data;
                }
                Module1.ZoneInvolvement involvement = Module1.setZoneInvolvement();
                #region "My Side"
                if (user == Module1.WatcherMySide)
                {
                    involvement = Module1.ParceDataJabber(pieceOfData, true);
                    //This takes care of ALOT

                    for (n = 1; n <= 5; n++)
                    {
                      if ( (involvement.Monsters[n] == true && involvement.target == Module1.animationArea.None) ||
                     (involvement.Xyz[n] == true           && involvement.target == Module1.animationArea.None) ||// no animation
                     (involvement.Monsters[n] == true && involvement.toArea == Module1.animationArea.Hand)  )//exception for Bouncing
                        
                          {
                            //And Module1.PlayerCurrentMonsters[n].Picture = "" Then
                            if (!string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[n].Name))
                            {
                                 refer = MonZone(n);
                                 UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[n], watcherMySideSet);
                            }
                            else
                            {
                                 refer = MonZone(n);
                                Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                             
                                for (int k = 5; k >= 1; k--) //removes residue from move()
                                {
                                   ZoneControl zImg = imgXyz((short)n, (short)k);
                                   if (zImg != null)
                                      removezImg(zImg);
                                }

                    
                            }
                        }
                      if ((involvement.ST[n]  && involvement.target == Module1.animationArea.None) ||
                          (involvement.ST[n]  && involvement.toArea == Module1.animationArea.Hand))
                        {
                            if (!string.IsNullOrEmpty(Module1.PlayerCurrentST[n].Name))
                            {
                                 refer = stZone(n);
                                 UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[n], watcherMySideSet);
                            }
                            else
                            {
                                 refer = stZone(n);
                                Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                            }
                        }
                    }

                    if ((involvement.FSpell && involvement.target == Module1.animationArea.None) || // no animation
                        (involvement.FSpell && involvement.toArea == Module1.animationArea.Hand))     //exception for bouncing
                    {
                        if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
                        {
                            UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, watcherMySideSet);
                        }
                        else
                        {
                            Module1.setImage(ref FieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
                        }
                    }

                    if (involvement.Graveyard)
                    {
                        if (Module1.PlayerCurrentGrave.CountNumCards() > 0)
                        {
                            UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], watcherMySideSet);
                        }
                        else
                        {
                            Module1.setImage(ref GraveZone, BLANK_IMAGE, UriKind.Relative);
                        }
                    }

                    if (involvement.RFG)
                    {
                        if (Module1.PlayerCurrentRFG.CountNumCards() > 0)
                        {
                            UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], watcherMySideSet);
                        }
                        else
                        {
                            Module1.setImage(ref rfgZone, BLANK_IMAGE, UriKind.Relative);
                        }
                    }

                    if (involvement.Hand > 0)
                    {
                        #region specialStuff
                        short maxImgHand = 0;
                        do
                        {
                            ZoneControl zimg = imgHand((short)(maxImgHand + 1));
                            if (zimg == null) { break; }
                            maxImgHand++;
                        } while (true);

                        short targetNumCards = (short)Module1.watcherNumCardsInHand;
                        bool moreThanTwoDiff = Math.Abs(targetNumCards - maxImgHand) > 1;
                        if (maxImgHand != targetNumCards)
                        {

                            while (maxImgHand < targetNumCards)
                            {
                                if (moreThanTwoDiff)
                                    watcherAddImgHand(false, maxImgHand + 1);
                                else
                                    watcherAddImgHand(false);
                                maxImgHand++;
                            }
                        }
                        
                        #endregion
                    }
                    for (n = 1; n <= 5; n++)
                    {
                        if (involvement.Xyz[n] == true)
                        {
                            #region otherspecialstuff
                            short maxXyz = 0;
                            do
                            {
                                ZoneControl zimg = imgXyz(n, (short)(maxXyz + 1));
                                if (zimg == null) { break; }
                                maxXyz++;
                            } while (true);

                            short targetNumCards = (short)Module1.PlayerOverlaid[n].CountNumCards();

                            if (maxXyz != targetNumCards)
                            {

                                while (maxXyz < targetNumCards)
                                {

                                    maxXyz++;
                                    addXyzImg(n, maxXyz);

                                }
                            }
                            #endregion
                        }
                    }
                    if (involvement.target > 0)
                    {
                        SQLReference.CardDetails stat = null;
                        switch (involvement.toArea)
                        {
                            case Module1.animationArea.Monster:
                                stat = Module1.PlayerCurrentMonsters[involvement.toIndex];
                                break;
                            case Module1.animationArea.ST:
                                stat = Module1.PlayerCurrentST[involvement.toIndex];
                                break;
                            case Module1.animationArea.FieldSpell:
                                stat = Module1.PlayerCurrentFSpell;
                                break;
                            case Module1.animationArea.Hand:
                                // stat = Module1.OpponentCurrentHand[Involvement.toIndex];
                                break;
                            case Module1.animationArea.Xyz:
                                //SOON
                                break;
                            case Module1.animationArea.Deck:

                                break;
                            case Module1.animationArea.Extra:

                                break;
                            case Module1.animationArea.Grave:
                                stat = Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()];
                                break;
                            case Module1.animationArea.RFG:
                                stat = Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()];
                                break;
                            default:
                                System.Diagnostics.Debug.Assert(false, "Bad target", Convert.ToString(Enum.GetName(typeof(Module1.animationArea), involvement.target)));
                                break;
                        }
                        //handles removing of imgHand at endbehavior
                        watcherAnimation(involvement.target, involvement.targetIndex, involvement.targetIndexXyz, stat,
                                          involvement.fromArea, involvement.fromIndex,
                                          involvement.toArea, involvement.toIndex,
                                          involvement.fromIndexXyz, involvement.toIndexXyz);

                    }

                    updateMySideHovers();
                    
                    lblPlayerLP.Text = "LP: " + Convert.ToString(Module1.PlayerLP);

                }
                #endregion
                #region "Other Side"
                else if (user == Module1.WatcherOtherSide)
                {
                    involvement = Module1.ParceDataJabber(data, false);
                    //This takes care of ALOT
                    for (n = 1; n <= 5; n++)
                    {
                        if ((involvement.Monsters[n] == true && involvement.target == Module1.animationArea.None) ||
                        (involvement.Xyz[n] == true && involvement.target == Module1.animationArea.None) ||// no animation
                        (involvement.Monsters[n] == true && involvement.toArea == Module1.animationArea.Hand))//exception for Bouncing
                        {
                            //And OpponentCurrentMonsters[n].Picture = "" Then
                            if (!string.IsNullOrEmpty(Module1.OpponentCurrentMonsters[n].Name))
                            {
                                refer = opMonZone(n); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.OpponentCurrentMonsters[n], opponentSet);
                            }
                            else
                            {
                                refer = opMonZone(n); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                                for (int k = 5; k >= 1; k--) //removes residue from move()
                                {
                                    ZoneControl opzImg = opimgXyz((short)n, (short)k);
                                    if (opzImg != null)
                                        removezImg(opzImg);
                                }
                            }
                        }
                        if ((involvement.ST[n] == true && involvement.target == Module1.animationArea.None) ||
                          (involvement.ST[n] == true && involvement.toArea == Module1.animationArea.Hand))
                        {
                            if (!string.IsNullOrEmpty(Module1.OpponentCurrentST[n].Name))
                            {
                                refer = opstZone(n); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.OpponentCurrentST[n], opponentSet);
                            }
                            else
                            {

                                refer = opstZone(n); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                            }
                        }
                    }

                    if ((involvement.FSpell && involvement.target == Module1.animationArea.None) || // no animation
                        (involvement.FSpell && involvement.toArea == Module1.animationArea.Hand))     //exception for bouncing
                    {
                        if (!string.IsNullOrEmpty(Module1.OpponentCurrentFSpell.Name))
                        {
                            UpdatePictureBoxDuelField(ref opFieldSpellZone.baseImage, opFieldSpellZone.Name, Module1.OpponentCurrentFSpell, opponentSet);
                        }
                        else
                        {
                            Module1.setImage(ref opFieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
                        }
                    }

                    if (involvement.Graveyard)
                    {
                        if (Module1.OpponentCurrentGrave.CountNumCards() > 0)
                        {
                            UpdatePictureBoxDuelField(ref opGraveZone, opGraveZone.Name, Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()], opponentSet);
                        }
                        else
                        {
                            Module1.setImage(ref opGraveZone, BLANK_IMAGE, UriKind.Relative);
                        }
                    }

                    if (involvement.RFG)
                    {
                        if (Module1.OpponentCurrentRFG.CountNumCards() > 0)
                        {
                            UpdatePictureBoxDuelField(ref oprfgZone, oprfgZone.Name, Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()], opponentSet);
                        }
                        else
                        {
                            Module1.setImage(ref oprfgZone, BLANK_IMAGE, UriKind.Relative);
                        }
                    }

                    if (involvement.Hand > 0)
                    {
                        #region specialStuff
                        short maxOpImgHand = 0;
                        do
                        {
                            ZoneControl zimg = opimgHand((short)(maxOpImgHand + 1));
                            if (zimg == null) { break; }
                            maxOpImgHand++;
                        } while (true);

                        short targetNumCards = (short)Module1.NumCardsInOpHand;
                        bool moreThanTwoDiff = Math.Abs(targetNumCards - maxOpImgHand) > 1;

                        if (maxOpImgHand != targetNumCards)
                        {

                            while (maxOpImgHand < targetNumCards)
                            {
                                if (moreThanTwoDiff)
                                    addopImgHand(false, maxOpImgHand + 1);
                                else
                                     addopImgHand(false);
                                maxOpImgHand++;
                            }
                        }

                        #endregion
                    }
                    for (n = 1; n <= 5; n++)
                    {
                        if (involvement.Xyz[n] == true)
                        {
                            #region otherspecialstuff
                            short maxXyz = 0;
                            do
                            {
                                ZoneControl zimg = opimgXyz(n, (short)(maxXyz + 1));
                                if (zimg == null) { break; }
                                maxXyz++;
                            } while (true);

                            short targetNumCards = (short)Module1.OpponentOverlaid[n].CountNumCards();

                            if (maxXyz != targetNumCards)
                            {

                                while (maxXyz < targetNumCards)
                                {

                                    maxXyz++;
                                    addopXyzImg(n, maxXyz);

                                }
                            }
                            #endregion
                        }
                    }
                    if (involvement.target > 0)
                    {
                        SQLReference.CardDetails stat = null;
                        switch (involvement.toArea)
                        {
                            case Module1.animationArea.Monster:
                                stat = Module1.OpponentCurrentMonsters[involvement.toIndex];
                                break;
                            case Module1.animationArea.ST:
                                stat = Module1.OpponentCurrentST[involvement.toIndex];
                                break;
                            case Module1.animationArea.FieldSpell:
                                stat = Module1.OpponentCurrentFSpell;
                                break;
                            case Module1.animationArea.Hand:
                                // stat = Module1.OpponentCurrentHand[Involvement.toIndex];
                                break;
                            case Module1.animationArea.Xyz:
                                //SOON
                                break;
                            case Module1.animationArea.Deck:

                                break;
                            case Module1.animationArea.Extra:

                                break;
                            case Module1.animationArea.Grave:
                                stat = Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()];
                                break;
                            case Module1.animationArea.RFG:
                                stat = Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()];
                                break;
                            default:
                                System.Diagnostics.Debug.Assert(false, "Bad target", Convert.ToString(Enum.GetName(typeof(Module1.animationArea), involvement.target)));
                                break;
                        }
                        //handles removing of imgHand at endbehavior
                        opponentAnimation(involvement.target, involvement.targetIndex, involvement.targetIndexXyz, stat,
                                          involvement.fromArea, involvement.fromIndex,
                                          involvement.toArea, involvement.toIndex,
                                          involvement.fromIndexXyz, involvement.toIndexXyz);

                    }

                    updateOtherSideHovers();

                    lblOpponentLP.Text = "LP: " + Module1.OpponentLP.ToString();


                }
                #endregion
                short misccounter = 0;
                string miscstring = null;
                try
                {
                    misccounter = (short)Module1.WatcherRequestAction.IndexOf("~");
                    if (misccounter != -1)
                    {
                    miscstring = Module1.WatcherRequestAction.Substring(misccounter + 1, Module1.WatcherRequestAction.Length - misccounter - 1);
                    Module1.WatcherRequestAction = Module1.WatcherRequestAction.Substring(0, misccounter);
                    }

                }
                catch (Exception)
                {
                }
                if (data.Contains("Ended their turn. It is now the other player's turn."))
                {
                    TurnCount += 1;
                    //DontFireTurnCount = true;
                    txtTurnCount.Text = TurnCount.ToString();
                   // DontFireTurnCount = false;
                }

                //----------------------------------' 'Non - Numerical

                switch (Module1.WatcherRequestAction)
                {

                    //----------------------------------------------------------------------------------
                    case "tst-cnnctn":

                        break;
                    case "png-sccssfl":

                        if (Module1.TagDuel == false)
                        {
                            if (WatcherCurrentlyPinging == Module1.WatcherMySide)
                            {
                                addMessage("Connection to " + WatcherCurrentlyPinging + " OK");
                                WatcherCurrentlyPinging = Module1.WatcherOtherSide;
                            }
                            else if (WatcherCurrentlyPinging == Module1.WatcherOtherSide)
                            {
                               addMessage("Connection to " + WatcherCurrentlyPinging + " OK");
                                WatcherCurrentlyPinging = "";
                                PingTimer.Stop();
                                PingTimer.Tick -= pingTimer_Tick;
                                Module1.sock.SendMessage(Module1.socketSerialize("Server", Module1.username, Module1.myRoomID, MessageType.DuelEnter));
                            }
                        }
                        break;

                    case "Opponent Pool":
                        if (user == Module1.WatcherMySide)
                            watcherMySideSet = miscstring;
                        if (user == Module1.WatcherOtherSide)
                            opponentSet = miscstring;
                        break;
                    case "Opponent Surrendered":

                        break;
                    case "Give Control":

                        break;
                    case "Give Control ST":

                        break;
                    case "Confirm Control":

                        break;
                    case "Confirm Control ST":

                        break;


                    case "Turn Count Changed":

                        break;

                    case "hnd-shffl":
                     
                        break;

                    default:
                        try
                        {
                            

                            if (!string.IsNullOrEmpty(Module1.WatcherRequestAction))
                                addMessage(Module1.WatcherRequestAction.Replace("Opponent", user));

                        }
                        catch
                        {
                        }
                        break;
                }
                Module1.WatcherRequestAction = "";
            } while (multipleJabberNumber < multipleJabberCount);
        }
public void FromWatcherArrival(string user, string data)
{
    if (data == "tst-cnnctn")
    {
        Module1.sock.SendMessage(Module1.socketSerialize(user, Module1.username, "{msg}png-sccssfl", MessageType.DuelMessage));
    }
    else
    {
        addMessage(user + ": " + data);
    }
}
public void sendFieldToWatcher(string watcherName)
{
    Module1.sock.SendMessage(Module1.socketSerialize(watcherName, Module1.username, SummarizeJabber(TheEvent: "Opponent Pool", Text: Module1.mySet, TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));
    Module1.sock.SendMessage(Module1.socketSerialize(watcherName, Module1.username, SummarizeJabber(Area1: "Monster", Area2: "ST", Stat1: "NGrave", Stat2: "NRFG", TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));
    Module1.sock.SendMessage(Module1.socketSerialize(watcherName, Module1.username, SummarizeJabber(Area1: "Grave", Area2: "RFG", Stat1: "LP", Stat2: "NHand", TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));
    Module1.sock.SendMessage(Module1.socketSerialize(watcherName, Module1.username, SummarizeJabber(Area1: "FSpell", Area2: "Over", Stat1: "NDeck", Stat2: "NEDeck", TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));

}
public delegate void client_MessageReceivedDelegate(string serializedMessage);
public void client_MessageReceived(string serializedMessage)
{
    List<SocketMessage> msges = Module1.ModifiedDeserialize(serializedMessage);
    foreach (SocketMessage msg in msges)
    {

        switch (msg.mType)
        {
            case MessageType.DuelMessage:
                if (Module1.IamWatching)
                {
                    if (msg.From == Module1.WatcherMySide || msg.From == Module1.WatcherOtherSide)
                    {
                        WatchersArrival(msg.From, msg.data);
                    }
                    else
                    {
                        FromWatcherArrival(msg.From, msg.data);
                    }

                }
                else
                {
                    if (msg.From == Module1.opponent)
                    {
                        DataArrivalJabber(msg.data, msg.From);
                    }
                    else
                    {
                        FromWatcherArrival(msg.From, msg.data);
                    }
                }
                break;
            // If amHost = True Then messagesStoredForWatcher.Add(msg.data)
            case MessageType.DuelEnter:

                if (msg.data != Module1.opponent && msg.data != Module1.username)
                {
                    addMessage(msg.data + " has joined the room (watcher)");
                    if (!Module1.IamWatching) sendFieldToWatcher(msg.data);
                }

                break;
            case MessageType.Leave:
                if (msg.data == Module1.opponent)
                    addMessage("Your opponent has left the game.");
                //   else
                //        addMessage(msg.data + " has left the duel.");
                break;
            //End If
        }
    }
}
#endregion


      
    #region "Stats and Menus"
void imgHand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Module1.IamWatching) { return; }
            ZoneControl zImg = (ZoneControl)sender;
            if (zImg.Name.Contains("blank")) return;
            int index = Convert.ToInt32(zImg.Name.Substring(7, 1));
            if (index > Module1.PlayerCurrentHand.CountNumCards())
            {
                removezImg(zImg); return;
            }
            lstMyHand.SelectedIndex = index - 1;
            getHandStats();
        }
void opImgHand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    ZoneControl zImg = sender as ZoneControl;
    short index = Convert.ToInt16(zImg.Name.Substring(9, zImg.Name.Length - 9));
   if (Module1.OpponentCurrentHand[index] != null) genericShowStats(Module1.OpponentCurrentHand[index]);
}
void resetopHandReveal()
{
    for (short i = 1; i <= 20; i++)
    {
        Module1.OpponentCurrentHand[i] = null;
        if (i <= Module1.NumCardsInOpHand)
        {
            ZoneControl zImg = opimgHand(i);
            if (zImg != null) Module1.setImage(ref zImg.baseImage, "back.jpg", UriKind.Relative);

        }
    }
}


private void genericShowStats(SQLReference.CardDetails stats)
        {
            int n = 0;
            Image imgRefer = null;
            SeeChangeStatsAndFire();
            lblDuelName.Text = stats.Name;

            for (n = 1; n <= 12; n++)
            {
                imgStars(n).Source = null;
            }

            for (n = 1; n < stats.Level; n++)
            {
                imgRefer = imgStars(n); Module1.setImage(ref imgRefer, "Star.jpg", UriKind.Relative);
            }
            
            DontFireChangeEventATK = true; DontFireChangeEventDEF = true;
            DontFireChangeEventCounters = true;
            if (!stats.IsMonster())
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDuelATK.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDuelATK.Visibility = System.Windows.Visibility.Visible;
                lblDuelATK.Text = stats.ATK.ToStringCountingQuestions();
            }

            if (!stats.IsMonster())
            {
                lblDEFplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDuelDEF.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblDEFplaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDuelDEF.Visibility = System.Windows.Visibility.Visible;
                lblDuelDEF.Text = stats.DEF.ToStringCountingQuestions();
            }
            // // DontFireChangeEvent = false;
            // DontFireChangeEventCounters = false;
            if ( !stats.IsMonster() && !string.IsNullOrEmpty(stats.Type))
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Visible;
                switch (stats.Type)
                {
                    case "Continuous":
                        Module1.setImage(ref imgSTIcon, "ContinuousIcon.jpg", UriKind.Relative); break;
                    case "Counter":
                        Module1.setImage(ref imgSTIcon, "CounterIcon.jpg", UriKind.Relative); break;
                    case "Equip":
                        Module1.setImage(ref imgSTIcon, "EquipIcon.jpg", UriKind.Relative); break;
                    case "Field":
                        Module1.setImage(ref imgSTIcon, "FieldIcon.jpg", UriKind.Relative); break;
                    case "Quick-Play":
                        Module1.setImage(ref imgSTIcon, "Quick-PlayIcon.jpg", UriKind.Relative); break;
                    case "Ritual":
                        Module1.setImage(ref imgSTIcon, "RitualIcon.jpg", UriKind.Relative); break;
                }
            }
            else
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            lblDuelLore.Text = stats.Lore;
            lblDuelType.Text = stats.Type.NotDisplayEffect();

            cmbCounters.Visibility = System.Windows.Visibility.Visible;
            cmbCounters.Text = stats.Counters.ToString();

            lblOverlaid.Visibility = System.Windows.Visibility.Collapsed;
            cmbOverlaid.Visibility = System.Windows.Visibility.Collapsed;
           
            if (stats.Attribute == null)
                return;
            switch (stats.Attribute.ToLower())
            {
                case "dark":
                    Module1.setImage(ref imgDuelAttribute, "Dark.jpg", UriKind.Relative); break;
                case "light":
                    Module1.setImage(ref imgDuelAttribute, "Light.jpg", UriKind.Relative); break;
                case "earth":
                    Module1.setImage(ref imgDuelAttribute, "Earth.jpg", UriKind.Relative); break;
                case "wind":
                    Module1.setImage(ref imgDuelAttribute, "Wind.jpg", UriKind.Relative); break;
                case "fire":
                    Module1.setImage(ref imgDuelAttribute, "Fire.jpg", UriKind.Relative); break;
                case "water":
                    Module1.setImage(ref imgDuelAttribute, "Water.jpg", UriKind.Relative); break;
                case "spell":
                    Module1.setImage(ref imgDuelAttribute, "SpellIcon.jpg", UriKind.Relative); break;
                case "trap":
                    Module1.setImage(ref imgDuelAttribute, "TrapIcon.jpg", UriKind.Relative); break;
            }

        }
private void lstMyHand_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
           
            getHandStats();
        }
private void getHandStats()
        {
            try
            {
                short Index = (short)(lstMyHand.SelectedIndex + 1);
                if (Index == 0)
                    return;

                genericShowStats(Module1.PlayerCurrentHand[Index]);
                cmbCounters.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
            
        }
private void resetXyzLayout()
        {
            for (short k = 1; k <= 5; k++)
            {
                Canvas.SetZIndex(MonZone(k), 0);
                if (Module1.PlayerCurrentMonsters[k].Name != null && Module1.PlayerCurrentMonsters[k].Name != "")
                {
                   
                    System.Diagnostics.Debug.Assert(Canvas.GetZIndex(MonZone(k)) < Canvas.GetZIndex(ctxtMonster));
                    for (short j = 1; j <= Module1.PlayerOverlaid[k].CountNumCards(); j++)
                    {
                        Canvas.SetZIndex(imgXyz(k, j), (j * -1));
                        System.Diagnostics.Debug.WriteLine(imgXyz(k, j).Name + " ZIndex set to " + Canvas.GetZIndex(imgXyz(k, j)).ToString());
                    }
                }
            } 
        }
private void resetOpXyzLayout()
{
    for (short k = 1; k <= 5; k++)
    {
        Canvas.SetZIndex(opMonZone(k), 0);
        if (Module1.OpponentCurrentMonsters[k].Name != null && Module1.OpponentCurrentMonsters[k].Name != "")
        {
           
            for (short j = 1; j <= Module1.OpponentOverlaid[k].CountNumCards(); j++)
            {
                Canvas.SetZIndex(opimgXyz(k, j), (j * -1));
            }
        }
    }
}

private void MonZone_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string zoneName = ((ZoneControl)sender).Name;
            int Index = Convert.ToInt32(zoneName.Substring(7, zoneName.Length - 7));
            int n = 0;
            Image imgRefer = null;
            SeeChangeStatsAndFire();
            if (Module1.IamWatching && Module1.PlayerCurrentMonsters[Index].Facedown)
                return;
            if (goingToAttackZone == Index)
            {
                goingToAttackZone = 0;
                imgBattle.Visibility = System.Windows.Visibility.Collapsed;
                imgBattleOrigin.Visibility = System.Windows.Visibility.Collapsed;

            }
            if (goingToMoveZone == Index)
            {
                goingToMoveZone = 0;
                imgMove.Visibility = System.Windows.Visibility.Collapsed;
                imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (goingToMoveZone > 0)
            {
                Move(Index);
                return;

            }

            if (string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[Index].Name))
            {
                resetXyzLayout();
                return;
            }
     

            MonSelectedZone = Index;
            Module1.GlobalZone = Index;

            lblDuelName.Text = Module1.PlayerCurrentMonsters[Index].Name;

            for (n = 1; n <= 12; n++)
            {
                imgStars(n).Source = null;
            }

            for (n = 1; n <= Module1.PlayerCurrentMonsters[Index].Level; n++)
            {
                imgRefer = imgStars(n); Module1.setImage(ref imgRefer, "Star.jpg", UriKind.Relative);
            }
            DontFireChangeEventATK = true; DontFireChangeEventDEF = true;
            DontFireChangeEventCounters = true;
            if (!Module1.PlayerCurrentMonsters[Index].IsMonster())
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDuelATK.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDuelATK.Visibility = System.Windows.Visibility.Visible;
                lblDuelATK.Text = Module1.PlayerCurrentMonsters[Index].ATK.ToStringCountingQuestions();
                lblDuelATK.IsEnabled = true;
            }

            if (!Module1.PlayerCurrentMonsters[Index].IsMonster())
            {
                lblDEFplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDuelDEF.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                lblDEFplaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDuelDEF.Visibility = System.Windows.Visibility.Visible;
                lblDuelDEF.Text = Module1.PlayerCurrentMonsters[Index].DEF.ToStringCountingQuestions();
                lblDuelDEF.IsEnabled = true;
            }
            // // DontFireChangeEvent = false;
            // DontFireChangeEventCounters = false;
            if (!Module1.PlayerCurrentMonsters[Index].IsMonster() && !string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[Index].Type))
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Visible;
                if (Module1.PlayerCurrentMonsters[Index].Type == "Continuous")
                {
                    Module1.setImage(ref imgSTIcon, "ContinuousIcon.jpg", UriKind.Relative);
                }
                else if (Module1.PlayerCurrentMonsters[Index].Type == "Counter")
                {
                    Module1.setImage(ref imgSTIcon, "CounterIcon.jpg", UriKind.Relative);
                }
                else if (Module1.PlayerCurrentMonsters[Index].Type == "Equip")
                {
                    Module1.setImage(ref imgSTIcon, "EquipIcon.jpg", UriKind.Relative);
                }
                else if (Module1.PlayerCurrentMonsters[Index].Type == "Field")
                {
                    Module1.setImage(ref imgSTIcon, "FieldIcon.jpg", UriKind.Relative);
                }
                else if (Module1.PlayerCurrentMonsters[Index].Type == "Quick-Play")
                {
                    Module1.setImage(ref imgSTIcon, "Quick-PlayIcon.jpg", UriKind.Relative);
                }
                else if (Module1.PlayerCurrentMonsters[Index].Type == "Ritual")
                {
                    Module1.setImage(ref imgSTIcon, "RitualIcon.jpg", UriKind.Relative);
                }
            }
            else
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            lblDuelLore.Text = Module1.PlayerCurrentMonsters[Index].Lore;
            lblDuelType.Text = Module1.PlayerCurrentMonsters[Index].Type.NotDisplayEffect();

            cmbCounters.Visibility = System.Windows.Visibility.Visible;
            cmbCounters.Text = Module1.PlayerCurrentMonsters[Index].Counters.ToString();
            cmbCounters.IsEnabled = true;
           
           
            if (Module1.PlayerCurrentMonsters[Index].Attribute == null)
                return;

            string attr = Module1.PlayerCurrentMonsters[Index].Attribute.ToLower();


            if (attr == "dark")
                Module1.setImage(ref imgDuelAttribute, "Dark.jpg", UriKind.Relative);

            else if (attr == "light")
                Module1.setImage(ref imgDuelAttribute, "Light.jpg", UriKind.Relative);

            else if (attr == "earth")
                Module1.setImage(ref imgDuelAttribute, "Earth.jpg", UriKind.Relative);

            else if (attr == "wind")
                Module1.setImage(ref imgDuelAttribute, "Wind.jpg", UriKind.Relative);

            else if (attr == "fire")
                Module1.setImage(ref imgDuelAttribute, "Fire.jpg", UriKind.Relative);

            else if (attr == "water")
                Module1.setImage(ref imgDuelAttribute, "Water.jpg", UriKind.Relative);

            else if (attr == "spell")
                Module1.setImage(ref imgDuelAttribute, "SpellIcon.jpg", UriKind.Relative);

            else if (attr == "trap")
                Module1.setImage(ref imgDuelAttribute, "TrapIcon.jpg", UriKind.Relative);

            
            //System.Diagnostics.Debug.Assert(Canvas.GetZIndex(MonZone(Index)) < Canvas.GetZIndex(ctxtMonster), "MonZone is " + Canvas.GetZIndex(MonZone(Index)) + ", ctxt is " + Canvas.GetZIndex(ctxtMonster));
            for (short k = 1; k <= Module1.PlayerOverlaid[Index].CountNumCards(); k++)
            {
               // if (string.IsNullOrEmpty(Module1.PlayerOverlaid[Index, k])) { break; }
                 Canvas.SetZIndex(imgXyz(Index, k), (k * -1) + 6);
                 System.Diagnostics.Debug.WriteLine(imgXyz(Index, k).Name + " ZIndex set to " + Canvas.GetZIndex(imgXyz(Index, k)).ToString());
            }

            Canvas.SetZIndex(MonZone(Index), 6);
            Canvas.SetZIndex(ctxtMonster, 10);
            Canvas.SetZIndex(ctxtXyz, 10);
            Canvas.SetZIndex(ctxtSpellTrap, 10);
            
            
        }
private void stZone_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string zoneName = ((ZoneControl)sender).Name;
            short Index = Convert.ToInt16(zoneName.Substring(6, zoneName.Length - 6));
           if (Module1.IamWatching && Module1.PlayerCurrentST[Index].Facedown)
                return;
            SeeChangeStatsAndFire();

            if (goingToMoveZone - 5 == Index)
            {
                goingToMoveZone = 0;
                imgMove.Visibility = System.Windows.Visibility.Collapsed;
                imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (goingToMoveZone > 0 && string.IsNullOrEmpty(Module1.PlayerCurrentST[Index].Name))
            {
                Move((short)(Index + 5));
                return;

            }

            Module1.GlobalZone = (short)(Index + 5);

            if (Module1.PlayerCurrentST[Index].Name == null) { return; }

            cmbCounters.IsEnabled = true;
           genericShowStats(Module1.PlayerCurrentST[Index]);
            
        }

private void opMonZone_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string zoneName = ((ZoneControl)sender).Name;
            short Index = Convert.ToInt16(zoneName.Substring(9, zoneName.Length - 9));
            SeeChangeStatsAndFire();
            if (goingToAttackZone > 0 && !string.IsNullOrEmpty(Module1.OpponentCurrentMonsters[Index].Name))
            {
                Attack(Index);
            }
            if (Module1.OpponentCurrentMonsters[Index].Facedown)
                return;

            if (Module1.OpponentCurrentMonsters[Index].Name == null) { resetOpXyzLayout(); return; }

            lblDuelATK.IsEnabled = false;
            lblDuelDEF.IsEnabled = false;
            cmbCounters.IsEnabled = false;
            genericShowStats(Module1.OpponentCurrentMonsters[Index]);

            Canvas.SetZIndex(opMonZone(Index), 6);

            //System.Diagnostics.Debug.Assert(Canvas.GetZIndex(MonZone(Index)) < Canvas.GetZIndex(ctxtMonster), "MonZone is " + Canvas.GetZIndex(MonZone(Index)) + ", ctxt is " + Canvas.GetZIndex(ctxtMonster));
            for (short k = 1; k <= Module1.OpponentOverlaid[Index].CountNumCards(); k++)
            {
                // if (string.IsNullOrEmpty(Module1.PlayerOverlaid[Index, k])) { break; }
                Canvas.SetZIndex(opimgXyz(Index, k), (k * -1) + 6);
                System.Diagnostics.Debug.WriteLine(opimgXyz(Index, k).Name + " ZIndex set to " + Canvas.GetZIndex(opimgXyz(Index, k)).ToString());
            }
        }
private void opstZone_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
            string zoneName = ((ZoneControl)sender).Name;
            short Index = Convert.ToInt16(zoneName.Substring(8, zoneName.Length - 8));
            SeeChangeStatsAndFire();

            if (Module1.OpponentCurrentST[Index].Facedown)
                return;

            if (Module1.OpponentCurrentST[Index].Name == null) { return; }
            cmbCounters.IsEnabled = false;
            genericShowStats(Module1.OpponentCurrentST[Index]);
            
        }

private void FieldSpellZone_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           if (Module1.IamWatching && Module1.PlayerCurrentFSpell.Facedown)
                return;
            SeeChangeStatsAndFire();
            if (goingToMoveZone == 11)
            {
                goingToMoveZone = 0;
                imgMove.Visibility = System.Windows.Visibility.Collapsed;
                imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (goingToMoveZone > 0 && string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
            {
                Move(11);
                return;
            }

            if (Module1.PlayerCurrentFSpell.Name == null) { return; }
           Module1.GlobalZone = 11;
           cmbCounters.IsEnabled = true;
           genericShowStats(Module1.PlayerCurrentFSpell);

            
        }
private void opFieldSpellZone_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SeeChangeStatsAndFire();

            if (Module1.OpponentCurrentFSpell.Facedown)
                return;

            if (Module1.OpponentCurrentFSpell.Name == null) { return; }
            cmbCounters.IsEnabled = false;
            genericShowStats(Module1.OpponentCurrentFSpell);

            
        }

private void XyzImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{

    ZoneControl zImg = (ZoneControl)sender;
    int firstUndscor = zImg.Name.IndexOf("_");
    int secondUndscor = zImg.Name.IndexOf("_", firstUndscor + 1);
    int matZone = Convert.ToInt32(zImg.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
    int matIndex = Convert.ToInt32(zImg.Name.Substring(secondUndscor + 1, 1));

    // SQLReference.CardDetails stats = Module1.CardStats[Module1.findID(Module1.PlayerOverlaid[matZone, matIndex])];
    SQLReference.CardDetails stats = Module1.PlayerOverlaid[matZone][matIndex];
    genericShowStats(stats);

}
private void opXyzImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    ZoneControl zImg = (ZoneControl)sender;
    int firstUndscor = zImg.Name.IndexOf("_");
    int secondUndscor = zImg.Name.IndexOf("_", firstUndscor + 1);
    int matZone = Convert.ToInt32(zImg.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
    int matIndex = Convert.ToInt32(zImg.Name.Substring(secondUndscor + 1, 1));
    //  lblDuelName.Text = Module1.OpponentOverlaid[matZone, matIndex]; 
    genericShowStats(Module1.OpponentOverlaid[matZone][matIndex]);
}



void DeckZone_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
    {
        if (Module1.IamWatching)
            return;
        DrawCard();
    }
}
void GraveZone_Click(System.Object sender, MouseButtonEventArgs e)               {showViewForm("Graveyard");}
void opGraveZone_MouseLeftButtonUp(System.Object sender, MouseButtonEventArgs e) {showViewForm("Opponent Graveyard");}
void oprfgZone_MouseLeftButtonUp(System.Object sender, MouseButtonEventArgs e)   {showViewForm("Opponent RFG");}
void rfgZone_MouseLeftButtonUp(System.Object sender, MouseButtonEventArgs e)     {showViewForm("RFG");}
void ExtraDeckZone_MouseLeftButtonUp(System.Object sender, MouseButtonEventArgs e){ if (Module1.IamWatching == false)showViewForm("Extra Deck");}

private void MonZone_MouseOver(object sender, System.Windows.Input.MouseEventArgs e)
{
    string zoneName = ((ZoneControl)sender).Name;
    short Index = Convert.ToInt16(zoneName.Substring(7, zoneName.Length - 7));


    if (goingToMoveZone == 0)
        return;

    // if (string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[Index].Name))
    addImgBattleToZone(zoneName, ref imgMove);
}
private void stZone_MouseOver(object sender, System.Windows.Input.MouseEventArgs e)
{
    if (goingToMoveZone == 0)
        return;
    string zoneName = ((ZoneControl)sender).Name;
    short Index = Convert.ToInt16(zoneName.Substring(6, zoneName.Length - 6));
    if (string.IsNullOrEmpty(Module1.PlayerCurrentST[Index].Name))
        addImgBattleToZone(zoneName, ref imgMove);
}
private void opMonZone_MouseOver(object sender, System.Windows.Input.MouseEventArgs e)
{
    if (goingToAttackZone == 0)
        return;
    string zoneName = ((ZoneControl)sender).Name;
    short Index = Convert.ToInt16(zoneName.Substring(9, zoneName.Length - 9));
    if (!string.IsNullOrEmpty(Module1.OpponentCurrentMonsters[Index].Name))
        addImgBattleToZone(zoneName, ref imgBattle);

}
private void fieldspellzone_MouseOver(object sender, System.Windows.Input.MouseEventArgs e)
{
    if (goingToMoveZone > 0 && string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
    {
        addImgBattleToZone("FieldSpellZone", ref imgMove);
    }
}

private void DeckZone_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    if (Module1.IamWatching)
        return;
    e.Handled = true;
    HideAllContext();
    ctxtDeck.Visibility = System.Windows.Visibility.Visible;
    FollowMouse(ref DeckTransform, ctxtDeck, e.GetPosition(LayoutRoot), false);
}
private void lstMyHand_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    if (lstMyHand.Items.Count == 0 || lstMyHand.SelectedIndex == -1)
        return;
    if (Module1.IamWatching)
        return;
    e.Handled = true;
    
    // If lstMyHand.SelectedIndex > -1 Then
    HideAllContext();
    handContextIndex = (short)(lstMyHand.SelectedIndex + 1);
    ctxtHand.Visibility = System.Windows.Visibility.Visible;
    FollowMouse(ref HandTransform, ctxtHand, e.GetPosition(LayoutRoot), true);
    // End If
}
private void imgHand_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    if (lstMyHand.Items.Count == 0)
        return;
    if (Module1.IamWatching)
        return;
    e.Handled = true;

    ZoneControl zImg = (ZoneControl)sender;
    int index = Convert.ToInt32(zImg.Name.Replace("imgHand", ""));
    if (index > Module1.PlayerCurrentHand.CountNumCards())
    {
        removezImg(zImg); return;
    }
    lstMyHand.SelectedIndex = index - 1;

    // If lstMyHand.SelectedIndex > -1 Then
    HideAllContext();
    handContextIndex = (short)index;
    ctxtHand.Visibility = System.Windows.Visibility.Visible;
    FollowMouse(ref HandTransform, ctxtHand, e.GetPosition(LayoutRoot), true);

}
private void XyzImg_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
{
    ZoneControl imageSender = (ZoneControl)sender;
    int firstUndscor = imageSender.Name.IndexOf("_");
    int secondUndscor = imageSender.Name.IndexOf("_", firstUndscor + 1);
    int matZone = Convert.ToInt32(imageSender.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
    int matIndex = Convert.ToInt32(imageSender.Name.Substring(secondUndscor + 1, 1));

    if (Module1.IamWatching)
        return;
    monContextIndex = (short)matZone;
    xyzContextIndex = (short)matIndex;
    e.Handled = true;
    HideAllContext();

    ctxtXyz.Visibility = System.Windows.Visibility.Visible;
    FollowMouse(ref  XyzTransform, ctxtXyz, e.GetPosition(LayoutRoot), true);
}
       
private void MonZone_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    ZoneControl imageSender = (ZoneControl)sender;
    short index = Convert.ToInt16(imageSender.Name.Substring(7, imageSender.Name.Length - 7));

    if (Module1.IamWatching)
        return;
    monContextIndex = index;
    e.Handled = true;
    HideAllContext();
    if (!string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[index].Name))
    {
        resetOpXyzLayout();
        ctxtMonster.Visibility = System.Windows.Visibility.Visible;
        FollowMouse(ref MonsterTransform, ctxtMonster, e.GetPosition(LayoutRoot), true);
    }
    else
    {
        ctxtMonsterEmpty.Visibility = System.Windows.Visibility.Visible;
        FollowMouse(ref MonsterEmptyTransform, ctxtMonsterEmpty, e.GetPosition(LayoutRoot), true);
    }

}
private void stZone_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    ZoneControl imageSender = (ZoneControl)sender;
    if (object.ReferenceEquals(imageSender, FieldSpellZone))
    {
        stContextIndex = 6;

    }
    else
    {
        stContextIndex = Convert.ToInt16(imageSender.Name.Substring(6, imageSender.Name.Length - 6));
    }


    if (Module1.IamWatching)
        return;
    //stContextIndex = index;
    e.Handled = true;
    HideAllContext();

    if (stContextIndex == 6 || !string.IsNullOrEmpty(Module1.PlayerCurrentST[stContextIndex].Name))
    {
        ctxtSpellTrap.Visibility = System.Windows.Visibility.Visible;
        resetXyzLayout();
        FollowMouse(ref SpellTrapTransform, ctxtSpellTrap, e.GetPosition(LayoutRoot), true);
    }


}
public void CopyCanvasCoordinates(UIElement source, UIElement target)
{
    Canvas.SetLeft(target, Canvas.GetLeft(source));
    Canvas.SetTop(target, Canvas.GetTop(source));
}
public void FollowMouse(ref TranslateTransform transform, ContextMenu context, System.Windows.Point point, bool displayAbove)
{
    double moveRight = point.X - Canvas.GetLeft(context);
    double moveDown = point.Y - Canvas.GetTop(context);
    if (displayAbove)
    {
        if (!double.IsNaN(context.Height))
        {
            moveDown -= context.Height;
        }
        else
        {

            moveDown -= 100.0;
        }
    }
    try
    {
        if (Canvas.GetZIndex(context) < 1)
            Canvas.SetZIndex(context, 1);
    }
    catch
    {

    }
    transform.X = moveRight;
    transform.Y = moveDown;

}
private void addImgBattleToZone(string ZoneName, ref Image img)
{
    if (ZoneName == "lblOpponentLP")
    {
        TextBlock zoneControl = (TextBlock)FindName(ZoneName);

       // img.Margin = zoneControl.Margin;
        CopyCanvasCoordinates(zoneControl, img);
    }
    else if (ZoneName == "FieldSpellZone")
    {
        //img.Margin = FieldSpellZone.Margin;
        CopyCanvasCoordinates(FieldSpellZone, img);
    }
    else
    {
        ZoneControl zoneControl = (ZoneControl)FindName(ZoneName);

     //   img.Margin = zoneControl.Margin;
        CopyCanvasCoordinates(zoneControl, img);
    }
    img.Visibility = System.Windows.Visibility.Visible;
}
private void HideAllContext()
{
    ctxtDeck.Visibility = System.Windows.Visibility.Collapsed;
    ctxtHand.Visibility = System.Windows.Visibility.Collapsed;
    ctxtMonster.Visibility = System.Windows.Visibility.Collapsed;
    ctxtSpellTrap.Visibility = System.Windows.Visibility.Collapsed;
    ctxtMonsterEmpty.Visibility = System.Windows.Visibility.Collapsed;
    ctxtXyz.Visibility = System.Windows.Visibility.Collapsed;
}
public void DeckContextItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
{
    HideAllContext();
    switch (itemText)
    {
        case "View":
            SummarizeJabber(TheEvent: "Viewing Deck");
            showViewForm("Deck");
            break;
        case "Shuffle":
            Module1.Shuffle();
            SummarizeJabber(TheEvent: "Shuffle");
            break;
        case "Mill":
            Mill(false);
            break;
        case "Banish Top":
            Mill(true);
            break;
        case "Surrender":
            System.Windows.MessageBoxResult answer = MessageBox.Show("Are you sure you want to surrender?", "Surrender", MessageBoxButton.OKCancel);
            if (answer == MessageBoxResult.OK)
            {
                SummarizeJabber(TheEvent: "Opponent Surrendered");
                answer = MessageBox.Show("You Lose!" + Environment.NewLine + Environment.NewLine + "Play again with this Opponent? (side deck will open)", "Play Again", MessageBoxButton.OKCancel);
                resetStats();
                // JabberClient1.LeaveDuelAsync(username, myRoomID)
                //messageTimer.Stop();
                if (answer == MessageBoxResult.OK)
                {
                    //   Module1.sockStack.Push(sock);
                   // Module1.sock.Disconnect();
                    Module1.sideDecking = true;
                    Module1.warnOnExitMessage = "";
                    this.NavigationService.Navigate(new System.Uri("/DeckEditor", UriKind.Relative));

                }
                else
                {
                    RateForm.Show();
                    //   Me.NavigationService.Navigate(New System.Uri("/Lobby", UriKind.Relative))
                }
            }
            break;
    }

}
public void HandContextItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
{
    HideAllContext();
    switch (itemText)
    {
        case "Summon / Activate":
            SummonOrActivateFromHand(handContextIndex);
            break;
        case "Set (Monster)":
            SetFromHand(handContextIndex, true);
            break;
        case "Set (S/T)":
            SetFromHand(handContextIndex, false);
            break;
        case "Discard":
            Discard(handContextIndex, false);
            break;
        case "Discard at Random":
            short randhand = (short)Module1.Rand(1, Module1.PlayerCurrentHand.CountNumCards(), new Random());

            Discard(randhand, true);
            break;
        case "Discard All":
            int temp = Module1.PlayerCurrentHand.CountNumCards();
            for (int n = Module1.PlayerCurrentHand.CountNumCards(); n >= 1; n += -1)
            {
                Discard(n, false);
            }

            break;
        case "Banish":
            BanishFromHand(handContextIndex, false);
            break;
        case "Banish Facedown":
            BanishFromHand(handContextIndex, true);
            break;
        case "Reveal Card":
            SummarizeJabber(Area1: "Reveal", Index1: handContextIndex, TheEvent: "Reveal", Text: Module1.PlayerCurrentHand[handContextIndex].Name);
            break;
        case "Reveal All":
            //  string returnstring = "";
            List<string> jabberMessages = new List<string>();
            for (short n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
            {
                jabberMessages.Add(SummarizeJabber(Area1: "Reveal", Index1: n, returnOnly: true));

            }
            SendJabber(combineMultipleJabbers(jabberMessages));
            SummarizeJabber(TheEvent: "Reveal All");
            break;
        case "To Top of Deck":
            HandToTop(handContextIndex);
            break;
        case "To Bottom of Deck":
            HandToBottom(handContextIndex);

            break;
    }

}
public void MonsterContextItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
{
    HideAllContext();

    switch (itemText)
    {
        case "Send to Grave":
            if (Module1.PlayerCurrentMonsters[monContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
            Destroy(monContextIndex);
            break;
        case "Return to Hand":
            if (Module1.PlayerCurrentMonsters[monContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
            Bounce(monContextIndex);
            break;
        case "Banish":
            if (Module1.PlayerCurrentMonsters[monContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
            Banish(monContextIndex);
            break;
        case "Switch Position":
            ChangePositionOrActivate(monContextIndex);
            break;
        case "Flip":
            if (Module1.PlayerCurrentMonsters[monContextIndex].Facedown)
                FlipFaceUp(monContextIndex);
            else  
                FlipFaceDown(monContextIndex);
            break;
        case "To Top of Deck":
            if (Module1.PlayerCurrentMonsters[monContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
            Spin(monContextIndex);
            break;
        case "To Bottom of Deck":
            if (Module1.PlayerCurrentMonsters[monContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
            ToBottom(monContextIndex);
            break;
        case "Change Control":
            GiveControl(monContextIndex);
            break;
        case "Attack...":
            imgBattleOrigin.Visibility = System.Windows.Visibility.Visible;
            addImgBattleToZone("MonZone" + monContextIndex, ref imgBattleOrigin);
            //imgBattle.Visibility = Windows.Visibility.Visible
            goingToAttackZone = monContextIndex;
            break;
        case "Move to Zone...":
            imgMoveOrigin.Visibility = System.Windows.Visibility.Visible;
            addImgBattleToZone("MonZone" + monContextIndex, ref imgMoveOrigin);
            // imgMove.Visibility = Windows.Visibility.Visible
            goingToMoveZone = monContextIndex;
            break;
    }

}
public void MonsterEmptyContextItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
{
    HideAllContext();
    switch (itemText)
    {
        case "Summon Token":
            summonToken(monContextIndex);
            break;
    }

}
public void SpellTrapContextItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
{
    HideAllContext();

    switch (itemText)
    {
        case "Activate":
            if (stContextIndex < 6)
            {
                ChangePositionOrActivate((short)(stContextIndex + 5));
            }
            else
            {
                ChangePositionOrActivate(11);
            }
            break;
        case "Send to Grave":
            if (stContextIndex < 6)
            {
                if (Module1.PlayerCurrentST[stContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                Destroy((short)(stContextIndex + 5));
            }
            else
            {
                if (Module1.PlayerCurrentFSpell.OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                Destroy(11);
            }
            break;
        case "Return to Hand":
            if (stContextIndex < 6)
            {
                if (Module1.PlayerCurrentST[stContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                Bounce((short)(stContextIndex + 5));
            }
            else
            {
                if (Module1.PlayerCurrentFSpell.OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                Bounce(11);
            }
            break;
        case "Banish":
            if (stContextIndex < 6)
            {
                if (Module1.PlayerCurrentST[stContextIndex].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                Banish((short)(stContextIndex + 5));
            }
            else
            {
                if (Module1.PlayerCurrentFSpell.OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                Banish(11);
            }
            break;
        case "Flip Face-Down":
            if (stContextIndex < 6)
                FlipFaceDown((short)(stContextIndex + 5));
            else
                FlipFaceDown(11);
            break;
        case "To Top of Deck":
            if (stContextIndex < 6)
                Spin((short)(stContextIndex + 5));
            else
                Spin(11);

            break;
        case "To Bottom of Deck":
            if (stContextIndex < 6)
            {
                ToBottom((short)(stContextIndex + 5));
            }
            else
            {
                ToBottom(11);
            }
            break;
        case "Change Control":

            GiveControlST(stContextIndex);
            break;
        case "Move to Zone...":
            imgMoveOrigin.Visibility = System.Windows.Visibility.Visible;
            if (stContextIndex < 6)
            {
                addImgBattleToZone("stZone" + stContextIndex, ref imgMoveOrigin);
                goingToMoveZone = (short)(stContextIndex + 5);
            }
            else
            {
                addImgBattleToZone("FieldSpellZone", ref imgMoveOrigin);
                goingToMoveZone = 11;
            }
            break;
        // imgMove.Visibility = Windows.Visibility.Visible

    }

}
public void XyzContextItem_Clicked(string itemText, System.Windows.RoutedEventArgs e)
{
    HideAllContext();

    switch (itemText)
    {
        case "Detatch":
            DetatchXyzMaterial(monContextIndex, xyzContextIndex);
            break;
    }
}
        #endregion
    #region "Properties"
        public Image imgStars(int index)
        {
                Image img = (Image)LayoutRoot.FindName("imgStars" + index);
                return img;
          
        }
        public ZoneControl imgHand(int index)
        {
            ZoneControl zImg = (ZoneControl)LayoutRoot.FindName("imgHand" + index.ToString());
            return zImg;
        }
        public ZoneControl opimgHand(int index)
        {
            ZoneControl zImg = (ZoneControl)LayoutRoot.FindName("opimgHand" + index.ToString());
            return zImg;
        }
        public ZoneControl imgXyz(int MonIndex, int MaterialIndex)
        {
            ZoneControl zImg = (ZoneControl)LayoutRoot.FindName("XyzMat_" + MonIndex.ToString() + "_" + MaterialIndex.ToString());
            return zImg;
        }
        public ZoneControl opimgXyz(int MonIndex, int MaterialIndex)
        {
            ZoneControl zImg = (ZoneControl)LayoutRoot.FindName("opXyzMat_" + MonIndex.ToString() + "_" + MaterialIndex.ToString());
            return zImg;

        }
        public ZoneControl MonZone(int index)
        {
            ZoneControl img = (ZoneControl)LayoutRoot.FindName("MonZone" + index);
            return img;
        }
        public ZoneControl fieldZone(int index)
        {
            if (index == 0) return null;
            if (index > 0 && index < 6)
            {
                return MonZone(index);
            }
            else if (index > 5 && index < 11)
            {
                return stZone(index - 5);
            }
            else if (index == 11)
            {
                return FieldSpellZone;
            }
            return null;
        }
        public ZoneControl stZone(int index)
        {
            ZoneControl img = (ZoneControl)LayoutRoot.FindName("stZone" + index);
            return img;
        }
        public ZoneControl opMonZone(int index)
        {
            ZoneControl img = (ZoneControl)LayoutRoot.FindName("opMonZone" + index);
            return img;
        }
        public ZoneControl opstZone(int index)
        {
            ZoneControl img = (ZoneControl)LayoutRoot.FindName("opstZone" + index);
            return img;
        }
        public static SQLReference.CardDetails PlayerCurrentField(int index)
        {
            if (index >= 1 && index <= 5)
                return Module1.PlayerCurrentMonsters[index];
            if (index >= 6 && index <= 10)
                return Module1.PlayerCurrentST[index - 5];
            if (index == 11)
                return Module1.PlayerCurrentFSpell;

            return null;
        }
        private static int NumOfPlayerMonsters
        {
            get
            {
                short functionReturnValue = 0;
                functionReturnValue = 0;
                for (int n = 1; n <= 5; n++)
                {
                    if (!string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[n].Name))
                        functionReturnValue += 1;
                }
                return functionReturnValue;
           
            }
        }
        private static int NumOfPlayerST
        {
            get
            {
                short functionReturnValue = 0;
                functionReturnValue = 0;
                for (int n = 1; n <= 5; n++)
                {
                    if (!string.IsNullOrEmpty(Module1.PlayerCurrentST[n].Name))
                        functionReturnValue += 1;
                }
                return functionReturnValue;
          
            }
        }
        #endregion
    #region "Movement"
private string DrawCard(bool returnOnly = false)
        {
            try
            {
                if (Module1.PlayerCurrentDeck.CountNumCards() == 0)
                    return "";
                if (Module1.PlayerCurrentHand.CountNumCards() == 20)
                {
                    MsgBox("You cannot have more than 20 cards in your hand.");
                    return "";
                }

                // Module1.PlayerCurrentHand.CountNumCards() += 1;
                DontFireChangeEventATK = true; DontFireChangeEventDEF = true;
                lstMyHand.Items.Add(Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name);

                Module1.PlayerCurrentHand.Add(Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()]);
                Module1.PlayerCurrentDeck.RemoveAt(Module1.PlayerCurrentDeck.CountNumCards());
                //Module1.PlayerCurrentDeck.CountNumCards() -= 1;
                if (Module1.PlayerCurrentDeck.CountNumCards() == 0)
                {
                    Module1.setImage(ref DeckZone, BLANK_IMAGE, UriKind.Relative);
                }

                lstMyHand.SelectedIndex = lstMyHand.Items.Count - 1;
                lblHoverDeck.Text = Module1.PlayerCurrentDeck.CountNumCards().ToString();
                
                addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
                imgHand(Module1.PlayerCurrentHand.CountNumCards()).animationTimer.setInMotion(DeckZone, imgHand(Module1.PlayerCurrentHand.CountNumCards()), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());

                Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(), "DeckZone", "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());

                if (returnOnly)
                {
                    return SummarizeJabber(Stat1: "NHand", Stat2: "NDeck", TheEvent: "Draw Card", returnOnly: returnOnly, bundle: bundle);
                }
                else
                {
                    SummarizeJabber(Stat1: "NHand", Stat2: "NDeck", TheEvent: "Draw Card", returnOnly: returnOnly, bundle: bundle);
                    return "";
                }
                
            }
            catch (Exception ex)
            {
                MsgBox("Unexpected error: " + ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
                return "";
            }


        }
void updateMySideHovers()
{
    lblHoverGrave.Text = Module1.PlayerCurrentGrave.CountNumCards().ToString();
    lblHoverRFG.Text = Module1.PlayerCurrentRFG.CountNumCards().ToString();
  

    if (Module1.IamWatching)
    {
        lblHoverDeck.Text = Module1.watcherNumCardsInDeck.ToString();
        lblHoverEDeck.Text = Module1.watcherNumCardsInEDeck.ToString();
    }
    else
    {
        lblHoverDeck.Text = Module1.PlayerCurrentDeck.CountNumCards().ToString();
        lblHoverEDeck.Text = Module1.PlayerCurrentEDeck.CountNumCards().ToString();
    }
}
void updateOtherSideHovers()
{
    lblHoverOpGrave.Text = Module1.OpponentCurrentGrave.CountNumCards().ToString();
    lblHoverOpRFG.Text = Module1.OpponentCurrentRFG.CountNumCards().ToString();
    lblHoverOpDeck.Text = Module1.NumCardsInopDeck.ToString();
    lblHoverOpEDeck.Text = Module1.NumCardsInopEDeck.ToString();
    lblOpponentHandCount.Text = "Hand: " + Module1.NumCardsInOpHand.ToString();
    
}
public string AddFromGraveToHand(int Index)
	{
		string returnmessage = null;
		if (Module1.BelongsInExtra(Module1.PlayerCurrentGrave[Index])) {
			Module1.ReturnToExtra(Module1.PlayerCurrentGrave[Index], "Graveyard");
			returnmessage = "Returned " + "\"" + Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name + "\"" + " from their Graveyard to Extra Deck.";
		} else {
		 Module1.PlayerCurrentHand.Add(Module1.PlayerCurrentGrave[Index].toTrueStats());
           
            lstMyHand.Items.Add(Module1.PlayerCurrentGrave[Index].Name);
			returnmessage = "Added " + "\"" + Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name + "\"" + " from their Graveyard to their Hand.";
		}
       Module1.PlayerCurrentGrave.RemoveAt(Index);

        if (Module1.PlayerCurrentGrave.CountNumCards() == 0)
            Module1.setImage(ref GraveZone, BLANK_IMAGE, UriKind.Relative);
        else if (Module1.PlayerCurrentGrave.CountNumCards() == Index - 1)
            UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet);

        Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(), 
                                                            "GraveZone", 
                                                            "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
    
        SummarizeJabber(Area1:"rmGrave", Index1:Index, Stat1:"NHand", bundle: bundle);

        addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
        updateImgHandPositions();
        double targetLeft = calculateDesirableImgHandPos(Module1.PlayerCurrentHand.CountNumCards());
        ZoneControl zImg = imgHand(Module1.PlayerCurrentHand.CountNumCards());
        zImg.animationTimer.setInMotion(GraveZone, new Point(targetLeft, imgHandTop), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
       
		return returnmessage;
	}
public string AddFromRemovedFromPlayToHand(int Index, bool facedown)
	    {
		string returnmessage = null;
		if (Module1.BelongsInExtra(Module1.PlayerCurrentRFG[Index])) {

			Module1.ReturnToExtra(Module1.PlayerCurrentRFG[Index], "RFG");
			returnmessage = "Returned " + "\"" + Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name + "\"" + " from their RFG to Extra Deck.";
		
        } else {
			
            Module1.PlayerCurrentHand.Add(Module1.PlayerCurrentRFG[Index].toTrueStats());
            lstMyHand.Items.Add(Module1.PlayerCurrentRFG[Index].Name);
            if (facedown)
                returnmessage = "Added a facedown card from their RFG to their Hand.";
            else
			    returnmessage = "Added " + "\"" + Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name + "\"" + " from their RFG to their Hand.";
		}

        Module1.PlayerCurrentRFG.RemoveAt(Index);
		
       if (Module1.PlayerCurrentRFG.CountNumCards() == 0)
            Module1.setImage(ref rfgZone, BLANK_IMAGE, UriKind.Relative);
        else if (Module1.PlayerCurrentRFG.CountNumCards() == Index - 1)
            UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], Module1.mySet);


        Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(),
                                                            "rfgZone",
                                                            "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
         
        SummarizeJabber(Area1:"rmRFG", Index1:Index, Stat1:"NHand", bundle:bundle);

        addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
        updateImgHandPositions();
        double targetLeft = calculateDesirableImgHandPos(Module1.PlayerCurrentHand.CountNumCards());
        ZoneControl zImg = imgHand(Module1.PlayerCurrentHand.CountNumCards());
        zImg.animationTimer.setInMotion(rfgZone, new Point(targetLeft, imgHandTop), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
        

		return returnmessage;
	}
public void AddFromDeckToHand(int Index)
	{
		Module1.PlayerCurrentHand.Add(Module1.PlayerCurrentDeck[Index].toTrueStats());
           
        lstMyHand.Items.Add(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name);

        Module1.PlayerCurrentDeck.RemoveAt(Index);
		//Module1.PlayerCurrentDeck.CountNumCards() = (short)(Module1.PlayerCurrentDeck.CountNumCards() - 1);
        Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(),
                                                            "DeckZone",
                                                            "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());

		SummarizeJabber(Stat1:"NHand", Stat2:"NDeck", bundle:bundle);
        Module1.Shuffle();
        SummarizeJabber(TheEvent: "Shuffle");
        addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
        updateImgHandPositions();
        double targetLeft = calculateDesirableImgHandPos(Module1.PlayerCurrentHand.CountNumCards());
        ZoneControl zImg = imgHand(Module1.PlayerCurrentHand.CountNumCards());
        zImg.endCallFunction = shuffleHand;
    
        zImg.animationTimer.setInMotion(DeckZone, new Point(targetLeft, imgHandTop), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
        
	}
private bool SSfromDeck(int Index)
	{
		
		SelectedZone = FindEmptyMonZone();
		if (SelectedZone == 6)
			return true;
        ZoneControl refer = MonZone(SelectedZone);

		Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[SelectedZone], Module1.PlayerCurrentDeck[Index]);
        Module1.PlayerCurrentDeck.RemoveAt(Index);

        MonZone(SelectedZone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentMonsters[SelectedZone].ID), "MonZone" + SelectedZone.ToString(), false, false);
        MonZone(SelectedZone).animationTimer.setInMotion(DeckZone, MonZone(SelectedZone), 300, 10, false, "MonZone" + SelectedZone.ToString());

        Module1.animationBundle bundle = createBundleToSend("MonZone" + SelectedZone.ToString(),
                                                            "DeckZone",
                                                            "MonZone" + SelectedZone.ToString());

        SummarizeJabber(Area1:"Monster", Index1:SelectedZone, Stat1:"NDeck", bundle:bundle);
		return false;
	}
private bool SSfromgrave(int Index)
	{
		SelectedZone = FindEmptyMonZone();
		if (SelectedZone == 6)
			return true;
		Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[SelectedZone], Module1.PlayerCurrentGrave[Index]);

        Module1.PlayerCurrentGrave.RemoveAt(Index);
        if (Module1.PlayerCurrentGrave.CountNumCards() == 0)
            Module1.setImage(ref GraveZone, BLANK_IMAGE, UriKind.Relative);
		else if (Index == Module1.PlayerCurrentGrave.CountNumCards() + 1)
            UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet);
      //  ZoneControl refer = MonZone(SelectedZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[SelectedZone], Module1.mySet);

        Module1.animationBundle bundle = createBundleToSend("MonZone" + SelectedZone.ToString(),
                                                                "GraveZone",
                                                                "MonZone" + SelectedZone.ToString());

    
    SummarizeJabber(Area1:"Monster", Index1:SelectedZone, Area2:"rmGrave", Index2:Index, bundle:bundle);
		
        MonZone(SelectedZone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentMonsters[SelectedZone].ID), "MonZone" + SelectedZone.ToString(), false, false);
        MonZone(SelectedZone).animationTimer.setInMotion(GraveZone, MonZone(SelectedZone), 300, 10, false, "MonZone" + SelectedZone.ToString());

		return false;

	}
private bool SSfromRFG(int Index)
	{
		if (NumOfPlayerMonsters == 5)
			return true;
		SelectedZone = FindEmptyMonZone();
		if (SelectedZone == 6)
			return true;
		Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[SelectedZone],Module1.PlayerCurrentRFG[Index]);
		Module1.PlayerCurrentMonsters[SelectedZone].Facedown = false;

        Module1.PlayerCurrentRFG.RemoveAt(Index);

		if (Module1.PlayerCurrentRFG.CountNumCards() == 0)
            Module1.setImage(ref rfgZone, BLANK_IMAGE, UriKind.Relative);
		else if (Index == (short)(Module1.PlayerCurrentRFG.CountNumCards() + 1))
            UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], Module1.mySet);
        //ZoneControl refer = MonZone(SelectedZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[SelectedZone], Module1.mySet);

        Module1.animationBundle bundle = createBundleToSend("MonZone" + SelectedZone.ToString(),
                                                                  "rfgZone",
                                                                  "MonZone" + SelectedZone.ToString());
    
    SummarizeJabber(Area1:"Monster", Index1:SelectedZone, Area2:"rmRFG", Index2:Index, bundle:bundle);
		

        MonZone(SelectedZone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentMonsters[SelectedZone].ID), "MonZone" + SelectedZone.ToString(), false, false);
        MonZone(SelectedZone).animationTimer.setInMotion(rfgZone, MonZone(SelectedZone), 300, 10, false, "MonZone" + SelectedZone.ToString());

		return false;
	}
private bool PlaceSTOnFieldFromRemovedFromPlay(int Index)
	{
        stSelectedZone = FindEmptySTZone();
		List<string> jabberMessages = new List<string>();
        Module1.animationBundle bundle;
		if (stSelectedZone == 6)
			return true;
		if (!Module1.PlayerCurrentRFG[Index].IsMonster() && Module1.PlayerCurrentRFG[Index].Type == "Field") {
			if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name)) {
				jabberMessages.Add(Destroy(11, true, true));
			}
			Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell,Module1.CardStats[Module1.findID(Module1.PlayerCurrentRFG[Index].Name)]);
         
            FieldSpellZone.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentFSpell.ID), "FieldSpellZone", false, false);
            FieldSpellZone.animationTimer.setInMotion(rfgZone, FieldSpellZone, 300, 10, false, "FieldSpellZone");

            bundle = createBundleToSend("FieldSpellZone", "rfgZone", "FieldSpellZone");

        } else {
			Module1.copyCardDetails(ref Module1.PlayerCurrentST[stSelectedZone], Module1.CardStats[Module1.findID(Module1.PlayerCurrentRFG[Index].Name)]);
           
            stZone(stSelectedZone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentST[stSelectedZone].ID), "stZone" + stSelectedZone.ToString(), false, false);
            stZone(stSelectedZone).animationTimer.setInMotion(rfgZone, stZone(stSelectedZone), 300, 10, false, "stZone" + stSelectedZone.ToString());

            bundle = createBundleToSend("stZone" + stSelectedZone.ToString(), "rfgZone", "stZone" + stSelectedZone.ToString());
        }

        Module1.PlayerCurrentRFG.RemoveAt(Index);

        
        jabberMessages.Add(SummarizeJabber(Area1: "ST", Index1: stSelectedZone, Area2: "rmRFG", Index2: (short)Module1.NumberOnList, bundle:bundle, returnOnly: true));
		if (jabberMessages.Count > 1) {
			SendJabber(combineMultipleJabbers(jabberMessages));
		} else {
			SendJabber(jabberMessages[0]);
		}
		if (Module1.NumberOnList == (short)(Module1.PlayerCurrentRFG.CountNumCards() + 1)) {
			if (Module1.PlayerCurrentRFG.CountNumCards() == 0) {
				Module1.setImage(ref rfgZone, BLANK_IMAGE, UriKind.Relative);
			} else {
                UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], Module1.mySet);
			}
		}
		return false;
	}
private bool PlaceSTOnFieldFromGraveyard(int Index)
	{
       stSelectedZone = FindEmptySTZone();
		List<string> jabberMessages = new List<string>();
        Module1.animationBundle bundle;
		if (stSelectedZone == 6)
			return true;
		if (!Module1.PlayerCurrentGrave[Index].IsMonster() && Module1.PlayerCurrentGrave[Index].Type == "Field") {
            if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
            {
                jabberMessages.Add(Destroy(11, true, true));
            }
           
                Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.CardStats[Module1.findID(Module1.PlayerCurrentGrave[Index].Name)]);
                FieldSpellZone.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentFSpell.ID), "FieldSpellZone", false, false);
                FieldSpellZone.animationTimer.setInMotion(GraveZone, FieldSpellZone, 300, 10, false, "FieldSpellZone");

                bundle = createBundleToSend("FieldSpellZone", "GraveZone", "FieldSpellZone");

        } else {
			Module1.copyCardDetails(ref Module1.PlayerCurrentST[stSelectedZone], Module1.CardStats[Module1.findID(Module1.PlayerCurrentGrave[Index].Name)]);
            stZone(stSelectedZone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentST[stSelectedZone].ID), "stZone" + stSelectedZone.ToString(), false, false);
            stZone(stSelectedZone).animationTimer.setInMotion(GraveZone, stZone(stSelectedZone), 300, 10, false, "stZone" + stSelectedZone.ToString());
         
            bundle = createBundleToSend("stZone" + stSelectedZone.ToString(), "GraveZone", "stZone" + stSelectedZone.ToString());
        }
	  Module1.PlayerCurrentGrave.RemoveAt(Index);
	  jabberMessages.Add(SummarizeJabber(Area1: "ST", Index1: stSelectedZone, Area2: "rmGrave", Index2: Index, bundle:bundle, returnOnly:true));
		if (jabberMessages.Count > 1) {
			SendJabber(combineMultipleJabbers(jabberMessages));
		} else {
			SendJabber(jabberMessages[0]);
		}
		if (Module1.NumberOnList ==(short)(Module1.PlayerCurrentGrave.CountNumCards() + 1)) {
			if (Module1.PlayerCurrentGrave.CountNumCards() == 0) {
				Module1.setImage(ref GraveZone, BLANK_IMAGE, UriKind.Relative);
			} else {
                UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet);
			}
		}
		return false;
	}
private bool PlaceSTOnFieldFromDeck(int Index)
	{
       stSelectedZone = FindEmptySTZone();
		List<string> jabberMessages = new List<string>();
        Module1.animationBundle bundle;
		if (stSelectedZone == 6)
			return true;
        if (!Module1.PlayerCurrentDeck[Index].IsMonster() && Module1.PlayerCurrentDeck[Index].Type == "Field")
        {
			if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name)) {
				jabberMessages.Add(Destroy(11, true, true));
			}
			Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.CardStats[Module1.findID(Module1.PlayerCurrentDeck[Index].Name)]);
            FieldSpellZone.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentFSpell.ID), "FieldSpellZone", false, false);
            FieldSpellZone.animationTimer.setInMotion(DeckZone, FieldSpellZone, 300, 10, false, "FieldSpellZone");

            bundle = createBundleToSend("FieldSpellZone", "DeckZone", "FieldSpellZone");

		} else {
			Module1.copyCardDetails(ref Module1.PlayerCurrentST[stSelectedZone],Module1.CardStats[Module1.findID(Module1.PlayerCurrentDeck[Index].Name)]);
           // refer = stZone(stSelectedZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[stSelectedZone], Module1.mySet);
            stZone(stSelectedZone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentST[stSelectedZone].ID), "stZone" + stSelectedZone.ToString(), false, false);
            stZone(stSelectedZone).animationTimer.setInMotion(DeckZone, stZone(stSelectedZone), 300, 10, false, "stZone" + stSelectedZone.ToString());

            bundle = createBundleToSend("stZone" + stSelectedZone.ToString(), "DeckZone", "stZone" + stSelectedZone.ToString());
        }
	
        Module1.PlayerCurrentDeck.RemoveAt(Index);
		

		jabberMessages.Add(SummarizeJabber(Area1:"ST", Index1:stSelectedZone, Stat1:"NDeck", bundle:bundle, returnOnly:true));
		if (jabberMessages.Count > 1) {
			SendJabber(combineMultipleJabbers(jabberMessages));
		} else {
			SendJabber(jabberMessages[0]);
		}

		return false;
	}
private bool NormalSynchroSummon(int Index)
        {
            if (Module1.PlayerCurrentEDeck[Index].IsMonster())
            {
                SelectedZone = FindEmptyMonZone();
                if (SelectedZone == 6)
                    return true;
                Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[SelectedZone], Module1.PlayerCurrentEDeck[Index]);
                ZoneControl refer = MonZone(SelectedZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[SelectedZone], Module1.mySet);

                Module1.PlayerCurrentEDeck.RemoveAt(Index);
                if (Module1.PlayerCurrentEDeck.CountNumCards() == 0)
                    Module1.setImage(ref ExtraDeckZone, BLANK_IMAGE, UriKind.Relative);


                Module1.animationBundle bundle = createBundleToSend("MonZone" + SelectedZone.ToString(),
                                                                    "ExtraDeckZone",
                                                                    "MonZone" + SelectedZone.ToString());

                SummarizeJabber(Area1:"Monster", Index1:SelectedZone, Stat1:"NEDeck", bundle:bundle);
            }
            else
            {
                if (Module1.PlayerCurrentEDeck[Index].Type == "Field")
                {
                    if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
                    {
                        Destroy(11);
                    }
                    Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.PlayerCurrentEDeck[Index]);
                    UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);


                    Module1.PlayerCurrentEDeck.RemoveAt(Index);
                    if (Module1.PlayerCurrentEDeck.CountNumCards() == 0)
                        Module1.setImage(ref ExtraDeckZone, BLANK_IMAGE, UriKind.Relative);

                    Module1.animationBundle bundle = createBundleToSend("FieldSpellZone",
                                                                    "ExtraDeckZone",
                                                                    "FieldSpellZone");

                    SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", bundle: bundle);

                }
                else
                {
                    SelectedZone = FindEmptySTZone();
                    if (SelectedZone == 6)
                        return true;
                    Module1.copyCardDetails(ref Module1.PlayerCurrentST[SelectedZone], Module1.PlayerCurrentEDeck[Index]);
                    ZoneControl refer = stZone(SelectedZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[SelectedZone], Module1.mySet);

                    Module1.PlayerCurrentEDeck.RemoveAt(Index);
                    if (Module1.PlayerCurrentEDeck.CountNumCards() == 0)
                        Module1.setImage(ref ExtraDeckZone, BLANK_IMAGE, UriKind.Relative);


                    Module1.animationBundle bundle = createBundleToSend("stZone" + SelectedZone.ToString(),
                                                    "ExtraDeckZone",
                                                    "stZone" + SelectedZone.ToString());

                    SummarizeJabber(Area1: "ST", Index1: SelectedZone, Stat1: "NEDeck", bundle: bundle);
                }
            }


         
            return false;
        }

private string Destroy(int zone, bool returnOnly = false, bool skipAnimation = false)
	{
        string functionReturnValue = null; ZoneControl refer = null;

        if (string.IsNullOrEmpty(PlayerCurrentField(zone).Name))
            return "";
        if (PlayerCurrentField(zone).Type.Contains("Token"))
        {
            setAsNothing(PlayerCurrentField(zone));
            refer = fieldZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
            return SummarizeJabber(Area1: Module1.DeriveAreaFromGlobalZone(zone), Index1: Convert.ToInt16(Module1.DeriveIndexFromGlobalZone(zone)), TheEvent: "Destroyed " + Module1.DeriveAreaFromGlobalZone(zone), Text: "Token", returnOnly: returnOnly);      
        }


		if (zone < 6) {         

           DetatchAllXyzMaterial(zone);
		  Module1.PlayerCurrentGrave.Add(Module1.PlayerCurrentMonsters[zone].toTrueStats());
            setAsNothing(Module1.PlayerCurrentMonsters[zone]);
            if (!skipAnimation)
            {
                MonZone(zone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].ID), "GraveZone", false, true);
                MonZone(zone).animationTimer.setInMotion(MonZone(zone), GraveZone, 300, 10, true, "MonZone" + zone.ToString());
            }
            else
            {
                 refer = MonZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                 UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet);
            }

            Module1.animationBundle bundle = createBundleToSend("MonZone" + zone.ToString(), "MonZone" + zone.ToString(), "GraveZone");
            return SummarizeJabber(Area1: "Monster", Index1: zone, Area2: "Grave", Index2: (short)Module1.PlayerCurrentGrave.CountNumCards(), TheEvent: "Destroyed Monster", Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name, returnOnly: returnOnly, bundle: bundle);

		}

		if (zone > 5 && zone < 11) {
			if (string.IsNullOrEmpty(Module1.PlayerCurrentST[zone - 5].Name))
				return "";
            Module1.PlayerCurrentGrave.Add(Module1.PlayerCurrentST[zone - 5].toTrueStats());
            setAsNothing(Module1.PlayerCurrentST[zone - 5]);

            stZone(zone - 5).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].ID), "GraveZone", false, true);
            stZone(zone - 5).animationTimer.setInMotion(stZone(zone - 5), GraveZone, 300, 10, true, "stZone" + (zone - 5).ToString());

            Module1.animationBundle bundle = createBundleToSend("stZone" + (zone - 5).ToString(), "stZone" + (zone - 5).ToString(), "GraveZone");
            return SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Area2: "Grave", Index2: (short)Module1.PlayerCurrentGrave.CountNumCards(), TheEvent: "Destroyed ST", Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name, returnOnly: returnOnly, bundle: bundle);
		}



		if (zone == 11) {
			if (string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
				return functionReturnValue;
			    Module1.PlayerCurrentGrave.Add(Module1.PlayerCurrentFSpell.toTrueStats());
             	setAsNothing(Module1.PlayerCurrentFSpell);

                FieldSpellZone.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].ID), "GraveZone", false, true);
                FieldSpellZone.animationTimer.setInMotion(FieldSpellZone, GraveZone, 300, 10, true, "FieldSpellZone");

                Module1.animationBundle bundle = createBundleToSend("FieldSpellZone", "FieldSpellZone", "GraveZone");
            return SummarizeJabber(Area1: "FSpell", Index1: 1, Area2: "Grave", Index2: (short)Module1.PlayerCurrentGrave.CountNumCards(), TheEvent: "Destroyed FSpell", Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name, returnOnly: returnOnly, bundle: bundle);
		
		}
		return functionReturnValue;

	}
private void Banish(int zone)
	{
        ZoneControl refer = null;

        if (string.IsNullOrEmpty(PlayerCurrentField(zone).Name))
            return;
        if (PlayerCurrentField(zone).Type.Contains("Token"))
        {
            setAsNothing(PlayerCurrentField(zone));
            refer = fieldZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
            SummarizeJabber(Area1: Module1.DeriveAreaFromGlobalZone(zone), Index1: Convert.ToInt16(Module1.DeriveIndexFromGlobalZone(zone)), TheEvent: "Destroyed " + Module1.DeriveAreaFromGlobalZone(zone), Text: "Token");
            return;
        }


		if (zone < 6) {

           DetatchAllXyzMaterial(zone);
		   Module1.PlayerCurrentRFG.Add(Module1.PlayerCurrentMonsters[zone].toTrueStats());
            setAsNothing(Module1.PlayerCurrentMonsters[zone]);

            MonZone(zone).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].ID), "rfgZone", false, true);
            MonZone(zone).animationTimer.setInMotion(MonZone(zone), rfgZone , 300, 10, true, "MonZone" + zone.ToString());

            Module1.animationBundle bundle = createBundleToSend("MonZone" + zone.ToString(), "MonZone" + zone.ToString(), "rfgZone");
            SummarizeJabber(Area1: "Monster", Index1: zone, Area2: "RFG", Index2: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NRFG", TheEvent: "Banished Monster", Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name, bundle: bundle);

		}

		if (zone > 5 && zone < 11) {
			
            Module1.PlayerCurrentRFG.Add(Module1.PlayerCurrentST[zone - 5].toTrueStats());
          	setAsNothing(Module1.PlayerCurrentST[zone - 5]);

            stZone(zone - 5).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].ID), "rfgZone", false, true);
            stZone(zone - 5).animationTimer.setInMotion(stZone(zone - 5), rfgZone, 300, 10, true, "stZone" + (zone - 5).ToString());

            Module1.animationBundle bundle = createBundleToSend("stZone" + (zone - 5).ToString(), "stZone" + (zone - 5).ToString(), "rfgZone");
            SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Area2: "RFG", Index2: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NRFG", TheEvent: "Banished ST", Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name, bundle: bundle);
		}



		if (zone == 11) {
			
			if (!string.IsNullOrEmpty(PlayerCurrentField(zone).Attribute)) {
			   Module1.PlayerCurrentRFG.Add(Module1.PlayerCurrentFSpell.toTrueStats());
            	setAsNothing(Module1.PlayerCurrentFSpell);

                FieldSpellZone.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].ID), "rfgZone", false, true);
                FieldSpellZone.animationTimer.setInMotion(FieldSpellZone, rfgZone, 300, 10, true, "FieldSpellZone");

                Module1.animationBundle bundle = createBundleToSend("FieldSpellZone", "FieldSpellZone", "rfgZone");
                SummarizeJabber(Area1:"FSpell", Index1:1, Area2:"RFG", Index2:(short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1:"NRFG", TheEvent:"Banished Field Spell", Text:Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name, bundle: bundle);
			}
		}

	}
private void Bounce(int zone)
	{
        ZoneControl refer = null;
        bool facedown;

        if (string.IsNullOrEmpty(PlayerCurrentField(zone).Name))
            return;
        if (PlayerCurrentField(zone).Type.Contains("Token"))
        {
            setAsNothing(PlayerCurrentField(zone));
            refer = fieldZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
            SummarizeJabber(Area1: Module1.DeriveAreaFromGlobalZone(zone), Index1: Convert.ToInt16(Module1.DeriveIndexFromGlobalZone(zone)), TheEvent: "Destroyed " + Module1.DeriveAreaFromGlobalZone(zone), Text: "Token");
            return;
        }

		if (zone < 6) {

            refer = MonZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
			DetatchAllXyzMaterial(zone);
            facedown = Module1.PlayerCurrentMonsters[zone].Facedown;
			if (Module1.BelongsInExtra(Module1.PlayerCurrentMonsters[zone])) {
				Module1.ReturnToExtra(Module1.PlayerCurrentMonsters[zone], "Field");
				setAsNothing(Module1.PlayerCurrentMonsters[zone]);
                if (facedown)
                    SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card");
                else
                    SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name);
            
            } else {
			     Module1.PlayerCurrentHand.Add(Module1.PlayerCurrentMonsters[zone].toTrueStats());
                lstMyHand.Items.Add(Module1.PlayerCurrentMonsters[zone].Name);
               
				setAsNothing(Module1.PlayerCurrentMonsters[zone]);
                Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(), "MonZone" + zone.ToString(), "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
				if  (facedown)
                    SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NHand", TheEvent: "Returned", Text:"a card", bundle: bundle);
                else
                    SummarizeJabber(Area1:"Monster", Index1:zone, Stat1:"NHand", TheEvent:"Returned", Text:Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name, bundle: bundle);

                addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
                updateImgHandPositions();
                double targetLeft = calculateDesirableImgHandPos(Module1.PlayerCurrentHand.CountNumCards());
                ZoneControl zImg = imgHand(Module1.PlayerCurrentHand.CountNumCards());
                zImg.animationTimer.setInMotion(MonZone(zone), new Point(targetLeft, imgHandTop), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
        
            }

          

          
		}

		if (zone > 5 && zone < 11) {
	
		    refer = stZone(zone - 5); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
            facedown = Module1.PlayerCurrentST[zone - 5].Facedown;
			if (Module1.BelongsInExtra(Module1.PlayerCurrentST[zone - 5])) {
				Module1.ReturnToExtra(Module1.PlayerCurrentST[zone - 5], "Field");
				setAsNothing(Module1.PlayerCurrentST[zone - 5]);
				if (facedown)
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card");
                else
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name);
            } else {
			   Module1.PlayerCurrentHand.Add(Module1.statsFromId(Module1.findID(Module1.PlayerCurrentST[zone - 5].Name)));
                lstMyHand.Items.Add(Module1.PlayerCurrentST[zone - 5].Name);
				setAsNothing(Module1.PlayerCurrentST[zone - 5]);
                Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(), "stZone" + (zone - 5).ToString(), "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
               
                
                if (facedown)
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NHand", TheEvent: "Returned", Text:"a card", bundle: bundle);
                else
                    SummarizeJabber(Area1:"ST", Index1:(short)(zone - 5), Stat1:"NHand", TheEvent:"Returned", Text:Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name, bundle: bundle);


                addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
                updateImgHandPositions();
                double targetLeft = calculateDesirableImgHandPos(Module1.PlayerCurrentHand.CountNumCards());
                ZoneControl zImg = imgHand(Module1.PlayerCurrentHand.CountNumCards());
                zImg.animationTimer.setInMotion(stZone(zone - 5), new Point(targetLeft, imgHandTop), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
       
            }


         
        }



		if (zone == 11) {

			if (!string.IsNullOrEmpty(PlayerCurrentField(zone).Attribute)) {
				Module1.setImage(ref FieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
                facedown = Module1.PlayerCurrentFSpell.Facedown;
                if (Module1.BelongsInExtra(Module1.PlayerCurrentFSpell))
                {
                    Module1.ReturnToExtra(Module1.PlayerCurrentFSpell, "Field");
					if (facedown)
                        SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card");
                    else
                        SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name);
                } else {
					Module1.PlayerCurrentHand.Add(Module1.statsFromId(Module1.findID(Module1.PlayerCurrentFSpell.Name)));
                    lstMyHand.Items.Add(Module1.PlayerCurrentFSpell.Name);
					setAsNothing(Module1.PlayerCurrentFSpell);

                    Module1.animationBundle bundle = createBundleToSend("imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString(), "FieldSpellZone", "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
                 
                    if (facedown)
                        SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NHand", TheEvent: "Returned", Text:"a card", bundle: bundle);
                    else
                        SummarizeJabber(Area1:"FSpell", Index1:1, Stat1:"NHand", TheEvent:"Returned", Text:Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].Name, bundle: bundle);

                    addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), true);
                    updateImgHandPositions();
                    double targetLeft = calculateDesirableImgHandPos(Module1.PlayerCurrentHand.CountNumCards());
                    ZoneControl zImg = imgHand(Module1.PlayerCurrentHand.CountNumCards());
                    zImg.animationTimer.setInMotion(FieldSpellZone, new Point(targetLeft, imgHandTop), 300, 10, false, "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString());
        
                
                
                }

                
			}
		}
       
       
      
        
	}

private void Spin(int zone)
	{
        ZoneControl refer = null;
        bool facedown;

        if (string.IsNullOrEmpty(PlayerCurrentField(zone).Name))
            return;
        if (PlayerCurrentField(zone).Type.Contains("Token"))
        {
            setAsNothing(PlayerCurrentField(zone));
            refer = fieldZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
            SummarizeJabber(Area1: Module1.DeriveAreaFromGlobalZone(zone), Index1: Convert.ToInt16(Module1.DeriveIndexFromGlobalZone(zone)), TheEvent: "Destroyed " + Module1.DeriveAreaFromGlobalZone(zone), Text: "Token");
            return;
        }

		if (zone < 6) {
			
         	DetatchAllXyzMaterial(zone);
            facedown = Module1.PlayerCurrentMonsters[zone].Facedown;
			if (Module1.BelongsInExtra(Module1.PlayerCurrentMonsters[zone])) {
				Module1.ReturnToExtra(Module1.PlayerCurrentMonsters[zone], "Field");
				setAsNothing(Module1.PlayerCurrentMonsters[zone]);

                MonZone(zone).setEndBehavior(0, "", false, true);
                MonZone(zone).animationTimer.setInMotion(MonZone(zone), ExtraDeckZone, 300, 10, true, "MonZone" + zone.ToString());

                Module1.animationBundle bundle = createBundleToSend("MonZone" + zone.ToString(), "MonZone" + zone.ToString(), "ExtraDeckZone");
                
              if (facedown)
                  SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card", bundle: bundle);
                else
                  SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name, bundle: bundle);
			} else {
				Module1.PlayerCurrentDeck.Add(Module1.PlayerCurrentMonsters[zone].toTrueStats());
				setAsNothing(Module1.PlayerCurrentMonsters[zone]);

                MonZone(zone).setEndBehavior(0, "", false, true);
                MonZone(zone).animationTimer.setInMotion(MonZone(zone), DeckZone, 300, 10, true, "MonZone" + zone.ToString());

                Module1.animationBundle bundle = createBundleToSend("MonZone" + zone.ToString(), "MonZone" + zone.ToString(), "DeckZone");
                if (facedown)
                    SummarizeJabber(Area1:"Monster", Index1:zone, Stat1: "NDeck", TheEvent:"Spin", Text:"a card", bundle: bundle);
			    else
                    SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NDeck", TheEvent: "Spin", Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name, bundle: bundle);
            }




		}

		if (zone > 5 && zone < 11) {
			
            facedown = Module1.PlayerCurrentST[zone - 5].Facedown;
         	if (Module1.BelongsInExtra(Module1.PlayerCurrentST[zone - 5])) {
				Module1.ReturnToExtra(Module1.PlayerCurrentST[zone - 5], "Field");
				setAsNothing(Module1.PlayerCurrentST[zone - 5]);

                stZone(zone - 5).setEndBehavior(0, "", false, true);
                stZone(zone - 5).animationTimer.setInMotion(stZone(zone - 5), ExtraDeckZone, 300, 10, true, "stZone" + (zone - 5).ToString());


                Module1.animationBundle bundle = createBundleToSend("stZone" + (zone - 5).ToString(), "stZone" + (zone - 5).ToString(), "ExtraDeckZone");

                if (facedown)
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card", bundle: bundle);
                else
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name, bundle: bundle);

			} else {
				//Module1.PlayerCurrentDeck.CountNumCards() = (short)(Module1.PlayerCurrentDeck.CountNumCards() + 1);
				//SQLReference.CardDetails tempstats = Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()]; Module1.copyCardDetails(ref tempstats , Module1.PlayerCurrentST[zone - 5]);
                Module1.PlayerCurrentDeck.Add(Module1.PlayerCurrentST[zone - 5].toTrueStats());
				setAsNothing(Module1.PlayerCurrentST[zone - 5]);

                stZone(zone - 5).setEndBehavior(0, "", false, true);
                stZone(zone - 5).animationTimer.setInMotion(stZone(zone - 5), DeckZone, 300, 10, true, "stZone" + (zone - 5).ToString());

                Module1.animationBundle bundle = createBundleToSend("stZone" + (zone - 5).ToString(), "stZone" + (zone-5).ToString(), "DeckZone");
                if (facedown)
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NDeck", TheEvent: "Spin", Text: "a card", bundle:bundle);
			    else
                    SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NDeck", TheEvent: "Spin", Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name, bundle: bundle);
             }
		}



		if (zone == 11) {
			
			if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Attribute)) {
                facedown = Module1.PlayerCurrentFSpell.Facedown;
                if (Module1.BelongsInExtra(Module1.PlayerCurrentFSpell))
                {
                    Module1.ReturnToExtra(Module1.PlayerCurrentFSpell, "Field");

                    FieldSpellZone.setEndBehavior(0, "", false, true);
                    FieldSpellZone.animationTimer.setInMotion(FieldSpellZone, ExtraDeckZone, 300, 10, true, "FieldSpellZone");

                    Module1.animationBundle bundle = createBundleToSend("FieldSpellZone", "FieldSpellZone", "ExtraDeckZone");
                    if (facedown)
                        SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card", bundle: bundle);
				    else
                        SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name, bundle: bundle); 
                } else {
                 
                    Module1.PlayerCurrentDeck.Add(Module1.PlayerCurrentFSpell.toTrueStats());
					setAsNothing(Module1.PlayerCurrentFSpell);

                    FieldSpellZone.setEndBehavior(0, "", false, true);
                    FieldSpellZone.animationTimer.setInMotion(FieldSpellZone, DeckZone, 300, 10, true, "FieldSpellZone");

                    Module1.animationBundle bundle = createBundleToSend("FieldSpellZone", "FieldSpellZone", "DeckZone");
                    if (facedown)
                        SummarizeJabber(Area1:"FSpell", Index1:1, Stat1: "NDeck", TheEvent:"Spin", Text:"a card", bundle: bundle);
				    else
                        SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NDeck", TheEvent: "Spin", Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name, bundle: bundle);
                }



			}
		}

	}
private void ToBottom(int zone)
{
    ZoneControl refer = null;
    bool facedown;

    if (string.IsNullOrEmpty(PlayerCurrentField(zone).Name))
        return;
    if (PlayerCurrentField(zone).Type.Contains("Token"))
    {
        setAsNothing(PlayerCurrentField(zone));
        refer = fieldZone(zone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
        SummarizeJabber(Area1: Module1.DeriveAreaFromGlobalZone(zone), Index1: Convert.ToInt16(Module1.DeriveIndexFromGlobalZone(zone)), TheEvent: "Destroyed " + Module1.DeriveAreaFromGlobalZone(zone), Text: "Token");
        return;
    }


    if (zone < 6)
    {
       
        DetatchAllXyzMaterial(zone);
        facedown = Module1.PlayerCurrentMonsters[zone].Facedown;
        if (Module1.BelongsInExtra(Module1.PlayerCurrentMonsters[zone]))
        {
            Module1.ReturnToExtra(Module1.PlayerCurrentMonsters[zone], "Field");
            setAsNothing(Module1.PlayerCurrentMonsters[zone]);

            MonZone(zone).setEndBehavior(0, "", false, true);
            MonZone(zone).animationTimer.setInMotion(MonZone(zone), ExtraDeckZone, 300, 10, true, "MonZone" + zone.ToString());

            Module1.animationBundle bundle = createBundleToSend("MonZone" + zone.ToString(), "MonZone" + zone.ToString(), "ExtraDeckZone");
            if (facedown)
                SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card", bundle: bundle);
            else
                SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name, bundle: bundle);
        }
        else
        {
            Module1.PlayerCurrentDeck.Insert(1, Module1.PlayerCurrentMonsters[zone].toTrueStats());
            setAsNothing(Module1.PlayerCurrentMonsters[zone]);

            MonZone(zone).setEndBehavior(0, "", false, true);
            MonZone(zone).animationTimer.setInMotion(MonZone(zone), DeckZone, 300, 10, true, "MonZone" + zone.ToString());

            Module1.animationBundle bundle = createBundleToSend("MonZone" + zone.ToString(), "MonZone" + zone.ToString(), "DeckZone");
            if (facedown)
                SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NDeck", TheEvent: "To Bottom", Text: "a card", bundle: bundle);
            else
                SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NDeck", TheEvent: "To Bottom", Text: Module1.PlayerCurrentDeck[1].Name, bundle: bundle);
        }




    }

    if (zone > 5 && zone < 11)
    {
     
        facedown = Module1.PlayerCurrentST[zone - 5].Facedown;
        if (Module1.BelongsInExtra(Module1.PlayerCurrentST[zone - 5]))
        {
            Module1.ReturnToExtra(Module1.PlayerCurrentST[zone - 5], "Field");
            setAsNothing(Module1.PlayerCurrentST[zone - 5]);

            stZone(zone - 5).setEndBehavior(0, "", false, true);
            stZone(zone - 5).animationTimer.setInMotion(stZone(zone - 5), ExtraDeckZone, 300, 10, true, "stZone" + (zone - 5).ToString());

            Module1.animationBundle bundle = createBundleToSend("stZone" + (zone - 5).ToString(), "stZone" + (zone - 5).ToString(), "ExtraDeckZone");
            if (facedown)
                SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card", bundle: bundle);
            else
                SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name, bundle: bundle);

        }
        else
        {
          
            Module1.PlayerCurrentDeck.Insert(1, Module1.PlayerCurrentST[zone - 5].toTrueStats());
            setAsNothing(Module1.PlayerCurrentST[zone - 5]);

            stZone(zone - 5).setEndBehavior(0, "", false, true);
            stZone(zone - 5).animationTimer.setInMotion(stZone(zone - 5), DeckZone, 300, 10, true, "stZone" + (zone - 5).ToString());

            Module1.animationBundle bundle = createBundleToSend("stZone" + (zone - 5).ToString(), "stZone" + (zone - 5).ToString(), "DeckZone");
            if (facedown)
                SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NDeck", TheEvent: "To Bottom", Text: "a card", bundle: bundle);
            else
                SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), Stat1: "NDeck", TheEvent: "To Bottom", Text: Module1.PlayerCurrentDeck[1].Name, bundle: bundle);
        }
    }



    if (zone == 11)
    {
        
        if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Attribute))
        {
            facedown = Module1.PlayerCurrentFSpell.Facedown;
            if (Module1.BelongsInExtra(Module1.PlayerCurrentFSpell))
            {
                Module1.ReturnToExtra(Module1.PlayerCurrentFSpell, "Field");

                FieldSpellZone.setEndBehavior(0, "", false, true);
                FieldSpellZone.animationTimer.setInMotion(FieldSpellZone, ExtraDeckZone, 300, 10, true, "FieldSpellZone");

                Module1.animationBundle bundle = createBundleToSend("FieldSpellZone", "FieldSpellZone", "ExtraDeckZone");
                if (facedown)
                    SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: "a card", bundle: bundle);
                else
                    SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NEDeck", TheEvent: "Returned to Extra Deck from Field", Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name, bundle: bundle);
            }
            else
            {
               
                Module1.PlayerCurrentDeck.Insert(1, Module1.PlayerCurrentFSpell.toTrueStats());
                setAsNothing(Module1.PlayerCurrentFSpell);

                FieldSpellZone.setEndBehavior(0, "", false, true);
                FieldSpellZone.animationTimer.setInMotion(FieldSpellZone, DeckZone, 300, 10, true, "FieldSpellZone");

                Module1.animationBundle bundle = createBundleToSend("FieldSpellZone", "FieldSpellZone", "DeckZone");
                if (facedown)
                    SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NDeck", TheEvent: "To Bottom", Text: "a card", bundle: bundle);
                else
                    SummarizeJabber(Area1: "FSpell", Index1: 1, Stat1: "NDeck", TheEvent: "To Bottom", Text: Module1.PlayerCurrentDeck[1].Name, bundle: bundle);
            }



        }
    }

}
public void SummonOrActivateFromHand(int index)
{
    ZoneControl zImg = imgHand(index);

    if (index == 0 || index > Module1.PlayerCurrentHand.CountNumCards())
        return;
    List<string> jabberList = new List<string>();
    //Spell / Trap
    if (!Module1.PlayerCurrentHand[index].IsMonster())
    {
        //Field Spell
        if (Module1.PlayerCurrentHand[index].Type == "Field")
        {
            if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
            {
                jabberList.Add(Destroy(11, true));
            }

            zImg.setEndBehavior(Module1.PlayerCurrentHand[index], "FieldSpellZone", true, false);
            zImg.animationTimer.setInMotion(zImg, FieldSpellZone, 300, 10, false, "imgHand" + index.ToString());

            Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.PlayerCurrentHand[index]);
            Module1.PlayerCurrentHand.RemoveAt(index);

            lstMyHand.Items.RemoveAt(index - 1);

            Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "FieldSpellZone");

            jabberList.Add(SummarizeJabber(Area1: "FSpell", Stat1: "NHand", TheEvent: "Played", Text: Module1.PlayerCurrentFSpell.Name, bundle: bundle));
            if (jabberList.Count > 1)
            {
                SendJabber(combineMultipleJabbers(jabberList));
            }
            else
            {
                SendJabber(jabberList[0]);
            }

        }
        else
        {
            short zone = FindEmptySTZone();
            if (zone == 6)
                return;

            zImg.setEndBehavior(Module1.PlayerCurrentHand[index], "stZone" + zone.ToString(), true, false);
            zImg.animationTimer.setInMotion(zImg, stZone(zone), 300, 10, false, "imgHand" + index.ToString());

            Module1.copyCardDetails(ref Module1.PlayerCurrentST[zone], Module1.PlayerCurrentHand[index]);
            Module1.PlayerCurrentHand.RemoveAt(index);

            lstMyHand.Items.RemoveAt(index - 1);

            Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "stZone" + zone.ToString());
            SummarizeJabber(Area1: "ST", Index1: zone, Stat1: "NHand", TheEvent: "Played", Text: Module1.PlayerCurrentST[zone].Name, bundle: bundle);

        }

        //Monster
    }
    else
    {
        short zone = FindEmptyMonZone();
        if (zone == 6)
            return;

        Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[zone], Module1.PlayerCurrentHand[index]);
        zImg.setEndBehavior(Module1.PlayerCurrentHand[index], "MonZone" + zone.ToString(), true, false);
        zImg.animationTimer.setInMotion(zImg, MonZone(zone), 300, 10, false, "imgHand" + index.ToString());

        Module1.PlayerCurrentHand.RemoveAt(index);

        lstMyHand.Items.RemoveAt(index - 1);
        Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "MonZone" + zone.ToString());
        SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NHand", TheEvent: "Played", Text: Module1.PlayerCurrentMonsters[zone].Name, bundle: bundle);
    }



}
public void SetFromHand(int index, bool monster)
{
    ZoneControl zImg = imgHand(index);
    if (index == 0 || index > Module1.PlayerCurrentHand.CountNumCards())
        return;
    List<string> jabberList = new List<string>();
    Module1.PlayerCurrentHand.ModifyCardInList(index, "Facedown", true);
    //Spell / Trap
    if (!monster) //string.IsNullOrEmpty(Module1.PlayerCurrentHand[index].ATK))
    {
        //Field Spell
        if (!Module1.PlayerCurrentHand[index].IsMonster() && Module1.PlayerCurrentHand[index].Type == "Field")
        {
            if (!string.IsNullOrEmpty(Module1.PlayerCurrentFSpell.Name))
            {
                jabberList.Add(Destroy(11, true));
            }
            zImg.setEndBehavior(Module1.PlayerCurrentHand[index], "FieldSpellZone", true, false);
            zImg.animationTimer.setInMotion(zImg, FieldSpellZone, 300, 10, false, "imgHand" + index.ToString());
            Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "FieldSpellZone");
            Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.PlayerCurrentHand[index]);

            Module1.PlayerCurrentHand.RemoveAt(index);
            lstMyHand.Items.RemoveAt(index - 1);

            jabberList.Add(SummarizeJabber(Area1: "FSpell", Stat1: "NHand", TheEvent: "Set", Text: Module1.PlayerCurrentFSpell.Name, returnOnly: true, bundle: bundle));
            if (jabberList.Count > 1)
            {
                SendJabber(combineMultipleJabbers(jabberList));
            }
            else
            {
                SendJabber(jabberList[0]);
            }
        }
        else
        {
            short zone = FindEmptySTZone();
            if (zone == 6)
                return;
            zImg.setEndBehavior(Module1.PlayerCurrentHand[index], "stZone" + zone.ToString(), true, false);
            zImg.animationTimer.setInMotion(zImg, stZone(zone), 300, 10, false, "imgHand" + index.ToString());

            Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "stZone" + zone.ToString());
            Module1.copyCardDetails(ref Module1.PlayerCurrentST[zone], Module1.PlayerCurrentHand[index]);

            Module1.PlayerCurrentHand.RemoveAt(index);
            lstMyHand.Items.RemoveAt(index - 1);

            SummarizeJabber(Area1: "ST", Index1: zone, Stat1: "NHand", TheEvent: "Set", Text: Module1.PlayerCurrentST[zone].Name, bundle: bundle);

        }
        //Monster
    }
    else
    {
        short zone = FindEmptyMonZone();
        if (zone == 6)
            return;

        Module1.PlayerCurrentHand.ModifyCardInList(index, "IsItHorizontal", true);

        zImg.setEndBehavior(Module1.PlayerCurrentHand[index], "MonZone" + zone.ToString(), true, false);
        zImg.animationTimer.setInMotion(zImg, MonZone(zone), 300, 10, false, "imgHand" + index.ToString());
        Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "MonZone" + zone.ToString());
        Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[zone], Module1.PlayerCurrentHand[index]);

        Module1.PlayerCurrentHand.RemoveAt(index);
        lstMyHand.Items.RemoveAt(index - 1);

        SummarizeJabber(Area1: "Monster", Index1: zone, Stat1: "NHand", TheEvent: "Set", Text: Module1.PlayerCurrentMonsters[zone].Name, bundle: bundle);
    }


}
public void Attack(int opZone)
{
    imgBattle.Visibility = System.Windows.Visibility.Collapsed;
    imgBattleOrigin.Visibility = System.Windows.Visibility.Collapsed;
    if (opZone == 0)
    {
        SummarizeJabber(TheEvent: "Attacking Directly", Text: goingToAttackZone.ToString());
    }
    else
    {
        SummarizeJabber(TheEvent: "Attacking", Text: opZone.ToString() + "|" + goingToAttackZone.ToString());
    }

    goingToAttackZone = 0;
}
public void Move(int newZone)
{
    ZoneControl refer = null;
    switch (goingToMoveZone)
    {
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
            if (newZone >= 1 && newZone <= 5)
            {
                if (!string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[newZone].Name)) //Overlay
                {
                    //if (Module1.PlayerCurrentMonsters[goingToMoveZone].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                    if (Module1.PlayerCurrentMonsters[goingToMoveZone].Type.Contains("Token"))
                    {
                        setAsNothing(Module1.PlayerCurrentMonsters[goingToMoveZone]);
                        refer = MonZone(goingToMoveZone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                        SummarizeJabber(Area1: "Monster", Index1: goingToMoveZone, TheEvent: "Destroyed Monster", Text: "Token");
                        return;
                    }
                    AttachXyzMaterial(newZone, goingToMoveZone);
                }
                else //Move
                {
                    Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[newZone], Module1.PlayerCurrentMonsters[goingToMoveZone]);
                    refer = MonZone(newZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentMonsters[newZone], Module1.mySet);
                    refer = MonZone(goingToMoveZone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                    setAsNothing(Module1.PlayerCurrentMonsters[goingToMoveZone]);
                    MoveXyz(goingToMoveZone, newZone);
                    SummarizeJabber(Area1: "Monster", Index1: goingToMoveZone, Area2: "Monster", Index2: newZone, TheEvent: "Moved Monster Zone", Text: Convert.ToString(newZone));
                    SummarizeJabber(Area1: "Over", Index1: goingToMoveZone, Area2: "Over", Index2: newZone);
                    resetXyzLayout();
                }
            }

            if (newZone >= 6 && newZone <= 10)
            {

                DetatchAllXyzMaterial(goingToMoveZone);
                Module1.copyCardDetails(ref Module1.PlayerCurrentST[newZone - 5], Module1.PlayerCurrentMonsters[goingToMoveZone]);
                refer = stZone(newZone - 5); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentST[newZone - 5], Module1.mySet);
                refer = MonZone(goingToMoveZone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                setAsNothing(Module1.PlayerCurrentMonsters[goingToMoveZone]);
                if (Module1.PlayerCurrentST[newZone - 5].Facedown == false)
                {
                    SummarizeJabber(Area1: "Monster", Index1: goingToMoveZone, Area2: "ST", Index2: (short)(newZone - 5), TheEvent: "Moved Monster to ST Zone", Text: Module1.PlayerCurrentST[newZone - 5].Name);
                }
                else
                {
                    SummarizeJabber(Area1: "Monster", Index1: goingToMoveZone, Area2: "ST", Index2: (short)(newZone - 5), TheEvent: "Moved Monster to ST Zone", Text: "");
                }
            }
            if (newZone == 11)
            {
                DetatchAllXyzMaterial(goingToMoveZone);
                Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.PlayerCurrentMonsters[goingToMoveZone]);
                UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);
                refer = MonZone(goingToMoveZone); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                setAsNothing(Module1.PlayerCurrentMonsters[goingToMoveZone]);
                if (Module1.PlayerCurrentFSpell.Facedown == false)
                {
                    SummarizeJabber(Area1: "Monster", Index1: goingToMoveZone, Area2: "FSpell", TheEvent: "Moved Monster to Field Spell Zone", Text: Module1.PlayerCurrentFSpell.Name);
                }
                else
                {
                    SummarizeJabber(Area1: "Monster", Index1: goingToMoveZone, Area2: "FSpell", TheEvent: "Moved Monster to Field Spell Zone", Text: "");
                }
            }
            break;
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
            if (newZone >= 1 && newZone <= 5)
            {
                if (!string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[newZone].Name)) //Overlay
                {
                    if (Module1.PlayerCurrentST[goingToMoveZone - 5].OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                    AttachXyzMaterial(newZone, goingToMoveZone);
                }
                else //Move
                {
                    Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[newZone], Module1.PlayerCurrentST[goingToMoveZone - 5]);
                    refer = MonZone(newZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentMonsters[newZone], Module1.mySet);
                    refer = stZone(goingToMoveZone - 5); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                    setAsNothing(Module1.PlayerCurrentST[goingToMoveZone - 5]);

                    if (Module1.PlayerCurrentMonsters[newZone].Facedown == false)
                    {
                        SummarizeJabber(Area1: "ST", Index1: (short)(goingToMoveZone - 5), Area2: "Monster", Index2: newZone, TheEvent: "Moved ST to Monster Zone", Text: Module1.PlayerCurrentMonsters[newZone].Name);
                    }
                    else
                    {
                        SummarizeJabber(Area1: "ST", Index1: (short)(goingToMoveZone - 5), Area2: "Monster", Index2: newZone, TheEvent: "Moved ST to Monster Zone", Text: "");
                    }
                }
                resetXyzLayout();
            }

            if (newZone >= 6 && newZone <= 10)
            {
                Module1.copyCardDetails(ref Module1.PlayerCurrentST[newZone - 5], Module1.PlayerCurrentST[goingToMoveZone - 5]);
                refer = stZone(newZone - 5); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentST[newZone - 5], Module1.mySet);
                refer = stZone(goingToMoveZone - 5); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                setAsNothing(Module1.PlayerCurrentST[goingToMoveZone - 5]);

                SummarizeJabber(Area1: "ST", Index1: (short)(goingToMoveZone - 5), Area2: "ST", Index2: (short)(newZone - 5), TheEvent: "Moved ST Zone", Text: Convert.ToString(newZone - 5));

            }
            if (newZone == 11)
            {
                Module1.copyCardDetails(ref Module1.PlayerCurrentFSpell, Module1.PlayerCurrentST[goingToMoveZone - 5]);
                UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);
                refer = stZone(goingToMoveZone - 5); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                setAsNothing(Module1.PlayerCurrentST[goingToMoveZone - 5]);
                if (Module1.PlayerCurrentFSpell.Facedown == false)
                {
                    SummarizeJabber(Area1: "ST", Index1: (short)(goingToMoveZone - 5), Area2: "FSpell", TheEvent: "Moved ST to Field Spell Zone", Text: Module1.PlayerCurrentFSpell.Name);
                }
                else
                {
                    SummarizeJabber(Area1: "ST", Index1: (short)(goingToMoveZone - 5), Area2: "FSpell", TheEvent: "Moved ST to Field Spell Zone", Text: "");
                }
            }
            break;
        case 11:
            if (newZone >= 1 && newZone <= 5)
            {
                if (!string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[newZone].Name)) //Overlay
                {
                    if (Module1.PlayerCurrentFSpell.OpponentOwned) { MsgBox("You can't do this action on a card your opponent owns."); return; }
                    AttachXyzMaterial(newZone, goingToMoveZone);
                }
                else //Move
                {
                    Module1.copyCardDetails(ref Module1.PlayerCurrentMonsters[newZone], Module1.PlayerCurrentFSpell);
                    refer = MonZone(newZone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentMonsters[newZone], Module1.mySet);
                    Module1.setImage(ref FieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
                    setAsNothing(Module1.PlayerCurrentFSpell);
                    if (Module1.PlayerCurrentMonsters[newZone].Facedown == false)
                    {
                        SummarizeJabber(Area1: "FSpell", Area2: "Monster", Index2: newZone, TheEvent: "Moved Field Spell to Monster Zone", Text: Module1.PlayerCurrentMonsters[newZone].Name);
                    }
                    else
                    {
                        SummarizeJabber(Area1: "FSpell", Area2: "Monster", Index2: newZone, TheEvent: "Moved Field Spell to Monster Zone", Text: "");
                    }
                }
            }

            if (newZone >= 6 && newZone <= 10)
            {
                Module1.copyCardDetails(ref Module1.PlayerCurrentST[newZone - 5], Module1.PlayerCurrentFSpell);
                refer = stZone(newZone - 5); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentST[newZone - 5], Module1.mySet);
                Module1.setImage(ref FieldSpellZone.baseImage, BLANK_IMAGE, UriKind.Relative);
                setAsNothing(Module1.PlayerCurrentFSpell);
                if (Module1.PlayerCurrentST[newZone - 5].Facedown == false)
                {
                    SummarizeJabber(Area1: "FSpell", Area2: "ST", Index2: (short)(newZone - 5), TheEvent: "Moved Field Spell to ST Zone", Text: Module1.PlayerCurrentST[newZone - 5].Name);
                }
                else
                {
                    SummarizeJabber(Area1: "FSpell", Area2: "ST", Index2: (short)(newZone - 5), TheEvent: "Moved Field Spell to ST Zone", Text: "");
                }
            }

            break;
    }
    goingToMoveZone = 0;
    imgMove.Visibility = System.Windows.Visibility.Collapsed;
    imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
}
public void summonToken(int zone)
{
    ZoneControl refer = null;
    Module1.PlayerCurrentMonsters[zone].Name = "Token";
    Module1.PlayerCurrentMonsters[zone].ID = 0;
    Module1.PlayerCurrentMonsters[zone].Level = 1;
    Module1.PlayerCurrentMonsters[zone].ATK = 0;
    Module1.PlayerCurrentMonsters[zone].DEF = 0;
    Module1.PlayerCurrentMonsters[zone].Lore = "This is a token";
    Module1.PlayerCurrentMonsters[zone].Type = "Token";
    Module1.PlayerCurrentMonsters[zone].Attribute = "Light";
    Module1.PlayerCurrentMonsters[zone].IsItHorizontal = true;
    refer = MonZone(zone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentMonsters[zone], Module1.mySet);

    SummarizeJabber(Area1: "Monster", Index1: zone, TheEvent: "Summon Token");
}
public void Mill(bool andBanish)
{
    if (Module1.PlayerCurrentDeck.CountNumCards() == 0)
        return;
    if (andBanish)
    {
        Module1.PlayerCurrentRFG.Add(Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].toTrueStats());
        Module1.PlayerCurrentDeck.RemoveAt(Module1.PlayerCurrentDeck.CountNumCards());
        UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], Module1.mySet);
        SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NRFG", Stat2: "NDeck", TheEvent: "Banish Top Card", Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);


    }
    else
    {
        Module1.PlayerCurrentGrave.Add(Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].toTrueStats());
        Module1.PlayerCurrentDeck.RemoveAt(Module1.PlayerCurrentDeck.CountNumCards());
        UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet);
        SummarizeJabber(Area1: "Grave", Index1: (short)Module1.PlayerCurrentGrave.CountNumCards(), Stat1: "NGrave", Stat2: "NDeck", TheEvent: "Mill Top Card", Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name);
    }
}
public void Discard(int index, bool random)
{
    if (index > Module1.PlayerCurrentHand.CountNumCards())
        return;
    Module1.PlayerCurrentGrave.Add(Module1.PlayerCurrentHand[index]);
    Module1.PlayerCurrentHand.RemoveAt(index);
    lstMyHand.Items.RemoveAt(index - 1);

    imgHand(index).setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].ID), "GraveZone", true, false);
    imgHand(index).animationTimer.setInMotion(imgHand(index), GraveZone, 300, 10, false, "imgHand" + index.ToString());

    Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "GraveZone");


    if (random)
    {
        SummarizeJabber(Area1: "Grave", Index1: (short)Module1.PlayerCurrentGrave.CountNumCards(), Stat1: "NHand", Stat2: "NGrave", TheEvent: "Discarded at Random", Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name, bundle: bundle);
    }
    else
    {
        SummarizeJabber(Area1: "Grave", Index1: (short)Module1.PlayerCurrentGrave.CountNumCards(), Stat1: "NHand", Stat2: "NGrave", TheEvent: "Discarded", Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name, bundle: bundle);
    }

}
public void BanishFromHand(int index, bool facedown)
{
    if (index > Module1.PlayerCurrentHand.CountNumCards())
        return;
    Module1.PlayerCurrentRFG.Add(Module1.PlayerCurrentHand[index]);
    Module1.PlayerCurrentHand.RemoveAt(index);
    Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Facedown = facedown;

    lstMyHand.Items.RemoveAt(index - 1);

    // Set facedown (-1 is back.jpg) as RFG zone's image if facedown, otherwise the id of the card
    imgHand(index).setEndBehavior(facedown ? -1 : Module1.findIndexFromTrueID(Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].ID), "rfgZone", true, false);
    imgHand(index).animationTimer.setInMotion(imgHand(index), rfgZone, 300, 10, false, "imgHand" + index.ToString());
    Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "rfgZone");
    if (facedown)
        SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NHand", TheEvent: "Banished Card in Hand Facedown", bundle: bundle);
    else
       SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NHand", TheEvent: "Banished Card in Hand", Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name, bundle: bundle);

}
public void HandToTop(int index)
{

    Module1.PlayerCurrentDeck.Add(Module1.PlayerCurrentHand[index]);
    Module1.PlayerCurrentHand.RemoveAt(index);
    lstMyHand.Items.RemoveAt(index - 1);

    imgHand(index).setEndBehavior(-1, "DeckZone", true, false);
    Module1.setImage(ref imgHand(index).baseImage, "back.jpg", UriKind.Relative);
    imgHand(index).animationTimer.setInMotion(imgHand(index), DeckZone, 300, 10, false, "imgHand" + index.ToString());
    Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "DeckZone");
    SummarizeJabber(Stat1: "NHand", Stat2: "NDeck", TheEvent: "Returned to Top of Deck", bundle: bundle);

}
public void HandToBottom(int index)
{
    Module1.PlayerCurrentDeck.Insert(1, Module1.PlayerCurrentHand[index]);
    Module1.PlayerCurrentHand.RemoveAt(index);
   
    lstMyHand.Items.RemoveAt(index - 1);

    imgHand(index).setEndBehavior(-1, "DeckZone", true, false);
    Module1.setImage(ref imgHand(index).baseImage, "back.jpg", UriKind.Relative);
    imgHand(index).animationTimer.setInMotion(imgHand(index), DeckZone, 300, 10, false, "imgHand" + index.ToString());
    Module1.animationBundle bundle = createBundleToSend("imgHand" + index.ToString(), "imgHand" + index.ToString(), "DeckZone");
    SummarizeJabber(Stat1: "NHand", Stat2: "NDeck", TheEvent: "Returned to Bottom of Deck", bundle: bundle);
}
public void FlipFaceDown(int zone)
	{
        ZoneControl refer = null;
		if (zone < 6) {
			Module1.PlayerCurrentMonsters[zone].Facedown = true;
			Module1.PlayerCurrentMonsters[zone].IsItHorizontal = true;
            refer = MonZone(zone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[zone], Module1.mySet);
			SummarizeJabber(Area1:"Monster", Index1:zone, TheEvent:"Flipped Monster Face Down");
		} else if (zone > 5 && zone < 11) {
			Module1.PlayerCurrentST[zone - 5].Facedown = true;
            refer = stZone(zone - 5); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[zone - 5], Module1.mySet);
			SummarizeJabber(Area1:"ST", Index1:(short)(zone - 5), TheEvent:"Flipped ST Face Down");
		} else if (zone == 11) {
			Module1.PlayerCurrentFSpell.Facedown = true;
            UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);
			SummarizeJabber(Area1:"FSpell", TheEvent:"Flipped ST Face Down");
		}

	}
public void FlipFaceUp(int zone)
{
    ZoneControl refer = null;
    if (zone < 6)
    {
        Module1.PlayerCurrentMonsters[zone].Facedown = false;
        refer = MonZone(zone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentMonsters[zone], Module1.mySet);
        SummarizeJabber(Area1: "Monster", Index1: zone, TheEvent: "Flipped Monster Face Up");
    }
    else if (zone > 5 && zone < 11)
    {
        Module1.PlayerCurrentST[zone - 5].Facedown = false;
        refer = stZone(zone - 5); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name, Module1.PlayerCurrentST[zone - 5], Module1.mySet);
        SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), TheEvent: "Flipped ST Face Up");
    }
    else if (zone == 11)
    {
        Module1.PlayerCurrentFSpell.Facedown = true;
        UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);
        SummarizeJabber(Area1: "FSpell", TheEvent: "Flipped ST Face Up");
    }

}
public void ChangePositionOrActivate(int zone)
	{
        ZoneControl refer = null;
		if (zone < 6) {
			//facedown DEF
			if (Module1.PlayerCurrentMonsters[zone].Facedown) {
				Module1.PlayerCurrentMonsters[zone].Facedown = false;
				Module1.PlayerCurrentMonsters[zone].IsItHorizontal = false;
			//faceup DEF
			} else if (Module1.PlayerCurrentMonsters[zone].IsItHorizontal) {
				Module1.PlayerCurrentMonsters[zone].IsItHorizontal = false;
			//Faceup ATK
			} else {
				Module1.PlayerCurrentMonsters[zone].IsItHorizontal = true;
			}
            refer = MonZone(zone); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentMonsters[zone], Module1.mySet);
			SummarizeJabber(Area1:"Monster", Index1:zone,TheEvent: "Switched Position",Text: Module1.PlayerCurrentMonsters[zone].Name);
		} else if (zone > 5 && zone < 11) {
			Module1.PlayerCurrentST[zone - 5].Facedown = false;
            refer = stZone(zone - 5); UpdatePictureBoxDuelField(ref refer.baseImage, refer.Name,Module1.PlayerCurrentST[zone - 5], Module1.mySet);
            string stIconString;
            if (!Module1.PlayerCurrentST[zone - 5].IsMonster() && string.IsNullOrEmpty(Module1.PlayerCurrentST[zone - 5].Type))
                stIconString = "Normal";
            else
                stIconString = Module1.PlayerCurrentST[zone - 5].Type;
            SummarizeJabber(Area1: "ST", Index1: (short)(zone - 5), TheEvent: "Activated ST", Text: Module1.PlayerCurrentST[zone - 5].Name + "(" + stIconString + ")");
		} else if (zone == 11) {
			Module1.PlayerCurrentFSpell.Facedown = false;
            UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);
			SummarizeJabber(Area1:"FSpell",TheEvent:"Activated ST",Text: Module1.PlayerCurrentFSpell.Name);
		}
	}
public void DetatchXyzMaterial(int Zone, int MaterialIndex)
	{
		if (string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[Zone].Name))
			return;
		if (Module1.PlayerOverlaid[Zone][MaterialIndex] == null)
			return;
	      Module1.PlayerCurrentGrave.Add(Module1.PlayerOverlaid[Zone][MaterialIndex]);
          Module1.PlayerOverlaid[Zone].RemoveAt(MaterialIndex);

        ZoneControl zImg = imgXyz(Zone, MaterialIndex);
       zImg.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].ID), "GraveZone", true, false);
        zImg.animationTimer.setInMotion(zImg, GraveZone, 300, 10, false, zImg.Name);

        Module1.animationBundle bundle = createBundleToSend(
            "XyzMat_" + Zone.ToString() + "_" + MaterialIndex.ToString(),
            "XyzMat_" + Zone.ToString() + "_" + MaterialIndex.ToString(),
            "GraveZone");

       		SummarizeJabber(Area1:"Over", Index1:Zone, Area2:"Grave", Index2:(short)Module1.PlayerCurrentGrave.CountNumCards(), TheEvent:"Detatch Material", Text:Module1.PlayerCurrentMonsters[Zone].Name, bundle: bundle);
	}
public void updateXyzPosition(ZoneControl zImg, int materialIndex)
{
   zImg.RenderTransform.SetValue(TranslateTransform.XProperty, (double)(materialIndex * 10));
    Canvas.SetZIndex(zImg, materialIndex * -1);
}
public void DetatchAllXyzMaterial(int Zone)
	{
       if (string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[Zone].Name))
			return;
       bool atLeastOne = false;
		while (Module1.PlayerOverlaid[Zone].CountNumCards() > 0)
        {
			
                atLeastOne = true;
	

                Module1.PlayerCurrentGrave.Add(Module1.PlayerOverlaid[Zone][Module1.PlayerOverlaid[Zone].CountNumCards()]);
		
				
                ZoneControl zImg = imgXyz(Zone, (short)Module1.PlayerOverlaid[Zone].CountNumCards());
                Module1.animationBundle bundle = createBundleToSend(
                 "XyzMat_" + Zone.ToString() + "_" + Module1.PlayerOverlaid[Zone].CountNumCards().ToString(),
                 "XyzMat_" + Zone.ToString() + "_" + Module1.PlayerOverlaid[Zone].CountNumCards().ToString(),
                 "GraveZone");


                Module1.PlayerOverlaid[Zone].RemoveAt(Module1.PlayerOverlaid[Zone].CountNumCards());

                zImg.setEndBehavior(Module1.findIndexFromTrueID(Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].ID), "GraveZone", true, false);
                zImg.animationTimer.setInMotion(zImg, GraveZone, 300, 10, false, zImg.Name);

            
            
            SummarizeJabber(Area1: "Over", Index1: Zone, Area2: "Grave", Index2: (short)Module1.PlayerCurrentGrave.CountNumCards(), bundle: bundle); 
			//}
		}
      
      // if (atLeastOne) { SummarizeJabber(Area1: "Over", Index1: Zone, Area2: "Grave", Index2:(short)Module1.PlayerCurrentGrave.CountNumCards()); }

	}
public void AttachXyzMaterial(int Zone, int ZoneofMaterial)
	{
        ZoneControl refer = null;
		if (string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[Zone].Name))
			return;
        if (Module1.PlayerOverlaid[Zone].CountNumCards() == 5)
        {
            MsgBox("Sorry, only 5 monsters may be overlaid at a time.");
            return;
        }
		//short n = 0;
		//for (n = 1; n <= Module1.; n++) {
		//	if (string.IsNullOrEmpty(Module1.PlayerOverlaid[Zone, n]) || Module1.PlayerOverlaid[Zone, n] == null) {
                if (ZoneofMaterial >= 1 && ZoneofMaterial <= 5)
                {
                    Module1.PlayerOverlaid[Zone].Add(Module1.PlayerCurrentMonsters[ZoneofMaterial].toTrueStats());
                    DetatchAllXyzMaterial(ZoneofMaterial);
                    addXyzImg(Zone, Module1.PlayerOverlaid[Zone].CountNumCards());
                    setAsNothing(Module1.PlayerCurrentMonsters[ZoneofMaterial]);
                    refer = MonZone(ZoneofMaterial); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                    SummarizeJabber(Area1: "Monster", Index1: ZoneofMaterial, Area2: "Over", Index2: Zone, TheEvent: "Attach Material", Text: Module1.PlayerCurrentMonsters[Zone].Name);
                    
                   // break;
                }
                else if (ZoneofMaterial >= 6 && ZoneofMaterial <= 10)
                {
                    Module1.PlayerOverlaid[Zone].Add(Module1.PlayerCurrentST[ZoneofMaterial - 5].toTrueStats());
                    addXyzImg(Zone, Module1.PlayerOverlaid[Zone].CountNumCards());
                   
                    setAsNothing(Module1.PlayerCurrentST[ZoneofMaterial - 5]);
                    refer = stZone(ZoneofMaterial - 5); Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                    SummarizeJabber(Area1: "ST", Index1: (short)(ZoneofMaterial - 5), Area2: "Over", Index2: Zone, TheEvent: "Attach Material", Text: Module1.PlayerCurrentMonsters[Zone].Name);
                   // break;
                }
                else if (ZoneofMaterial == 11)
                {
                    Module1.PlayerOverlaid[Zone].Add(Module1.PlayerCurrentFSpell.toTrueStats());
                    addXyzImg(Zone, (short)Module1.PlayerOverlaid[Zone].CountNumCards());
                   
                    setAsNothing(Module1.PlayerCurrentFSpell);
                    
                    refer = FieldSpellZone; Module1.setImage(ref refer.baseImage, BLANK_IMAGE, UriKind.Relative);
                    SummarizeJabber(Area1: "FSpell", Area2: "Over", Index2: Zone, TheEvent: "Attach Material", Text: Module1.PlayerCurrentMonsters[Zone].Name);
                   // break;
                }


	
	}
private void GiveControl(int zone)
{
    DetatchAllXyzMaterial(zone);
    if (Module1.PlayerCurrentMonsters[zone].OpponentOwned == false)
    { SummarizeJabber(TheEvent: "Give Control", Text: Convert.ToString(zone)); }
    else { SummarizeJabber(TheEvent: "Give Back Control", Text: Convert.ToString(zone)); }
    ZoneofSwitchmonster = zone;

}
private void GiveControlST(int zone)
{
    if (zone == 6)
    {
        if (Module1.PlayerCurrentFSpell.OpponentOwned == false)
            SummarizeJabber(TheEvent: "Give Control ST", Text: "0");
        else
            SummarizeJabber(TheEvent: "Give Back Control ST", Text: "0");
    }
    else
    {
        if (Module1.PlayerCurrentST[zone].OpponentOwned == false)
            SummarizeJabber(TheEvent: "Give Control ST", Text: Convert.ToString(zone));
        else
            SummarizeJabber(TheEvent: "Give Back Control ST", Text: Convert.ToString(zone));
    }
    ZoneofSwitchST = zone;

}
#endregion
      


#region FormEvents
private void viewForm_Closed(object sender, EventArgs e)
	{
        //LayoutRoot.Children.Remove(viewForm);
		string apassalong = null;
		if (!string.IsNullOrEmpty(Module1.LogViewAction)) {
            if (Module1.PlayerCurrentGrave.CountNumCards() > 0) { UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet); }
            else { Module1.setImage(ref GraveZone, BLANK_IMAGE, UriKind.Relative); }
            if (Module1.PlayerCurrentRFG.CountNumCards() > 0) { UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], Module1.mySet); }
            else { Module1.setImage(ref rfgZone, BLANK_IMAGE, UriKind.Relative); }
			apassalong = Module1.LogViewAction;
			Module1.LogViewAction = "";
           
			if (apassalong == ("Banished from Deck")) {
				SummarizeJabber(Area1:"RFG", Index1:(short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1:"NDeck", Stat2:"NRFG", TheEvent:apassalong, Text:Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);


			} else if (apassalong == ("Banished from Graveyard")) {
                SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Area2: "rmGrave", Index2:(short)Module1.NumberOnList,  Stat1: "NGrave", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);


			} else if (apassalong == ("Banished from Extra Deck")) {
                SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NRFG", Stat2: "NEDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);

            } else if (apassalong == ("Banished from Deck Facedown")){
                SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NDeck", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);


			} else if (apassalong == ("Banished from Graveyard Facedown")) {
                SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Area2: "rmGrave", Index2:(short)Module1.NumberOnList,  Stat1: "NGrave", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);


			} else if (apassalong == ("Banished from Extra Deck Facedown")) {
                SummarizeJabber(Area1: "RFG", Index1: (short)Module1.PlayerCurrentRFG.CountNumCards(), Stat1: "NRFG", Stat2: "NEDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name);


			} else if (apassalong == ("Sent to Graveyard from RFG")) {
                SummarizeJabber(Area1: "Grave", Index1: (short)Module1.PlayerCurrentGrave.CountNumCards(), Area2: "rmRFG", Index2: (short)Module1.NumberOnList, Stat1: "NGrave", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name);


			} else if (apassalong == ("Sent to Graveyard from Deck")) {
                SummarizeJabber(Area1: "Grave", Index1: (short)Module1.PlayerCurrentGrave.CountNumCards(), Stat1: "NDeck", Stat2: "NGrave", TheEvent: apassalong, Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name);


			} else if (apassalong == ("Sent to Graveyard from Extra Deck")) {
                SummarizeJabber(Area1: "Grave", Index1: (short)Module1.PlayerCurrentGrave.CountNumCards(), Stat1: "NGrave", Stat2: "NEDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name);


			} else if (apassalong == ("Returned to Top of Deck from RFG")) {
                SummarizeJabber(Area1: "rmRFG", Index1: (short)Module1.NumberOnList, Stat1: "NDeck", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name);

            }
            else if (apassalong == ("Returned to Top of Deck from RFG Facedown"))
            {  SummarizeJabber(Area1: "rmRFG", Index1: (short)Module1.NumberOnList, Stat1: "NDeck", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name);

			} else if (apassalong == ("Returned to Top of Deck from Graveyard")) {
                SummarizeJabber(Area1: "rmGrave", Index1: (short)Module1.NumberOnList, Stat1: "NGrave", Stat2: "NDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name);


			} else if (apassalong == ("Returned to Top of Deck from Deck")) {
                SummarizeJabber(Stat1: "NDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[Module1.PlayerCurrentDeck.CountNumCards()].Name);


			} else if (apassalong == ("Returned to Bottom of Deck from Graveyard")) {
                SummarizeJabber(Area1: "rmGrave", Index1: (short)Module1.NumberOnList, Stat1: "NGrave", Stat2: "NDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[1].Name);


			} else if (apassalong == ("Returned to Bottom of Deck from RFG")) {
                SummarizeJabber(Area1: "rmRFG", Index1: (short)Module1.NumberOnList, Stat1: "NDeck", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[1].Name);
            } else if (apassalong == ("Returned to Bottom of Deck from RFG Facedown")) {
                SummarizeJabber(Area1: "rmRFG", Index1: (short)Module1.NumberOnList, Stat1: "NDeck", Stat2: "NRFG", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[1].Name);


			} else if (apassalong == ("Returned to Bottom of Deck from Deck")) {
                SummarizeJabber(Stat1: "NDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentDeck[1].Name);

            } else if (apassalong == ("Returned to Extra Deck from Field")){
                SummarizeJabber(Stat1: "NEDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name);

            } else if (apassalong == ("Returned to Extra Deck from Graveyard")){
                SummarizeJabber(Area1: "rmGrave", Index1: (short)Module1.NumberOnList, Stat1: "NEDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name);

            } else if (apassalong == ("Returned to Extra Deck from RFG")){
                SummarizeJabber(Area1: "rmRFG", Index1: (short)Module1.NumberOnList, Stat1: "NEDeck", TheEvent: apassalong, Text: Module1.PlayerCurrentEDeck[Module1.PlayerCurrentEDeck.CountNumCards()].Name);


			} else if (apassalong.Contains("Added to Hand")) {
                SummarizeJabber(TheEvent: apassalong);


			} else if (apassalong.Contains("Place on Field")) {
                SummarizeJabber(TheEvent: apassalong, Text: "");

			}

		}

	}
private void DuelField_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if (e.Key == Key.Enter) {
			if (string.IsNullOrEmpty(txtSendText.Text.Trim()))
				return;

			try {
				if (txtSendText.Text.Substring(0, 5) == @"/sub " && Module1.IamWatching == false) {
					int res = Convert.ToInt32(txtSendText.Text.Substring( 5, txtSendText.Text.Length - 5).Trim());
					Module1.PlayerLP -= res;
					lblPlayerLP.Text = "LP: " + Module1.PlayerLP;
					SummarizeJabber(Stat1:"LP", TheEvent:"Lost Lifepoints", Text:res.ToString());
					txtSendText.Text = "";
					return;
				}
                if (txtSendText.Text.Substring(0, 5) == @"/add " && Module1.IamWatching == false)
                {
					int res = Convert.ToInt32(txtSendText.Text.Substring( 5, txtSendText.Text.Length - 5).Trim());
					Module1.PlayerLP += res;
					lblPlayerLP.Text = "LP: " + Module1.PlayerLP;
                    SummarizeJabber(Stat1: "LP", TheEvent: "Gained Lifepoints", Text: res.ToString());
					txtSendText.Text = "";
					return;
				}

			} catch (Exception ) {
			}
            if (Module1.IamWatching == false)
            {
                SummarizeJabber(TheEvent: "Message", Text: txtSendText.Text);
            }
            else
            {
                SendJabber(txtSendText.Text);
                addMessage("You  : " + txtSendText.Text);
            }
			
			txtSendText.Text = "";
		}
	}
private void DuelField_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
   
            SeeChangeStatsAndFire();
            HideAllContext();
        }
#endregion

        #region "Phases"
        private void cmdDrawPhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.SemiBold;
		cmdStandbyPhase.FontWeight = FontWeights.Normal;
		cmdMainPhase1.FontWeight = FontWeights.Normal;
		cmdBattlePhase.FontWeight = FontWeights.Normal;
		cmdMainPhase2.FontWeight = FontWeights.Normal;
		cmdEndPhase.FontWeight = FontWeights.Normal;
		SummarizeJabber(TheEvent:"Draw Phase");
        
	}
        private void cmdStandbyPhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.Normal;
		cmdStandbyPhase.FontWeight = FontWeights.SemiBold;
		cmdMainPhase1.FontWeight = FontWeights.Normal;
		cmdBattlePhase.FontWeight = FontWeights.Normal;
		cmdMainPhase2.FontWeight = FontWeights.Normal;
		cmdEndPhase.FontWeight = FontWeights.Normal;
        SummarizeJabber(TheEvent: "Standby Phase");
	}
        private void cmdMainPhase1_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.Normal;
		cmdStandbyPhase.FontWeight = FontWeights.Normal;
		cmdMainPhase1.FontWeight = FontWeights.SemiBold;
		cmdBattlePhase.FontWeight = FontWeights.Normal;
		cmdMainPhase2.FontWeight = FontWeights.Normal;
		cmdEndPhase.FontWeight = FontWeights.Normal;
        SummarizeJabber(TheEvent: "Main Phase 1");
	}
        private void cmdBattlePhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.Normal;
		cmdStandbyPhase.FontWeight = FontWeights.Normal;
		cmdMainPhase1.FontWeight = FontWeights.Normal;
		cmdBattlePhase.FontWeight = FontWeights.SemiBold;
		cmdMainPhase2.FontWeight = FontWeights.Normal;
		cmdEndPhase.FontWeight = FontWeights.Normal;
        SummarizeJabber(TheEvent: "Battle Phase");
	}
        private void cmdMainPhase2_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.Normal;
		cmdStandbyPhase.FontWeight = FontWeights.Normal;
		cmdMainPhase1.FontWeight = FontWeights.Normal;
		cmdBattlePhase.FontWeight = FontWeights.Normal;
		cmdMainPhase2.FontWeight = FontWeights.SemiBold;
		cmdEndPhase.FontWeight = FontWeights.Normal;
        SummarizeJabber(TheEvent: "Main Phase 2");
	}
        private void cmdEndPhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.Normal;
		cmdStandbyPhase.FontWeight = FontWeights.Normal;
		cmdMainPhase1.FontWeight = FontWeights.Normal;
		cmdBattlePhase.FontWeight = FontWeights.Normal;
		cmdMainPhase2.FontWeight = FontWeights.Normal;
		cmdEndPhase.FontWeight = FontWeights.SemiBold;
        SummarizeJabber(TheEvent: "End Phase");
	}
        private void cmdEndTurn_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		cmdDrawPhase.FontWeight = FontWeights.Normal;
		cmdStandbyPhase.FontWeight = FontWeights.Normal;
		cmdMainPhase1.FontWeight = FontWeights.Normal;
		cmdBattlePhase.FontWeight = FontWeights.Normal;
		cmdMainPhase2.FontWeight = FontWeights.Normal;
		cmdEndPhase.FontWeight = FontWeights.Normal;
        SummarizeJabber(TheEvent: "End Turn");
	}
        #endregion
       
        /// <summary>
        /// Graveyard, Deck, RFG, Extra Deck
        /// </summary>
        /// <param name="whatImViewing"></param>
        /// <remarks></remarks>
        public void showViewForm(string whatImViewing)
        {
            viewForm.WhatImViewing = whatImViewing;
            viewForm.Show();
            //Canvas.SetLeft(viewForm, this.Width / 2);
            //Canvas.SetTop(viewForm, this.Height / 2);
            //LayoutRoot.Children.Add(viewForm);
        }


#region "Other Form Features"
private void cmdDie_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		SummarizeJabber(TheEvent:"Rolled a Die", Text:Convert.ToString(Module1.Rand(1, 6, new Random())));
	}
private void cmdCoin_Click(System.Object sender, System.Windows.RoutedEventArgs e)
	{
		int result = Module1.Rand(1, 2, new Random());
		if (result == 1) {
			SummarizeJabber(TheEvent:"Flipped a Coin", Text:"Heads");
		} else {
			SummarizeJabber(TheEvent:"Flipped a Coin", Text:"Tails");
		}
	}
private void cmdSendText_Click(System.Object sender, System.Windows.RoutedEventArgs e)
{
    if (string.IsNullOrEmpty(txtSendText.Text.Trim()))
        return;
    txtSendText.Text = Module1.getRidOfHarmfulCharacters(txtSendText.Text);
    try
    {
        if (txtSendText.Text.Substring(0, 5) == @"/sub " && Module1.IamWatching == false)
        {
            int res = Convert.ToInt32(txtSendText.Text.Substring(5, txtSendText.Text.Length - 5).Trim());
            Module1.PlayerLP -= res;
            lblPlayerLP.Text = "LP: " + Module1.PlayerLP;
            //SummarizeJabber(Stat1:"LP", TheEvent:"Lost Lifepoints", returnOnly:res.ToString());
            SummarizeJabber(Stat1: "LP", TheEvent: "Lost Lifepoints", Text: res.ToString());
            txtSendText.Text = "";
            return;
        }
        if (txtSendText.Text.Substring(0, 5) == @"/add " && Module1.IamWatching == false)
        {
            int res = Convert.ToInt32(txtSendText.Text.Substring(5, txtSendText.Text.Length - 5).Trim());
            Module1.PlayerLP += res;
            lblPlayerLP.Text = "LP: " + Module1.PlayerLP;
            SummarizeJabber(Stat1: "LP", TheEvent: "Gained Lifepoints", Text: res.ToString());
            txtSendText.Text = "";
            return;
        }

    }
    catch (Exception)
    {
    }

    if (Module1.IamWatching == false)
    {
        //SummarizeJabber(TheEvent:"Message", Text:txtSendText.Text);
        SummarizeJabber(TheEvent: "Message", Text: txtSendText.Text);
    }
    else
    {
        SendJabber(txtSendText.Text);
        addMessage("You  : " + txtSendText.Text);
    }
    // LogandSend("Message", txtSendText.Text)
    txtSendText.Text = "";
}
private void txtTurnCount_TextChanged(System.Object sender, System.Windows.Controls.TextChangedEventArgs e)
	{
        if (Module1.IamWatching) { return; }
        if (DontFireTurnCount)
        { DontFireTurnCount = false; return; }
        DontFireTurnCount = true;
		txtTurnCount.Text = txtTurnCount.Text.Trim();
        DontFireTurnCount = false;
        try
        {
            if (Module1.isNumeric(txtTurnCount.Text))
            {
                TurnCount = Convert.ToInt16(txtTurnCount.Text);
                SummarizeJabber(TheEvent: "Turn Count Changed", Text: txtTurnCount.Text);
            }
        }
        catch (OverflowException)
        {
            MessageBox.Show("This value is too large.");
            txtTurnCount.Text = "0";
        }


	}
private void cmdShuffleHand_Click(object sender, RoutedEventArgs e)
{
    shuffleHand();
}
private void shuffleHand()
{
    if (Module1.PlayerCurrentHand.CountNumCards() == 0) { return; }
    cmdShuffleHand.IsEnabled = false;
    Random newRand = new Random();
    lstMyHand.Items.Clear();

    int[] tempIds = new int[Module1.PlayerCurrentHand.CountNumCards() + 1];
    int tempNumCardsInHand = Module1.PlayerCurrentHand.CountNumCards();
    short nThroughHand = 1;
    do
    {
        int randHand = Module1.Rand(1, Module1.PlayerCurrentHand.CountNumCards(), newRand);
        if (tempIds[randHand] == 0)
        {
            tempIds[randHand] = Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[nThroughHand].ID);
            nThroughHand++;
        }
    } while (nThroughHand <= Module1.PlayerCurrentHand.CountNumCards());

    Module1.PlayerCurrentHand.ClearAndAdd();


    for (short n = 1; n <= tempNumCardsInHand; n++)
    {
        lstMyHand.Items.Add(Module1.CardStats[tempIds[n]].Name);
        Module1.PlayerCurrentHand.Add(Module1.CardStats[tempIds[n]]);
    }

    for (short n = 1; n <= tempNumCardsInHand; n++)
    {
        imgHand(n).setEndBehavior(tempIds[n], "imgHand" + n.ToString(), false, false);
        double prevLeft = imgHand(oppositeOfHand(n, (short)Module1.PlayerCurrentHand.CountNumCards())).CLeft() + (double)imgHand(oppositeOfHand(n, Module1.PlayerCurrentHand.CountNumCards())).RenderTransform.GetValue(TranslateTransform.XProperty);
        imgHand(n).animationTimer.setInMotion(imgHand(n), new Point(prevLeft, imgHandTop), 300, 10, false, "imgHand" + n.ToString(), true);
    }

    ZoneControl[] tempRefs = new ZoneControl[Module1.PlayerCurrentHand.CountNumCards() + 1];
    for (short n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
    {
        tempRefs[n] = imgHand(n);
        imgHand(n).Name = "blank" + n.ToString();
    }
    for (short n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
    {
        tempRefs[n].Name = "imgHand" + oppositeOfHand(n, Module1.PlayerCurrentHand.CountNumCards()).ToString();
    }

    SummarizeJabber(TheEvent: "hnd-shffl");
}
private void opShuffleHand()
{
    for (short n = 1; n <= Module1.NumCardsInOpHand; n++)
    {
        Image refer = opimgHand(n).baseImage;
        Module1.setImage(ref refer, "back.jpg", UriKind.Relative);
        opimgHand(n).setEndBehavior(0, "", false, false);
        double prevLeft = opimgHand(oppositeOfHand(n, Module1.NumCardsInOpHand)).CLeft() + (double)opimgHand(oppositeOfHand(n, Module1.NumCardsInOpHand)).RenderTransform.GetValue(TranslateTransform.XProperty);
        opimgHand(n).animationTimer.setInMotion(opimgHand(n), new Point(prevLeft, 5), 300, 10, false, "opimgHand" + n.ToString(), true);
    }

    ZoneControl[] tempRefs = new ZoneControl[Module1.NumCardsInOpHand + 1];
    for (short n = 1; n <= Module1.NumCardsInOpHand; n++)
    {
        tempRefs[n] = opimgHand(n);
        opimgHand(n).Name = "blank" + n.ToString();
    }
    for (short n = 1; n <= Module1.NumCardsInOpHand; n++)
    {
        tempRefs[n].Name = "opimgHand" + oppositeOfHand(n, Module1.NumCardsInOpHand).ToString();
    }
    resetopHandReveal();
}
private static short oppositeOfHand(short index, short maxNum) { return (short)((maxNum + 1) - index); }
private static short oppositeOfHand(short index, int maxNum) { return (short)((maxNum + 1) - index); }
private void cmdGainLP_Click(System.Object sender, System.Windows.RoutedEventArgs e)
{
    if (Module1.TagDuel && Module1.IamActive == false)
        return;
    if (Module1.IamWatching)
        return;
    try
    {
        Module1.PlayerLP = Module1.PlayerLP + Convert.ToInt32(txtLPChange.Text);
    }
    catch
    {
        MsgBox("Must be a number.");
        return;
    }
    lblPlayerLP.Text = "LP: " + Module1.PlayerLP.ToString();

    SummarizeJabber(Stat1: "LP", TheEvent: "Gained Lifepoints", Text: txtLPChange.Text);
}
private void cmdLoseLP_Click(System.Object sender, System.Windows.RoutedEventArgs e)
{
    if (Module1.TagDuel && Module1.IamActive == false)
        return;
    if (Module1.IamWatching)
        return;
    try
    {
        Module1.PlayerLP = Module1.PlayerLP - Convert.ToInt32(txtLPChange.Text);
    }
    catch
    {
        MsgBox("Must be a number.");
        return;
    }
    lblPlayerLP.Text = "LP: " + Module1.PlayerLP.ToString();
    SummarizeJabber(Stat1: "LP", TheEvent: "Lost Lifepoints", Text: txtLPChange.Text);
}
private void cmdDrawFive_Click(System.Object sender, System.Windows.RoutedEventArgs e)
{

    List<string> jabberMessages = new List<string>();
    updateImgHandPositions();
    jabberMessages.Add(DrawCard(true));
    updateImgHandPositions();
    jabberMessages.Add(DrawCard(true));
    updateImgHandPositions();
    jabberMessages.Add(DrawCard(true));
    updateImgHandPositions();
    jabberMessages.Add(DrawCard(true));
    updateImgHandPositions();
    jabberMessages.Add(DrawCard(true));
    SendJabber(combineMultipleJabbers(jabberMessages));
}
#endregion
#region "My Animation and Graphics"
// public delegate void UpdatePictureBoxDuelFieldDelegate(ref Image pBox, SQLReference.CardDetails stats, string theSet, bool ignoreSpecialImages = false);
//public UpdatePictureBoxDuelFieldDelegate UpdatePictureBoxInvoked;// = new UpdatePictureBoxDuelFieldDelegate(UpdatePictureBoxDuelField);

public void UpdatePictureBoxDuelField(ref Image pBox, string parentName, SQLReference.CardDetails stats, string theSet, bool ignoreSpecialImages = false)
        {
            //string cardname;// = stats.Name;
            if (stats == null) { return; }
            
            bool isFromField = fromField(pBox.Name);
            if (isFromField)
            {
                if (theSet == Module1.mySet && stats.OpponentOwned) //On player's side of the field but opponent owned
                    theSet = opponentSet;
                else if (theSet == opponentSet && stats.OpponentOwned) //On opponent's side of the field but player owned
                    theSet = Module1.mySet;
            }
            try
            {

                if (stats.Facedown)
                {
                    Module1.setImage(ref pBox, "back.jpg", UriKind.Relative);
                }
                else if (string.IsNullOrEmpty(stats.Name))
                {
                    Module1.setImage(ref pBox, BLANK_IMAGE, UriKind.Relative);
                    return;
                }
               
                else
                {
                    if (Module1.cardsWithImages.Contains(Module1.getRealImageName(stats.Name, stats.ID, theSet)) && ignoreSpecialImages == false)
                    {
                        Module1.setImage(ref pBox, Module1.toPortalURL(stats.Name, stats.ID, theSet), UriKind.Absolute);
                    }
                    else
                    {
                   
                        if (stats.Type.Contains("Token"))
                        {
                            Module1.setImage(ref pBox, "token.jpg", UriKind.Relative);

                        }
                        else if (stats.Attribute.Contains("Trap"))
                        {
                            Module1.setImage(ref pBox, "trap.jpg", UriKind.Relative);

                        }
                        else if (stats.Attribute.Contains( "Spell"))
                        {
                            Module1.setImage(ref pBox, "magic.jpg", UriKind.Relative);

                        }
                        else if (stats.Type.Contains("/Ritual"))
                        {
                            Module1.setImage(ref pBox, "ritual.jpg", UriKind.Relative);

                        }
                        else if (stats.Type.Contains("/Synchro"))
                        {
                            Module1.setImage(ref pBox, "synchro.jpg", UriKind.Relative);

                        }
                        else if (stats.Type.Contains("/Fusion"))
                        {
                            Module1.setImage(ref pBox, "fusion.jpg", UriKind.Relative);

                        }
                        else if (stats.Type.Contains("/Xyz"))
                        {
                            Module1.setImage(ref pBox, "xyz.jpg", UriKind.Relative);

                        }
                        else if (stats.Type.Contains("/Effect") && Module1.IsOrange(stats))
                        {
                            Module1.setImage(ref pBox, "monstereffect.jpg", UriKind.Relative);


                        }
                        else
                        {
                            Module1.setImage(ref pBox, "monster.jpg", UriKind.Relative);
                        }
                    }
                }
                pBox.Opacity = 1;

               
                if (isFromField)
                {
                    ZoneControl paren = (ZoneControl)LayoutRoot.FindName(parentName);
                    if (stats.IsItHorizontal && Convert.ToDouble(paren.RenderTransform.ReadLocalValue(CompositeTransform.RotationProperty)) == 0.0)
                    {
                         Rotate90Degrees(ref paren);
                    }
                    else if (stats.IsItHorizontal == false && Convert.ToDouble(paren.RenderTransform.ReadLocalValue(CompositeTransform.RotationProperty)) == 90.0)
                    {
                        cancelRotation(ref paren);
                    }
                }

            }
            catch
            {

            }
        }
public static void Rotate90Degrees(ref ZoneControl img) { img.RenderTransform.SetValue(CompositeTransform.RotationProperty, 90.0); }
public static void cancelRotation(ref ZoneControl img) { img.RenderTransform.SetValue(CompositeTransform.RotationProperty, 0.0); }
private void anyImage_Failed(object sender, ExceptionRoutedEventArgs e)
{

    Image sourceImage = (Image)sender;
    SQLReference.CardDetails stats = new SQLReference.CardDetails();
    stats.Name = "no name"; stats.Type = "Aqua"; //Default to monster.jpg
    int index = 0;
    if (sourceImage.Name.Contains("opMonZone"))
    {
        index = Convert.ToInt32(sourceImage.Name.Replace("opMonZone", ""));
        if (Module1.OpponentCurrentMonsters[index] != null && 
            Module1.OpponentCurrentMonsters[index].Name != null)
            stats = Module1.OpponentCurrentMonsters[index];
    }
    else if (sourceImage.Name.Contains("opstZone"))
    {
        index = Convert.ToInt32(sourceImage.Name.Replace("opstZone", ""));
        if (Module1.OpponentCurrentST[index] != null &&
           Module1.OpponentCurrentST[index].Name != null)
        stats = Module1.OpponentCurrentST[index];
    }
    else if (sourceImage.Name.Contains("MonZone"))
    {
        index = Convert.ToInt32(sourceImage.Name.Replace("MonZone", ""));
        if (Module1.PlayerCurrentMonsters[index] != null &&
             Module1.PlayerCurrentMonsters[index].Name != null)
        stats = Module1.PlayerCurrentMonsters[index];
    }
    else if (sourceImage.Name.Contains("stZone"))
    {
        index = Convert.ToInt32(sourceImage.Name.Replace("stZone", ""));
        if (Module1.PlayerCurrentST[index] != null &&
             Module1.PlayerCurrentST[index].Name != null)
        stats = Module1.PlayerCurrentST[index];
    }
    else if (sourceImage.Name.Contains("opFieldSpellZone"))
    {
        if (Module1.OpponentCurrentFSpell != null &&
          Module1.OpponentCurrentFSpell.Name != null)
        stats = Module1.OpponentCurrentFSpell;
    }
    else if (sourceImage.Name.Contains("FieldSpellZone"))
    {
        if (Module1.PlayerCurrentFSpell != null &&
          Module1.PlayerCurrentFSpell.Name != null)
        stats = Module1.PlayerCurrentFSpell;
    }
    else if (sourceImage.Name.Contains("opGraveZone"))
    {
        if (Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()] != null &&
          Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()].Name != null)
        stats = Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()];
    }
    else if (sourceImage.Name.Contains("GraveZone"))
    {
        if (Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()] != null &&
         Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()].Name != null)
        stats = Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()];
    }
    else if (sourceImage.Name.Contains("oprfgZone"))
    {
        if (Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()] != null &&
  Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()].Name != null)
        stats = Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()];
    }
    else if (sourceImage.Name.Contains("rfgZone"))
    {
        if (Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()] != null &&
Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()].Name != null)
        stats = Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()];
    }
    else if (sourceImage.Name.Contains("opimghand"))
    {
       
    }
    else if (sourceImage.Name.Contains("imgHand"))
    {
        index = Convert.ToInt32(sourceImage.Name.Replace("imgHand", ""));
        if (Module1.PlayerCurrentHand[index] != null &&
            Module1.PlayerCurrentHand[index].Name != null)
        stats = Module1.PlayerCurrentHand[index];
    }
    else if (sourceImage.Name.Contains("opXyzImg"))
    {
        int firstUndscor = sourceImage.Name.IndexOf("_");
        int secondUndscor = sourceImage.Name.IndexOf("_", firstUndscor + 1);
        int matZone = Convert.ToInt32(sourceImage.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
        int matIndex = Convert.ToInt32(sourceImage.Name.Substring(secondUndscor + 1, 1));
        if (Module1.OpponentOverlaid[matZone].CountNumCards() >= matIndex &&
            Module1.OpponentOverlaid[matZone][matIndex] != null &&
            Module1.OpponentOverlaid[matZone][matIndex].Name != null)
            stats = Module1.OpponentOverlaid[matZone][matIndex];
    }
    else if (sourceImage.Name.Contains("XyzImg"))
    {
        int firstUndscor = sourceImage.Name.IndexOf("_");
        int secondUndscor = sourceImage.Name.IndexOf("_", firstUndscor + 1);
        int matZone = Convert.ToInt32(sourceImage.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
        int matIndex = Convert.ToInt32(sourceImage.Name.Substring(secondUndscor + 1, 1));

        if (Module1.PlayerOverlaid[matZone].CountNumCards() >= matIndex &&
        Module1.PlayerOverlaid[matZone][matIndex] != null &&
        Module1.PlayerOverlaid[matZone][matIndex].Name != null)
            stats = Module1.PlayerOverlaid[matZone][matIndex];
    }
    else
        return;

    UpdatePictureBoxDuelField(ref sourceImage, sourceImage.Name, stats, Module1.mySet, true);
}

private void animationTimer_Tick(ZoneControl sender)
{
    //if (currentlyAnimating == "")
    //    currentlyAnimating = sender.Name;
    //else if (currentlyAnimating != sender.Name)
    //    return;

    if (sender.Visibility == System.Windows.Visibility.Collapsed) { sender.Visibility = System.Windows.Visibility.Visible; }

    switch (sender.RenderTransform.GetType().ToString())
    {
        case "System.Windows.Media.TranslateTransform":
            TranslateTransform trans = (TranslateTransform)sender.RenderTransform;

            if (sender.animationTimer.currentIteration == 0)
            {
                transformToArea(sender, new Point(sender.animationTimer._xOrigin, sender.animationTimer._yOrigin));
            }
            else
            {
                trans.X = trans.X + sender.animationTimer._xVelocity;
                trans.Y = trans.Y + sender.animationTimer._yVelocity;
            }
            break;
        case "System.Windows.Media.CompositeTransform":
            CompositeTransform comtrans = (CompositeTransform)sender.RenderTransform;

            if (sender.animationTimer.currentIteration == 0)
            {
                Canvas.SetZIndex(sender, 1);
                transformToArea(sender, new Point(sender.animationTimer._xOrigin, sender.animationTimer._yOrigin));
            }
            else
            {
                comtrans.TranslateX = comtrans.TranslateX + sender.animationTimer._xVelocity;
                comtrans.TranslateY = comtrans.TranslateY + sender.animationTimer._yVelocity;
            }
            break;
    }


    sender.animationTimer.currentIteration++;

    //  System.Diagnostics.Debug.WriteLine("{0} moved, iteration {1}.", sender.Name, sender.animationTimer.currentIteration);


    //Last iteration
    if (sender.animationTimer.currentIteration >= sender.animationTimer.necessaryIterations)
    {
        //sender.animationTimer.Tick -= (s, args);
        sender.animationTimer.Stop();
        cmdShuffleHand.IsEnabled = true;
        if (sender.animationTimer._resetAtEnd)
        {
            switch (sender.RenderTransform.GetType().ToString())
            {
                case "System.Windows.Media.TranslateTransform":
                    TranslateTransform trans = (TranslateTransform)sender.RenderTransform;
                    trans.X = 0;
                    trans.Y = 0;
                    break;
                case "System.Windows.Media.CompositeTransform":
                    CompositeTransform comtrans = (CompositeTransform)sender.RenderTransform;
                    comtrans.TranslateX = 0;
                    comtrans.TranslateY = 0;
                    break;
            }
        }
        if (!string.IsNullOrEmpty(sender.endUpdateZone))
        {
            try
            {
                Image img = (Image)LayoutRoot.FindName(sender.endUpdateZone);
                if (img != null)
                {
                    if (sender.endStat != null)
                        UpdatePictureBoxDuelField(ref img, sender.Name, sender.endStat, Module1.mySet);
                    else
                        Module1.setImage(ref img, BLANK_IMAGE, UriKind.Relative);
                    setAsNothing(sender.endStat);
                }
            }
            catch
            {
                ZoneControl img = (ZoneControl)LayoutRoot.FindName(sender.endUpdateZone);

                if (img == null && sender.endUpdateZone.Contains("imgHand"))
                {
                    addImgHand(Module1.findIndexFromTrueID(Module1.PlayerCurrentHand[Module1.PlayerCurrentHand.CountNumCards()].ID), false);
                    img = (ZoneControl)LayoutRoot.FindName(sender.endUpdateZone);
                }
                if (img != null)
                {
                    if (sender.endStat != null)
                        UpdatePictureBoxDuelField(ref img.baseImage, img.Name, sender.endStat, Module1.mySet);
                    else
                        Module1.setImage(ref img.baseImage, BLANK_IMAGE, UriKind.Relative);
                }
            }
        }

        if (sender.removeAtEnd)
        {
            removezImg(sender);
        }
        if (sender.blankAtEnd)
        {
            Module1.setImage(ref sender.baseImage, BLANK_IMAGE, UriKind.Relative);
        }

        if (sender.endCallFunction != null)
        {
            sender.endCallFunction.Invoke();
            sender.endCallFunction = null;
        }

        Canvas.SetZIndex(sender, 0);
        if (Module1.animationQueue.Count > 0) { Module1.animationQueue.Dequeue(); }
        if (Module1.animationQueue.Count > 0)
        {
            ZoneControl newSender = (ZoneControl)LayoutRoot.FindName(Module1.animationQueue.Peek());
            if (newSender != null)
            {
                if (object.ReferenceEquals(sender, newSender))
                {
                    newSender.setEndBehavior(newSender.secondaryendStat, newSender.secondaryendUpdateZone, newSender.secondaryremoveAtEnd, newSender.secondaryblankAtEnd);
                    newSender.animationTimer.setInMotion(new Point(newSender.animationTimer.secondary_xOrigin, newSender.animationTimer.secondary_yOrigin), new Point(newSender.animationTimer.secondary_xDestination, newSender.animationTimer.secondary_yDestination),
                    300, 10, false, newSender.Name);
                }
                else { newSender.animationTimer.Start(); }

            }
        }
    }

}
private static bool fromField(string pboxName)
{
    if (pboxName.Contains("MonZone") || pboxName.Contains("stZone") || pboxName.Contains("FieldSpellZone"))
        return true;
    return false;
}
private void updateImgHandPositions()
{
    const double leftmost = 256;
    double distanceAcross;

    if (Module1.PlayerCurrentHand.CountNumCards() <= 6)
        distanceAcross = 21.8;
    else
    {
        distanceAcross = (439 - (Module1.PlayerCurrentHand.CountNumCards() * 55)) / (Module1.PlayerCurrentHand.CountNumCards() - 1);
        //TranslateTransform trans = (TranslateTransform)imgHand(3).RenderTransform;

    }
    for (short n = 1; n <= Module1.PlayerCurrentHand.CountNumCards(); n++)
    {
        if (n == 1)
        {
            ZoneControl zImg = imgHand(n);
            if (zImg != null)
                translateHand(ref zImg, leftmost);
        }
        else
        {
            ZoneControl zImg = imgHand(n);
            if (zImg != null)
            {
                double prevleft = imgHand((short)(n - 1)).CLeft();
                TranslateTransform trans = (TranslateTransform)imgHand((short)(n - 1)).RenderTransform;
                translateHand(ref zImg, prevleft + trans.X + 55 + distanceAcross);
            }

        }
    }

}
private void watcherUpdateImgHandPositions()
{
   
    const double leftmost = 256;
    double distanceAcross;

    if (Module1.watcherNumCardsInHand <= 6)
        distanceAcross = 21.8;
    else
    {
        distanceAcross = (439 - (Module1.watcherNumCardsInHand * 55)) / (Module1.watcherNumCardsInHand - 1);

    }
    for (short n = 1; n <= Module1.watcherNumCardsInHand; n++)
    {
        if (n == 1)
        {
            ZoneControl zImg = imgHand(n);
            if (zImg != null)
                translateHand(ref zImg, leftmost);
        }
       
        else
        {
            ZoneControl zImg = imgHand(n);
            if (zImg != null)
            {
                double prevleft = imgHand((short)(n - 1)).CLeft();
                TranslateTransform trans = (TranslateTransform)imgHand((short)(n - 1)).RenderTransform;
                translateHand(ref zImg, prevleft + trans.X + 55 + distanceAcross);
            }
        
        }
    }

}
private void updateOpImgHandPositions()
{


    const double leftmost = 256;
    double distanceAcross;

    if (Module1.NumCardsInOpHand <= 6)
        distanceAcross = 21.8;
    else
    {
        distanceAcross = (439 - (Module1.NumCardsInOpHand * 55)) / (Module1.NumCardsInOpHand - 1);
    }
    for (short n = 1; n <= Module1.NumCardsInOpHand; n++)
    {
        if (n == 1)
        {
            ZoneControl zImg = opimgHand(n);
            if (zImg != null)
                translateHand(ref zImg, leftmost);
        }
        else
        {
            ZoneControl zImg = opimgHand(n);
            if (zImg != null)
            {
                double prevleft = opimgHand((short)(n - 1)).CLeft();
                TranslateTransform trans = (TranslateTransform)opimgHand((short)(n - 1)).RenderTransform;
                translateHand(ref zImg, prevleft + trans.X + 55 + distanceAcross);
            }
        }
    }

}
private void translateHand(ref ZoneControl img, double newLeft)
{
    if (img == null)
        return;
    TranslateTransform trans = (TranslateTransform)img.RenderTransform;
    trans.X = newLeft - img.CLeft();
    trans.Y = 0;
}
private static double getTrans(ZoneControl zImg, bool trueXfalseY)
{
    TranslateTransform trans = (TranslateTransform)zImg.RenderTransform;
    if (trueXfalseY)
        return trans.X;
    else
        return trans.Y;

}
private void MoveXyz(int oldZone, int newZone)
{
    int oldZoneMatNum = Module1.PlayerOverlaid[oldZone].CountNumCards();

    ZoneControl zImg;
    for (int n = 1; n <= oldZoneMatNum; n++)
    {
        Module1.PlayerOverlaid[newZone].Add(Module1.PlayerOverlaid[oldZone][n]);

        zImg = imgXyz(oldZone, n);
       // zImg.Margin = new Thickness(MonZone(newZone).CLeft(), MonZone(newZone).CTop(), 0, 0);
        CopyCanvasCoordinates(MonZone(newZone), zImg);
        zImg.Name = "XyzMat_" + newZone.ToString() + "_" + n.ToString();
    }
    Module1.PlayerOverlaid[oldZone].ClearAndAdd();
}

/// <summary>
/// Returns the absolute point before the transform
/// </summary>
/// <param name="zImg"></param>
/// <param name="leftAndTop"></param>
/// <returns></returns>
private static Point transformToArea(ZoneControl zImg, Point leftAndTop)
{
    switch (zImg.RenderTransform.GetType().ToString())
    {
        case "System.Windows.Media.TranslateTransform":
            TranslateTransform trans = (TranslateTransform)zImg.RenderTransform;
            double prevX = zImg.CLeft() + trans.X;
            double prevY = zImg.CTop() + trans.Y;
            trans.X = leftAndTop.X - zImg.CLeft();
            trans.Y = leftAndTop.Y - zImg.CTop();
            return new Point(prevX, prevY);

        case "System.Windows.Media.CompositeTransform":
            CompositeTransform comtrans = (CompositeTransform)zImg.RenderTransform;
            double prevX2 = zImg.CLeft() + comtrans.TranslateX;
            double prevY2 = zImg.CTop() + comtrans.TranslateY;
            comtrans.TranslateX = leftAndTop.X - zImg.CLeft();
            comtrans.TranslateY = leftAndTop.Y - zImg.CTop();
            return new Point(prevX2, prevY2);

    }
    return new Point(0, 0);
}
private void removezImg(ZoneControl zImg)
{
    zImg.baseImage_Failed -= anyImage_Failed;
   zImg.RenderTransform = null;
    zImg.baseImage.Source = null;
    LayoutRoot.Children.Remove(zImg);
    if (zImg.Name.Contains("opimgHand"))
    {
        short index = Convert.ToInt16(zImg.Name.Substring(9, zImg.Name.Length - 9));
        for (short n = (short)(index + 1); n <= Module1.NumCardsInOpHand + 1; n++)
        {
            opimgHand(n).Name = "opimgHand" + (n - 1).ToString();
        }
        updateOpImgHandPositions();
        zImg.MouseLeftButtonUp -= opImgHand_MouseLeftButtonUp;
    }
    else if (zImg.Name.Contains("imgHand"))
    {
        short index = Convert.ToInt16(zImg.Name.Substring(7, zImg.Name.Length - 7));
        if (Module1.IamWatching)
        {
            for (short n = (short)(index + 1); n <= Module1.watcherNumCardsInHand + 1; n++)
            {
                imgHand(n).Name = "imgHand" + (n - 1).ToString();
            }
            watcherUpdateImgHandPositions();
        }
        else
        {
            for (short n = (short)(index + 1); n <= Module1.PlayerCurrentHand.CountNumCards() + 1; n++)
            {
                imgHand(n).Name = "imgHand" + (n - 1).ToString();
            }
            updateImgHandPositions();
            zImg.MouseRightButtonDown -= imgHand_MouseRightButtonDown;
        }
        zImg.MouseLeftButtonUp -= imgHand_MouseLeftButtonUp;
    }

    if (zImg.Name.Contains("opXyzMat"))
    {
        int firstUndscor = zImg.Name.IndexOf("_");
        int secondUndscor = zImg.Name.IndexOf("_", firstUndscor + 1);
        int matZone = Convert.ToInt32(zImg.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
        int matIndex = Convert.ToInt32(zImg.Name.Substring(secondUndscor + 1, 1));
        for (short n = (short)(matIndex + 1); n <= Module1.OpponentOverlaid[matZone].CountNumCards() + 1; n++) //opponent overlaid is not modified at first
        {                                                                                                      //new monsters are sent first in move(), then new xyzs
            ZoneControl opXyz = opimgXyz((short)matZone, n);                                                     //so opOverlaid() will be 1 higher than it should be
            if (opXyz != null)
                opXyz.Name = "opXyzMat_" + matZone.ToString() + "_" + (n - 1).ToString();

        }
        updateopXyzPositions((short)matZone);
        zImg.MouseLeftButtonUp -= opXyzImg_MouseLeftButtonUp;
    }
    else if (zImg.Name.Contains("XyzMat"))
    {
        int firstUndscor = zImg.Name.IndexOf("_");
        int secondUndscor = zImg.Name.IndexOf("_", firstUndscor + 1);
        int matZone = Convert.ToInt32(zImg.Name.Substring(firstUndscor + 1, secondUndscor - firstUndscor - 1));
        int matIndex = Convert.ToInt32(zImg.Name.Substring(secondUndscor + 1, 1));
        for (short n = (short)(matIndex + 1); n <= Module1.PlayerOverlaid[matZone].CountNumCards() + 1; n++)
        {
            imgXyz((short)matZone, n).Name = "XyzMat_" + matZone.ToString() + "_" + (n - 1).ToString();

        }
        updateXyzPositions((short)matZone);
        zImg.MouseLeftButtonUp -= XyzImg_MouseLeftButtonUp;
        zImg.MouseRightButtonDown -= XyzImg_MouseRightButtonDown;
    }



}
private double calculateDesirableImgHandPos(short index)
{
    const double leftmost = 256;
    double distanceAcross;

    if (Module1.PlayerCurrentHand.CountNumCards() <= 6)
        distanceAcross = 21.8;
    else
    {
        distanceAcross = (439 - (Module1.PlayerCurrentHand.CountNumCards() * 55)) / (Module1.PlayerCurrentHand.CountNumCards() - 1);
    }

    if (index == 1)
        return leftmost;
    else
    {
        double prevleft = imgHand((short)(index - 1)).CLeft();
        TranslateTransform trans = (TranslateTransform)imgHand((short)(index - 1)).RenderTransform;
        return prevleft + trans.X + 55 + distanceAcross;
    }


}
private double calculateDesirableImgHandPos(int index)
{
    const double leftmost = 256;
    double distanceAcross;

    if (Module1.PlayerCurrentHand.CountNumCards() <= 6)
        distanceAcross = 21.8;
    else
    {
        distanceAcross = (439 - (Module1.PlayerCurrentHand.CountNumCards() * 55)) / (Module1.PlayerCurrentHand.CountNumCards() - 1);
    }

    if (index == 1)
        return leftmost;
    else
    {
        double prevleft = imgHand((short)(index - 1)).CLeft();
        TranslateTransform trans = (TranslateTransform)imgHand((short)(index - 1)).RenderTransform;
        return prevleft + trans.X + 55 + distanceAcross;
    }


}
private void updateXyzPositions(int zone)
{

    int materialNum = Module1.PlayerOverlaid[zone].CountNumCards();
    for (short n = 1; n <= materialNum; n++)
    {
        ZoneControl zImg = imgXyz(zone, n);
        zImg.RenderTransform.SetValue(TranslateTransform.XProperty, (double)(n * 10));
    }
}
private void updateopXyzPositions(int zone)
{
    short materialNum = (short)Module1.OpponentOverlaid[zone].CountNumCards();
    for (short n = 1; n <= materialNum; n++)
    {
        ZoneControl zImg = opimgXyz(zone, n);

        if (zImg != null) zImg.RenderTransform.SetValue(TranslateTransform.XProperty, (double)(n * 10));
    }
}
private void addImgHand(int statID, bool invisibleAtFirst)
{
    ZoneControl zImg = new ZoneControl();
    zImg.Name = "imgHand" + Module1.PlayerCurrentHand.CountNumCards().ToString();
    zImg.RenderTransform = new TranslateTransform();
    zImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
    zImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
    zImg.Width = 55;
    zImg.Height = 81;
    zImg.baseImage.Opacity = 1;
    Canvas.SetLeft(zImg, 256); Canvas.SetTop(zImg, imgHandTop);
    if (invisibleAtFirst) { zImg.Visibility = System.Windows.Visibility.Collapsed; }
    zImg.setEndBehavior(statID, zImg.Name, false, false);
    LayoutRoot.Children.Add(zImg);
    updateImgHandPositions();

    Module1.setImage(ref zImg.baseImage, "back.jpg", UriKind.Relative);
    zImg.MouseLeftButtonUp += new MouseButtonEventHandler(imgHand_MouseLeftButtonUp);
    zImg.MouseRightButtonDown += new MouseButtonEventHandler(imgHand_MouseRightButtonDown);
    zImg.baseImage_Failed += anyImage_Failed;
    zImg.animationTimer_Tick += (s, args) =>
    {
        animationTimer_Tick(zImg);
    };

}
private void addopImgHand(bool invisibleAtFirst, int multipleUpdate = 0)
{
    ZoneControl zImg = new ZoneControl();
    if (multipleUpdate > 0)
        zImg.Name = "opimgHand" + multipleUpdate.ToString();
    else
        zImg.Name = "opimgHand" + Module1.NumCardsInOpHand.ToString(); //Module1.OpponentCurrentHand.CountNumCards().ToString();
    zImg.RenderTransform = new TranslateTransform();
    zImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
    zImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
    zImg.Width = 55;
    zImg.Height = 81;
    zImg.baseImage.Opacity = 1;
    Canvas.SetLeft(zImg, Canvas.GetLeft(opDeckZone)); Canvas.SetTop(zImg, 5); //256, 5
    if (invisibleAtFirst) { zImg.Visibility = System.Windows.Visibility.Collapsed; }
    zImg.setEndBehavior(0, "", false, false);
    LayoutRoot.Children.Add(zImg);
    updateOpImgHandPositions();

    Module1.setImage(ref zImg.baseImage, "back.jpg", UriKind.Relative);
    zImg.baseImage_Failed += anyImage_Failed;
    zImg.MouseLeftButtonUp += new MouseButtonEventHandler(opImgHand_MouseLeftButtonUp);
    zImg.animationTimer_Tick += (s, args) =>
    {
        animationTimer_Tick(zImg);
    };
}
private void watcherAddImgHand(bool invisibleAtFirst, int multipleUpdate = 0)
{
    ZoneControl zImg = new ZoneControl();
    if (multipleUpdate > 0)
        zImg.Name = "imgHand" + multipleUpdate.ToString();
    else
        zImg.Name = "imgHand" + Module1.watcherNumCardsInHand.ToString(); 
    zImg.RenderTransform = new TranslateTransform();
    zImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
    zImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
    zImg.Width = 55;
    zImg.Height = 81;
    zImg.baseImage.Opacity = 1;
    //zImg.Margin = new Thickness(256, imgHandTop, 0, 0);
    Canvas.SetLeft(zImg, 256); Canvas.SetTop(zImg, imgHandTop);
    if (invisibleAtFirst) { zImg.Visibility = System.Windows.Visibility.Collapsed; }
    zImg.setEndBehavior(0, "", false, false);
    LayoutRoot.Children.Add(zImg);
    watcherUpdateImgHandPositions();

    Module1.setImage(ref zImg.baseImage, "back.jpg", UriKind.Relative);
    zImg.baseImage_Failed += anyImage_Failed;
    zImg.MouseLeftButtonUp += new MouseButtonEventHandler(imgHand_MouseLeftButtonUp);
    zImg.animationTimer_Tick += (s, args) =>
    {
        animationTimer_Tick(zImg);
    };
}

private void imgHand_MouseOver(object sender, System.EventArgs e)
{
    Image img = (Image)sender;
    TranslateTransform trans = (TranslateTransform)img.RenderTransform;
    trans.Y -= 30;
}
private void imgHand_MouseLeave(object sender, System.EventArgs e)
{
    Image img = (Image)sender;
    TranslateTransform trans = (TranslateTransform)img.RenderTransform;
    trans.Y += 30;
}
private void addXyzImg(int MonsterZone, int MaterialIndex)
{
    ZoneControl zImg = new ZoneControl();
    zImg.Name = "XyzMat_" + MonsterZone.ToString() + "_" + MaterialIndex.ToString();
    zImg.RenderTransform = new TranslateTransform();
    zImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
    zImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
    zImg.Width = 55;
    zImg.Height = 81;
    zImg.baseImage.Opacity = 1;
    Canvas.SetLeft(zImg, MonZone(MonsterZone).CLeft()); Canvas.SetTop(zImg, MonZone(MonsterZone).CTop()); //305
    zImg.RenderTransform.SetValue(TranslateTransform.XProperty, (double)(MaterialIndex * 10));
    //zImg.setEndBehavior(0, zImg.Name, false, false);
    LayoutRoot.Children.Add(zImg);
    Canvas.SetZIndex(zImg, MaterialIndex * -1);

    zImg.MouseLeftButtonUp += new MouseButtonEventHandler(XyzImg_MouseLeftButtonUp);
    zImg.MouseRightButtonDown += new MouseButtonEventHandler(XyzImg_MouseRightButtonDown);
    zImg.baseImage_Failed += anyImage_Failed;
    zImg.animationTimer_Tick += (s, args) =>
    {
        animationTimer_Tick(zImg);
    };
    UpdatePictureBoxDuelField(ref zImg.baseImage, zImg.Name, Module1.PlayerOverlaid[MonsterZone][MaterialIndex], Module1.mySet, false);
}
private void addopXyzImg(int opMonsterZone, int opMaterialIndex)
{
    ZoneControl zImg = new ZoneControl();
    zImg.Name = "opXyzMat_" + opMonsterZone.ToString() + "_" + opMaterialIndex.ToString();
    zImg.RenderTransform = new TranslateTransform();
    zImg.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
    zImg.VerticalAlignment = System.Windows.VerticalAlignment.Top;
    zImg.Width = 55;
    zImg.Height = 81;
    zImg.baseImage.Opacity = 1;
    Canvas.SetLeft(zImg, opMonZone(opMonsterZone).CLeft() + (opMaterialIndex * 10)); Canvas.SetTop(zImg, opMonZone(opMonsterZone).CTop()); //181
    //zImg.setEndBehavior(0, zImg.Name, false, false);
    LayoutRoot.Children.Add(zImg);
    Canvas.SetZIndex(zImg, opMaterialIndex * -1);

    zImg.MouseLeftButtonUp += new MouseButtonEventHandler(opXyzImg_MouseLeftButtonUp);
    zImg.baseImage_Failed += anyImage_Failed;
    zImg.animationTimer_Tick += (s, args) =>
    {
        animationTimer_Tick(zImg);
    };
    UpdatePictureBoxDuelField(ref zImg.baseImage, zImg.Name, Module1.OpponentOverlaid[opMonsterZone][opMaterialIndex], opponentSet, false);
}
private double calculateNewXyzLeft(int MonZoneIndex, double MonZoneLeft)
{
    int overlaidNum = Module1.PlayerOverlaid[MonZoneIndex].CountNumCards();
    return MonZoneLeft + ((overlaidNum) * 10);

}


#endregion
#region OpAnimation
private static Module1.animationBundle createBundleToSend(string target, string from, string to)
        {
            Module1.animationBundle bundle = new Module1.animationBundle();


            if (from == "GraveZone")
                bundle.fromArea = Module1.animationArea.Grave;
            else if (from == "rfgZone")
                bundle.fromArea = Module1.animationArea.RFG;
            else if (from == "DeckZone")
                bundle.fromArea = Module1.animationArea.Deck;
            else if (from == "ExtraDeckZone")
                bundle.fromArea = Module1.animationArea.Extra;
            else if (from == "FieldSpellZone")
                bundle.fromArea = Module1.animationArea.FieldSpell;
            else
            {
                if (from.Contains("XyzMat"))
                {
                    bundle.fromArea = Module1.animationArea.Xyz;
                    bundle.fromIndex = Convert.ToInt16(from.Substring(7, 1));
                    bundle.fromIndexXyz = Convert.ToInt16(from.Substring(from.Length - 1, 1));
                }
                else
                {
                  bundle.fromArea = animationAreaFromString(from);
                  bundle.fromIndex = Convert.ToInt16(from.Replace(getNameWithoutIndex(from), ""));
                }
            }

            if (target == "FieldSpellZone")
                bundle.target = Module1.animationArea.FieldSpell;
            else
            {
                if (target.Contains("XyzMat"))
                {
                    bundle.target = Module1.animationArea.Xyz;
                    bundle.targetIndex = Convert.ToInt16(from.Substring(7, 1));
                    bundle.targetIndexXyz = Convert.ToInt16(from.Substring(from.Length - 1, 1));
                }
                else
                {
                      bundle.target = animationAreaFromString(target);
                      bundle.targetIndex = Convert.ToInt16( target.Replace(getNameWithoutIndex(target), ""));
                }
            }
           

            if (to == "GraveZone")
                bundle.toArea = Module1.animationArea.Grave;
            else if (to == "rfgZone")
                bundle.toArea = Module1.animationArea.RFG;
            else if (to == "DeckZone")
                bundle.toArea = Module1.animationArea.Deck;
            else if (to == "ExtraDeckZone")
                bundle.toArea = Module1.animationArea.Extra;
            else if (to == "FieldSpellZone")
                bundle.toArea = Module1.animationArea.FieldSpell;
            else
            {
                if (to.Contains("XyzMat"))
                {
                    bundle.toArea = Module1.animationArea.Xyz;
                    bundle.toIndex = Convert.ToInt16(to.Substring(7, 1));
                    bundle.toIndexXyz = Convert.ToInt16(to.Substring(to.Length - 1, 1));
                }
                else
                {
                    bundle.toArea = animationAreaFromString(to);
                    bundle.toIndex = Convert.ToInt16(to.Replace(getNameWithoutIndex(to), ""));
                }
            }

            return bundle;

        }
        private static string getNameWithoutIndex(string name)
        {
            System.Text.StringBuilder strBld = new System.Text.StringBuilder(name);
            strBld =
            strBld.Replace("0", "").Replace("1", "")
                .Replace("2", "").Replace("3", "")
                .Replace("4", "").Replace("5", "")
                .Replace("6", "").Replace("7", "")
                .Replace("8", "").Replace("9", "");
            return strBld.ToString();

        }
        private void opponentAnimation(Module1.animationArea target, int targetIndex, int targetIndexXyz, SQLReference.CardDetails stats,
                                       Module1.animationArea from, int fromIndex,
                                       Module1.animationArea to, int toIndex,
                                       int fromIndexXyz = 0, int toIndexXyz = 0)
        {
            bool removeIt; bool blankIt = false; bool resetIt = false;

            if (to == Module1.animationArea.Hand && from != Module1.animationArea.Hand)
            {
                if (stats != null)  stats.Facedown = true;
            }

            if (from == Module1.animationArea.Hand && to != Module1.animationArea.Hand)
                removeIt = true;
            else if (to == Module1.animationArea.Grave && target == Module1.animationArea.Xyz)
                removeIt = true;
            else
            {
                removeIt = false;
                if (to == Module1.animationArea.Grave || to == Module1.animationArea.RFG || to == Module1.animationArea.Extra)
                    blankIt = true;
                else
                    blankIt = false;
            }

            if (from == Module1.animationArea.Monster || from == Module1.animationArea.ST || from == Module1.animationArea.FieldSpell || from == Module1.animationArea.Xyz)
            {
                resetOpXyzLayout();
                if (target != Module1.animationArea.Hand && removeIt == false)
                    resetIt = true;
            }
            else if (to == Module1.animationArea.Monster || to == Module1.animationArea.ST || to == Module1.animationArea.FieldSpell || to == Module1.animationArea.Xyz)
                resetOpXyzLayout();

            ZoneControl zTarget = zImgFromAnimationArea(target, targetIndex, targetIndexXyz);
            Point toPoint; Point fromPoint;

            if (isOpZoneControl(from))
            {
                ZoneControl zFrom = zImgFromAnimationArea(from, fromIndex, fromIndexXyz);
                Point fromTransAdd = getTranslateAdd(zFrom);
                fromPoint = new Point(zFrom.CLeft() + fromTransAdd.X, zFrom.CTop() + fromTransAdd.Y);
                if (isOpZoneControl(to))
                {
                    ZoneControl zTo = zImgFromAnimationArea(to, toIndex, toIndexXyz);
                    Point toTransAdd = getTranslateAdd(zTo);
                    toPoint = new Point(zTo.CLeft() + toTransAdd.X, zTo.CTop() + toTransAdd.Y);
                }
                else
                {
                    Image iTo = ImgFromAnimationArea(to);
                    toPoint = new Point(iTo.CLeft(), iTo.CTop());
                }
            }
            else
            {
                Image iFrom = ImgFromAnimationArea(from);
                fromPoint = new Point(iFrom.CLeft(), iFrom.CTop());
                if (isOpZoneControl(to))
                {
                    ZoneControl zTo = zImgFromAnimationArea(to, toIndex, toIndexXyz);
                    Point toTransAdd = getTranslateAdd(zTo);
                    toPoint = new Point(zTo.CLeft() + toTransAdd.X, zTo.CTop() + toTransAdd.Y);
                }
                else
                {
                    Image iTo = ImgFromAnimationArea(to);
                    toPoint = new Point(iTo.CLeft(), iTo.CTop());
                }
            }
            if (to == Module1.animationArea.Deck || to == Module1.animationArea.Extra || to == Module1.animationArea.Hand)
                zTarget.setEndBehavior(-1 /*facedown*/, "", removeIt, blankIt); 
            else
                zTarget.setEndBehavior(stats,
                                 opNameFromAnimationArea(to, toIndex, toIndexXyz),
                                 removeIt, blankIt);

           zTarget.animationTimer.setInMotion(fromPoint, toPoint, 300, 10, resetIt, zTarget.Name);


        }
        private void watcherAnimation(Module1.animationArea target, int targetIndex, int targetIndexXyz, SQLReference.CardDetails stats,
                                      Module1.animationArea from, int fromIndex,
                                      Module1.animationArea to, int toIndex,
                                      int fromIndexXyz = 0, int toIndexXyz = 0)
        {
            bool removeIt; bool blankIt = false; bool resetIt = false;

            if (to == Module1.animationArea.Hand && from != Module1.animationArea.Hand)
            {
                if (stats != null) stats.Facedown = true;
            }

            if (from == Module1.animationArea.Hand && to != Module1.animationArea.Hand)
                removeIt = true;
            else if (to == Module1.animationArea.Grave && target == Module1.animationArea.Xyz)
                removeIt = true;
            else
            {
                removeIt = false;
                if (to == Module1.animationArea.Grave || to == Module1.animationArea.RFG || to == Module1.animationArea.Extra)
                    blankIt = true;
                else
                    blankIt = false;
            }

            if (from == Module1.animationArea.Monster || from == Module1.animationArea.ST || from == Module1.animationArea.FieldSpell || from == Module1.animationArea.Xyz)
            {
                resetXyzLayout();
                if (target != Module1.animationArea.Hand && removeIt == false)
                    resetIt = true;
            }
            else if (to == Module1.animationArea.Monster || to == Module1.animationArea.ST || to == Module1.animationArea.FieldSpell || to == Module1.animationArea.Xyz)
                resetXyzLayout();

            ZoneControl zTarget = watcherzImgFromAnimationArea(target, targetIndex, targetIndexXyz);
            Point toPoint; Point fromPoint;

            if (isOpZoneControl(from))
            {
                ZoneControl zFrom = watcherzImgFromAnimationArea(from, fromIndex, fromIndexXyz);
                Point fromTransAdd = getTranslateAdd(zFrom);
                fromPoint = new Point(zFrom.CLeft() + fromTransAdd.X, zFrom.CTop() + fromTransAdd.Y);
                if (isOpZoneControl(to))
                {
                    ZoneControl zTo = watcherzImgFromAnimationArea(to, toIndex, toIndexXyz);
                    Point toTransAdd = getTranslateAdd(zTo);
                    toPoint = new Point(zTo.CLeft() + toTransAdd.X, zTo.CTop() + toTransAdd.Y);
                }
                else
                {
                    Image iTo = ImgFromAnimationArea(to);
                    toPoint = new Point(iTo.CLeft(), iTo.CTop());
                }
            }
            else
            {
                Image iFrom = watcherImgFromAnimationArea(from);
                fromPoint = new Point(iFrom.CLeft(), iFrom.CTop());
                if (isOpZoneControl(to))
                {
                    ZoneControl zTo = watcherzImgFromAnimationArea(to, toIndex, toIndexXyz);
                    Point toTransAdd = getTranslateAdd(zTo);
                    toPoint = new Point(zTo.CLeft() + toTransAdd.X, zTo.CTop() + toTransAdd.Y);
                }
                else
                {
                    Image iTo = watcherImgFromAnimationArea(to);
                    toPoint = new Point(iTo.CLeft(), iTo.CTop());
                }
            }
            if (to == Module1.animationArea.Deck || to == Module1.animationArea.Extra || to == Module1.animationArea.Hand)
                zTarget.setEndBehavior(-1 /*facedown*/, "", removeIt, blankIt);
            else
                zTarget.setEndBehavior(stats,
                                 watcherNameFromAnimationArea(to, toIndex, toIndexXyz),
                                 removeIt, blankIt);

            //  zTarget.setEndBehavior(stats, opNameFromAnimationArea(to, toIndex, toIndexXyz), removeIt, blankIt);
            zTarget.animationTimer.setInMotion(fromPoint, toPoint, 300, 10, resetIt, zTarget.Name);

        }
        private static bool isOpZoneControl(Module1.animationArea place)
        {
            if (place == Module1.animationArea.Extra || place == Module1.animationArea.Grave || place == Module1.animationArea.RFG || place == Module1.animationArea.Deck)
                return false;
            else
                return true;
        }
        private static Point getTranslateAdd(ZoneControl zImg)
        {
            if (zImg == null)
                zImg = null;
            switch (zImg.RenderTransform.GetType().ToString())
            {
                case "System.Windows.Media.TranslateTransform":
                    TranslateTransform trans = (TranslateTransform)zImg.RenderTransform;
                    return new Point(trans.X, trans.Y);

                case "System.Windows.Media.CompositeTransform":
                    CompositeTransform cTrans = (CompositeTransform)zImg.RenderTransform;
                    return new Point(cTrans.TranslateX, cTrans.TranslateY);
                default:
                    return new Point(0, 0);
            }
        }
        private ZoneControl zImgFromAnimationArea(Module1.animationArea place, int index, int xyzMatindex = 0)
        {
            switch (place)
            {
                case Module1.animationArea.FieldSpell:
                    return opFieldSpellZone;
                case Module1.animationArea.Monster:
                    return opMonZone(index);
                case Module1.animationArea.ST:
                    return opstZone(index);
                case Module1.animationArea.Hand:
                    return opimgHand(index);
                case Module1.animationArea.Xyz:
                    return opimgXyz(index, xyzMatindex);
                default:
                    return null;
            }
        }
        private ZoneControl watcherzImgFromAnimationArea(Module1.animationArea place, int index, int xyzMatindex = 0)
        {
            switch (place)
            {
                case Module1.animationArea.FieldSpell:
                    return FieldSpellZone;
                case Module1.animationArea.Monster:
                    return MonZone(index);
                case Module1.animationArea.ST:
                    return stZone(index);
                case Module1.animationArea.Hand:
                    return imgHand(index);
                case Module1.animationArea.Xyz:
                    return imgXyz(index, xyzMatindex);
                default:
                    return null;
            }
        }
        Image ImgFromAnimationArea(Module1.animationArea place)
        {
            switch (place)
            {
                case Module1.animationArea.Grave:
                    return opGraveZone;
                case Module1.animationArea.RFG:
                    return oprfgZone;
                case Module1.animationArea.Extra:
                    return opExtraDeckZone;
                case Module1.animationArea.Deck:
                    return opDeckZone;
                default:
                    return null;
            }
        }
        Image watcherImgFromAnimationArea(Module1.animationArea place)
        {
            switch (place)
            {
                case Module1.animationArea.Grave:
                    return GraveZone;
                case Module1.animationArea.RFG:
                    return rfgZone;
                case Module1.animationArea.Extra:
                    return ExtraDeckZone;
                case Module1.animationArea.Deck:
                    return DeckZone;
                default:
                    return null;
            }
        }
        private static string opNameFromAnimationArea(Module1.animationArea place, int index, int xyzIndex = 0)
        {
            switch (place)
            {
                case Module1.animationArea.Hand:
                    return "opimgHand" + index.ToString();
                case Module1.animationArea.Monster:
                    return "opMonZone" + index.ToString();
                case Module1.animationArea.ST:
                    return "opstZone" + index.ToString();
                case Module1.animationArea.Grave:
                    return "opGraveZone";
                case Module1.animationArea.RFG:
                    return "oprfgZone";
                case Module1.animationArea.FieldSpell:
                    return "opFieldSpellZone";
                case Module1.animationArea.Extra:
                    return "opExtraDeckZone";
                case Module1.animationArea.Xyz:
                    return "opXyzMat_" + index.ToString() + "_" + xyzIndex.ToString();
                default:
                    return "";
            }
            
        }
        private static string watcherNameFromAnimationArea(Module1.animationArea place, int index, int xyzIndex = 0)
        {
            switch (place)
            {
                case Module1.animationArea.Hand:
                    return "imgHand" + index.ToString();
                case Module1.animationArea.Monster:
                    return "MonZone" + index.ToString();
                case Module1.animationArea.ST:
                    return "stZone" + index.ToString();
                case Module1.animationArea.Grave:
                    return "GraveZone";
                case Module1.animationArea.RFG:
                    return "rfgZone";
                case Module1.animationArea.FieldSpell:
                    return "FieldSpellZone";
                case Module1.animationArea.Extra:
                    return "ExtraDeckZone";
                case Module1.animationArea.Xyz:
                    return "XyzMat_" + index.ToString() + "_" + xyzIndex.ToString();
                default:
                    return "";
            }

        }
        private static Module1.animationArea animationAreaFromString(string place)
        {
            if (place.Contains("imgHand"))
                    return Module1.animationArea.Hand;

                if (place.Contains("MonZone"))
                    return Module1.animationArea.Monster;
              if (place.Contains("stZone"))
                    return Module1.animationArea.ST;
               if (place.Contains("GraveZone"))
                    return Module1.animationArea.Grave;
              if (place.Contains("rfgZone"))
                    return Module1.animationArea.RFG;
             if (place.Contains("ExtraDeckZone"))
                    return Module1.animationArea.Extra;
               if (place.Contains("FieldSpellZone"))
                    return Module1.animationArea.FieldSpell;
               
             return Module1.animationArea.None;

          
        }
        #endregion
        

        private void lblOpponentLP_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (goingToAttackZone > 0)
            {
                Attack(0);
            }
        }

        private void Image11_MouseLeftButtonUp(System.Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (goingToAttackZone > 0)
            {
                Attack(0);
            }
        }

        private void lblOpponentLP_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (goingToAttackZone > 0)
            {
                addImgBattleToZone("lblOpponentLP", ref imgBattle);
            }
        }

        private void resetStats()
        {
            Module1.PlayerCurrentHand.ClearAndAdd(true);
           if (!Module1.noInternet) Module1.PlayerCurrentDeck.ClearAndAdd(true);
            Module1.PlayerCurrentGrave.ClearAndAdd(true);
            Module1.PlayerCurrentRFG.ClearAndAdd(true);
            Module1.PlayerCurrentEDeck.ClearAndAdd(true);
            Module1.PlayerCurrentMonsters.ClearArray();
            Module1.PlayerCurrentST.ClearArray();
            Module1.PlayerCurrentFSpell = new SQLReference.CardDetails();

            for (short n = 1; n <= Module1.realDeckLength; n++)
                Module1.PlayerCurrentDeck.Add(Module1.CardStats[Module1.realDeckIDs[n]]);

            for (short n = 1; n <= Module1.realEDeckLength; n++)
                Module1.PlayerCurrentEDeck.Add(Module1.CardStats[Module1.realEDeckIDs[n]]);
            
        }


private void lstMyHand_MouseEnter(System.Object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Module1.TagDuel)
            {
                lstMyHand.Width = 240;
            }
        }
private void lstMyHand_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Module1.TagDuel)
            {
                lstMyHand.Width = 129;
            }
        }
#region "Edit Fields"
private void lblDuelATK_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Module1.IamWatching) { return; }
            if (Module1.isNumeric(lblDuelATK.Text) && !DontFireChangeEventATK && MonSelectedZone > 0)
            {
                ToChangeATK = true;
                Module1.PlayerCurrentMonsters[MonSelectedZone].ATK = lblDuelATK.Text.ToIntCountingQuestions();
            }
            else { DontFireChangeEventATK = false; }
        }
private void lblDuelDEF_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Module1.IamWatching) { return; }
            if (Module1.isNumeric(lblDuelDEF.Text) && !DontFireChangeEventDEF && MonSelectedZone > 0)
            {
                ToChangeDEF = true;
                Module1.PlayerCurrentMonsters[MonSelectedZone].DEF = lblDuelDEF.Text.ToIntCountingQuestions();
            }
            else { DontFireChangeEventDEF = false; }
        }
private void lblDuelATK_GotFocus(object sender, RoutedEventArgs e)
        {
            DontFireChangeEventATK = false;
        }
private void lblDuelDEF_GotFocus(object sender, RoutedEventArgs e)
        {
            DontFireChangeEventDEF = false;
        }
private void cmbCounters_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Module1.IamWatching) { return; }
            if (DontFireChangeEventCounters) { DontFireChangeEventCounters = false; return; }
            if (Module1.isNumeric(cmbCounters.Text.Trim()))
            {
                string area = Module1.DeriveAreaFromGlobalZone();
                switch (area)
                {
                    case "Monster":
                        Module1.PlayerCurrentMonsters[Module1.GlobalZone].Counters = Convert.ToSByte(cmbCounters.Text.Trim());
                        SummarizeJabber(Area1: "Monster", Index1: (short)Module1.GlobalZone, TheEvent: "Changed Counters", Text: Module1.GlobalZone.ToString());
                        break;
                    case "ST":
                        Module1.PlayerCurrentST[Module1.GlobalZone - 5].Counters = Convert.ToSByte(cmbCounters.Text.Trim());
                        SummarizeJabber(Area1: "ST", Index1: (short)(Module1.GlobalZone - 5), TheEvent: "Changed Counters", Text: Module1.GlobalZone.ToString());
                        break;
                    case "FSpell":
                        Module1.PlayerCurrentFSpell.Counters = Convert.ToSByte(cmbCounters.Text.Trim());
                        SummarizeJabber(Area1: "FSpell", TheEvent: "Changed Counters", Text: Module1.GlobalZone.ToString());
                        break;
                }
                

            }
        }
private void cmbCounters_GotFocus(object sender, RoutedEventArgs e)
        {
            DontFireChangeEventCounters = false;
        }
private void SeeChangeStatsAndFire()
{
    if (Module1.IamWatching)
        return;
    if (ToChangeATK == true)
    {
        ToChangeATK = false;

        SummarizeJabber(Area1: "Monster", Index1: MonSelectedZone, Element1: "ATK", TheEvent: "Changed ATK", Text: Module1.PlayerCurrentMonsters[MonSelectedZone].Name);

    }
    if (ToChangeDEF == true)
    {
        ToChangeDEF = false;
        SummarizeJabber(Area1: "Monster", Index1: MonSelectedZone, Element1: "DEF", TheEvent: "Changed DEF", Text: Module1.PlayerCurrentMonsters[MonSelectedZone].Name);
    }
}
private void cmdDetatch_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOverlaid.SelectionBoxItem != null)
            {
                DetatchXyzMaterial(MonSelectedZone, (short)(cmbOverlaid.SelectedIndex + 1));
                cmbOverlaid.Items.RemoveAt(cmbOverlaid.SelectedIndex);
           
            }
        }

#endregion

public static short FindEmptyMonZone()
        {
            short n = 0;
            for (n = 1; n <= 5; n++)
            {
                if (string.IsNullOrEmpty(Module1.PlayerCurrentMonsters[n].Name))
                {
                    return n;
                }
            }
            return n;

        }
public static short FindEmptySTZone()
        {
            short n = 0;
            for (n = 1; n <= 5; n++)
            {
                if (string.IsNullOrEmpty(Module1.PlayerCurrentST[n].Name))
                {
                    return n;
                }
            }
            return n;
        }
private static void setAsNothing(SQLReference.CardDetails stats)
        {
            if (stats == null)
            { return; }
            stats.Name = null;
            stats.Level = 0;
            stats.Type = null;
            stats.Attribute = null;
            stats.Facedown = false;
            stats.IsItHorizontal = false;
            stats.Lore = null;
            stats.ATK = 0;
            stats.DEF = 0;
            stats.ID = 0;
            stats.SpecialSet = null;

            stats.Creator = null;
            stats.Counters = 0;
            stats.OpponentOwned = false;
        }




        private void button1_Click(object sender, RoutedEventArgs e)
        {
            testTimer = new System.Windows.Threading.DispatcherTimer();
            testTimer.Tick += new EventHandler(testTimer_Tick);
            testTimer.Interval = new TimeSpan(0, 0, 3);
            testTimer.Start();
        }

        void testTimer_Tick(object sender, EventArgs e)
        {

            if (Module1.PlayerCurrentHand.CountNumCards() > 0)
                SummonOrActivateFromHand((short)Module1.PlayerCurrentHand.CountNumCards());
            else
            { if (Module1.PlayerCurrentMonsters[1].Name != null) Bounce(1); }
            
        }
        private MainPage MyMainPage
        {
            get
            {
                return ((MainPage)((Canvas)((Border)((Frame)Parent).Parent).Parent).Parent);
            }
        }
        private void bordDuelChat_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.GeneralTransform gt = bordDuelChat.TransformToVisual(Application.Current.RootVisual);
            Point offset = gt.Transform(new Point(0, 0));
            MyMainPage.floatChatReal.Position = offset;
            MyMainPage.floatChatReal.Width = bordDuelChat.Width;
            MyMainPage.floatChatReal.Height = bordDuelChat.Height;
        }

        private void cmdRefresh_Click(object sender, RoutedEventArgs e)
        {
            ZoneControl zImg;
            for (int n = 1; n <= 5; n++)
            {

                zImg = MonZone(n); UpdatePictureBoxDuelField(ref zImg.baseImage, zImg.Name, Module1.PlayerCurrentMonsters[n], Module1.mySet);
                zImg = stZone(n); UpdatePictureBoxDuelField(ref zImg.baseImage, zImg.Name, Module1.PlayerCurrentST[n], Module1.mySet);
                zImg = opMonZone(n); UpdatePictureBoxDuelField(ref zImg.baseImage, zImg.Name, Module1.OpponentCurrentMonsters[n], opponentSet);
                zImg = opstZone(n); UpdatePictureBoxDuelField(ref zImg.baseImage, zImg.Name, Module1.OpponentCurrentST[n], opponentSet);

                updateXyzPositions((short)n);
                updateopXyzPositions((short)n);
            }
            UpdatePictureBoxDuelField(ref FieldSpellZone.baseImage, FieldSpellZone.Name, Module1.PlayerCurrentFSpell, Module1.mySet);
            UpdatePictureBoxDuelField(ref opFieldSpellZone.baseImage, opFieldSpellZone.Name, Module1.OpponentCurrentFSpell, opponentSet);
        
            if (Module1.PlayerCurrentGrave.CountNumCards() > 0)
                UpdatePictureBoxDuelField(ref GraveZone, GraveZone.Name, Module1.PlayerCurrentGrave[Module1.PlayerCurrentGrave.CountNumCards()], Module1.mySet);
            else
                Module1.setImage(ref GraveZone, BLANK_IMAGE, UriKind.Relative);

            if (Module1.PlayerCurrentRFG.CountNumCards() > 0)
                UpdatePictureBoxDuelField(ref rfgZone, rfgZone.Name, Module1.PlayerCurrentRFG[Module1.PlayerCurrentRFG.CountNumCards()], Module1.mySet);
            else
                Module1.setImage(ref rfgZone, BLANK_IMAGE, UriKind.Relative);


            if (Module1.OpponentCurrentGrave.CountNumCards() > 0)
                UpdatePictureBoxDuelField(ref opGraveZone, opGraveZone.Name, Module1.OpponentCurrentGrave[Module1.OpponentCurrentGrave.CountNumCards()], opponentSet);
            else
                Module1.setImage(ref opGraveZone, BLANK_IMAGE, UriKind.Relative);

            if (Module1.OpponentCurrentRFG.CountNumCards() > 0)
                UpdatePictureBoxDuelField(ref oprfgZone, oprfgZone.Name, Module1.OpponentCurrentRFG[Module1.OpponentCurrentRFG.CountNumCards()], opponentSet);
            else
                Module1.setImage(ref oprfgZone, BLANK_IMAGE, UriKind.Relative);


            updateImgHandPositions();
            updateOpImgHandPositions();


            MyMainPage.refreshFloatChat(bordDuelChat.Width, bordDuelChat.Height);
        }

      
        private void LayoutRoot_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void lstMessages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


     
    }

}
