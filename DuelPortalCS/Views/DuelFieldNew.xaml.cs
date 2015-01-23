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
using DuelPortalCS;
using DuelPortalCS.SQLReference;
namespace DuelPortalCS.Views
{
    public partial class DuelFieldNew : Page
    {
        public const string BLANK_IMAGE = null;
        public const int TOKEN_ID = -2;
        public System.Windows.Threading.DispatcherTimer testTimer;
        public frmView viewForm;
        public bool isDraggingViewForm = false;
        public Point viewFormDragStartPoint;
        public class DraggingImage
        {
            public Image _image;
            public Point _offset;
            public bool _isXyz;
            public DraggingImage(Image image, Point offset, bool isXyz = false) { _image = image; _offset = offset; _isXyz = isXyz; 
                                    _image.IsHitTestVisible = false;}
        }
        public List<DraggingImage> dragImages = new List<DraggingImage>();
        public int imageDragIndexXyz = 0;
        public int imageDragIndex = 0;
        public Area imageDragArea = Area.None;
        public Point imageDragOffset;

        int TurnCount = 1;
        public bool DontFireTurnCount;
        public int ZoneofSwitch = -1;
        public int ZoneofEdit = 0;
        public string WatcherCurrentlyPinging = M.WatcherMySide;
        public string watcherMySideSet;
        public string opponentSet;

        private bool _playerIsConnected = false;
        public bool playerIsConnected
        {
            get
            {
                return _playerIsConnected;
            }
            set
            {
                _playerIsConnected = value;
                cmdDrawPhase.IsEnabled = _playerIsConnected; cmdStandbyPhase.IsEnabled = _playerIsConnected; cmdMainPhase1.IsEnabled = _playerIsConnected; cmdBattlePhase.IsEnabled = _playerIsConnected; cmdMainPhase2.IsEnabled = _playerIsConnected; cmdEndPhase.IsEnabled = _playerIsConnected; cmdEndTurn.IsEnabled = _playerIsConnected;
                txtTurnCount.IsEnabled = _playerIsConnected;
                cmdGainLP.IsEnabled = _playerIsConnected; cmdLoseLP.IsEnabled = _playerIsConnected;
            }
        }

        public HashSet<string> watchersReceived = new HashSet<string>();

        public System.Windows.Threading.DispatcherTimer PingTimer;
        //The Zone of the attacking monster
        public int goingToAttackZone = 0;
        //The Zone of the card to move. global.
        public int goingToMoveZone = 0;

        public const int ANIMATION_SPEED_MS = 500;


        Queue<AnimationData> AnimationQueue = new Queue<AnimationData>();
        public bool isAnimating = false;

        public int pingTimerNumber = 0;
        public bool allowReconnect = false;


        public DuelFieldNew()
        {
            InitializeComponent();
           
        }

        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            M.ScaleCanvas(LayoutRoot, this);
            M.ScreenResized += delegate
            {
                M.ScaleCanvas(LayoutRoot, this);
            };

            M.PlayerGrave.ClearAndAdd();
            M.PlayerRFG.ClearAndAdd();
            M.OpponentGrave.ClearAndAdd();
            M.OpponentRFG.ClearAndAdd();
            M.PlayerHand.ClearAndAdd();
            M.PlayerLP = 8000;
            lblPlayerLP.Text = "LP: 8000";
            lblOpponentLP.Text = "LP: 8000";

            lblPlayerName.Text = M.IamWatching ? M.WatcherMySide : M.username;
            lblOpponentName.Text = M.IamWatching ? M.WatcherOtherSide : M.opponent;


            cmdShuffleHand.Content = "Shuffle" + Environment.NewLine + "Hand";
            cmdEndTurn.Content = "End" + Environment.NewLine + "Turn";
 
            System.Windows.Browser.HtmlPage.Document.SetProperty("title", "Duel Portal Online");
            M.warnOnExitMessage = "Are you sure you want to quit this duel?";
            if (MyMainPage.floatChatReal != null) MyMainPage.floatChatReal.Close();



            sendDelegate = new Action<string>(sendMessageInvoke);
            timerDelegate = new Action(startPingTimer);
            addMessageInvoke = new Action<string, bool>(addMessage);

            ctxtDeck.myArea = ContextMenu.Area.Deck;
            ctxtDeck.onLoaded();
            ctxtDeck.RenderTransform = new TranslateTransform();
            ctxtDeck.Visibility = System.Windows.Visibility.Collapsed;

            ctxtHand.myArea = ContextMenu.Area.Hand;
            ctxtHand.onLoaded();
            ctxtHand.RenderTransform = new TranslateTransform();
            ctxtHand.Visibility = System.Windows.Visibility.Collapsed;

            ctxtMonster.myArea = ContextMenu.Area.Monster_Full;
            ctxtMonster.onLoaded();
            ctxtMonster.RenderTransform = new TranslateTransform();
            ctxtMonster.Visibility = System.Windows.Visibility.Collapsed;

            ctxtMonsterEmpty.myArea = ContextMenu.Area.Monster_Empty;
            ctxtMonsterEmpty.onLoaded();
            ctxtMonsterEmpty.RenderTransform = new TranslateTransform();
            ctxtMonsterEmpty.Visibility = System.Windows.Visibility.Collapsed;

            ctxtSpellTrap.myArea = ContextMenu.Area.ST;
            ctxtSpellTrap.onLoaded();
            ctxtSpellTrap.RenderTransform = new TranslateTransform();
            ctxtSpellTrap.Visibility = System.Windows.Visibility.Collapsed;

            ctxtXyz.myArea = ContextMenu.Area.Xyz;
            ctxtXyz.onLoaded();
            ctxtXyz.RenderTransform = new TranslateTransform();
            ctxtXyz.Visibility = System.Windows.Visibility.Collapsed;
            ctxtXyz.Item_ClickedXyz += ctxtXyz_Item_ClickedXyz;

            imgBattleOrigin.RenderTransform = new CompositeTransform();
            imgBattleOrigin.RenderTransform.SetValue(CompositeTransform.CenterXProperty, imgBattleOrigin.Width / 2);
            imgBattleOrigin.RenderTransform.SetValue(CompositeTransform.CenterYProperty, imgBattleOrigin.Height / 2);

            for (int n = 1; n <= 5; n++)
            {

                BordMon(n).RenderTransform = new RotateTransform();
                BordMon(n).RenderTransform.SetValue(RotateTransform.CenterXProperty, BordMon(n).Width / 1.5);
                BordMon(n).RenderTransform.SetValue(RotateTransform.CenterYProperty, BordMon(n).Height / 2);
                BordOpMon(n).RenderTransform = new RotateTransform();
                BordOpMon(n).RenderTransform.SetValue(RotateTransform.CenterXProperty, BordOpMon(n).Width / 1.5);
                BordOpMon(n).RenderTransform.SetValue(RotateTransform.CenterYProperty, BordOpMon(n).Height / 2);

                BordST(n).RenderTransform = new RotateTransform();
                BordST(n).RenderTransform.SetValue(RotateTransform.CenterXProperty, BordST(n).Width / 2);
                BordST(n).RenderTransform.SetValue(RotateTransform.CenterYProperty, BordST(n).Height / 2);
                BordOpST(n).RenderTransform = new RotateTransform();
                BordOpST(n).RenderTransform.SetValue(RotateTransform.CenterXProperty, BordOpST(n).Width / 2);
                BordOpST(n).RenderTransform.SetValue(RotateTransform.CenterYProperty, BordOpST(n).Height / 2);

                M.PlayerMonsters[n] = new SQLReference.CardDetails();
                M.PlayerST[n] = new SQLReference.CardDetails();
                M.OpponentMonsters[n] = new SQLReference.CardDetails();
                M.OpponentST[n] = new SQLReference.CardDetails();
                M.PlayerOverlaid[n] = new List<SQLReference.CardDetails>(); M.PlayerOverlaid[n].ClearAndAdd();
                M.OpponentOverlaid[n] = new List<SQLReference.CardDetails>(); M.OpponentOverlaid[n].ClearAndAdd();


            }
            BordMon(1).MouseLeftButtonDown += (s,e2)=> { BordField_MouseLeftButtonDown(Area.Monster, 1, e2); };
            BordST(1).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.ST, 1, e2); };
            BordMon(2).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.Monster, 2, e2); };
            BordST(2).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.ST, 2, e2); };
            BordMon(3).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.Monster, 3, e2); };
            BordST(3).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.ST, 3, e2); };
            BordMon(4).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.Monster, 4, e2); };
            BordST(4).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.ST, 4, e2); };
            BordMon(5).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.Monster, 5, e2); };
            BordST(5).MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.ST, 5, e2); };
            BordFSpell.MouseLeftButtonDown += (s, e2) => { BordField_MouseLeftButtonDown(Area.FieldSpell, 0, e2); };
            BordFSpell.RenderTransform = new RotateTransform();
            BordFSpell.RenderTransform.SetValue(RotateTransform.CenterXProperty, BordFSpell.Width / 2);
            BordFSpell.RenderTransform.SetValue(RotateTransform.CenterYProperty, BordFSpell.Height / 2);
            BordOpFSpell.RenderTransform = new RotateTransform();
            BordOpFSpell.RenderTransform.SetValue(RotateTransform.CenterXProperty, BordOpFSpell.Width / 2);
            BordOpFSpell.RenderTransform.SetValue(RotateTransform.CenterYProperty, BordOpFSpell.Height / 2);

            viewForm = new frmView();
            viewForm.MouseLeftButtonDown += viewForm_MouseLeftButtonDown;
            viewForm.MouseLeftButtonUp += viewForm_MouseLeftButtonUp;
            viewForm.MouseMove += viewForm_MouseMove;
            viewForm.Closed += viewForm_Closed;


            if (M.IamWatching)
            {
                cmdDrawPhase.IsEnabled = false;
                cmdStandbyPhase.IsEnabled = false;
                cmdMainPhase1.IsEnabled = false;
                cmdBattlePhase.IsEnabled = false;
                cmdMainPhase2.IsEnabled = false;
                cmdEndPhase.IsEnabled = false;
                cmdEndTurn.Visibility = System.Windows.Visibility.Collapsed;
                cmdGainLP.Visibility = System.Windows.Visibility.Collapsed;
                cmdLoseLP.Visibility = System.Windows.Visibility.Collapsed;
                txtLPChange.Visibility = System.Windows.Visibility.Collapsed;
                cmdShuffleHand.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {

                M.Shuffle();
            }


            if (M.PlayerEDeck.CountNumCards() > 0)
                M.setImage(BordEDeck, "back.jpg", UriKind.Relative);



           
            if (M.noInternet) WatcherCurrentlyPinging = "b";

            if (M.IamWatching)
            {
                M.WatcherHand = new Dictionary<int, CardDetails>();
            }
            else
            {
                playerIsConnected = false;
                SQLReference.Service1ConsoleClient client = new Service1ConsoleClient();
                client.getAvatarImageCompleted += (s, e2) =>
                {
                    if (e2.Error == null && !string.IsNullOrEmpty(e2.Result))
                        M.setImage(PlayerAvatar, e2.Result, UriKind.Absolute);
                };
                client.getAvatarImageAsync(M.myUsernameId);
                
                
            }

            //playerIsConnected = true; *soon return;

            M.sock.SendMessage(M.socketSerialize("Server", M.username, M.myRoomID, MessageType.DuelEnter));
            startPingTimer();
        }

        private DuelEvents getDuelEventFromAreas(Area from, Area to, bool toBottomOfDeck)
        {
            switch (from)
            {
                case Area.Grave:
                    switch (to)
                    {
                        case Area.Hand:
                            return DuelEvents.Grave_To_Hand;
                        case Area.Monster:
                            return DuelEvents.Grave_To_Monster;
                        case Area.ST:
                            return DuelEvents.Grave_To_ST;
                        case Area.FieldSpell:
                            return DuelEvents.Grave_To_ST;
                        case Area.Deck:
                            return toBottomOfDeck ? DuelEvents.Grave_To_Bottom : DuelEvents.Grave_To_Top;
                        case Area.RFG:
                            return DuelEvents.Grave_To_RFG;
                        case Area.Extra:
                            return DuelEvents.Grave_To_Extra;
                    }
                    break;
                case Area.RFG:
                    switch (to)
                    {
                        case Area.Hand:
                            return DuelEvents.RFG_To_Hand;
                        case Area.Monster:
                            return DuelEvents.RFG_To_Monster;
                        case Area.ST:
                            return DuelEvents.RFG_To_ST;
                        case Area.FieldSpell:
                            return DuelEvents.RFG_To_ST;
                        case Area.Deck:
                            return toBottomOfDeck ? DuelEvents.RFG_To_Bottom : DuelEvents.RFG_To_Top;
                        case Area.Grave:
                            return DuelEvents.RFG_To_Grave;
                        case Area.Extra:
                            return DuelEvents.RFG_To_Extra;
                    }
                    break;
                case Area.Deck:
                        switch (to)
                        {
                            case Area.Deck:
                                return toBottomOfDeck ? DuelEvents.Deck_To_Bottom : DuelEvents.Deck_To_Top;
                            case Area.Hand:
                                return DuelEvents.Deck_To_Hand;
                            case Area.Monster:
                                return DuelEvents.Deck_To_Monster;
                            case Area.ST:
                                return DuelEvents.Deck_To_ST;
                            case Area.FieldSpell:
                                return DuelEvents.Deck_To_ST;
                            case Area.Grave:
                                return DuelEvents.Deck_To_Grave;
                            case Area.RFG:
                                return DuelEvents.Deck_To_RFG;
                        }
                    break;
                case Area.Extra:
                        switch (to)
                        {
                            case Area.Monster:
                                return DuelEvents.Extra_To_Monster;
                            case Area.ST:
                                return DuelEvents.Extra_To_ST;
                            case Area.FieldSpell:
                                return DuelEvents.Extra_To_ST;
                            case Area.Grave:
                                return DuelEvents.Extra_To_Grave;
                            case Area.RFG:
                                return DuelEvents.Extra_To_RFG;
                        }
                    break;
            }
            return DuelEvents.None;
        }

        void DestroyFieldSpell()
        {
            CardDetails stats = MoveStats(Area.FieldSpell, Area.Grave, 1);
            animationBundle bundle = createBundle(Area.FieldSpell, Area.Grave, true);
            Animate(bundle, stats, DuelEvents.ST_To_Grave);
            SummarizeJabber(Area1: Area.FieldSpell,
                Area2: Area.Grave, Index2: M.PlayerGrave.CountNumCards(),
                duelEvent: DuelEvents.ST_To_Grave, Text: stats.Name,
                bundle: bundle);
        }
        Area getAreaFromZone(int zone)
        {
            if (zone >= 1 & zone <= 5)
                return Area.Monster;
            if (zone >= 6 & zone <= 10)
                return Area.ST;
            if (zone == 11)
                return Area.FieldSpell;

            return Area.None;
        }
        CardDetails getStatsFromArea(Area area, int index, bool isPlayer, int xyzIndex = 0)
        {
            if (isPlayer)
            {
                switch (area)
                {
                    case Area.Deck:
                        return M.IamWatching ? null : M.PlayerDeck[index];
                    case Area.Extra:
                        return M.IamWatching ? null : M.PlayerEDeck[index];
                    case Area.Hand:
                        return (M.IamWatching ? ( M.WatcherHand != null && M.WatcherHand.ContainsKey(index) ? M.WatcherHand[index] : null)  : M.PlayerHand[index]);
                    case Area.Monster:
                        return M.PlayerMonsters[index];
                    case Area.ST:
                        return M.PlayerST[index];
                    case Area.FieldSpell:
                        return M.PlayerFSpell;
                    case Area.Grave:
                    case Area.rmGrave:
                        return M.PlayerGrave[M.PlayerGrave.CountNumCards()];
                    case Area.RFG:
                    case Area.rmRFG:
                        return M.PlayerRFG[M.PlayerRFG.CountNumCards()];
                    case Area.Xyz:
                        if (xyzIndex > 0)
                            return M.PlayerOverlaid[index][xyzIndex];
                        else
                            throw new Exception("Invalid Parameters");

                }
            }
            else
            {
                switch (area)
                {
                    case Area.Deck:
                    case Area.Extra:
                        return null;
                    case Area.Hand:
                        return (M.OpponentHand != null && M.OpponentHand.ContainsKey(index)) ? M.OpponentHand[index] : null;
                    case Area.Monster:
                        return M.OpponentMonsters[index];
                    case Area.ST:
                        return M.OpponentST[index];
                    case Area.FieldSpell:
                        return M.OpponentFSpell;
                    case Area.Grave:
                    case Area.rmGrave:
                        return M.OpponentGrave[M.OpponentGrave.CountNumCards()];
                    case Area.RFG:
                    case Area.rmRFG:
                        return M.OpponentRFG[M.OpponentRFG.CountNumCards()];
                    case Area.Xyz:
                        if (xyzIndex > 0)
                            return M.OpponentOverlaid[index][xyzIndex];
                        else
                            throw new Exception("Invalid Parameters");

                }
            }
            return null;
        }
        int getIndexFromZone(Area area, int globalZone)
        {
            switch (area)
            {
                case Area.Monster:
                    return globalZone;
                case Area.ST:
                    return globalZone - 5;
                default:
                    return -1;
            }
        }
        int getGlobalFromZone(Area area, int localIndex)
        {
            switch (area)
            {
                case Area.Monster:
                    return localIndex;
                case Area.ST:
                    return localIndex + 5;
                default:
                    return 11;
            }
        }
        DuelEvents getEventFromMoveZones(Area fromArea, Area toArea)
        {
            switch (fromArea)
            {
                case Area.Monster:
                    switch (toArea)
                    {
                        case Area.Monster:
                            return DuelEvents.Monster_Move;
                        case Area.ST:
                            return DuelEvents.Monster_Move_To_ST;
                        case Area.FieldSpell:
                            return DuelEvents.Monster_Move_To_FSpell;
                    }
                    break;
                case Area.ST:
                    switch (toArea)
                    {
                        case Area.Monster:
                            return DuelEvents.ST_Move_To_Monster;
                        case Area.ST:
                            return DuelEvents.ST_Move;
                        case Area.FieldSpell:
                            return DuelEvents.ST_Move_To_FSpell;
                    }
                    break;
                case Area.FieldSpell:
                    switch (toArea)
                    {
                        case Area.Monster:
                            return DuelEvents.FSpell_Move_To_Monster;
                        case Area.ST:
                            return DuelEvents.FSpell_Move_To_ST;
                    }
                    break;
            }
            return DuelEvents.None;
        }
        /// <summary>
        /// From Zone = goingToMoveZone
        /// </summary>
        /// <param name="toZone"></param>
        void moveCard(int toZone)
        {

            


            CardDetails stats = null; animationBundle bundle = null;
                Area fromArea = getAreaFromZone(goingToMoveZone);
                Area toArea = getAreaFromZone(toZone);
                int fromIndex = getIndexFromZone(fromArea, goingToMoveZone);
                int toIndex = getIndexFromZone(toArea, toZone);

                if (fromArea == Area.Monster && M.PlayerOverlaid[fromIndex].CountNumCards() > 0)
                {
                    if (toArea == Area.Monster)
                    {
                        animationBundle matBundle = new animationBundle();
                        matBundle.isPlayer = true;
                        matBundle.fromIndex = fromIndex;
                        matBundle.toIndex = toIndex;
                        matBundle.specialAnimation = SpecialAnimation.MoveXyz;
                        Animate(matBundle, null, DuelEvents.None);


                        int matIndex = 1;
                        while (M.PlayerOverlaid[fromIndex].CountNumCards() > 0)
                        {
                            MoveStats(Area.Xyz, Area.Xyz, fromIndex, toIndex, 1, matIndex);
                            matIndex++;
                        }

                        SummarizeJabber(Area1: Area.Xyz, Index1: fromIndex,
                             Area2: Area.Xyz, Index2: toIndex,
                             bundle: matBundle);
                    }
                    else
                        detachAllMaterial(fromIndex, true);
                }


                DuelEvents duelEvent = getEventFromMoveZones(fromArea, toArea);
                bool wasFacedown = getStatsFromArea(fromArea, fromIndex, true).Facedown;
                stats = MoveStats(fromArea, toArea, fromIndex, toIndex);
                bundle = createBundle(fromArea, toArea, true, fromIndex, toIndex);
                Animate(bundle, stats, duelEvent);
                
                SummarizeJabber(Area1: fromArea, Index1: fromIndex, Area2: toArea, Index2: toIndex,
                    duelEvent: duelEvent, Text: wasFacedown ? null : stats.Name, bundle: bundle);
                goingToMoveZone = 0;
                imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
                imgMoveDestination.Visibility = System.Windows.Visibility.Collapsed;
        }
        void overlayCard(int toZone, int fromZone)
        {
            CardDetails stats = null; animationBundle bundle = null;
            Area fromArea = getAreaFromZone(fromZone);
            int fromIndex = getIndexFromZone(fromArea, fromZone);
            stats = MoveStats(fromArea, Area.Xyz, fromIndex, toZone);
            bundle = createBundle(fromArea, Area.Xyz, true, fromIndex, toZone);
            Animate(bundle, stats, DuelEvents.Attach_Material);
            SummarizeJabber(Area1: fromArea, Index1: fromIndex, Area2: Area.Xyz, Index2: toZone,
                duelEvent: DuelEvents.Attach_Material, Text: M.PlayerMonsters[toZone].Name,
                bundle: bundle);
            goingToMoveZone = 0;
            imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
            imgMoveDestination.Visibility = System.Windows.Visibility.Collapsed;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attackerZone"></param>
        /// <param name="defenderZone">6 = Direct Attack</param>
        void attack(int attackerZone, int defenderZone, bool playerIsAttacking, Point mousePoint)
        {
            LayoutRoot.MouseMove -= LayoutRoot_MouseMove_Attack;
            Point originPoint, targetPoint;
           
            if (playerIsAttacking)
            {
                originPoint = getCenterOfAttackTarget(true, attackerZone);
                targetPoint = new Point(mousePoint.X - imgBattleOrigin.Width / 2, mousePoint.Y - imgBattleOrigin.Height / 2);
                goingToAttackZone = 0;
                if (defenderZone == 6)
                    SummarizeJabber(duelEvent: DuelEvents.Attack_Direct, Text: attackerZone.ToString());
                else
                    SummarizeJabber(duelEvent: DuelEvents.Attack, Text: attackerZone.ToString() + "|" + defenderZone.ToString());

            }else{
                targetPoint = getCenterOfAttackTarget(true, defenderZone);
                originPoint = getCenterOfAttackTarget(false, attackerZone); 
                showLabelikeImage(imgBattleOrigin, BordOpMon(attackerZone));
                rotateSwordToPoint(targetPoint);
            }

            AttackAnimation(originPoint, targetPoint);
          
        }
        void positionSword(int zone)
        {
            PositionWithoutTransforms(imgBattleOrigin, imgBattleOrigin.RenderTransform as CompositeTransform);
            Point point = getCenterOfAttackTarget(true, zone);
            Canvas.SetLeft(imgBattleOrigin, point.X);
            Canvas.SetTop(imgBattleOrigin, point.Y);
        }
        void rotateSwordToPoint(Point destination)
        {
            PositionWithoutTransforms(imgBattleOrigin, imgBattleOrigin.RenderTransform as CompositeTransform);
            Point origin = new Point(imgBattleOrigin.CLeft(), imgBattleOrigin.CTop());
            imgBattleOrigin.RenderTransform.SetValue(CompositeTransform.RotationProperty, GetRotationAngle(origin, destination));

        }
        /// <summary>
        /// Choose zone=6 for avatars
        /// </summary>
        /// <param name="isPlayer"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        Point getCenterOfAttackTarget(bool isPlayer, int zone)
        {
            if (isPlayer)
            {
                if (zone == 6)
                    return new Point(PlayerAvatar.CLeft() + (PlayerAvatar.Width / 2) - (imgBattleOrigin.Width / 2),
                                     PlayerAvatar.CTop() + (PlayerAvatar.Height / 2) - (imgBattleOrigin.Height / 2));
                else
                    return new Point(BordMon(zone).CLeft() + (BordMon(zone).Width / 2) - (imgBattleOrigin.Width / 2),
                                     BordMon(zone).CTop() + (BordMon(zone).Height / 2) - (imgBattleOrigin.Height / 2));
            }
            else
            {
                if (zone == 6)
                    return new Point(OpponentAvatar.CLeft() + (OpponentAvatar.Width / 2) - (imgBattleOrigin.Width / 2),
                                     OpponentAvatar.CTop() + (OpponentAvatar.Height / 2) - (imgBattleOrigin.Height / 2));
                else
                    return new Point(BordOpMon(zone).CLeft() + (BordOpMon(zone).Width / 2) - (imgBattleOrigin.Width / 2),
                                     BordOpMon(zone).CTop() + (BordOpMon(zone).Height / 2) - (imgBattleOrigin.Height / 2));
            }
        }
        void detachAllMaterial(int monZone, bool includeAnimation)
        {
            if (M.PlayerOverlaid[monZone].CountNumCards() == 0) return;

            while (M.PlayerOverlaid[monZone].CountNumCards() > 0)
            {
                int xyzIndex = M.PlayerOverlaid[monZone].CountNumCards();
                CardDetails stats = MoveStats(Area.Xyz, Area.Grave, monZone, fromIndexXyz: xyzIndex);
                animationBundle bundle = createBundle(Area.Xyz, Area.Grave, true, monZone, fromIndexXyz: xyzIndex);
                if (includeAnimation)
                    Animate(bundle, stats, DuelEvents.Material_To_Grave);
                else
                    UpdatePictureBoxDuelField(BordGrave, stats, stats.OpponentOwned ? opponentSet : M.mySet, true);
                SummarizeJabber(Area1: Area.Grave, Index1: M.PlayerGrave.CountNumCards(),
                    bundle: bundle);
            }
            SummarizeJabber(Area.Xyz, monZone);
        }
        void viewForm_Closed(object sender, System.EventArgs e)
        {
            if (viewForm.oncloseFromArea != Area.None)
            {
                animationBundle bundle = new animationBundle();
                bundle.fromArea = viewForm.oncloseFromArea;
                bundle.toArea = viewForm.oncloseToArea;
                bundle.fromIndex = viewForm.oncloseFromIndex;
                bundle.toIndex = viewForm.oncloseToIndex;
                bundle.isPlayer = true;
                DuelEvents duelEvent = getDuelEventFromAreas(bundle.fromArea, bundle.toArea, bundle.toIndex == 1 ? true : false);
                DuelNumeric statUpdate1 = DuelNumeric.None, statUpdate2 = DuelNumeric.None;
                Area areaUpdate1 = Area.None, areaUpdate2 = Area.None;
                
                
                switch (bundle.fromArea)
                {
                    case Area.Deck:
                        statUpdate1 = DuelNumeric.NumDeck;
                        break;
                    case Area.Extra:
                        statUpdate1 = DuelNumeric.NumEDeck;
                        break;
                    case Area.Grave:
                        areaUpdate1 = Area.rmGrave;
                        break;
                    case Area.RFG:
                        areaUpdate1 = Area.rmRFG;
                        break;
                }

                switch (bundle.toArea)
                {
                    case Area.Deck:
                        statUpdate2 = DuelNumeric.NumDeck;
                        break;
                    case Area.Extra:
                        statUpdate2 = DuelNumeric.NumEDeck;
                        break;
                    case Area.Hand:
                        statUpdate2 = DuelNumeric.NumHand;
                        break;
                    case Area.Grave:
                    case Area.RFG:
                        areaUpdate2 = bundle.toArea;
                        break;
                    case Area.Monster:
                        areaUpdate2 = bundle.toArea;
                        bundle.toIndex = FindEmptyBordMon();
                        if (bundle.toIndex < 1) return;
                        break;
                    case Area.ST:
                        areaUpdate2 = bundle.toArea;
                        bundle.toIndex = FindEmptyBordST();
                        if (bundle.toIndex < 1) return;
                        break;
                    case Area.FieldSpell:
                        areaUpdate2 = bundle.toArea;
                        if (M.PlayerFSpell.Name != null)
                            DestroyFieldSpell();
                        break;
                }

                bool wasFacedown = getStatsFromArea(bundle.fromArea, bundle.fromIndex, true).Facedown;
                CardDetails stats = MoveStats(bundle.fromArea, bundle.toArea, bundle.fromIndex, bundle.toIndex);
                stats.Facedown = viewForm.oncloseBanishFacedown;
                if (bundle.toArea == Area.RFG) M.PlayerRFG[M.PlayerRFG.CountNumCards()].Facedown = stats.Facedown;
                Animate(bundle, stats, duelEvent);
                SummarizeJabber(Area1: areaUpdate1, Index1: bundle.fromIndex, Area2: areaUpdate2, Index2: bundle.toIndex,
                    Stat1: statUpdate1, Stat2: statUpdate2,
                    duelEvent: duelEvent, Text: wasFacedown ? null : stats.Name, bundle: bundle);
            }
        }


        int FindEmptyBordMon()
        {
            for (int n = 1; n <= 5; n++)
            {
                if (string.IsNullOrEmpty(M.PlayerMonsters[n].Name))
                    return n;
            }
            return 0;
        }
        int FindEmptyBordST()
        {
            for (int n = 1; n <= 5; n++)
            {
                if (string.IsNullOrEmpty(M.PlayerST[n].Name))
                    return n;
            }
            return 0;
        }

        void viewForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingViewForm)
                dragViewForm(e.GetPosition(LayoutRoot));
        }
        void dragViewForm(Point p)
        {
            Point toMove = new Point();
            toMove.X = p.X - viewFormDragStartPoint.X;
            toMove.Y = p.Y - viewFormDragStartPoint.Y;
            viewForm.DragChildWindow(toMove);
            viewFormDragStartPoint = p;
        }
        void viewForm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingViewForm = false;
        }
        void viewForm_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewFormDragStartPoint = e.GetPosition(LayoutRoot);
            isDraggingViewForm = true;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (M.sock != null)
                M.sock.SendMessage(M.socketSerialize("Server", M.username, M.myRoomID, MessageType.DuelLeave));
            base.OnNavigatedFrom(e);
        }
        private void rateForm_Closed(object sender, System.EventArgs e)
        {
            M.warnOnExitMessage = null;
            this.NavigationService.Navigate(new System.Uri("/Lobby", UriKind.Relative));
        }

        private static void MsgBox(string text) { MessageBox.Show(text); }
        #region "Minor Classes and Enums"
        public enum DuelEvents
        {
            None,
            Message,
            Offer_Rematch,
            Accept_Rematch,
            Refuse_Rematch,
            Test_Connection,
            Ping_Successful,
            Send_Field_To_Watcher,
            Opponent_ID,
            Opponent_Pool,
            Flip_Coin,
            Roll_Die,

            Draw_Phase,
            Standby_Phase,
            Main_Phase_1,
            Battle_Phase,
            Main_Phase_2,
            End_Phase,
            End_Turn,
            Extra_To_Grave,
            Extra_To_Grave_Random,
            Extra_To_RFG,
            Extra_To_Monster,
            Extra_To_ST,

            Turn_Count_Change,
            Lose_Lifepoints,
            Gain_Lifepoints,

            Hand_Shuffle,
            Hand_To_Grave,
            Hand_To_Grave_Random,
            Hand_To_Grave_All,
            Hand_To_RFG,
            Hand_Reveal,
            Hand_Reveal_All,
            Hand_To_Top,
            Hand_To_Bottom,
            Hand_To_Extra,
            Opponent_Draw_From_Player,

            Deck_Draw,
            Deck_View,
            Shuffle,
            Deck_To_Hand,
            Deck_To_Monster,
            Deck_To_ST,
            Deck_To_Grave,
            Deck_To_Grave_Mill,
            Deck_To_RFG,
            Deck_To_RFG_Mill,
            Deck_To_Top,
            Deck_To_Bottom,

            Bottom_Return,

            Top_To_Grave,
            Top_To_RFG,
            Set,
            Play,

            Grave_To_Extra,
            Grave_To_RFG,
            Grave_To_Hand,
            Grave_To_Top,
            Grave_To_Bottom,
            Grave_To_Monster,
            Grave_To_ST,
           
            RFG_To_Extra,
            RFG_To_Grave,
            RFG_To_Top,
            RFG_To_Bottom,
            RFG_To_Hand,
            RFG_To_Monster,
            RFG_To_ST,

            Field_To_Extra,

            Monster_Activate,
            Monster_To_Grave,
            Monster_To_RFG,
            Monster_Move,
            Monster_Move_To_ST,
            Monster_Move_To_FSpell,
            Monster_Flip_Facedown,
            Monster_Flip_Faceup,
            ATK_Change,
            DEF_Change,
            Attack,
            Attack_Direct,
            Attach_Material,
            Material_To_Grave,
            Material_To_RFG,
            Material_To_Monster,
            Material_To_ST,
            Material_To_FSpell,
            Material_To_Hand,
            Material_To_Deck,
            Material_To_Extra,
            Switch_Position,

            Bounce,
            Spin,

            Give_Control,
            Give_Back_Control,
            Confirm_Control,
            Confirm_Back_Control,
            Failed_Control_Switch,


            ST_Move,
            ST_Move_To_Monster,
            ST_Move_To_FSpell,
            ST_Flip_Facedown,
            ST_Flip_Faceup,
            ST_Activate,
            ST_To_Grave,
            ST_To_RFG,

            FSpell_Move_To_Monster,
            FSpell_Move_To_ST,
            FSpell_To_Grave,
            FSpell_To_RFG,

            Summon_Token,
            Destroy_Token,
            Add_Counter,
            Remove_Counter,
            Change_Counter,
            Target
        }
        const int REVEAL_ALL = 99;
        public class ZoneInvolvement
        {
            public Dictionary<int, bool> Monsters = new Dictionary<int,bool>();
            public Dictionary<int, bool> ST = new Dictionary<int,bool>();
            public bool FSpell;
            public bool Graveyard;
            public bool RFG;
            public bool Deck;
            public bool Extra;
            public int Hand;
            public bool WatcherHand; //Needed to send num cards in hand to watcher, and update with back.jpgs
            public Dictionary<int, bool> Xyz = new Dictionary<int,bool>();
            public Area fromArea { get; set; }
            public Area toArea { get; set; }
            public int fromIndex { get; set; }
            public int toIndex { get; set; }
            public int fromIndexXyz { get; set; }
            public int toIndexXyz { get; set; }

            public bool isPlayer { get; set; }
            public DuelFieldNew.DuelEvents dEvent;
            public string dText;
            public bool hasAnimation = false;
        }
        public enum Area
        { None, Monster, ST, FieldSpell, Grave, RFG, Extra, Hand, Xyz, Deck, rmGrave, rmRFG, Special }
        public enum StatElement
        { None, Name, Type, Attribute, Level, ATK, DEF, Lore, Counters, Horizontal, Facedown }
        public enum DuelNumeric
        { None, NumHand, NumDeck, NumEDeck, LP }
        public class animationBundle
        {
            public Area fromArea { get; set; }
            public Area toArea { get; set; }
            public int fromIndex { get; set; }
            public int toIndex { get; set; }
            public int fromIndexXyz { get; set; }
            public SpecialAnimation specialAnimation { get; set; }
            public bool isPlayer { get; set; }
            //public bool nullifyOrigin { get; set; }
        }
        #endregion
        private void HandleMessageFromDuelEvent(DuelEvents dEvent, string Text, bool isPlayer, string watcherReceiveFromPlayer = null)
        {
            string miscstring = null;
            bool dontReplaceTheir = false;
            bool dontAddPlayers = false;
            bool dontBold = false;
            switch (dEvent)
            {
                case DuelEvents.Offer_Rematch:
                    if (!isPlayer && watcherReceiveFromPlayer == null)
                    {
                        MessageBoxResult answer = MessageBox.Show("Your opponent has offered a rematch. Accept?", "Play Again", MessageBoxButton.OKCancel);
                        
                        if (answer == MessageBoxResult.OK)
                        {
                            SummarizeJabber(duelEvent: DuelEvents.Accept_Rematch);
                            
                            M.sideDecking = true;
                            M.warnOnExitMessage = null;
                            this.NavigationService.Navigate(new System.Uri("/DeckEditorNew", UriKind.Relative));
                        }
                        else
                        {
                            SummarizeJabber(duelEvent: DuelEvents.Refuse_Rematch);
                            RateUser RateForm = new RateUser();
                            RateForm.Show();
                        }
                    }
                    break;
                case DuelEvents.Accept_Rematch:
                    if (!isPlayer && watcherReceiveFromPlayer == null)
                    {
                        M.sideDecking = true;
                        M.warnOnExitMessage = null;
                        this.NavigationService.Navigate(new System.Uri("/DeckEditorNew", UriKind.Relative));
                    }
                    else
                    {
                        miscstring = "accepted a rematch offer. Wait here for the next round!";
                        watcherResetDuel();
                    }
                    break;
                case DuelEvents.Refuse_Rematch:
                    if (!isPlayer && watcherReceiveFromPlayer == null)
                    {
                        addMessage("Your opponent has refused a rematch.", true);
                        RateUser RateForm2 = new RateUser();
                        RateForm2.Show();
                    }
                    break;
                case DuelEvents.Test_Connection:
                    if (!isPlayer && watcherReceiveFromPlayer == null)
                    {
                        SummarizeJabber(duelEvent: DuelEvents.Ping_Successful);
                        SummarizeJabber(Stat1: DuelNumeric.NumEDeck, duelEvent: DuelEvents.Opponent_Pool, Text: M.mySet);
                        SummarizeJabber(duelEvent: DuelEvents.Opponent_ID, Text: M.myUsernameId.ToString());
                    }
                    

                    break;
                case DuelEvents.Ping_Successful:
                    if (!isPlayer || M.IamWatching)
                    {
                        if (watcherReceiveFromPlayer == null) //From opponent, i am not watcher
                        {
                            if (!playerIsConnected || allowReconnect)
                            {
                                PingTimer.Stop();
                                if (!allowReconnect)
                                {
                                    allowReconnect = true;
                                    /*List<string> multiMessage = new List<string>();
                                   
                                    multiMessage.Add(DrawCard(true));
                                    multiMessage.Add(DrawCard(true));
                                    multiMessage.Add(DrawCard(true));
                                    multiMessage.Add(DrawCard(true));
                                    multiMessage.Add(DrawCard(true));
                                    SendJabber(combineMultipleJabbers(multiMessage));*/
                                }
                                else
                                    allowReconnect = true;
                                miscstring = "Connection to Opponent OK";
                                dontAddPlayers = true;
                                playerIsConnected = true;
                               
                               
                            }
                        }
                        else
                        {
                            if (WatcherCurrentlyPinging == M.WatcherMySide) //Am watcher, First ping the player's side
                            {
                                miscstring = "Connection to " + WatcherCurrentlyPinging + " OK";
                                dontAddPlayers = true;
                                WatcherCurrentlyPinging = M.WatcherOtherSide;
                            }
                            else if (WatcherCurrentlyPinging == M.WatcherOtherSide) //Then ping the opponent's side
                            {
                                miscstring = "Connection to " + WatcherCurrentlyPinging + " OK";
                                dontAddPlayers = true;
                                WatcherCurrentlyPinging = null;
                                PingTimer.Stop();
                                PingTimer.Tick -= pingTimer_Tick;
                                M.sock.SendMessage(M.socketSerialize("Server", M.username, M.myRoomID, MessageType.DuelEnter));
                            }
                        }
                    }
                    break;
                case DuelEvents.Hand_Shuffle:
                    if (!isPlayer)
                    {
                        animationBundle bundle = new animationBundle();
                        bundle.specialAnimation = SpecialAnimation.ShuffleHand;
                        bundle.isPlayer = false;
                        Animate(bundle, null, DuelEvents.Hand_Shuffle);
                    }
                    break;
                case DuelEvents.Target:
                    if (!isPlayer)
                    {
                        showTargetBorder(true, Convert.ToInt32(Text));

                    }
                    break;
                case DuelEvents.Opponent_Pool:
                    if (!isPlayer)
                        opponentSet = Text;
                    else if (M.IamWatching && watcherReceiveFromPlayer == M.WatcherMySide)
                        watcherMySideSet = Text;
                    else if (M.IamWatching && watcherReceiveFromPlayer == M.WatcherOtherSide)
                        opponentSet = Text;
                    break;
                case DuelEvents.Opponent_ID:
                    if (!isPlayer)
                    {
                        SQLReference.Service1ConsoleClient client = new Service1ConsoleClient();
                        client.getAvatarImageCompleted += (s, e) =>
                            {
                                if (e.Error == null && !string.IsNullOrEmpty(e.Result))
                                {
                                    if (M.IamWatching)
                                    {
                                        if (watcherReceiveFromPlayer == M.WatcherMySide)
                                            M.setImage(PlayerAvatar, e.Result, UriKind.Absolute);
                                        else if (watcherReceiveFromPlayer == M.WatcherOtherSide)
                                            M.setImage(OpponentAvatar, e.Result, UriKind.Absolute);
                                    }
                                    else
                                        M.setImage(OpponentAvatar, e.Result, UriKind.Absolute);

                                }
                            };
                        client.getAvatarImageAsync(Convert.ToInt32(Text));
                    }
                    break;
                case DuelEvents.Flip_Coin:
                    miscstring = "flipped a Coin, the result was " + Text;
                    break;
                case DuelEvents.Roll_Die:
                    miscstring = "rolled a Die, the result was " + Text;
                    break;
                case DuelEvents.Deck_Draw:
                    miscstring = "drew a card.";
                    if (!isPlayer && !M.IamWatching && !playerIsConnected)
                    {
                        PingTimer.Stop();
                        if (!allowReconnect)
                        {
                            allowReconnect = true;
                            /*List<string> multiMessage = new List<string>();

                            multiMessage.Add(DrawCard(true));
                            multiMessage.Add(DrawCard(true));
                            multiMessage.Add(DrawCard(true));
                            multiMessage.Add(DrawCard(true));
                            multiMessage.Add(DrawCard(true));
                            SendJabber(combineMultipleJabbers(multiMessage));*/
                        }
                        else
                            allowReconnect = true;
                        miscstring = "Connection to Opponent OK";
                        dontAddPlayers = true;
                        playerIsConnected = true;
                    }
                    break;
                case DuelEvents.Monster_Activate:
                    miscstring = "activated the effect of " + "\"" + Text + "\"" + ".";
                    break;
                case DuelEvents.Message:
                    miscstring = ": " + Text;
                    dontReplaceTheir = true;
                    dontBold = true;
                    break;
                case DuelEvents.Lose_Lifepoints:
                    miscstring = "lost " + Text + " Life Points.";
                    break;
                case DuelEvents.Gain_Lifepoints:
                    miscstring = "gained " + Text + " Life Points.";
                    break;
                case DuelEvents.Hand_To_Grave:
                    miscstring = "discarded " + "\"" + Text + "\"" + " from their hand.";
                    break;
                case DuelEvents.Hand_To_Grave_Random:
                    miscstring = "discarded " + "\"" + Text + "\"" + " from their hand at random.";
                    break;
                case DuelEvents.Hand_To_Grave_All:
                    miscstring = "discarded all cards from their hand.";
                    break;
                case DuelEvents.Shuffle:
                    miscstring = "shuffled their deck.";
                    break;
                case DuelEvents.Top_To_Grave:
                    miscstring = "sent " + "\"" + Text + "\"" + " from the top of their deck to the Graveyard.";
                    break;
                case DuelEvents.Top_To_RFG:
                    miscstring = "banished " + "\"" + Text + "\"" + " from the top of their deck.";
                    break;


                case DuelEvents.Set:
                    miscstring = "set a card from their hand.";
                    break;
                case DuelEvents.Play:
                    miscstring = "played " + "\"" + Text + "\"" + " from their hand.";
                    break;
                case DuelEvents.Hand_To_Top:
                    miscstring = "returned a card in their hand to the top of their deck.";
                    break;
                case DuelEvents.Hand_To_Bottom:
                    miscstring = "returned a card in their hand to the bottom of their deck.";
                    break;
                case DuelEvents.Hand_To_Extra:
                    miscstring = "returned a card in their hand to their Extra Deck.";
                    break;
                case DuelEvents.Hand_To_RFG:
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "banished a card Facedown from their hand.";
                    else
                        miscstring = "banished " + "\"" + Text + "\"" + " from their hand.";
                    break;
                case DuelEvents.Deck_View:
                    if (isPlayer && watcherReceiveFromPlayer == null)
                        miscstring = "are viewing your deck.";
                    else
                        miscstring = "is viewing their deck.";
                    break;
                case DuelEvents.Hand_Reveal:
                    miscstring = "revealed a card in their hand : " + Text;
                    break;
                case DuelEvents.Hand_Reveal_All:
                    miscstring = "revealed their entire hand.";
                    break;
                case DuelEvents.Monster_Move_To_ST:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Monster") + " to a Spell/Trap zone."; 
                    break;
                case DuelEvents.ST_Move_To_Monster:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Spell/Trap") + " to a Monster zone."; 
                    break;
                case DuelEvents.Monster_Move:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Monster") + " to another Monster zone."; 
                    break;
                case DuelEvents.ST_Move:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Spell/Trap") + " to another Spell/Trap zone."; 
                    break;
                case DuelEvents.Monster_Move_To_FSpell:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Monster") + " to the Field Spell zone."; 
                    break;
                case DuelEvents.ST_Move_To_FSpell:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Spell/Trap") + " to the Field Spell zone."; 
                    break;
                case DuelEvents.FSpell_Move_To_Monster:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Field Spell") + " to a Monster zone."; 
                    break;

                case DuelEvents.FSpell_Move_To_ST:
                    miscstring = "moved " + (Text != null ? "\"" + Text + "\"" : "a Field Spell") + " to a Spell/Trap zone."; 
                    break;
                case DuelEvents.Monster_Flip_Facedown:
                    miscstring = "flipped their monster " + Text + " Face-Down."; //TODO
                    break;
                case DuelEvents.ST_Flip_Facedown:
                    miscstring = "flipped their Spell/Trap " + Text + " Face-Down."; //TODO
                    break;
                case DuelEvents.Monster_Flip_Faceup:
                    miscstring = "flipped their Monster " + Text + " Face-Up."; //TODO
                    break;
                case DuelEvents.ST_Flip_Faceup:
                    miscstring = "flipped their Spell/Trap " + Text + " Face-Up."; //TODO
                    break;
                case DuelEvents.ATK_Change:
                    miscstring = "changed ATK of their monster " + "\"" + Text + "\".";
                    break;
                case DuelEvents.DEF_Change:
                    miscstring = "changed DEF of their monster " + "\"" + Text + "\".";
                    break;
                case DuelEvents.ST_Activate:
                    miscstring = "activated their Spell/Trap card " + "\"" + Text + "\".";
                    break;
                case DuelEvents.Monster_To_RFG:
                    miscstring = "banished their monster " + "\"" + Text + "\".";
                    break;
                case DuelEvents.ST_To_RFG:
                    miscstring = "banished their Spell/Trap card " + "\"" + Text + "\".";
                    break;
                case DuelEvents.FSpell_To_RFG:
                    miscstring = "banished their Field Spell card " + "\"" + Text + "\".";
                    break;
                case DuelEvents.Bounce:
                    miscstring = "returned " + (Text != null ? "\"" + Text + "\"" : "a card") + " from the field to their hand.";
                    break;
                case DuelEvents.Spin:
                    miscstring = "returned " + (Text != null ? "\"" + Text + "\"" : "a card") + " from the field to the top of their deck.";
                    break;
                case DuelEvents.Bottom_Return:
                    miscstring = "returned " + (Text != null ? "\"" + Text + "\"" : "a card") + " from the field to the bottom of their deck.";
                    break;
                case DuelEvents.Switch_Position:
                    miscstring = "switched the Battle position of " + "\"" + Text + "\".";
                    break;
                case DuelEvents.Summon_Token:
                    miscstring = "summoned a token.";
                    break;
                case DuelEvents.Destroy_Token:
                    miscstring = "destroyed a token.";
                    break;
                case DuelEvents.Change_Counter:
                    miscstring = "changed the number of counters on their card " +
                        PlayerCurrentField(Convert.ToInt32(Text)).Name + " ( " + PlayerCurrentField(Convert.ToInt32(Text)).Counters.ToString() + " counters)";
                    break;
                case DuelEvents.Opponent_Draw_From_Player:
                    miscstring = "drew a card from their opponent's deck.";
                    break;
                case DuelEvents.Give_Control:
                case DuelEvents.Give_Back_Control:
                    if (!isPlayer && watcherReceiveFromPlayer == null)
                    {
                        int opponentZone = string.IsNullOrEmpty(Text) ? 0 : Convert.ToInt32(Text);
                        int newZone;
                        Area switchArea = getAreaFromZone(opponentZone);
                        switch (switchArea)
                        {
                            case Area.Monster:
                                newZone = FindEmptyBordMon();
                                if (newZone > 0 && M.OpponentMonsters[opponentZone].Name != null)
                                {
                                    M.copyCardDetails(ref M.PlayerMonsters[newZone], M.OpponentMonsters[opponentZone]);
                                    M.PlayerMonsters[newZone].OpponentOwned = dEvent == DuelEvents.Give_Control;
                                    UpdatePictureBoxDuelField(BordMon(newZone), M.PlayerMonsters[newZone], opponentSet);
                                    miscstring = "gave control of their monster \"" + M.PlayerMonsters[newZone].Name + "\" to you";
                                    SummarizeJabber(Area1: Area.Monster, Index1: newZone, duelEvent: DuelEvents.Confirm_Control, Text: newZone.ToString());
                                }
                                else
                                    SummarizeJabber(duelEvent: DuelEvents.Failed_Control_Switch);
                                break;
                            case Area.ST:
                                opponentZone -= 5; //It was global, now it is lined up
                                newZone = FindEmptyBordST();
                                if (newZone > 0 && M.OpponentST[opponentZone].Name != null)
                                {
                                    M.copyCardDetails(ref M.PlayerST[newZone], M.OpponentST[opponentZone]);
                                    M.PlayerST[newZone].OpponentOwned = dEvent == DuelEvents.Give_Control;
                                    UpdatePictureBoxDuelField(BordST(newZone), M.PlayerST[newZone], opponentSet);
                                    miscstring = "gave control of their Spell/Trap \"" + M.PlayerST[newZone].Name + "\" to you";
                                    SummarizeJabber(Area1: Area.ST, Index1: newZone, duelEvent: DuelEvents.Confirm_Control, Text: (newZone + 5).ToString());
                                }
                                else
                                    SummarizeJabber(duelEvent: DuelEvents.Failed_Control_Switch);
                                break;
                            case Area.FieldSpell:
                                if (M.PlayerFSpell.Name == null && M.OpponentFSpell.Name != null)
                                {
                                    M.copyCardDetails(ref M.PlayerFSpell, M.OpponentFSpell);
                                    M.PlayerFSpell.OpponentOwned = dEvent == DuelEvents.Give_Control;
                                    UpdatePictureBoxDuelField(BordFSpell, M.PlayerFSpell, opponentSet);
                                    miscstring = "gave control of their Field Spell \"" + M.PlayerFSpell.Name + "\" to you";
                                    SummarizeJabber(Area1: Area.FieldSpell, duelEvent: DuelEvents.Confirm_Control, Text: "11");
                                }
                                else
                                    SummarizeJabber(duelEvent: DuelEvents.Failed_Control_Switch);
                                break;

                        }
                    }
                   
                    break;
              
                case DuelEvents.Confirm_Control:
                case DuelEvents.Confirm_Back_Control:
                    if (!isPlayer && watcherReceiveFromPlayer == null)
                    {
                        resetStatDisplay();
                        int opponentZone = string.IsNullOrEmpty(Text) ? 0 : Convert.ToInt32(Text);
                        Area switchArea = getAreaFromZone(opponentZone);
                        dontAddPlayers = true;
                        switch (switchArea)
                        {
                            case Area.Monster:
                                miscstring = "You gave control of your monster \"" + M.OpponentMonsters[opponentZone].Name + "\" to " + M.opponent + ".";
                                //dontAddPlayers
                                M.OpponentMonsters[opponentZone].OpponentOwned = dEvent == DuelEvents.Confirm_Control;
                                M.setAsNothing(M.PlayerMonsters[ZoneofSwitch]);
                                UpdatePictureBoxDuelField(BordMon(ZoneofSwitch), null, null);
                                SummarizeJabber(Area1: Area.Monster, Index1: ZoneofSwitch);
                                break;
                            case Area.ST:
                                opponentZone -= 5; //It was global, now it is lined up
                                ZoneofSwitch -= 5;
                                miscstring = "You gave control of your Spell/Trap \"" + M.OpponentST[opponentZone].Name + "\" to " + M.opponent + ".";
                                //dontAddPlayers
                                M.OpponentST[opponentZone].OpponentOwned = dEvent == DuelEvents.Confirm_Control;
                                M.setAsNothing(M.PlayerST[ZoneofSwitch]);
                                UpdatePictureBoxDuelField(BordST(ZoneofSwitch), null, null);
                                SummarizeJabber(Area1: Area.ST, Index1: ZoneofSwitch);
                                break;
                            case Area.FieldSpell:
                                miscstring = "You gave control of your Field Spell \"" + M.OpponentFSpell.Name + "\" to " + M.opponent + ".";
                                //dontAddPlayers
                                M.OpponentFSpell.OpponentOwned = dEvent == DuelEvents.Confirm_Control;
                                M.setAsNothing(M.PlayerFSpell);
                                UpdatePictureBoxDuelField(BordFSpell, null, null);
                                SummarizeJabber(Area1: Area.FieldSpell);
                                break;
                        }
                        ZoneofSwitch = -1;
                    }
                    break;
               
                case DuelEvents.Failed_Control_Switch:
                    if (!isPlayer) 
                        ZoneofSwitch = -1;
                    break;

                case DuelEvents.Field_To_Extra:
                    miscstring = "returned " + "\"" + Text + "\"" + " from the Field to their Extra Deck.";
                    break;
                case DuelEvents.Grave_To_Extra:
                    miscstring = "returned " + "\"" + Text + "\"" + " from their Graveyard to their Extra Deck.";
                    break;
                case DuelEvents.RFG_To_Extra:
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "returned a facedown card from their RFG to their Extra Deck.";
                    else
                        miscstring = "returned " + "\"" + Text + "\"" + " from their RFG to their Extra Deck.";
                    break;
                case DuelEvents.Attack:
                    if (isPlayer || ( M.IamWatching && watcherReceiveFromPlayer == M.WatcherMySide)) //Either is player or watcher is on player's side (receiving from that side)
                    {  //Attacker|Defender
                        int bar = Text.IndexOf("|");
                        int myMon = Convert.ToInt32(Text.Substring(0, Text.Length - 2));
                        int opMon = Convert.ToInt32(Text.Substring(bar + 1, 1));
                        if (M.OpponentMonsters[opMon].Facedown == false)
                        {
                            miscstring = "are attacking " + M.OpponentMonsters[opMon].Name +
                           " (zone " + opMon + ") with " + M.PlayerMonsters[myMon].Name +
                           " (zone " + myMon + ")";
                        }
                        else
                        {
                            miscstring = "are attacking a Face-Down monster (zone " + opMon + ") with " +
                            M.PlayerMonsters[myMon].Name +
                            " (zone " + myMon + ")";
                        }
                        if (M.IamWatching)
                        {
                            positionSword(myMon);
                            rotateSwordToPoint(getCenterOfAttackTarget(false, opMon));
                            AttackAnimation(getCenterOfAttackTarget(true, myMon),
                                            getCenterOfAttackTarget(false, opMon));
                        }
                    }
                    else
                    { //Attacker|Defender
                        int bar = Text.IndexOf("|");
                        int opMon = Convert.ToInt32(Text.Substring(0, Text.Length - 2));
                        int myMon = Convert.ToInt32(Text.Substring(bar + 1, 1));
                        if (M.PlayerMonsters[myMon].Facedown == false)
                        {
                            miscstring = "is attacking " + M.PlayerMonsters[myMon].Name +
                           " (zone " + myMon + ") with " + M.OpponentMonsters[opMon].Name +
                           " (zone " + opMon + ")";
                        }
                        else
                        {
                            miscstring = "is attacking a Face-Down monster (zone " + myMon + ") with " +
                            M.OpponentMonsters[opMon].Name +
                            " (zone " + opMon + ")";
                        }
                        attack(opMon, myMon, false, default(Point));
                        //AttackAnimation(new Point(BordOpMon(opMon).CLeft() + (BordOpMon(opMon).Width / 2), BordOpMon(opMon).CTop() + (BordOpMon(opMon).Height / 2)),
                         //               new Point(BordMon(myMon).CLeft()   + (BordMon(myMon).Width / 2), BordMon(myMon).CTop() + (BordMon(myMon).Width / 2) ));
                    }
                    break;
                case DuelEvents.Attack_Direct:
                    int zone = Convert.ToInt32(Text);
                    if (isPlayer || (M.IamWatching && watcherReceiveFromPlayer == M.WatcherMySide))
                    {
                        miscstring = "are attacking directly with " + "\"" + M.PlayerMonsters[zone].Name + "\".";
                        if (M.IamWatching)
                        {
                            positionSword(zone);
                            rotateSwordToPoint(getCenterOfAttackTarget(false, 6));
                            AttackAnimation(getCenterOfAttackTarget(true, zone),
                                            getCenterOfAttackTarget(false, 6));
                        }
                    }
                    else
                    {
                        miscstring = "is attacking directly with " + "\"" + M.OpponentMonsters[zone].Name + "\".";
                        attack(zone, 6, false, default(Point));
                        //AttackAnimation(new Point(BordOpMon(zone).CLeft() + (BordOpMon(zone).Width / 2), BordOpMon(zone).CTop() + (BordOpMon(zone).Height / 2)),
                       //                 new Point(PlayerAvatar.CLeft() + (PlayerAvatar.Width / 2), PlayerAvatar.CTop() + (PlayerAvatar.Height / 2)));
                    }
                    break;

                case DuelEvents.Monster_To_Grave:
                    miscstring = "destroyed their Monster, " + "\"" + Text + "\".";
                    break;
                case DuelEvents.ST_To_Grave:
                    miscstring = "destroyed their Spell/Trap Card, " + "\"" + Text + "\".";
                    break;
                case DuelEvents.FSpell_To_Grave:
                    miscstring = "destroyed their Field Spell Card, " + "\"" + Text + "\".";

                    break;
                case DuelEvents.Attach_Material:
                    miscstring = "attached \"" + Text + "\" as Xyz material.";
                    break;
                case DuelEvents.Material_To_Grave:
                    miscstring = "detached \"" + Text + "\" as Xyz material.";
                    break;
                case DuelEvents.Material_To_Monster:
                    miscstring = "Special Summoned their Xyz material \"" + Text + "\".";
                    break;
                case DuelEvents.Material_To_ST:
                    miscstring = "Moved their Xyz material \"" + Text + "\" to a Spell/Trap zone.";
                    break;
                case DuelEvents.Material_To_FSpell:
                    miscstring = "Moved their Xyz material \"" + Text + "\" to the Field Spell zone.";
                    break;
                case DuelEvents.Material_To_Hand:
                    miscstring = "Returned their Xyz material \"" + Text + "\" to their hand.";
                    break;
                case DuelEvents.Material_To_RFG:
                    miscstring = "Banished their Xyz material \"" + Text + "\".";
                    break;
                case DuelEvents.Material_To_Deck:
                    miscstring = "Returned their Xyz material \"" + Text + "\" to their deck.";
                    break;
                case DuelEvents.Material_To_Extra:
                    miscstring = "Returned their Xyz material \"" + Text + "\" to their extra deck.";
                    break;
                case DuelEvents.Grave_To_RFG:
                    miscstring = "banished " + "\"" + Text + "\"" + " from their Graveyard.";
                    break;
                case DuelEvents.Deck_To_RFG:
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "banished a card Face-Down from their Deck.";
                    else
                        miscstring = "banished " + "\"" + Text + "\"" + " from their Deck.";
                    break;
                case DuelEvents.Deck_To_RFG_Mill:
                    miscstring = "banished " + "\"" + Text + "\"" + " from the top of their Deck.";
                    break;
                case DuelEvents.Extra_To_RFG:
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "banished a card Face-Down from their Extra Deck";
                    else
                        miscstring = "banished " + "\"" + Text + "\"" + " from their Extra Deck.";
                    break;
                case DuelEvents.RFG_To_Grave:
                    miscstring = "sent " + "\"" + Text + "\"" + " to their Graveyard from their RFG.";
                    break;
                case DuelEvents.Deck_To_Grave:
                    miscstring = "sent " + "\"" + Text + "\"" + " to their Graveyard from their Deck.";
                    break;
                case DuelEvents.Deck_To_Grave_Mill:
                    miscstring = "sent " + "\"" + Text + "\"" + " from the top of their Deck to the Graveyard.";
                    break; 
                case DuelEvents.Extra_To_Grave:
                    miscstring = "sent " + "\"" + Text + "\"" + " to their Graveyard from their Extra Deck.";
                    break;
                case DuelEvents.Deck_To_Top:
                    miscstring = "returned " + "\"" + Text + "\"" + " from their Deck to the top of their Deck.";
                    break;
                case DuelEvents.RFG_To_Top:
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "returned a facedown card from their RFG to the top of their Deck.";
                    else
                        miscstring = "returned " + "\"" + Text + "\"" + " from their RFG to the top of their Deck.";
                    break;
                case DuelEvents.Grave_To_Top:
                    miscstring = "returned " + "\"" + Text + "\"" + " from their Graveyard to the top of their Deck.";
                    break;
                case DuelEvents.Deck_To_Bottom:
                    miscstring = "returned " + "\"" + Text + "\"" + " from their Deck to the Bottom of their Deck.";
                    break;
                case DuelEvents.RFG_To_Bottom:
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "returned a facedown card from their RFG to the Bottom of their Deck.";
                    else
                        miscstring = "returned " + "\"" + Text + "\"" + " from their RFG to the Bottom of their Deck.";
                    break;
                case DuelEvents.Grave_To_Bottom:
                    miscstring = "returned " + "\"" + Text + "\"" + " from their Graveyard to the Bottom of their Deck.";
                    break;
                case DuelEvents.Deck_To_Hand:
                    //AddFromDeckToHand(M.NumberOnList); TODO
                    miscstring = "added " + "\"" + Text + "\"" + " from their Deck to their Hand.";
                    break;
                case DuelEvents.Grave_To_Hand:
                    //miscstring = AddFromGraveToHand(M.NumberOnList); TODO
                    miscstring = "added \"" + Text + "\" from their Graveyard to their Hand.";
                    break;
                case DuelEvents.RFG_To_Hand: //TODO
                    if (string.IsNullOrEmpty(Text))
                        miscstring = "added a Face-Down card from their RFG to their Hand.";
                    else
                        miscstring = "added \"" + Text + "\" from their RFG to their Hand.";
                    break;
                case DuelEvents.RFG_To_Monster: //TODO
                    miscstring = "special Summoned " + "\"" + Text + "\"" + " from their RFG.";
                    break;
                case DuelEvents.RFG_To_ST: //TODO
                    miscstring = "activated \"" + Text + "\" from their RFG.";
                    break;

                case DuelEvents.Grave_To_Monster: //TODO
                    miscstring = "special Summoned " + "\"" + Text + "\"" + " from their Grave.";
                    break;
                case DuelEvents.Grave_To_ST: //TODO
                    miscstring = "activated \"" + Text + "\" from their Grave.";
                    break;

                case DuelEvents.Deck_To_Monster: //TODO
                    miscstring = "special Summoned " + "\"" + Text + "\"" + " from their Deck.";
                    break;
                case DuelEvents.Deck_To_ST: //TODO
                    miscstring = "activated \"" + Text + "\" from their Deck.";
                    break;

                case DuelEvents.Extra_To_Monster: //TODO
                    miscstring = "special Summoned " + "\"" + Text + "\"" + " from their Extra Deck.";
                    break;
                case DuelEvents.Extra_To_ST: //TODO
                    miscstring = "activated \"" + Text + "\" from their Extra Deck.";
                    break;
                ///''''''''''''''''''''''''''''''''''''''
                case DuelEvents.Draw_Phase:
                    miscstring = "entered their Draw Phase";
                    if (!isPlayer) ChangePhase(Phase.Draw);
                    break;
                case DuelEvents.Standby_Phase:
                    miscstring = "entered their Standby Phase";
                    if (!isPlayer) ChangePhase(Phase.Standby);
                    break;
                case DuelEvents.Main_Phase_1:
                    miscstring = "entered their Main Phase 1";
                    if (!isPlayer) ChangePhase(Phase.Main1);
                    break;
                case DuelEvents.Battle_Phase:
                    miscstring = "entered their Battle Phase";
                    if (!isPlayer) ChangePhase(Phase.Battle);
                    break;
                case DuelEvents.Main_Phase_2:
                    miscstring = "entered their Main Phase 2";
                    if (!isPlayer) ChangePhase(Phase.Main2);
                    break;
                case DuelEvents.End_Phase:
                    miscstring = "entered their End Phase";
                    if (!isPlayer) ChangePhase(Phase.End);
                    break;
                case DuelEvents.End_Turn:
                    miscstring = "ended their turn. It is now the other player's turn.";
                    if (!isPlayer) ChangePhase(Phase.EndTurn);
                    DontFireTurnCount = true;
                    TurnCount++;
                    txtTurnCount.Text = TurnCount.ToString();
                    break;
                case DuelEvents.Turn_Count_Change:
                   // if (!isPlayer || M.IamWatching)
                   // {
                        DontFireTurnCount = true;
                        TurnCount = Convert.ToInt32(Text);
                        txtTurnCount.Text = Text;
                        miscstring = "The turn count was changed to " + Text + ".";
                   // }
                    break;

                case DuelEvents.None:
                case DuelEvents.Send_Field_To_Watcher:
                    break;
                default:
                    throw new Exception("Oops! Forgot " + dEvent.ToString());
            }

            string message;

            if (miscstring == null)
                message = null;
            else if (M.IamWatching) //Watcher
                message = (dontAddPlayers ? "" : (watcherReceiveFromPlayer ?? "You"  )) + " " + miscstring;
            else if (isPlayer & !dontReplaceTheir)
                message = (dontAddPlayers ? "" : "You ") + miscstring.Replace("their", "your");
            else if (isPlayer & watcherReceiveFromPlayer == null)
                message = (dontAddPlayers ? "" : M.username) + " " + miscstring;
            else if (watcherReceiveFromPlayer == null)
                message = (dontAddPlayers ? "" : M.opponent) + " " + miscstring;
            else //Is Watcher
                message = (dontAddPlayers ? "" : watcherReceiveFromPlayer) + " " + miscstring;

            if (message != null)
                addMessage(message, !dontBold);

        }

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
        public string SummarizeJabber(Area Area1 = Area.None, int Index1 = 0, StatElement Element1 = StatElement.None,
                                       Area Area2 = Area.None, int Index2 = 0, StatElement Element2 = StatElement.None,
                                       DuelNumeric Stat1 = DuelNumeric.None, DuelNumeric Stat2 = DuelNumeric.None, DuelEvents duelEvent = DuelEvents.None, string Text = null,
                                       bool TagSendToSingle = false, bool returnOnly = false, animationBundle bundle = null, CardDetails SpecialStat1 = null, CardDetails SpecialStat2 = null)
        {


            System.Text.StringBuilder strBld = new System.Text.StringBuilder();
            int n = 0;

            //Areas: Hand, Monster, ST, Grave, Deck, EDeck, RFG, FSpell, Over
            #region Area1
            //Wants to summarize entire area
            if (Area1 != Area.None && Index1 <= 0)
            {
                strBld.Append("{" + Area1 + "}");
                switch (Area1)
                {
                    case Area.Hand:
                        for (n = 1; n <= M.PlayerHand.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerHand[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerHand[n].ID) + "|");
                            //strBld.Append(M.PlayerHand[n].SpecialSet + "|");
                            strBld.Append(M.PlayerHand[n].Type + "|");
                            strBld.Append(M.PlayerHand[n].Attribute + "|");
                            strBld.Append(M.PlayerHand[n].Level + "|");
                            //strBld.Append(M.PlayerHand[n].STicon + "|");
                            strBld.Append(M.PlayerHand[n].ATK + "|");
                            strBld.Append(M.PlayerHand[n].DEF + "|");
                            strBld.Append(M.PlayerHand[n].Lore + "|");
                            strBld.Append(M.PlayerHand[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerHand[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerHand[n].Facedown) + "|");
                        }

                        break;
                    case Area.Monster:
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(M.PlayerMonsters[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerMonsters[n].ID) + "|");
                            //strBld.Append(M.PlayerMonsters[n].SpecialSet + "|");
                            strBld.Append(M.PlayerMonsters[n].Type + "|");
                            strBld.Append(M.PlayerMonsters[n].Attribute + "|");
                            strBld.Append(M.PlayerMonsters[n].Level + "|");
                            // strBld.Append(M.PlayerMonsters[n].STicon + "|");
                            strBld.Append(M.PlayerMonsters[n].ATK + "|");
                            strBld.Append(M.PlayerMonsters[n].DEF + "|");
                            strBld.Append(M.PlayerMonsters[n].Lore + "|");
                            strBld.Append(M.PlayerMonsters[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerMonsters[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerMonsters[n].Facedown) + "|");
                        }

                        break;
                    case Area.FieldSpell:
                        strBld.Append(M.PlayerFSpell.Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerFSpell.ID) + "|");
                        //strBld.Append(M.PlayerFSpell.SpecialSet + "|");
                        strBld.Append(M.PlayerFSpell.Type + "|");
                        strBld.Append(M.PlayerFSpell.Attribute + "|");
                        strBld.Append(M.PlayerFSpell.Level + "|");
                        // strBld.Append(M.PlayerFSpell.STicon + "|");
                        strBld.Append(M.PlayerFSpell.ATK + "|");
                        strBld.Append(M.PlayerFSpell.DEF + "|");
                        strBld.Append(M.PlayerFSpell.Lore + "|");
                        strBld.Append(M.PlayerFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.Facedown) + "|");
                        break;
                    case Area.Xyz:
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(returnXyzSummarize(n));
                            //for (short m = 1; m <= 5; m++)
                            //{
                            //    strBld.Append(M.PlayerOverlaid[n, m] + "|");
                            //}
                        }

                        break;


                    case Area.ST:
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(M.PlayerST[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerST[n].ID) + "|");
                            // strBld.Append(M.PlayerST[n].SpecialSet + "|");
                            strBld.Append(M.PlayerST[n].Type + "|");
                            strBld.Append(M.PlayerST[n].Attribute + "|");
                            strBld.Append(M.PlayerST[n].Level + "|");
                            //strBld.Append(M.PlayerST[n].STicon + "|");
                            strBld.Append(M.PlayerST[n].ATK + "|");
                            strBld.Append(M.PlayerST[n].DEF + "|");
                            strBld.Append(M.PlayerST[n].Lore + "|");
                            strBld.Append(M.PlayerST[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerST[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerST[n].Facedown) + "|");
                        }

                        break;
                    case Area.Deck:
                        for (n = 1; n <= M.PlayerDeck.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerDeck[n].ID) + "|");
                            //strBld.Append(M.PlayerDeck[n].SpecialSet + "|");
                            strBld.Append(M.PlayerDeck[n].Type + "|");
                            strBld.Append(M.PlayerDeck[n].Attribute + "|");
                            strBld.Append(M.PlayerDeck[n].Level + "|");
                            //strBld.Append(M.PlayerDeck[n].STicon + "|");
                            strBld.Append(M.PlayerDeck[n].ATK + "|");
                            strBld.Append(M.PlayerDeck[n].DEF + "|");
                            strBld.Append(M.PlayerDeck[n].Lore + "|");
                            strBld.Append(M.PlayerDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerDeck[n].Facedown) + "|");
                        }

                        break;
                    case Area.Grave:
                        for (n = 1; n <= M.PlayerGrave.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerGrave[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerGrave[n].ID) + "|");
                            //strBld.Append(M.PlayerGrave[n].SpecialSet + "|");
                            strBld.Append(M.PlayerGrave[n].Type + "|");
                            strBld.Append(M.PlayerGrave[n].Attribute + "|");
                            strBld.Append(M.PlayerGrave[n].Level + "|");
                            // strBld.Append(M.PlayerGrave[n].STicon + "|");
                            strBld.Append(M.PlayerGrave[n].ATK + "|");
                            strBld.Append(M.PlayerGrave[n].DEF + "|");
                            strBld.Append(M.PlayerGrave[n].Lore + "|");
                            strBld.Append(M.PlayerGrave[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerGrave[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerGrave[n].Facedown) + "|");
                        }

                        break;
                    case Area.RFG:
                        for (n = 1; n <= M.PlayerRFG.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerRFG[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerRFG[n].ID) + "|");
                            // strBld.Append(M.PlayerRFG[n].SpecialSet + "|");
                            strBld.Append(M.PlayerRFG[n].Type + "|");
                            strBld.Append(M.PlayerRFG[n].Attribute + "|");
                            strBld.Append(M.PlayerRFG[n].Level + "|");
                            // strBld.Append(M.PlayerRFG[n].STicon + "|");
                            strBld.Append(M.PlayerRFG[n].ATK + "|");
                            strBld.Append(M.PlayerRFG[n].DEF + "|");
                            strBld.Append(M.PlayerRFG[n].Lore + "|");
                            strBld.Append(M.PlayerRFG[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerRFG[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerRFG[n].Facedown) + "|");
                        }

                        break;
                    case Area.Extra:
                        for (n = 1; n <= M.PlayerEDeck.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerEDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerEDeck[n].ID) + "|");
                            //strBld.Append(M.PlayerEDeck[n].SpecialSet + "|");
                            strBld.Append(M.PlayerEDeck[n].Type + "|");
                            strBld.Append(M.PlayerEDeck[n].Attribute + "|");
                            strBld.Append(M.PlayerEDeck[n].Level + "|");
                            //strBld.Append(M.PlayerEDeck[n].STicon + "|");
                            strBld.Append(M.PlayerEDeck[n].ATK + "|");
                            strBld.Append(M.PlayerEDeck[n].DEF + "|");
                            strBld.Append(M.PlayerEDeck[n].Lore + "|");
                            strBld.Append(M.PlayerEDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerEDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerEDeck[n].Facedown) + "|");
                        }
                        break;
                    case Area.Special:
                        if (SpecialStat1 == null) throw new Exception("SpecialStat1 is null!");
                            strBld.Append(SpecialStat1.Name + "|");
                            strBld.Append(Convert.ToString(SpecialStat1.ID) + "|");
                            //strBld.Append(M.PlayerEDeck[n].SpecialSet + "|");
                            strBld.Append(SpecialStat1.Type + "|");
                            strBld.Append(SpecialStat1.Attribute + "|");
                            strBld.Append(SpecialStat1.Level + "|");
                            //strBld.Append(M.PlayerEDeck[n].STicon + "|");
                            strBld.Append(SpecialStat1.ATK + "|");
                            strBld.Append(SpecialStat1.DEF + "|");
                            strBld.Append(SpecialStat1.Lore + "|");
                            strBld.Append(SpecialStat1.Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(SpecialStat1.IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(SpecialStat1.Facedown) + "|");
                        break;
                }
            }

            //Wants to summarize a specific card
            if (Index1 > 0 && Area1 != Area.None && Element1 == StatElement.None)
            {
                strBld.Append("{" + Area1 + "|" + Index1.ToString() + "}");
                n = Index1;
                switch (Area1)
                {
                    case Area.Hand:
                        strBld.Append(M.PlayerHand[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerHand[n].ID) + "|");
                        strBld.Append(M.PlayerHand[n].Type + "|");
                        strBld.Append(M.PlayerHand[n].Attribute + "|");
                        strBld.Append(M.PlayerHand[n].Level + "|");
                        strBld.Append(M.PlayerHand[n].ATK + "|");
                        strBld.Append(M.PlayerHand[n].DEF + "|");
                        strBld.Append(M.PlayerHand[n].Lore + "|");
                        strBld.Append(M.PlayerHand[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerHand[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerHand[n].Facedown) + "|");

                        break;
                    case Area.Monster:

                        strBld.Append(M.PlayerMonsters[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerMonsters[n].ID) + "|");
                        strBld.Append(M.PlayerMonsters[n].Type + "|");
                        strBld.Append(M.PlayerMonsters[n].Attribute + "|");
                        strBld.Append(M.PlayerMonsters[n].Level + "|");
                        strBld.Append(M.PlayerMonsters[n].ATK + "|");
                        strBld.Append(M.PlayerMonsters[n].DEF + "|");
                        strBld.Append(M.PlayerMonsters[n].Lore + "|");
                        strBld.Append(M.PlayerMonsters[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerMonsters[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerMonsters[n].Facedown) + "|");

                        break;
                    case Area.Xyz:
                        strBld.Append(returnXyzSummarize(n));
                        //for (short m = 1; m <= 5; m++)
                        //{
                        //    strBld.Append( M.PlayerOverlaid[n, m] + "|");
                        //}


                        break;


                    case Area.FieldSpell:
                        strBld.Append(M.PlayerFSpell.Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerFSpell.ID) + "|");
                        // strBld.Append(M.PlayerFSpell.SpecialSet + "|");
                        strBld.Append(M.PlayerFSpell.Type + "|");
                        strBld.Append(M.PlayerFSpell.Attribute + "|");
                        strBld.Append(M.PlayerFSpell.Level + "|");
                        //  strBld.Append(M.PlayerFSpell.STicon + "|");
                        strBld.Append(M.PlayerFSpell.ATK + "|");
                        strBld.Append(M.PlayerFSpell.DEF + "|");
                        strBld.Append(M.PlayerFSpell.Lore + "|");
                        strBld.Append(M.PlayerFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.Facedown) + "|");

                        break;
                    case Area.ST:

                        strBld.Append(M.PlayerST[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerST[n].ID) + "|");
                        //strBld.Append(M.PlayerST[n].SpecialSet + "|");
                        strBld.Append(M.PlayerST[n].Type + "|");
                        strBld.Append(M.PlayerST[n].Attribute + "|");
                        strBld.Append(M.PlayerST[n].Level + "|");
                        //strBld.Append(M.PlayerST[n].STicon + "|");
                        strBld.Append(M.PlayerST[n].ATK + "|");
                        strBld.Append(M.PlayerST[n].DEF + "|");
                        strBld.Append(M.PlayerST[n].Lore + "|");
                        strBld.Append(M.PlayerST[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerST[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerST[n].Facedown) + "|");

                        break;
                    case Area.Deck:

                        strBld.Append(M.PlayerDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerDeck[n].ID) + "|");
                        // strBld.Append(M.PlayerDeck[n].SpecialSet + "|");
                        strBld.Append(M.PlayerDeck[n].Type + "|");
                        strBld.Append(M.PlayerDeck[n].Attribute + "|");
                        strBld.Append(M.PlayerDeck[n].Level + "|");
                        // strBld.Append(M.PlayerDeck[n].STicon + "|");
                        strBld.Append(M.PlayerDeck[n].ATK + "|");
                        strBld.Append(M.PlayerDeck[n].DEF + "|");
                        strBld.Append(M.PlayerDeck[n].Lore + "|");
                        strBld.Append(M.PlayerDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerDeck[n].Facedown) + "|");

                        break;
                    case Area.Grave:

                        strBld.Append(M.PlayerGrave[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerGrave[n].ID) + "|");
                        // strBld.Append(M.PlayerGrave[n].SpecialSet + "|");
                        strBld.Append(M.PlayerGrave[n].Type + "|");
                        strBld.Append(M.PlayerGrave[n].Attribute + "|");
                        strBld.Append(M.PlayerGrave[n].Level + "|");
                        //strBld.Append(M.PlayerGrave[n].STicon + "|");
                        strBld.Append(M.PlayerGrave[n].ATK + "|");
                        strBld.Append(M.PlayerGrave[n].DEF + "|");
                        strBld.Append(M.PlayerGrave[n].Lore + "|");
                        strBld.Append(M.PlayerGrave[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerGrave[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerGrave[n].Facedown) + "|");

                        break;
                    case Area.RFG:

                        strBld.Append(M.PlayerRFG[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerRFG[n].ID) + "|");
                        // strBld.Append(M.PlayerRFG[n].SpecialSet + "|");
                        strBld.Append(M.PlayerRFG[n].Type + "|");
                        strBld.Append(M.PlayerRFG[n].Attribute + "|");
                        strBld.Append(M.PlayerRFG[n].Level + "|");
                        //strBld.Append(M.PlayerRFG[n].STicon + "|");
                        strBld.Append(M.PlayerRFG[n].ATK + "|");
                        strBld.Append(M.PlayerRFG[n].DEF + "|");
                        strBld.Append(M.PlayerRFG[n].Lore + "|");
                        strBld.Append(M.PlayerRFG[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerRFG[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerRFG[n].Facedown) + "|");

                        break;
                    //case "rmGrave": break; TODO?
                    //case "rmRFG": break; TODO?
                    case Area.Extra:

                        strBld.Append(M.PlayerEDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerEDeck[n].ID) + "|");
                        // strBld.Append(M.PlayerEDeck[n].SpecialSet + "|");
                        strBld.Append(M.PlayerEDeck[n].Type + "|");
                        strBld.Append(M.PlayerEDeck[n].Attribute + "|");
                        strBld.Append(M.PlayerEDeck[n].Level + "|");
                        //strBld.Append(M.PlayerEDeck[n].STicon + "|");
                        strBld.Append(M.PlayerEDeck[n].ATK + "|");
                        strBld.Append(M.PlayerEDeck[n].DEF + "|");
                        strBld.Append(M.PlayerEDeck[n].Lore + "|");
                        strBld.Append(M.PlayerEDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerEDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerEDeck[n].Facedown) + "|");

                        break;
                }

            }

            //Wants to summarize an element of a specific card
            if (Index1 > 0 && Area1 != Area.None && Element1 != StatElement.None)
            {
                strBld.Append("{" + Area1 + "|" + Index1.ToString() + "|" + Element1 + "}");
                n = Index1;
                switch (Area1)
                {
                    case Area.Hand:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerHand[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerHand[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerHand[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerHand[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerHand[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Monster:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerMonsters[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerMonsters[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerMonsters[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerMonsters[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerMonsters[n].Facedown) + "|");
                                break;
                        }

                        break;
                    case Area.ST:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerST[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerST[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerST[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerST[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerST[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.FieldSpell:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerFSpell.ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerFSpell.DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerFSpell.Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerFSpell.IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerFSpell.Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Grave:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerGrave[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerGrave[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerGrave[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerGrave[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerGrave[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.RFG:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerRFG[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerRFG[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerRFG[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerRFG[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerRFG[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Deck:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerDeck[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerDeck[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerDeck[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerDeck[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Extra:
                        switch (Element1)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerEDeck[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerEDeck[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerEDeck[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerEDeck[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerEDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                }
            }

            #endregion
            #region Area2
            ///'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //Wants to summarize entire area
            if (Area2 != Area.None && Index2 <= 0)
            {
                strBld.Append("{" + Area2 + "}");
                switch (Area2)
                {
                    case Area.Hand:
                        for (n = 1; n <= M.PlayerHand.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerHand[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerHand[n].ID) + "|");
                            // strBld.Append(M.PlayerHand[n].SpecialSet + "|");
                            strBld.Append(M.PlayerHand[n].Type + "|");
                            strBld.Append(M.PlayerHand[n].Attribute + "|");
                            strBld.Append(M.PlayerHand[n].Level + "|");
                            //strBld.Append(M.PlayerHand[n].STicon + "|");
                            strBld.Append(M.PlayerHand[n].ATK + "|");
                            strBld.Append(M.PlayerHand[n].DEF + "|");
                            strBld.Append(M.PlayerHand[n].Lore + "|");
                            strBld.Append(M.PlayerHand[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerHand[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerHand[n].Facedown) + "|");
                        }

                        break;
                    case Area.Monster:
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(M.PlayerMonsters[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerMonsters[n].ID) + "|");
                            // strBld.Append(M.PlayerMonsters[n].SpecialSet + "|");
                            strBld.Append(M.PlayerMonsters[n].Type + "|");
                            strBld.Append(M.PlayerMonsters[n].Attribute + "|");
                            strBld.Append(M.PlayerMonsters[n].Level + "|");
                            //strBld.Append(M.PlayerMonsters[n].STicon + "|");
                            strBld.Append(M.PlayerMonsters[n].ATK + "|");
                            strBld.Append(M.PlayerMonsters[n].DEF + "|");
                            strBld.Append(M.PlayerMonsters[n].Lore + "|");
                            strBld.Append(M.PlayerMonsters[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerMonsters[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerMonsters[n].Facedown) + "|");
                        }

                        break;
                    case Area.FieldSpell:
                        strBld.Append(M.PlayerFSpell.Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerFSpell.ID) + "|");
                        // strBld.Append(M.PlayerFSpell.SpecialSet + "|");
                        strBld.Append(M.PlayerFSpell.Type + "|");
                        strBld.Append(M.PlayerFSpell.Attribute + "|");
                        strBld.Append(M.PlayerFSpell.Level + "|");
                        //strBld.Append(M.PlayerFSpell.STicon + "|");
                        strBld.Append(M.PlayerFSpell.ATK + "|");
                        strBld.Append(M.PlayerFSpell.DEF + "|");
                        strBld.Append(M.PlayerFSpell.Lore + "|");
                        strBld.Append(M.PlayerFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.Facedown) + "|");

                        break;
                    case Area.Xyz:
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(returnXyzSummarize(n));
                            //for (short m = 1; m <= 5; m++)
                            //{
                            //     strBld.Append(M.PlayerOverlaid[n, m] + "|");
                            //}
                        }

                        break;
                    case Area.ST:
                        for (n = 1; n <= 5; n++)
                        {
                            strBld.Append(M.PlayerST[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerST[n].ID) + "|");
                            //  strBld.Append(M.PlayerST[n].SpecialSet + "|");
                            strBld.Append(M.PlayerST[n].Type + "|");
                            strBld.Append(M.PlayerST[n].Attribute + "|");
                            strBld.Append(M.PlayerST[n].Level + "|");
                            //strBld.Append(M.PlayerST[n].STicon + "|");
                            strBld.Append(M.PlayerST[n].ATK + "|");
                            strBld.Append(M.PlayerST[n].DEF + "|");
                            strBld.Append(M.PlayerST[n].Lore + "|");
                            strBld.Append(M.PlayerST[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerST[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerST[n].Facedown) + "|");
                        }

                        break;
                    case Area.Deck:
                        for (n = 1; n <= M.PlayerDeck.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerDeck[n].ID) + "|");
                            //  strBld.Append(M.PlayerDeck[n].SpecialSet + "|");
                            strBld.Append(M.PlayerDeck[n].Type + "|");
                            strBld.Append(M.PlayerDeck[n].Attribute + "|");
                            strBld.Append(M.PlayerDeck[n].Level + "|");
                            //strBld.Append(M.PlayerDeck[n].STicon + "|");
                            strBld.Append(M.PlayerDeck[n].ATK + "|");
                            strBld.Append(M.PlayerDeck[n].DEF + "|");
                            strBld.Append(M.PlayerDeck[n].Lore + "|");
                            strBld.Append(M.PlayerDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerDeck[n].Facedown) + "|");
                        }

                        break;
                    case Area.Grave:
                        for (n = 1; n <= M.PlayerGrave.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerGrave[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerGrave[n].ID) + "|");
                            //  strBld.Append(M.PlayerGrave[n].SpecialSet + "|");
                            strBld.Append(M.PlayerGrave[n].Type + "|");
                            strBld.Append(M.PlayerGrave[n].Attribute + "|");
                            strBld.Append(M.PlayerGrave[n].Level + "|");
                            //strBld.Append(M.PlayerGrave[n].STicon + "|");
                            strBld.Append(M.PlayerGrave[n].ATK + "|");
                            strBld.Append(M.PlayerGrave[n].DEF + "|");
                            strBld.Append(M.PlayerGrave[n].Lore + "|");
                            strBld.Append(M.PlayerGrave[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerGrave[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerGrave[n].Facedown) + "|");
                        }

                        break;
                    case Area.RFG:
                        for (n = 1; n <= M.PlayerRFG.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerRFG[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerRFG[n].ID) + "|");
                            // strBld.Append(M.PlayerRFG[n].SpecialSet + "|");
                            strBld.Append(M.PlayerRFG[n].Type + "|");
                            strBld.Append(M.PlayerRFG[n].Attribute + "|");
                            strBld.Append(M.PlayerRFG[n].Level + "|");
                            //strBld.Append(M.PlayerRFG[n].STicon + "|");
                            strBld.Append(M.PlayerRFG[n].ATK + "|");
                            strBld.Append(M.PlayerRFG[n].DEF + "|");
                            strBld.Append(M.PlayerRFG[n].Lore + "|");
                            strBld.Append(M.PlayerRFG[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerRFG[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerRFG[n].Facedown) + "|");
                        }

                        break;
                    case Area.Extra:
                        for (n = 1; n <= M.PlayerEDeck.CountNumCards(); n++)
                        {
                            strBld.Append(M.PlayerEDeck[n].Name + "|");
                            strBld.Append(Convert.ToString(M.PlayerEDeck[n].ID) + "|");
                            // strBld.Append(M.PlayerEDeck[n].SpecialSet + "|");
                            strBld.Append(M.PlayerEDeck[n].Type + "|");
                            strBld.Append(M.PlayerEDeck[n].Attribute + "|");
                            strBld.Append(M.PlayerEDeck[n].Level + "|");
                            //strBld.Append(M.PlayerEDeck[n].STicon + "|");
                            strBld.Append(M.PlayerEDeck[n].ATK + "|");
                            strBld.Append(M.PlayerEDeck[n].DEF + "|");
                            strBld.Append(M.PlayerEDeck[n].Lore + "|");
                            strBld.Append(M.PlayerEDeck[n].Counters.ToString() + "|");
                            strBld.Append(Convert.ToByte(M.PlayerEDeck[n].IsItHorizontal) + "|");
                            strBld.Append(Convert.ToByte(M.PlayerEDeck[n].Facedown) + "|");
                        }

                        break;
                    case Area.Special:
                        if (SpecialStat2 == null) throw new Exception("SpecialStat2 is null!");
                        strBld.Append(SpecialStat2.Name + "|");
                        strBld.Append(Convert.ToString(SpecialStat2.ID) + "|");
                        //strBld.Append(M.PlayerEDeck[n].SpecialSet + "|");
                        strBld.Append(SpecialStat2.Type + "|");
                        strBld.Append(SpecialStat2.Attribute + "|");
                        strBld.Append(SpecialStat2.Level + "|");
                        //strBld.Append(M.PlayerEDeck[n].STicon + "|");
                        strBld.Append(SpecialStat2.ATK + "|");
                        strBld.Append(SpecialStat2.DEF + "|");
                        strBld.Append(SpecialStat2.Lore + "|");
                        strBld.Append(SpecialStat2.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(SpecialStat2.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(SpecialStat2.Facedown) + "|");
                        break;
                }
            }

            //Wants to summarize a specific card
            if (Index2 > 0 && Area2 != Area.None &&  Element2 == StatElement.None)
            {
                strBld.Append("{" + Area2 + "|" + Index2.ToString() + "}");
                n = Index2;
                switch (Area2)
                {
                    case Area.Hand:
                        strBld.Append(M.PlayerHand[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerHand[n].ID) + "|");
                        // strBld.Append(M.PlayerHand[n].SpecialSet + "|");
                        strBld.Append(M.PlayerHand[n].Type + "|");
                        strBld.Append(M.PlayerHand[n].Attribute + "|");
                        strBld.Append(M.PlayerHand[n].Level + "|");
                        // strBld.Append(M.PlayerHand[n].STicon + "|");
                        strBld.Append(M.PlayerHand[n].ATK + "|");
                        strBld.Append(M.PlayerHand[n].DEF + "|");
                        strBld.Append(M.PlayerHand[n].Lore + "|");
                        strBld.Append(M.PlayerHand[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerHand[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerHand[n].Facedown) + "|");

                        break;
                    case Area.Monster:

                        strBld.Append(M.PlayerMonsters[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerMonsters[n].ID) + "|");
                        //  strBld.Append(M.PlayerMonsters[n].SpecialSet + "|");
                        strBld.Append(M.PlayerMonsters[n].Type + "|");
                        strBld.Append(M.PlayerMonsters[n].Attribute + "|");
                        strBld.Append(M.PlayerMonsters[n].Level + "|");
                        // strBld.Append(M.PlayerMonsters[n].STicon + "|");
                        strBld.Append(M.PlayerMonsters[n].ATK + "|");
                        strBld.Append(M.PlayerMonsters[n].DEF + "|");
                        strBld.Append(M.PlayerMonsters[n].Lore + "|");
                        strBld.Append(M.PlayerMonsters[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerMonsters[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerMonsters[n].Facedown) + "|");

                        break;
                    case Area.Xyz:
                        strBld.Append(returnXyzSummarize(n));
                        //for (short m = 1; m <= 5; m++)
                        //{
                        //    strBld.Append(M.PlayerOverlaid[n, m] + "|");
                        //}


                        break;
                    case Area.FieldSpell:
                        strBld.Append(M.PlayerFSpell.Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerFSpell.ID) + "|");
                        //  strBld.Append(M.PlayerFSpell.SpecialSet + "|");
                        strBld.Append(M.PlayerFSpell.Type + "|");
                        strBld.Append(M.PlayerFSpell.Attribute + "|");
                        strBld.Append(M.PlayerFSpell.Level + "|");
                        //strBld.Append(M.PlayerFSpell.STicon + "|");
                        strBld.Append(M.PlayerFSpell.ATK + "|");
                        strBld.Append(M.PlayerFSpell.DEF + "|");
                        strBld.Append(M.PlayerFSpell.Lore + "|");
                        strBld.Append(M.PlayerFSpell.Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerFSpell.Facedown) + "|");

                        break;

                    case Area.ST:

                        strBld.Append(M.PlayerST[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerST[n].ID) + "|");
                        //  strBld.Append(M.PlayerST[n].SpecialSet + "|");
                        strBld.Append(M.PlayerST[n].Type + "|");
                        strBld.Append(M.PlayerST[n].Attribute + "|");
                        strBld.Append(M.PlayerST[n].Level + "|");
                        // strBld.Append(M.PlayerST[n].STicon + "|");
                        strBld.Append(M.PlayerST[n].ATK + "|");
                        strBld.Append(M.PlayerST[n].DEF + "|");
                        strBld.Append(M.PlayerST[n].Lore + "|");
                        strBld.Append(M.PlayerST[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerST[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerST[n].Facedown) + "|");

                        break;
                    case Area.Deck:

                        strBld.Append(M.PlayerDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerDeck[n].ID) + "|");
                        // strBld.Append(M.PlayerDeck[n].SpecialSet + "|");
                        strBld.Append(M.PlayerDeck[n].Type + "|");
                        strBld.Append(M.PlayerDeck[n].Attribute + "|");
                        strBld.Append(M.PlayerDeck[n].Level + "|");
                        //strBld.Append(M.PlayerDeck[n].STicon + "|");
                        strBld.Append(M.PlayerDeck[n].ATK + "|");
                        strBld.Append(M.PlayerDeck[n].DEF + "|");
                        strBld.Append(M.PlayerDeck[n].Lore + "|");
                        strBld.Append(M.PlayerDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerDeck[n].Facedown) + "|");

                        break;
                    case Area.Grave:

                        strBld.Append(M.PlayerGrave[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerGrave[n].ID) + "|");
                        // strBld.Append(M.PlayerGrave[n].SpecialSet + "|");
                        strBld.Append(M.PlayerGrave[n].Type + "|");
                        strBld.Append(M.PlayerGrave[n].Attribute + "|");
                        strBld.Append(M.PlayerGrave[n].Level + "|");
                        //strBld.Append(M.PlayerGrave[n].STicon + "|");
                        strBld.Append(M.PlayerGrave[n].ATK + "|");
                        strBld.Append(M.PlayerGrave[n].DEF + "|");
                        strBld.Append(M.PlayerGrave[n].Lore + "|");
                        strBld.Append(M.PlayerGrave[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerGrave[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerGrave[n].Facedown) + "|");

                        break;
                    case Area.RFG:

                        strBld.Append(M.PlayerRFG[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerRFG[n].ID) + "|");
                        // strBld.Append(M.PlayerRFG[n].SpecialSet + "|");
                        strBld.Append(M.PlayerRFG[n].Type + "|");
                        strBld.Append(M.PlayerRFG[n].Attribute + "|");
                        strBld.Append(M.PlayerRFG[n].Level + "|");
                        //strBld.Append(M.PlayerRFG[n].STicon + "|");
                        strBld.Append(M.PlayerRFG[n].ATK + "|");
                        strBld.Append(M.PlayerRFG[n].DEF + "|");
                        strBld.Append(M.PlayerRFG[n].Lore + "|");
                        strBld.Append(M.PlayerRFG[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerRFG[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerRFG[n].Facedown) + "|");

                        break;
                    case Area.Extra:

                        strBld.Append(M.PlayerEDeck[n].Name + "|");
                        strBld.Append(Convert.ToString(M.PlayerEDeck[n].ID) + "|");
                        //  strBld.Append(M.PlayerEDeck[n].SpecialSet + "|");
                        strBld.Append(M.PlayerEDeck[n].Type + "|");
                        strBld.Append(M.PlayerEDeck[n].Attribute + "|");
                        strBld.Append(M.PlayerEDeck[n].Level + "|");
                        // strBld.Append(M.PlayerEDeck[n].STicon + "|");
                        strBld.Append(M.PlayerEDeck[n].ATK + "|");
                        strBld.Append(M.PlayerEDeck[n].DEF + "|");
                        strBld.Append(M.PlayerEDeck[n].Lore + "|");
                        strBld.Append(M.PlayerEDeck[n].Counters.ToString() + "|");
                        strBld.Append(Convert.ToByte(M.PlayerEDeck[n].IsItHorizontal) + "|");
                        strBld.Append(Convert.ToByte(M.PlayerEDeck[n].Facedown) + "|");

                        break;
                }

            }

            //Wants to summarize an element of a specific card
            if (Index2 > 0 && Area2 != Area.None && Element2 != StatElement.None)
            {
                strBld.Append("{" + Area2 + "|" + Index2.ToString() + "|" + Element2 + "}");
                n = Index2;
                switch (Area2)
                {
                    case Area.Hand:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerHand[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerHand[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerHand[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerHand[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerHand[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Monster:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerMonsters[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerMonsters[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerMonsters[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerMonsters[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerMonsters[n].Facedown) + "|");

                                break;
                        }
                        break;
                    case Area.ST:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerST[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerST[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerST[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerST[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerST[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.FieldSpell:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerFSpell.ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerFSpell.DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerFSpell.Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerFSpell.IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerFSpell.Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Grave:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerGrave[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerGrave[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerGrave[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerGrave[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerGrave[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.RFG:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerRFG[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerRFG[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerRFG[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerRFG[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerRFG[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Deck:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerDeck[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerDeck[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerDeck[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerDeck[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                    case Area.Extra:
                        switch (Element2)
                        {
                            case StatElement.ATK:
                                strBld.Append(M.PlayerEDeck[n].ATK + "|");
                                break;
                            case StatElement.DEF:
                                strBld.Append(M.PlayerEDeck[n].DEF + "|");
                                break;
                            case StatElement.Counters:
                                strBld.Append(M.PlayerEDeck[n].Counters.ToString() + "|");
                                break;
                            case StatElement.Horizontal:
                                strBld.Append(Convert.ToByte(M.PlayerEDeck[n].IsItHorizontal) + "|");
                                break;
                            case StatElement.Facedown:
                                strBld.Append(Convert.ToByte(M.PlayerEDeck[n].Facedown) + "|");
                                break;
                        }
                        break;
                }
            }
            #endregion
            #region Stats
            ///'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            //Stats: NHand, NDeck, NEDeck, NGrave, NRFG, LP (Stands for "Number")

            if (Stat1 != DuelNumeric.None)
            {
                strBld.Append("{" + Stat1 + "}");
                switch (Stat1)
                {
                    case DuelNumeric.NumHand:
                        strBld.Append(M.PlayerHand.CountNumCards() + "|");
                        break;
                    case DuelNumeric.NumDeck:
                        strBld.Append(M.PlayerDeck.CountNumCards() + "|");
                        break;
                    case DuelNumeric.NumEDeck:
                        strBld.Append(M.PlayerEDeck.CountNumCards() + "|");
                        break;
                    //case DuelNumeric.NumGrave:
                    //    strBld.Append(M.PlayerGrave.CountNumCards() + "|");
                    //    break;
                    //case DuelNumeric.NumRFG:
                    //    strBld.Append(M.PlayerRFG.CountNumCards() + "|");
                    //    break;
                    case DuelNumeric.LP:
                        strBld.Append(M.PlayerLP + "|");
                        break;
                }
            }
            if (Stat2 != DuelNumeric.None)
            {
                strBld.Append("{" + Stat2 + "}");
                switch (Stat2)
                {
                    case DuelNumeric.NumHand:
                        strBld.Append(M.PlayerHand.CountNumCards() + "|");
                        break;
                    case DuelNumeric.NumDeck:
                        strBld.Append(M.PlayerDeck.CountNumCards() + "|");
                        break;
                    case DuelNumeric.NumEDeck:
                        strBld.Append(M.PlayerEDeck.CountNumCards() + "|");
                        break;
                    //case DuelNumeric.NumGrave:
                    //    strBld.Append(M.PlayerGrave.CountNumCards() + "|");
                    //    break;
                    //case DuelNumeric.NumRFG:
                    //    strBld.Append(M.PlayerRFG.CountNumCards() + "|");
                    //    break;
                    case DuelNumeric.LP:
                        strBld.Append(M.PlayerLP + "|");
                        break;
                }
            }


            #endregion


            HandleMessageFromDuelEvent(duelEvent, Text, true);
            strBld.Append("{" + "msg" + "}"); 
            strBld.Append( ((int)duelEvent).ToString() + "|" + Text);


            #region animation

            if (bundle != null)
            {
                strBld.Append("{ani}" +
                   ((int)bundle.fromArea).ToString() + "|" +
                   ((int)bundle.fromIndex).ToString() + "|" +
                   ((int)bundle.fromIndexXyz).ToString() + "|" +
                   ((int)bundle.toArea).ToString() + "|" +
                   ((int)bundle.toIndex).ToString() + "|" +
                   ((int)bundle.specialAnimation).ToString());
                
            }

            #endregion

            if (returnOnly)
            {
                return strBld.ToString();
            }
            else
            {
                SendJabber(strBld.ToString(), TagSendToSingle);
                return null;
            }



        }
        private static string combineMultipleJabbers(List<string> messages)
        {
            while (messages.Contains(null))
            {
                messages.Remove(null);
            }
            return string.Join("_", messages.ToArray());
        }
      
        private static string returnXyzSummarize(int n)
        {
            System.Text.StringBuilder strBld = new System.Text.StringBuilder();

            for (int m = 1; m <= M.PlayerOverlaid[n].CountNumCards(); m++)
            {
                strBld.Append(M.PlayerOverlaid[n][m].Name + "|");
                strBld.Append(Convert.ToString(M.PlayerOverlaid[n][m].ID) + "|");
                strBld.Append(M.PlayerOverlaid[n][m].Type + "|");
                strBld.Append(M.PlayerOverlaid[n][m].Attribute + "|");
                strBld.Append(M.PlayerOverlaid[n][m].Level + "|");
                strBld.Append(M.PlayerOverlaid[n][m].ATK + "|");
                strBld.Append(M.PlayerOverlaid[n][m].DEF + "|");
                strBld.Append(M.PlayerOverlaid[n][m].Lore + "|");
                strBld.Append(M.PlayerOverlaid[n][m].Counters.ToString() + "|");
                strBld.Append(Convert.ToByte(M.PlayerOverlaid[n][m].IsItHorizontal) + "|");
                strBld.Append(Convert.ToByte(M.PlayerOverlaid[n][m].Facedown) + "|");
            }
            strBld.Append("-|");
            return strBld.ToString();

        }
        #region "Ping"
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
        public Action timerDelegate;
        private void pingTimer_Tick(object sender, System.EventArgs e)
        {
            if (M.IamWatching)
            {
                if (!string.IsNullOrEmpty(WatcherCurrentlyPinging))
                {
                    M.sock.SendMessage(M.socketSerialize(WatcherCurrentlyPinging, M.username, SummarizeJabber(duelEvent: DuelEvents.Test_Connection, returnOnly: true), MessageType.DuelMessage));
                }
            }
            else
            {
                SummarizeJabber(duelEvent: DuelEvents.Test_Connection);
            }
            pingTimerNumber += 1;

            if (pingTimerNumber == 50)
            {
             //   PingTimer.Stop();
             //   PingTimer.Tick -= pingTimer_Tick;
             //   this.Dispatcher.BeginInvoke(addMessageInvoke, new object[2] { "We can't connect to opponent. They may have left the duel.", false });
            }
        }
        #endregion
        public Action<string> sendDelegate;
        public void sendMessageInvoke(string serializedMessage)
        {
            M.sock.SendMessage(serializedMessage);
        }

        private Action<string, bool> addMessageInvoke;
        public void addMessage(string message, bool bold)
        {
            if (message == null) return;
            string[] splitm;
            splitm = new string[10];
            System.Text.StringBuilder newmessage = new System.Text.StringBuilder();
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
                newmessage.Append( splitm[n] + Environment.NewLine);

            newmessage.Append(message.TrimStart());

            while (lstMessages.Items.Count > 50)
                lstMessages.Items.RemoveAt(0);


            TextBlock lblNewMessage = new TextBlock();
            lblNewMessage.FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal;
            lblNewMessage.Text = newmessage.ToString();

            lstMessages.Items.Add(lblNewMessage);
            lstMessages.UpdateLayout();
            lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]); //to blank
        }

        void RemoveChildImage(Border bord)
        {
            bord.Child = null;
        }
        void RemoveChildImage(Image image)
        {
            image = null;
        }

        private void SendJabber(string data, bool TagSendToSingle = false)
        {

            try
            {
                if (M.IamWatching)
                {
                    M.sock.SendMessage(M.socketSerialize("All", M.username, data, MessageType.DuelMessage));
                }
                else
                {
                    M.sock.SendMessage(M.socketSerialize("All", M.username, data, MessageType.DuelMessage));
                }


            }
            catch { }

        }
        public void DataArrivalJabber(string data, string FromWhom = null)
        {
            string pieceOfData = null;
            int multipleJabberNumber = 0;
            int multipleJabberCount = 0;
            bool isWatcherFromMySide = M.IamWatching && FromWhom == M.WatcherMySide;
            do
            {
                if (data.Contains("_"))
                {
                    string[] dataArray = data.Split('_');
                    multipleJabberCount = dataArray.Count();
                    pieceOfData = dataArray[multipleJabberNumber];
                    multipleJabberNumber += 1;
                }
                else
                {
                    pieceOfData = data;
                }

                if (isWatcherFromMySide)
                {
                    if (M.WatcherHand.Count > 0)
                    {
                        M.WatcherHand.Clear();
                        for (int n = 1; n <= M.watcherNumCardsInHand; n++)
                        {
                            M.setImage(ImgHand(n), "back.jpg", UriKind.Relative);
                            ChangeImgHandEvents(ImgHand(n), false, null);
                        }
                    }
                }
                else
                {
                    if (M.OpponentHand.Count > 0)
                    {
                        M.OpponentHand.Clear();
                        for (int n = 1; n <= M.NumCardsInOpHand; n++)
                        {
                            M.setImage(ImgOpHand(n), "back.jpg", UriKind.Relative);
                            ChangeImgOpHandEvents(ImgOpHand(n), false, null);
                        }
                    }
                }



                ZoneInvolvement Involvement = ParceDataJabber(pieceOfData, isWatcherFromMySide);
                

                if (!Involvement.hasAnimation)
                {
                    if (isWatcherFromMySide) {
                        #region "Watcher's Side"
                        for (int n = 1; n <= 5; ++n)
                        {
                            if (Involvement.Monsters.ContainsKey(n) && Involvement.Monsters[n])
                                UpdatePictureBoxDuelField(BordMon(n), M.PlayerMonsters[n], watcherMySideSet);
                            if (Involvement.ST.ContainsKey(n) && Involvement.ST[n])
                                UpdatePictureBoxDuelField(BordST(n), M.OpponentST[n], watcherMySideSet);
                            if (Involvement.Xyz != null && Involvement.Xyz.ContainsKey(n) && Involvement.Xyz[n])
                            {
                                for (int m = 1; m <= M.PlayerOverlaid[n].CountNumCards(); m++)
                                    UpdatePictureBoxDuelField(StaXyz(n), m, M.PlayerOverlaid[n][m], watcherMySideSet);
                            }
                        }
                        if (Involvement.FSpell)
                            UpdatePictureBoxDuelField(BordFSpell, M.PlayerFSpell, watcherMySideSet);

                        if (Involvement.Hand > 0)
                        {
                            if (Involvement.Hand == REVEAL_ALL)
                            {
                                for (int n = 1; n <= M.watcherNumCardsInHand; n++)
                                {
                                    ChangeImgHandEvents(ImgHand(n), true, M.WatcherHand[n]);
                                    UpdatePictureBoxDuelField(ImgHand(n), M.WatcherHand[n], watcherMySideSet, false);
                                   
                                }
                            }
                            else
                            {
                                ChangeImgHandEvents(ImgHand(Involvement.Hand), true, M.WatcherHand[Involvement.Hand]);
                                UpdatePictureBoxDuelField(ImgHand(Involvement.Hand), M.WatcherHand[Involvement.Hand], watcherMySideSet, false);
                            }
                        }
                        
                        if (Involvement.Graveyard)
                            UpdatePictureBoxDuelField(BordGrave, M.PlayerGrave[M.PlayerGrave.CountNumCards()], watcherMySideSet);
                        if (Involvement.RFG)
                            UpdatePictureBoxDuelField(BordRFG, M.PlayerRFG[M.PlayerRFG.CountNumCards()], watcherMySideSet);
                        if (Involvement.Extra)
                        {
                            if (M.watcherNumCardsInEDeck > 0)
                                M.setImage(BordEDeck, "back.jpg", UriKind.Relative);
                            else
                                M.setImage(BordEDeck, BLANK_IMAGE, UriKind.Relative);
                        }
                        #endregion
                    } else {
                        #region "Opponent's Side"
                        for (int n = 1; n <= 5; ++n)
                        {
                            if (Involvement.Monsters.ContainsKey(n) && Involvement.Monsters[n])
                                UpdatePictureBoxDuelField(BordOpMon(n), M.OpponentMonsters[n], opponentSet);
                            if (Involvement.ST.ContainsKey(n) && Involvement.ST[n])
                                UpdatePictureBoxDuelField(BordOpST(n), M.OpponentST[n], opponentSet);
                            if (Involvement.Xyz != null && Involvement.Xyz.ContainsKey(n) && Involvement.Xyz[n])
                            {
                                for (int m = 1; m <= M.OpponentOverlaid[n].CountNumCards(); m++)
                                    UpdatePictureBoxDuelField(StaOpXyz(n), m, M.OpponentOverlaid[n][m], opponentSet);
                            }
                        }
                        if (Involvement.FSpell)
                            UpdatePictureBoxDuelField(BordOpFSpell, M.OpponentFSpell, opponentSet);

                        if (Involvement.Hand > 0)
                        {
                            if (Involvement.Hand == REVEAL_ALL)
                            {
                                for (int n = 1; n <= M.NumCardsInOpHand; n++)
                                {
                                    //ChangeImgOpHandEvents(ImgOpHand(n), true, M.OpponentHand[n]);
                                    UpdatePictureBoxDuelField(ImgOpHand(n), M.OpponentHand[n], opponentSet, false);
                                }
                            }
                            else
                            {
                                //ChangeImgOpHandEvents(ImgOpHand(Involvement.Hand), true, M.OpponentHand[Involvement.Hand]);
                                UpdatePictureBoxDuelField(ImgOpHand(Involvement.Hand), M.OpponentHand[Involvement.Hand], opponentSet, false);
                            }
                        }
                        if (Involvement.Graveyard)
                            UpdatePictureBoxDuelField(BordOpGrave, M.OpponentGrave[M.OpponentGrave.CountNumCards()], opponentSet);
                        if (Involvement.RFG)
                            UpdatePictureBoxDuelField(BordOpRFG, M.OpponentRFG[M.OpponentRFG.CountNumCards()], opponentSet);
                        if (Involvement.Extra)
                        {
                            if (M.NumCardsInopEDeck> 0)
                                M.setImage(BordOpEDeck, "back.jpg", UriKind.Relative);
                            else
                                M.setImage(BordOpEDeck, BLANK_IMAGE, UriKind.Relative);
                        }
                        #endregion
                    }
                }

                if (Involvement.dEvent == DuelEvents.Send_Field_To_Watcher)
                {
                    if (isWatcherFromMySide) {
                        if (Involvement.WatcherHand)
                        {
                            for (int n = 1; n <= M.watcherNumCardsInHand; n++){
                                UpdatePictureBoxDuelField(staHand, n, null, watcherMySideSet);
                                M.setImage(ImgHand(n), "back.jpg", UriKind.Relative);
                            }
                        }
                    } else {
                        if (Involvement.WatcherHand)
                        {
                            for (int n = 1; n <= M.NumCardsInOpHand; n++)
                            {
                                UpdatePictureBoxDuelField(staOpHand, n, null, opponentSet);
                                M.setImage(ImgOpHand(n), "back.jpg", UriKind.Relative);
                            }
                        }
                    }
                }
                

                

            } while (multipleJabberNumber < multipleJabberCount);
        }
        public void WatchersArrival(string user, string data)
        {
        }
        public void FromWatcherArrival(string user, string data)
        {
            string pieceOfData = null;
            int multipleJabberNumber = 0;
            int multipleJabberCount = 0;
            do
            {
                if (data.Contains("_"))
                {
                    string[] dataArray = data.Split('_');
                    multipleJabberCount = dataArray.Count();
                    pieceOfData = dataArray[multipleJabberNumber];
                    multipleJabberNumber += 1;
                }
                else
                {
                    pieceOfData = data;
                }

                pieceOfData = pieceOfData.Replace("{msg}", "");
                int bar = pieceOfData.IndexOf("|");
                DuelEvents dEvent =   (DuelEvents)Convert.ToInt32(pieceOfData.Substring(0, bar));
                switch (dEvent)
                {
                    case DuelEvents.Message:
                        addMessage(user + ": " + pieceOfData.Substring(bar + 1, pieceOfData.Length - bar - 1), false);
                        break;
                    case DuelEvents.Test_Connection:
                        M.sock.SendMessage(M.socketSerialize(user, M.username, SummarizeJabber(duelEvent: DuelEvents.Ping_Successful, returnOnly: true), MessageType.DuelMessage));
                        if (!watchersReceived.Contains(user))
                        {
                            sendFieldToWatcher(user);
                            addMessage(user + " has joined the room (watcher)", false);
                        }
                        break;
                }


            } while (multipleJabberNumber < multipleJabberCount);
        }
        public void sendFieldToWatcher(string watcherName)
        {
            M.sock.SendMessage(M.socketSerialize(watcherName, M.username, SummarizeJabber(duelEvent: DuelEvents.Opponent_Pool, Text: M.mySet, TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));
            M.sock.SendMessage(M.socketSerialize(watcherName, M.username, SummarizeJabber(Area1: Area.Monster, Area2: Area.ST, duelEvent: DuelEvents.Send_Field_To_Watcher, TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));
            M.sock.SendMessage(M.socketSerialize(watcherName, M.username, SummarizeJabber(Area1: Area.Grave, Area2: Area.RFG, Stat1: DuelNumeric.LP, Stat2: DuelNumeric.NumHand, duelEvent: DuelEvents.Send_Field_To_Watcher, TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));
            M.sock.SendMessage(M.socketSerialize(watcherName, M.username, SummarizeJabber(Area1: Area.FieldSpell, Area2: Area.Xyz, Stat1: DuelNumeric.NumDeck, Stat2: DuelNumeric.NumEDeck, duelEvent: DuelEvents.Send_Field_To_Watcher, TagSendToSingle: true, returnOnly: true), MessageType.DuelMessage));

        }

        public void HandleSocketMessage(SocketMessage msg)
        {
            switch (msg.mType)
            {
                case MessageType.DuelMessage:
                    if (M.IamWatching)
                    {
                        if (msg.From == M.WatcherMySide || msg.From == M.WatcherOtherSide)
                        {
                            DataArrivalJabber(msg.data, msg.From);
                        }
                        else
                        {
                            FromWatcherArrival(msg.From, msg.data);
                        }

                    }
                    else
                    {
                        if (msg.From == M.opponent)
                        {
                            DataArrivalJabber(msg.data);
                        }
                        else
                        {
                            FromWatcherArrival(msg.From, msg.data);
                        }
                    }
                    break;
                case MessageType.DuelEnter:

                    if (msg.data != M.opponent && msg.data != M.username)
                    {
                        watchersReceived.Remove(msg.data);
                    }

                    break;
                case MessageType.DuelLeave:
                case MessageType.Leave:
                    if (msg.data == M.opponent)
                        addMessage("Your opponent has left the game.", false);
                    else if (msg.data == M.username)
                        addMessage("You have been disconnected.", false);
                    

                    break;
            }
        }

        #region "Parsing"
        public ZoneInvolvement ParceDataJabber(string data, bool WatcherFromMySide = false)
        {
            int Lbrac = 0;
            int Rbrac = -1;
            string midl = null;
            int InitLbar = -1;
            int InitRbar = 0;
            int RightEndOfData = 0;
            string Place = null;
            int Index = 0;
            string Element = null;
            int n;
            CardDetails specialAnimationStats = null; //For drawing from opponent, 
            ZoneInvolvement Involvement = new ZoneInvolvement();

            do
            {
                Lbrac = data.IndexOf("{", Rbrac + 1);
                if (Lbrac == -1)
                    break;
                Rbrac = data.IndexOf("}", Lbrac + 1);

                try
                {
                    midl = data.Substring(Lbrac + 1, Rbrac - Lbrac - 1);
                }
                catch
                {
                    break;
                    //This means there's no more bracket groups
                }

                //Lbrac = InStr(Rbrac + 1, data, "{")

                Place = null;
                Index = 0;
                Element = null;
                InitLbar = -1;
                string midofinitbar = null;
                do
                {
                    InitRbar = midl.IndexOf("|", InitLbar + 1);
                    if (InitRbar == -1)
                    {
                        // InitRbar = InStr(InitLbar, midl, "}")
                        midofinitbar = midl;
                        if (string.IsNullOrEmpty(Place))
                        {
                            Place = midofinitbar;
                        }
                        else if (Index == 0)
                        {
                            Index = Convert.ToInt32(midofinitbar.Substring(InitLbar + 1, midofinitbar.Length - InitLbar - 1));
                        }
                        else if (string.IsNullOrEmpty(Element))
                        {
                            Element = midofinitbar.Substring(InitLbar + 1, midofinitbar.Length - InitLbar - 1);
                        }
                        break;
                    }
                    midofinitbar = midl.Substring(InitLbar + 1, InitRbar - InitLbar - 1);
                    InitLbar = InitRbar;
                    if (string.IsNullOrEmpty(Place))
                    {
                        Place = midofinitbar;
                    }
                    else if (Index == 0)
                    {
                        Index = Convert.ToInt32(midofinitbar);
                    }
                    else if (string.IsNullOrEmpty(Element))
                    {
                        Element = midofinitbar;
                    }
                } while (true);

                string myData = null;
                //The data to parce
                RightEndOfData = data.IndexOf("{", Rbrac);
                //Takes the RBrac from the last data round
                // and finds the next {, which in-between will be the data to parce
                if (RightEndOfData == -1)
                {
                    myData = data.Substring(Rbrac + 1, data.Length - Rbrac - 1);
                }
                else
                {
                    myData = data.Substring(Rbrac + 1, RightEndOfData - Rbrac - 1);
                    //Right end is represented by the {
                }

                    switch (Place)
                    {
                        //NOTE: THIS CHECKS FOR CARDDETAILS, STATS AND MESSAGES
                        case "Hand":
                            if (Index > 0)
                            {
                                Involvement.Hand = Index;
                                CardDetails tempHand = new CardDetails();
                                ParceIntricaciesJabber(myData, tempHand);
                                if (!WatcherFromMySide) M.OpponentHand.Add(Index, tempHand);
                                else { if (M.WatcherHand == null) M.WatcherHand = new Dictionary<int, CardDetails>(); M.WatcherHand.Add(Index, tempHand); }
                            }
                            else
                            {
                                Involvement.Hand = REVEAL_ALL;
                                CardDetails[] tempHand = new CardDetails[(WatcherFromMySide ? M.watcherNumCardsInHand : M.NumCardsInOpHand) + 1];
                                for (n = 1; n <= (WatcherFromMySide ? M.watcherNumCardsInHand : M.NumCardsInOpHand); n++) tempHand[n] = new CardDetails();
                                ParceIntricaciesJabber(myData, tempHand);
                                for (n = 1; n <= (WatcherFromMySide ? M.watcherNumCardsInHand : M.NumCardsInOpHand); n++)
                                {
                                    if (!WatcherFromMySide) M.OpponentHand.Add(n, tempHand[n]);
                                    else { if (M.WatcherHand == null) M.WatcherHand = new Dictionary<int, CardDetails>(); M.WatcherHand.Add(n, tempHand[n]); }
                                }
                            }
                            break;


                        case "Monster":
                            ParceIntricaciesJabber(myData, WatcherFromMySide ? M.PlayerMonsters : M.OpponentMonsters, Index, Element);
                            if (Index > 0)
                            {
                                Involvement.Monsters[Index] = true;
                            }
                            else
                            {
                                for (n = 1; n <= 5; n++)
                                {
                                    Involvement.Monsters[n] = true;
                                }
                            }
                            break;
                        case "ST":
                            ParceIntricaciesJabber(myData,  WatcherFromMySide ? M.PlayerST : M.OpponentST, Index, Element);
                            if (Index > 0)
                            {
                                Involvement.ST[Index] = true;
                            }
                            else
                            {
                                for (n = 1; n <= 5; n++)
                                {
                                    Involvement.ST[n] = true;
                                }
                            }
                            break;
                        case "Deck":
                            break;
                        case "Grave":
                            ParceIntricaciesJabber(myData, WatcherFromMySide ? M.PlayerGrave : M.OpponentGrave, Index, Element);
                            Involvement.Graveyard = true;
                            break;
                        case "rmGrave":
                            if (!WatcherFromMySide) M.OpponentGrave.RemoveAt(Index);
                            else                    M.PlayerGrave.RemoveAt(Index);
                            Involvement.Graveyard = true;
                            break;
                        case "FieldSpell":
                            ParceIntricaciesJabber(myData, WatcherFromMySide ? M.PlayerFSpell : M.OpponentFSpell, Index, Element);
                            Involvement.FSpell = true;
                            break;

                        case "RFG":
                            ParceIntricaciesJabber(myData, WatcherFromMySide ? M.PlayerRFG : M.OpponentRFG, Index, Element);
                            Involvement.RFG = true;
                            break;
                        case "rmRFG":
                            if (!WatcherFromMySide) M.OpponentRFG.RemoveAt(Index);
                            else                    M.PlayerRFG.RemoveAt(Index);
                            Involvement.RFG = true;
                            break;
                        case "Xyz": 
                            ParceIntricaciesJabber(myData, WatcherFromMySide ? M.PlayerOverlaid : M.OpponentOverlaid, Index);
                            if (Index > 0)
                            {
                                Involvement.Xyz[Index] = true;
                            }
                            else
                            {
                                for (n = 1; n <= 5; n++)
                                {
                                    Involvement.Xyz[n] = true;
                                }
                            }
                            break;
                        case "Special":
                            specialAnimationStats = new CardDetails();
                            ParceIntricaciesJabber(myData, specialAnimationStats); 
                            break;
                        case "NumHand":
                            if (!WatcherFromMySide) {
                                M.NumCardsInOpHand = Convert.ToInt32(myData.Substring(0, myData.Length - 1));
                                lblOpponentHandCount.Text = "Hand: " + M.NumCardsInOpHand.ToString();
                                Involvement.WatcherHand = M.IamWatching;
                            } else {
                                M.watcherNumCardsInHand = Convert.ToInt32(myData.Substring(0, myData.Length - 1));
                                Involvement.WatcherHand = M.IamWatching;
                            }
                            break;
                        case "NumDeck":
                            if (!WatcherFromMySide) {
                                M.NumCardsInopDeck = Convert.ToInt32(myData.Substring(0, myData.Length - 1));
                            } else {
                                M.watcherNumCardsInDeck = Convert.ToInt32(myData.Substring(0, myData.Length - 1));
                            }
                            Involvement.Deck = true;
                            break;
                        case "NumEDeck":
                            if (!WatcherFromMySide) {
                                M.NumCardsInopEDeck = Convert.ToInt32(myData.Substring(0, myData.Length - 1));
                            } else {
                                M.watcherNumCardsInEDeck = Convert.ToInt32(myData.Substring(0, myData.Length - 1));
                            }
                            Involvement.Extra = true;
                            break;
                        case "LP":
                            if (!WatcherFromMySide) {
                                lblOpponentLP.Text = "LP: " + myData.Substring(0, myData.Length - 1);
                            } else {
                                lblPlayerLP.Text = "LP: " + myData.Substring(0, myData.Length - 1);
                            }
                            break;
                        case "msg":
                            int bar = myData.IndexOf("|");
                            Involvement.dEvent = (DuelEvents)Convert.ToInt32(myData.Substring(0, bar));
                            Involvement.dText = myData.Substring(bar + 1, myData.Length - bar - 1);
                            Involvement.dText = Involvement.dText == "" ? null : Involvement.dText;
                            HandleMessageFromDuelEvent(Involvement.dEvent, Involvement.dText, false, M.IamWatching ? ( WatcherFromMySide ? M.WatcherMySide : M.WatcherOtherSide   ) : null );
                            
                            
                            break;

                        case "ani":
                            string[] extracted = myData.Split('|');
                            animationBundle bundle = new animationBundle();
                            bundle.fromArea = (Area)Convert.ToInt32(extracted[0]);
                            bundle.fromIndex = Convert.ToInt32(extracted[1]);
                            bundle.fromIndexXyz = Convert.ToInt32(extracted[2]);
                            bundle.toArea = (Area)Convert.ToInt32(extracted[3]);
                            bundle.toIndex = Convert.ToInt32(extracted[4]);
                            bundle.specialAnimation = (SpecialAnimation)Convert.ToInt32(extracted[5]);
                            bundle.isPlayer = WatcherFromMySide;

                            CardDetails stats = null;
                            switch (bundle.specialAnimation)
                            {
                                case SpecialAnimation.OpponentDrawFromPlayer:
                                    if (specialAnimationStats == null) throw new Exception("OpponentDrawFromPlayer, no stats sent!");

                                    specialAnimationStats.OpponentOwned = true;
                                    M.PlayerHand.Add(specialAnimationStats);
                                    SummarizeJabber(Stat1: DuelNumeric.NumHand);
                                    stats = new CardDetails();
                                    M.copyCardDetails(stats, specialAnimationStats);

                                    break;
                                default:
                                    stats = getStatsFromArea(bundle.toArea, bundle.toIndex, WatcherFromMySide,
                                bundle.toArea == Area.Xyz ?
                                (bundle.isPlayer ? M.PlayerOverlaid[bundle.toIndex].CountNumCards() : M.OpponentOverlaid[bundle.toIndex].CountNumCards() )
                                : 0);
                                    break;
                            }

                           

                            Animate(bundle, stats, Involvement.dEvent);

                            Involvement.hasAnimation = true;
                            break;
                        default:
                            throw new Exception("Oops! Forgot " + Place);
                    }

                    
            } while (true);
            return Involvement;
        }
        public static void ParceIntricaciesJabber(string data, List<CardDetails> WhichArray, int Index = 0, string SpecificElement = null)
        {
            if (string.IsNullOrEmpty(data))
                return;
            int LBar = -1;
            int RBar = 0;
            int n = 1;
            //int maxIndex = WhichArray.CountNumCards();
            if (string.IsNullOrEmpty(SpecificElement))
            {
                try
                {
                    if (Index > 0)
                        n = Index;



                    do //while (n <= maxIndex)
                    {

                        RBar = data.IndexOf("|", LBar + 1);
                        if (RBar == -1) { break; }

                        while (n > WhichArray.CountNumCards())
                        {
                            WhichArray.Add(new CardDetails());
                        }


                        WhichArray[n].Name = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;



                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].ID = Convert.ToInt32(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Type = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Attribute = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        if (LBar == -1) { break; }


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Level = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].ATK = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        if (LBar == -1) { break; }


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].DEF = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Lore = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Counters = Convert.ToSByte(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray[n].IsItHorizontal = false;
                        }
                        else
                        {
                            WhichArray[n].IsItHorizontal = true;
                        }
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray[n].Facedown = false;
                        }
                        else
                        {
                            WhichArray[n].Facedown = true;
                        }
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        n += 1;


                        if (Index > 0)
                            break;

                    } while (true);

                    n -= 1; //this is the max number of cards in list
                    while (WhichArray.CountNumCards() > n) { WhichArray.RemoveAt(WhichArray.CountNumCards()); }
                    //cuts out the extraneous cards
                }
                catch
                {

                }
            }


            if (!string.IsNullOrEmpty(SpecificElement))
            {
                switch (SpecificElement)
                {
                    case "ATK":
                        WhichArray[Index].ATK = data.Substring(0, data.Length - 1).ToIntCountingQuestions();
                        break;
                    case "DEF":
                        WhichArray[Index].DEF = data.Substring(0, data.Length - 1).ToIntCountingQuestions();
                        break;
                    case "Counters":
                        WhichArray[Index].Counters = Convert.ToSByte(data.Substring(0, data.Length - 1));
                        break;
                    case "IsItHorizontal":
                        WhichArray[Index].IsItHorizontal = data.Substring(0, data.Length - 1).StrToBool();
                        break;
                    case "Facedown":
                        WhichArray[Index].Facedown = data.Substring(0, data.Length - 1).StrToBool();
                        break;
                }
            }
        }
        public static void ParceIntricaciesJabber(string data, CardDetails[] WhichArray, int Index = 0, string SpecificElement = null)
        {
            if (string.IsNullOrEmpty(data))
                return;
            int LBar = -1;
            int RBar = 0;
            int n = 1;
            if (string.IsNullOrEmpty(SpecificElement))
            {
                try
                {
                    if (Index > 0)
                        n = Index;
                    do
                    {
                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Name = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].ID = Convert.ToInt32(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Type = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Attribute = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Level = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].ATK = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].DEF = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Lore = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n].Counters = Convert.ToSByte(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray[n].IsItHorizontal = false;
                        }
                        else
                        {
                            WhichArray[n].IsItHorizontal = true;
                        }
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray[n].Facedown = false;
                        }
                        else
                        {
                            WhichArray[n].Facedown = true;
                        }
                        LBar = RBar;

                        if (Index > 0)
                            break;

                        n += 1;
                    } while (true);
                }
                catch
                {
                   
                   
                }
            }


            if (!string.IsNullOrEmpty(SpecificElement))
            {
                switch (SpecificElement)
                {
                    case "ATK":
                        WhichArray[Index].ATK = data.Substring(0, data.Length - 1).ToIntCountingQuestions();
                        break;
                    case "DEF":
                        WhichArray[Index].DEF = data.Substring(0, data.Length - 1).ToIntCountingQuestions();
                        break;
                    case "Counters":
                        WhichArray[Index].Counters = Convert.ToSByte(data.Substring(0, data.Length - 1));
                        break;
                    case "IsItHorizontal":
                        WhichArray[Index].IsItHorizontal = data.Substring(0, data.Length - 1).StrToBool();
                        break;
                    case "Facedown":
                        WhichArray[Index].Facedown = data.Substring(0, data.Length - 1).StrToBool();
                        break;
                }
            }
        }
        public static void ParceIntricaciesJabber(string data, CardDetails WhichArray, int Index = 0, string SpecificElement = null)
        {
            if (string.IsNullOrEmpty(data))
                return;
            int LBar = -1;
            int RBar = 0;
            int n = 1;
            if (string.IsNullOrEmpty(SpecificElement))
            {
                try
                {
                    if (Index > 0)
                        n = Index;
                    do
                    {
                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.Name = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.ID = Convert.ToInt32(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.Type = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.Attribute = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;
                        
                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.Level = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.ATK = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.DEF = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.Lore = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray.Counters = Convert.ToSByte(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray.IsItHorizontal = false;
                        }
                        else
                        {
                            WhichArray.IsItHorizontal = true;
                        }
                        LBar = RBar;


                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray.Facedown = false;
                        }
                        else
                        {
                            WhichArray.Facedown = true;
                        }
                        LBar = RBar;

                        if (Index > 0)
                            break;

                        n += 1;
                    } while (true);
                }
                catch
                {
                }
            }


            if (!string.IsNullOrEmpty(SpecificElement))
            {
                switch (SpecificElement)
                {
                    case "ATK":
                        WhichArray.ATK = data.Substring(0, data.Length - 1).ToIntCountingQuestions();
                        break;
                    case "DEF":
                        WhichArray.DEF = data.Substring(0, data.Length - 1).ToIntCountingQuestions();
                        break;
                    case "Counters":
                        WhichArray.Counters = Convert.ToSByte(data.Substring(0, data.Length - 1));
                        break;
                    case "IsItHorizontal":
                        WhichArray.IsItHorizontal = data.Substring(0, data.Length - 1).StrToBool();
                        break;
                    case "Facedown":
                        WhichArray.Facedown = data.Substring(0, data.Length - 1).StrToBool();
                        break;
                }
            }
        }
        /// <summary>
        /// Parses an array of cardlists (usually the Xyz)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="WhichArray"></param>
        /// <param name="ZoneIndex"></param>
        public static void ParceIntricaciesJabber(string data, List<CardDetails>[] WhichArray, int ZoneIndex = 0)
        {
            int materialIndex = 1;
            int LBar = -1;
            int RBar = 0;
            string compareData = null;
            int zonesLooping;

            if (ZoneIndex > 0)
            { zonesLooping = 1; }
            else
            { zonesLooping = 5; }

            try
            {
                for (int n = 1; n <= zonesLooping; n++)
                {
                    materialIndex = 1;
                    WhichArray[n].ClearAndAdd();
                    if (ZoneIndex > 0) { n = ZoneIndex; }
                    do
                    {


                        RBar = data.IndexOf("|", LBar + 1);
                        compareData = data.Substring(LBar + 1, RBar - LBar - 1);
                        if (compareData == "-") //end of a zone
                        {
                            LBar = RBar;
                            materialIndex -= 1; //this is the max number of cards in list
                            while (WhichArray[n].CountNumCards() > materialIndex) { WhichArray[n].RemoveAt(WhichArray[n].CountNumCards()); }
                            break;
                        }

                        while (materialIndex > WhichArray[n].CountNumCards()) { WhichArray[n].Add(new CardDetails()); }

                        WhichArray[n][materialIndex].Name = compareData;
                        LBar = RBar;



                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].ID = Convert.ToInt32(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].Type = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].Attribute = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        if (LBar == -1) { break; }


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].Level = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        if (LBar == -1) { break; }


                        //RBar = data.IndexOf("|", LBar + 1);
                        //WhichArray[n][materialIndex].STicon = data.Substring(LBar + 1, RBar - LBar - 1);
                        //LBar = RBar;

                        //if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].ATK = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        if (LBar == -1) { break; }


                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].DEF = data.Substring(LBar + 1, RBar - LBar - 1).ToIntCountingQuestions();
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].Lore = data.Substring(LBar + 1, RBar - LBar - 1);
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        WhichArray[n][materialIndex].Counters = Convert.ToSByte(data.Substring(LBar + 1, RBar - LBar - 1));
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray[n][materialIndex].IsItHorizontal = false;
                        }
                        else
                        {
                            WhichArray[n][materialIndex].IsItHorizontal = true;
                        }
                        LBar = RBar;

                        if (LBar == -1) { break; }

                        RBar = data.IndexOf("|", LBar + 1);
                        if (data.Substring(LBar + 1, RBar - LBar - 1) == "0")
                        {
                            WhichArray[n][materialIndex].Facedown = false;
                        }
                        else
                        {
                            WhichArray[n][materialIndex].Facedown = true;
                        }
                        LBar = RBar;

                        if (RBar == data.Length - 1) { break; }

                        materialIndex += 1;

                        //if (ZoneIndex > 0)
                        //    break;


                    } while (true);


                }


                //cuts out the extraneous cards
            }
            catch { }

        }
       
        #endregion
        #endregion
        void changeEditMode(bool isEditing)
        {
            if (isEditing)
            {
                txtATK.IsEnabled = true;
                txtDEF.IsEnabled = true;
                txtCounters.IsEnabled = true;
                cmdEditStats.Content = "Save";
            }
            else
            {
                txtATK.IsEnabled = false;
                txtDEF.IsEnabled = false;
                txtCounters.IsEnabled = false;
                cmdEditStats.Content = "Edit";
            }
        }
        private void genericShowStats(SQLReference.CardDetails stats, bool isFromField, bool isEditable)
        {
            int n = 0;
            lblName.Text = stats.Name;

            if (stats.Type == null)
            {
                for (n = 1; n <= 12 - stats.Level; n++) 
                {
                    ImgStars(n).Source = null;
                }
            }
            else if (stats.Type.Contains("Xyz"))
            {
                for (n = 1; n <= 12 - stats.Level; n++)
                {
                    ImgStars(n).Source = null;
                }
                for (n = 12 - stats.Level + 1; n <= 12; n++)
                {
                    M.setImage(ImgStars(n), "RankStar.jpg", UriKind.Relative);
                }
            }
            else
            {
                for (n = 1; n <= stats.Level; n++)
                {
                    M.setImage(ImgStars(n), "Star.jpg", UriKind.Relative);
                }
                for (n = stats.Level + 1; n <= 12; n++)
                {
                    ImgStars(n).Source = null;
                }
            }

            if (stats.IsMonster())
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Visible;
                txtATK.Visibility = System.Windows.Visibility.Visible;
                txtATK.Text = stats.ATK.ToStringCountingQuestions();
                lblDEFplaceholder.Visibility = System.Windows.Visibility.Visible;
                txtDEF.Visibility = System.Windows.Visibility.Visible;
                txtDEF.Text = stats.DEF.ToStringCountingQuestions();
            }
            else
            {
                lblATKplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                txtATK.Visibility = System.Windows.Visibility.Collapsed;
                lblDEFplaceholder.Visibility = System.Windows.Visibility.Collapsed;
                txtDEF.Visibility = System.Windows.Visibility.Collapsed;
            }

            changeEditMode(false);
            cmdEditStats.Visibility = isEditable && stats.Name != null && !M.IamWatching ? Visibility.Visible : Visibility.Collapsed;
            
             

            // // DontFireChangeEvent = false;
            // DontFireChangeEventCounters = false;
            if (stats.Type != null && !stats.IsMonster())
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Visible;
                M.setImage(imgSTIcon, M.TypeToImageName(stats.Type), UriKind.Relative);
            }
            else
            {
                imgSTIcon.Visibility = System.Windows.Visibility.Collapsed;
            }

            lblLore.Text = stats.Lore == null ? "" : stats.Lore;
            lblType.Text = stats.Type == null ? null : stats.Type.NotDisplayEffect();

            if (isFromField)
            {
                txtCounters.Visibility = System.Windows.Visibility.Visible;
                lblCountersLabel.Visibility = System.Windows.Visibility.Visible;
                txtCounters.Text = stats.Counters.ToString();
            }
            else
            {
                txtCounters.Visibility = System.Windows.Visibility.Collapsed;
                lblCountersLabel.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (stats.Attribute == null)
                M.setImage(imgAttribute, BLANK_IMAGE, UriKind.Relative);
            else
                M.setImage(imgAttribute, M.AttributeToImageName(stats.Attribute), UriKind.Relative);


        }
        private void showContextMenu(ContextMenu menu, int index, MouseButtonEventArgs mouseArgs, int xyzIndex = -1)
        {
            HideAllContext();
            menu.Visibility = System.Windows.Visibility.Visible;
            menu._contextIndex = index;
            menu._xyzIndex = xyzIndex;
            TranslateTransform trans = menu.RenderTransform as TranslateTransform;
            trans.X = mouseArgs.GetPosition(LayoutRoot).X - menu.CLeft();
            trans.Y = mouseArgs.GetPosition(LayoutRoot).Y - menu.CTop() - (menu.ActualHeight > 0 ? menu.ActualHeight : menu.ContentStackPanel.Height);
        }
        
        public void UpdatePictureBoxDuelField(Image pBox, SQLReference.CardDetails stats, string theSet, bool isFromField, bool ignoreSpecialImages = false)
        {

            if (stats == null) 
            {
                M.setImage(pBox, BLANK_IMAGE, UriKind.Relative);
                return;    
            }
            if (isFromField)
            {
                if (M.IamWatching)
                {
                    if (theSet == M.mySet)
                    {
                        if (stats.OpponentOwned)
                            theSet = opponentSet;
                        else
                            theSet = watcherMySideSet;
                    }
                    else if (theSet == opponentSet)
                    {
                        if (stats.OpponentOwned)
                            theSet = watcherMySideSet;
                    }
                        
                }
                else
                {
                    if (theSet == M.mySet && stats.OpponentOwned) //On player's side of the field but opponent owned
                        theSet = opponentSet;
                    else if (theSet == opponentSet && stats.OpponentOwned) //On opponent's side of the field but player owned
                        theSet = M.mySet;
                }
            }
            try
            {

                if (stats.Facedown)
                {
                    M.setImage(pBox, "back.jpg", UriKind.Relative);
                }
                else if (string.IsNullOrEmpty(stats.Name))
                {
                    M.setImage(pBox, BLANK_IMAGE, UriKind.Relative);
                    return;
                }

                else
                {
                    if (stats.Type.Contains("Token"))
                    {
                        M.setImage(pBox, "token.jpg", UriKind.Relative);
                    }
                    else if (stats.Attribute.Contains("Trap"))
                    {
                        M.setImage(pBox, "trap.jpg", UriKind.Relative);
                    }
                    else if (stats.Attribute.Contains("Spell"))
                    {
                        M.setImage(pBox, "magic.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Ritual"))
                    {
                        M.setImage(pBox, "ritual.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Synchro"))
                    {
                        M.setImage(pBox, "synchro.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Fusion"))
                    {
                        M.setImage(pBox, "fusion.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Xyz"))
                    {
                        M.setImage(pBox, "xyz.jpg", UriKind.Relative);
                    }
                    else if (stats.Type.Contains("/Effect") && M.IsOrange(stats))
                    {
                        M.setImage(pBox, "monstereffect.jpg", UriKind.Relative);
                    }
                    else
                    {
                        M.setImage(pBox, "monster.jpg", UriKind.Relative);
                    }


                    if (M.cardsWithImages.Contains(M.getRealImageName(stats.Name, stats.ID, theSet)) && ignoreSpecialImages == false)
                    {
                        M.setImage(pBox, M.toPortalURL(stats.Name, stats.ID, theSet), UriKind.Absolute);
                    }
                }


            }
            catch
            {

            }
        }
        public void UpdatePictureBoxDuelField(Border bordBox, SQLReference.CardDetails stats, string theSet, bool ignoreSpecialImages = false)
        {
            bool isFromField = fromField(bordBox);
            if (bordBox.Child == null && stats != null)
            {
                Image image = new Image();
                bordBox.Child = image;
                UpdatePictureBoxDuelField(bordBox.Child as Image, stats, theSet, isFromField, ignoreSpecialImages);
            }
            else if (bordBox.Child != null)
                UpdatePictureBoxDuelField(bordBox.Child as Image, stats, theSet, isFromField, ignoreSpecialImages);
            
            if (isFromField) 
            {
                if (stats == null)
                {
                    cancelRotation(bordBox);
                }
                else if (stats.IsItHorizontal && (bordBox.RenderTransform as RotateTransform).Angle == 0.0)
                {
                    Rotate90Degrees(bordBox);
                }
                else if (stats.IsItHorizontal == false && (bordBox.RenderTransform as RotateTransform).Angle == 90.0)
                {
                    cancelRotation(bordBox);
                }
            }
        }
        public void UpdatePictureBoxDuelField(OverlapStackPanel stack, int statIndex, SQLReference.CardDetails stats, string theSet, bool ignoreSpecialImages = false)
        {
            if (stack.Children.Count < statIndex)
            {
                Image image = null; 
                if (object.ReferenceEquals(stack, staHand))
                {
                    image = CreateImage(name: "imgHand" + statIndex);
                    stack.OverlapAdd(image);
                    ChangeImgHandEvents(image, true, stats);
                }
                else if (object.ReferenceEquals(stack, staOpHand))
                {
                    image = CreateImage(name: "imgOpHand" + statIndex);
                    stack.OverlapAdd(image);
                }
                else
                {
                    for (int n = 1; n <= 5; n++)
                    {
                        if (object.ReferenceEquals(stack, StaXyz(n)))
                        {
                            image = CreateImage(name: "ImgXyz_" + n + "_" + statIndex);
                            stack.OverlapAdd(image);
                            ChangeImgXyzEvents(image, true, stats); break;
                        }
                        else if (object.ReferenceEquals(stack, StaOpXyz(n)))
                        {
                            image = CreateImage(name: "ImgOpXyz_" + n + "_" + statIndex);
                            stack.OverlapAdd(image);
                            ChangeImgOpXyzEvents(image, true, stats); break;
                        }
                    }
                }
               
            }


            UpdatePictureBoxDuelField(stack.Children[statIndex - 1] as Image, stats, theSet, ignoreSpecialImages);
        }
        
        bool fromField(Image image)
        {
            FrameworkElement parent = image.Parent as FrameworkElement;
            if (parent.Name.Contains("Mon") || parent.Name.Contains("ST") || parent.Name.Contains("FSpell"))
                return true;
            else
                return false;
        }
        bool fromField(Border border)
        {
            if (border.Name.Contains("Mon") || border.Name.Contains("ST") || border.Name.Contains("FSpell"))
                return true;
            else
                return false;
        }
        public static void Rotate90Degrees(FrameworkElement fw)
        {
            if (fw.RenderTransform.GetType() == typeof(MatrixTransform)) fw.RenderTransform = new RotateTransform();
            (fw.RenderTransform as RotateTransform).Angle = 90;
        }
        public static void cancelRotation(FrameworkElement fw)
        {
            if (fw.RenderTransform.GetType() == typeof(MatrixTransform)) fw.RenderTransform = new RotateTransform();
            (fw.RenderTransform as RotateTransform).Angle = 0;
        }

        void watcherResetDuel()
        {


            while (staHand.Children.Count > 0)
                staHand.OverlapRemoveAt(0);
            while (staOpHand.Children.Count > 0)
                staOpHand.OverlapRemoveAt(0);
            if (M.WatcherHand != null) M.WatcherHand.Clear();
            if (M.OpponentHand != null) M.OpponentHand.Clear();
            M.watcherNumCardsInHand = 0;
            M.NumCardsInOpHand = 0;


            for (int n = 1; n <= 5; ++n)
            {
                while (StaXyz(n).Children.Count > 0)
                    StaXyz(n).OverlapRemoveAt(0);
                while (StaOpXyz(n).Children.Count > 0)
                    StaOpXyz(n).OverlapRemoveAt(0);
                UpdatePictureBoxDuelField(BordMon(n), null, null);
                UpdatePictureBoxDuelField(BordST(n), null, null);
                UpdatePictureBoxDuelField(BordOpMon(n), null, null);
                UpdatePictureBoxDuelField(BordOpST(n), null, null);

                M.setAsNothing(M.PlayerMonsters[n]);
                M.setAsNothing(M.OpponentMonsters[n]);
                M.setAsNothing(M.PlayerST[n]);
                M.setAsNothing(M.OpponentST[n]);
            }
            UpdatePictureBoxDuelField(BordFSpell, null, null);
            UpdatePictureBoxDuelField(BordOpFSpell, null, null);
            M.setAsNothing(M.PlayerFSpell);
            M.setAsNothing(M.OpponentFSpell);

            M.setImage(BordDeck, "back.jpg", UriKind.Relative);
            M.setImage(BordOpDeck, "back.jpg", UriKind.Relative);
            M.setImage(BordEDeck, BLANK_IMAGE, UriKind.Relative);
            M.setImage(BordOpEDeck, BLANK_IMAGE, UriKind.Relative);
            M.watcherNumCardsInDeck = 0;
            M.watcherNumCardsInEDeck = 0;
            M.NumCardsInopDeck = 0;
            M.NumCardsInopEDeck = 0;

            M.setImage(BordGrave, BLANK_IMAGE, UriKind.Relative);
            M.setImage(BordRFG, BLANK_IMAGE, UriKind.Relative);
            M.setImage(BordOpGrave, BLANK_IMAGE, UriKind.Relative);
            M.setImage(BordOpRFG, BLANK_IMAGE, UriKind.Relative);
            M.PlayerGrave.ClearAndAdd();
            M.PlayerRFG.ClearAndAdd();
            M.OpponentGrave.ClearAndAdd();
            M.OpponentRFG.ClearAndAdd();


            
        }
        private MainPage MyMainPage
        {
            get
            {
                return ((MainPage)((Canvas)((Border)((Frame)Parent).Parent).Parent).Parent);
            }
        }
        #region "Properties"
        public Image ImgStars(int index)
        {
            Image img = (Image)LayoutRoot.FindName("imgStars" + index);
            return img;

        }
        public Image ImgHand(int index)
        {
                Image Img = staHand.Children[index - 1] as Image;
                return Img;
        }
        public Image ImgOpHand(int index)
        {
                Image Img = staOpHand.Children[index - 1] as Image;
                return Img;
            
        }
        public Image ImgXyz(int MonIndex, int MaterialIndex)
        {
            OverlapStackPanel parent = StaXyz(MonIndex);
            return parent.Children[MaterialIndex - 1] as Image;
        }
        public Image ImgOpXyz(int MonIndex, int MaterialIndex)
        {
            OverlapStackPanel parent = StaOpXyz(MonIndex);
            return parent.Children[MaterialIndex - 1] as Image;
        }
        public Image ImgMon(int index)
        {
            return BordMon(index).Child as Image;
        }
        public Image ImgField(int index)
        {
            if (index == 0) return null;
            if (index > 0 && index < 6)
            {
                return ImgMon(index);
            }
            else if (index > 5 && index < 11)
            {
                return ImgST(index - 5);
            }
            else if (index == 11)
            {
                return ImgFSpell();
            }
            return null;
        }
        public Image ImgST(int index)
        {
            return BordST(index).Child as Image;
        }
        public Image ImgFSpell()
        {
            return BordFSpell.Child as Image;
        }
        public Image ImgOpMon(int index)
        {
            return BordOpMon(index).Child as Image;
        }
        public Image ImgOpST(int index)
        {
            return BordOpST(index).Child as Image;
        }
        public Image ImgOpFSpell()
        {
            return BordOpFSpell.Child as Image;
        }
        public Image ImgDeck() { return BordDeck.Child as Image; }
        public Image ImgEDeck() { return BordEDeck.Child as Image; }
        public Image ImgOpDeck() { return BordOpDeck.Child as Image; }
        public Image ImgOpEDeck() { return BordOpEDeck.Child as Image; }
        public Image ImgGrave() { return BordGrave.Child as Image; }
        public Image ImgOpGrave() { return BordOpGrave.Child as Image; }
        public Image ImgRFG() { return BordRFG.Child as Image; }
        public Image ImgOpRFG() { return BordOpRFG.Child as Image; }
        public Border BordMon(int index)
        {
            Border Bord = (Border)LayoutRoot.FindName("BordMon" + index);
            return Bord;
        }
        public Border BordField(int index)
        {
            if (index == 0) return null;
            if (index > 0 && index < 6)
            {
                return BordMon(index);
            }
            else if (index > 5 && index < 11)
            {
                return BordST(index - 5);
            }
            else if (index == 11)
            {
                return BordFSpell;
            }
            return null;
        }
        public Border BordField(int index, Area area)
        {
            switch (area)
            {
                case Area.Monster:
                    return BordMon(index);
                case Area.ST:
                    return BordST(index - 5);
                case Area.FieldSpell:
                    return BordFSpell;
                default:
                    return null;
            }
        }
        public Border BordST(int index)
        {
            Border Bord = (Border)LayoutRoot.FindName("BordST" + index);
            return Bord;
        }
        public Border BordOpMon(int index)
        {
            Border Bord = (Border)LayoutRoot.FindName("BordOpMon" + index);
            return Bord;
        }
        public Border BordOpST(int index)
        {
            Border Bord = (Border)LayoutRoot.FindName("BordOpST" + index);
            return Bord;
        }
        public OverlapStackPanel StaXyz(int index)
        {
            OverlapStackPanel sta = LayoutRoot.FindName("staXyz" + index) as OverlapStackPanel;
            return sta;
        }
        public OverlapStackPanel StaOpXyz(int index)
        {
            OverlapStackPanel sta = LayoutRoot.FindName("staOpXyz" + index) as OverlapStackPanel;
            return sta;
        }
        public static SQLReference.CardDetails PlayerCurrentField(int index)
        {
            if (index >= 1 && index <= 5)
                return M.PlayerMonsters[index];
            if (index >= 6 && index <= 10)
                return M.PlayerST[index - 5];
            if (index == 11)
                return M.PlayerFSpell;

            return null;
        }
        public static SQLReference.CardDetails PlayerCurrentField(int index, Area area)
        {
            switch (area)
            {
                case Area.Monster:
                    return PlayerCurrentField(index);
                case Area.ST:
                    return PlayerCurrentField(index + 5);
                case Area.FieldSpell:
                    return PlayerCurrentField(11);
            }
            return null;
        }
        public static SQLReference.CardDetails OpponentCurrentField(int index)
        {
            if (index >= 1 && index <= 5)
                return M.OpponentMonsters[index];
            if (index >= 6 && index <= 10)
                return M.OpponentST[index - 5];
            if (index == 11)
                return M.OpponentFSpell;

            return null;
        }
        private static int NumOfPlayerMonsters
        {
            get
            {
                int functionReturnValue = 0;
                functionReturnValue = 0;
                for (int n = 1; n <= 5; n++)
                {
                    if (!string.IsNullOrEmpty(M.PlayerMonsters[n].Name))
                        functionReturnValue += 1;
                }
                return functionReturnValue;

            }
        }
        private static int NumOfPlayerST
        {
            get
            {
                int functionReturnValue = 0;
                functionReturnValue = 0;
                for (int n = 1; n <= 5; n++)
                {
                    if (!string.IsNullOrEmpty(M.PlayerST[n].Name))
                        functionReturnValue += 1;
                }
                return functionReturnValue;

            }
        }
        #endregion
        #region "Phases"
        enum Phase
        { None, Draw, Standby, Main1, Battle, Main2, End, EndTurn }
        private void ChangePhase(Phase phase)
        {
            cmdDrawPhase.FontWeight = FontWeights.Normal;
            cmdStandbyPhase.FontWeight = FontWeights.Normal;
            cmdMainPhase1.FontWeight = FontWeights.Normal;
            cmdBattlePhase.FontWeight = FontWeights.Normal;
            cmdMainPhase2.FontWeight = FontWeights.Normal;
            cmdEndPhase.FontWeight = FontWeights.Normal;
            switch (phase)
            {
                case Phase.Draw:
                    cmdDrawPhase.FontWeight = FontWeights.SemiBold;
                    break;
                case Phase.Standby:
                    cmdStandbyPhase.FontWeight = FontWeights.SemiBold;
                    break;
                case Phase.Main1:
                    cmdMainPhase1.FontWeight = FontWeights.SemiBold;
                    break;
                case Phase.Battle:
                    cmdBattlePhase.FontWeight = FontWeights.SemiBold;
                    break;
                case Phase.Main2:
                    cmdMainPhase2.FontWeight = FontWeights.SemiBold;
                    break;
                case Phase.End:
                    cmdEndPhase.FontWeight = FontWeights.SemiBold;
                    break;
            }
        }
        private void cmdDrawPhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.Draw);
            SummarizeJabber(duelEvent: DuelEvents.Draw_Phase);

        }
        private void cmdStandbyPhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.Standby);
            SummarizeJabber(duelEvent: DuelEvents.Standby_Phase);
        }
        private void cmdMainPhase1_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.Main1);
            SummarizeJabber(duelEvent: DuelEvents.Main_Phase_1);
        }
        private void cmdBattlePhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.Battle);
            SummarizeJabber(duelEvent: DuelEvents.Battle_Phase);
        }
        private void cmdMainPhase2_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.Main2);
            SummarizeJabber(duelEvent: DuelEvents.Main_Phase_2);
        }
        private void cmdEndPhase_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.End);
            SummarizeJabber(duelEvent: DuelEvents.End_Phase);

        }
        private void cmdEndTurn_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            ChangePhase(Phase.EndTurn);
            SummarizeJabber(duelEvent: DuelEvents.End_Turn);
        }
        #endregion
        #region "Other Form Features"
        private void cmdDie_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (M.IamWatching) return;
            SummarizeJabber(duelEvent:DuelEvents.Roll_Die, Text: Convert.ToString(M.Rand(1, 6, new Random())));
        }
        private void cmdCoin_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (M.IamWatching) return;
            int result = M.Rand(1, 2, new Random());
            SummarizeJabber(duelEvent: DuelEvents.Flip_Coin, Text: result == 1 ? "Heads" : "Tails");

        }
        private void cmdSendText_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSendText.Text.Trim()))
                return;
            txtSendText.Text = M.getRidOfHarmfulCharacters(txtSendText.Text);

                if (txtSendText.Text.Length >= 5 && txtSendText.Text.Substring(0, 5) == "/sub " && !M.IamWatching && playerIsConnected)
                {
                    try
                    {
                        int res = Convert.ToInt32(txtSendText.Text.Substring(5, txtSendText.Text.Length - 5).Trim());
                        M.PlayerLP -= res;
                        lblPlayerLP.Text = "LP: " + M.PlayerLP;
                        SummarizeJabber(Stat1: DuelNumeric.LP, duelEvent: DuelEvents.Lose_Lifepoints, Text: res.ToString());
                        txtSendText.Text = "";
                    }
                    catch (OverflowException) { }
                    return;
                }
                if (txtSendText.Text.Length >= 5 && txtSendText.Text.Substring(0, 5) == @"/add " && !M.IamWatching && playerIsConnected)
                {
                    try
                    {
                        int res = Convert.ToInt32(txtSendText.Text.Substring(5, txtSendText.Text.Length - 5).Trim());
                        M.PlayerLP += res;
                        lblPlayerLP.Text = "LP: " + M.PlayerLP;
                        SummarizeJabber(Stat1: DuelNumeric.LP, duelEvent: DuelEvents.Gain_Lifepoints, Text: res.ToString());
                        txtSendText.Text = "";
                    }
                    catch (OverflowException) { }
                    return;
                }

            SummarizeJabber(duelEvent:DuelEvents.Message, Text: txtSendText.Text);
          
            txtSendText.Text = "";
        }
        private void txtTurnCount_TextChanged(System.Object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (M.IamWatching) { return; }
            if (DontFireTurnCount)
            {
                DontFireTurnCount = false;
                return;
            }
            try
            {
                if (M.isNumeric(txtTurnCount.Text))
                {
                    SummarizeJabber(duelEvent:DuelEvents.Turn_Count_Change, Text: txtTurnCount.Text);
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
            animationBundle bundle = new animationBundle();
            bundle.isPlayer = true;
            bundle.specialAnimation = SpecialAnimation.ShuffleHand;
            cmdShuffleHand.IsEnabled = false;
            Animate(bundle, null, DuelEvents.Hand_Shuffle);
        }
        private void shuffleHand()
        {
            if (M.PlayerHand.CountNumCards() == 0) { cmdShuffleHand.IsEnabled = true; NextAnimation(); return; }
            
            staHand.Shuffle(this, M.PlayerHand,
                (Action)(() => { for (int n = 1; n <= M.PlayerHand.CountNumCards(); n++) M.setImage(ImgHand(n), "back.jpg", UriKind.Relative); }),
                (Action)(() =>
                {
                     for (int n = 1; n <= M.PlayerHand.CountNumCards(); n++)
                    {
                        try
                        {
                            UpdatePictureBoxDuelField(ImgHand(n), M.PlayerHand[n], M.mySet, false);
                        }
                        catch { break; }
                    }
                    cmdShuffleHand.IsEnabled = true; }));
            

            SummarizeJabber(duelEvent: DuelEvents.Hand_Shuffle);
        }
        private void opShuffleHand()
        {
            staOpHand.Shuffle(this);
        }
        private void moveMaterialAnimation(int fromZone, int toZone, bool isPlayer)
        {
            Image[] allChildren = new Image[isPlayer ? StaXyz(fromZone).Children.Count : StaOpXyz(fromZone).Children.Count];
            double[] leftsOfChildren = new double[isPlayer ? StaXyz(fromZone).Children.Count : StaOpXyz(fromZone).Children.Count];
            for (int n = (isPlayer ? StaXyz(fromZone).Children.Count : StaOpXyz(fromZone).Children.Count); n >= 1; n--)
            {
                leftsOfChildren[n - 1] = isPlayer ?
                    StaXyz(fromZone).CLeftItem(n - 1) :
                    StaOpXyz(fromZone).CLeftItem(n - 1);
            }

            for (int n = (isPlayer ? StaXyz(fromZone).Children.Count : StaOpXyz(fromZone).Children.Count); n >= 1; n--)
            {
                DoubleAnimation leftAnimation = new DoubleAnimation();
                Image item = isPlayer ? ImgXyz(fromZone, n) : ImgOpXyz(fromZone, n);
                allChildren[n - 1] = item;

                leftAnimation.From = leftsOfChildren[n - 1];
                leftAnimation.To = isPlayer ?
                    leftAnimation.From + BordMon(toZone).CLeft() - BordMon(fromZone).CLeft() :
                    leftAnimation.From + BordOpMon(toZone).CLeft() - BordOpMon(fromZone).CLeft();
                leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));
                leftAnimation.EasingFunction = new QuadraticEase();

                if (isPlayer)
                {
                    StaXyz(fromZone).Children.Remove(item);
                    LayoutRoot.Children.Add(item);
                    Image placeholder = CloneCardImage(item);
                    StaXyz(toZone).OverlapAddPlaceholder(placeholder);
                }
                else
                {
                    StaOpXyz(fromZone).Children.Remove(item);
                    LayoutRoot.Children.Add(item);
                    Image placeholder = CloneCardImage(item);
                    StaOpXyz(toZone).OverlapAddPlaceholder(placeholder);
                }



                if (n == 1)
                {
                    leftAnimation.Completed += delegate
                    {
                        for (int x = 0; x <= allChildren.Length - 1; x++)
                        {
                            LayoutRoot.Children.Remove(allChildren[x]);
                            if (isPlayer)
                                StaXyz(toZone).OverlapReplacePlaceholderAt(x, allChildren[x]);
                            else
                                StaOpXyz(toZone).OverlapReplacePlaceholderAt(x, allChildren[x]);
                        }
                        NextAnimation();
                    };
                }

                item.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            }
        }
        private void opponentDrawFromPlayerAnimation()
        {

        }
        private void cmdGainLP_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (M.IamWatching)
                return;

            if (txtLPChange.Text.Trim() == string.Empty)
                return;

            try
            {
                M.PlayerLP = M.PlayerLP + Convert.ToInt32(txtLPChange.Text);
            }
            catch
            {
                MsgBox("Must be a number.");
                return;
            }
            lblPlayerLP.Text = "LP: " + M.PlayerLP.ToString();

            SummarizeJabber(Stat1: DuelNumeric.LP, duelEvent:DuelEvents.Gain_Lifepoints, Text: txtLPChange.Text);
        }
        private void cmdLoseLP_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (M.IamWatching)
                return;


            if (txtLPChange.Text.Trim() == string.Empty)
                return;

            try
            {
                M.PlayerLP = M.PlayerLP - Convert.ToInt32(txtLPChange.Text);
            }
            catch
            {
                MsgBox("Must be a number.");
                return;
            }
            lblPlayerLP.Text = "LP: " + M.PlayerLP.ToString();
            SummarizeJabber(Stat1: DuelNumeric.LP, duelEvent: DuelEvents.Lose_Lifepoints, Text: txtLPChange.Text);
        }

        #endregion


        Image CreateImage(SQLReference.CardDetails stats = null, string setName = null, ImageSource source = null, string name = null)
        {
            Image image = new Image();
            image.Name = name;
            image.Width = 55;
            image.Height = 81;
            if (source != null)
                image.Source = source;
            else if (stats != null & setName != null)
                UpdatePictureBoxDuelField(image, stats, setName, false);
            
            return image;
        }
        Image CloneCardImage(Image image)
        {
            Image newImage = new Image();
            newImage.Width = 55; 
            newImage.Height = 81;
         
            newImage.Source = image.Source;
            return newImage;
        }


        #region "Animation"
        public enum SpecialAnimation
        {
            None, 
            ShuffleHand,
            MoveXyz,
            OpponentDrawFromPlayer
        }
        class AnimationData
        {
            public animationBundle bundle;
            public AnimationEventDelegate onComplete = null;
            public AnimationEventDelegate onBeforeAnimation = null;
            public CardDetails imageStats = null;
            public AnimationData(SpecialAnimation sAni, bool isPlayer) { bundle = new animationBundle(); bundle.specialAnimation = sAni; bundle.isPlayer = isPlayer; }
            public AnimationData(animationBundle _bundle) { bundle = _bundle; }
            public AnimationData(animationBundle _bundle, CardDetails _imageStats = null, AnimationEventDelegate _onBeforeAnimation = null, AnimationEventDelegate _onComplete = null) { bundle = _bundle; imageStats = _imageStats; onBeforeAnimation = _onBeforeAnimation; onComplete = _onComplete; }
        }
        public void NextAnimation()
        {
            object lockObject = new object();
            lock (lockObject)
            {
                isAnimating = false;
                if (AnimationQueue.Count > 0)
                {
                    AnimationData data = AnimationQueue.Dequeue();
                    Animate(data.bundle, data.imageStats, data.onBeforeAnimation, data.onComplete);
                    System.Diagnostics.Debug.WriteLine("Dequeued: " + AnimationQueue.Count + " from " + (data.bundle.isPlayer ? "Player" : "Opponent"));

                }
            }
        }
        public class AnimationCompleteEventArgs
        {
            public FrameworkElement toContainer;
            public FrameworkElement fromContainer;
            public Image image;
            public CardDetails imageStats;
            public AnimationCompleteEventArgs(FrameworkElement _toContainer, 
                FrameworkElement _fromContainer, 
                Image _image,
                CardDetails _imageStats)
            { toContainer = _toContainer; fromContainer = _fromContainer; image = _image; imageStats = _imageStats; }
        }
        public delegate void AnimationEventDelegate(AnimationCompleteEventArgs e);
        //void Animate(Image from, Border to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null) { }
        //void Animate(Image from, Image to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null) { }
        //void Animate(Image from, OverlapStackPanel to, CardDetails imageStats, AnimationEventDelegate onBeforeAction = null, AnimationEventDelegate onComplete = null) { }
        //void Animate(Border from, Image to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null) { }
        void Animate(Border from, Border to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null)
        {
            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = from.CLeft();
            leftAnimation.To = to.CLeft();
            leftAnimation.EasingFunction = new QuadraticEase();
            leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            DoubleAnimation topAnimation = new DoubleAnimation();
            topAnimation.From = from.CTop();
            topAnimation.To = to.CTop();
            topAnimation.EasingFunction = new QuadraticEase();
            topAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            Image image;
            image = CloneCardImage(from.Child as Image);

            LayoutRoot.Children.Add(image);

            if (onBeforeAnimation != null) onBeforeAnimation.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));

            topAnimation.Completed += (s, e) =>
            {
                object o = new object();
              //  lock (o){
                    LayoutRoot.Children.Remove(image);
                    to.Child = image;
                    if (onComplete != null) onComplete.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));
               // }
                NextAnimation();

            };

            isAnimating = true;
            image.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            image.BeginAnimation(Canvas.TopProperty, topAnimation);

        }
        void Animate(Border from, OverlapStackPanel to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null)
        {
            Image image;

            image = CloneCardImage(from.Child as Image);
            
            to.OverlapAddPlaceholder(CloneCardImage(image));

            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = from.CLeft();
            leftAnimation.To = to.CLeft() + to.WidthAllItems - from.Width; 
            leftAnimation.EasingFunction = new QuadraticEase();
            leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));
            
            DoubleAnimation topAnimation = new DoubleAnimation();
            topAnimation.From = from.CTop();
            topAnimation.To = to.CTop();
            topAnimation.EasingFunction = new QuadraticEase();
            topAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            LayoutRoot.Children.Add(image);

            if (onBeforeAnimation != null) onBeforeAnimation.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));

            int overlapIndex = to.Children.Count - 1;

            topAnimation.Completed += (s, e) =>
            {
                 object o = new object();
               //  lock (o) {
                     LayoutRoot.Children.Remove(image);
                     to.OverlapReplacePlaceholderAt(overlapIndex, image);
                     if (onComplete != null) onComplete.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));
                // }
                NextAnimation();
            };

            isAnimating = true;
            image.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            image.BeginAnimation(Canvas.TopProperty, topAnimation);
        }
        void Animate(OverlapStackPanel from, Image fromImage, Border to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null)
        {
            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = from.CLeftItem(fromImage);
            leftAnimation.To = to.CLeft();
            leftAnimation.EasingFunction = new QuadraticEase();
            leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            DoubleAnimation topAnimation = new DoubleAnimation();
            topAnimation.From = from.CTop();
            topAnimation.To = to.CTop();
            topAnimation.EasingFunction = new QuadraticEase();
            topAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            Image image;
            from.OverlapRemove(fromImage);
            image = CloneCardImage(fromImage);

            LayoutRoot.Children.Add(image);

            if (onBeforeAnimation != null) onBeforeAnimation.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));

            topAnimation.Completed += (s, e) =>
            {
                object o = new object();
                lock (o)
                {
                    LayoutRoot.Children.Remove(image);
                    to.Child = image;
                    if (onComplete != null) onComplete.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));
                }
                NextAnimation();


            };

            isAnimating = true;
            image.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            image.BeginAnimation(Canvas.TopProperty, topAnimation);
        }
        //void Animate(OverlapStackPanel from, Image fromImage, Image to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null) { }
        void Animate(OverlapStackPanel from, Image fromImage, OverlapStackPanel to, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null) 
        {
            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = from.CLeftItem(fromImage);
            leftAnimation.To = to.CLeft();
            leftAnimation.EasingFunction = new QuadraticEase();
            leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            DoubleAnimation topAnimation = new DoubleAnimation();
            topAnimation.From = from.CTop();
            topAnimation.To = to.CTop();
            topAnimation.EasingFunction = new QuadraticEase();
            topAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));

            Image image;


            from.OverlapRemove(fromImage);
            image = CloneCardImage(fromImage);
            to.OverlapAddPlaceholder(CloneCardImage(image));

            LayoutRoot.Children.Add(image);

            if (onBeforeAnimation != null) onBeforeAnimation.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));

            int overlapIndex = to.Children.Count - 1;

            topAnimation.Completed += (s, e) =>
            {
                object o = new object();
                lock (o)
                {
                    LayoutRoot.Children.Remove(image);
                    to.OverlapReplacePlaceholderAt(overlapIndex, image);
                    if (onComplete != null) onComplete.Invoke(new AnimationCompleteEventArgs(to, from, image, imageStats));
                }
                    NextAnimation();
                

            };

            isAnimating = true;
            image.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            image.BeginAnimation(Canvas.TopProperty, topAnimation);
        }
        void Animate(animationBundle bundle, CardDetails imageStats, AnimationEventDelegate onBeforeAnimation = null, AnimationEventDelegate onComplete = null)
        {
            if (isAnimating)
            {
                  object lockObject = new object();
                  lock (lockObject)
                  {
                     AnimationQueue.Enqueue(new AnimationData(bundle, imageStats, onBeforeAnimation, onComplete));
                      System.Diagnostics.Debug.WriteLine("Enqueued : " + AnimationQueue.Count + " from " + (bundle.isPlayer ? "Player" : "Opponent"));
                  }
                return;
            }
            switch (bundle.specialAnimation)
            {
                case SpecialAnimation.ShuffleHand:
                    isAnimating = true;
                    if (bundle.isPlayer)
                        shuffleHand();
                    else
                        opShuffleHand();
                    return;
                case SpecialAnimation.MoveXyz:
                    isAnimating = true;
                    moveMaterialAnimation(bundle.fromIndex, bundle.toIndex, bundle.isPlayer);
                    
                    return;
                case SpecialAnimation.OpponentDrawFromPlayer:
                    isAnimating = true;
                    if (bundle.isPlayer)
                        Animate(BordDeck, staOpHand, null, onBeforeAnimation, onComplete);
                    else
                        Animate(BordOpDeck, staHand, M.PlayerHand[M.PlayerHand.CountNumCards()], onBeforeAnimation, onComplete);

                    return;
                case SpecialAnimation.None:
                    break;
                default:
                    throw new NotImplementedException();
            }
           
           
            bool fromIsStack = false;
            bool toIsStack = false;
            FrameworkElement fromFWE = null;
            FrameworkElement toFWE = null;
            if (bundle.isPlayer)
            {
                #region "IsPlayer"
                switch (bundle.fromArea)
                {
                    case Area.Deck:
                        fromFWE = BordDeck;
                        break;
                    case Area.Extra:
                        fromFWE = BordEDeck;
                        break;
                    case Area.Grave:
                        fromFWE = BordGrave;
                        break;
                    case Area.RFG:
                        fromFWE = BordRFG;
                        break;
                    case Area.Hand:
                        fromFWE = staHand;
                        fromIsStack = true;
                        break;
                    case Area.Monster:
                        fromFWE = BordMon(bundle.fromIndex);
                        break;
                    case Area.ST:
                        fromFWE = BordST(bundle.fromIndex);
                        break;
                    case Area.FieldSpell:
                        fromFWE = BordFSpell;
                        break;
                    case Area.Xyz:
                        fromFWE = StaXyz(bundle.fromIndex);
                        fromIsStack = true;
                        break;
                }
                switch (bundle.toArea)
                {
                    case Area.Deck:
                        toFWE = BordDeck;
                        break;
                    case Area.Extra:
                        toFWE = BordEDeck;
                        break;
                    case Area.Grave:
                        toFWE = BordGrave;
                        break;
                    case Area.RFG:
                        toFWE = BordRFG;
                        break;
                    case Area.Hand:
                        toFWE = staHand;
                        toIsStack = true;
                        break;
                    case Area.Monster:
                        toFWE = BordMon(bundle.toIndex);
                        break;
                    case Area.ST:
                        toFWE = BordST(bundle.toIndex);
                        break;
                    case Area.FieldSpell:
                        toFWE = BordFSpell;
                        break;
                    case Area.Xyz:
                        toFWE = StaXyz(bundle.toIndex);
                        toIsStack = true;
                        break;
                }
                #endregion
            }
            else
            {
                #region "IsNotPlayer"
                switch (bundle.fromArea)
                {
                    case Area.Deck:
                        fromFWE = BordOpDeck;
                        break;
                    case Area.Extra:
                        fromFWE = BordOpEDeck;
                        break;
                    case Area.Grave:
                        fromFWE = BordOpGrave;
                        break;
                    case Area.RFG:
                        fromFWE = BordOpRFG;
                        break;
                    case Area.Hand:
                        fromFWE = staOpHand;
                        fromIsStack = true;
                        break;
                    case Area.Monster:
                        fromFWE = BordOpMon(bundle.fromIndex);
                        break;
                    case Area.ST:
                        fromFWE = BordOpST(bundle.fromIndex);
                        break;
                    case Area.FieldSpell:
                        fromFWE = BordOpFSpell;
                        break;
                    case Area.Xyz:
                        fromFWE = StaOpXyz(bundle.fromIndex);
                        fromIsStack = true;
                        break;
                }
                switch (bundle.toArea)
                {
                    case Area.Deck:
                        toFWE = BordOpDeck;
                        break;
                    case Area.Extra:
                        toFWE = BordOpEDeck;
                        break;
                    case Area.Grave:
                        toFWE = BordOpGrave;
                        break;
                    case Area.RFG:
                        toFWE = BordOpRFG;
                        break;
                    case Area.Hand:
                        toFWE = staOpHand;
                        toIsStack = true;
                        break;
                    case Area.Monster:
                        toFWE = BordOpMon(bundle.toIndex);
                        break;
                    case Area.ST:
                        toFWE = BordOpST(bundle.toIndex);
                        break;
                    case Area.FieldSpell:
                        toFWE = BordOpFSpell;
                        break;
                    case Area.Xyz:
                        toFWE = StaOpXyz(bundle.toIndex);
                        toIsStack = true;
                        break;
                }
                #endregion
            }

            if (fromIsStack)
            {
                if (bundle.fromIndexXyz > 0)
                {
                    if (toIsStack)
                        Animate(fromFWE as OverlapStackPanel, (fromFWE as OverlapStackPanel).Children[bundle.fromIndexXyz - 1] as Image, toFWE as OverlapStackPanel, imageStats, onBeforeAnimation, onComplete);
                    else // toIsBorder
                        Animate(fromFWE as OverlapStackPanel, (fromFWE as OverlapStackPanel).Children[bundle.fromIndexXyz - 1] as Image, toFWE as Border, imageStats, onBeforeAnimation, onComplete);
                }
                else
                {
                    if (toIsStack)
                        Animate(fromFWE as OverlapStackPanel, (fromFWE as OverlapStackPanel).Children[bundle.fromIndex - 1] as Image, toFWE as OverlapStackPanel, imageStats, onBeforeAnimation, onComplete);
                    else // toIsBorder
                        Animate(fromFWE as OverlapStackPanel, (fromFWE as OverlapStackPanel).Children[bundle.fromIndex - 1] as Image, toFWE as Border, imageStats, onBeforeAnimation, onComplete);
                }
            }
            else //fromIsBorder
            {
                if (toIsStack)
                    Animate(fromFWE as Border, toFWE as OverlapStackPanel, imageStats, onBeforeAnimation, onComplete);
                else //toIsBorder
                    Animate(fromFWE as Border, toFWE as Border, imageStats, onBeforeAnimation, onComplete);
            }
            
        }
        void Animate(animationBundle bundle, CardDetails imageStats, DuelEvents dEvent) { Animate(bundle, imageStats, getBeforeAnimationEvent(bundle, dEvent), getOnCompleteEvent(bundle, dEvent)); }
       
        void AttackAnimation(Point origin, Point target)
        {
            imgBattleOrigin.Visibility = System.Windows.Visibility.Visible;
            imgBattleOrigin.Opacity = 1.0;
           // GetRotationAngle();
            PositionWithoutTransforms(imgBattleOrigin, imgBattleOrigin.RenderTransform as CompositeTransform);

            QuadraticEase ease = new QuadraticEase();
            ease.EasingMode = EasingMode.EaseIn;
            
            DoubleAnimation leftAnimation = new DoubleAnimation();
            leftAnimation.From = origin.X;
            leftAnimation.To = target.X;
            leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));
            leftAnimation.EasingFunction = ease;

            DoubleAnimation topAnimation = new DoubleAnimation();
            topAnimation.From = origin.Y;
            topAnimation.To = target.Y;
            topAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, ANIMATION_SPEED_MS));
            topAnimation.EasingFunction = ease;

            topAnimation.Completed += delegate
                {
                    DoubleAnimation fadeAnimation = new DoubleAnimation();
                    fadeAnimation.From = 1;
                    fadeAnimation.To = 0;
                    fadeAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
                    imgBattleOrigin.BeginAnimation(Image.OpacityProperty, fadeAnimation);
                };

            imgBattleOrigin.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            imgBattleOrigin.BeginAnimation(Canvas.TopProperty, topAnimation);
        }
        /// <summary>
        /// Changes the Framework element's RenderTransform's Translate Properties to 0, and repositions it purely based on Canvas coordinates.
        /// </summary>
        /// <param name="fwe"></param>
        /// <param name="transform"></param>
        void PositionWithoutTransforms(FrameworkElement fwe, CompositeTransform transform)
        {
            Canvas.SetLeft(fwe, fwe.CLeft() + transform.TranslateX);
            Canvas.SetTop(fwe, fwe.CTop() + transform.TranslateY);
            transform.TranslateX = 0;
            transform.TranslateY = 0;
        }
        #endregion

        void moveMaterial(int fromZone, int toZone)
        {
            while (M.PlayerOverlaid[fromZone].CountNumCards() > 0)
            {
                MoveStats(Area.Xyz, Area.Xyz, fromZone, toZone, 1, M.PlayerOverlaid[toZone].Count);
            }
            animationBundle matBundle = createBundle(Area.Xyz, Area.Xyz, true, fromZone, toZone, specialAnimation: SpecialAnimation.MoveXyz);
            SummarizeJabber(Area1: Area.Xyz, Index1: fromZone, Area2: Area.Xyz, Index2: toZone,
                bundle: matBundle);
        }

        private void LayoutRoot_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

    

        

        private int GetIndexOfCard(FrameworkElement img)
        {
            int onesPlace = -1;
            int tensPlace = -1;
            for (int n = img.Name.Length - 1; n > -1; n--)
            {
                if (img.Name[n] >= '0' && img.Name[n] <= '9')
                {
                    if (onesPlace == -1)
                    {
                        onesPlace = img.Name[n] - '0';
                    }
                    else
                    {
                        tensPlace = img.Name[n] - '0';
                        return tensPlace * 10 + onesPlace;
                    }
                }
                else
                    return onesPlace;
            }
            return -1;
        }
        private KeyValuePair<int, int> GetIndexOfXyzCard(FrameworkElement img)
        {
            int zone = -1;
            int matIndex = -1;

            OverlapStackPanel parent = img.Parent as OverlapStackPanel;
            if (parent != null)
            {
                zone = GetIndexOfCard(parent); //Not really a "card" but it works.
                matIndex = parent.GetIndexOfChild(img) + 1;
            }
            return new KeyValuePair<int, int>(zone, matIndex);
            

            /*
            for (int n = img.Name.Length -1; n > -1; n--)
            {
                if (img.Name[n] >= '0' && img.Name[n] <= '9')
                {
                    if (matIndex == -1)
                    {
                        matIndex = img.Name[n] - '0';
                    }
                    else
                    {
                        zone = img.Name[n] - '0';
                    }
                }
                else if (img.Name[n] == '_')
                    continue;
                else
                    break;
            }
            return new KeyValuePair<int,int>(zone, matIndex);
             */
        }

        private animationBundle createBundle(Area from, Area to, bool isPlayer, int fromIndex = 0, int toIndex = 0, int fromIndexXyz = 0, SpecialAnimation specialAnimation = SpecialAnimation.None)
        {
            animationBundle bundle = new animationBundle();
            bundle.fromArea = from;
            bundle.toArea = to;
            bundle.fromIndex = fromIndex;
            bundle.toIndex = toIndex;
            bundle.fromIndexXyz = fromIndexXyz;
            bundle.specialAnimation = specialAnimation;
            bundle.isPlayer = isPlayer;

            return bundle;
        }
        private AnimationEventDelegate getBeforeAnimationEvent(animationBundle bundle, DuelEvents dEvent)
        {
            AnimationEventDelegate onBeforeAnimation = null;
            if (bundle.isPlayer)
            {
                #region "Is Player FromArea"
                switch (bundle.fromArea)
                {
                    case Area.Deck:
                        onBeforeAnimation = delegate
                            {
                                if (M.PlayerDeck.CountNumCards() == 0)
                                    UpdatePictureBoxDuelField(BordDeck, null, null);
                            };
                        break;
                    case Area.Extra:
                        onBeforeAnimation = delegate
                        {
                            if (M.PlayerEDeck.CountNumCards() == 0)
                                UpdatePictureBoxDuelField(BordEDeck, null, null);
                        };
                        break;
                    case Area.Grave:
                        onBeforeAnimation = (e) =>
                            {
                                if (M.PlayerGrave.CountNumCards() == 0)
                                    UpdatePictureBoxDuelField(BordGrave, null, null);
                                else
                                    UpdatePictureBoxDuelField(BordGrave, M.PlayerGrave[M.PlayerGrave.CountNumCards()], M.mySet);

                                 UpdatePictureBoxDuelField(e.image, e.imageStats, M.mySet, false);
                            };
                        break;
                    case Area.RFG:
                        onBeforeAnimation = (e) =>
                        {
                            if (M.PlayerRFG.CountNumCards() == 0)
                                UpdatePictureBoxDuelField(BordRFG, null, null);
                            else
                                UpdatePictureBoxDuelField(BordRFG, M.PlayerRFG[M.PlayerRFG.CountNumCards()], M.mySet);

                            UpdatePictureBoxDuelField(e.image, e.imageStats, M.mySet, false);
                        };
                        break;
                    case Area.Monster:
                    case Area.ST:
                    case Area.FieldSpell:
                        onBeforeAnimation = (e) =>
                            {
                                UpdatePictureBoxDuelField(e.fromContainer as Border, null, null);
                            };
                        break;

                }
                #endregion
            }
            else
            {
                switch (bundle.fromArea)
                {
                    case Area.Deck:
                        onBeforeAnimation = delegate
                        {
                            if (M.NumCardsInopDeck == 0)
                                UpdatePictureBoxDuelField(BordOpDeck, null, null);
                        };
                        break;
                    case Area.Extra:
                        onBeforeAnimation = delegate
                        {
                            if (M.NumCardsInopEDeck == 0)
                                UpdatePictureBoxDuelField(BordOpEDeck, null, null);
                        };
                        break;
                    case Area.Grave:
                        onBeforeAnimation = (e) =>
                        {
                            if (M.OpponentGrave.CountNumCards() == 0)
                                UpdatePictureBoxDuelField(BordOpGrave, null, null);
                            else
                                UpdatePictureBoxDuelField(BordOpGrave, M.OpponentGrave[M.OpponentGrave.CountNumCards()], opponentSet);

                            UpdatePictureBoxDuelField(e.image, e.imageStats, opponentSet, false);
                        };
                        break;
                    case Area.RFG:
                        onBeforeAnimation = (e) =>
                        {
                            if (M.OpponentRFG.CountNumCards() == 0)
                                UpdatePictureBoxDuelField(BordOpRFG, null, null);
                            else
                                UpdatePictureBoxDuelField(BordOpRFG, M.OpponentRFG[M.OpponentRFG.CountNumCards()], opponentSet);

                            UpdatePictureBoxDuelField(e.image, e.imageStats, opponentSet, false);
                        };
                        break;
                    case Area.Monster:
                    case Area.ST:
                    case Area.FieldSpell:
                        onBeforeAnimation = (e) =>
                        {
                            UpdatePictureBoxDuelField(e.fromContainer as Border, null, null);
                        };
                        break;
                }
            }


            switch (dEvent)
            {
                case DuelEvents.Opponent_Draw_From_Player:
                    onBeforeAnimation = delegate
                    {
                        if (M.PlayerDeck.CountNumCards() == 0)
                            UpdatePictureBoxDuelField(BordDeck, null, null);
                    };
                    break;

            }




            return onBeforeAnimation;
        }
        private AnimationEventDelegate getOnCompleteEvent(animationBundle bundle, DuelEvents dEvent)
        {
            AnimationEventDelegate onComplete = null;
            if (bundle.isPlayer)
            {

                switch (bundle.fromArea)
                {
                    case Area.Deck:
                    case Area.Extra:
                        onComplete += (e) =>
                            {
                                if (bundle.toArea != Area.Hand & bundle.toArea != Area.Extra & bundle.toArea != Area.Deck)
                                    UpdatePictureBoxDuelField(e.image, e.imageStats, M.mySet, false);
                            };
                        break;
                    case Area.Hand:
                        if (M.IamWatching)
                        {
                            onComplete += (e) =>
                                {
                                    if (bundle.toArea != Area.Hand & bundle.toArea != Area.Extra & bundle.toArea != Area.Deck)
                                        UpdatePictureBoxDuelField(e.image, e.imageStats, watcherMySideSet, false);

                                };
                        }
                        break;
                }
                switch (bundle.toArea)
                {
                    case Area.Hand:
                        onComplete += (e) =>
                            {
                                if (!M.IamWatching)
                                {
                                    ChangeImgHandEvents(e.image, true, e.imageStats);
                                    UpdatePictureBoxDuelField(e.image, e.imageStats, M.mySet, false);
                                } else {
                                    M.setImage(e.image, "back.jpg", UriKind.Relative);

                                }
                            };
                        break;
                    case Area.Deck:
                        onComplete += delegate
                            {
                                M.setImage(BordDeck, "back.jpg", UriKind.Relative);
                            };
                        break;
                    case Area.Extra:
                        onComplete += delegate
                            {
                                M.setImage(BordEDeck, "back.jpg", UriKind.Relative);
                            };
                        break;
                    case Area.Xyz:
                        onComplete += (e) =>
                            {
                                ChangeImgXyzEvents(e.image, true, e.imageStats);
                            };
                        break;
                    case Area.Grave:
                        onComplete += (e) =>
                            {
                                UpdatePictureBoxDuelField(BordGrave, e.imageStats, M.mySet);
                            };
                        break;
                    case Area.RFG:
                        onComplete += (e) =>
                            {
                                UpdatePictureBoxDuelField(BordRFG, e.imageStats, M.mySet);
                            };
                        break;
                }
            }
            else
            {
                switch (bundle.fromArea)
                {
                    case Area.Hand:
                    case Area.Deck:
                    case Area.Extra:
                        onComplete += (e) =>
                        {
                            if (bundle.toArea != Area.Hand & bundle.toArea != Area.Extra & bundle.toArea != Area.Deck)
                                UpdatePictureBoxDuelField(e.image, e.imageStats, opponentSet, false);
                        };
                        break;
                    case Area.Monster:
                        onComplete += (e) =>
                            {
                                UpdatePictureBoxDuelField(e.image, e.imageStats, opponentSet, false);
                            };
                        break;
                    case Area.Xyz:
                        onComplete += (e) =>
                            {
                                ChangeImgOpXyzEvents(e.image, false, e.imageStats);
                            };
                        break;
                }
                switch (bundle.toArea)
                {
                    case Area.Hand:
                          onComplete += (e) =>
                            {
                                ChangeImgOpHandEvents(e.image, true, null);
                                M.setImage(e.image, "back.jpg", UriKind.Relative);
                            };
                        break;
                    case Area.Deck:
                    case Area.Extra:
                        onComplete += (e) =>
                            {
                                M.setImage(e.image, "back.jpg", UriKind.Relative);
                            };
                        break;
                    case Area.Monster:
                    case Area.ST:
                    case Area.FieldSpell:
                        onComplete += (e) =>
                        {
                            UpdatePictureBoxDuelField(e.toContainer as Border, e.imageStats, opponentSet);
                        };
                        break;
                    case Area.Xyz:
                        onComplete += (e) =>
                            {
                                ChangeImgOpXyzEvents(e.image, true, e.imageStats);
                            };
                        break;
                    case Area.Grave:
                        onComplete += (e) =>
                        {
                            UpdatePictureBoxDuelField(BordOpGrave, e.imageStats, opponentSet, bundle.fromArea == Area.Xyz ? true : false );
                        };
                        break;
                    case Area.RFG:
                        onComplete += (e) =>
                        {
                            UpdatePictureBoxDuelField(BordOpRFG, e.imageStats, opponentSet);
                        };
                        break;
                }
            }


            switch (dEvent)
            {
                case DuelEvents.Set:
                    onComplete += (e) =>
                        {
                            UpdatePictureBoxDuelField(e.toContainer as Border, e.imageStats, M.mySet);
                        };
                    break;
                case DuelEvents.Monster_Move:
                    onComplete += (e) =>
                        {
                            if (e.imageStats.IsItHorizontal)
                            {
                                Rotate90Degrees(e.toContainer);
                            }
                        };
                    break;
                case DuelEvents.Opponent_Draw_From_Player:
                    onComplete += (e) =>
                        {
                            if (bundle.isPlayer)
                            {
                                M.setImage(e.image, "back.jpg", UriKind.Relative);
                            }
                            else
                            {
                                if (M.IamWatching)
                                {
                                    M.setImage(e.image, "back.jpg", UriKind.Relative);
                                }
                                else
                                {
                                    ChangeImgHandEvents(e.image, true, e.imageStats);
                                    UpdatePictureBoxDuelField(e.image, e.imageStats, opponentSet, false);
                                }
                            }
                        };
                    break;
            }

            return onComplete;
        }
        private string DrawCard(bool returnOnly = false)
        {

            if (M.PlayerDeck.CountNumCards() == 0)
                return null;
            else if (M.PlayerHand.CountNumCards() == 20)
            {
                MessageBox.Show("You can't have more than 20 cards in your hand.");
                return null;
            }
            else
            {
               // addMessage("draw card", false);
                CardDetails stats = MoveStats(Area.Deck, Area.Hand, M.PlayerDeck.CountNumCards());

                animationBundle bundle = createBundle(Area.Deck, Area.Hand, true);

                Animate(bundle, stats, DuelEvents.Deck_Draw);

                if (returnOnly)
                    return SummarizeJabber(Stat1: DuelNumeric.NumHand, Stat2: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Deck_Draw, bundle: bundle, returnOnly: true);
                else
                {
                    SummarizeJabber(Stat1: DuelNumeric.NumHand, Stat2: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Deck_Draw, bundle: bundle);
                    return null;
                }
                
           
            }
        }

        public void Image_Failed(Image senderImage, SQLReference.CardDetails stats)
        {
            if (senderImage == null || stats == null) return;
            if (string.IsNullOrEmpty(stats.Attribute))
            {
                M.setImage(senderImage, "token.jpg", UriKind.Relative);
            }
            else
                UpdatePictureBoxDuelField(senderImage, stats, M.mySet, false, true);
        }

        public void ChangeImgHandEvents(Image imgHand, bool subscribe, CardDetails stats)
        {
            if (subscribe)
            {
                imgHand.MouseLeftButtonUp += ImgHand_MouseLeftButtonUp;
                imgHand.MouseLeftButtonDown += ImgHand_MouseLeftButtonDown;
                imgHand.MouseEnter += ImgHand_MouseEnter;
                imgHand.MouseRightButtonUp += ImgHand_MouseRightButtonUp;
                imgHand.ImageFailed += (s, e) => { Image_Failed(imgHand, stats); };
            }
            else
            {
                imgHand.MouseLeftButtonUp -= ImgHand_MouseLeftButtonUp;
                imgHand.MouseLeftButtonDown -= ImgHand_MouseLeftButtonDown;
                imgHand.MouseEnter -= ImgHand_MouseEnter;
                imgHand.MouseRightButtonUp -= ImgHand_MouseRightButtonUp;
            }
        }
        public void ChangeImgOpHandEvents(Image imgOpHand, bool subscribe, CardDetails stats)
        {
            if (subscribe)
            {
                imgOpHand.MouseLeftButtonUp += ImgOpHand_MouseLeftButtonUp;
                imgOpHand.MouseEnter += ImgOpHand_MouseEnter;
                //imgOpHand.ImageFailed += (s, e) => { Image_Failed(imgOpHand, stats); }; TODO may come back to haunt me
            }
            else
            {
                imgOpHand.MouseLeftButtonUp -= ImgOpHand_MouseLeftButtonUp;
                imgOpHand.MouseEnter -= ImgOpHand_MouseEnter;
            }
        }
        public void ImgHand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OverlapStackPanel parent = (sender as Image).Parent as OverlapStackPanel;
            if (parent == null) return;

            int statIndex = parent.GetIndexOfChild(sender) + 1;
            if (statIndex < 1 || statIndex > M.PlayerHand.CountNumCards())
                return;
            if (ImgHand(statIndex) == null) return;
            genericShowStats(M.PlayerHand[statIndex], false, false);
            
        }
        public void ImgHand_MouseEnter(object sender, MouseEventArgs e)
        {
            OverlapStackPanel parent = (sender as Image).Parent as OverlapStackPanel;
            if (parent == null) return;

            int statIndex = parent.GetIndexOfChild(sender) + 1;
            if (statIndex < 1 || statIndex > M.PlayerHand.CountNumCards())
                return;
            if (ImgHand(statIndex) == null) return;
            showNameLabel(ImgHand(statIndex), M.PlayerHand[statIndex].Name);
        }
        public void ImgHand_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching) return;
            OverlapStackPanel parent = (sender as Image).Parent as OverlapStackPanel;
            if (parent == null) return;

            int statIndex = parent.GetIndexOfChild(sender) + 1;
            if (statIndex < 1 || statIndex > M.PlayerHand.CountNumCards())
                return;
            if (ImgHand(statIndex) == null) return;
            showContextMenu(ctxtHand, statIndex, e);
        }
        public void ImgOpHand_MouseEnter(object sender, MouseEventArgs e)
        {
            OverlapStackPanel parent = (sender as Image).Parent as OverlapStackPanel;
            if (parent == null) return;

            int statIndex = parent.GetIndexOfChild(sender) + 1;
            if (statIndex < 1 || statIndex > M.OpponentHand.Count + 1)
                return;
            if (ImgOpHand(statIndex) == null) return;
            if (M.OpponentHand.ContainsKey(statIndex))
                showNameLabel(ImgOpHand(statIndex), M.OpponentHand[statIndex].Name, displayUnder: true);
        }
        public void ImgOpHand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OverlapStackPanel parent = (sender as Image).Parent as OverlapStackPanel;
            if (parent == null) return;

            int statIndex = parent.GetIndexOfChild(sender) + 1;

            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                showTargetBorder(false, statIndex + 11);
                return;
            }

            if (statIndex < 1 || statIndex > M.OpponentHand.Count + 1)
                return;


            if (ImgOpHand(statIndex) == null) return;
            if (M.OpponentHand.ContainsKey(statIndex))
                genericShowStats(M.OpponentHand[statIndex], false, false);
        }
        public void BordMon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (!M.IamWatching || !M.PlayerMonsters[statIndex].Facedown)
                genericShowStats(M.PlayerMonsters[statIndex], true, true);
            ZoneofEdit = statIndex;
            if (goingToMoveZone > 0)
            {
                if (goingToMoveZone == statIndex)
                {
                    HideAllLabelike();
                    return;
                }

                if (M.PlayerMonsters[statIndex].Name == null)
                    moveCard(statIndex);
                else
                    overlayCard(statIndex, goingToMoveZone);
                return;
            }
            if (goingToAttackZone > 0)
            {
                if (goingToAttackZone == statIndex)
                {
                    HideAllLabelike();
                    return;
                }
            }
            if (imageDragArea != Area.None)
            {

                if (M.PlayerMonsters[statIndex].Name != null)
                {
                    
                    if ( (statIndex != imageDragIndex || imageDragArea != Area.Monster) && 
                         (statIndex != imageDragIndex || imageDragArea != Area.Xyz) &&
                        M.PlayerOverlaid[statIndex].CountNumCards() < 5 )
                        HandleImageDrag(true, Area.Xyz, statIndex, M.PlayerOverlaid[statIndex].Count);
                    else
                        HandleImageDrag(false, imageDragArea, imageDragIndex, imageDragIndexXyz);
                }
                else
                    HandleImageDrag(true, Area.Monster, statIndex);
            }
            
        }
        public void BordMon_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching) return;
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (M.PlayerMonsters[statIndex].Name == null)
                showContextMenu(ctxtMonsterEmpty, statIndex, e);
            else
                showContextMenu(ctxtMonster, statIndex, e);
        }
        public void BordMon_MouseEnter(object sender, MouseEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (goingToMoveZone > 0)
            {
                showLabelikeImage(imgMoveDestination, sender as FrameworkElement);
            }
            else
            {
                if (!M.IamWatching || !M.PlayerMonsters[statIndex].Facedown)
                    showNameLabel(BordMon(statIndex), M.PlayerMonsters[statIndex].Name);
            }
        }
        public void BordST_MouseEnter(object sender, MouseEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (goingToMoveZone > 0)
            {
                if (M.PlayerST[statIndex].Name == null)
                    showLabelikeImage(imgMoveDestination, sender as FrameworkElement);
            }
            else
            {
                if (!M.IamWatching || !M.PlayerST[statIndex].Facedown)
                    showNameLabel(BordST(statIndex), M.PlayerST[statIndex].Name);
            }
            
        }
        public void BordFSpell_MouseEnter(object sender, MouseEventArgs e)
        {
            if (goingToMoveZone > 0)
            {
                if (M.PlayerFSpell.Name == null)
                    showLabelikeImage(imgMoveDestination, sender as FrameworkElement);
            }
            else
            {
                if (!M.IamWatching || !M.PlayerFSpell.Facedown)
                    showNameLabel(BordFSpell, M.PlayerFSpell.Name);
            }
        }
        public void BordST_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (statIndex == -1)
            {
                ZoneofEdit = 11;
                if (!M.IamWatching || !M.PlayerFSpell.Facedown)
                  genericShowStats(M.PlayerFSpell, true, true);
            }
            else
            {
                ZoneofEdit = statIndex + 5;
                if (!M.IamWatching || !M.PlayerST[statIndex].Facedown)
                    genericShowStats(M.PlayerST[statIndex], true, true);
            }

            if (goingToMoveZone > 0)
            {
                if (statIndex == -1 ? M.PlayerFSpell.Name != null : M.PlayerST[statIndex].Name != null)
                { //Destination is already occupied
                    HideAllLabelike();
                    return;

                }
                if (goingToMoveZone == statIndex + 5) //Destination is same as origin
                {
                    HideAllLabelike();
                    return;
                }
                moveCard(statIndex == -1 ? 11 : statIndex + 5);
                return;
            }
            if (imageDragArea != Area.None)
            {
                if ((statIndex == -1 ? M.PlayerFSpell.Name == null : M.PlayerST[statIndex].Name == null))
                    HandleImageDrag(true,
                                statIndex == -1 ? Area.FieldSpell : Area.ST,
                                statIndex);
                else
                    HandleImageDrag(false, imageDragArea, imageDragIndex, imageDragIndexXyz);
            }
        }
        public void BordST_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching) return;
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (statIndex == -1)
            {
                if (M.PlayerFSpell.Name == null)
                    return;
            }
            else
            {
                if (M.PlayerST[statIndex].Name == null)
                    return;
            }
            showContextMenu(ctxtSpellTrap, statIndex, e); //Field Spell will be -1
        }
        private void BordDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (imageDragArea != Area.None)
            {
                HandleImageDrag(true, Area.Deck, M.PlayerDeck.Count);
                return;
            }

            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e) && !M.IamWatching && playerIsConnected)
            {
                DrawCard();
            }
        }
        private void BordDeck_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching || !playerIsConnected) return;
            showContextMenu(ctxtDeck, 0, e);
        }
        public void ChangeImgXyzEvents(Image imgXyz, bool subscribe, CardDetails stats)
        {
            if (subscribe)
            {
                imgXyz.MouseEnter += ImgXyz_MouseEnter;
                imgXyz.MouseLeave += ImgXyz_MouseLeave;
                imgXyz.MouseLeftButtonUp += ImgXyz_MouseLeftButtonUp;
                imgXyz.MouseLeftButtonDown += ImgXyz_MouseLeftButtonDown;
                imgXyz.MouseRightButtonUp += ImgXyz_MouseRightButtonUp;
                imgXyz.ImageFailed += (s, e) => { Image_Failed(imgXyz, stats); };
            }
            else
            {
                imgXyz.MouseEnter -= ImgXyz_MouseEnter;
                imgXyz.MouseLeave -= ImgXyz_MouseLeave;
                imgXyz.MouseLeftButtonUp -= ImgXyz_MouseLeftButtonUp;
                imgXyz.MouseLeftButtonDown -= ImgXyz_MouseLeftButtonDown;
                imgXyz.MouseRightButtonUp -= ImgXyz_MouseRightButtonUp;
            }
        }
        public void ChangeImgOpXyzEvents(Image imgOpXyz, bool subscribe, CardDetails stats)
        {
            if (subscribe)
            {
                imgOpXyz.MouseEnter += ImgOpXyz_MouseEnter;
                imgOpXyz.MouseLeave += ImgOpXyz_MouseLeave;
                imgOpXyz.MouseLeftButtonUp += ImgOpXyz_MouseLeftButtonUp;
                imgOpXyz.ImageFailed += (s, e) => { Image_Failed(imgOpXyz, stats); };
            }
            else
            {
                imgOpXyz.MouseEnter -= ImgOpXyz_MouseEnter;
                imgOpXyz.MouseLeave -= ImgOpXyz_MouseLeave;
                imgOpXyz.MouseLeftButtonUp -= ImgOpXyz_MouseLeftButtonUp;
            }
        }
        private void ImgXyz_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching) return;
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(sender as Image);
            if (zoneToIndex.Key == -1) return;
            if (M.PlayerOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            showContextMenu(ctxtXyz, zoneToIndex.Key, e, zoneToIndex.Value);
        }
        private void ImgXyz_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(sender as Image);
            if (zoneToIndex.Key == -1) return;
            if (M.PlayerOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            genericShowStats(M.PlayerOverlaid[zoneToIndex.Key][zoneToIndex.Value], false, false);
        }
        private void ImgXyz_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(img);
            if (zoneToIndex.Key == -1) return;
            if (M.PlayerOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            showNameLabel(ImgXyz(zoneToIndex.Key, zoneToIndex.Value), M.PlayerOverlaid[zoneToIndex.Key][zoneToIndex.Value].Name);
            Canvas.SetZIndex(img, Canvas.GetZIndex(img) + 5);
        }
        private void ImgXyz_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(img);
            if (zoneToIndex.Key == -1) return;
            if (M.PlayerOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            Canvas.SetZIndex(img, Canvas.GetZIndex(img) - 5);
        }
        private void ImgOpXyz_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(sender as Image);
            if (zoneToIndex.Key == -1) return;
            if (M.OpponentOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgOpXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            genericShowStats(M.OpponentOverlaid[zoneToIndex.Key][zoneToIndex.Value], false, false);
        }
        private void ImgOpXyz_MouseEnter(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(img);
            if (zoneToIndex.Key == -1) return;
            if (M.OpponentOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgOpXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            showNameLabel(ImgOpXyz(zoneToIndex.Key, zoneToIndex.Value), M.OpponentOverlaid[zoneToIndex.Key][zoneToIndex.Value].Name);
            Canvas.SetZIndex(img, Canvas.GetZIndex(img) + 5);
        }
        private void ImgOpXyz_MouseLeave(object sender, MouseEventArgs e)
        {
            Image img = sender as Image;
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(img);
            if (zoneToIndex.Key == -1) return;
            if (M.OpponentOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgOpXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;
            Canvas.SetZIndex(img, Canvas.GetZIndex(img) - 5);
        }
        private void BordEDeck_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (imageDragArea != Area.None)
            {
                HandleImageDrag(true, Area.Extra, M.PlayerEDeck.Count);
                return;
            }
            if (!M.IamWatching && playerIsConnected)
                showViewForm(Area.Extra, true);
        }
        private void BordGrave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (imageDragArea != Area.None)
            {
                HandleImageDrag(true, Area.Grave, M.PlayerGrave.Count);
                return;
            }
            showViewForm(Area.Grave, true);
        }
        private void BordRFG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (imageDragArea != Area.None)
            {
                HandleImageDrag(true, Area.RFG, M.PlayerRFG.Count);
                return;
            }
            showViewForm(Area.RFG, true);
        }
        public void BordOpMon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);

            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                showTargetBorder(false, statIndex);
                return;
            }


            if (!M.OpponentMonsters[statIndex].Facedown)
                genericShowStats(M.OpponentMonsters[statIndex], true, false);
            if (goingToAttackZone > 0)
            {
                attack(goingToAttackZone, statIndex, true, e.GetPosition(LayoutRoot));
                return;
            }
        }
        void BordOpMon_MouseEnter(object sender, MouseEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (M.OpponentMonsters[statIndex].Name != null && M.OpponentMonsters[statIndex].Facedown == false)
                showNameLabel(BordOpMon(statIndex), M.OpponentMonsters[statIndex].Name, displayUnder: true);

        }
        private void BordOpST_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);


            if (ClassLibrary.MouseButtonHelper.IsDoubleClick(sender, e))
            {
                if (statIndex == -1) //FSpell
                    showTargetBorder(false, 11);
                else
                    showTargetBorder(false, statIndex + 5);
                return;
            }


            if ( statIndex == -1 ? !M.OpponentFSpell.Facedown : !M.OpponentST[statIndex].Facedown)
                genericShowStats(statIndex == -1 ? M.OpponentFSpell : M.OpponentST[statIndex], true, false);
        }
        void BordOpST_MouseEnter(object sender, MouseEventArgs e)
        {
            int statIndex = GetIndexOfCard(sender as FrameworkElement);
            if (statIndex == -1) //Field Spell
            {
                if (M.OpponentFSpell.Name != null && M.OpponentFSpell.Facedown == false)
                    showNameLabel(BordOpFSpell, M.OpponentFSpell.Name, displayUnder: true);
            }
            else
            {
                if (M.OpponentST[statIndex].Name != null && M.OpponentST[statIndex].Facedown == false)
                    showNameLabel(BordOpST(statIndex), M.OpponentST[statIndex].Name, displayUnder: true);
            }
        }
        private void BordOpGrave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showViewForm(Area.Grave, false);
        }
        private void BordOpRFG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            showViewForm(Area.RFG, false);
        }
        private void BordOpDeck_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordOpDeck, "Deck: " + M.NumCardsInopDeck, true);
        }
        private void BordOpEDeck_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordOpEDeck, "Extra: " + M.NumCardsInopEDeck, true);
        }
        private void BordOpGrave_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordOpGrave, "Grave: " + M.OpponentGrave.CountNumCards(), true);
        }
        private void BordOpRFG_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordOpRFG, "RFG: " + M.OpponentRFG.CountNumCards(), true);
        }
        private void BordEDeck_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordEDeck, "Extra: " + (M.IamWatching ? M.watcherNumCardsInEDeck : M.PlayerEDeck.CountNumCards() ), true);
        }
        private void BordDeck_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordDeck, "Deck: " + (M.IamWatching ? M.watcherNumCardsInDeck : M.PlayerDeck.CountNumCards()  ), true);
        }
        private void BordGrave_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordGrave, "Grave: " + M.PlayerGrave.CountNumCards(), true);
        }
        private void BordRFG_MouseEnter(object sender, MouseEventArgs e)
        {
            showNameLabel(BordRFG, "RFG: " + M.PlayerRFG.CountNumCards(), true);
        }
        #region "Dragging"
        private void BordField_MouseLeftButtonDown(Area area, int index, MouseEventArgs e)
        {
            if (M.IamWatching) return;
            if (imageDragArea != Area.None || isAnimating || ZoneofSwitch != -1) return; //Possible bug; cannot pick up image while already dragging one
            Border bord = BordField(area == Area.Monster ? index : (area == Area.ST ? index + 5 : 11));

            ZoneofEdit = area == Area.Monster ? index : (area == Area.ST ? index + 5 : 11);

            switch (area)
            {
                case Area.Monster:
                    genericShowStats(M.PlayerMonsters[index], true, true);
                    break;
                case Area.ST:
                    genericShowStats(M.PlayerST[index], true, true);
                    break;
                case Area.FieldSpell:
                    genericShowStats(M.PlayerFSpell, true, true);
                    break;
            }


            if (bord == null) throw new Exception("BordField sender is not Border");

            if (bord.Child as Image != null && goingToMoveZone == 0)
            {
                Point mousePoint = e.GetPosition(LayoutRoot);
                DraggingImage imageDrag = new DraggingImage(bord.Child as Image, 
                                                            new Point(mousePoint.X - bord.CLeft(), mousePoint.Y - bord.CTop()));
                bord.Child = null;
                LayoutRoot.Children.Add(imageDrag._image);
                imageDragArea = area;
                imageDragIndex = index;
                LayoutRoot.MouseMove += LayoutRoot_MouseMove_Drag;
                LayoutRoot.CaptureMouse();
                Canvas.SetLeft(imageDrag._image, bord.CLeft());
                Canvas.SetTop(imageDrag._image, bord.CTop());
                dragImages.Add(imageDrag);
                if (area == Area.Monster && M.PlayerOverlaid[index].CountNumCards() > 0) //Remove material
                {
                    for (int n = StaXyz(index).Children.Count - 1; n >= 0; --n )
                    {
                        Image imageRef = StaXyz(index).Children[n] as Image;
                        double left = StaXyz(index).CLeftItem(imageRef);
                        dragImages.Add(new DraggingImage(imageRef,
                                 new Point(mousePoint.X - StaXyz(index).CLeftItem(imageRef), mousePoint.Y - StaXyz(index).CTop()),
                                 true));
                        StaXyz(index).OverlapRemove(imageRef);
                        LayoutRoot.Children.Add(imageRef);
                        Canvas.SetLeft(imageRef, left);
                        Canvas.SetTop(imageRef, StaXyz(index).CTop());

                    }
                }
            }
        }
        private void ImgHand_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching) return;
            if (imageDragArea != Area.None || isAnimating) return; //Possible bug; cannot pick up image while already dragging one
            Image imgSender = sender as Image;
            OverlapStackPanel parent = imgSender.Parent as OverlapStackPanel;
            int statIndex = parent.GetIndexOfChild(sender) + 1;
            genericShowStats(M.PlayerHand[statIndex], false, false);
            Point mousePoint = e.GetPosition(LayoutRoot);
            DraggingImage imageDrag = new DraggingImage(imgSender,
                                                        new Point(mousePoint.X - parent.CLeftItem(statIndex - 1), 
                                                                 mousePoint.Y - parent.CTop()));
            imageDragArea = Area.Hand;
            imageDragIndex = statIndex;
            dragImages.Add(imageDrag);

            parent.OverlapRemove(imgSender);
            parent.OverlapInsertPlaceholder(statIndex - 1, CreateImage(name: "imgHand" + statIndex));

            LayoutRoot.Children.Add(imgSender);
            Canvas.SetLeft(imgSender, mousePoint.X - imageDrag._offset.X);
            Canvas.SetTop(imgSender, mousePoint.Y - imageDrag._offset.Y);
            LayoutRoot.MouseMove += LayoutRoot_MouseMove_Drag;
            LayoutRoot.CaptureMouse();
        }
        private void ImgXyz_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (M.IamWatching) return;
            if (imageDragArea != Area.None || isAnimating) return; //Possible bug; cannot pick up image while already dragging one
            KeyValuePair<int, int> zoneToIndex = GetIndexOfXyzCard(sender as Image);
            if (zoneToIndex.Key == -1) return;
            if (M.PlayerOverlaid[zoneToIndex.Key].CountNumCards() < zoneToIndex.Value) return;
            if (ImgXyz(zoneToIndex.Key, zoneToIndex.Value) == null) return;

            genericShowStats(M.PlayerOverlaid[zoneToIndex.Key][zoneToIndex.Value], true, false);

            Point mousePoint = e.GetPosition(LayoutRoot);
            DraggingImage dragImage = new DraggingImage(sender as Image,
                                                           new Point(mousePoint.X - StaXyz(zoneToIndex.Key).CLeftItem(zoneToIndex.Value - 1)
                                                                   , mousePoint.Y - StaXyz(zoneToIndex.Key).CTop()));

            StaXyz(zoneToIndex.Key).OverlapRemove(dragImage._image);
            LayoutRoot.Children.Add(dragImage._image);
            imageDragArea = Area.Xyz;
            imageDragIndex = zoneToIndex.Key;
            imageDragIndexXyz = zoneToIndex.Value;
            dragImages.Add(dragImage);
            Canvas.SetLeft(dragImage._image, mousePoint.X - dragImage._offset.X);
            Canvas.SetTop(dragImage._image, mousePoint.Y - dragImage._offset.Y);
            LayoutRoot.MouseMove += LayoutRoot_MouseMove_Drag;
            LayoutRoot.CaptureMouse();

        }
        /// <summary>
        /// NOTE: HANDLER IS ONLY ADDED WHEN GOING TO DRAG!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LayoutRoot_MouseMove_Drag(object sender, MouseEventArgs e)
        {

            foreach (DraggingImage imageDrag in dragImages)
            {
            Point ePoint = e.GetPosition(LayoutRoot);
            Canvas.SetLeft(imageDrag._image, ePoint.X - imageDrag._offset.X);
            Canvas.SetTop(imageDrag._image, ePoint.Y - imageDrag._offset.Y);
            }
        }
        void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HideAllContext();
            if (imageDragArea != Area.None)
            {
                Point mousePoint = e.GetPosition(LayoutRoot);
                for (int n = 1; n <= 5; ++n)
                {
                    if (BordMon(n).ContainsPoint(mousePoint))
                    {
                        if (M.PlayerMonsters[n].Name != null)
                        {

                            if ((n != imageDragIndex || imageDragArea != Area.Monster) &&
                                (n != imageDragIndex || imageDragArea != Area.Xyz) &&
                                M.PlayerOverlaid[n].CountNumCards() < 5 &&
                                (PlayerCurrentField(imageDragIndex, imageDragArea) == null || PlayerCurrentField(imageDragIndex, imageDragArea).ID != TOKEN_ID))
                                     HandleImageDrag(true, Area.Xyz, n, M.PlayerOverlaid[n].Count);

                        }
                        else
                            HandleImageDrag(true, Area.Monster, n);
                    }

                    if (BordST(n).ContainsPoint(mousePoint))
                    {
                        if (M.PlayerST[n].Name == null)
                            HandleImageDrag(true, Area.ST, n);
                    }

                }

                if (BordFSpell.ContainsPoint(mousePoint))
                {
                    if (M.PlayerFSpell.Name == null)
                        HandleImageDrag(true, Area.FieldSpell, 0);
                }

                if (staHand.PointIsNearControl(mousePoint))
                {
                    if (PlayerCurrentField(imageDragIndex, imageDragArea) != null &&
                        HandleTokenDestroying(getGlobalFromZone(imageDragArea, imageDragIndex)))
                        return;

                    if (imageDragArea != Area.Hand)
                        HandleImageDrag(true, Area.Hand, M.PlayerHand.Count);
                }

                if (BordGrave.ContainsPoint(mousePoint))
                {
                    if (PlayerCurrentField(imageDragIndex, imageDragArea) != null &&
                        HandleTokenDestroying(getGlobalFromZone(imageDragArea, imageDragIndex)))
                        return;


                    HandleImageDrag(true, Area.Grave, M.PlayerGrave.Count);
                }
                if (BordRFG.ContainsPoint(mousePoint))
                {
                    if (PlayerCurrentField(imageDragIndex, imageDragArea) != null &&
                        HandleTokenDestroying(getGlobalFromZone(imageDragArea, imageDragIndex)))
                        return;


                    HandleImageDrag(true, Area.RFG, M.PlayerRFG.Count);
                }
                if (BordDeck.ContainsPoint(mousePoint))
                {
                    if (PlayerCurrentField(imageDragIndex, imageDragArea) != null &&
                        HandleTokenDestroying(getGlobalFromZone(imageDragArea, imageDragIndex)))
                        return;


                    HandleImageDrag(true, Area.Deck, M.PlayerDeck.Count);
                }
                if (BordEDeck.ContainsPoint(mousePoint))
                {
                    if (PlayerCurrentField(imageDragIndex, imageDragArea) != null &&
                        HandleTokenDestroying(getGlobalFromZone(imageDragArea, imageDragIndex)))
                        return;

                    HandleImageDrag(true, Area.Extra, M.PlayerEDeck.Count);
                }

                HandleImageDrag(false, imageDragArea, imageDragIndex, imageDragIndexXyz);


            }
        }

        private void HandleImageDrag(bool doMove, Area dropArea, int dropIndex, int dropIndexXyz = 0)
        {
            Area fromArea = imageDragArea; int fromIndex = imageDragIndex; int fromIndexXyz = imageDragIndexXyz;
            List<DraggingImage> images = new List<DraggingImage>(dragImages);
            resetImageDrag();

            if (doMove) //Is actually a move, do stats rearranging
            {
                resetStatDisplay();
                if ( fromArea == Area.Monster &&
                    (dropArea == Area.Monster || dropArea == Area.Xyz) &&
                    M.PlayerOverlaid[fromIndex].CountNumCards() > 0) //Monster to Monster, move material
                {
                    moveMaterial(fromIndex, dropIndex);

                }
                else if (fromArea == Area.Monster &&
                        M.PlayerOverlaid[fromIndex].CountNumCards() > 0
                         ) //Monster to other, detach all material first
                {
                    detachAllMaterial(fromIndex, false);

                    if (dropArea == Area.Grave) dropIndex = M.PlayerGrave.Count; //Index changes after detaching material
                }

                bool wasFacedown = getStatsFromArea(fromArea, fromIndex, true, fromIndexXyz).Facedown;
                CardDetails stats = MoveStats(fromArea, dropArea, fromIndex, dropIndex, fromIndexXyz == 0 ? -1 : fromIndexXyz);
                SummarizeJabber(Area1: getAppropriateAreaForImageDrag(fromArea, true),
                                Index1: fromIndex,
                                Area2: getAppropriateAreaForImageDrag(dropArea, false),
                                Index2: dropIndex,
                                Stat1: getAppropriateStat(fromArea), Stat2: getAppropriateStat(dropArea),
                                duelEvent: getAppropriateDuelEventForImageDrag(fromArea, dropArea), Text: wasFacedown ? null : stats.Name,
                                bundle: createBundle(fromArea, dropArea, true, fromIndex, dropIndex, fromIndexXyz));
                                
            }

           for (int n = images.Count - 1; n > -1; n--)
            {
                DraggingImage di = images[n];
                switch (fromArea)
                {
                    case Area.Hand:
                        if (doMove)
                            staHand.OverlapRemoveAt(fromIndex - 1);
                        
                        ChangeImgHandEvents(di._image, false, null);
                        di._image.Margin = new Thickness(0);
                        break;
                    case Area.Xyz:
                        ChangeImgXyzEvents(di._image, false, null);
                        di._image.Margin = new Thickness(0);
                        break;
                    case Area.Monster:
                        UpdatePictureBoxDuelField(BordMon(fromIndex), null, null);
                        break;
                }
                switch (dropArea)
                {
                    case Area.Monster:
                        if (di._isXyz)
                            StaXyz(dropIndex).OverlapAdd(di._image);
                        else
                        {
                            BordMon(dropIndex).Child = di._image;
                            UpdatePictureBoxDuelField(BordMon(dropIndex), M.PlayerMonsters[dropIndex], M.mySet);
                        }
                        break;
                    case Area.Xyz:
                        StaXyz(dropIndex).OverlapInsert(dropIndexXyz - 1, di._image);
                        ChangeImgXyzEvents(di._image, true, null);
                        break;
                    case Area.ST:
                    case Area.FieldSpell:
                        if (!di._isXyz)
                            BordField(dropArea == Area.ST ? dropIndex + 5 : 11).Child = di._image;
                        break;
                    case Area.Hand:
                        if (!di._isXyz)
                        {
                            if (doMove)
                                staHand.OverlapInsert(dropIndex - 1, di._image);
                            else
                                staHand.OverlapReplacePlaceholderAt(dropIndex - 1, di._image);
                            ChangeImgHandEvents(di._image, true, null);
                            UpdatePictureBoxDuelField(di._image, M.PlayerHand[dropIndex], M.mySet, false);
                        }
                        break;
                    case Area.Grave:
                        if (!di._isXyz)
                            UpdatePictureBoxDuelField(BordGrave, M.PlayerGrave[M.PlayerGrave.CountNumCards()], M.mySet);
                        break;
                    case Area.RFG:
                        if (!di._isXyz)
                            UpdatePictureBoxDuelField(BordRFG, M.PlayerRFG[M.PlayerRFG.CountNumCards()], M.mySet);
                        break;
                    case Area.Extra:
                        if (!di._isXyz)
                            M.setImage(BordEDeck, "back.jpg", UriKind.Relative);
                        break;
                    case Area.Deck:
                        if (!di._isXyz)
                            M.setImage(BordDeck, "back.jpg", UriKind.Relative);
                        break;

                }
            }

      

        }
        void resetImageDrag()
        {
            foreach (var v in dragImages) LayoutRoot.Children.Remove(v._image);
            LayoutRoot.MouseMove -= LayoutRoot_MouseMove_Drag;
            LayoutRoot.ReleaseMouseCapture();
            imageDragIndex = 0;
            imageDragIndexXyz = 0;
            imageDragArea = Area.None;
            foreach (var v in dragImages) v._image.IsHitTestVisible = true;
            dragImages.Clear();
        }
        DuelEvents getAppropriateDuelEventForImageDrag(Area fromArea, Area toArea)
        {
            DuelEvents moveEvent = getEventFromMoveZones(fromArea, toArea);
            if (moveEvent != DuelEvents.None) return moveEvent;  //Just a little shortcut, not to reuse code.

            switch (fromArea)
            {
                case Area.Monster:
                #region "Monster"
                    switch (toArea)
                    {
                        case Area.Deck:
                            return DuelEvents.Spin;
                        case Area.Extra:
                            return DuelEvents.Field_To_Extra;
                        case Area.Grave:
                            return DuelEvents.Monster_To_Grave;
                        case Area.RFG:
                            return DuelEvents.Monster_To_RFG;
                        case Area.Hand:
                            return DuelEvents.Bounce;
                        case Area.Xyz:
                            return DuelEvents.Attach_Material;
                    }
                #endregion
                    break;
                case Area.ST:
                    #region "ST"
                    switch (toArea)
                    {
                        case Area.Deck:
                            return DuelEvents.Spin;
                        case Area.Extra:
                            return DuelEvents.Field_To_Extra;
                        case Area.Grave:
                            return DuelEvents.ST_To_Grave;
                        case Area.RFG:
                            return DuelEvents.ST_To_RFG;
                        case Area.Hand:
                            return DuelEvents.Bounce;
                        case Area.Xyz:
                            return DuelEvents.Attach_Material;
                    }
                    #endregion
                    break;
                case Area.FieldSpell:
                    #region "FieldSpell"
                    switch (toArea)
                    {
                        case Area.Deck:
                            return DuelEvents.Spin;
                        case Area.Extra:
                            return DuelEvents.Field_To_Extra;
                        case Area.Grave:
                            return DuelEvents.FSpell_To_Grave;
                        case Area.RFG:
                            return DuelEvents.FSpell_To_RFG;
                        case Area.Hand:
                            return DuelEvents.Bounce;
                        case Area.Xyz:
                            return DuelEvents.Attach_Material;
                    }
                    #endregion
                    break;
                case Area.Hand:
                    #region "Hand"
                    switch (toArea)
                    {
                        case Area.Monster:
                        case Area.ST:
                        case Area.FieldSpell:
                            return DuelEvents.Play;
                        case Area.Grave:
                            return DuelEvents.Hand_To_Grave;
                        case Area.RFG:
                            return DuelEvents.Hand_To_RFG;
                        case Area.Deck:
                            return DuelEvents.Hand_To_Top;
                        case Area.Extra:
                            return DuelEvents.Hand_To_Extra;
                    }
                #endregion
                    break;
                case Area.Xyz:
                #region "Xyz"
                    switch (toArea)
                    {
                        case Area.Grave:
                            return DuelEvents.Material_To_Grave;
                        case Area.RFG:
                            return DuelEvents.Material_To_RFG;
                        case Area.Hand:
                            return DuelEvents.Material_To_Hand;
                        case Area.Monster:
                            return DuelEvents.Material_To_Monster;
                        case Area.ST:
                            return DuelEvents.Material_To_ST;
                        case Area.FieldSpell:
                            return DuelEvents.Material_To_FSpell;
                        case Area.Deck:
                            return DuelEvents.Material_To_Deck;
                        case Area.Extra:
                            return DuelEvents.Material_To_Extra;
                        case Area.Xyz:
                            return DuelEvents.Attach_Material;
                    }
                #endregion
                    break;
            }
            return DuelEvents.None;
        }
        DuelNumeric getAppropriateStat(Area area)
        {
            switch (area)
            {
                case Area.Deck:
                    return DuelNumeric.NumDeck;
                case Area.Extra:
                    return DuelNumeric.NumEDeck;
                case Area.Hand:
                    return DuelNumeric.NumHand;
            }
            return DuelNumeric.None;
        }
        Area getAppropriateAreaForImageDrag(Area area, bool isFrom)
        {
            switch (area)
            {
                case Area.Grave:
                    if (isFrom)
                        return Area.rmGrave;
                    break;
                case Area.RFG:
                    if (isFrom)
                      return Area.rmRFG;
                    break;
                case Area.Hand:
                case Area.Extra:
                case Area.Deck:
                    return Area.None;
                
            }
            return area;
        }
        
        const int DropSensitivityX = 10;
        const int DropSensitivityY = 5;
        private bool IsImageDragCloseEnough(Point dropPoint, Border bord)
        {
            return (Math.Abs(bord.CLeft() - dropPoint.X) < DropSensitivityX) && (Math.Abs(bord.CTop() - dropPoint.Y) < DropSensitivityY);
        }
        private bool IsImageDragCloseEnough(Point dropPoint, OverlapStackPanel oSta)
        {
            return (dropPoint.X + DropSensitivityX > oSta.CLeft() || dropPoint.X - DropSensitivityX < oSta.CLeft() + oSta.ExpandWidth)
                && (dropPoint.Y + DropSensitivityY > oSta.CTop()  || dropPoint.Y - DropSensitivityY < oSta.CTop() + oSta.Height);
        }
        #endregion
        public CardDetails MoveStats(Area fromArea, Area toArea,
                              int fromIndex, int toIndex = -1, /*adds onto end of collection*/
                              int fromIndexXyz = -1, int toIndexXyz = -1)
        {
            SQLReference.CardDetails stats = null;

            switch (fromArea)
            {
                case Area.Deck:
                    stats = M.PlayerDeck[fromIndex];
                    M.PlayerDeck.RemoveAt(fromIndex);
                    break;
                case Area.Extra:
                    stats = M.PlayerEDeck[fromIndex];
                    M.PlayerEDeck.RemoveAt(fromIndex);
                    break;
                case Area.Grave:
                    stats = M.PlayerGrave[fromIndex];
                    M.PlayerGrave.RemoveAt(fromIndex);
                    break;
                case Area.RFG:
                    stats = M.PlayerRFG[fromIndex];
                    M.PlayerRFG.RemoveAt(fromIndex);
                    break;
                case Area.Hand:
                    stats = M.PlayerHand[fromIndex];
                    M.PlayerHand.RemoveAt(fromIndex);
                    break;
                case Area.Monster:
                    M.copyCardDetails(ref stats, M.PlayerMonsters[fromIndex]);
                    if (toArea != Area.Monster) stats.IsItHorizontal = false;
                    if (toArea != Area.Monster && toArea != Area.ST && toArea != Area.FieldSpell) stats.Facedown = false;
                    M.setAsNothing(M.PlayerMonsters[fromIndex]);
                    break;
                case Area.ST:
                    M.copyCardDetails(ref stats, M.PlayerST[fromIndex]);
                    if (toArea != Area.Monster && toArea != Area.ST && toArea != Area.FieldSpell) stats.Facedown = false;
                    M.setAsNothing(M.PlayerST[fromIndex]);
                    break;
                case Area.FieldSpell:
                    M.copyCardDetails(ref stats, M.PlayerFSpell);
                    if (toArea != Area.Monster && toArea != Area.ST && toArea != Area.FieldSpell) stats.Facedown = false;
                    M.setAsNothing(M.PlayerFSpell);
                    break;
                case Area.Xyz:
                    stats = M.PlayerOverlaid[fromIndex][fromIndexXyz];
                    M.PlayerOverlaid[fromIndex].RemoveAt(fromIndexXyz);
                    break;
                case Area.None:
                    return null;
            }

            switch (toArea)
            {
                case Area.Deck:
                    stats.Defaultize();
                    if (toIndex == -1)
                        M.PlayerDeck.Add(stats);
                    else
                        M.PlayerDeck.Insert(toIndex, stats);
                    break;
                case Area.Extra:
                    stats.Defaultize();
                    if (toIndex == -1)
                        M.PlayerEDeck.Add(stats);
                    else
                        M.PlayerEDeck.Insert(toIndex, stats);
                    break;
                case Area.Grave:
                    stats.Defaultize();
                    if (toIndex == -1)
                        M.PlayerGrave.Add(stats);
                    else
                        M.PlayerGrave.Insert(toIndex, stats);
                    break;
                case Area.RFG:
                    stats.Defaultize();
                    if (toIndex == -1)
                        M.PlayerRFG.Add(stats);
                    else
                        M.PlayerRFG.Insert(toIndex, stats);
                    break;
                case Area.Hand:
                    stats.Defaultize();
                    if (toIndex == -1)
                        M.PlayerHand.Add(stats);
                    else
                        M.PlayerHand.Insert(toIndex, stats);
                    break;
                case Area.Monster:
                    if (toIndex == -1)
                        M.copyCardDetails(ref M.PlayerMonsters[FindEmptyBordMon()], stats);
                    else
                        M.copyCardDetails(ref M.PlayerMonsters[toIndex], stats);
                    break;
                case Area.ST:
                    if (toIndex == -1)
                        M.copyCardDetails(ref M.PlayerST[FindEmptyBordMon()], stats);
                    else
                        M.copyCardDetails(ref M.PlayerST[toIndex], stats);
                    break;
                case Area.FieldSpell:
                    M.copyCardDetails(ref M.PlayerFSpell, stats);
                    break;
                case Area.Xyz:
                     if (toIndexXyz == -1)
                        M.PlayerOverlaid[toIndex].Add(stats);
                    else
                         M.PlayerOverlaid[toIndex].Insert(toIndexXyz, stats);
                    break;
            }

            return stats;

        }

        void showViewForm(Area area, bool isPlayer)
        {
            viewForm.area = area;
            viewForm.isPlayer = isPlayer;
            viewForm.opponentSet = opponentSet;
            viewForm.AllMonsterZonesFull = FindEmptyBordMon() == 0;
            viewForm.AllSTZonesFull = FindEmptyBordST() == 0;
            viewForm.Show();

        }
        void showLabelikeImage(Image labelike, FrameworkElement target)
        {
            labelike.Visibility = System.Windows.Visibility.Visible;
            labelike.Opacity = 1;
            TranslateTransform trans;
            if (labelike.RenderTransform.GetType() == typeof(TranslateTransform))
            {
                trans = labelike.RenderTransform as TranslateTransform;
                trans.X += target.CLeft() - (labelike.CLeft() + trans.X) + (target.ActualWidth / 2) - (labelike.ActualWidth / 2);
                trans.Y += target.CTop() - (labelike.CTop() + trans.Y) + (target.ActualHeight / 2) - (labelike.ActualHeight / 2);
            }
            else if (labelike.RenderTransform.GetType() == typeof(CompositeTransform))
            {
                CompositeTransform compos = labelike.RenderTransform as CompositeTransform;
                compos.TranslateX += target.CLeft() - (labelike.CLeft() + compos.TranslateX) + (target.ActualWidth / 2) - (labelike.ActualWidth / 2);
                compos.TranslateY += target.CTop() - (labelike.CTop() + compos.TranslateY) + (target.ActualHeight / 2) - (labelike.ActualHeight / 2);
            }
            else
            {
                trans = new TranslateTransform();
                trans.X = target.CLeft() - labelike.CLeft() + (target.ActualWidth / 2) - (labelike.ActualWidth / 2);
                trans.Y = target.CTop() - labelike.CTop() + (target.ActualHeight / 2) - (labelike.ActualHeight / 2);
                labelike.RenderTransform = trans;
            }
        }
        
        void showNameLabel(FrameworkElement elementToDisplayAgainst, string name, bool dontReplaceSpaces = false, bool displayUnder = false)
        {
                if (name == null)
                {
                    lblNamePopup.Visibility = System.Windows.Visibility.Collapsed;
                    return;
                }
                lblNamePopup.Visibility = System.Windows.Visibility.Visible;
                
               // lblNamePopupLayoutUpdatedEventHandler = new EventHandler((s, e) =>
                //    {
                        TextBlock block = lblNamePopup.Children[0] as TextBlock;
                        block.Text = dontReplaceSpaces ? name : name.Replace(' ', '\n');
                        //block.LayoutUpdated -= lblNamePopupLayoutUpdatedEventHandler;
                        if (elementToDisplayAgainst.Parent == null)
                        {
                           // addMessage("DEBUG: " + n + "'s parent is NULL!");
                            return;
                        }
                        if (elementToDisplayAgainst.Parent.GetType() == typeof(OverlapStackPanel))
                        {
                            Canvas.SetLeft(lblNamePopup, (elementToDisplayAgainst.Parent as OverlapStackPanel).CLeftItem(elementToDisplayAgainst));
                            if (displayUnder)
                                Canvas.SetTop(lblNamePopup, (elementToDisplayAgainst.Parent as OverlapStackPanel).CTop() + (elementToDisplayAgainst.Parent as OverlapStackPanel).ActualHeight );
                            else
                                Canvas.SetTop(lblNamePopup, (elementToDisplayAgainst.Parent as OverlapStackPanel).CTop() - (block.Text.Count(s => s == '\n') + 1) * 16);
                        }
                        else
                        {
                            Canvas.SetLeft(lblNamePopup, elementToDisplayAgainst.CLeft());
                            if (displayUnder)
                                Canvas.SetTop(lblNamePopup, elementToDisplayAgainst.CTop() + elementToDisplayAgainst.ActualHeight);
                            else
                                Canvas.SetTop(lblNamePopup, elementToDisplayAgainst.CTop() - (block.Text.Count(s => s == '\n') + 1) * 16);
                        }
                        
                  //  });
                //TextBlock block2 = lblNamePopup.Children[0] as TextBlock;
                //block2.LayoutUpdated += lblNamePopupLayoutUpdatedEventHandler;

                
            
        }
        /// <summary>
        /// Target index meaning:
        /// 1 - 5: Monster
        /// 6 - 10: ST
        /// 11: FSpell
        /// 12+: Hand
        /// </summary>
        /// <param name="targetIndex"></param>
        void showTargetBorder(bool isPlayer, int targetIndex)
        {
            FrameworkElement elementToDisplayAgainst = null;
            if (targetIndex.InBetween(1, 5))
            {
                if (isPlayer)
                    elementToDisplayAgainst = BordMon(targetIndex);
                else
                    elementToDisplayAgainst = BordOpMon(targetIndex);

            }
            else if (targetIndex.InBetween(6, 10))
            {
                if (isPlayer)
                    elementToDisplayAgainst = BordST(targetIndex - 5);
                else
                    elementToDisplayAgainst = BordOpST(targetIndex - 5);
            }
            else if (targetIndex == 11)
            {
                if (isPlayer)
                    elementToDisplayAgainst = BordFSpell;
                else
                    elementToDisplayAgainst = BordOpFSpell;
            }
            else //hand
            {
                if (isPlayer)
                {
                    if (targetIndex - 11 > staHand.Children.Count) return;
                    elementToDisplayAgainst = staHand.Children[targetIndex - 12] as FrameworkElement;
                }
                else
                {
                    if (targetIndex - 11 > staOpHand.Children.Count) return;
                    elementToDisplayAgainst = staOpHand.Children[targetIndex - 12] as FrameworkElement;
                }
            }

            bordTarget.Visibility = System.Windows.Visibility.Visible;
            if (targetIndex > 11)
            {
                Canvas.SetLeft(bordTarget, isPlayer ? staHand.CLeftItem(elementToDisplayAgainst) : staOpHand.CLeftItem(elementToDisplayAgainst));
                Canvas.SetTop(bordTarget, elementToDisplayAgainst.CTop());
            }
            else
            {
                Canvas.SetLeft(bordTarget, elementToDisplayAgainst.CLeft());
                Canvas.SetTop(bordTarget, elementToDisplayAgainst.CTop());
            }

            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.From = 0; fadeAnimation.To = 1;
            fadeAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
            fadeAnimation.AutoReverse = true;
            fadeAnimation.RepeatBehavior = new RepeatBehavior(2);
            fadeAnimation.Completed += delegate
            {
                bordTarget.Visibility = System.Windows.Visibility.Collapsed;
            };
            bordTarget.BeginAnimation(Border.OpacityProperty, fadeAnimation);

            if (!isPlayer)
            {
                SummarizeJabber(duelEvent: DuelEvents.Target, Text: targetIndex.ToString());
            }
        }
 
        
        /// <summary>
        /// NOTE: HANDLER IS ONLY ADDED WHEN GOING TO ATTACK!!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LayoutRoot_MouseMove_Attack(object sender, MouseEventArgs e)
        {
            double xPos = (imgBattleOrigin.RenderTransform as CompositeTransform).TranslateX + imgBattleOrigin.CLeft() + (imgBattleOrigin.Width / 2);
            double yPos = (imgBattleOrigin.RenderTransform as CompositeTransform).TranslateY + imgBattleOrigin.CTop() + (imgBattleOrigin.Height / 2);

            Point ePoint = e.GetPosition(LayoutRoot);
 
            (imgBattleOrigin.RenderTransform as CompositeTransform).Rotation = GetRotationAngle(new Point(xPos, yPos), ePoint);
        }

        private double GetRotationAngle(Point origin, Point target)
        {
            double theta = Math.Atan(
                (target.X - origin.X) / (origin.Y - target.Y)
                ) * 180 / Math.PI;
           
            if (target.Y > origin.Y) //Mouse is under image
                theta = theta - 180;
            return theta;
        }
        private void ctxtHand_Item_Clicked(string itemText, int contextIndex)
        {
            int emptyZone = 0; CardDetails stats = null; animationBundle bundle = null;
            HideAllContext();
            HideAllLabelike();
            switch (itemText)
            {
                case "Summon / Activate":
                    if (M.PlayerHand[contextIndex].IsMonster())
                    {
                        emptyZone = FindEmptyBordMon();
                        if (emptyZone == 0) return; //All zones full
                        stats = MoveStats(Area.Hand, Area.Monster, contextIndex, emptyZone);
                        bundle = createBundle(Area.Hand, Area.Monster, true, contextIndex, emptyZone);
                        Animate(bundle, stats, DuelEvents.Play);
                        SummarizeJabber(Area1: Area.Monster, Index1: emptyZone, Stat1: DuelNumeric.NumHand,
                            duelEvent: DuelEvents.Play, Text: M.PlayerMonsters[emptyZone].Name
                            , bundle: bundle);
                    }
                    else if (M.PlayerHand[contextIndex].Type == "Field")
                    {
                        if (M.PlayerFSpell.Name != null)
                            DestroyFieldSpell();
                        stats = MoveStats(Area.Hand, Area.FieldSpell, contextIndex);
                        bundle = createBundle(Area.Hand, Area.FieldSpell, true, contextIndex);
                        Animate(bundle, stats, DuelEvents.Play);
                        SummarizeJabber(Area1: Area.FieldSpell, Index1: emptyZone, Stat1: DuelNumeric.NumHand,
                            duelEvent: DuelEvents.Play, Text: M.PlayerFSpell.Name
                            , bundle: bundle);
                    }
                    else
                    {
                        emptyZone = FindEmptyBordST();
                        if (emptyZone == 0) return; //All zones full
                        stats = MoveStats(Area.Hand, Area.ST, contextIndex, emptyZone);
                        bundle = createBundle(Area.Hand, Area.ST, true, contextIndex, emptyZone);
                        Animate(bundle, stats, DuelEvents.Play);
                        SummarizeJabber(Area1: Area.ST, Index1: emptyZone, Stat1: DuelNumeric.NumHand,
                            duelEvent: DuelEvents.Play, Text: M.PlayerST[emptyZone].Name
                            , bundle: bundle);
                    }
                    break;



                case "Set (Monster)":
                    emptyZone = FindEmptyBordMon();
                    if (emptyZone == 0) return; //All zones full
                    stats = MoveStats(Area.Hand, Area.Monster, contextIndex, emptyZone);
                    stats.Facedown = true; stats.IsItHorizontal = true;
                    M.copyCardDetails(ref M.PlayerMonsters[emptyZone], stats); //Need to copy again, stats is a new instance, won't change monsters
                    bundle = createBundle(Area.Hand, Area.Monster, true, contextIndex, emptyZone);
                    Animate(bundle, stats, DuelEvents.Set);
                    SummarizeJabber(Area1: Area.Monster, Index1: emptyZone, Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Set
                        , bundle: bundle);
                    break;
                case "Set (S/T)":
                    emptyZone = FindEmptyBordST();
                    if (emptyZone == 0) return; //All zones full
                    stats = MoveStats(Area.Hand, Area.ST, contextIndex, emptyZone);
                    stats.Facedown = true;
                    M.copyCardDetails(ref M.PlayerST[emptyZone], stats);
                    bundle = createBundle(Area.Hand, Area.ST, true, contextIndex, emptyZone);
                    Animate(bundle, stats, DuelEvents.Set);
                    SummarizeJabber(Area1: Area.ST, Index1: emptyZone, Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Set
                        , bundle: bundle);
                    break;
                case "Discard":
                    stats = MoveStats(Area.Hand, Area.Grave, contextIndex);
                    bundle = createBundle(Area.Hand, Area.Grave, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Hand_To_Grave);
                    SummarizeJabber(Area1: Area.Grave, Index1: M.PlayerGrave.CountNumCards(),
                        Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Hand_To_Grave, Text: M.PlayerGrave[M.PlayerGrave.CountNumCards()].Name,
                        bundle: bundle);
                    break;
                case "Discard at Random":
                    contextIndex = M.Rand(1, M.PlayerHand.CountNumCards(), new Random());
                    stats = MoveStats(Area.Hand, Area.Grave, contextIndex);
                    bundle = createBundle(Area.Hand, Area.Grave, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Hand_To_Grave_Random);
                    SummarizeJabber(Area1: Area.Grave, Index1: M.PlayerGrave.CountNumCards(),
                        Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Hand_To_Grave_Random, Text: M.PlayerGrave[M.PlayerGrave.CountNumCards()].Name,
                        bundle: bundle);
                    break;
                case "Discard All":
                    for (int n = M.PlayerHand.CountNumCards(); n >= 1; n--)
                    {
                        stats = MoveStats(Area.Hand, Area.Grave, n);
                        bundle = createBundle(Area.Hand, Area.Grave, true, n);
                        Animate(bundle, stats, DuelEvents.Hand_To_Grave_Random);
                        SummarizeJabber(Area1: Area.Grave, Index1: M.PlayerGrave.CountNumCards(),
                            Stat1: DuelNumeric.NumHand,
                            bundle: bundle);
                    }
                    SummarizeJabber(duelEvent: DuelEvents.Hand_To_Grave_All);
                    break;
                case "Banish":
                    stats = MoveStats(Area.Hand, Area.RFG, contextIndex);
                    bundle = createBundle(Area.Hand, Area.RFG, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Hand_To_RFG);
                    SummarizeJabber(Area1: Area.RFG, Index1: M.PlayerRFG.CountNumCards(),
                        Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Hand_To_RFG, Text: M.PlayerRFG[M.PlayerRFG.CountNumCards()].Name,
                        bundle: bundle);
                    break;
                case "Banish Facedown":
                    stats = MoveStats(Area.Hand, Area.RFG, contextIndex);
                    stats.Facedown = true;
                    M.copyCardDetails(M.PlayerRFG[M.PlayerRFG.CountNumCards()], stats);
                    bundle = createBundle(Area.Hand, Area.RFG, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Hand_To_RFG);
                    SummarizeJabber(Area1: Area.RFG, Index1: M.PlayerRFG.CountNumCards(),
                        Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Hand_To_RFG, 
                        bundle: bundle);
                    break;
                case "Reveal Card":
                    SummarizeJabber(Area.Hand, Index1: contextIndex,
                        duelEvent: DuelEvents.Hand_Reveal, Text: M.PlayerHand[contextIndex].Name);
                    break;
                case "Reveal All":
                   // System.Text.StringBuilder handBld = new System.Text.StringBuilder(" ");
                    //for (int x = 1; x <= M.PlayerHand.CountNumCards(); x++)
                     //   handBld.Append(M.PlayerHand[x].Name + ", ");

                    SummarizeJabber(Area.Hand,
                        duelEvent: DuelEvents.Hand_Reveal_All/*, Text: handBld.ToString().Substring(0, handBld.Length - 2)*/);
                    break;
                case "To Top of Deck":
                    stats = MoveStats(Area.Hand, Area.Deck, contextIndex);
                    bundle = createBundle(Area.Hand, Area.Deck, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Hand_To_Top);
                    SummarizeJabber(
                        Stat1: DuelNumeric.NumHand, Stat2: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Hand_To_Top,
                        bundle: bundle);
                    break;
                case "To Bottom of Deck":
                    stats = MoveStats(Area.Hand, Area.Deck, contextIndex, 1);
                    bundle = createBundle(Area.Hand, Area.Deck, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Hand_To_Bottom);
                    SummarizeJabber(
                        Stat1: DuelNumeric.NumHand, Stat2: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Hand_To_Bottom,
                        bundle: bundle);
                    break;
            }
        }
        private void ctxtDeck_Item_Clicked(string itemText, int contextIndex)
        {
            CardDetails stats = null; animationBundle bundle = null;
            HideAllContext();
            HideAllLabelike();
            switch (itemText)
            {
                case "View":
                    SummarizeJabber(duelEvent: DuelEvents.Deck_View);
                    showViewForm(Area.Deck, true);
                    break;
                case "Shuffle":
                    M.Shuffle();
                    SummarizeJabber(duelEvent: DuelEvents.Shuffle);
                    break;
                case "Opponent Draw":
                    if (M.PlayerDeck.CountNumCards() == 0) break;
                    stats = M.PlayerDeck[M.PlayerDeck.CountNumCards()];
                    M.PlayerDeck.RemoveAt(M.PlayerDeck.CountNumCards());
                    bundle = new animationBundle();
                    bundle.isPlayer = true;
                    bundle.specialAnimation = SpecialAnimation.OpponentDrawFromPlayer;
                    Animate(bundle, null, DuelEvents.Opponent_Draw_From_Player);
                   
                    SummarizeJabber(Area1: Area.Special, SpecialStat1: stats, Stat1: DuelNumeric.NumDeck, duelEvent: DuelEvents.Opponent_Draw_From_Player, bundle: bundle);
                    
                    break;
                case "Mill":
                    if (M.PlayerDeck.CountNumCards() == 0) break;
                    stats = MoveStats(Area.Deck, Area.Grave, M.PlayerDeck.CountNumCards());
                    bundle = createBundle(Area.Deck, Area.Grave, true);
                    Animate(bundle, stats, DuelEvents.Deck_To_Grave_Mill);
                    SummarizeJabber(Area1: Area.Grave, Index1: M.PlayerGrave.CountNumCards(),
                        Stat1: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Deck_To_Grave_Mill, Text: stats.Name, bundle: bundle);
                    break;
                case "Banish Top":
                    if (M.PlayerDeck.CountNumCards() == 0) break;
                    stats = MoveStats(Area.Deck, Area.RFG, M.PlayerDeck.CountNumCards());
                    bundle = createBundle(Area.Deck, Area.RFG, true);
                    Animate(bundle, stats, DuelEvents.Deck_To_RFG_Mill);
                    SummarizeJabber(Area1: Area.RFG, Index1: M.PlayerRFG.CountNumCards(),
                        Stat1: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Deck_To_RFG_Mill, Text: stats.Name, bundle: bundle);
                    break;
                case "Offer Rematch":
                    System.Windows.MessageBoxResult answer = MessageBox.Show("Are you sure you want to offer a rematch?", "Rematch", MessageBoxButton.OKCancel);
                    if (answer == MessageBoxResult.OK)
                    {
                        SummarizeJabber(duelEvent: DuelEvents.Offer_Rematch);
                    }
                    break;
            }
        }
        private void ctxtMonster_Item_Clicked(string itemText, int contextIndex)
        {
            CardDetails stats = null; animationBundle bundle = null; bool wasFacedown;
            HideAllContext();
            HideAllLabelike();
            switch (itemText)
            {
                case "Send to Grave":
                    detachAllMaterial(contextIndex, true);
                    if (HandleTokenDestroying(contextIndex)) return;
                    stats = MoveStats(Area.Monster, Area.Grave, contextIndex);
                    bundle = createBundle(Area.Monster, Area.Grave, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Monster_To_Grave);
                    SummarizeJabber(Area1: Area.Monster, Index1: contextIndex,
                       Area2: Area.Grave, Index2: M.PlayerGrave.CountNumCards(),
                       duelEvent: DuelEvents.Monster_To_Grave, Text: stats.Name,
                       bundle: bundle);
                    break;
                case "Return to Hand":
                    detachAllMaterial(contextIndex, true);
                    if (HandleTokenDestroying(contextIndex)) return;
                    wasFacedown = M.PlayerMonsters[contextIndex].Facedown;
                    stats = MoveStats(Area.Monster, Area.Hand, contextIndex);
                    bundle = createBundle(Area.Monster, Area.Hand, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Bounce);
                    SummarizeJabber(Area1: Area.Monster, Index1: contextIndex,
                        Stat1: DuelNumeric.NumHand,
                        duelEvent: DuelEvents.Bounce, Text: wasFacedown ? null : stats.Name,
                       bundle: bundle);
                    break;
                case "Banish":
                    detachAllMaterial(contextIndex, true);
                    if (HandleTokenDestroying(contextIndex)) return;
                    stats = MoveStats(Area.Monster, Area.RFG, contextIndex);
                    bundle = createBundle(Area.Monster, Area.RFG, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Monster_To_RFG);
                    SummarizeJabber(Area1: Area.Monster, Index1: contextIndex,
                       Area2: Area.RFG, Index2: M.PlayerRFG.CountNumCards(),
                       duelEvent: DuelEvents.Monster_To_RFG, Text: stats.Name,
                       bundle: bundle);
                    break;
                case "Switch Position":
                    M.PlayerMonsters[contextIndex].IsItHorizontal = !M.PlayerMonsters[contextIndex].IsItHorizontal;
                    M.PlayerMonsters[contextIndex].Facedown = false;
                    UpdatePictureBoxDuelField(BordMon(contextIndex), M.PlayerMonsters[contextIndex], M.mySet);
                    SummarizeJabber(Area.Monster, Index1: contextIndex,
                        duelEvent: DuelEvents.Switch_Position, Text: M.PlayerMonsters[contextIndex].Name);
                    break;
                case "Flip":
                    M.PlayerMonsters[contextIndex].Facedown = !M.PlayerMonsters[contextIndex].Facedown;
                    M.PlayerMonsters[contextIndex].IsItHorizontal = true;
                    UpdatePictureBoxDuelField(BordMon(contextIndex), M.PlayerMonsters[contextIndex], M.mySet);
                    SummarizeJabber(Area.Monster, Index1: contextIndex,
                        duelEvent: DuelEvents.Switch_Position, Text: M.PlayerMonsters[contextIndex].Name);
                   break;
                case "To Top of Deck":
                   detachAllMaterial(contextIndex, true);
                   if (HandleTokenDestroying(contextIndex)) return;
                   wasFacedown = M.PlayerMonsters[contextIndex].Facedown;
                    stats = MoveStats(Area.Monster, Area.Deck, contextIndex);
                    bundle = createBundle(Area.Monster, Area.Deck, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Spin);
                    SummarizeJabber(Area1: Area.Monster, Index1: contextIndex,
                        Stat1: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Spin, Text: wasFacedown ? null : stats.Name,
                       bundle: bundle);
                    break;
                case "To Bottom of Deck":
                    detachAllMaterial(contextIndex, true);
                    if (HandleTokenDestroying(contextIndex)) return;
                    wasFacedown = M.PlayerMonsters[contextIndex].Facedown;
                    stats = MoveStats(Area.Monster, Area.Deck, contextIndex, 1);
                    bundle = createBundle(Area.Monster, Area.Deck,true, contextIndex, 1);
                    Animate(bundle, stats, DuelEvents.Bottom_Return);
                    SummarizeJabber(Area1: Area.Monster, Index1: contextIndex,
                        Stat1: DuelNumeric.NumDeck,
                        duelEvent: DuelEvents.Bottom_Return,  Text: wasFacedown ? null : stats.Name,
                       bundle: bundle);
                    break;
                case "To Extra Deck":
                    detachAllMaterial(contextIndex, true);
                    if (HandleTokenDestroying(contextIndex)) return;
                    wasFacedown = M.PlayerMonsters[contextIndex].Facedown;
                    stats = MoveStats(Area.Monster, Area.Extra, contextIndex);
                    bundle = createBundle(Area.Monster, Area.Extra, true, contextIndex);
                    Animate(bundle, stats, DuelEvents.Field_To_Extra);
                    SummarizeJabber(Area1: Area.Monster, Index1: contextIndex,
                        Stat1: DuelNumeric.NumEDeck,
                        duelEvent: DuelEvents.Field_To_Extra, Text: wasFacedown ? null : stats.Name,
                       bundle: bundle);
                    break;
                case "Change Control":
                    if (ZoneofSwitch != -1)
                        MessageBox.Show("Still waiting for opponent's response.");
                    else
                    {
                        ZoneofSwitch = contextIndex;
                        SummarizeJabber(duelEvent: M.PlayerMonsters[contextIndex].OpponentOwned ? DuelEvents.Give_Back_Control : DuelEvents.Give_Control, Text: contextIndex.ToString());
                    }
                    break;
                case "Attack...":
                    showLabelikeImage(imgBattleOrigin, BordMon(contextIndex));
                    goingToAttackZone = contextIndex;
                    LayoutRoot.MouseMove += LayoutRoot_MouseMove_Attack;
                    break;
                case "Move to Zone...":
                    if (goingToMoveZone == 0)
                    {
                        showLabelikeImage(imgMoveOrigin, BordMon(contextIndex));
                        goingToMoveZone = contextIndex;
                    }
                    break;
            }
        }
        private void ctxtSpellTrap_Item_Clicked(string itemText, int contextIndex)
        {
            CardDetails stats = null; animationBundle bundle = null; bool wasFacedown;
            HideAllContext();
            HideAllLabelike();
            switch (itemText)
            {
                case "Activate":
                    if (contextIndex == -1) //Field Spell
                    {
                        M.PlayerFSpell.Facedown = false;
                        UpdatePictureBoxDuelField(BordFSpell, M.PlayerFSpell, M.mySet);
                        SummarizeJabber(Area1: Area.FieldSpell, Index1: 1, Element1: StatElement.Facedown,
                            duelEvent: DuelEvents.ST_Activate, Text: M.PlayerFSpell.Name);
                    }
                    else
                    {
                        M.PlayerST[contextIndex].Facedown = false;
                        UpdatePictureBoxDuelField(BordST(contextIndex), M.PlayerST[contextIndex], M.mySet);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex, Element1: StatElement.Facedown,
                            duelEvent: DuelEvents.ST_Activate, Text: M.PlayerST[contextIndex].Name);
                    }
                    break;
                case "Send to Grave":
                    if (contextIndex == -1) //Field Spell
                    {
                        if (HandleTokenDestroying(11)) return;
                        DestroyFieldSpell();
                    }
                    else
                    {
                        if (HandleTokenDestroying(contextIndex + 5)) return;
                        stats = MoveStats(Area.ST, Area.Grave, contextIndex);
                        bundle = createBundle(Area.ST, Area.Grave,true, contextIndex);
                        Animate(bundle, stats, DuelEvents.ST_To_Grave);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex,
                            Area2: Area.Grave, Index2: M.PlayerGrave.CountNumCards(),
                            duelEvent: DuelEvents.ST_To_Grave, Text: stats.Name,
                            bundle: bundle);
                    }
                    break;
                case "Return to Hand":
                    if (contextIndex == -1) //Field Spell
                    {
                        if (HandleTokenDestroying(11)) return;
                        wasFacedown = M.PlayerFSpell.Facedown;
                        stats = MoveStats(Area.FieldSpell, Area.Hand, 1);
                        bundle = createBundle(Area.FieldSpell, Area.Hand, true, 1);
                        Animate(bundle, stats, DuelEvents.Bounce);
                        SummarizeJabber(Area1: Area.FieldSpell,
                            Stat1: DuelNumeric.NumHand,
                            duelEvent: DuelEvents.Bounce, Text: wasFacedown ? null : stats.Name,
                            bundle: bundle);
                    }
                    else
                    {
                        if (HandleTokenDestroying(contextIndex + 5)) return;
                        wasFacedown = M.PlayerST[contextIndex].Facedown;
                        stats = MoveStats(Area.ST, Area.Hand, contextIndex);
                        bundle = createBundle(Area.ST, Area.Hand, true, contextIndex);
                        Animate(bundle, stats, DuelEvents.Bounce);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex,
                            Stat1: DuelNumeric.NumHand,
                            duelEvent: DuelEvents.Bounce, Text: wasFacedown ? null : stats.Name,
                            bundle: bundle);
                    }
                    break;
                case "Banish":
                    if (contextIndex == -1) //Field Spell
                    {
                        if (HandleTokenDestroying(11)) return;
                        stats = MoveStats(Area.FieldSpell, Area.RFG, 1);
                        bundle = createBundle(Area.FieldSpell, Area.RFG, true);
                        Animate(bundle, stats, DuelEvents.ST_To_RFG);
                        SummarizeJabber(Area1: Area.FieldSpell,
                            Area2: Area.RFG, Index2: M.PlayerRFG.CountNumCards(),
                            duelEvent: DuelEvents.ST_To_RFG, Text: stats.Name,
                            bundle: bundle);
                    }
                    else
                    {
                        if (HandleTokenDestroying(contextIndex + 5)) return;
                        stats = MoveStats(Area.ST, Area.RFG, contextIndex);
                        bundle = createBundle(Area.ST, Area.RFG, true, contextIndex);
                        Animate(bundle, stats, DuelEvents.ST_To_RFG);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex,
                            Area2: Area.RFG, Index2: M.PlayerRFG.CountNumCards(),
                            duelEvent: DuelEvents.ST_To_RFG, Text: stats.Name,
                            bundle: bundle);
                    }
                    break;
                case "Flip Face-Down":
                    if (contextIndex == -1) //Field Spell
                    {
                        M.PlayerFSpell.Facedown = true;
                        UpdatePictureBoxDuelField(BordFSpell, M.PlayerFSpell, M.mySet);
                        SummarizeJabber(Area1: Area.FieldSpell, Index1: 1, Element1: StatElement.Facedown,
                            duelEvent: DuelEvents.ST_Activate, Text: M.PlayerFSpell.Name);
                    }
                    else
                    {
                        M.PlayerST[contextIndex].Facedown = true;
                        UpdatePictureBoxDuelField(BordST(contextIndex), M.PlayerST[contextIndex], M.mySet);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex, Element1: StatElement.Facedown,
                            duelEvent: DuelEvents.ST_Activate, Text: M.PlayerST[contextIndex].Name);
                    }
                    break;
                case "To Top of Deck":
                    if (contextIndex == -1) //Field Spell
                    {
                        if (HandleTokenDestroying(11)) return;
                        wasFacedown = M.PlayerFSpell.Facedown;
                        stats = MoveStats(Area.FieldSpell, Area.Deck, 1);
                        bundle = createBundle(Area.FieldSpell, Area.Deck, true);
                        Animate(bundle, stats, DuelEvents.Spin);
                        SummarizeJabber(Area1: Area.FieldSpell,
                           Stat1: DuelNumeric.NumDeck,
                            duelEvent: DuelEvents.Spin, Text: wasFacedown ? null : stats.Name,
                            bundle: bundle);
                    }
                    else
                    {
                        if (HandleTokenDestroying(contextIndex + 5)) return;
                        wasFacedown = M.PlayerST[contextIndex].Facedown;
                        stats = MoveStats(Area.ST, Area.Deck, contextIndex);
                        bundle = createBundle(Area.ST, Area.Deck, true, contextIndex);
                        Animate(bundle, stats, DuelEvents.Spin);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex,
                            Stat1: DuelNumeric.NumDeck,
                            duelEvent: DuelEvents.Spin, Text: wasFacedown ? null : stats.Name,
                            bundle: bundle);
                    }
                    break;
                case "To Bottom of Deck":
                    if (contextIndex == -1) //Field Spell
                    {
                        if (HandleTokenDestroying(11)) return;
                        wasFacedown = M.PlayerFSpell.Facedown;
                        stats = MoveStats(Area.FieldSpell, Area.Deck, 1, 1);
                        bundle = createBundle(Area.FieldSpell, Area.Deck, true, 1, 1);
                        Animate(bundle, stats, DuelEvents.Bottom_Return);
                        SummarizeJabber(Area1: Area.FieldSpell,
                           Stat1: DuelNumeric.NumDeck,
                            duelEvent: DuelEvents.Bottom_Return, Text: wasFacedown ? null : stats.Name,
                            bundle: bundle);
                    }
                    else
                    {
                        if (HandleTokenDestroying(contextIndex + 5)) return;
                        wasFacedown = M.PlayerST[contextIndex].Facedown;
                        stats = MoveStats(Area.ST, Area.Deck, contextIndex, 1);
                        bundle = createBundle(Area.ST, Area.Deck, true, contextIndex, 1);
                        Animate(bundle, stats, DuelEvents.Bottom_Return);
                        SummarizeJabber(Area1: Area.ST, Index1: contextIndex,
                            Stat1: DuelNumeric.NumDeck,
                            duelEvent: DuelEvents.Bottom_Return, Text: wasFacedown ? null : stats.Name,
                            bundle: bundle);
                    }
                    break;
                case "Change Control":
                    if (ZoneofSwitch != -1)
                        MessageBox.Show("Still waiting for opponent's response.");
                    else
                    {
                        if (contextIndex == -1)
                        {
                            ZoneofSwitch = 11;
                            SummarizeJabber(duelEvent: M.PlayerFSpell.OpponentOwned ? DuelEvents.Give_Back_Control : DuelEvents.Give_Control, Text: ZoneofSwitch.ToString());
                        }
                        else
                        {
                            ZoneofSwitch = contextIndex + 5;
                            SummarizeJabber(duelEvent: M.PlayerST[contextIndex].OpponentOwned ? DuelEvents.Give_Back_Control : DuelEvents.Give_Control, Text: ZoneofSwitch.ToString());

                        }
                    }
                    break;
                case "Move to Zone...":
                    HideAllLabelike();
                    if (goingToMoveZone == 0)
                    {
                        if (contextIndex == -1)
                        {
                            showLabelikeImage(imgMoveOrigin, BordFSpell);
                            goingToMoveZone = 11;
                        }
                        else
                        {
                            showLabelikeImage(imgMoveOrigin, BordST(contextIndex));
                            goingToMoveZone = contextIndex + 5;
                        }
                    }
                    break;
            }
        }
        private void ctxtMonsterEmpty_Item_Clicked(string itemText, int contextIndex)
        {
            HideAllContext();
            HideAllLabelike();
            switch (itemText)
            {
                case "Summon Token":
                    M.PlayerMonsters[contextIndex].Name = "Token";
                    M.PlayerMonsters[contextIndex].Type = "Token";
                    M.PlayerMonsters[contextIndex].Attribute = "Light";
                    M.PlayerMonsters[contextIndex].Level = 1; 
                    M.PlayerMonsters[contextIndex].Lore = "This is a token.";
                    M.PlayerMonsters[contextIndex].IsItHorizontal = true;
                    M.PlayerMonsters[contextIndex].ID = TOKEN_ID;
                    Image image = new Image();
                    image.Width = 55;
                    image.Height = 81;
                    BordMon(contextIndex).Child = image;
                    UpdatePictureBoxDuelField(BordMon(contextIndex), M.PlayerMonsters[contextIndex], M.mySet);
                    SummarizeJabber(Area.Monster, Index1: contextIndex,
                        duelEvent: DuelEvents.Summon_Token);
                    break;
            }
        }
        private void ctxtXyz_Item_ClickedXyz(string itemText, int contextIndex, int xyzIndex)
        {
            HideAllContext();
            HideAllLabelike();
            switch (itemText)
            {
                case "Detach":
                    CardDetails stats = MoveStats(Area.Xyz, Area.Grave, contextIndex, fromIndexXyz: xyzIndex);
                    animationBundle bundle = createBundle(Area.Xyz, Area.Grave, true, contextIndex, fromIndexXyz: xyzIndex);
                    Animate(bundle, stats, DuelEvents.Material_To_Grave);
                    SummarizeJabber(Area1: Area.Xyz, Index1: contextIndex,
                        Area2: Area.Grave, Index2: M.PlayerGrave.CountNumCards(),
                        duelEvent: DuelEvents.Material_To_Grave, Text: M.PlayerMonsters[contextIndex].Name,
                        bundle: bundle);
                    break;
            }
        }

        void resetStatDisplay()
        {
            lblName.Text = "";
            lblLore.Text = "";
            for (int n = 1; n <= 12; n++)
            {
                M.setImage(ImgStars(n), BLANK_IMAGE, UriKind.Relative);
            }
            M.setImage(imgAttribute, BLANK_IMAGE, UriKind.Relative);
            lblType.Text = "";
         
            lblATKplaceholder.Visibility = System.Windows.Visibility.Collapsed;
            lblDEFplaceholder.Visibility = System.Windows.Visibility.Collapsed;
            txtATK.Visibility = System.Windows.Visibility.Collapsed;
            txtDEF.Visibility = System.Windows.Visibility.Collapsed;
            cmdEditStats.Visibility = System.Windows.Visibility.Collapsed;
            txtCounters.Text = "0";
        }
        private void HideAllContext()
        {
            ctxtDeck.Visibility = System.Windows.Visibility.Collapsed;
            ctxtHand.Visibility = System.Windows.Visibility.Collapsed;
            ctxtMonster.Visibility = System.Windows.Visibility.Collapsed;
            ctxtSpellTrap.Visibility = System.Windows.Visibility.Collapsed;
            ctxtMonsterEmpty.Visibility = System.Windows.Visibility.Collapsed;
            ctxtXyz.Visibility = System.Windows.Visibility.Collapsed;
            lblNamePopup.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void HideAllLabelike()
        {
            imgMoveOrigin.Visibility = System.Windows.Visibility.Collapsed;
            imgMoveDestination.Visibility = System.Windows.Visibility.Collapsed;
            imgBattleOrigin.Visibility = System.Windows.Visibility.Collapsed;
            goingToMoveZone = 0;
            goingToAttackZone = 0;
            LayoutRoot.MouseMove -= LayoutRoot_MouseMove_Attack;
            
        }

        bool HandleTokenDestroying(int globalZone)
        {

            CardDetails stats = PlayerCurrentField(globalZone);
            if (stats.ID == TOKEN_ID)
            {
                M.setAsNothing(stats);
                if (imageDragArea != Area.None) resetImageDrag();

                UpdatePictureBoxDuelField(BordField(globalZone), null, null);
                SummarizeJabber(Area1: getAreaFromZone(globalZone), Index1: getIndexFromZone(getAreaFromZone(globalZone), globalZone),
                    duelEvent: DuelEvents.Destroy_Token);
                return true;
            }
            else
                return false;
        }

        private void OpponentAvatar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (goingToAttackZone > 0)
            {
                attack(goingToAttackZone, 6, true, e.GetPosition(LayoutRoot)); //6 is Direct attack.
            }
        }

        private void cmdEditStats_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.Assert(ZoneofEdit > 0);

            if (cmdEditStats.Content.ToString() == "Edit")
                changeEditMode(true);
            else
            {
                if (txtATK.Visibility == Visibility.Visible && M.isNumeric(txtATK.Text) && Convert.ToInt32(txtATK.Text) != PlayerCurrentField(ZoneofEdit).ATK)
                {
                    PlayerCurrentField(ZoneofEdit).ATK = Convert.ToInt32(txtATK.Text);
                    SummarizeJabber(Area1: getAreaFromZone(ZoneofEdit), Index1: getIndexFromZone(getAreaFromZone(ZoneofEdit), ZoneofEdit), Element1: StatElement.ATK,
                        duelEvent: DuelEvents.ATK_Change, Text: PlayerCurrentField(ZoneofEdit).Name);
                }
                if (txtDEF.Visibility == Visibility.Visible && M.isNumeric(txtDEF.Text) && Convert.ToInt32(txtDEF.Text) != PlayerCurrentField(ZoneofEdit).DEF)
                {
                    PlayerCurrentField(ZoneofEdit).DEF = Convert.ToInt32(txtDEF.Text);
                    SummarizeJabber(Area1: getAreaFromZone(ZoneofEdit), Index1: getIndexFromZone(getAreaFromZone(ZoneofEdit), ZoneofEdit), Element1: StatElement.DEF,
                        duelEvent: DuelEvents.DEF_Change, Text: PlayerCurrentField(ZoneofEdit).Name);
                }
                if (M.isNumeric(txtCounters.Text) && Convert.ToSByte(txtCounters.Text) != PlayerCurrentField(ZoneofEdit).Counters)
                {
                    PlayerCurrentField(ZoneofEdit).Counters = Convert.ToSByte(txtCounters.Text);
                    SummarizeJabber(Area1: getAreaFromZone(ZoneofEdit), Index1: getIndexFromZone(getAreaFromZone(ZoneofEdit), ZoneofEdit), Element1: StatElement.Counters,
                        duelEvent: DuelEvents.Change_Counter, Text: ZoneofEdit.ToString());
                }
                changeEditMode(false);
            }
            
        }

        private void imgWatch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!M.IamWatching)
                addMessage("Use the box under the watch to change the turn count.", false);
        }

        private void txtSendText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                cmdSendText_Click(null, null);
        }

        private void Avatar_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Image senderImage = sender as Image;
            if (senderImage == null) return;
            M.setImage(senderImage, "blankavatar.jpg", UriKind.Relative);
        }






    }


    public class OverlapStackPanel : StackPanel, System.ComponentModel.INotifyPropertyChanged
    {
        double currentbuffer = 10;

        public bool WidthExpandable { get; set; }
        public double MaxSuggestedWidth { get; set; }

        public bool PointIsNearControl(Point point)
        {
            return this.CLeft() + (this.Width / 2) - (this.ExpandWidth / 2) - 50 < point.X
                && this.CLeft() + (this.Width / 2) + (this.ExpandWidth / 2) + 50 > point.X
                && this.CTop() < point.Y
                && this.CTop() + this.Height > point.Y;
        }

        private void changebuffer(double pixels)
        {
            if (ExpandWidth != MaxSuggestedWidth) return;
            Width -= currentbuffer;
            Width += pixels;
            currentbuffer = pixels;
        }
        public void OverlapAddPlaceholder(FrameworkElement e)
        {
            e.Opacity = 0;
            OverlapAdd(e);
        }
        public void OverlapAdd(FrameworkElement e)
        {
            this.Children.Add(e);
            onAddChild();
        }
        public void OverlapInsert(int index, FrameworkElement e)
        {
            this.Children.Insert(index, e);
            onAddChild();
        }
        public void OverlapInsertPlaceholder(int index, FrameworkElement e)
        {
            e.Opacity = 0;
            OverlapInsert(index, e);
        }
        private void onAddChild()
        {
            makeRoom(Children.Count);
            bool overflowIsZero = Overflow == 0;
            ExpandWidth = WidthAllItems;
            if (overflowIsZero && Overflow > 0)
            {
                makeRoom(Children.Count);
            }
        }
        public void OverlapRemove(FrameworkElement e)
        {
            this.Children.Remove(e);
            ExpandWidth = WidthAllItems;
            
            makeRoom(Children.Count);
            ExpandWidth = WidthAllItems;
            if (Overflow > 0)
                makeRoom(Children.Count);           
        }

        public void OverlapReplacePlaceholderAt(int index, FrameworkElement e)
        {
            e.Margin = (Children[index] as FrameworkElement).Margin;
            Children[index] = e;
        }
        public void OverlapRemoveAt(int index)
        {
            this.Children.RemoveAt(index);
            ExpandWidth = WidthAllItems;

            makeRoom(Children.Count);
            ExpandWidth = WidthAllItems;
            if (Overflow > 0)
                makeRoom(Children.Count); 
        }
        public double ExpandWidth
        {
            get
            {
                return double.IsNaN(ActualWidth) && ActualWidth > 0 ? ActualWidth - currentbuffer 
                                                                    : Width - currentbuffer;
            }
            set
            {
                if (value > MaxSuggestedWidth)
                {
                    value = MaxSuggestedWidth;
                }
                if (WidthExpandable)
                {
                    Canvas.SetLeft(this, Canvas.GetLeft(this) - ((value + currentbuffer - this.Width) / 2));
                }
                    this.Width = value + currentbuffer;

                /*DoubleAnimation leftAnimation = new DoubleAnimation();
                leftAnimation.From = this.CLeft();
                leftAnimation.To = this.CLeft() - ((value + currentbuffer - this.Width) / 2);
                leftAnimation.EasingFunction = new QuadraticEase();
                leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                DoubleAnimation widthAnimation = new DoubleAnimation();
                widthAnimation.From = ExpandWidth + currentbuffer;
                widthAnimation.To = value + currentbuffer;
                widthAnimation.EasingFunction = new QuadraticEase();
                widthAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                ClassLibrary.AnimationHelp.BeginAnimation(this, Canvas.LeftProperty, leftAnimation);
                ClassLibrary.AnimationHelp.BeginAnimation(this, OverlapStackPanel.WidthProperty, widthAnimation);*/
                
            }

        }

        public int GetIndexOfChild(object child)
        {
            for (int n = 0; n < Children.Count; n++)
            {
                if (object.ReferenceEquals(Children[n], child))
                    return n;
            }
            return -1;
        }
        public double CLeftItem(FrameworkElement child)
        {
            double totalLeft = this.CLeft();
            for (int n = 0; n < Children.Count; ++n)
            {
                if (object.ReferenceEquals(Children[n], child))
                {
                    return totalLeft + child.Margin.Left + child.Margin.Right;
                }
                else
                {
                    totalLeft += trueWidth(Children[n]);
                }
            }
            return totalLeft;
        }
        public double CLeftItem(int index)
        {
            return CLeftItem(Children[index] as FrameworkElement);
        }
        public void Shuffle(DuelFieldNew duelFieldInstance, List<CardDetails> listToShuffle = null, Action onBeforeAnimation = null, Action onCompleted = null)
        {
            
            Random newRand = new Random();
            
            Dictionary<int, int> mappedIndexes = new Dictionary<int, int>();
            List<int> availableIndexes = new List<int>();
            List<CardDetails> tempList = null;
            if (listToShuffle != null)
            {
                tempList = new List<CardDetails>();
                while (listToShuffle.Count > 1)
                { tempList.Add(listToShuffle[1]); listToShuffle.RemoveAt(1); } //Preserve 0 index }

            }
            int numberOfCards = this.Children.Count;
            int currentCount = 0;
            for (int n = 0; n < numberOfCards; n++) availableIndexes.Add(n);

            if (availableIndexes.Count > 1) //Don't shuffle if only 1 !
            {
                 
                while (availableIndexes.Count > 0)
                {
                    int rand = availableIndexes[newRand.Next(0, availableIndexes.Count)];
                    if (rand != currentCount)
                    {
                        mappedIndexes.Add(currentCount, rand);
                        availableIndexes.Remove(rand);
                        currentCount++;
                       
                    }
                    else if (mappedIndexes.Count > 0 ) //Can't be the same as it was, swap with a random previous
                    {
                        int swapIndex = newRand.Next(0, mappedIndexes.Count);
                        mappedIndexes.Add(currentCount, mappedIndexes[swapIndex]); //Swap
                        mappedIndexes[swapIndex] = rand;                           //Swap
                        availableIndexes.Remove(rand);
                        currentCount++;

                    }
                }
            }
            else
                mappedIndexes.Add(0, 0);

            if (listToShuffle != null)
            {
                for (int n = 0; n < numberOfCards; n++)
                {
                    listToShuffle.Add(tempList[mappedIndexes[n]]);
                }
            }


            if (onBeforeAnimation != null) onBeforeAnimation.DynamicInvoke();

            FrameworkElement[] AllChildren = new FrameworkElement[numberOfCards];
            double[] leftsOfChildren = new double[numberOfCards];
            for (int n = 0; n < numberOfCards; n++) leftsOfChildren[n] = this.CLeftItem(n);

            for (int n = numberOfCards - 1; n >= 0; --n)
            {
                FrameworkElement image = this.Children[n] as FrameworkElement;
                AllChildren[mappedIndexes[n]] = image;
                DoubleAnimation leftAnimation = new DoubleAnimation();
                leftAnimation.From = leftsOfChildren[n];
                leftAnimation.To = leftsOfChildren[numberOfCards - n - 1];
                leftAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                this.Children.Remove(image);
                duelFieldInstance.LayoutRoot.Children.Add(image);
                Canvas.SetTop(image, this.CTop());
                if (n == numberOfCards - 1) //Only do ONCE
                {
                    leftAnimation.Completed += delegate
                    {
                        for (int k = 0; k < numberOfCards; k++)
                        {
                            duelFieldInstance.LayoutRoot.Children.Remove(AllChildren[k]);
                            this.Children.Add(AllChildren[k]);                           
                        }
                        makeRoom(Children.Count);
                        if (onCompleted != null) onCompleted.DynamicInvoke();
                        duelFieldInstance.NextAnimation();
                    };
                }
                
                image.BeginAnimation(Canvas.LeftProperty, leftAnimation);
            }


        }

        private double Overflow
        {
            get
            {
                if (Children.Count == 0) return 0;
                if (ExpandWidth < MaxSuggestedWidth) return 0;
                double width = trueWidth(Children[0]);
                return (width * Children.Count) - ExpandWidth;
            }
        }
        private double trueWidth(UIElement e)
        {   
            FrameworkElement f = e as FrameworkElement;
            return double.IsNaN(f.Width) ? f.ActualWidth + f.Margin.Left + f.Margin.Right
                                         : f.Width + f.Margin.Left + f.Margin.Right;
        }
        private void makeRoom(int count)
        {
            for( int n = 0; n < Children.Count; n++)
            {
                (Children[n] as FrameworkElement).Margin = new Thickness(n == 0 ? 0 : TypicalMargin(count),0,0,0);
            }
        }
        private double TypicalMargin(int count)
        {
                return (-1) * Overflow / (count - 1);
        }

        public double WidthAllItems
        {
            get
            {
                double total = 0;
                foreach (FrameworkElement e in Children)
                {
                    total += trueWidth(e);
                }
                return total;
            }
        }


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }

    public static class AnimationHelp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="prop">The property to animate</param>
        /// <param name="animation">Details of the animation, usually a DoubleAnimation</param>
        /// <returns>Returns the storyboard, so an infinite animation can be stopped</returns>
        public static Storyboard BeginAnimation(this DependencyObject e, DependencyProperty prop, Timeline animation)
        {
            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(animation, e);
            Storyboard.SetTargetProperty(animation, new PropertyPath(prop));
            sb.Children.Add(animation);
            sb.Begin();
            return sb;
        }
      
    }
}
