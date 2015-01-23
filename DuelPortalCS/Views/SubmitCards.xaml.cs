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
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace DuelPortalCS.Views
{
    public partial class SubmitCards : Page
    {
        
        public frmEdit editForm = new frmEdit();
        public cldInput inputForm = new cldInput("");
        public cldSimpleDialog publicCardWarningForm = new cldSimpleDialog("You have chosen to make your card public." +
                Environment.NewLine + Environment.NewLine +
                "Once a card is public, it is accessible by anyone, and can be edited by anyone." + Environment.NewLine +
                "You agree to leave this card up to the mercy of the community, and if deemed Overpowered, possibly changed or deleted by them." +
                Environment.NewLine + Environment.NewLine +
                "Are you sure you want to do this?", true);
        public bool useYCMTemplate = false;
        public SubmitCards()
        {
            InitializeComponent();
        }
        //Executes when the user navigates to this page.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            M.CenterGrid(LayoutRoot, this);
            M.ScreenResized += delegate
            {
                M.CenterGrid(LayoutRoot, this);
            };
            changeModes(false);
            ChangeVisibilities(true);
            
            M.EditExistingCardID = 0;
            TextBlock tb = new TextBlock();
            tb.Text = "Generate from YCM or YugiCo";
            tb.TextWrapping = TextWrapping.Wrap;
            cmdYCMURLGenerate.Content = tb;

            TextBlock tb3 = new TextBlock();
            tb3.Text = "Commit Card Changes";
            tb3.TextWrapping = TextWrapping.Wrap;
            cmdCommit.Content = tb3;

            lblYCMorYugico.Content = "YCM: Right-Click and View Image on the Card in the Card Maker. Paste URL here." +
                Environment.NewLine + 
                                     "YugiCo: Go to the main page of the card. Paste URL here.";

            editForm.Closed += editForm_Closed;
            sqlCli = new SQLReference.Service1ConsoleClient();
            sqlCli.InetConnectionCompleted += EndGenerateFromYugiCo;
            sqlCli.submitCardCompleted += sqlCli_EndSubmitCard;
            sqlCli.editCardCompleted += sqlCli_EndEditCard;
            sqlCli.deleteCardCompleted += sqlCli_EndDeleteCard;
            sqlCli.AddOrChangeCardCCGCompleted += sqlCli_AddOrChangeCardCCGCompleted;

            publicCardWarningForm.Closed += new EventHandler(publicCardWarningForm_Closed);
           

        
            if ( !M.isLoggedIn || !M.isDefaultSet)
            {
                chkPublic.IsEnabled = false;
                chkUneditable.IsEnabled = false;
                lblFlavorHelper.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                chkPublic.IsEnabled = true;
                chkUneditable.IsEnabled = true;
                lblFlavorHelper.Visibility = System.Windows.Visibility.Collapsed;
            }

        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            editForm.Closed -= editForm_Closed;
            publicCardWarningForm.Closed -= publicCardWarningForm_Closed;
           
            if (sqlCli != null)
            {
                sqlCli.InetConnectionCompleted -= EndGenerateFromYugiCo;
                sqlCli.submitCardCompleted -= sqlCli_EndSubmitCard;
                sqlCli.editCardCompleted -= sqlCli_EndEditCard;
                sqlCli.deleteCardCompleted -= sqlCli_EndDeleteCard;
                sqlCli.CloseAsync();
            }
            base.OnNavigatingFrom(e);
        }
        #region "Control Arrays"
        private RadioButton radType(int index)
        {
            RadioButton rad = (RadioButton)LayoutRoot.FindName("radType_" + index);
            return rad;
        }
        private RadioButton radAttrib(int index)
        {
            RadioButton rad = (RadioButton)LayoutRoot.FindName("radAttrib_" + index);
            return rad;
        }
        private RadioButton radSTIcon(int index)
        {
            RadioButton rad = (RadioButton)LayoutRoot.FindName("radSTIcon_" + index);
            return rad;
        }
        private CheckBox chkMonEffects(int index)
        {
                CheckBox chkbox = (CheckBox)LayoutRoot.FindName("chkMonEffects_" + index);
                return chkbox; 
        }
        #endregion

        #region "Images"
        string UnderlyingImageUrl;
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        int loadingFrame = 0;
        EventHandler<SQLReference.saveTempImageCompletedEventArgs> saveTempImageEventHandler;

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cldEditImage imgForm = new cldEditImage();
            imgForm.cardID = M.EditExistingCardID;

            imgForm.Closed += imgForm_Closed;
            imgForm.Show();
        }
        public void UpdatePictureBox(ref Image pBox, SQLReference.CardDetails stats)
        {
            string cardname = stats.Name;

            try
            {
                if (stats.Name == null)
                {
                    M.setImage(pBox, "", UriKind.Relative);
                    return;
                }
                else
                {

                    M.setImage(pBox, M.TypeToImageName(stats.Type), UriKind.Relative);

                    if (M.cardsWithImages.Contains(M.getRealImageName(cardname, stats.ID, M.mySet)))
                    {
                        M.setImage(pBox, M.toPortalURL(cardname, stats.ID, M.mySet), UriKind.Absolute);
                    }
                }
                pBox.Opacity = 1;

            }
            catch
            {

            }
        }
        void UpdateCardImage(string url, bool useYCM, int existingCardID = 0)
        {
            if (url == "Delete")
            {

                M.setImage(imgCardsImage, "NoImage.png", UriKind.Relative);
                UnderlyingImageUrl = "Delete";
            }
            else if (!string.IsNullOrEmpty(url))
            {

                timer.Interval = new TimeSpan(0, 0, 0, 0, 800);
                timer.Tick += delegate
                {
                    loadingFrame++;
                    if (loadingFrame == 4) { loadingFrame = 1; }
                    M.setImage(imgCardsImage, "Loading" + loadingFrame + ".jpg", UriKind.Relative);
                };
                timer.Start();


                saveTempImageEventHandler = (s, res) =>
                {
                    sqlCli.saveTempImageCompleted -= saveTempImageEventHandler;
                    timer.Stop();
                    if (res.Result.Contains("ERROR"))
                    {
                        M.setImage(imgCardsImage, "ImageFailed.jpg", UriKind.Relative);
                        UnderlyingImageUrl = string.Empty;
                    }
                    else
                    {
                        M.setImage(imgCardsImage, res.Result, UriKind.Absolute);
                        UnderlyingImageUrl = res.UserState as string;
                    }
                };
                sqlCli.saveTempImageCompleted += saveTempImageEventHandler;
                if (useYCM && existingCardID > 0)
                {
                    url = M.formatYCM(existingCardID, url);
                    useYCMTemplate = false;
                }
                sqlCli.saveTempImageAsync(url, url);
            }
        }
        void imgForm_Closed(object sender, EventArgs e)
        {
            cldEditImage imgForm = sender as cldEditImage;
            imgForm.Closed -= imgForm_Closed;
            MyMainPage.enableNavigation();
            useYCMTemplate = imgForm.useYCMTemplate;
            UpdateCardImage(imgForm.imageURL, imgForm.useYCMTemplate, imgForm.cardID);
        }
        #endregion


        #region "Third Party Sites"
        private void cmdYCMURLGenerate_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            txtYCMURL.Text = txtYCMURL.Text.Trim();
            string thetext = txtYCMURL.Text;
            if (thetext.Contains("yugico.com") == true)
            {
                BeginGenerateFromYugiCo();
                return;
            }
            if (thetext.Contains("http://") == false)
            {
                MessageBox.Show("Cards need to have a URL!");
                return;
            }
            if (thetext.Contains("yugiohcardmaker.net") && thetext.Contains("name=") == false)
            {
                MessageBox.Show("This YCM card cannot be read. Did you follow the instructions correctly?");
                return;

            }

            if (thetext.Contains("name=") == false)
            {
                MessageBox.Show("This is not a YCM or YugiCo card. Follow the instructions please.");
                return;
            }
            string[] fields = new string[11];
            string cardtype = null;
            string subtype = null;
            int startbar = 0;
            int endbar = 0;


            startbar = thetext.IndexOf("name=") + 5;
            endbar = thetext.IndexOf("&", startbar);
            fields[1] = M.getridofascii(thetext.Substring(startbar, endbar - startbar));

            fields[2] = "0";

            startbar = thetext.IndexOf("set1=") + "set1=".Length;
            endbar = thetext.IndexOf("&", startbar);
            fields[3] = M.getridofascii(thetext.Substring(startbar, endbar - startbar));

            startbar = thetext.IndexOf("cardtype=") +9;
            endbar = thetext.IndexOf("&", startbar);
            cardtype = thetext.Substring(startbar, endbar - startbar);

            switch (cardtype)
            {
                case "Monster":
                    startbar = thetext.IndexOf("subtype=") + 8;
                    endbar = thetext.IndexOf("&", startbar);
                    subtype = thetext.Substring(startbar, endbar - startbar);

                    switch (subtype)
                    {
                        case "normal":
                        case "divine":
                            startbar = thetext.IndexOf("&type=") + "&type=".Length;
                            endbar = thetext.IndexOf("&", startbar);
                            fields[4] = M.getridofascii(thetext.Substring(startbar, endbar - startbar));
                            break;
                        case "effect":
                            startbar = thetext.IndexOf("&type=") + "&type=".Length;
                            endbar = thetext.IndexOf("&", startbar);
                            fields[4] = M.getridofascii(thetext.Substring(startbar, endbar - startbar)) + "/Effect";
                            break;
                        default:
                            startbar = thetext.IndexOf("&type=") + "&type=".Length;
                            endbar = thetext.IndexOf("&", startbar);
                            fields[4] = M.getridofascii(thetext.Substring(startbar, endbar - startbar)) + "/" + subtype.capitalizeFirstLetter() + "/Effect";
                            break;
                    }

                    startbar = thetext.IndexOf("attribute=") +10;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[5] = thetext.Substring(startbar, endbar - startbar);

                    startbar = thetext.IndexOf("level=") +6;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[6] = thetext.Substring(startbar, endbar - startbar);

                    fields[7] = "";

                    startbar = thetext.IndexOf("atk=") +4;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[8] = thetext.Substring(startbar, endbar - startbar);

                    startbar = thetext.IndexOf("def=") +4;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[9] = thetext.Substring(startbar, endbar - startbar);

                    break;
                case "Fusion":
                case "Synchro":
                case "Ritual":
                case "Xyz":
                    startbar = thetext.IndexOf("subtype=") + 8;
                    endbar = thetext.IndexOf("&", startbar);
                    subtype = thetext.Substring(startbar, endbar - startbar);

                    switch (subtype)
                    {
                        case "normal":
                        case "divine":
                            startbar = thetext.IndexOf("&type=") + "&type=".Length;
                            endbar = thetext.IndexOf("&", startbar);
                            fields[4] = M.getridofascii(thetext.Substring(startbar, endbar - startbar)) + "/" + cardtype;
                            break;
                        case "effect":
                            startbar = thetext.IndexOf("&type=") + "&type=".Length;
                            endbar = thetext.IndexOf("&", startbar);
                            fields[4] = M.getridofascii(thetext.Substring(startbar, endbar - startbar)) + "/" + cardtype + "/Effect";
                            break;
                        default:
                            startbar = thetext.IndexOf("&type=") + "&type=".Length;
                            endbar = thetext.IndexOf("&", startbar);
                            fields[4] = M.getridofascii(thetext.Substring(startbar, endbar - startbar)) + "/" + cardtype + "/" + subtype + "/Effect";
                            break;
                    }

                    startbar = thetext.IndexOf("attribute=") +10;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[5] = thetext.Substring(startbar, endbar - startbar);

                    startbar = thetext.IndexOf("level=") +6;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[6] = thetext.Substring(startbar, endbar - startbar);

                    fields[7] = "";

                    startbar = thetext.IndexOf("atk=") +4;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[8] = thetext.Substring(startbar, endbar - startbar);

                    startbar = thetext.IndexOf("def=") +4;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[9] = thetext.Substring(startbar, endbar - startbar);

                    break;
                case "Spell":
                case "Trap":
                    fields[4] = cardtype + " Card";
                    fields[5] = cardtype;
                    fields[6] = "";

                    startbar = thetext.IndexOf("trapmagictype=") + "trapmagictype=".Length;
                    endbar = thetext.IndexOf("&", startbar);
                    fields[7] = thetext.Substring(startbar, endbar - startbar);

                    fields[8] = "";
                    fields[9] = "";

                    break;
            }

            startbar = thetext.IndexOf("description=") +12;
            endbar = thetext.IndexOf("&", startbar);
            fields[10] = M.getridofascii(thetext.Substring(startbar, endbar - startbar));
            // txtResults.ReadOnly = False
            txtResults.Text = "|";
            for (short n = 1; n <= 10; n++)
            {
                txtResults.Text += fields[n] + "|";
            }

            UpdateCardImage(txtYCMURL.Text, false);
        }
        public void BeginGenerateFromYugiCo()
        {
            this.Cursor = Cursors.Wait;

            string thetext = txtYCMURL.Text;

            sqlCli.InetConnectionAsync(thetext);
            this.Cursor = Cursors.Wait;
        }
        public void EndGenerateFromYugiCo(object sender, SQLReference.InetConnectionCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                MessageBox.Show("The yugico card was not found.");
                Cursor = Cursors.Arrow;
                return;
            }

            string thetext = null;
            thetext = M.ByteArrayToString(e.Result);

            int startofstuff = 0;
            startofstuff = thetext.IndexOf("<div class=" + "\"" + "breadcrumb" + "\"" + ">");
            if (startofstuff == 0)
            {
                this.Cursor = Cursors.Arrow;
                return;
            }

           
            string namestring = null;
            string cardtypestring = null;
            string setstring = "";


            int namebeginning = thetext.IndexOf("<h1>", startofstuff);
            //Start of Set code + name

            int setbeginning = 0;
            int setend = 0;
            setbeginning = namebeginning + 4;
            setend = thetext.IndexOf("-", setbeginning) - 1;
            if (setend - setbeginning > 0)
               setstring = thetext.Substring(setbeginning, setend - setbeginning);

            namebeginning = thetext.IndexOf(" ", namebeginning);
            //Passes set code
            int nameend = thetext.IndexOf("</h1>", namebeginning);
            namestring = thetext.Substring(namebeginning + 1, nameend - namebeginning - 1).Trim().Replace("'", "");

            int cardtypebeginning = thetext.IndexOf("&amp;" + "\"" + ">", nameend) + 7;
            int cardtypeend = thetext.IndexOf("</a>", nameend);
            cardtypestring = thetext.Substring(cardtypebeginning, cardtypeend - cardtypebeginning).Trim();

            string[] fields = new string[11];
            startofstuff = cardtypeend;
            int LoreStart = 0;
            int LoreEnd = 0;
            string LoreString = null;
            switch (cardtypestring)
            {
                case "Monster":
                    int TypeIStart = 0;
                    int TypeIEnd = 0;
                    string TypeIString = null;

                    TypeIStart = thetext.IndexOf("Type I:</th>", startofstuff);
                    TypeIStart = thetext.IndexOf("&amp;" + "\"" + ">", TypeIStart)+ 7;
                    TypeIEnd = thetext.IndexOf("</a>", TypeIStart);
                    TypeIString = thetext.Substring(TypeIStart, TypeIEnd - TypeIStart);

                    if (TypeIEnd > -1)
                    {
                        startofstuff = TypeIEnd;
                    }

                    int MonsterTypeStart = -1 ;
                    int MonsterTypeEnd = -1;
                    string MonsterTypeString = null;
                    MonsterTypeStart = thetext.IndexOf("Monster Type:</th>", startofstuff);
                    if (MonsterTypeStart == -1)
                    {
                        MonsterTypeString = "";
                    }
                    else
                    {
                        MonsterTypeStart = thetext.IndexOf("&amp;" + "\"" + ">", MonsterTypeStart) +7;
                        MonsterTypeEnd = thetext.IndexOf("</a>", MonsterTypeStart);
                        MonsterTypeString = thetext.Substring(MonsterTypeStart, MonsterTypeEnd - MonsterTypeStart);
                    }

                    if (MonsterTypeEnd > -1)
                    {
                        startofstuff = MonsterTypeEnd;
                    }

                    int Type2Start = 0;
                    int Type2End = 0;
                    string Type2String = null;
                    Type2Start = thetext.IndexOf("Type II:</th>", startofstuff);
                    Type2Start = thetext.IndexOf("&amp;" + "\"" + ">", Type2Start)+ 7;
                    Type2End = thetext.IndexOf("</a>", Type2Start);
                    Type2String = thetext.Substring(Type2Start, Type2End - Type2Start);

                    if (Type2End > -1)
                    {
                        startofstuff = Type2End;
                    }

                    int AttributeStart = 0;
                    int AttributeEnd = 0;
                    string AttributeString = null;
                    AttributeStart = thetext.IndexOf("Attribute:</th>", startofstuff);
                    AttributeStart = thetext.IndexOf("&amp;" + "\"" + ">", AttributeStart)+ 7;
                    AttributeEnd = thetext.IndexOf("</a>", AttributeStart);
                    AttributeString = thetext.Substring(AttributeStart, AttributeEnd - AttributeStart);

                    startofstuff = AttributeEnd;

                    int LevelStart = 0;
                    int LevelEnd = 0;
                    string LevelString = null;
                    LevelStart = thetext.IndexOf("Level:</th>", startofstuff);
                    if (LevelStart == -1) { LevelStart = thetext.IndexOf("Rank:</th>", startofstuff); }
                    LevelStart = thetext.IndexOf("&amp;" + "\"" + ">", LevelStart)+ 7;
                    LevelEnd = thetext.IndexOf("</a>", LevelStart);
                    LevelString = thetext.Substring(LevelStart, LevelEnd - LevelStart);

                    startofstuff = LevelEnd;

                    int AttackStart = 0;
                    int AttackEnd = 0;
                    string AttackString = null;
                    AttackStart = thetext.IndexOf("Attack:</th>", startofstuff);
                    AttackStart = thetext.IndexOf("&amp;" + "\"" + ">", AttackStart)+ 7;
                    AttackEnd = thetext.IndexOf("</a>", AttackStart);
                    AttackString = thetext.Substring(AttackStart, AttackEnd - AttackStart);

                    startofstuff = AttackEnd;

                    int DefenceStart = 0;
                    int DefenceEnd = 0;
                    string DefenceString = null;
                    DefenceStart = thetext.IndexOf("Defense:</th>", startofstuff);
                    if (DefenceStart == -1) { DefenceStart = thetext.IndexOf("Defence:</th>", startofstuff); }
                DefenceStart = thetext.IndexOf("&amp;" + "\"" + ">", DefenceStart)+ 7;
                DefenceEnd = thetext.IndexOf("</a>", DefenceStart);
                    DefenceString = thetext.Substring(DefenceStart, DefenceEnd - DefenceStart);

                    startofstuff = DefenceEnd;

                    //Different Formatting
                    LoreStart = thetext.IndexOf("Card text</strong>", startofstuff);
                    LoreStart = thetext.IndexOf("<br>", LoreStart)+ 4;
                    LoreEnd = thetext.IndexOf("</p>", LoreStart);
                    LoreString = thetext.Substring(LoreStart, LoreEnd - LoreStart);


                    fields[1] = namestring;
                    fields[2] = Convert.ToString(M.TotalCards + 1);
                    fields[3] = setstring;

                    switch (TypeIString)
                    {
                        case "Token":
                            MessageBox.Show("Sorry, Tokens cannot be entered into the database.");
                            this.Cursor = Cursors.Arrow;
                            return;

                           
                        case "Normal":
                            if (string.IsNullOrEmpty(MonsterTypeString))
                            {
                                fields[4] = Type2String;
                            }
                            else
                            {
                                fields[4] = Type2String + "/" + MonsterTypeString;
                            }
                            break;
                        case "Effect":
                            if (string.IsNullOrEmpty(MonsterTypeString))
                            {
                                fields[4] = Type2String + "/Effect";
                            }
                            else
                            {
                                fields[4] = Type2String + "/" + MonsterTypeString + "/Effect";
                            }

                            break;
                        default:
                            if (string.IsNullOrEmpty(MonsterTypeString))
                            {
                                fields[4] = Type2String + "/" + TypeIString + "/Effect";
                            }
                            else
                            {
                                fields[4] = Type2String + "/" + TypeIString + "/" + MonsterTypeString + "/Effect";
                            }
                            break;
                    }

                    fields[5] = AttributeString;
                    fields[6] = LevelString;
                    fields[7] = "";
                    fields[8] = AttackString;
                    fields[9] = DefenceString;
                    fields[10] = replaceHref(LoreString.Replace("'", ""));

                    break;
                // fields(12) = My.Settings.Username

                case "Spell":
                case "Trap":

                    int STStart = 0;
                    int STEnd = 0;
                    string STString = null;
                    STStart = thetext.IndexOf(cardtypestring + " type:</th>", startofstuff);
                    STStart = thetext.IndexOf("&amp;" + "\"" + ">", STStart)+ 7;
                    STEnd = thetext.IndexOf("</a>", STStart);
                    
                    STString = thetext.Substring(STStart, STEnd - STStart);

                    startofstuff = STEnd;

                    //Different Formatting
                   LoreStart = thetext.IndexOf("Card text</strong>", startofstuff);
                    LoreStart = thetext.IndexOf("<br>", LoreStart)+ 4;
                    LoreEnd = thetext.IndexOf("</p>", LoreStart);
                    LoreString = thetext.Substring(LoreStart, LoreEnd - LoreStart);

                    fields[1] = namestring;
                    fields[2] = Convert.ToString(M.TotalCards + 1);
                    fields[3] = setstring;
                    fields[4] = cardtypestring + " Card";
                    fields[5] = cardtypestring;
                    fields[6] = "";
                    if (STString == "Normal")
                    {
                        fields[7] = "";
                    }
                    else
                    {
                        fields[7] = STString;
                    }
                    fields[8] = "";
                    fields[9] = "";
                    fields[10] = replaceHref(LoreString.Replace("'", ""));

                    break;
                //fields(12) = My.Settings.Username

            }

            System.Text.StringBuilder outputstring = new System.Text.StringBuilder( "|" );
            for (short n = 1; n <= 10; n++)
            {
                outputstring.Append(fields[n] + "|");
            }
            //  outputstring &= username

            txtResults.Text = outputstring.ToString();


            try
            {
                int imageStart = 0;
                int imageEnd = 0;
                string imageURL = null;
                imageStart = thetext.IndexOf("Direct Link:", startofstuff);
                imageStart = thetext.IndexOf("value=" + "\"", imageStart) +7;
                imageEnd = thetext.IndexOf("</td>", imageStart) -2;
                imageURL = thetext.Substring(imageStart, imageEnd - imageStart);
                UpdateCardImage(imageURL, false);
            

            }
            catch 
            {
                UnderlyingImageUrl = string.Empty;

            }
             

            this.Cursor = Cursors.Arrow;
        }
        #endregion



        


       




        
        #region "Commands"
        private void cmdGenerate_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtResults.Text))
            {
                MessageBox.Show("Please upload or clear the card in the results box first.");
                return;
            }
            txtResults.Text = getYVDCoding();


        }
        private string getYVDCoding()
        {
            int MiscCounter = 0;
            string NormalorEffectstring = "";
            string AttrString = "";
            string Typestring = "";
            string Effectstring = "";
            string sticonstring = "";
            int n = 0;

            for (n = 1; n <= 22; n++)
            {
                if ((bool)radType(n).IsChecked)
                    Typestring = radType(n).Content.ToString();
            }
            for (n = 2; n <= 10; n++)
            {
                if ((bool)chkMonEffects(n).IsChecked)
                    Effectstring += "/" + chkMonEffects(n).Content;
            }

            if ((bool)chkMonEffects(1).IsChecked)
                NormalorEffectstring = "/Effect";
            if ((bool)chkMonEffects(4).IsChecked || (bool)chkMonEffects(5).IsChecked || (bool)chkMonEffects(6).IsChecked || (bool)chkMonEffects(3).IsChecked)
                NormalorEffectstring = "/Effect";
            for (n = 1; n <= 6; n++)
            {
                if ((bool)radAttrib(n).IsChecked)
                    AttrString = radAttrib(n).Content.ToString();
            }
            for (n = 1; n <= 7; n++)
            {
                if ((bool)radCardType_2.IsChecked)
                {
                    if ((bool)radSTIcon(n).IsChecked && n != 7)
                        sticonstring = radSTIcon(n).Content.ToString();
                }
                if ((bool)radCardType_3.IsChecked)
                {
                    if ((bool)radSTIcon(n).IsChecked && n < 3)
                        sticonstring = radSTIcon(n).Content.ToString();
                    if ((bool)radSTIcon(n).IsChecked && n == 7)
                        sticonstring = radSTIcon(n).Content.ToString();
                }


            }
            if ((bool)radSTIcon(1).IsChecked)
                sticonstring = "";


            if (string.IsNullOrEmpty(txtCardName.Text))
            {
                MessageBox.Show("Error: Name cannot be blank.");
                return "";
            }
            if (txtCardName.Text.Contains("_"))
            {
                MessageBox.Show("Error: Name cannot contain Underscores.");
                return "";
            }
            if (string.IsNullOrEmpty(txtLore.Text))
            {
                MessageBox.Show("Error: Lore cannot be blank.");
                return "";
            }

            if (txtCardName.Text == "-")
            {
                MessageBox.Show("Error: invalid name.");
                return "";
            }

            txtATK.Text = txtATK.Text.Trim();
            txtDEF.Text = txtDEF.Text.Trim();
            txtLVL.Text = txtLVL.Text.Trim();

            if ((bool)radCardType_1.IsChecked)
            {
                if (M.isNumeric(txtATK.Text) == false && txtATK.Text != "?")
                {
                    MessageBox.Show("Error: ATK, DEF and Level must be occupied with numbers.");
                    return "";
                }
                else if (M.isNumeric(txtDEF.Text) == false && txtDEF.Text != "?")
                {
                    MessageBox.Show("Error: ATK, DEF and Level must be occupied with numbers.");
                    return "";
                }
                else if (M.isNumeric(txtLVL.Text) == false)
                {
                    MessageBox.Show("Error: ATK, DEF and Level must be occupied with numbers.");
                    return "";
                }

                MiscCounter = Convert.ToInt32(txtLVL.Text);
                if (MiscCounter > 12 || MiscCounter < 1 || txtLVL.Text.Contains("."))
                {
                    MessageBox.Show("Error: Level must be an Integer between 1 and 12.");
                    return "";
                }



                if (string.IsNullOrEmpty(Typestring) || string.IsNullOrEmpty(AttrString))
                {
                    MessageBox.Show("Error: Must have a Type and Attribute.");
                    return "";
                }
            }

            if (txtSpecialFilter.Text == "--PUBLIC--" || txtSpecialFilter.Text == "--PRIVATE--")
            {
                MessageBox.Show("Error: " + txtSpecialFilter.Text + " is a key phrase and cannot be used as a Set name.");
                return "";
            }

            txtLore.Text = txtLore.Text.Replace("|", "");

            if (!M.isDefaultSet)
            {
                int ampersandCount = txtLore.Text.Count(s => s == '@');
                if (ampersandCount == 1)
                {
                    txtLore.Text = txtLore.Text.Replace("@", "|");
                }
                else if (ampersandCount > 1)
                {
                    MessageBox.Show("Error: More than one @ was found in the lore. Only use this character to separate the effect text from the flavor text, like so:" + Environment.NewLine + Environment.NewLine + "Draw 1 card.@This is some flavor text.");
                    return "";
                }
            }

            string result = "";
            if ((bool)radCardType_1.IsChecked)
            {
                result = "|" + txtCardName.Text + "|" + "" + "|" + txtSpecialFilter.Text + "|" + Typestring + Effectstring + NormalorEffectstring + "|" + AttrString + "|" + txtLVL.Text.Trim() + "|" + "|" + txtATK.Text.Trim() + "|" + txtDEF.Text.Trim() + "|" + txtLore.Text.Trim() + "|";

            }
            else
            {
                if ((bool)radCardType_2.IsChecked)
                {
                    result = "|" + txtCardName.Text + "|" + "" + "|" + txtSpecialFilter.Text + "|" + "Spell Card" + "|" + "Spell" + "|" + "|" + sticonstring + "|" + "|" + "|" + txtLore.Text.Trim() + "|";

                }
                else
                {
                    if ((bool)radCardType_3.IsChecked)
                    {
                        result = "|" + txtCardName.Text + "|" + "" + "|" + txtSpecialFilter.Text + "|" + "Trap Card" + "|" + "Trap" + "|" + "|" + sticonstring + "|" + "|" + "|" + txtLore.Text.Trim() + "|";

                    }
                }
            }

            return result;
        }
        private void cmdClear_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            txtResults.Text = string.Empty;
        }
        private void cmdUpload_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(M.username) || M.username.Contains("portalguest"))
            {
                MessageBox.Show("You cannot use this function until you are Logged In.");
                return;
            }

            if (txtResults.Text.Trim() == "")
            {
                MessageBox.Show("Press 'Generate' to generate the card first, then upload.");
                return;
            }

            for (int n = 1; n <= M.TotalCards; n++)
            {
                if (!string.IsNullOrEmpty(M.CardStats[n].Name) && M.CardStats[n].Name == txtCardName.Text)
                {
                    MessageBox.Show("A card by this name already exists (it might be a public card).");
                    return;
                }

            }




            string yvdCode = txtResults.Text;
            if (string.IsNullOrEmpty(yvdCode))
                return;
            int lbar = 0;
            int rbar = 0;
            string[] fields = new string[12];
            try
            {
                for (short n = 1; n <= 11; n++)
                {
                    rbar = yvdCode.IndexOf("|", lbar + 1);
                    if (rbar == -1) break;
                    fields[n] = yvdCode.Substring(lbar + 1, rbar - lbar - 1);
                    lbar = rbar;
                }


                SQLReference.CardDetails newStats = new SQLReference.CardDetails();
                newStats.Name = fields[1];
                // newStats.ID = fields(2)
                newStats.SpecialSet = fields[3];
                newStats.Type = fields[4];
                if (newStats.Type == "Spell Card" || newStats.Type == "Trap Card")
                    newStats.Type = "";
                newStats.Attribute = fields[5];
                newStats.Level = fields[6].ToIntCountingQuestions();
                if (newStats.Type == "")
                    newStats.Type = fields[7];

                newStats.ATK = fields[8].ToIntCountingQuestions();
                newStats.DEF = fields[9].ToIntCountingQuestions();

                newStats.Lore = fields[10];
                if (fields[11] != null) //Flavor text
                {
                    if (M.isDefaultSet)
                    {
                        MessageBox.Show("The 'Flavor Text' Field is only available to CCGers right now, sorry!");
                        return;
                    }
                    newStats.Flavor = fields[11];
                }
                newStats.Creator = M.username;
                newStats.Limit = 3;

                if (newStats.SpecialSet == "--PUBLIC--" || newStats.SpecialSet == "--PRIVATE--")
                {
                    MessageBox.Show("Error: " + newStats.SpecialSet + " is a key phrase and cannot be used as a Set name.");
                    return;
                }


                if (useYCMTemplate)
                {
                    UnderlyingImageUrl = M.formatYCM(newStats, UnderlyingImageUrl);
                    useYCMTemplate = false;
                }

                if (!M.isDefaultSet)
                {
                    newStats.ID = -1;
                    sqlCli.AddOrChangeCardCCGAsync(M.mySet, -1, newStats, UnderlyingImageUrl, newStats);
                }
                else if ((bool)chkPublic.IsChecked)
                {
                    newStats.Creator = "PUB";
                    sqlCli.submitCardAsync(newStats, UnderlyingImageUrl, false, true, newStats);
                }
               // else if ((bool)chkUneditable.IsChecked)
               //     sqlCli.submitCardAsync(newStats, UnderlyingImageUrl, true, false, newStats);
               // else
               //     sqlCli.submitCardAsync(newStats, UnderlyingImageUrl, false, false, newStats);

                cmdUpload.IsEnabled = false;

            }
            catch
            { MessageBox.Show("The code is not well formatted. Please re-generate it."); cmdUpload.IsEnabled = true; }

        }
        private void cmdEditExisting_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(M.username) || M.username.Contains("portalguest"))
            {
                MessageBox.Show("You cannot use this function until you are Logged In.");
                return;
            }



            editForm.Show();
        }
        private void cmdCancelEdit_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            changeModes(false);
            M.EditExistingCardID = 0;
        }
        private void cmdCommit_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            string yvdCode = getYVDCoding();
            if (string.IsNullOrEmpty(yvdCode))
                return;
            int lbar = 0;
            int rbar = 0;
            string[] fields = new string[12];
            for (int n = 1; n <= 11; n++)
            {
                rbar = yvdCode.IndexOf("|", lbar + 1);
                if (rbar == -1) break;
                fields[n] = yvdCode.Substring(lbar + 1, rbar - lbar - 1);
                lbar = rbar;
            }
            
            SQLReference.CardDetails newStats = new SQLReference.CardDetails();
            newStats.Name = fields[1];
            newStats.ID = M.CardStats[M.EditExistingCardID].ID;
            newStats.SpecialSet = fields[3];
            newStats.Type = fields[4];
            if (newStats.Type == "Spell Card" || newStats.Type == "Trap Card")
                newStats.Type = "";
            newStats.Attribute = fields[5];
            newStats.Level = fields[6].ToIntCountingQuestions();
            if (newStats.Type == "")
                 newStats.Type = fields[7];
            newStats.ATK = fields[8].ToIntCountingQuestions();
            newStats.DEF = fields[9].ToIntCountingQuestions();
            newStats.Lore = fields[10];
            if (fields[11] != null)
            {
                if (M.isDefaultSet)
                {
                    MessageBox.Show("The 'Flavor Text' Field is only available to CCGers right now, sorry!");
                    return;
                }
                newStats.Flavor = fields[11];

            }
            newStats.Limit = M.CardStats[M.EditExistingCardID].Limit;

            if (newStats.SpecialSet == "--PUBLIC--" || newStats.SpecialSet == "--PRIVATE--")
            {
                MessageBox.Show("Error: " + newStats.SpecialSet + " is a keyword and cannot be used as a Set Name.");
                return;
            }

            if (M.CardStats[M.EditExistingCardID].Name != newStats.Name) //Name was changed, check if it conflicts
            {
                for (int n = 1; n <= M.TotalCards; n++)
                {
                    if (!string.IsNullOrEmpty(M.CardStats[n].Name) && M.CardStats[n].Name == newStats.Name)
                    {
                        MessageBox.Show("A card by this name already exists (it might be a public card).");
                        return;
                    }
                }
            }
            if (M.isDefaultSet)
            {
                if ((bool)chkPublic.IsChecked)
                {
                    if (!M.isOwner(M.EditExistingCardID))
                    {
                        MessageBox.Show("You cannot make a card public if you aren't the original creator.");
                        return;
                    }
                    newStats.Creator = "PUB";
                }
                else if ((bool)chkUneditable.IsChecked)
                {
                   if (M.CardStats[M.EditExistingCardID].Creator.Contains("_") == false)
                      newStats.Creator = "_" + M.CardStats[M.EditExistingCardID].Creator;
                   else
                      newStats.Creator = M.CardStats[M.EditExistingCardID].Creator;
                }
                else
                    newStats.Creator = M.CardStats[M.EditExistingCardID].Creator.Replace("_", ""); //make editable
            }

            if (M.isDefaultSet)
                sqlCli.editCardAsync(M.CardStats[M.EditExistingCardID].ID, newStats, M.username, UnderlyingImageUrl, M.mySet, newStats);
            else
                sqlCli.AddOrChangeCardCCGAsync(M.mySet, M.CardStats[M.EditExistingCardID].ID, newStats, UnderlyingImageUrl, newStats);
        }
        private void cmdDelete_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            if (!M.isDefaultSet)
            {
                MessageBox.Show("Sorry, this feature is not supported yet for CCGs!");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this card?", "Delete Card", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
                return;
            sqlCli.deleteCardAsync(M.CardStats[M.EditExistingCardID], M.username);
        }
        #endregion
        
        #region "SqlCli Callbacks"
        public SQLReference.Service1ConsoleClient sqlCli;
        void sqlCli_EndSubmitCard(object sender, SQLReference.submitCardCompletedEventArgs e)
        {
            cmdUpload.IsEnabled = true;


            //Greater than zero, represents the id of card submitted
            if (e.Error == null)
            {
                if (e.Result.message != null)
                    MessageBox.Show(e.Result.message);
                SQLReference.CardDetails stats = (SQLReference.CardDetails)e.UserState;

                if (e.Result.cardSuccess == true)
                {


                    if (stats.Creator != "PUB")
                    {
                       if ((bool)chkUneditable.IsChecked)
                         stats.Creator = "_|" + M.username + "|";
                       else
                            stats.Creator = "|" + M.username + "|";
                    }
                    else
                        stats.Creator = "PUB";
                    stats.ID = e.Result.newCardTrueID;
                    M.CardStats.Add(stats);
                }

                if (e.Result.imageSuccess == true)
                {
                    M.cardsWithImages.Add(M.toProperImageNameNoExtension(M.findIndexFromTrueID(stats.ID)));
                    UnderlyingImageUrl = string.Empty;
                }

            }
            else
            {
                MessageBox.Show("There was an error submitting your card.");

            }

            txtYCMURL.Text = "";

        }
        void sqlCli_EndEditCard(object sender, SQLReference.editCardCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show(e.Result.message);
                SQLReference.CardDetails newStats = (SQLReference.CardDetails)e.UserState;
                if (e.Result.imageSuccess == true)
                {
                    if (e.Result.cardHasImageNow == true)
                        M.cardsWithImages.Add(M.toProperImageNameNoExtension(M.findIndexFromTrueID(newStats.ID)));
                    else if (e.Result.cardHasImageNow == false)
                        M.cardsWithImages.Remove(M.toProperImageNameNoExtension(M.findIndexFromTrueID(newStats.ID)));

                    UnderlyingImageUrl = string.Empty;
                }
                if (e.Result.cardSuccess == true)
                    M.CardStats[M.EditExistingCardID] = newStats;

                M.EditExistingCardID = 0;

                changeModes(false);


            }
            else
            {
                MessageBox.Show("There was an error editing your card.");
            }

            
           
        }
        void sqlCli_EndDeleteCard(object sender, SQLReference.deleteCardCompletedEventArgs e)
        {
            if (e.Result == true)
            {
                MessageBox.Show("Card deleted.");

                M.cardsWithImages.Remove(M.toProperImageNameNoExtension(M.EditExistingCardID));
                M.CardStats.RemoveAt(M.EditExistingCardID);
                changeModes(false);
                M.EditExistingCardID = 0;

            }
            else
            {
                MessageBox.Show("There was an error deleting your card.");
            }
        }
        void sqlCli_AddOrChangeCardCCGCompleted(object sender, SQLReference.AddOrChangeCardCCGCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show(e.Result.message);
                SQLReference.CardDetails newStats = (SQLReference.CardDetails)e.UserState;
                if (e.Result.imageSuccess == true)
                {
                    if (e.Result.cardHasImageNow == true)
                        M.cardsWithImages.Add(M.toProperImageNameNoExtension(M.findIndexFromTrueID(newStats.ID)));
                    else if (e.Result.cardHasImageNow == false)
                        M.cardsWithImages.Remove(M.toProperImageNameNoExtension(M.findIndexFromTrueID(newStats.ID)));

                    UnderlyingImageUrl = string.Empty;
                }
                if (e.Result.cardSuccess == true)
                {
                    if (newStats.ID == -1)
                        M.CardStats.Add(newStats);
                    else
                        M.CardStats[M.EditExistingCardID] = newStats;

                }

                M.EditExistingCardID = 0;

                changeModes(false);


            }
            else
            {
                MessageBox.Show("There was an error submitting/editing your card.");
            }
        }
        #endregion

        #region "Form Maintenance"
        private void radCardType_1_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            for (int n = 1; n <= 6; n++)
            {
                radSTIcon(n).Visibility = System.Windows.Visibility.Collapsed;
            }
            radSTIcon_7.Visibility = System.Windows.Visibility.Collapsed;

            ChangeVisibilities(true);
        }
        private void radCardType_2_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            for (int n = 1; n <= 6; n++)
            {
                radSTIcon(n).Visibility = System.Windows.Visibility.Visible;
            }
            radSTIcon_7.Visibility = System.Windows.Visibility.Collapsed;


            ChangeVisibilities(false);

        }
        private void radCardType_3_Checked(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            for (int n = 3; n <= 6; n++)
            {
                radSTIcon(n).Visibility = System.Windows.Visibility.Collapsed;
            }
            radSTIcon_1.Visibility = System.Windows.Visibility.Visible;
            radSTIcon_2.Visibility = System.Windows.Visibility.Visible;
            radSTIcon_7.Visibility = System.Windows.Visibility.Visible;

            ChangeVisibilities(false);
        }
        private void radType_CheckChanged(System.Object sender, RoutedEventArgs e)
        {
            if (LayoutRoot == null || sender == null) return;
            RadioButton radSender = (RadioButton)sender;
            int index = Convert.ToInt32(radSender.Name.Substring("radType_".Length, radSender.Name.Length - "radType_".Length));
            
            for (int n = 1; n <= 22; n++)
            {
                if (n != index)
                {
                    radType(n).IsChecked = false;
                }
            }
            //noRecursion = false;
        }
        private void radSTIcon_CheckChanged(System.Object sender, RoutedEventArgs e)
        {
            CheckBox chkSender = (CheckBox)sender;
            int index = Convert.ToInt32(chkSender.Name.Substring(11, chkSender.Name.Length - 11));
            //noRecursion = true;
            for (int n = 1; n <= 7; n++)
            {
                if (n != index)
                {
                    radSTIcon(n).IsChecked = false;
                }
            }
        }
     
        private void radAttrib_CheckChanged(System.Object sender, RoutedEventArgs e)
        {
            CheckBox chkSender = (CheckBox)sender;
            int index = Convert.ToInt32(chkSender.Name.Substring(11, chkSender.Name.Length - 11));
            for (int n = 1; n <= 6; n++)
            {
                if (n != index)
                {
                    radAttrib(n).IsChecked = false;
                }
            }

        }
        private void editForm_Closed(object sender, EventArgs e)
        {
            int n = 0;

            if (M.EditExistingCardID > 0)
            {

                changeModes(true);


                if (M.cardsWithImages.Contains(M.toProperImageNameNoExtension(M.EditExistingCardID)))
                    UpdatePictureBox(ref imgCardsImage, M.CardStats[M.EditExistingCardID]);
                else
                    M.setImage(imgCardsImage, "NoImage.png", UriKind.Relative);


                txtCardName.Text = M.CardStats[M.EditExistingCardID].Name;
                txtLore.Text = M.CardStats[M.EditExistingCardID].Lore;
                if (!string.IsNullOrEmpty(M.CardStats[M.EditExistingCardID].Flavor)) txtLore.Text += "@" + M.CardStats[M.EditExistingCardID].Flavor;
                txtSpecialFilter.Text = M.CardStats[M.EditExistingCardID].SpecialSet;
                txtATK.Text = M.CardStats[M.EditExistingCardID].ATK.ToStringCountingQuestions();
                txtDEF.Text = M.CardStats[M.EditExistingCardID].DEF.ToStringCountingQuestions();
                txtLVL.Text = M.CardStats[M.EditExistingCardID].Level.ToStringCountingQuestions();
                if (M.CardStats[M.EditExistingCardID].Creator == "PUB")
                    chkUneditable.IsEnabled = false;
                else
                    chkUneditable.IsEnabled = true;
                string first = "";
                string second = "";
                string third = "";
                try
                {
                    first = M.getFirstType(M.CardStats[M.EditExistingCardID].Type);
                    second = M.getSecondType(M.CardStats[M.EditExistingCardID].Type);
                    third = M.getThirdType(M.CardStats[M.EditExistingCardID].Type);

                }
                catch (Exception)
                {
                }
                for (n = 1; n <= 22; n++)
                {
                    if (radType(n).Content.ToString() == first && radType(n).IsChecked == false)
                    {
                        radType(n).IsChecked = true;
                        break;
                    }
                }
                for (n = 3; n <= 11; n++)
                {
                    if ((chkMonEffects(n).Content.ToString() == second && chkMonEffects(n).IsChecked == false) || (chkMonEffects(n).Content.ToString() == third && chkMonEffects(n).IsChecked == false))
                    {
                        chkMonEffects(n).IsChecked = true;
                        break;
                    }
                }
                chkMonEffects(1).IsChecked = false;
                chkMonEffects(2).IsChecked = false;
                if (M.CardStats[M.EditExistingCardID].Type.Contains("Effect") && M.CardStats[M.EditExistingCardID].IsMonster())
                    chkMonEffects(2).IsChecked = true; //Effect
                else if (M.CardStats[M.EditExistingCardID].IsMonster())
                    chkMonEffects(1).IsChecked = true; //Normal



                if (M.CardStats[M.EditExistingCardID].Attribute == "Spell" && radCardType_2.IsChecked == false)
                {
                    radCardType_2.IsChecked = true;
                }
                else if (M.CardStats[M.EditExistingCardID].Attribute == "Trap" && radCardType_3.IsChecked == false)
                {
                    radCardType_3.IsChecked = true;

                }
                else if (M.CardStats[M.EditExistingCardID].Attribute != "Spell" && M.CardStats[M.EditExistingCardID].Attribute != "Trap" && radCardType_1.IsChecked == false)
                {
                    radCardType_1.IsChecked = true;
                }


                for (n = 1; n <= 6; n++)
                {
                    if (M.CardStats[M.EditExistingCardID].Attribute == radAttrib(n).Content.ToString() && radAttrib(n).IsChecked == false)
                    {
                        radAttrib(n).IsChecked = true;
                        break;
                    }
                }
                for (n = 1; n <= 7; n++)
                {
                    if (!M.CardStats[M.EditExistingCardID].IsMonster() &&
                        !string.IsNullOrEmpty(M.CardStats[M.EditExistingCardID].Type) &&
                        M.CardStats[M.EditExistingCardID].Type == radSTIcon(n).Content.ToString())
                    {
                        radSTIcon(n).IsChecked = true;
                        break;
                    }
                }
            }
            else
            {
                cmdCommit.Visibility = System.Windows.Visibility.Collapsed;
                cmdCancelEdit.Visibility = System.Windows.Visibility.Collapsed;
                cmdDelete.Visibility = System.Windows.Visibility.Collapsed;
                cmdGenerate.IsEnabled = true;
                cmdUpload.IsEnabled = true;
            }

        }
        public void changeModes(bool InEditMode)
        {
            if (InEditMode)
            {

                cmdCommit.Visibility = System.Windows.Visibility.Visible;
                cmdDelete.Visibility = System.Windows.Visibility.Visible;

                cmdCancelEdit.Visibility = System.Windows.Visibility.Visible;


                cmdEditExisting.Visibility = System.Windows.Visibility.Collapsed;
                cmdGenerate.IsEnabled = false;
                cmdUpload.IsEnabled = false;
                cmdClear.IsEnabled = false;
                cmdYCMURLGenerate.IsEnabled = false;
                txtYCMURL.IsEnabled = false;
            }
            else
            {

                cmdCommit.Visibility = System.Windows.Visibility.Collapsed;
                cmdDelete.Visibility = System.Windows.Visibility.Collapsed;
                cmdCancelEdit.Visibility = System.Windows.Visibility.Collapsed;


                cmdEditExisting.Visibility = System.Windows.Visibility.Visible;
                cmdGenerate.IsEnabled = true;
                cmdUpload.IsEnabled = true;
                cmdClear.IsEnabled = true;
                cmdYCMURLGenerate.IsEnabled = true;
                txtYCMURL.IsEnabled = true;
            }



        }
        void ChangeVisibilities(bool isMonster)
        {
            if (isMonster)
            {
                txtATK.Visibility = System.Windows.Visibility.Visible;
                txtDEF.Visibility = System.Windows.Visibility.Visible;
                txtLVL.Visibility = System.Windows.Visibility.Visible;
                lblATKPlaceholder.Visibility = System.Windows.Visibility.Visible;
                lblDEFPlaceholder.Visibility = System.Windows.Visibility.Visible;
                lblLVLPlaceholder.Visibility = System.Windows.Visibility.Visible;
                BordIcon.Visibility = System.Windows.Visibility.Collapsed;
                BordType.Visibility = System.Windows.Visibility.Visible;
                BordAttribute.Visibility = System.Windows.Visibility.Visible;
                BordSubtype.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                txtATK.Visibility = System.Windows.Visibility.Collapsed;
                txtDEF.Visibility = System.Windows.Visibility.Collapsed;
                txtLVL.Visibility = System.Windows.Visibility.Collapsed;
                lblATKPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblDEFPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
                lblLVLPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
                BordIcon.Visibility = System.Windows.Visibility.Visible;
                BordType.Visibility = System.Windows.Visibility.Collapsed;
                BordAttribute.Visibility = System.Windows.Visibility.Collapsed;
                BordSubtype.Visibility = System.Windows.Visibility.Collapsed;

            }
        }
        private void chkPublic_Checked(object sender, RoutedEventArgs e)
        {
            publicCardWarningForm.attemptShow();

        }

        void publicCardWarningForm_Closed(object sender, EventArgs e)
        {
            if (publicCardWarningForm.DialogResult == false)
                chkPublic.IsChecked = false;
        }
        private MainPage MyMainPage
        {
            get
            {
                return ((MainPage)((Canvas)((Border)((Frame)Parent).Parent).Parent).Parent);
            }
        }
        #endregion

        #region "Other"
        private string replaceHref(string str)
        {
            int lbar = 0, rbar = 0;
            str = str.Replace("</a>", "");
            str = str.Replace("<br/>", Environment.NewLine);
            do
            {
                lbar = str.IndexOf("<a href");
                if (lbar == -1 || rbar == -1)
                { break; }
                rbar = str.IndexOf(">", lbar);
              
                string takeout = str.Substring(lbar, rbar - lbar + 1);
                str = str.Replace(takeout, "");
                lbar = rbar;
            } while (true);

            return str;
        }

        #endregion

      





    }
}
