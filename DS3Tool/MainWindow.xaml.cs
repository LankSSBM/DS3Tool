﻿using DS3Tool.services;
using Microsoft.Win32;
using MiscUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;


namespace DS3Tool
{
    public partial class MainWindow : Window, IDisposable
    {
        DS3Process _process = null;
        private CinderPhaseManager _cinderManager;
        private NoClipService noclipService;
        private const string CINDER_ENEMY_ID = "c5280_0000";

        private bool disposedValue;

        System.Windows.Threading.DispatcherTimer _timer = new System.Windows.Threading.DispatcherTimer();
        string _normalTitle = "";

        bool _hooked = false;
        bool _hotkeysEnabled = true;
        bool _initializing = false;

        bool _freeCamFirstActivation = true;
        bool panelsCollapsed = false;


        public Dictionary<string, Item> ItemDictionary { get; private set; }
        public Dictionary<string, BonfireLocation> BonfireDictionary { get; private set; }

        (float, float, float, float)? savedPos = null;

        public MainWindow()
        {
            GameVersionCheck();
            InitializeComponent();

            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            LoadItemsFromCsv("items.csv");
            LoadBonfiresFromCsv("bonfires.csv");


            var assInfo = Assembly.GetEntryAssembly().GetName();
            Title = "DS3Tool v" + assInfo.Version;
            _normalTitle = Title;

            Closed += MainWindow_Closed;
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;

        retry:
            try
            {
                _process = new DS3Process();
            }
            catch { _process = null; }
            if (null == _process)
            {
                var res = MessageBox.Show("Could not attach to the game. This could be because it's not running, or because it was blocked by another process.\r\n\r\nClick Yes to try launching the game automatically, or No to just try attaching again.", "BlameLank", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    if (!LaunchUtils.launchGame())
                    {
                        MessageBox.Show("Could not launch game.", "Sadge", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {//success but wait a bit for it to start.
                        for (int i = 0; i < 30; i++)
                        {
                            System.Threading.Thread.Sleep(1000);
                            if (DS3Process.checkGameRunning())
                            {
                                System.Threading.Thread.Sleep(1000); 
                                break;
                            }
                        }
                    }
                    goto retry;
                }
                else if (res == MessageBoxResult.No)
                {
                    goto retry;
                }
                Close();
            }
            else
            {//we good
                _process.patchLogos();
                _timer.Tick += _timer_Tick;
                _timer.Interval = TimeSpan.FromSeconds(0.1);
                _timer.Start();

                _cinderManager = new CinderPhaseManager(_process);
                noclipService = new NoClipService(_process);


            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _initializing = true;
            loadWindowState();
            _initializing = false;

            if (_hotkeysEnabled)
            {
                hotkeyInit();
            }

            doUpdateCheck();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                var windowInfo = $"{Left} {Top} {chkSteamInputEnum.IsChecked} {chkStayOnTop.IsChecked} " +
                    $"{resistsPanel.Visibility} {PlayerPanel.Visibility} {EnemyPanel.Visibility} " +
                    $"{ViewsPanel.Visibility} {MovementPanel.Visibility} {MeshPanel.Visibility} {MiscPanel.Visibility} " +
                    $"{!chkEnableHotkeys.IsChecked}";
                File.WriteAllText(windowStateFile(), windowInfo);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        private void loadWindowState()
        {
            try
            {
                var windowInfo = File.ReadAllText(windowStateFile());
                if (string.IsNullOrEmpty(windowInfo)) { return; }
                var spl = windowInfo.Split(' ');
                var left = double.Parse(spl[0]);
                var top = double.Parse(spl[1]);
                _hotkeysEnabled = bool.Parse(spl[11]);
                chkEnableHotkeys.IsChecked = !_hotkeysEnabled;
                btnHotkeys.IsEnabled = _hotkeysEnabled;

                if ((left + Width) > System.Windows.SystemParameters.VirtualScreenWidth || (top + Height) > System.Windows.SystemParameters.VirtualScreenHeight)
                {
                    Console.WriteLine("Not restoring position, would go off-screen");
                }
                else
                {
                    Left = left;
                    Top = top;
                }

                chkSteamInputEnum.IsChecked = bool.Parse(spl[2]);
                chkStayOnTop.IsChecked = bool.Parse(spl[3]);

                RestorePanelVisibility(resistsPanelControl, resistsPanel, spl[4]);
                RestorePanelVisibility(PlayerPanelControl, PlayerPanel, spl[5]);
                RestorePanelVisibility(EnemyPanelControl, EnemyPanel, spl[6]);
                RestorePanelVisibility(ViewsPanelControl, ViewsPanel, spl[7]);
                RestorePanelVisibility(MovementPanelControl, MovementPanel, spl[8]);
                RestorePanelVisibility(MeshPanelControl, MeshPanel, spl[9]);
                RestorePanelVisibility(MiscPanelControl, MiscPanel, spl[10]);

            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        public async void doUpdateCheck(bool force = false)
        {
            HttpClient httpClient = new HttpClient();
            var checkFile = Utils.getFnameInAppdata("LastUpdateCheck", "DS3Tool");
            var lastCheckDate = Utils.getFileDate(checkFile);
            var sinceLastCheck = DateTime.Now - lastCheckDate;
            Utils.debugWrite($"Last check was {sinceLastCheck.TotalDays} days ago");

            string apiUrl = $"https://api.github.com/repos/LankSSBM/DS3Tool/releases/latest";

            if (force || sinceLastCheck.TotalDays >= 1)
            {
                try
                {
                    string currentVersionStr = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    string webVersionStr = "";
                    Version currentVersion = new Version(currentVersionStr);

                    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DS3Tool", currentVersionStr));
                    Stream responseStream = await httpClient.GetStreamAsync(apiUrl);
                    StreamReader reader = new StreamReader(responseStream);

                    string line;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        int tagIndex = line.IndexOf("\"tag_name\":", StringComparison.OrdinalIgnoreCase);

                        if (tagIndex == -1) return;

                        int quoteStart = line.IndexOf('"', tagIndex + "\"tag_name\":".Length) + 1;
                        int quoteEnd = line.IndexOf('"', quoteStart);

                        if (quoteStart == -1 || quoteEnd == -1) return;

                        webVersionStr = line.Substring(quoteStart, quoteEnd - quoteStart).TrimStart('v');
                    }

                    if (currentVersion.CompareTo(new Version(webVersionStr)) == 1)
                    {
                        AppUpdateTxt.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        AppUpdateTxt.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error checking for updates. {e.Message}");
                }

            }
        }

        private void GameVersionCheck()
        {
            string gameExePath = LaunchUtils.GetDarkSouls3ExePath();
            if (!string.IsNullOrEmpty(gameExePath) && File.Exists(gameExePath))
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(gameExePath);

                if (fileVersionInfo.FileVersion != "1.15.0.0")
                {
                    MessageBox.Show("Your Dark Souls 3 patch might not be 1.15. Most features may not work properly unless you update the game to patch 1.15.",
                        "Version Mismatch", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Your Dark Souls 3 patch may not be 1.15 or the game might not be located at the expected path. Ensure the game is updated to this version for proper functionality.",
                        "Version Check Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void OpenNewVersionInBrowser(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/LankSSBM/DS3Tool/releases");
        }

        private string GetDarkSouls3ExePath()
        {
            string steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
            if (string.IsNullOrEmpty(steamPath))
                return null;

            List<string> libraries = new List<string>();

            string steamInstallConfigPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf"); // contains info about steam games install locations
            if (File.Exists(steamInstallConfigPath))
            {

                string[] lines = File.ReadAllLines(steamInstallConfigPath);
                var regex = new Regex(@"""path""\s+""(.+?)"""); // search the config for lines with the text: "path" 

                foreach (string line in lines)
                {
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        string path = match.Groups[1].Value.Replace(@"\\", @"\");
                        libraries.Add(path);
                    }
                }

            }

            foreach (string installDir in libraries)
            {
                string gamePath = Path.Combine(installDir, "steamapps", "common", "DARK SOULS III", "Game", "DarkSoulsIII.exe");
                if (File.Exists(gamePath))
                {
                    return gamePath;
                }
            }

            return null;
        }

        public class Item
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string Type { get; set; }

            public Item(string name, string address, string type = null)
            {
                Name = name;
                Address = address;
                Type = type;
            }
        }

        public struct BonfireLocation
        {
            public int Offset { get; }
            public int StartBit { get; }
            public int Id { get; }

            public BonfireLocation(int offset, int startBit, int Id)
            {
                Offset = offset;
                StartBit = startBit;
                this.Id = Id;
            }
        }


        private void LoadItemsFromCsv(string fileName)
        {
            ItemDictionary = new Dictionary<string, Item>();

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    if (columns.Length >= 2)
                    {
                        string name = columns[0].Trim('"', ' ');
                        string address = columns[1].Trim('"', ' ');

                        string type = columns.Length > 2 ? columns[2].Trim('"', ' ') : null;

                        var item = new Item(name, address, type);
                        ItemDictionary[name] = item;
                    }
                }
            }
        }

        private void LoadBonfiresFromCsv(string fileName)
        {
            BonfireDictionary = new Dictionary<string, BonfireLocation>();

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    if (columns.Length >= 4)
                    {
                        string name = columns[0].Trim('"', ' ');
                        int offset = Convert.ToInt32(columns[1].Trim('"', ' '), 16);

                        int startBit = int.Parse(columns[2].Trim('"', ' '));
                        int id = int.Parse(columns[3].Trim('"', ' '));

                        BonfireLocation bonfireLocation = new BonfireLocation(offset, startBit, id);

                        BonfireDictionary[name] = bonfireLocation;
                    }
                }
            }
        }

        private void RestorePanelVisibility(DockPanel dockPanel, StackPanel stackPanel, string panelVisibility)
        {
            if (panelVisibility != Visibility.Visible.ToString())
            {
                stackPanel.Visibility = Visibility.Collapsed;
                FixPanelArrows(dockPanel, panelVisibility);
            }
        }

        static string windowStateFile()
        {
            return Utils.getFnameInAppdata("windowstate.txt", "DS3Tool");
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var good = _process?.weGood ?? false;
            mainPanel.IsEnabled = good;
            Title = good ? _normalTitle : "BlameLank";

            if (!good) { return; }
            if (_hooked)
            {
                try
                {
                    updateTargetInfo();
                }
                catch { }
            }
        }

        void updateTargetInfo()
        {
            var hp = _process.getSetTargetInfo(DS3Process.TargetInfo.HP);
            var hpmax = _process.getSetTargetInfo(DS3Process.TargetInfo.MAX_HP);
            var poise = _process.getSetTargetInfo(DS3Process.TargetInfo.POISE);
            var poisemax = _process.getSetTargetInfo(DS3Process.TargetInfo.MAX_POISE);
            var poisetimer = _process.getSetTargetInfo(DS3Process.TargetInfo.POISE_TIMER);

            string enemyId = _process.GetSetTargetEnemyID();

            StackPanel cinderPanel = (StackPanel)FindName("MasterCinderPanel");

            if (enemyId == CINDER_ENEMY_ID)
            {
                if (cinderPanel != null)
                {
                    cinderPanel.Visibility = Visibility.Visible;
                }

            }
            else
            {
                if (cinderPanel != null)
                {
                    cinderPanel.Visibility = Visibility.Collapsed;
                }
            }


            //Console.WriteLine($"{hp} {hpmax} {poise} {poisemax} {poisetimer}");
            if (double.IsNaN(hp)) { return; }

            if (hpBar.Value > hpmax) { hpBar.Value = 0; }
            hpBar.Maximum = hpmax;
            hpBar.Value = hp > 0 ? hp : 0;
            hpText.Text = $"HP: {(int)hp} / {(int)hpmax}";

            if (poiseBar.Value > poisemax) { poiseBar.Value = 0; }
            poiseBar.Maximum = double.IsNaN(poisemax) ? 1 : poisemax;
            poiseBar.Value = double.IsNaN(poise) ? 0 : poise > 0 ? poise : 0;
            poiseText.Text = $"Poise: {poise:F1} / {poisemax:F1}";

            //timer max is a bit annoying. you can try and 'find' it by observing the max and resetting on target switch...
            if (poisetimer < 0) { poiseTimerBar.Value = 0; poiseTimerBar.Maximum = 1; } //make sure not to lower maximum below old value
            if (poisetimer > poiseTimerBar.Maximum) { poiseTimerBar.Maximum = poisetimer; }
            var timeValToSet = poisetimer < 0 ? 0 : poisetimer > poiseTimerBar.Maximum ? poiseTimerBar.Maximum : poisetimer;
            poiseTimerBar.Value = timeValToSet;
            poiseTimerText.Text = $"Poise reset time: {poisetimer:F1}";

            if (resistsPanel.Visibility == Visibility.Visible)
            {
                var resistNames = new List<string>() { "Poison", "Toxic", "Bleed", "Curse", "Frost" };
                var resistBars = new List<ProgressBar>() { poisonBar, toxicBar, bleedBar, curseBar, frostBar };
                var resistText = new List<TextBlock>() { poisonText, toxicText, bleedText, curseText, frostText };

                for (int i = 0; i < resistNames.Count; i++)
                {
                    var statInd = DS3Process.TargetInfo.POISON + i * 2;
                    var statIndMax = statInd + 1;
                    var statAmount = _process.getSetTargetInfo(statInd);
                    var statMax = _process.getSetTargetInfo(statIndMax);
                    var statBar = resistBars[i];
                    var statText = resistText[i];
                    var statName = resistNames[i];

                    if (statBar.Value > statMax) { statBar.Value = 0; }
                    statBar.Maximum = statMax;
                    statBar.Value = statAmount > 0 ? statAmount : 0;
                    statText.Text = $"{statName}: {(int)statAmount} / {(int)statMax}";
                }
            }
        }



        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Dispose(true);
        }

        private void colMeshAOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.COL_MESH_MAIN);
            _process.freezeOn(DS3Process.DebugOpts.DISABLE_MAP);
        }

        private void colMeshAOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.COL_MESH_MAIN);
            _process.offAndUnFreeze(DS3Process.DebugOpts.DISABLE_MAP);
        }

        private void colMeshBOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.COL_MESH_VISUAL);
            _process.freezeOn(DS3Process.DebugOpts.DISABLE_MAP);
        }

        private void colMeshBOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.COL_MESH_VISUAL);
            _process.offAndUnFreeze(DS3Process.DebugOpts.DISABLE_MAP);
        }

        private void changeMeshColours(object sender, RoutedEventArgs e)
        {
            _process.cycleMeshColours();
        }

        private void charMeshOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.CHARACTER_MESH);
            _process.freezeOn(DS3Process.DebugOpts.ALL_CHRS_DBG_DRAW_FLAG);
        }

        private void charMeshOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.CHARACTER_MESH);
            _process.offAndUnFreeze(DS3Process.DebugOpts.ALL_CHRS_DBG_DRAW_FLAG);
        }

        private void charModelHideOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.DISABLE_CHARACTER);
        }

        private void charModelHideOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.DISABLE_CHARACTER);
        }

        private void hitboxOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.HITBOX_VIEW);
        }

        private void hitboxOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.HITBOX_VIEW);
        }

        private void impactOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.IMPACT_VIEW);
        }

        private void impactOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.IMPACT_VIEW);
        }

        private void noDeathOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.NO_DEATH);
        }
        private void noDeathOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.NO_DEATH);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    if (_process != null)
                    {
                        _process.Dispose();
                        _process = null;
                    }
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer = null;
                    }


                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void doQuitout(object sender, RoutedEventArgs e)
        {
            _process.enableOpt(DS3Process.DebugOpts.INSTANT_QUITOUT);
        }

        private void oneHPOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.ONE_HP);
        }

        private void oneHPOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.ONE_HP);
        }

        private void maxHPOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.MAX_HP);
        }

        private void maxHPOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.MAX_HP);
        }

        private void noAIOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.DISABLE_AI);
        }

        private void noAIOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.DISABLE_AI);
        }

        private void noStamOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.NO_STAM);
        }

        private void noStamOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.NO_STAM);
        }

        private void noFPOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.NO_FP);
        }

        private void noFPOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.NO_FP);
        }

        private void noGoodsOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.NO_GOODS_CONSUM);
        }

        private void noGoodsOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.NO_GOODS_CONSUM);
        }

        private void oneShotOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.ONE_SHOT);
        }

        private void oneShotOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.ONE_SHOT);
        }


        private void repeatActionOn(object sender, RoutedEventArgs e)
        {
            _process.setEnemyRepeatActionPatch(true);
        }

        private void repeatActionOff(object sender, RoutedEventArgs e)
        {
            _process.setEnemyRepeatActionPatch(false);
        }

        private void steamInputEnumDisableOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.DISABLE_STEAM_INPUT_ENUM);
        }

        private void steamInputEnumDisableOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.DISABLE_STEAM_INPUT_ENUM);
        }

        private void chkEnableTarget_Checked(object sender, RoutedEventArgs e)
        {
            if (!_process.installTargetHook())
            {
                MessageBox.Show("Could not install hook. This could be because a Cheat Engine table has already installed its own hook. Restart the game and try again.", "Sadge", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _hooked = true;
            targetPanel.Opacity = 1;
            targetPanel.IsEnabled = true;
            targetPanel.Visibility = Visibility.Visible;
        }

        private void chkEnableTarget_Unchecked(object sender, RoutedEventArgs e)
        {
            _process.cleanUpTargetHook();
            _hooked = false;
            targetPanel.Opacity = 1;
            targetPanel.IsEnabled = true;
            targetPanel.Visibility = Visibility.Collapsed;
        }

        private void stayOnTop(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void dontStayOnTop(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }

        private void targetHpFreezeOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.TARGET_HP);
        }

        private void targetHpFreezeOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.TARGET_HP);
        }

        private void killTarget(object sender, RoutedEventArgs e)
        {
            _process.getSetTargetInfo(DS3Process.TargetInfo.HP, 0);
        }

        void setHPStr(string str)
        {
            if (string.IsNullOrEmpty(str)) { return; }
            int setAmount = 0;
            if (str.EndsWith("%"))
            {
                str = str.Substring(0, str.Length - 1);
                if (!float.TryParse(str, out var perc)) { return; }
                var maxHP = _process.getSetTargetInfo(DS3Process.TargetInfo.MAX_HP);
                setAmount = (int)(maxHP * perc / 100.0f);
            }
            else
            {
                if (!int.TryParse(str, out var hp)) { return; }
                setAmount = hp;
            }
            _process.getSetTargetInfo(DS3Process.TargetInfo.HP, setAmount);
        }

        private void setHP(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (null == button) { return; }
            var str = button.Content as string;
            setHPStr(str);
        }

        private void setHPCustom(object sender, RoutedEventArgs e)
        {
            var amount = Microsoft.VisualBasic.Interaction.InputBox("Enter hp value (or put % for percentage)", "HP", "420");
            setHPStr(amount);
        }

        private void eventDrawOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.EVENT_DRAW);
        }

        private void eventDrawOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.EVENT_DRAW);
        }

        private void eventStopOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.EVENT_STOP);
        }

        private void eventStopOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.EVENT_STOP);
        }

        private void hiddenDebugMenuOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.HIDDEN_DEBUG_MENU);
        }

        private void hiddenDebugMenuOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.HIDDEN_DEBUG_MENU);
        }

        private void enemyTargetingOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.ALL_DEBUG_DRAWING);
            _process.freezeOn(DS3Process.DebugOpts.ENEMY_TARGETING_A);
            _process.freezeOn(DS3Process.DebugOpts.ENEMY_TARGETING_B);
        }

        private void enemyTargetingOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.ENEMY_TARGETING_A);
            _process.offAndUnFreeze(DS3Process.DebugOpts.ENEMY_TARGETING_B);
            _process.offAndUnFreeze(DS3Process.DebugOpts.ALL_DEBUG_DRAWING); //it might be fine to leave this one but w/e
        }

        private void freeCamOn(object sender, RoutedEventArgs e)
        {
            if (_freeCamFirstActivation || Keyboard.IsKeyDown(Key.LeftShift))
            {
                _freeCamFirstActivation = false;
                _process.moveCamToPlayer();
            }
            _process.freezeOn(DS3Process.DebugOpts.FREE_CAMERA);
        }

        private void freeCamOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.FREE_CAMERA);
        }

        private void restPlayerControlOn(object sender, RoutedEventArgs e)
        {
            _process.doFreeCamPlayerControlPatch();
        }

        private void restPlayerControlOff(object sender, RoutedEventArgs e)
        {
            _process.undoFreeCamPlayerControlPatch();
        }

        private void soundViewOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.SOUND_VIEW);
        }

        private void soundViewOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.SOUND_VIEW);
        }

        private void setGameSpeed(object sender, RoutedEventArgs e)
        {
            var existing = _process.getSetGameSpeed();
            var newVal = Microsoft.VisualBasic.Interaction.InputBox("Enter game speed multiplier", "Game Speed", existing.ToString());

            if (!float.TryParse(newVal, out var newValFlt)) { return; } // if you put junk, return
            if (string.IsNullOrEmpty(newVal) && newValFlt <= 0) { return; } // if you put non-junk but its stupid, return

            _process.getSetGameSpeed(newValFlt);
        }

        private void noGravOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.NO_GRAVITY);
        }

        private void noGravOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.NO_GRAVITY);
        }

        private void noClipOn(object sender, RoutedEventArgs e)
        {
            noclipService.EnableNoClip();


        }

        private void noClipOff(object sender, RoutedEventArgs e)
        {

            noclipService.disableNoClip();

        }



        private void savePos(object sender, RoutedEventArgs e)
        {
            savedPos = _process.getSetPlayerLocalCoords();
            restorePosButton.IsEnabled = true;
        }

        private void restorePos(object sender, RoutedEventArgs e)
        {
            if (savedPos.HasValue)
            {
                _process.getSetPlayerLocalCoords(savedPos);
            }
        }

        private void noDeathAllOn(object sender, RoutedEventArgs e)
        {
            _process.freezeOn(DS3Process.DebugOpts.ALL_CHR_NO_DEATH);
        }

        private void noDeathAllOff(object sender, RoutedEventArgs e)
        {
            _process.offAndUnFreeze(DS3Process.DebugOpts.ALL_CHR_NO_DEATH);
        }

        /*private void emberOn(object sender, RoutedEventArgs e)
        {

        }

        private void emberOff(object sender, RoutedEventArgs e)
        {

        }*/

        private void instantDeath(object sender, RoutedEventArgs e)
        {
            _process.getSetPlayerHP(0);
        }

        //hotkey implementation kept at the end. basically a copy/paste from ertool. would be nice to share the code.

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        const int WM_HOTKEY = 0x0312;


        [Flags]
        public enum Modifiers
        {
            NO_MOD = 0x0000,
            ALT = 0x0001,
            CTRL = 0x0002,
            SHIFT = 0x0004,
            WIN = 0x0008
        }

        public enum HOTKEY_ACTIONS
        {
            QUITOUT = 1,
            TELEPORT_SAVE,
            TELEPORT_LOAD,
            KILL_TARGET, FREEZE_TARGET_HP,
            COL_MESH_A, COL_MESH_B, COL_MESH_CYCLE,
            CHAR_MESH, HIDE_MODELS,
            HITBOX_A, HITBOX_B,
            NO_DEATH, ALL_NO_DEATH,
            ONE_HP, MAX_HP, DIE,
            DISABLE_AI, REPEAT_ENEMY_ACTIONS,
            INF_STAM, INF_FP, INF_CONSUM,
            NO_GRAVITY,
            SOUND_VIEW, TARGETING_VIEW,
            EVENT_VIEW, EVENT_STOP,
            FREE_CAMERA, FREE_CAMERA_CONTROL, NO_CLIP,
            DISABLE_STEAM_INPUT_ENUM,
            GAME_SPEED_50PC, GAME_SPEED_75PC, GAME_SPEED_100PC, GAME_SPEED_150PC, GAME_SPEED_200PC, GAME_SPEED_300PC, GAME_SPEED_500PC, GAME_SPEED_1000PC,
            CINDER_SWORD, CINDER_STAFF, CINDER_LANCE, CINDER_CURVED, CINDER_GWYN,
        }


        const string hotkeyFileName = "ds3tool_hotkeys.txt";

        Dictionary<string, Modifiers> modMap = new Dictionary<string, Modifiers>();
        Dictionary<string, HOTKEY_ACTIONS> actionMap = new Dictionary<string, HOTKEY_ACTIONS>();
        Dictionary<string, Key> keyMap = new Dictionary<string, Key>();

        Dictionary<int, List<HOTKEY_ACTIONS>> registeredHotkeys = new Dictionary<int, List<HOTKEY_ACTIONS>>();

        static string getHotkeyFileAppData()
        {
            return Utils.getFnameInAppdata(hotkeyFileName, "DS3Tool");
        }
        static string hotkeyFile()
        {//local file can override (an older tool version used a local file)
            if (File.Exists(hotkeyFileName)) { return hotkeyFileName; }
            return getHotkeyFileAppData();
        }


        private void hotkeySetup(object sender, RoutedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                clearRegisteredHotkeys();
                if (!parseHotkeys())
                {
                    MessageBox.Show("Failed to parse hotkey file.");
                }
                else
                {
                    MessageBox.Show(registeredHotkeys.Count + " hotkeys registered.");
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                var psi = new ProcessStartInfo(System.IO.Path.GetDirectoryName(getHotkeyFileAppData()));
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                var res = MessageBox.Show("Reset hotkey file?", "Reset hotkeys", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    generateDefaultHotkeyFile();
                    var psi = new ProcessStartInfo(hotkeyFile());
                    psi.UseShellExecute = true;
                    Process.Start(psi);
                }
            }
            else
            {
                if (!File.Exists(hotkeyFile())) { generateDefaultHotkeyFile(); }
                var psi = new ProcessStartInfo(hotkeyFile());
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
        }

        void setUpMapsForHotkeys()
        {
            foreach (var mod in Enum.GetValues(typeof(Modifiers)))
            {
                modMap[mod.ToString()] = (Modifiers)mod;
            }
            foreach (var act in Enum.GetValues(typeof(HOTKEY_ACTIONS)))
            {
                actionMap[act.ToString()] = (HOTKEY_ACTIONS)act;
            }
            foreach (var k in Enum.GetValues(typeof(Key)))
            {
                keyMap[k.ToString()] = (Key)k;
            }
        }

        bool parseHotkeys(string linesStr = null)
        {
            try
            {
                string[] lines;
                if (!string.IsNullOrEmpty(linesStr))
                {
                    lines = linesStr.Split('\r', '\n');
                }
                else
                {
                    lines = File.ReadAllLines(hotkeyFile());
                }

                var hotkeyMap = new Dictionary<(Key, Modifiers), List<HOTKEY_ACTIONS>>();
                foreach (var line in lines)
                {
                    if (line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("//")) { continue; }
                    var modifiers = Modifiers.NO_MOD;
                    HOTKEY_ACTIONS? action = null;
                    Key? hotkey = null;
                    var spl = line.Split(' ');
                    foreach (var s in spl)
                    {
                        if (modMap.ContainsKey(s)) { modifiers |= modMap[s]; }
                        if (actionMap.ContainsKey(s)) { action = actionMap[s]; }
                        if (keyMap.ContainsKey(s)) { hotkey = keyMap[s]; }
                    }
                    if (action.HasValue && hotkey.HasValue)
                    {
                        var key = (hotkey.Value, modifiers);
                        if (!hotkeyMap.ContainsKey(key))
                        {
                            hotkeyMap.Add(key, new List<HOTKEY_ACTIONS>());
                        }
                        hotkeyMap[key].Add(action.Value);
                    }
                }

                int i = 0;
                foreach (var kvp in hotkeyMap)
                {
                    registeredHotkeys.Add(i, kvp.Value);
                    RegisterHotKey(new WindowInteropHelper(this).Handle, i, (uint)kvp.Key.Item2, (uint)KeyInterop.VirtualKeyFromKey(kvp.Key.Item1));
                    var debugStr = $"Hotkey {i} set: {kvp.Key} ->";
                    foreach (var act in kvp.Value)
                    {
                        debugStr += " " + act.ToString();
                    }
                    Utils.debugWrite(debugStr);
                    i++;
                }
                btnHotkeys.Foreground = registeredHotkeys.Count > 0 ? Brushes.Blue : Brushes.Black;
                return true;
            }
            catch { }
            return false;
        }

        void clearRegisteredHotkeys()
        {
            foreach (var h in registeredHotkeys)
            {
                UnregisterHotKey(new WindowInteropHelper(this).Handle, h.Key);
            }
            registeredHotkeys.Clear();
        }

        string generateDefaultHotkeyFile(bool writeOut = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine(";Set hotkeys below. Some example hotkeys are provided.");
            sb.AppendLine(";If you don't want to use hotkeys, just remove all the hotkeys listed.");
            sb.AppendLine(";Restart DS3Tool after updating the hotkeys, or ctrl+click the hotkeys button.");
            sb.AppendLine(";Lines starting with ; are ignored.");
            sb.AppendLine(";All text is case-sensitive.");
            sb.AppendLine(";Note that these are global hotkeys and may conflict with other applications. If a given key doesn't work, try using a modifier. (Eg. F12 may not work but CTRL F12 should.) Some of the more obscure keys may also not work.");
            sb.AppendLine(";To generate a fresh hotkey file, alt+click the hotkey setup button.");
            sb.Append(";Valid actions:");
            foreach (var kvp in actionMap) { sb.Append(" " + kvp.Key); }
            sb.AppendLine();
            sb.Append(";Valid modifier keys:");
            foreach (var kvp in modMap) { sb.Append(" " + kvp.Key); }
            sb.AppendLine();
            sb.Append(";Valid keys:");
            foreach (var kvp in keyMap) { sb.Append(" " + kvp.Key); }
            sb.AppendLine();

            sb.AppendLine(Modifiers.CTRL.ToString() + " " + Key.Z.ToString() + " " + HOTKEY_ACTIONS.QUITOUT.ToString());
            sb.AppendLine(Modifiers.CTRL.ToString() + " " + Key.C.ToString() + " " + HOTKEY_ACTIONS.TELEPORT_SAVE.ToString());
            sb.AppendLine(Modifiers.CTRL.ToString() + " " + Key.V.ToString() + " " + HOTKEY_ACTIONS.TELEPORT_LOAD.ToString());
            sb.AppendLine(Modifiers.CTRL.ToString() + " " + Key.K.ToString() + " " + HOTKEY_ACTIONS.KILL_TARGET.ToString());

            if (writeOut) { File.WriteAllText(hotkeyFile(), sb.ToString()); }
            return sb.ToString();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                Utils.debugWrite($"Got hotkey id {id}");
                if (!registeredHotkeys.ContainsKey(id))
                {
                    Utils.debugWrite($"Invalid hotkey {id}");
                }
                else
                {
                    var actList = registeredHotkeys[id];
                    foreach (var act in actList)
                    {
                        Utils.debugWrite($"Doing action {act}");
                        doAct(act);
                    }
                }
            }

            return IntPtr.Zero;
        }

        void doAct(HOTKEY_ACTIONS act)
        {
            switch (act)
            {
                case HOTKEY_ACTIONS.QUITOUT: doQuitout(null, null); break;
                case HOTKEY_ACTIONS.TELEPORT_SAVE: savePos(null, null); break;
                case HOTKEY_ACTIONS.TELEPORT_LOAD: restorePos(null, null); break;
                case HOTKEY_ACTIONS.KILL_TARGET: killTarget(null, null); break;
                case HOTKEY_ACTIONS.FREEZE_TARGET_HP: chkFreezeHP.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.COL_MESH_A: chkColMeshA.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.COL_MESH_B: chkColMeshB.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.COL_MESH_CYCLE: changeMeshColours(null, null); break;
                case HOTKEY_ACTIONS.CHAR_MESH: chkCharMesh.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.HIDE_MODELS: chkHideModels.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.HITBOX_A: chkHitboxA.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.HITBOX_B: chkHitboxB.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.NO_DEATH: chkPlayerNoDeath.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.ALL_NO_DEATH: chkAllNoDeath.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.ONE_HP: chkOneHP.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.MAX_HP: chkMaxHP.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.DIE: instantDeath(null, null); break;
                case HOTKEY_ACTIONS.DISABLE_AI: chkDisableAI.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.REPEAT_ENEMY_ACTIONS: chkRepeatAction.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.INF_STAM: chkInfStam.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.INF_FP: chkInfFP.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.INF_CONSUM: chkInfConsum.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.NO_GRAVITY: chkPlayerNoGrav.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.SOUND_VIEW: chkSoundView.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.TARGETING_VIEW: chkTargetingView.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.EVENT_VIEW: chkEventView.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.EVENT_STOP: chkEventStop.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.FREE_CAMERA: chkFreeCam.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.FREE_CAMERA_CONTROL: chkFreeCamControl.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.NO_CLIP: chkNoClip.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.DISABLE_STEAM_INPUT_ENUM: chkSteamInputEnum.IsChecked ^= true; break;
                case HOTKEY_ACTIONS.GAME_SPEED_50PC: _process.getSetGameSpeed(0.5f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_75PC: _process.getSetGameSpeed(0.75f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_100PC: _process.getSetGameSpeed(1.0f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_150PC: _process.getSetGameSpeed(1.5f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_200PC: _process.getSetGameSpeed(2.0f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_300PC: _process.getSetGameSpeed(3.0f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_500PC: _process.getSetGameSpeed(5.0f); break;
                case HOTKEY_ACTIONS.GAME_SPEED_1000PC: _process.getSetGameSpeed(10.0f); break;
                case HOTKEY_ACTIONS.CINDER_SWORD:   if (_process.GetSetTargetEnemyID() == CINDER_ENEMY_ID) { _cinderManager.SetPhase(0, chkLockPhase.IsChecked ?? false); } break;
                case HOTKEY_ACTIONS.CINDER_LANCE:   if (_process.GetSetTargetEnemyID() == CINDER_ENEMY_ID) { _cinderManager.SetPhase(1, chkLockPhase.IsChecked ?? false); } break;
                case HOTKEY_ACTIONS.CINDER_CURVED:  if (_process.GetSetTargetEnemyID() == CINDER_ENEMY_ID) { _cinderManager.SetPhase(2, chkLockPhase.IsChecked ?? false); } break;
                case HOTKEY_ACTIONS.CINDER_STAFF:   if (_process.GetSetTargetEnemyID() == CINDER_ENEMY_ID) { _cinderManager.SetPhase(3, chkLockPhase.IsChecked ?? false); } break;
                case HOTKEY_ACTIONS.CINDER_GWYN:    if (_process.GetSetTargetEnemyID() == CINDER_ENEMY_ID) { _cinderManager.SetPhase(4, chkLockPhase.IsChecked ?? false); } break;


                default: Utils.debugWrite("Action not handled: " + act.ToString()); break;
            }
        }

        void hotkeyInit()
        {//called from main window loaded
            try
            {
                //register for message passing
                var source = PresentationSource.FromVisual(this as Visual) as HwndSource;
                if (null == source) { Utils.debugWrite("Could not make hwnd source"); }
                source.AddHook(WndProc);

                //hotkeys
                setUpMapsForHotkeys();

                if (File.Exists(hotkeyFileName) && !File.Exists(getHotkeyFileAppData()))
                {
                    MessageBox.Show("Hotkey mapping will be moved to AppData. Shift-click Hotkey Setup if you need to access this folder.");
                    File.Move(hotkeyFileName, getHotkeyFileAppData());
                }

                if (File.Exists(hotkeyFile()))
                {
                    if (!parseHotkeys())
                    {
                        MessageBox.Show("Failed to parse hotkey file.");
                    }
                }
                else
                {//none by default
                }
            }
            catch (Exception ex) { Utils.debugWrite(ex.ToString()); }
        }

        private void OpenSpawnItem(object sender, RoutedEventArgs e)
        {
            var itemSpawn = new ItemSpawn(_process, ItemDictionary);
            itemSpawn.Owner = this;
            itemSpawn.Show();
        }

        private void EditStats(object sender, RoutedEventArgs e)
        {
            var stats = _process.GetSetPlayerStats().Where(item => item.Item1 != "SOULS").ToList();

            var editor = new StatsEditor(stats, (x) =>
            {
                _process.GetSetPlayerStats(x);
            });
            editor.Owner = this;
            editor.Show();
        }

        private void EditSouls(object sender, RoutedEventArgs e)
        {
            var stats = _process.GetSetPlayerStats().Where(item => item.Item1 == "SOULS").ToList();

            var editor = new StatsEditor(stats, (x) =>
            {
                _process.GetSetPlayerStats(x);
            });
            editor.Owner = this;
            editor.Show();
        }

        private void EditNewGame(object sender, RoutedEventArgs e)
        {
            var stats = new List<(string, int)>();

            int ngLevel = _process.GetSetNewGameLevel();

            stats.Add(("NG+ Cycle", ngLevel));

            var editor = new StatsEditor(stats, (x) =>
            {
                _process.GetSetNewGameLevel(x[0].Item2);
            });
            editor.Owner = this;
            editor.Show();
        }

        private void dockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DockPanel dockPanel && dockPanel.Tag is StackPanel stackPanel)
            {
                stackPanel.Visibility = stackPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                FixPanelArrows(dockPanel, stackPanel.Visibility.ToString());
            }
        }

        private void FixPanelArrows(DockPanel panel, string visibility)
        {
            var textBox = panel.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBox != null)
            {
                textBox.Text = visibility == Visibility.Visible.ToString() ?
                                                        textBox.Text.Substring(0, textBox.Text.Length - 1) + "▼" :
                                                        textBox.Text.Substring(0, textBox.Text.Length - 1) + "▲";
            }
        }

        private void ToggleCollapse(object sender, RoutedEventArgs e)
        {
            var newVisibility = panelsCollapsed ? Visibility.Visible : Visibility.Collapsed;

            foreach (UIElement element in mainPanel.Children)
            {
                if (element is DockPanel dockPanel)
                {
                    FixPanelArrows(dockPanel, newVisibility.ToString());
                }
                if (element is StackPanel stackPanel && stackPanel.Name != null && stackPanel.Visibility != newVisibility)
                {
                    dockPanel_MouseLeftButtonDown(stackPanel, null);
                    stackPanel.Visibility = newVisibility;
                }
            }

            panelsCollapsed = !panelsCollapsed;

            if (sender is Button button)
            {
                button.Content = newVisibility == Visibility.Visible ? "▼" : "▲";
            }
        }

        private void UnlockBonfire(object sender, RoutedEventArgs e)
        {
            var unlockBonFire = new BonfireUnlock(_process, BonfireDictionary);
            unlockBonFire.Owner = this;
            unlockBonFire.Show();
        }


        private void OnPhaseButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (int.TryParse(button.Tag?.ToString(), out int phaseIndex))
            {
                _cinderManager.SetPhase(phaseIndex, chkLockPhase.IsChecked ?? false);
            }
            else
            {
                Debug.WriteLine($"Failed to parse phase index from button tag: {button.Tag}");
            }
        }

        private void OnCastSoulMass(object sender, RoutedEventArgs e)
        {
            _cinderManager.CastSoulMass();
        }

        private void OnLockPhaseChanged(object sender, RoutedEventArgs e)
        {
            _cinderManager.TogglePhaseLock(chkLockPhase.IsChecked ?? false);
        }

        private void OnEndlessSoulmassChanged(object sender, RoutedEventArgs e)
        {
            if (chkEndlessSoulmass.IsChecked == true)
            {
               _cinderManager.EnableEndlessSoulmass();
            }
            else
            {
                _cinderManager.DisableEndlessSoulmass();
            }
        }


        int? lastSetHP = null;

        private void btnSetPlayerHP_Click(object sender, RoutedEventArgs e)
        {
            var existing = _process.getSetPlayerHP();
            var newVal = Microsoft.VisualBasic.Interaction.InputBox("Enter HP", "Set Player HP", existing.ToString());
            if (string.IsNullOrEmpty(newVal)) { return; }
            if (int.TryParse(newVal, out var newValInt))
            {
                _process.getSetPlayerHP(newValInt);
                lastSetHP = newValInt;
            }
        }

        private void disableHotkeys(object sender, RoutedEventArgs e)
        {
            btnHotkeys.IsEnabled = _hotkeysEnabled = false;
            if (!_initializing)
            {
                MessageBox.Show("For this to take effect, you must restart DS3Tool.\nYour preference will be saved.", "BlameLank");
            }
        }

        private void enableHotkeys(object sender, RoutedEventArgs e)
        {
            btnHotkeys.IsEnabled = _hotkeysEnabled = true;
            if (!_initializing)
            {
                MessageBox.Show("For this to take effect, you must restart DS3Tool.\nYour preference will be saved.", "BlameLank");
            }
        }
    }
}
