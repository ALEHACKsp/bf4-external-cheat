using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using InputManager;

namespace BF4_Private_By_Tejisav
{
    public partial class Overlay : Form
    {
        public enum MainMenuList
        {
            Save_Settings,
            VISUALS,
            AIMBOT,
            REMOVALS,
            MISC
        }

        public enum VisualMenuList
        {
            ESP_BACK,
            ESP,
            ESP_Box,
            ESP_BoxType,
            ESP_Distance,
            ESP_Health,
            ESP_Line,
            ESP_Name,
            ESP_Bone,
            ESP_Enemy,
            ESP_Vehicle,
            Radar,
            Info,
            Overheat,
            NoSky,
            NoFog
        }

        public enum AimbotMenuList
        {
            AIM_BACK,
            AIM,
            AIM_AutoAim,
            AIM_AutoShoot,
            AIM_AimAtAll,
            AIM_StickTarget,
            AIM_Type,
            AIM_Location,
            AIM_Fov,
            AIM_Key,
            AIM_Vehicle,
            AIM_Vehicle_Key,
            AIM_Visible_Check,
            AIM_Driver_First
        }

        public enum RemovalMenuList
        {
            REMOVAL_BACK,
            RCS,
            NoSpread,
            NoGravity,
            NoBreath
        }
        public enum MiscMenuList
        {
            
            MISC_BACK,
            BulletsPerShell,
            VehicleBulletsPershell,
            BulletsPerShot,
            VehicleBulletsPerShot,
            OneHitKill,
            VehicleOneHitKill,
            RateOfFire,
            FireRate,
            JetSpeed,
            Teleport,
            DestroyFriendly,
            Unlock_All
        }

        string SettingsPath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Settings.ini";

        // Process
        private Process process = null;
        private Thread updateStream = null, windowStream = null, AimbotStream = null, AutoShootStream = null;

        // Game Data
        private List<Player> players = null;
        private Player localPlayer = null;
        private Matrix viewProj, m_ViewMatrixInverse;
        //private float real_gravity;
        private int spectatorCount = 0;
        private bool Menu_Enabled;
        private int RadarScale = 1;
        private int[] Aim_Keys;
        private bool IsTargetting;
        private string LastTargetName;
        private int WantedJetSpeed = 299;
        private int MaxJetSpeed = 302;
        private float[] FiringRate, SpreadValue;
        private int WeaponID = 0;
        private int NotificationTimer = 0;
        private string Notificationstring;
        private Bitmap RadarBitmap;
        private Dictionary<string, int> RadarIcons;
        string[] Jets;
        private bool Teleported, WaitTpPatch;
        private int TpPatchTimer = 0;

        // Keys Control
        private KeysManager manager;

        // Menu Data
        private MainMenuItems MainMenuItems;
        private VisualMenuItems VisualMenuItems;
        private AimbotMenuItems AimbotMenuItems;
        private RemovalMenuItems RemovalMenuItems;
        private MiscMenuItems MiscMenuItems;
        private int selectedMainMenuIndex = 0;
        private int selectedVisualMenuIndex = 0;
        private int selectedAimbotMenuIndex = 0;
        private int selectedRemovalMenuIndex = 0;
        private int selectedMiscMenuIndex = 0;

        // Handle
        private IntPtr handle;

        // Color
        private Color enemyColor = new Color(255, 0, 0, 200),
            enemyColorVisible = new Color(255, 255, 0, 220),
            enemyColorVehicle = new Color(255, 129, 72, 200),
            enemySkeletonColor = new Color(245, 114, 0, 255),
            friendlyColor = new Color(0, 255, 0, 200),
            friendlyColorVehicle = new Color(64, 154, 200, 255),
            friendSkeletonColor = new Color(46, 228, 213, 255);

        // SharpDX
        private WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush solidColorBrush;
        private Factory factory;
        private bool IsResize = false;
        private bool IsGameWindowSelected = false;

        // SharpDX Font
        private TextFormat fontLarge, fontSmall;
        private FontFactory fontFactory;
        private const string fontFamily = "Consolas";//"Calibri";
        private const float fontSizeLarge = 18.0f;
        private const float fontSizeSmall = 14.0f;

        // Screen Size
        private Rectangle rect;

        // Init
        public Overlay(Process process)
        {
            this.process = process;
            handle = Handle;

            int initialStyle = Managed.GetWindowLong(Handle, -20);
            Managed.SetWindowLong(Handle, -20, initialStyle | 0x80000 | 0x20);

            Managed.SetLayeredWindowAttributes(Handle, (uint)0, 255, Managed.LWA_ALPHA);
            IntPtr HWND_TOPMOST = new IntPtr(-1);
            const uint SWP_NOSIZE = 0x0001;
            const uint SWP_NOMOVE = 0x0002;
            const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

            Managed.SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            OnResize(null);
            Managed.SetLayeredWindowAttributes(Handle, (uint)0, 255, Managed.LWA_ALPHA);

            InitializeComponent();
        }

        // Set window style
        protected override void OnResize(EventArgs e)
        {
            int[] margins = new int[] { 0, 0, rect.Width, rect.Height };
            Managed.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        // INIT
        private void DrawWindow_Load(object sender, EventArgs e)
        {
            TopMost = true;
            Visible = true;
            FormBorderStyle = FormBorderStyle.None;
            //this.WindowState = FormWindowState.Maximized;
            Width = rect.Width;
            Height = rect.Height;

            // Window name
            Name = Process.GetCurrentProcess().ProcessName + "~Overlay";
            Text = Process.GetCurrentProcess().ProcessName + "~Overlay";

            // Init factory
            factory = new Factory();
            fontFactory = new FontFactory();

            // Render settings
            renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = Handle,
                PixelSize = new Size2(rect.Width, rect.Height),
                PresentOptions = PresentOptions.None
            };

            // Init device
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            // Init brush
            solidColorBrush = new SolidColorBrush(device, Color.White);

            // Init font's
            fontLarge = new TextFormat(fontFactory, fontFamily, fontSizeLarge);
            fontSmall = new TextFormat(fontFactory, fontFamily, fontSizeSmall);

            // Open process
            RPM.OpenProcess(process.Id);

            // Init player array
            players = new List<Player>();
            localPlayer = new Player();

            // Radar RadarBitmap
            RadarBitmap = new Bitmap(device, LoadBitmap(Properties.Resources.IconsDX));
            RadarIcons = new Dictionary<string, int>();
            RadarIcons.Add("T99", 10);
            RadarIcons.Add("M1ABRAMS", 10);
            RadarIcons.Add("T90", 10);
            RadarIcons.Add("LAV25", 4);
            RadarIcons.Add("ZBD09", 4);
            RadarIcons.Add("AME_BTR90", 4);
            RadarIcons.Add("Z11", 11);
            RadarIcons.Add("AH6", 11);
            RadarIcons.Add("KA60", 13);
            RadarIcons.Add("UH1Y", 13);
            RadarIcons.Add("Z9", 13);
            RadarIcons.Add("HIMARS", 7);
            RadarIcons.Add("AAV", 7);
            RadarIcons.Add("MI28", 12);
            RadarIcons.Add("AH1Z", 12);
            RadarIcons.Add("Z10", 12);
            RadarIcons.Add("9K22", 5);
            RadarIcons.Add("LAVAD", 5);
            RadarIcons.Add("PGZ95", 5);
            RadarIcons.Add("A10", 9);
            RadarIcons.Add("Q5", 9);
            RadarIcons.Add("SU39", 9);
            RadarIcons.Add("AME_F35", 8);
            RadarIcons.Add("J20", 8);
            RadarIcons.Add("PAKFA", 8);
            RadarIcons.Add("DV15", 6);
            RadarIcons.Add("PWC", 6);
            RadarIcons.Add("CB90", 6);

            Jets = new string[] { "A10", "Q5", "SU39", "AME_F35", "J20", "PAKFA" };

            // Init update thread
            updateStream = new Thread(new ParameterizedThreadStart(Update));
            updateStream.Start();

            // Init window thread (resize / position)
            windowStream = new Thread(new ParameterizedThreadStart(SetWindow));
            windowStream.Start();

            AimbotStream = new Thread(() =>
            {
                Thread.Sleep(1);
            });
            AimbotStream.Start();

            AutoShootStream = new Thread(() =>
            {
                Thread.Sleep(1);
            });
            AutoShootStream.Start();

            // Menuitems
            Aim_Keys = new int[]
			{
				1,
				2,
                5,
                6,
				162,
				164
			};

            FiringRate = new float[]
            {
                1000,
                1200,
                1500,
                2000
            };

            SpreadValue = new float[]
            {
                0.00f,
                0.00f,
                0.05f,
                0.10f,
                0.15f,
                0.20f,
                0.25f,
                0.30f,
                0.35f,
                0.40f,
                0.45f,
                0.50f,
                1.00f
            };

            MainMenuItems = new MainMenuItems();
            MainMenuItems.MenuIndex.Add(new StoreMenuItem(false, "[SAVE SETTINGS]", false));
            MainMenuItems.MenuIndex.Add(new StoreMenuItem(false, "VISUALS", false));
            MainMenuItems.MenuIndex.Add(new StoreMenuItem(false, "AIMBOT", false));
            MainMenuItems.MenuIndex.Add(new StoreMenuItem(false, "REMOVALS", false));
            MainMenuItems.MenuIndex.Add(new StoreMenuItem(false, "MISC", false));

            VisualMenuItems = new VisualMenuItems();
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(false, "<BACK", false));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "ESP", false));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Box", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
			{
				"2D",
				"3D"
			}, "-Box Type", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Distance", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Health", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Line", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Name", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Bone", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Enemy Only", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Vehicle", true));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "Radar", false));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "Player Info", false));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(true, "Overheat Info", false));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(false, "NoSky", false));
            VisualMenuItems.MenuIndex.Add(new StoreMenuItem(false, "NoFog", false));

            AimbotMenuItems = new AimbotMenuItems();
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(false, "<BACK", false));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(true, "AIMBOT", false));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(false, "-Auto Aim", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(false, "-Auto Shoot", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-AIM At All", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(false, "-Stick Target", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(1, new string[]
			{
				"Auto",
				"Fov",
				"Dist",
			}, "-Type", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
			{
				"Head",
				"Neck",
				"Chest",
				"Stomach",
				"Spine"
			}, "-Location", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(2, new string[]
			{
				"10%",
				"15%",
				"20%",
				"25%",
				"30%",
				"35%",
				"40%"
			}, "-FOV", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(1, new string[]
			{
                "LButton",
                "RButton",
                "XButton1",
                "XButton2",
                "LCtrl",
				"LAlt"
			}, "-Key", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Vehicle", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(3, new string[]
			{
                "LButton",
                "RButton",
                "XButton1",
                "XButton2",
                "LCtrl",
                "LAlt"
            }, "-Vehicle Key", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Visible Check", true));
            AimbotMenuItems.MenuIndex.Add(new StoreMenuItem(true, "-Driver First", true));

            RemovalMenuItems = new RemovalMenuItems();
            RemovalMenuItems.MenuIndex.Add(new StoreMenuItem(false, "<BACK", false));
            RemovalMenuItems.MenuIndex.Add(new StoreMenuItem(false, "RCS", false));
            RemovalMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
            {
                "Off",
                "0",
                "0.05",
                "0.10",
                "0.15",
                "0.20",
                "0.25",
                "0.30",
                "0.35",
                "0.40",
                "0.45",
                "0.50",
                "1",
            }, "NoSpread", false));
            RemovalMenuItems.MenuIndex.Add(new StoreMenuItem(false, "NoGravity", false));
            RemovalMenuItems.MenuIndex.Add(new StoreMenuItem(false, "NoBreath", false));

            MiscMenuItems = new MiscMenuItems();
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "<BACK", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
			{
				"Off",
				"1",
				"2",
				"3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            }, "Soldier BulletsPerShell", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
            {
                "Off",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            }, "Vehicle BulletsPerShell", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
            {
                "Off",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            }, "Soldier BulletsPerShot", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
            {
                "Off",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
            }, "Vehicle BulletsPerShot", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "Soldier OneHitKill", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "Vehicle OneHitKill", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "Rate Of Fire", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(0, new string[]
			{
				"1000",
				"1200",
				"1500",
				"2000",
			}, "-FireRate", true));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "JetSpeed Controller", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "Teleport", false));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "-Destroy Friendly", true));
            MiscMenuItems.MenuIndex.Add(new StoreMenuItem(false, "Unlock All", false));

            ReadSettings();

            // Init Key Listener
            manager = new KeysManager();
            manager.AddKey(Keys.Insert);
            manager.AddKey(Keys.Left);
            manager.AddKey(Keys.Up);
            manager.AddKey(Keys.Right);
            manager.AddKey(Keys.Down);
            manager.AddKey(Keys.Delete);
            manager.AddKey(Keys.Home);
            //manager.KeyUpEvent += new KeysManager.KeyHandler(KeyUpEvent);
            manager.KeyDownEvent += new KeysManager.KeyHandler(KeyDownEvent);
        }

        // Key Down Event
        private void KeyDownEvent(int keyId, string keyName)
        {
            if ((Keys)keyId == Keys.Insert || Menu_Enabled)
            {
                switch ((Keys)keyId)
                {
                    case Keys.Left:
                        if (selectedMainMenuIndex == 0)
                        {
                            SaveSettings();
                            NotificationTimer = 75;
                            Notificationstring = "SETTINGS SAVED !";
                        }
                        else if (MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled)
                        {
                            if (MainMenuItems[MainMenuList.VISUALS].Enabled)
                            {
                                if (VisualMenuItems.MenuIndex[selectedVisualMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Enabled = !VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Enabled;
                                    if (selectedVisualMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }
                                    else if (selectedVisualMenuIndex == 14)
                                    {
                                        long pWorldRenderSettings = RPM.ReadInt64(Offsets.WorldRenderSettings.GetInstance());
                                        if (RPM.IsValid(pWorldRenderSettings))
                                        {
                                            if (VisualMenuItems[VisualMenuList.NoSky].Enabled)
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyEnable, 0);
                                            }
                                            else
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyEnable, 1);
                                            }
                                        }
                                    }
                                    else if (selectedVisualMenuIndex == 15)
                                    {
                                        long pWorldRenderSettings = RPM.ReadInt64(Offsets.WorldRenderSettings.GetInstance());
                                        if (RPM.IsValid(pWorldRenderSettings))
                                        {
                                            if (VisualMenuItems[VisualMenuList.NoFog].Enabled)
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyFogEnable, 0);
                                            }
                                            else
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyFogEnable, 1);
                                            }
                                        }
                                    }
                                    return;
                                }
                                if (VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Value > 0)
                                {
                                    VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Value--;
                                    return;
                                }
                            }
                            else if (MainMenuItems[MainMenuList.AIMBOT].Enabled)
                            {
                                if (AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Enabled = !AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Enabled;
                                    if (selectedAimbotMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }
                                    return;
                                }
                                if (AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Value > 0)
                                {
                                    AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Value--;
                                    return;
                                }
                            }
                            else if (MainMenuItems[MainMenuList.REMOVALS].Enabled)
                            {
                                if (RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Enabled = !RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Enabled;
                                    if (selectedRemovalMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }
                                    return;
                                }
                                if (RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Value > 0)
                                {
                                    RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Value--;
                                    return;
                                }
                            }
                            else if (MainMenuItems[MainMenuList.MISC].Enabled)
                            {
                                if (MiscMenuItems.MenuIndex[selectedMiscMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Enabled = !MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Enabled;
                                    if (selectedMiscMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }
                                    else if (selectedMiscMenuIndex == 12)
                                    {
                                        long SYNCEDSETTINGS = RPM.ReadInt64(0x1423717C0); //48 8B 1D ? ? ? ? 48 85 DB 75 26 48 8D 15 ? ? ? ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B D8 48 89 05 ? ? ? ? 48 85 C0 0F 84 ? ? ? ? 0F B6 4B 54 88 4C 24 60 0F B6 4B 55 88 4C 24 62
                                        if (SYNCEDSETTINGS != 0)
                                        {
                                            if (MiscMenuItems[MiscMenuList.Unlock_All].Enabled)
                                            {
                                                RPM.WriteByte(SYNCEDSETTINGS + 0x54, 1);
                                            }
                                            else
                                            {
                                                RPM.WriteByte(SYNCEDSETTINGS + 0x54, 0);
                                            }
                                        }
                                    }
                                    return;
                                }
                                if (MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Value > 0)
                                {
                                    MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Value--;
                                    return;
                                }
                            }
                        }
                        /*else
                        {
                            MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                        }*/
                        break;
                    case Keys.Up:
                        if (MainMenuItems[MainMenuList.VISUALS].Enabled)
                        {
                            selectedVisualMenuIndex = ((selectedVisualMenuIndex == 0) ? (VisualMenuItems.MenuIndex.Count - 1) : (selectedVisualMenuIndex - 1));
                        }
                        else if (MainMenuItems[MainMenuList.AIMBOT].Enabled)
                        {
                            selectedAimbotMenuIndex = ((selectedAimbotMenuIndex == 0) ? (AimbotMenuItems.MenuIndex.Count - 1) : (selectedAimbotMenuIndex - 1));
                        }
                        else if (MainMenuItems[MainMenuList.REMOVALS].Enabled)
                        {
                            selectedRemovalMenuIndex = ((selectedRemovalMenuIndex == 0) ? (RemovalMenuItems.MenuIndex.Count - 1) : (selectedRemovalMenuIndex - 1));
                        }
                        else if (MainMenuItems[MainMenuList.MISC].Enabled)
                        {
                            selectedMiscMenuIndex = ((selectedMiscMenuIndex == 0) ? (MiscMenuItems.MenuIndex.Count - 1) : (selectedMiscMenuIndex - 1));
                        }
                        else
                        {
                            selectedMainMenuIndex = ((selectedMainMenuIndex == 0) ? (MainMenuItems.MenuIndex.Count - 1) : (selectedMainMenuIndex - 1));
                        }
                        break;
                    case Keys.Right:
                        if (selectedMainMenuIndex == 0)
                        {
                            SaveSettings();
                            NotificationTimer = 75;
                            Notificationstring = "SETTINGS SAVED !";
                        }
                        else if (MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled)
                        {
                            if (MainMenuItems[MainMenuList.VISUALS].Enabled)
                            {
                                if (VisualMenuItems.MenuIndex[selectedVisualMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Enabled = !VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Enabled;
                                    /*if (selectedVisualMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }*/
                                    if (selectedVisualMenuIndex == 14)
                                    {
                                        long pWorldRenderSettings = RPM.ReadInt64(Offsets.WorldRenderSettings.GetInstance());
                                        if (RPM.IsValid(pWorldRenderSettings))
                                        {
                                            if (VisualMenuItems[VisualMenuList.NoSky].Enabled)
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyEnable, 0);
                                            }
                                            else
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyEnable, 1);
                                            }
                                        }
                                    }
                                    else if (selectedVisualMenuIndex == 15)
                                    {
                                        long pWorldRenderSettings = RPM.ReadInt64(Offsets.WorldRenderSettings.GetInstance());
                                        if (RPM.IsValid(pWorldRenderSettings))
                                        {
                                            if (VisualMenuItems[VisualMenuList.NoFog].Enabled)
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyFogEnable, 0);
                                            }
                                            else
                                            {
                                                RPM.WriteByte(pWorldRenderSettings + Offsets.WorldRenderSettings.m_SkyFogEnable, 1);
                                            }
                                        }
                                    }
                                    return;
                                }
                                if (VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Value < VisualMenuItems.MenuIndex[selectedVisualMenuIndex].ValueNames.Length - 1)
                                {
                                    VisualMenuItems.MenuIndex[selectedVisualMenuIndex].Value++;
                                    return;
                                }
                            }
                            else if (MainMenuItems[MainMenuList.AIMBOT].Enabled)
                            {
                                if (AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Enabled = !AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Enabled;
                                    /*if (selectedAimbotMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }*/
                                    return;
                                }
                                if (AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Value < AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].ValueNames.Length - 1)
                                {
                                    AimbotMenuItems.MenuIndex[selectedAimbotMenuIndex].Value++;
                                    return;
                                }
                            }
                            else if (MainMenuItems[MainMenuList.REMOVALS].Enabled)
                            {
                                if (RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Enabled = !RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Enabled;
                                    /*if (selectedRemovalMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }*/
                                    return;
                                }
                                if (RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Value < RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].ValueNames.Length - 1)
                                {
                                    RemovalMenuItems.MenuIndex[selectedRemovalMenuIndex].Value++;
                                    return;
                                }
                            }
                            else if (MainMenuItems[MainMenuList.MISC].Enabled)
                            {
                                if (MiscMenuItems.MenuIndex[selectedMiscMenuIndex].itemtype == ItemType.Boolean)
                                {
                                    MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Enabled = !MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Enabled;
                                    /*if (selectedMiscMenuIndex == 0)
                                    {
                                        MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                                    }*/
                                    if (selectedMiscMenuIndex == 12)
                                    {
                                        long SYNCEDSETTINGS = RPM.ReadInt64(0x1423717C0); //48 8B 1D ? ? ? ? 48 85 DB 75 26 48 8D 15 ? ? ? ? 48 8B 0D ? ? ? ? E8 ? ? ? ? 48 8B D8 48 89 05 ? ? ? ? 48 85 C0 0F 84 ? ? ? ? 0F B6 4B 54 88 4C 24 60 0F B6 4B 55 88 4C 24 62
                                        if (SYNCEDSETTINGS != 0)
                                        {
                                            if (MiscMenuItems[MiscMenuList.Unlock_All].Enabled)
                                            {
                                                RPM.WriteByte(SYNCEDSETTINGS + 0x54, 1);
                                            }
                                            else
                                            {
                                                RPM.WriteByte(SYNCEDSETTINGS + 0x54, 0);
                                            }
                                        }
                                    }
                                    return;
                                }
                                if (MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Value < MiscMenuItems.MenuIndex[selectedMiscMenuIndex].ValueNames.Length - 1)
                                {
                                    MiscMenuItems.MenuIndex[selectedMiscMenuIndex].Value++;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled = !MainMenuItems.MenuIndex[selectedMainMenuIndex].Enabled;
                        }
                        break;
                    case Keys.Down:
                        if (MainMenuItems[MainMenuList.VISUALS].Enabled)
                        {
                            selectedVisualMenuIndex = ((selectedVisualMenuIndex == VisualMenuItems.MenuIndex.Count - 1) ? 0 : (selectedVisualMenuIndex + 1));
                        }
                        else if (MainMenuItems[MainMenuList.AIMBOT].Enabled)
                        {
                            selectedAimbotMenuIndex = ((selectedAimbotMenuIndex == AimbotMenuItems.MenuIndex.Count - 1) ? 0 : (selectedAimbotMenuIndex + 1));
                        }
                        else if (MainMenuItems[MainMenuList.REMOVALS].Enabled)
                        {
                            selectedRemovalMenuIndex = ((selectedRemovalMenuIndex == RemovalMenuItems.MenuIndex.Count - 1) ? 0 : (selectedRemovalMenuIndex + 1));
                        }
                        else if (MainMenuItems[MainMenuList.MISC].Enabled)
                        {
                            selectedMiscMenuIndex = ((selectedMiscMenuIndex == MiscMenuItems.MenuIndex.Count - 1) ? 0 : (selectedMiscMenuIndex + 1));
                        }
                        else
                        {
                            selectedMainMenuIndex = ((selectedMainMenuIndex == MainMenuItems.MenuIndex.Count - 1) ? 0 : (selectedMainMenuIndex + 1));
                        }
                        break;
                    case Keys.Insert:
                        Menu_Enabled = !Menu_Enabled;
                        break;
                }
            }
            if (MiscMenuItems[MiscMenuList.OneHitKill].Enabled)
            {
                switch ((Keys)keyId)
                {
                    case Keys.Delete:
                        long pGContext1 = RPM.ReadInt64(Offsets.ClientGameContext.GetInstance());
                        if (!RPM.IsValid(pGContext1))
                            return;

                        long pPlayerManager1 = RPM.ReadInt64(pGContext1 + Offsets.ClientGameContext.m_pPlayerManager);
                        if (!RPM.IsValid(pPlayerManager1))
                            return;

                        long pLocalPlayer1 = RPM.ReadInt64(pPlayerManager1 + Offsets.ClientPlayerManager.m_pLocalPlayer);
                        if (!RPM.IsValid(pLocalPlayer1))
                            return;

                        long pLocalSoldier1 = RPM.ReadInt64(pLocalPlayer1 + Offsets.ClientPlayer.m_pControlledControllable);
                        if (!RPM.IsValid(pLocalSoldier1))
                            return;

                        long pClientWeaponComponent1 = RPM.ReadInt64(pLocalSoldier1 + Offsets.ClientSoldierEntity.m_soldierWeaponsComponent);
                        if (RPM.IsValid(pClientWeaponComponent1))
                        {
                            long pWeaponHandle1 = RPM.ReadInt64(pClientWeaponComponent1 + Offsets.ClientSoldierWeaponsComponent.m_handler);
                            Int32 ActiveSlot1 = RPM.ReadInt32(pClientWeaponComponent1 + Offsets.ClientSoldierWeaponsComponent.m_activeSlot);

                            if (RPM.IsValid(pWeaponHandle1))
                            {
                                long pSoldierWeapon1 = RPM.ReadInt64(pWeaponHandle1 + ActiveSlot1 * 0x8);
                                if (RPM.IsValid(pSoldierWeapon1))
                                {
                                    long pWeapon1 = RPM.ReadInt64(pSoldierWeapon1 + Offsets.ClientSoldierWeapon.m_pWeapon);
                                    if (RPM.IsValid(pWeapon1))
                                    {
                                        long pModifier1 = RPM.ReadInt64(pWeapon1 + Offsets.ClientWeapon.m_pModifier);
                                        if (RPM.IsValid(pModifier1))
                                        {
                                            WeaponID = RPM.ReadInt32(pWeapon1 + Offsets.ClientWeapon.m_pModifier);
                                            NotificationTimer = 75;
                                            Notificationstring = "WEAPONID CHANGED !";
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case Keys.Home:
                        WeaponID = 0;
                        NotificationTimer = 75;
                        Notificationstring = "WEAPONID KILLED !";
                        break;
                }
            }
        }

        // Check is Game Run
        private bool IsGameRun()
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == process.ProcessName)
                    return true;
            }
            return false;
        }

        // Update Thread
        private void Update(object sender)
        {
            while (IsGameRun())
            {
                // Resize
                if (IsResize)
                {
                    device.Resize(new Size2(rect.Width, rect.Height));
                    //Console.WriteLine("Resize {0}/{1}", rect.Width, rect.Height);
                    IsResize = false;
                }

                // Begin Draw
                device.BeginDraw();
                device.Clear(new Color4(0.0f, 0.0f, 0.0f, 0.0f));

                // Check Window State
                if (IsGameWindowSelected)
                {
                    // Read & Draw Players
                    Read();
                    GetSpectators();
                    
                    if (VisualMenuItems[VisualMenuList.Radar].Enabled)
                    {
                        DrawRadar(5, 5, 280, 280);
                    }
                    if (VisualMenuItems[VisualMenuList.Info].Enabled)
                    {
                        DrawInfo(5, VisualMenuItems[VisualMenuList.Radar].Enabled ? 305 : 5, 145, 50);
                    }
                    /*int countlala = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        DrawWarn(rect.Width - 160, 50, 150, 80, "LALALALALALALALA", "Spectators", i);
                        if (i == 0 || i == 1 || i == 2 || i == 3)
                        {
                            countlala++;
                            DrawWarn(rect.Width - 160, 140, 150, 80, "LALALALALALALALA", "Watching", countlala);
                        }
                    }*/
                    /*if (Menu_Enabled)
                    {
                        DrawText((VisualMenuItems[VisualMenuList.Info].Enabled || VisualMenuItems[VisualMenuList.Radar].Enabled) ? 290 : 5, 5, "[INSERT] SHOW / HIDE MENU", new Color(158, 197, 85, 255), true);
                    }
                    else
                    {
                        DrawText((VisualMenuItems[VisualMenuList.Info].Enabled || VisualMenuItems[VisualMenuList.Radar].Enabled) ? 290 : 5, 5, "[INSERT] SHOW / HIDE MENU", Color.White, true);
                    }*/
                    // Draw Credits
                    //DrawTextCenter(rect.Width / 2 - 125, 5, 250, (int)fontLarge.FontSize, "BF4 PRIVATE BY TEJISAV V1.0", new Color(158, 197, 85, 255), true);

                    // Draw Spectator Count
                    DrawTextCenter(rect.Width / 2 - 125, rect.Height - (int)fontLarge.FontSize, 250, (int)fontLarge.FontSize, spectatorCount + " SPECTATOR(S) ON A SERVER", new Color(158, 197, 85, 255), true);

                    // Draw Menu
                    DrawMenuInfo(5, VisualMenuItems[VisualMenuList.Radar].Enabled && VisualMenuItems[VisualMenuList.Info].Enabled ? 370 : VisualMenuItems[VisualMenuList.Info].Enabled ? 70 : VisualMenuItems[VisualMenuList.Radar].Enabled ? 300 : 5, 200, 40);

                    if (Menu_Enabled)
                    {
                        DrawMainMenu(5, VisualMenuItems[VisualMenuList.Radar].Enabled && VisualMenuItems[VisualMenuList.Info].Enabled ? 410 : VisualMenuItems[VisualMenuList.Info].Enabled ? 110 : VisualMenuItems[VisualMenuList.Radar].Enabled ? 340 : 45);
                    }

                    if (NotificationTimer > 0)
                    {
                        DrawNotification(rect.Center.X - 125, 25, 250, 55, Notificationstring);
                        NotificationTimer--;
                    }
                }

                // End Draw
                device.EndDraw();
                //Thread.Sleep(Interval);
            }
            // Close Process
            RPM.CloseProcess();
            // Exit
            Environment.Exit(0);
        }

        // Read Game Memorry
        private void Read()
        {
            // Reset Old Data
            players.Clear();
            localPlayer = new Player();

            // Read Local
            #region Get Local Player
            long pGContext = RPM.ReadInt64(Offsets.ClientGameContext.GetInstance());
            if (!RPM.IsValid(pGContext))
                return;

            long pPlayerManager = RPM.ReadInt64(pGContext + Offsets.ClientGameContext.m_pPlayerManager);
            if (!RPM.IsValid(pPlayerManager))
                return;

            long pLocalPlayer = RPM.ReadInt64(pPlayerManager + Offsets.ClientPlayerManager.m_pLocalPlayer);
            if (!RPM.IsValid(pLocalPlayer))
            {
                WeaponID = 0;
                return;
            }

            //RPM.ReadInt64(pLocalPlayer + Offsets.ClientPlayer.m_pControlledControllable);
            long pLocalSoldier = GetClientSoldierEntity(pLocalPlayer, localPlayer);
            if (!RPM.IsValid(pLocalSoldier))
                return;

            long pHealthComponent = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_pHealthComponent);
            if (!RPM.IsValid(pHealthComponent))
                return;

            long m_pPredictedController = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_pPredictedController);
            if (!RPM.IsValid(m_pPredictedController))
                return;

            // Health
            localPlayer.Health = RPM.ReadFloat(pHealthComponent + Offsets.HealthComponent.m_Health);
            localPlayer.MaxHealth = RPM.ReadFloat(pHealthComponent + Offsets.HealthComponent.m_MaxHealth);

            if (localPlayer.Health <= 0) // YOU DEAD :D
            {
                WeaponID = 0;
                return;
            }

            // Origin
            localPlayer.Origin = RPM.ReadVector3(m_pPredictedController + Offsets.ClientSoldierPrediction.m_Position);
            if (AimbotMenuItems[AimbotMenuList.AIM].Enabled && !localPlayer.InVehicle)
            {
                localPlayer.Velocity = RPM.ReadVector3(m_pPredictedController + Offsets.ClientSoldierPrediction.m_Velocity);
            }

            // Other
            localPlayer.Team = RPM.ReadInt32(pLocalPlayer + Offsets.ClientPlayer.m_teamId);
            //localPlayer.Name = NullTerminatedStringFix(RPM.ReadString(pLocalPlayer + Offsets.ClientPlayer.szName, 0x10));
            //localPlayer.Pose = RPM.ReadInt32(pLocalSoldier + Offsets.ClientSoldierEntity.m_poseType);
            localPlayer.Yaw = RPM.ReadFloat(pLocalSoldier + Offsets.ClientSoldierEntity.m_authorativeYaw);
            localPlayer.IsOccluded = RPM.ReadByte(pLocalSoldier + Offsets.ClientSoldierEntity.m_occluded);
            //localPlayer.pClientPlayer = pLocalSoldier;
            //localPlayer.pPlayerView = RPM.ReadInt64(pLocalSoldier + Offsets.ClientPlayer.m_PlayerView);

            // Render View
            long pGameRenderer = RPM.ReadInt64(Offsets.GameRenderer.GetInstance());
            if (RPM.IsValid(pGameRenderer))
            {
                long pRenderView = RPM.ReadInt64(pGameRenderer + Offsets.GameRenderer.m_pRenderView);
                if (RPM.IsValid(pRenderView))
                {
                    localPlayer.Fov.X = RPM.ReadFloat(pRenderView + Offsets.RenderView.m_fovX);
                    localPlayer.Fov.Y = RPM.ReadFloat(pRenderView + Offsets.RenderView.m_FovY);
                    // Read Screen Matrix
                    viewProj = RPM.ReadMatrix(pRenderView + Offsets.RenderView.m_ViewProj);
                    m_ViewMatrixInverse = RPM.ReadMatrix(pRenderView + Offsets.RenderView.m_ViewMatrixInverse);
                }
            }
            if (VisualMenuItems[VisualMenuList.Info].Enabled)
            {
                long pShotsStat = RPM.ReadInt64(Offsets.ShotsStat.GetInstance());
                if (RPM.IsValid(pShotsStat))
                {
                    localPlayer.shotHit = RPM.ReadInt32(pShotsStat + Offsets.ShotsStat.m_shotHit);
                    localPlayer.shotsFired = RPM.ReadInt32(pShotsStat + Offsets.ShotsStat.m_shotsFired);
                }
            }
            long pCurrentWeaponFiring = RPM.ReadInt64(Offsets.OFFSET_CURRENT_WEAPONFIRING);
            // Weapon Ammo
            if (localPlayer.InVehicle && (localPlayer.IsDriver || RPM.IsValid(pCurrentWeaponFiring)))
            {
                localPlayer.IsVehicleWeapon = true;
                if (RPM.IsValid(pCurrentWeaponFiring))
                {
                    localPlayer.IsValidWeapon = true;
                    localPlayer.Ammo = RPM.ReadInt32(pCurrentWeaponFiring + Offsets.WeaponFiring.m_projectilesLoaded);
                    localPlayer.AmmoClip = RPM.ReadInt32(pCurrentWeaponFiring + Offsets.WeaponFiring.m_projectilesInMagazines);
                    localPlayer.Heating = RPM.ReadFloat(pCurrentWeaponFiring + Offsets.WeaponFiring.m_overheatPenaltyTimer);
                    if (AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled || MiscMenuItems[MiscMenuList.OneHitKill].Enabled || MiscMenuItems[MiscMenuList.VehicleBulletsPershell].Value != 0 || MiscMenuItems[MiscMenuList.VehicleBulletsPerShot].Value != 0)
                    {
                        long pPrimaryFire = RPM.ReadInt64(pCurrentWeaponFiring + Offsets.WeaponFiring.m_pPrimaryFire);
                        if (RPM.IsValid(pPrimaryFire))
                        {
                            long pShotConfigData1 = RPM.ReadInt64(pPrimaryFire + Offsets.PrimaryFire.m_FiringData);
                            if (RPM.IsValid(pShotConfigData1))
                            {
                                if (MiscMenuItems[MiscMenuList.VehicleBulletsPershell].Value != 0)
                                {
                                    if (RPM.ReadInt32(pShotConfigData1 + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShell) != MiscMenuItems[MiscMenuList.VehicleBulletsPershell].Value)
                                        RPM.WriteInt32(pShotConfigData1 + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShell, MiscMenuItems[MiscMenuList.VehicleBulletsPershell].Value);
                                }
                                //DrawText(5, 600, "VehicleBulletsPershell:" + RPM.ReadInt32(pShotConfigData1 + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShell), Color.White, true);
                                if (MiscMenuItems[MiscMenuList.VehicleBulletsPerShot].Value != 0)
                                {
                                    if (RPM.ReadInt32(pShotConfigData1 + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShot) != MiscMenuItems[MiscMenuList.VehicleBulletsPerShot].Value)
                                        RPM.WriteInt32(pShotConfigData1 + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShot, MiscMenuItems[MiscMenuList.VehicleBulletsPerShot].Value);
                                }
                                //DrawText(5, 620, "VehicleBulletsPerShot:" + RPM.ReadInt32(pShotConfigData1 + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShot), Color.White, true);
                                long pProjectileData = RPM.ReadInt64(pShotConfigData1 + Offsets.ShotConfigData1.m_pProjectileData);
                                if (RPM.IsValid(pProjectileData))
                                {
                                    localPlayer.BulletGravity = RPM.ReadFloat(pProjectileData + Offsets.BulletEntityData.m_Gravity);
                                    localPlayer.BulletSpeed = RPM.ReadFloat(pShotConfigData1 + Offsets.ShotConfigData1.m_initialSpeed);
                                    //DrawText(5, 600, "VehicleStartDamage:" + RPM.ReadFloat(m_pProjectileData + Offsets.BulletEntityData.m_StartDamage), Color.White, true);
                                    //DrawText(5, 620, "VehicleEndDamage:" + RPM.ReadFloat(m_pProjectileData + Offsets.BulletEntityData.m_EndDamage), Color.White, true);
                                    if (MiscMenuItems[MiscMenuList.VehicleOneHitKill].Enabled)
                                    {
                                        RPM.WriteFloat(pProjectileData + Offsets.BulletEntityData.m_StartDamage, 110f);
                                        RPM.WriteFloat(pProjectileData + Offsets.BulletEntityData.m_EndDamage, 110f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                long pSoldierWeaponComponent = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_soldierWeaponsComponent);
                if (RPM.IsValid(pSoldierWeaponComponent))
                {
                    long pWeaponHandle = RPM.ReadInt64(pSoldierWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_handler);
                    Int32 ActiveSlot = RPM.ReadInt32(pSoldierWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_activeSlot);

                    // ZeroingLevel (-1,0,1,2,3,4)
                    localPlayer.zeroingDistanceLevel = RPM.ReadInt32(pSoldierWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_zeroingDistanceLevel);
                    //DrawText(5, 400, "ZeroingLevel:" + zeroingDistanceLevel, Color.White, true);

                    if (RPM.IsValid(pWeaponHandle))
                    {
                        long pSoldierWeapon = RPM.ReadInt64(pWeaponHandle + ActiveSlot * 0x8);
                        if (RPM.IsValid(pSoldierWeapon))
                        {
                            long m_authorativeAiming = RPM.ReadInt64(pSoldierWeapon + Offsets.ClientSoldierWeapon.m_authorativeAiming);
                            if (RPM.IsValid(m_authorativeAiming))
                            {
                                localPlayer.Sway = RPM.ReadVector2(m_authorativeAiming + Offsets.ClientSoldierAimingSimulation.m_sway);
                            }
                            long pCorrectedFiring = RPM.ReadInt64(pSoldierWeapon + Offsets.ClientSoldierWeapon.m_pPrimary);
                            if (RPM.IsValid(pCorrectedFiring))
                            {
                                if (AimbotMenuItems[AimbotMenuList.AIM].Enabled || MiscMenuItems[MiscMenuList.OneHitKill].Enabled || RemovalMenuItems[RemovalMenuList.NoGravity].Enabled || MiscMenuItems[MiscMenuList.RateOfFire].Enabled || MiscMenuItems[MiscMenuList.BulletsPerShell].Value != 0 || MiscMenuItems[MiscMenuList.BulletsPerShot].Value != 0)
                                {
                                    // For Sniper Distance Modes
                                    long pWeaponModifier = RPM.ReadInt64(pCorrectedFiring + Offsets.WeaponFiring.m_pWeaponModifier);
                                    if (RPM.IsValid(pWeaponModifier) && localPlayer.zeroingDistanceLevel != -1)
                                    {
                                        long pWeaponZeroingModifier = RPM.ReadInt64(pWeaponModifier + Offsets.WeaponModifier.m_pWeaponZeroingModifier);
                                        if (RPM.IsValid(pWeaponZeroingModifier))
                                        {
                                            long pModes = RPM.ReadInt64(pWeaponZeroingModifier + Offsets.WeaponZeroingModifier.m_pModes);
                                            if (RPM.IsValid(pModes))
                                            {
                                                localPlayer.zeroingModes = RPM.ReadVector2(pModes + localPlayer.zeroingDistanceLevel * 0x8);
                                                //DrawText(5, 420, "ZeroingDistance:" + zeroingModes.X, Color.White, true);
                                                //DrawText(5, 440, "ZeroingAngle:" + zeroingModes.Y, Color.White, true);
                                            }
                                        }
                                    }
                                    // For aimbot and other weapon stuff
                                    long pPrimaryFire = RPM.ReadInt64(pCorrectedFiring + Offsets.WeaponFiring.m_pPrimaryFire);
                                    if (RPM.IsValid(pPrimaryFire))
                                    {
                                        long pFiringFunctionData = RPM.ReadInt64(pPrimaryFire + Offsets.PrimaryFire.m_FiringData);
                                        if (RPM.IsValid(pFiringFunctionData))
                                        {
                                            localPlayer.BulletInitialPosition = RPM.ReadVector3(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_InitialPosition);
                                            localPlayer.BulletInitialSpeed = RPM.ReadVector3(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_InitialSpeed);
                                            //real_gravity = (float)((-1.0 * (BulletInitialPosition.Y + (BulletInitialSpeed.Y * 100) / BulletInitialSpeed.Z) * BulletInitialSpeed.Z * BulletInitialSpeed.Z) / (0.5 * 100 * 100));
                                            //DrawText(5, 620, "RealGravity:" + Math.Abs(real_gravity), Color.White, true);
                                            if (MiscMenuItems[MiscMenuList.BulletsPerShell].Value != 0)
                                            {
                                                if (RPM.ReadInt32(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShell) != MiscMenuItems[MiscMenuList.BulletsPerShell].Value)
                                                    RPM.WriteInt32(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShell, MiscMenuItems[MiscMenuList.BulletsPerShell].Value);
                                            }
                                            //DrawText(5, 600, "BulletsPerShell:" + RPM.ReadInt32(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShell), Color.White, true);
                                            if (MiscMenuItems[MiscMenuList.BulletsPerShot].Value != 0)
                                            {
                                                if (RPM.ReadInt32(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShot) != MiscMenuItems[MiscMenuList.BulletsPerShot].Value)
                                                    RPM.WriteInt32(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShot, MiscMenuItems[MiscMenuList.BulletsPerShot].Value);
                                            }
                                            //DrawText(5, 620, "BulletsPerShot:" + RPM.ReadInt32(pFiringFunctionData + Offsets.FiringFunctionData.m_ShotConfigData + Offsets.ShotConfigData.m_NumberOfBulletsPerShot), Color.White, true);
                                            if (MiscMenuItems[MiscMenuList.RateOfFire].Enabled && (ActiveSlot == 0 || ActiveSlot == 1))
                                            {
                                                float FRate = FiringRate[MiscMenuItems[MiscMenuList.FireRate].Value];
                                                RPM.WriteFloat(pFiringFunctionData + Offsets.FiringFunctionData.m_FireLogic + Offsets.FireLogicData.m_TriggerPullWeight, 0.15f);
                                                RPM.WriteFloat(pFiringFunctionData + Offsets.FiringFunctionData.m_FireLogic + Offsets.FireLogicData.m_RateOfFire, FRate);
                                                RPM.WriteFloat(pFiringFunctionData + Offsets.FiringFunctionData.m_FireLogic + Offsets.FireLogicData.m_RateOfFireForBurst, FRate);
                                                // You may use also this for reading rate of fire
                                                //RPM.WriteFloat(pFiringFunctionData + Offsets.FiringFunctionData.m_FireLogic + 0x01C8, 1200.000f);
                                            }
                                            //DrawText(5, 360, "RateOfFire: " + RPM.ReadFloat(pFiringFunctionData + Offsets.FiringFunctionData.m_FireLogic + Offsets.FireLogicData.m_RateOfFire), Color.White, true);
                                            long pProjectileData = RPM.ReadInt64(pFiringFunctionData + Offsets.ShotConfigData1.m_pProjectileData);
                                            if (RPM.IsValid(pProjectileData))
                                            {
                                                if (RemovalMenuItems[RemovalMenuList.NoGravity].Enabled && (ActiveSlot == 0 || ActiveSlot == 1))
                                                {
                                                    RPM.WriteFloat(pProjectileData + Offsets.BulletEntityData.m_Gravity, 0.0f);
                                                }
                                                localPlayer.BulletGravity = RPM.ReadFloat(pProjectileData + Offsets.BulletEntityData.m_Gravity);
                                                //DrawText(5, 600, "BulletGravity:" + Math.Abs(localPlayer.BulletGravity), Color.White, true);
                                                localPlayer.BulletSpeed = RPM.ReadFloat(pFiringFunctionData + Offsets.ShotConfigData1.m_initialSpeed);
                                                //DrawText(5, 560, "SoldierStartDamage:" + RPM.ReadFloat(pProjectileData + Offsets.BulletEntityData.m_StartDamage), Color.White, true);
                                                //DrawText(5, 580, "SoldierEndDamage:" + RPM.ReadFloat(pProjectileData + Offsets.BulletEntityData.m_EndDamage), Color.White, true);
                                                if (MiscMenuItems[MiscMenuList.OneHitKill].Enabled && (ActiveSlot == 0 || ActiveSlot == 1))
                                                {
                                                    RPM.WriteFloat(pProjectileData + Offsets.BulletEntityData.m_StartDamage, 110f);
                                                    RPM.WriteFloat(pProjectileData + Offsets.BulletEntityData.m_EndDamage, 110f);
                                                    long pWeapon = RPM.ReadInt64(pSoldierWeapon + Offsets.ClientSoldierWeapon.m_pWeapon);
                                                    if (RPM.IsValid(pWeapon))
                                                    {
                                                        RPM.WriteInt32(pWeapon + Offsets.ClientWeapon.m_pModifier, WeaponID);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if ((RemovalMenuItems[RemovalMenuList.RCS].Enabled || RemovalMenuItems[RemovalMenuList.NoSpread].Value != 0) && (ActiveSlot == 0 || ActiveSlot == 1))
                                {
                                    long pSway = RPM.ReadInt64(pCorrectedFiring + Offsets.WeaponFiring.m_pSway);
                                    if (RPM.IsValid(pSway))
                                    {
                                        long pSwayData = RPM.ReadInt64(pSway + Offsets.WeaponSway.m_pSwayData);
                                        if (RPM.IsValid(pSwayData))
                                        {
                                            if (RemovalMenuItems[RemovalMenuList.RCS].Enabled)
                                            {
                                                float FirstShotRecoilMultiplier = RPM.ReadFloat(pSwayData + Offsets.GunSwayData.m_FirstShotRecoilMultiplier);
                                                if (FirstShotRecoilMultiplier != 0f)
                                                {
                                                    RPM.WriteFloat(pSwayData + Offsets.GunSwayData.m_FirstShotRecoilMultiplier, 0f);
                                                    RPM.WriteFloat(pSwayData + Offsets.GunSwayData.m_ShootingRecoilDecreaseScale, 100f);
                                                }
                                            }
                                            if (RemovalMenuItems[RemovalMenuList.NoSpread].Value != 0)
                                            {
                                                float DeviationScaleFactorZoom = RPM.ReadFloat(pSwayData + Offsets.GunSwayData.m_DeviationScaleFactorZoom);
                                                if (DeviationScaleFactorZoom != SpreadValue[RemovalMenuItems[RemovalMenuList.NoSpread].Value])
                                                {
                                                    RPM.WriteFloat(pSwayData + Offsets.GunSwayData.m_DeviationScaleFactorZoom, SpreadValue[RemovalMenuItems[RemovalMenuList.NoSpread].Value]);
                                                    RPM.WriteFloat(pSwayData + Offsets.GunSwayData.m_GameplayDeviationScaleFactorZoom, SpreadValue[RemovalMenuItems[RemovalMenuList.NoSpread].Value]);
                                                    RPM.WriteFloat(pSwayData + Offsets.GunSwayData.m_DeviationScaleFactorNoZoom, SpreadValue[RemovalMenuItems[RemovalMenuList.NoSpread].Value]);
                                                    RPM.WriteFloat(pSwayData + Offsets.GunSwayData.m_GameplayDeviationScaleFactorNoZoom, SpreadValue[RemovalMenuItems[RemovalMenuList.NoSpread].Value]);
                                                }
                                            }
                                            //DrawText(5, 640, "DeviationScaleFactorZoom:" + RPM.ReadFloat(pSwayData + Offsets.GunSwayData.m_DeviationScaleFactorZoom), Color.White, true);
                                            //DrawText(5, 660, "GameplayDeviationScaleFactorZoom:" + RPM.ReadFloat(pSwayData + Offsets.GunSwayData.m_GameplayDeviationScaleFactorZoom), Color.White, true);
                                            //DrawText(5, 680, "DeviationScaleFactorNoZoom:" + RPM.ReadFloat(pSwayData + Offsets.GunSwayData.m_DeviationScaleFactorNoZoom), Color.White, true);
                                            //DrawText(5, 700, "GameplayDeviationScaleFactorNoZoom:" + RPM.ReadFloat(pSwayData + Offsets.GunSwayData.m_GameplayDeviationScaleFactorNoZoom), Color.White, true);
                                        }
                                    }
                                }
                                if (RemovalMenuItems[RemovalMenuList.NoBreath].Enabled && ActiveSlot == 0)
                                {
                                    long breathControlHandler = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_breathControlHandler);
                                    if (RPM.IsValid(breathControlHandler))
                                    {
                                        RPM.WriteFloat(breathControlHandler + Offsets.BreathControlHandler.m_Enabled, 0f);
                                    }
                                }
                                if (VisualMenuItems[VisualMenuList.Info].Enabled)
                                {
                                    localPlayer.Ammo = RPM.ReadInt32(pCorrectedFiring + Offsets.WeaponFiring.m_projectilesLoaded);
                                    localPlayer.AmmoClip = RPM.ReadInt32(pCorrectedFiring + Offsets.WeaponFiring.m_projectilesInMagazines);
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            // Pointer to Players Array
            long ppPlayer = RPM.ReadInt64(pPlayerManager + Offsets.ClientPlayerManager.m_ppPlayer);
            if (!RPM.IsValid(ppPlayer))
                return;

            // Reset
            //spectatorCount = 0;

            // Get Player by Id
            #region Get Player by Id
            for (uint i = 0; i < 70; i++)
            {
                // Create new Player
                Player player = new Player();

                // Pointer to ClientPlayer class (Player Array + (Id * Size of Pointer))
                long pPlayer = RPM.ReadInt64(ppPlayer + (i * sizeof(long)));
                if (!RPM.IsValid(pPlayer))
                    continue;

                if (pPlayer == pLocalPlayer)
                    continue;

                //player.pClientPlayer = pPlayer;
                //player.pOwnerPlayerView = RPM.ReadInt64(pPlayer + Offsets.ClientPlayer.m_ownPlayerView);
                //player.pPlayerView = RPM.ReadInt64(pPlayer + Offsets.ClientPlayer.m_PlayerView);
                //player.IsSpectator = Convert.ToBoolean(RPM.ReadByte(pPlayer + Offsets.ClientPlayer.m_isSpectator));
                // Name
                //player.Name = NullTerminatedStringFix(RPM.ReadString2(pPlayer + Offsets.ClientPlayer.szName, 0x10));
                player.Name = RPM.ReadString(pPlayer + Offsets.ClientPlayer.szName, 0x10);

                // Check Spectator
                /*if (player.IsSpectator)
                {
                    spectatorCount++;
                    DrawText(5, 600, "Spectator:" + player.Name, Color.White, true);
                }*/

                // RPM.ReadInt64(pPlayer + Offsets.ClientPlayer.m_pControlledControllable);
                long pClientSoldier = GetClientSoldierEntity(pPlayer, player);
                if (!RPM.IsValid(pClientSoldier))
                    continue;

                long pClientHealthComponent = RPM.ReadInt64(pClientSoldier + Offsets.ClientSoldierEntity.m_pHealthComponent);
                if (!RPM.IsValid(pClientHealthComponent))
                    continue;

                long pClientPredictedController = RPM.ReadInt64(pClientSoldier + Offsets.ClientSoldierEntity.m_pPredictedController);
                if (!RPM.IsValid(pClientPredictedController))
                    continue;

                if (VisualMenuItems[VisualMenuList.Radar].Enabled)
                {
                    if (player.InVehicle)
                    {
                        long pAttachedControllable = RPM.ReadInt64(pPlayer + Offsets.ClientPlayer.m_pAttachedControllable);
                        long pPhysicsEntity = RPM.ReadInt64(pAttachedControllable + Offsets.ClientSoldierEntity.m_pPhysicsEntity);
                        if (RPM.IsValid(pPhysicsEntity))
                        {
                            long pPhysicsEntityTransform = RPM.ReadInt64(pPhysicsEntity + Offsets.DynamicPhysicsEntity.m_EntityTransform);
                            if (RPM.IsValid(pPhysicsEntityTransform))
                            {
                                Matrix VehicleTransform = RPM.ReadMatrix(pPhysicsEntityTransform);
                                player.ShootSpace = new Vector3(VehicleTransform.M31, VehicleTransform.M32, VehicleTransform.M33);
                            }
                        }
                    }
                    else
                    {
                        long pClientWeaponComponent = RPM.ReadInt64(pClientSoldier + Offsets.ClientSoldierEntity.m_soldierWeaponsComponent);
                        if (RPM.IsValid(pClientWeaponComponent))
                        {
                            long pClientWeaponHandle = RPM.ReadInt64(pClientWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_handler);
                            int ClientActiveSlot = RPM.ReadInt32(pClientWeaponComponent + Offsets.ClientSoldierWeaponsComponent.m_activeSlot);
                            if (RPM.IsValid(pClientWeaponHandle))
                            {
                                long pClientSoldierWeapon = RPM.ReadInt64(pClientWeaponHandle + ClientActiveSlot * 8);
                                if (RPM.IsValid(pClientSoldierWeapon))
                                {
                                    long pClientWeapon = RPM.ReadInt64(pClientSoldierWeapon + Offsets.ClientSoldierWeapon.m_pWeapon);
                                    if (RPM.IsValid(pClientWeapon))
                                    {
                                        Matrix ClientTransform = RPM.ReadMatrix(pClientWeapon + Offsets.ClientWeapon.m_shootSpace);
                                        player.ShootSpace = new Vector3(ClientTransform.M31, ClientTransform.M32, ClientTransform.M33);
                                    }
                                }
                            }
                        }
                    }
                }
                // Health
                player.Health = RPM.ReadFloat(pClientHealthComponent + Offsets.HealthComponent.m_Health);
                player.MaxHealth = RPM.ReadFloat(pClientHealthComponent + Offsets.HealthComponent.m_MaxHealth);

                /*if (player.Health <= 0) // DEAD
                    continue;*/

                // Origin (Position in Game X, Y, Z)
                player.Origin = RPM.ReadVector3(pClientPredictedController + Offsets.ClientSoldierPrediction.m_Position);

                if (player.Origin.IsZero)
                    continue;

                if (AimbotMenuItems[AimbotMenuList.AIM].Enabled && !player.InVehicle)
                {
                    player.Velocity = RPM.ReadVector3(pClientPredictedController + Offsets.ClientSoldierPrediction.m_Velocity);
                }
                // Other
                player.Team = RPM.ReadInt32(pPlayer + Offsets.ClientPlayer.m_teamId);
                player.Pose = RPM.ReadInt32(pClientSoldier + Offsets.ClientSoldierEntity.m_poseType);
                player.Yaw = RPM.ReadFloat(pClientSoldier + Offsets.ClientSoldierEntity.m_authorativeYaw);
                player.IsOccluded = RPM.ReadByte(pClientSoldier + Offsets.ClientSoldierEntity.m_occluded);
                if (!VisualMenuItems[VisualMenuList.ESP_Enemy].Enabled || player.Team != localPlayer.Team)
                {
                    // EnemyDistance to You
                    player.Distance = Vector3.Distance(localPlayer.Origin, player.Origin);
                    player.DistanceCrosshair = Distance_Crosshair(player);
                    if (player.IsValid())
                    {
                        #region Bone ESP
                        if ((VisualMenuItems[VisualMenuList.ESP_Bone].Enabled || AimbotMenuItems[AimbotMenuList.AIM].Enabled))
                        {
                            player.BoneCheck = GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_HEAD, out player.Bone.BONE_HEAD);
                            // Player Bone
                            if (player.BoneCheck/*GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_HEAD, out player.Bone.BONE_HEAD)*/
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTELBOWROLL, out player.Bone.BONE_LEFTELBOWROLL)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTFOOT, out player.Bone.BONE_LEFTFOOT)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTHAND, out player.Bone.BONE_LEFTHAND)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTKNEEROLL, out player.Bone.BONE_LEFTKNEEROLL)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_LEFTSHOULDER, out player.Bone.BONE_LEFTSHOULDER)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_NECK, out player.Bone.BONE_NECK)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTELBOWROLL, out player.Bone.BONE_RIGHTELBOWROLL)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTFOOT, out player.Bone.BONE_RIGHTFOOT)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTHAND, out player.Bone.BONE_RIGHTHAND)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTKNEEROLL, out player.Bone.BONE_RIGHTKNEEROLL)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_RIGHTSHOULDER, out player.Bone.BONE_RIGHTSHOULDER)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_SPINE, out player.Bone.BONE_SPINE)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_SPINE1, out player.Bone.BONE_SPINE1)
                                && GetBonyById(pClientSoldier, (int)Offsets.UpdatePoseResultData.BONES.BONE_SPINE2, out player.Bone.BONE_SPINE2) && VisualMenuItems[VisualMenuList.ESP].Enabled && VisualMenuItems[VisualMenuList.ESP_Bone].Enabled)
                            {
                                DrawBone(player);
                            }
                        }
                        #endregion

                        Vector3 Foot, Head;
                        if (WorldToScreen(player.Origin, out Foot) && WorldToScreen(player.Origin, player.Pose, out Head))
                        {
                            float HeadToFoot = Foot.Y - Head.Y;
                            float BoxWidth = HeadToFoot / 2;
                            float X = Head.X - (BoxWidth) / 2;

                            #region ESP Color
                            Color color;
                            if (player.Team == localPlayer.Team)
                            {
                                color = friendlyColor;
                            }
                            else
                            {
                                color = player.IsVisible() ? enemyColorVisible : enemyColor;
                            }
                            #endregion

                            #region Draw ESP
                            if (VisualMenuItems[VisualMenuList.ESP].Enabled)
                            {
                                // ESP Box
                                if (VisualMenuItems[VisualMenuList.ESP_Box].Enabled)
                                {
                                    if (VisualMenuItems[VisualMenuList.ESP_BoxType].Value == 0)
                                    {
                                        DrawRect((int)X, (int)Head.Y, (int)BoxWidth, (int)HeadToFoot, color);
                                    }
                                    else
                                    {
                                        DrawAABB(player.GetAABB(), player.Origin, player.Yaw, color);
                                    }
                                }

                                // ESP Vehicle
                                if (VisualMenuItems[VisualMenuList.ESP_Vehicle].Enabled)
                                {
                                    DrawAABB(player.VehicleAABB, player.VehicleTranfsorm, player.Team == localPlayer.Team ? friendlyColorVehicle : enemyColorVehicle);
                                }

                                // ESP EnemyDistance
                                if (VisualMenuItems[VisualMenuList.ESP_Distance].Enabled)
                                {
                                    DrawText((int)X, (int)Foot.Y, (int)player.Distance + "m", Color.White, true);
                                }

                                // ESP Health
                                if (VisualMenuItems[VisualMenuList.ESP_Health].Enabled)
                                {
                                    DrawHealth((int)X, (int)Head.Y - 6, (int)BoxWidth, 3, (int)player.Health, (int)player.MaxHealth);

                                    // Vehicle Health
                                    if (player.InVehicle && player.IsDriver)
                                    {
                                        DrawHealth((int)X, (int)Head.Y - 10, (int)BoxWidth, 3, (int)player.VehicleHealth, (int)player.VehicleMaxHealth);
                                    }
                                }

                                if (VisualMenuItems[VisualMenuList.ESP_Line].Enabled && player.Distance < 100f)
                                {
                                    DrawLine(rect.Width / 2, rect.Height, (int)Foot.X, (int)Foot.Y, color);
                                }

                                if (VisualMenuItems[VisualMenuList.ESP_Name].Enabled)
                                {
                                    int int_ = (int)X + (int)BoxWidth / 2 - 100;
                                    int int_2 = (int)Head.Y - (VisualMenuItems[VisualMenuList.ESP_Health].Enabled ? 16 : 10) - (int)fontSmall.FontSize - 2;
                                    DrawTextCenter(int_, int_2, 200, (int)fontSmall.FontSize, player.Name, Color.White, true);
                                }
                            }
                            #endregion
                        }
                    }
                    // ADD IN ARRAY
                    players.Add(player);
                }
            }
            #endregion
            if (VisualMenuItems[VisualMenuList.Overheat].Enabled && localPlayer.IsValid() && localPlayer.IsValidWeapon)
            {
                int HeatingPercent = (int)Math.Round(localPlayer.Heating * 100f, 0);
                DrawFillRect(rect.Width / 2 - 471, rect.Height - 48, 282, 45, new Color(30, 30, 30, 130));
                DrawRect(rect.Width / 2 - 470, rect.Height - 47, 280, 43, Color.Black);
                DrawLine(rect.Width / 2 - 470, rect.Height - 20, rect.Width / 2 - 190, rect.Height - 20, Color.Black);
                DrawText(rect.Width / 2 - 465, rect.Height - 44, "Overheat: " + HeatingPercent + "%", Color.White, true);
                DrawProgress(rect.Width / 2 - 465, rect.Height - 15, 270, 5, HeatingPercent, 100);
            }
            if (MiscMenuItems[MiscMenuList.JetSpeed].Enabled && localPlayer.InVehicle && localPlayer.IsDriver && Jets.Contains(localPlayer.VehicleName))
            {
                float CurrentJetVelocity = localPlayer.Velocity.Length() * 3.6f;
                DrawText(5, 600, "Speed: " + (int)CurrentJetVelocity, Color.White, true);
                long BORDERINPUTNODE = RPM.ReadInt64(Offsets.OFFSET_BORDERINPUTNODE);
                if (RPM.IsValid(BORDERINPUTNODE))
                {
                    long pKeyboard = RPM.ReadInt64(BORDERINPUTNODE + Offsets.BorderInputNode.m_pKeyboard);
                    if (RPM.IsValid(pKeyboard))
                    {
                        long pDevice = RPM.ReadInt64(pKeyboard + Offsets.Keyboard.m_pDevice);
                        if (RPM.IsValid(pDevice))
                        {
                            if (CurrentJetVelocity <= WantedJetSpeed && CurrentJetVelocity > 20f)
                            {
                                RPM.WriteInt32(pDevice + Offsets.KeyboardDevice.m_Buffer + (long)Offsets.KeyboardDevice.InputDeviceKeys.IDK_W, 1);
                                RPM.WriteInt32(pDevice + Offsets.KeyboardDevice.m_Buffer + (long)Offsets.KeyboardDevice.InputDeviceKeys.IDK_S, 0);
                            }
                            else if (CurrentJetVelocity >= MaxJetSpeed)
                            {
                                RPM.WriteInt32(pDevice + Offsets.KeyboardDevice.m_Buffer + (long)Offsets.KeyboardDevice.InputDeviceKeys.IDK_W, 0);
                                RPM.WriteInt32(pDevice + Offsets.KeyboardDevice.m_Buffer + (long)Offsets.KeyboardDevice.InputDeviceKeys.IDK_S, 1);
                            }
                        }
                    }
                }
            }
            if (AimbotMenuItems[AimbotMenuList.AIM].Enabled && (AimbotMenuItems[AimbotMenuList.AIM_AutoAim].Enabled ? true : GetAimKey()))
            {
                if (localPlayer.IsVehicleWeapon && !localPlayer.IsValidWeapon)
                {
                    AimbotStream.Abort();
                }
                else if (!AimbotStream.IsAlive)
                {
                    AimbotStream = new Thread(Aimbot);
                    AimbotStream.Start();
                }
            }
            else
            {
                IsTargetting = false;
            }
            if (MiscMenuItems[MiscMenuList.Teleport].Enabled)
            {
                if (RPM.IsValid(pCurrentWeaponFiring))
                {
                    long pAiming = RPM.ReadInt64(RPM.ReadInt64(RPM.ReadInt64(pCurrentWeaponFiring + 0x48) + 0x68) + 0x28);
                    if (RPM.IsValid(pAiming))
                    {
                        RPM.WriteFloat(pAiming + 0x80, 1000.0f);
                    }
                }

                /*Int64 pClientParachuteComponent = RPM.ReadInt64(pLocalSoldier + 0xB50);
                if (RPM.IsValid(pClientParachuteComponent))
                {
                    Int64 pAimingContraints = RPM.ReadInt64(pClientParachuteComponent + 0xE0);
                    if (RPM.IsValid(pAimingContraints))
                    {
                        RPM.WriteFloat(pAimingContraints + 0x00, -180.0f);
                        RPM.WriteFloat(pAimingContraints + 0x04, 180.0f);
                        RPM.WriteFloat(pAimingContraints + 0x08, -180.0f);
                        RPM.WriteFloat(pAimingContraints + 0x0C, 180.0f);
                        RPM.WriteFloat(pAimingContraints + 0x18, -180.0f);
                        RPM.WriteFloat(pAimingContraints + 0x1C, 180.0f);
                    }
                    Int64 pParachuteComponentData = RPM.ReadInt64(pClientParachuteComponent + 0x10);
                    if (RPM.IsValid(pParachuteComponentData))
                    {
                        RPM.WriteFloat(pParachuteComponentData + 0xE4, 0f);
                    }
                }*/

                Int64 pclientVaultComponent = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_clientVaultComponent);
                if (RPM.IsValid(pclientVaultComponent))
                {
                    if((Convert.ToBoolean(Managed.GetKeyState(4) & Managed.KEY_PRESSED)) || (Convert.ToBoolean(Managed.GetKeyState(84) & Managed.KEY_PRESSED)) || Convert.ToBoolean(Managed.GetKeyState(89) & Managed.KEY_PRESSED))
                    {
                        RPM.WriteNop(0x1410065B9, new byte[] { 0x90, 0x90, 0x90, 0x90 }); // 11 Meter Patch //F3 0F 51 EA 0F 2F E8
                        RPM.WriteNop(0x1401478F0, new byte[] { 0xC3 }); // Animation Fix

                        RPM.WriteNop(0x140147975, new byte[] { 0x90, 0x90, 0x90, 0x90 });
                        RPM.WriteNop(0x1401479E3, new byte[] { 0x90, 0x90, 0x90 });
                        RPM.WriteNop(0x140147BD4, new byte[] { 0x90, 0x90, 0x90 });
                        RPM.WriteNop(0x140147CA4, new byte[] { 0x90, 0x90, 0x90 });
                        RPM.WriteNop(0x140B43E95, new byte[] { 0x00 });
                        /*if (RPM.IsValid(pClientParachuteComponent))
                        {
                            RPM.WriteByte(pClientParachuteComponent + 0x1A, 0);
                            RPM.WriteByte(pClientParachuteComponent + 0x14C, 0);
                            RPM.WriteByte(pClientParachuteComponent + 0xB4, 1);
                        }*/

                        if (Convert.ToBoolean(Managed.GetKeyState(4) & Managed.KEY_PRESSED))
                        {
                            RPM.WriteByte(pclientVaultComponent + 0xf0, 2);
                            Int64 pphysics = RPM.ReadInt64(pclientVaultComponent + 0x110);
                            if (RPM.IsValid(pphysics))
                            {
                                Keyboard.KeyUp(Keys.Space);
                                Keyboard.KeyDown(Keys.Space);
                                RPM.WriteVector3(pphysics + 0x30, new Vector3(localPlayer.Origin.X, localPlayer.Origin.Y + 500.0f, localPlayer.Origin.Z));
                            }
                            RPM.WriteByte(pclientVaultComponent + 0xf0, 1);
                        }
                        if(Convert.ToBoolean(Managed.GetKeyState(84) & Managed.KEY_PRESSED))
                        {
                            long pAngles = RPM.ReadInt64(Offsets.OFFSET_VIEWANGLES);
                            if (RPM.IsValid(pAngles))
                            {
                                long pauthorativeAiming = RPM.ReadInt64(pAngles + Offsets.ClientSoldierWeapon.m_authorativeAiming);
                                if (RPM.IsValid(pauthorativeAiming))
                                {
                                    Vector3 pRayEnd = RPM.ReadVector3(pauthorativeAiming + 0x160);
                                    if(!pRayEnd.IsZero)
                                    {
                                        RPM.WriteByte(pclientVaultComponent + 0xf0, 2);
                                        Int64 pphysics = RPM.ReadInt64(pclientVaultComponent + 0x110);
                                        if (RPM.IsValid(pphysics))
                                        {
                                            Keyboard.KeyUp(Keys.Space);
                                            Keyboard.KeyDown(Keys.Space);
                                            RPM.WriteVector3(pphysics + 0x30, pRayEnd);
                                        }
                                        RPM.WriteByte(pclientVaultComponent + 0xf0, 1);
                                    }
                                }
                            }
                        }
                        if (Convert.ToBoolean(Managed.GetKeyState(89) & Managed.KEY_PRESSED))
                        {
                            int TeleFov = rect.Width / 100 * (AimbotMenuItems[AimbotMenuList.AIM_Fov].Value * 5 + 5);
                            Player ClosestVehicle = new Player();
                            List<Player> FovVehicleList = new List<Player>();
                            foreach (Player player in players)
                            {
                                if (player.IsValid() && (MiscMenuItems[MiscMenuList.DestroyFriendly].Enabled || player.Team != localPlayer.Team) && player.DistanceCrosshair < TeleFov && player.InVehicle && player.IsDriver)
                                {
                                    FovVehicleList.Add(player);
                                }
                            }
                            if (FovVehicleList.Count > 0)
                            {
                                ClosestVehicle = DistanceCrosshairSortPlayers(FovVehicleList);
                                if (!ClosestVehicle.Origin.IsZero)
                                {
                                    RPM.WriteByte(pclientVaultComponent + 0xf0, 2);
                                    Int64 pphysics = RPM.ReadInt64(pclientVaultComponent + 0x110);
                                    if (RPM.IsValid(pphysics))
                                    {
                                        Keyboard.KeyUp(Keys.Space);
                                        Keyboard.KeyDown(Keys.Space);
                                        RPM.WriteVector3(pphysics + 0x30, ClosestVehicle.Origin);
                                    }
                                    RPM.WriteByte(pclientVaultComponent + 0xf0, 1);
                                }
                            }
                        }
                        Teleported = true;
                        WaitTpPatch = true;
                        TpPatchTimer = 50;
                    }
                    else
                    {
                        if (TpPatchTimer > 0)
                        {
                            TpPatchTimer--;
                        }
                        else if (WaitTpPatch)
                        {
                            RPM.WriteNop(0x1410065B9, new byte[] { 0xF3, 0x0F, 0x51, 0xEA }); // 11 Meter Patch //F3 0F 51 EA 0F 2F E8
                            RPM.WriteNop(0x1401478F0, new byte[] { 0x48 }); // Animation Fix
                            WaitTpPatch = false;
                        }

                        if (Teleported)
                        {
                            RPM.WriteNop(0x140147975, new byte[] { 0x48, 0x89, 0x79, 0x48 });
                            RPM.WriteNop(0x1401479E3, new byte[] { 0x89, 0x43, 0x48 });
                            RPM.WriteNop(0x140147BD4, new byte[] { 0x89, 0x73, 0x48 });
                            RPM.WriteNop(0x140147CA4, new byte[] { 0x89, 0x73, 0x48 });
                            RPM.WriteNop(0x140B43E95, new byte[] { 0x01 });
                            Teleported = false;
                        }

                        /*if (RPM.IsValid(pClientParachuteComponent))
                        {
                            RPM.WriteByte(pClientParachuteComponent + 0x1A, 1);
                            RPM.WriteByte(pClientParachuteComponent + 0x14C, 1);
                            RPM.WriteByte(pClientParachuteComponent + 0xB4, 0);
                        }*/
                    }
                    /*Int64 pValutData = RPM.ReadInt64(pclientVaultComponent + Offsets.ClientSoldierVaultComponent.m_data);
                    if (RPM.IsValid(pValutData))
                    {
                        RPM.WriteFloat(pValutData + Offsets.ClientSoldierVaultComponent.m_startHeightMax, 225.0f);
                    }*/
                }
            }
            /*else
            {
                Int64 pclientVaultComponent = RPM.ReadInt64(pLocalSoldier + Offsets.ClientSoldierEntity.m_clientVaultComponent);
                if (RPM.IsValid(pclientVaultComponent))
                {
                    Int64 pValutData = RPM.ReadInt64(pclientVaultComponent + Offsets.ClientSoldierVaultComponent.m_data);
                    if (RPM.IsValid(pValutData))
                    {
                        RPM.WriteFloat(pValutData + Offsets.ClientSoldierVaultComponent.m_startHeightMax, 1.5f);
                    }
                }
            }*/
        }

        private void Aimbot()
        {
            int Fov = rect.Width / 100 * (AimbotMenuItems[AimbotMenuList.AIM_Fov].Value * 5 + 5);
            Player ClosestSoldier = new Player();
            List<Player> DistancePlayerList = new List<Player>();
            List<Player> FovPlayerList = new List<Player>();
            bool IsSameTarget = false;
            bool DistanceAiming = false;
            bool AutoAimCheck = AimbotMenuItems[AimbotMenuList.AIM_AutoAim].Enabled ? GetAimKey() : true;
            foreach (Player player in players)
            {
                bool VisibleCheck = player.IsVisible() ? true : AimbotMenuItems[AimbotMenuList.AIM_Visible_Check].Enabled ? false : true;

                if (AimbotMenuItems[AimbotMenuList.AIM_StickTarget].Enabled && !AimbotMenuItems[AimbotMenuList.AIM_AutoAim].Enabled && IsTargetting && player.Name == LastTargetName && player.IsValid() && player.DistanceCrosshair < Fov && (VisibleCheck || (player.InVehicle && ((player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled) || player.BoneCheck))))
                {
                    ClosestSoldier = player;
                    IsSameTarget = true;
                    break;
                }
                else if (AimbotMenuItems[AimbotMenuList.AIM_AimAtAll].Enabled)
                {
                    if (AimbotMenuItems[AimbotMenuList.AIM_Type].Value == 0)
                    {
                        if (player.IsValid() && player.Team != localPlayer.Team && (VisibleCheck || (AutoAimCheck && player.InVehicle && ((player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled) || player.BoneCheck))))
                        {
                            if (player.Distance <= 50)
                            {
                                DistancePlayerList.Add(player);
                                DistanceAiming = true;
                            }
                            else if (!DistanceAiming && player.DistanceCrosshair < Fov)
                            {
                                FovPlayerList.Add(player);
                            }
                        }
                    }
                    else if (AimbotMenuItems[AimbotMenuList.AIM_Type].Value == 1)
                    {
                        if (player.IsValid() && player.Team != localPlayer.Team && (player.Distance < 7 || player.DistanceCrosshair < Fov) && (VisibleCheck || (AutoAimCheck && player.InVehicle && ((player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled) || player.BoneCheck))))
                        {
                            FovPlayerList.Add(player);
                        }
                    }
                    else
                    {
                        if (player.IsValid() && player.Team != localPlayer.Team && (VisibleCheck || (AutoAimCheck && player.InVehicle && ((player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled) || player.BoneCheck))))
                        {
                            DistancePlayerList.Add(player);
                        }
                    }
                }
                else
                {
                    if (AimbotMenuItems[AimbotMenuList.AIM_Type].Value == 0)
                    {
                        if (player.IsValid() && player.Team != localPlayer.Team && (VisibleCheck || (AutoAimCheck && player.InVehicle && player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled)))
                        {
                            if (player.Distance <= 50)
                            {
                                DistancePlayerList.Add(player);
                                DistanceAiming = true;
                            }
                            else if (!DistanceAiming && player.DistanceCrosshair < Fov)
                            {
                                FovPlayerList.Add(player);
                            }
                        }
                    }
                    else if (AimbotMenuItems[AimbotMenuList.AIM_Type].Value == 1)
                    {
                        if (player.IsValid() && player.Team != localPlayer.Team && (player.Distance < 7 || player.DistanceCrosshair < Fov) && (VisibleCheck || (AutoAimCheck && player.InVehicle && player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled)))
                        {
                            FovPlayerList.Add(player);
                        }
                    }
                    else
                    {
                        if (player.IsValid() && player.Team != localPlayer.Team && (VisibleCheck || (AutoAimCheck && player.InVehicle && player.IsDriver && localPlayer.InVehicle && AimbotMenuItems[AimbotMenuList.AIM_Vehicle].Enabled)))
                        {
                            DistancePlayerList.Add(player);
                        }
                    }
                }
            }
            if (IsSameTarget || DistancePlayerList.Count > 0 || FovPlayerList.Count > 0)
            {
                if (!IsSameTarget)
                {
                    if (AimbotMenuItems[AimbotMenuList.AIM_Type].Value == 0)
                    {
                        if (DistanceAiming)
                        {
                            ClosestSoldier = DistanceSortPlayers(DistancePlayerList);
                            //DrawText(5, 440, "Distance Aiming", Color.White, true);
                        }
                        else
                        {
                            ClosestSoldier = DistanceCrosshairSortPlayers(FovPlayerList);
                            //DrawText(5, 440, "Fov Aiming", Color.White, true);
                        }
                    }
                    else if (AimbotMenuItems[AimbotMenuList.AIM_Type].Value == 1)
                    {
                        ClosestSoldier = DistanceCrosshairSortPlayers(FovPlayerList);
                    }
                    else
                    {
                        ClosestSoldier = DistanceSortPlayers(DistancePlayerList);
                    }
                }
                Vector3 AimLocation = default(Vector3);
                bool driverBoneCheck = AimbotMenuItems[AimbotMenuList.AIM_Driver_First].Enabled && ClosestSoldier.BoneCheck;
                if (ClosestSoldier.InVehicle && localPlayer.InVehicle && !driverBoneCheck)
                {
                    AimLocation = ClosestSoldier.Origin;
                    AimLocation.Y += 0.4f;
                }
                else
                {
                    if (AimbotMenuItems[AimbotMenuList.AIM_Location].Value == 0)
                    {
                        AimLocation = ClosestSoldier.Bone.BONE_HEAD;
                    }
                    else if (AimbotMenuItems[AimbotMenuList.AIM_Location].Value == 1)
                    {
                        AimLocation = ClosestSoldier.Bone.BONE_NECK;
                    }
                    else if (AimbotMenuItems[AimbotMenuList.AIM_Location].Value == 2)
                    {
                        AimLocation = ClosestSoldier.Bone.BONE_SPINE2;
                    }
                    else if (AimbotMenuItems[AimbotMenuList.AIM_Location].Value == 3)
                    {
                        AimLocation = ClosestSoldier.Bone.BONE_SPINE1;
                    }
                    else
                    {
                        AimLocation = ClosestSoldier.Bone.BONE_SPINE;
                    }
                }
                if (!AimLocation.IsZero)
                {
                    Vector3 CorrectedDistanceVector = new Vector3
                    {
                        X = AimLocation.X - m_ViewMatrixInverse.M41,
                        Y = AimLocation.Y - (m_ViewMatrixInverse.M42/*HeadGlitch Update Fix*/ + localPlayer.BulletInitialPosition.Y),
                        Z = AimLocation.Z - m_ViewMatrixInverse.M43
                    };
                    float CorrectedClosestSoldierDistance = (float)Math.Sqrt((CorrectedDistanceVector.X * CorrectedDistanceVector.X) + (CorrectedDistanceVector.Y * CorrectedDistanceVector.Y) + (CorrectedDistanceVector.Z * CorrectedDistanceVector.Z));
                    //DrawText(5, 420, "D2:" + CorrectedClosestSoldierDistance, Color.White, true);
                    AimLocation = AimCorrection(localPlayer.Velocity, ClosestSoldier.Velocity, AimLocation, localPlayer.InVehicle ? ClosestSoldier.Distance : CorrectedClosestSoldierDistance, localPlayer.BulletSpeed, localPlayer.BulletGravity);
                    if (!AimLocation.IsZero)
                    {
                        Vector3 Screen;
                        #region Vehicle Aimbot Thingy
                        if (localPlayer.InVehicle && WorldToScreen(AimLocation, out Screen))
                        {
                            float ScreenX = rect.Width / 2;
                            float ScreenY = rect.Height / 2;
                            long pVehicleWeapon = Offsets.VehicleWeapon.GetInstance();
                            if (RPM.IsValid(pVehicleWeapon))
                            {
                                long pClientCameraComponent = RPM.ReadInt64(pVehicleWeapon + Offsets.VehicleWeapon.m_pClientCameraComponent);
                                if (RPM.IsValid(pClientCameraComponent))
                                {
                                    long pChaseorStaticCamera = RPM.ReadInt64(pClientCameraComponent + Offsets.ClientCameraComponent.m_pChaseorStaticCamera);
                                    if (RPM.IsValid(pChaseorStaticCamera))
                                    {
                                        Matrix CrossMatrix = RPM.ReadMatrix(pChaseorStaticCamera + Offsets.StaticCamera.m_CrossMatrix);
                                        Vector3 ForwardOffset = RPM.ReadVector3(pChaseorStaticCamera + Offsets.StaticCamera.m_ForwardOffset);
                                        Vector3 EVposition = new Vector3(CrossMatrix.M41, CrossMatrix.M42, CrossMatrix.M43);
                                        Vector3 EVVelocity = new Vector3(-CrossMatrix.M31, -CrossMatrix.M32, -CrossMatrix.M33);
                                        EVVelocity = -ForwardOffset;
                                        Vector3 VAimPosition = EVposition + EVVelocity * 100f;
                                        Vector3 VScreen;
                                        if (WorldToScreen(VAimPosition, out VScreen) && ForwardOffset.Y != 0f && ForwardOffset.Z != 0f && Math.Abs(rect.Center.Y - (int)VScreen.Y) <= 150 && Math.Abs(rect.Center.X - (int)VScreen.X) <= 150)
                                        {
                                            ScreenX = VScreen.X;
                                            ScreenY = VScreen.Y;
                                        }
                                    }
                                }
                            }
                            long pBorderInputNode = RPM.ReadInt64(Offsets.BorderInputNode.GetInstance());
                            if (RPM.IsValid(pBorderInputNode))
                            {
                                long pMouse = RPM.ReadInt64(pBorderInputNode + Offsets.BorderInputNode.m_pMouse);
                                if (RPM.IsValid(pMouse))
                                {
                                    long pDevice = RPM.ReadInt64(pMouse + Offsets.Mouse.m_pDevice);
                                    if (RPM.IsValid(pDevice))
                                    {
                                        int MouseDeltaX = (int)(Screen.X - ScreenX);
                                        int MouseDeltaY = (int)(Screen.Y - ScreenY);
                                        if (Math.Abs(MouseDeltaX) > 5)
                                        {
                                            RPM.WriteInt32(pDevice + Offsets.MouseDevice.m_Buffer + Offsets.MouseDevice.x, MouseDeltaX / 2);
                                        }
                                        else
                                        {
                                            RPM.WriteInt32(pDevice + Offsets.MouseDevice.m_Buffer + Offsets.MouseDevice.x, MouseDeltaX);
                                        }
                                        if (Math.Abs(MouseDeltaY) > 5)
                                        {
                                            RPM.WriteInt32(pDevice + Offsets.MouseDevice.m_Buffer + Offsets.MouseDevice.y, MouseDeltaY / 2);
                                        }
                                        else
                                        {
                                            RPM.WriteInt32(pDevice + Offsets.MouseDevice.m_Buffer + Offsets.MouseDevice.y, MouseDeltaY);
                                        }
                                        localPlayer.IsMeAiming = true;
                                    }
                                }
                            }
                        }
                        #endregion
                        else
                        {
                            Vector2 hitLocation = GetHitLocationPlayer(AimLocation);
                            if (hitLocation != hitLocation)
                                return;
                            RPM.WriteAngle(hitLocation.X, hitLocation.Y);
                            
                            localPlayer.IsMeAiming = true;
                        }
                        if (AimbotMenuItems[AimbotMenuList.AIM_AutoShoot].Enabled && localPlayer.IsMeAiming && !ClosestSoldier.InVehicle)
                        {
                            if (!AutoShootStream.IsAlive)
                            {
                                AutoShootStream = new Thread(AutoShootLogic);
                                AutoShootStream.Start();
                            }
                        }
                        LastTargetName = ClosestSoldier.Name;
                    }
                }
            }
            IsTargetting = true;
        }

        private bool GetAimKey()
        {
            int key;
            if (localPlayer.IsVehicleWeapon)
            {
                key = Aim_Keys[AimbotMenuItems[AimbotMenuList.AIM_Vehicle_Key].Value];
            }
            else
            {
                key = Aim_Keys[AimbotMenuItems[AimbotMenuList.AIM_Key].Value];
            }
            bool AimKey = Convert.ToBoolean(Managed.GetKeyState(key) & Managed.KEY_PRESSED);
            return AimKey;
        }

        private void AutoShootLogic()
        {
            Mouse.PressButton(Mouse.MouseKeys.Left, 10);
        }

        // Get SoldierEntity
        private long GetClientSoldierEntity(long pClientPlayer, Player player)
        {
            long pAttached = RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_pAttachedControllable);
            if (RPM.IsValid(pAttached))
            {
                long m_ClientSoldier = RPM.ReadInt64(RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_character)) - sizeof(long);
                if (RPM.IsValid(m_ClientSoldier))
                {
                    player.InVehicle = true;

                    long pVehicleEntity = RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_pAttachedControllable);
                    if (RPM.IsValid(pVehicleEntity))
                    {
                        // Driver
                        if (RPM.ReadInt32(pClientPlayer + Offsets.ClientPlayer.m_attachedEntryId) == 0)
                        {
                            // Vehicle AABB
                            if (VisualMenuItems[VisualMenuList.ESP_Vehicle].Enabled)
                            {
                                long pDynamicPhysicsEntity = RPM.ReadInt64(pVehicleEntity + Offsets.ClientVehicleEntity.m_pPhysicsEntity);
                                if (RPM.IsValid(pDynamicPhysicsEntity))
                                {
                                    long pPhysicsEntity = RPM.ReadInt64(pDynamicPhysicsEntity + Offsets.DynamicPhysicsEntity.m_EntityTransform);
                                    player.VehicleTranfsorm = RPM.ReadMatrix(pPhysicsEntity + Offsets.PhysicsEntityTransform.m_Transform);
                                    player.VehicleAABB = RPM.ReadAABB(pVehicleEntity + Offsets.ClientVehicleEntity.m_childrenAABB);
                                }
                            }
                            long _EntityData = RPM.ReadInt64(pVehicleEntity + Offsets.ClientSoldierEntity.m_data);
                            if (RPM.IsValid(_EntityData))
                            {
                                long _NameSid = RPM.ReadInt64(_EntityData + Offsets.VehicleEntityData.m_NameSid);

                                string strName = RPM.ReadString(_NameSid, 20);
                                if (strName.Length > 11)
                                {
                                    long pAttachedClient = RPM.ReadInt64(m_ClientSoldier + Offsets.ClientSoldierEntity.m_pPlayer);
                                    // AttachedControllable Max Health
                                    long p = RPM.ReadInt64(pAttachedClient + Offsets.ClientPlayer.m_pAttachedControllable);
                                    long p2 = RPM.ReadInt64(p + Offsets.ClientSoldierEntity.m_pHealthComponent);
                                    player.VehicleHealth = RPM.ReadFloat(p2 + Offsets.HealthComponent.m_vehicleHealth);

                                    // AttachedControllable Health
                                    player.VehicleMaxHealth = RPM.ReadFloat(_EntityData + Offsets.VehicleEntityData.m_FrontMaxHealth);

                                    // AttachedControllable Name
                                    player.VehicleName = strName.Remove(0, 11);
                                    player.IsDriver = true;
                                }
                            }
                        }
                        if (AimbotMenuItems[AimbotMenuList.AIM].Enabled)
                        {
                            long VehicleChassis = RPM.ReadInt64(pVehicleEntity + Offsets.ClientVehicleEntity.m_Chassis);
                            player.Velocity = RPM.ReadVector3(VehicleChassis + Offsets.ClientChassisComponent.m_Velocity);
                        }
                    }
                }
                return m_ClientSoldier;
            }
            return RPM.ReadInt64(pClientPlayer + Offsets.ClientPlayer.m_pControlledControllable);
        }

        // Get Window Rect
        private void SetWindow(object sender)
        {
            while (true)
            {
                IntPtr targetWnd = IntPtr.Zero;
                targetWnd = Managed.FindWindow(null, "Battlefield 4");

                if (targetWnd != IntPtr.Zero)
                {
                    // Game is Selected
                    if (Managed.GetForegroundWindow() != targetWnd)
                    {
                        IsGameWindowSelected = false;
                        continue;
                    }
                    // Reset
                    IsGameWindowSelected = true;
                    RECT targetSize = new RECT();
                    Managed.GetWindowRect(targetWnd, out targetSize);
                    RECT borderSize = new RECT();
                    Managed.GetClientRect(targetWnd, out borderSize);

                    int dwStyle = Managed.GetWindowLong(targetWnd, Managed.GWL_STYLE);

                    int windowheight;
                    int windowwidth;
                    int borderheight;
                    int borderwidth;

                    if (rect.Width != (targetSize.Bottom - targetSize.Top)
                        && rect.Width != (borderSize.Right - borderSize.Left))
                        IsResize = true;

                    rect.Width = targetSize.Right - targetSize.Left;
                    rect.Height = targetSize.Bottom - targetSize.Top;

                    if ((dwStyle & Managed.WS_BORDER) != 0)
                    {
                        windowheight = targetSize.Bottom - targetSize.Top;
                        windowwidth = targetSize.Right - targetSize.Left;

                        rect.Height = borderSize.Bottom - borderSize.Top;
                        rect.Width = borderSize.Right - borderSize.Left;

                        borderheight = (windowheight - borderSize.Bottom);
                        borderwidth = (windowwidth - borderSize.Right) / 2; //only want one side
                        borderheight -= borderwidth; //remove bottom

                        targetSize.Left += borderwidth;
                        targetSize.Top += borderheight;

                        rect.Left = targetSize.Left;
                        rect.Top = targetSize.Top;
                    }
                    Managed.MoveWindow(handle, targetSize.Left, targetSize.Top, rect.Width, rect.Height, true);
                }
                Thread.Sleep(300);
            }
        }

        private float Distance_Crosshair(Player player)
        {
            Vector3 vector;
            if (WorldToScreen(player.Origin, out vector))
            {
                float X = Math.Abs(vector.X - rect.Center.X);
                float Y = Math.Abs(vector.Y - rect.Center.Y);
                return (float)Math.Sqrt(X * X + Y * Y);
            }
            return 9999f;
        }

        // 3D In 2D
        private bool WorldToScreen(Vector3 _Enemy, int _Pose, out Vector3 _Screen)
        {
            _Screen = new Vector3(0, 0, 0);
            float HeadHeight = _Enemy.Y;

            #region HeadHeight
            if (_Pose == 0)
            {
                HeadHeight += 1.7f;
            }
            if (_Pose == 1)
            {
                HeadHeight += 1.15f;
            }
            if (_Pose == 2)
            {
                HeadHeight += 0.4f;
            }
            #endregion

            float ScreenW = (viewProj.M14 * _Enemy.X) + (viewProj.M24 * HeadHeight) + (viewProj.M34 * _Enemy.Z + viewProj.M44);

            if (ScreenW < 0.0001f)
                return false;

            float ScreenX = (viewProj.M11 * _Enemy.X) + (viewProj.M21 * HeadHeight) + (viewProj.M31 * _Enemy.Z + viewProj.M41);
            float ScreenY = (viewProj.M12 * _Enemy.X) + (viewProj.M22 * HeadHeight) + (viewProj.M32 * _Enemy.Z + viewProj.M42);

            _Screen.X = (rect.Width / 2) + (rect.Width / 2) * ScreenX / ScreenW;
            _Screen.Y = (rect.Height / 2) - (rect.Height / 2) * ScreenY / ScreenW;
            _Screen.Z = ScreenW;
            return true;
        }

        // 3D In 2D
        private bool WorldToScreen(Vector3 _Enemy, out Vector3 _Screen)
        {
            _Screen = new Vector3(0, 0, 0);
            float ScreenW = (viewProj.M14 * _Enemy.X) + (viewProj.M24 * _Enemy.Y) + (viewProj.M34 * _Enemy.Z + viewProj.M44);

            if (ScreenW < 0.0001f)
                return false;

            float ScreenX = (viewProj.M11 * _Enemy.X) + (viewProj.M21 * _Enemy.Y) + (viewProj.M31 * _Enemy.Z + viewProj.M41);
            float ScreenY = (viewProj.M12 * _Enemy.X) + (viewProj.M22 * _Enemy.Y) + (viewProj.M32 * _Enemy.Z + viewProj.M42);

            _Screen.X = (rect.Width / 2) + (rect.Width / 2) * ScreenX / ScreenW;
            _Screen.Y = (rect.Height / 2) - (rect.Height / 2) * ScreenY / ScreenW;
            _Screen.Z = ScreenW;
            return true;
        }

        // Get Roll
        private bool GetBonyById(long pEnemySoldier, int Id, out Vector3 _World)
        {
            _World = new Vector3();

            long pRagdollComp = RPM.ReadInt64(pEnemySoldier + Offsets.ClientSoldierEntity.m_ragdollComponent);
            if (!RPM.IsValid(pRagdollComp))
                return false;

            byte m_ValidTransforms = RPM.ReadByte(pRagdollComp + (Offsets.ClientRagDollComponent.m_ragdollTransforms + Offsets.UpdatePoseResultData.m_ValidTransforms));
            if (m_ValidTransforms != 1)
                return false;

            long pQuatTransform = RPM.ReadInt64(pRagdollComp + (Offsets.ClientRagDollComponent.m_ragdollTransforms + Offsets.UpdatePoseResultData.m_ActiveWorldTransforms));
            if (!RPM.IsValid(pQuatTransform))
                return false;

            _World = RPM.ReadVector3(pQuatTransform + Id * 0x20);
            return true;
        }

        // Close window event
        private void DrawWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateStream.Abort();
            windowStream.Abort();
            RPM.CloseProcess();
            // Close main process
            Environment.Exit(0);
        }

        public Bitmap LoadBitmap(System.Drawing.Bitmap drawingBitmap)
        {
            System.Drawing.Imaging.BitmapData bitmapData = drawingBitmap.LockBits(new System.Drawing.Rectangle(0, 0, drawingBitmap.Width, drawingBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            DataStream from = new DataStream(bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, true, false);
            BitmapProperties bitmapProperties = default(BitmapProperties);
            bitmapProperties.PixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
            Bitmap result = new Bitmap(device, new Size2(drawingBitmap.Width, drawingBitmap.Height), from, bitmapData.Stride, bitmapProperties);
            drawingBitmap.UnlockBits(bitmapData);
            return result;
        }
        private Vector2 GetHitLocationPlayer(Vector3 vAimLocation)
        {
            Vector3 VTarget = Vector3.Normalize(new Vector3
            {
                X = vAimLocation.X - m_ViewMatrixInverse.M41,
                Y = vAimLocation.Y - (m_ViewMatrixInverse.M42/*HeadGlitch Update Fix*/ + localPlayer.BulletInitialPosition.Y),
                Z = vAimLocation.Z - m_ViewMatrixInverse.M43
            });
            //HeadGlitch Update Fix
            float ExtraPitch = (float)Math.Atan2(localPlayer.BulletInitialSpeed.Y, localPlayer.BulletInitialSpeed.Z);
            //DrawText(5, 460, "ExtraPitch:" + ExtraPitch, Color.White, true);
            if (localPlayer.zeroingDistanceLevel != -1)
            {
                float ExtraAngle = (float)(localPlayer.zeroingModes.Y * (Math.PI / 180)); // 0.017453292519943295 Convert to radian
                ExtraPitch += ExtraAngle;
                //DrawText(5, 480, "FinalExtraPitch:" + ExtraPitch, Color.White, true);
            }
            Vector2 FinalAimLocation;
            FinalAimLocation.X = -(float)Math.Atan2((double)VTarget.X, (double)VTarget.Z);
            FinalAimLocation.Y = (float)Math.Atan2(VTarget.Y, Math.Sqrt(VTarget.X * VTarget.X + VTarget.Z * VTarget.Z));
            FinalAimLocation.Y -= ExtraPitch;
            FinalAimLocation -= localPlayer.Sway;
            return FinalAimLocation;
        }
        private Vector3 AimCorrection(Vector3 MyVelocity, Vector3 EnemyVelocity, Vector3 InVec, float EnemyDistance, float BulletSpeed, float BulletGravity)
        {
            float m_grav = Math.Abs(BulletGravity);
            float m_dist = EnemyDistance / Math.Abs(BulletSpeed);
            InVec += EnemyVelocity * m_dist;
            if (!localPlayer.IsVehicleWeapon)
            {
                InVec -= MyVelocity * m_dist;
            }
            InVec.Y += 0.5f * m_grav * m_dist * m_dist;
            //InVec.Y -= BulletInitialSpeed.Y * m_dist; //Can Use This Instead Of ExtraPitch
            return InVec;
        }

        private Player DistanceSortPlayers(List<Player> _Players)
        {
            List<Player> list = (
                from a in _Players
                orderby a.Distance
                select a).ToList<Player>();
            return list[0];
        }

        private Player DistanceCrosshairSortPlayers(List<Player> _Players)
        {
            List<Player> list = (
                from a in _Players
                orderby a.DistanceCrosshair
                select a).ToList<Player>();
            return list[0];
        }

        // Multiply Vector's
        public Vector3 Multiply(Vector3 vector, Matrix mat)
        {
            return new Vector3(mat.M11 * vector.X + mat.M21 * vector.Y + mat.M31 * vector.Z,
                                   mat.M12 * vector.X + mat.M22 * vector.Y + mat.M32 * vector.Z,
                                   mat.M13 * vector.X + mat.M23 * vector.Y + mat.M33 * vector.Z);
        }

        private void GetSpectators()
        {
            long pGameContext = RPM.ReadInt64(Offsets.ClientGameContext.GetInstance());
            if (!RPM.IsValid(pGameContext))
                return;

            long PManager = RPM.ReadInt64(pGameContext + Offsets.ClientGameContext.m_pPlayerManager);
            if (!RPM.IsValid(PManager))
                return;

            long pLPlayer = RPM.ReadInt64(PManager + Offsets.ClientPlayerManager.m_pLocalPlayer);
            if (!RPM.IsValid(pLPlayer))
                return;

            long pSpectator = RPM.ReadInt64(PManager + 0x0308);
            if (!RPM.IsValid(pSpectator))
                return;

            long speccount = (RPM.ReadInt64(PManager + 0x0310) - pSpectator) / 8;
            //DrawText(5, 510, "Spectator Count:" + speccount, Color.White, true);
            spectatorCount = 0;
            int WatchingCount = -1;
            //DrawText(5, 360, "Spectatorfukyou:Valid", Color.White, true);
            for (int i = 0; i < speccount; i++)
            {
                //DrawText(5, 380, "Spectatorfukyou:InLoop", Color.White, true);
                long pSpectatorClientPlayer = RPM.ReadInt64(pSpectator + (i * sizeof(long)));
                if (!RPM.IsValid(pSpectatorClientPlayer))
                    continue;

                bool spec = Convert.ToBoolean(RPM.ReadByte(pSpectatorClientPlayer + Offsets.ClientPlayer.m_isSpectator));
                if (spec)
                {
                    spectatorCount++;
                    //String SpecName = NullTerminatedStringFix(RPM.ReadString2(pSpectatorClientPlayer + Offsets.ClientPlayer.szName, 0x10));
                    String SpecName = RPM.ReadString(pSpectatorClientPlayer + Offsets.ClientPlayer.szName, 0x10);
                    //DrawText(5, 400, "SpectatorfukyouName:" + SpecName, Color.White, true);
                    DrawWarn(rect.Width - 160, rect.Height / 2 - 150, 150, 80, SpecName, "Spectators", i);

                    long pSpectatorPlayerView = RPM.ReadInt64(pSpectatorClientPlayer + Offsets.ClientPlayer.m_PlayerView);
                    if (RPM.IsValid(pSpectatorPlayerView) && RPM.ReadInt64(pSpectatorPlayerView + Offsets.ClientPlayerView.m_Owner) == pLPlayer)
                    {
                        WatchingCount++;
                        DrawWarn(rect.Width - 160, rect.Height / 2 - 60, 150, 80, SpecName, "Watching", WatchingCount);
                        //DrawText(5, 420 + (i * 20), "Spectatorfukyou[" + i + "]:" + SpecName + " is watching you", Color.White, true);
                    }
                }
            }
        }

        /*private string NullTerminatedStringFix(string NullTerminatedString)
        {
            string CorrectedString = null;
            for (int j = 0; j < NullTerminatedString.Length; j++)
            {
                if (NullTerminatedString[j] != 0)
                {
                    CorrectedString += NullTerminatedString[j];
                }
                else
                {
                    break;
                }
            }
            return CorrectedString;
        }*/

        public string PadBoth(string source, int length)
        {
            int spaces = length - source.Length;
            int padLeft = spaces / 2 + source.Length;
            return source.PadLeft(padLeft).PadRight(length);
        }

        // Draw Functions
        #region Draw Functions
        private void DrawRect(int X, int Y, int W, int H, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawRectangle(new Rectangle(X, Y, W, H), solidColorBrush);
        }

        private void DrawRect(int X, int Y, int W, int H, Color color, float stroke)
        {
            solidColorBrush.Color = color;
            device.DrawRectangle(new Rectangle(X, Y, W, H), solidColorBrush, stroke);
        }

        private void DrawFillRect(int X, int Y, int W, int H, Color color)
        {
            solidColorBrush.Color = color;
            device.FillRectangle(new RectangleF(X, Y, W, H), solidColorBrush);
        }

        private void DrawTriangle(Vector2[] pts, Color color)
        {
            Vector2[] array = new Vector2[]
			{
				new Vector2(pts[0].X, pts[0].Y),
				new Vector2(pts[1].X, pts[1].Y),
				new Vector2(pts[2].X, pts[2].Y)
			};
            PathGeometry pathGeometry = new PathGeometry(factory);
            GeometrySink geometrySink = pathGeometry.Open();
            geometrySink.BeginFigure(array[0], FigureBegin.Filled);
            geometrySink.AddLines(new Vector2[]
			{
				array[1],
				array[2]
			});
            geometrySink.EndFigure(FigureEnd.Open);
            geometrySink.Close();
            solidColorBrush.Color = color;
            device.DrawGeometry(pathGeometry, solidColorBrush);
            device.FillGeometry(pathGeometry, solidColorBrush);
            pathGeometry.Dispose();
            geometrySink.Dispose();
        }

        private void DrawText(int X, int Y, string text, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawText(text, fontLarge, new RectangleF(X, Y, fontLarge.FontSize * text.Length, fontLarge.FontSize), solidColorBrush);
        }

        private void DrawText(int X, int Y, string text, Color color, bool outline)
        {
            if (outline)
            {
                solidColorBrush.Color = Color.Black;
                device.DrawText(text, fontLarge, new RectangleF(X + 1, Y + 1, fontLarge.FontSize * text.Length, fontLarge.FontSize), solidColorBrush);
            }

            solidColorBrush.Color = color;
            device.DrawText(text, fontLarge, new RectangleF(X, Y, fontLarge.FontSize * text.Length, fontLarge.FontSize), solidColorBrush);
        }

        private void DrawText(int X, int Y, string text, Color color, bool outline, TextFormat format)
        {
            if (outline)
            {
                solidColorBrush.Color = Color.Black;
                device.DrawText(text, format, new RectangleF(X + 1, Y + 1, format.FontSize * text.Length, format.FontSize), solidColorBrush);
            }

            solidColorBrush.Color = color;
            device.DrawText(text, format, new RectangleF(X, Y, format.FontSize * text.Length, format.FontSize), solidColorBrush);
        }

        private void DrawTextCenter(int X, int Y, int W, int H, string text, Color color)
        {
            solidColorBrush.Color = color;
            TextLayout layout = new TextLayout(fontFactory, text, fontSmall, W, H);
            layout.TextAlignment = TextAlignment.Center;
            device.DrawTextLayout(new Vector2(X, Y), layout, solidColorBrush);
            layout.Dispose();
        }

        private void DrawTextCenter(int X, int Y, int W, int H, string text, Color color, bool outline)
        {
            TextLayout layout = new TextLayout(fontFactory, text, fontSmall, W, H);
            layout.TextAlignment = TextAlignment.Center;

            if (outline)
            {
                solidColorBrush.Color = Color.Black;
                device.DrawTextLayout(new Vector2(X + 1, Y + 1), layout, solidColorBrush);
            }

            solidColorBrush.Color = color;
            device.DrawTextLayout(new Vector2(X, Y), layout, solidColorBrush);
            layout.Dispose();
        }

        private void DrawLine(int X, int Y, int XX, int YY, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawLine(new Vector2(X, Y), new Vector2(XX, YY), solidColorBrush);
        }

        private void DrawLine(Vector3 w2s, Vector3 _w2s, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawLine(new Vector2(w2s.X, w2s.Y), new Vector2(_w2s.X, _w2s.Y), solidColorBrush);
        }

        private void DrawCircle(int X, int Y, int W, Color color)
        {
            solidColorBrush.Color = color;
            device.DrawEllipse(new Ellipse(new Vector2(X, Y), W, W), solidColorBrush);
        }

        private void DrawFillCircle(int X, int Y, int W, Color color)
        {
            solidColorBrush.Color = color;
            device.FillEllipse(new Ellipse(new Vector2(X, Y), W, W), solidColorBrush);
        }

        private void DrawImage(int X, int Y, int W, int H, Bitmap bitmap)
        {
            device.DrawBitmap(bitmap, new RectangleF(X, Y, W, H), 1.0f, BitmapInterpolationMode.Linear);
        }

        private void DrawImage(int X, int Y, int W, int H, Bitmap bitmap, float angle)
        {
            device.Transform = Matrix3x2.Rotation(angle, new Vector2(X + (H / 2), Y + (H / 2)));
            device.DrawBitmap(bitmap, new RectangleF(X, Y, W, H), 1.0f, BitmapInterpolationMode.Linear);
            device.Transform = Matrix3x2.Rotation(0);
        }

        private void DrawSprite(RectangleF destinationRectangle, Bitmap bitmap, RectangleF sourceRectangle)
        {
            device.DrawBitmap(bitmap, destinationRectangle, 1.0f, BitmapInterpolationMode.Linear, sourceRectangle);
        }

        private void DrawSprite(RectangleF destinationRectangle, Bitmap bitmap, RectangleF sourceRectangle, float angle)
        {
            Vector2 center = new Vector2();
            center.X = destinationRectangle.X + destinationRectangle.Width / 2;
            center.Y = destinationRectangle.Y + destinationRectangle.Height / 2;

            device.Transform = Matrix3x2.Rotation(angle, center);
            device.DrawBitmap(bitmap, destinationRectangle, 1.0f, BitmapInterpolationMode.Linear, sourceRectangle);
            device.Transform = Matrix3x2.Rotation(0);
        }

        private void DrawBone(Player player)
        {
            Vector3 BONE_HEAD,
            BONE_NECK,
            BONE_SPINE2,
            BONE_SPINE1,
            BONE_SPINE,
            BONE_LEFTSHOULDER,
            BONE_RIGHTSHOULDER,
            BONE_LEFTELBOWROLL,
            BONE_RIGHTELBOWROLL,
            BONE_LEFTHAND,
            BONE_RIGHTHAND,
            BONE_LEFTKNEEROLL,
            BONE_RIGHTKNEEROLL,
            BONE_LEFTFOOT,
            BONE_RIGHTFOOT;

            if (WorldToScreen(player.Bone.BONE_HEAD, out BONE_HEAD) &&
            WorldToScreen(player.Bone.BONE_NECK, out BONE_NECK) &&
            WorldToScreen(player.Bone.BONE_SPINE2, out BONE_SPINE2) &&
            WorldToScreen(player.Bone.BONE_SPINE1, out BONE_SPINE1) &&
            WorldToScreen(player.Bone.BONE_SPINE, out BONE_SPINE) &&
            WorldToScreen(player.Bone.BONE_LEFTSHOULDER, out BONE_LEFTSHOULDER) &&
            WorldToScreen(player.Bone.BONE_RIGHTSHOULDER, out BONE_RIGHTSHOULDER) &&
            WorldToScreen(player.Bone.BONE_LEFTELBOWROLL, out BONE_LEFTELBOWROLL) &&
            WorldToScreen(player.Bone.BONE_RIGHTELBOWROLL, out BONE_RIGHTELBOWROLL) &&
            WorldToScreen(player.Bone.BONE_LEFTHAND, out BONE_LEFTHAND) &&
            WorldToScreen(player.Bone.BONE_RIGHTHAND, out BONE_RIGHTHAND) &&
            WorldToScreen(player.Bone.BONE_LEFTKNEEROLL, out BONE_LEFTKNEEROLL) &&
            WorldToScreen(player.Bone.BONE_RIGHTKNEEROLL, out BONE_RIGHTKNEEROLL) &&
            WorldToScreen(player.Bone.BONE_LEFTFOOT, out BONE_LEFTFOOT) &&
            WorldToScreen(player.Bone.BONE_RIGHTFOOT, out BONE_RIGHTFOOT))
            {
                int stroke = 3;
                int strokeW = stroke % 2 == 0 ? stroke / 2 : (stroke - 1) / 2;

                // Color
                Color skeletonColor = player.Team == localPlayer.Team ? friendSkeletonColor : enemySkeletonColor;

                // RECT's
                DrawFillRect((int)BONE_HEAD.X - strokeW, (int)BONE_HEAD.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_NECK.X - strokeW, (int)BONE_NECK.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTSHOULDER.X - strokeW, (int)BONE_LEFTSHOULDER.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTELBOWROLL.X - strokeW, (int)BONE_LEFTELBOWROLL.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTHAND.X - strokeW, (int)BONE_LEFTHAND.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTSHOULDER.X - strokeW, (int)BONE_RIGHTSHOULDER.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTELBOWROLL.X - strokeW, (int)BONE_RIGHTELBOWROLL.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTHAND.X - strokeW, (int)BONE_RIGHTHAND.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_SPINE2.X - strokeW, (int)BONE_SPINE2.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_SPINE1.X - strokeW, (int)BONE_SPINE1.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_SPINE.X - strokeW, (int)BONE_SPINE.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_LEFTKNEEROLL.X - strokeW, (int)BONE_LEFTKNEEROLL.Y - strokeW, stroke, stroke, skeletonColor);
                DrawFillRect((int)BONE_RIGHTKNEEROLL.X - strokeW, (int)BONE_RIGHTKNEEROLL.Y - strokeW, 2, 2, skeletonColor);
                DrawFillRect((int)BONE_LEFTFOOT.X - strokeW, (int)BONE_LEFTFOOT.Y - strokeW, 2, 2, skeletonColor);
                DrawFillRect((int)BONE_RIGHTFOOT.X - strokeW, (int)BONE_RIGHTFOOT.Y - strokeW, 2, 2, skeletonColor);

                // Head -> Neck
                DrawLine((int)BONE_HEAD.X, (int)BONE_HEAD.Y, (int)BONE_NECK.X, (int)BONE_NECK.Y, skeletonColor);

                // Neck -> Left
                DrawLine((int)BONE_NECK.X, (int)BONE_NECK.Y, (int)BONE_LEFTSHOULDER.X, (int)BONE_LEFTSHOULDER.Y, skeletonColor);
                DrawLine((int)BONE_LEFTSHOULDER.X, (int)BONE_LEFTSHOULDER.Y, (int)BONE_LEFTELBOWROLL.X, (int)BONE_LEFTELBOWROLL.Y, skeletonColor);
                DrawLine((int)BONE_LEFTELBOWROLL.X, (int)BONE_LEFTELBOWROLL.Y, (int)BONE_LEFTHAND.X, (int)BONE_LEFTHAND.Y, skeletonColor);

                // Neck -> Right
                DrawLine((int)BONE_NECK.X, (int)BONE_NECK.Y, (int)BONE_RIGHTSHOULDER.X, (int)BONE_RIGHTSHOULDER.Y, skeletonColor);
                DrawLine((int)BONE_RIGHTSHOULDER.X, (int)BONE_RIGHTSHOULDER.Y, (int)BONE_RIGHTELBOWROLL.X, (int)BONE_RIGHTELBOWROLL.Y, skeletonColor);
                DrawLine((int)BONE_RIGHTELBOWROLL.X, (int)BONE_RIGHTELBOWROLL.Y, (int)BONE_RIGHTHAND.X, (int)BONE_RIGHTHAND.Y, skeletonColor);

                // Neck -> Center
                DrawLine((int)BONE_NECK.X, (int)BONE_NECK.Y, (int)BONE_SPINE2.X, (int)BONE_SPINE2.Y, skeletonColor);
                DrawLine((int)BONE_SPINE2.X, (int)BONE_SPINE2.Y, (int)BONE_SPINE1.X, (int)BONE_SPINE1.Y, skeletonColor);
                DrawLine((int)BONE_SPINE1.X, (int)BONE_SPINE1.Y, (int)BONE_SPINE.X, (int)BONE_SPINE.Y, skeletonColor);

                // Spine -> Left
                DrawLine((int)BONE_SPINE.X, (int)BONE_SPINE.Y, (int)BONE_LEFTKNEEROLL.X, (int)BONE_LEFTKNEEROLL.Y, skeletonColor);
                DrawLine((int)BONE_LEFTKNEEROLL.X, (int)BONE_LEFTKNEEROLL.Y, (int)BONE_LEFTFOOT.X, (int)BONE_LEFTFOOT.Y, skeletonColor);

                // Spine -> Right
                DrawLine((int)BONE_SPINE.X, (int)BONE_SPINE.Y, (int)BONE_RIGHTKNEEROLL.X, (int)BONE_RIGHTKNEEROLL.Y, skeletonColor);
                DrawLine((int)BONE_RIGHTKNEEROLL.X, (int)BONE_RIGHTKNEEROLL.Y, (int)BONE_RIGHTFOOT.X, (int)BONE_RIGHTFOOT.Y, skeletonColor);
            }
        }

        private void DrawHealth(int X, int Y, int W, int H, int Health, int MaxHealth)
        {
            if (Health <= 0)
                Health = 1;

            if (MaxHealth < Health)
                MaxHealth = 100;

            int progress = (int)(Health / ((float)MaxHealth / 100));
            int w = (int)((float)W / 100 * progress);

            if (w <= 2)
                w = 3;

            Color color = new Color(255, 0, 0, 255);
            if (progress >= 20) color = new Color(255, 165, 0, 255);
            if (progress >= 40) color = new Color(255, 255, 0, 255);
            if (progress >= 60) color = new Color(173, 255, 47, 255);
            if (progress >= 80) color = new Color(0, 255, 0, 255);

            DrawFillRect(X, Y - 1, W + 1, H + 2, Color.Black);
            DrawFillRect(X + 1, Y, w - 1, H, color);
        }

        private void DrawProgress(int X, int Y, int W, int H, int Value, int MaxValue)
        {
            int progress = (int)(Value / ((float)MaxValue / 100));
            int w = (int)((float)W / 100 * progress);

            Color color = new Color(0, 255, 0, 255);
            if (progress >= 20) color = new Color(173, 255, 47, 255);
            if (progress >= 40) color = new Color(255, 255, 0, 255);
            if (progress >= 60) color = new Color(255, 165, 0, 255);
            if (progress >= 80) color = new Color(255, 0, 0, 255);

            DrawFillRect(X, Y - 1, W + 1, H + 2, Color.Black);
            if (w >= 2)
            {
                DrawFillRect(X + 1, Y, w - 1, H, color);
            }
        }

        private void DrawAABB(AxisAlignedBox aabb, Matrix tranform, Color color)
        {
            Vector3 m_Position = new Vector3(tranform.M41, tranform.M42, tranform.M43);
            Vector3 fld = Multiply(new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Min.Z), tranform) + m_Position;
            Vector3 brt = Multiply(new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 bld = Multiply(new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 frt = Multiply(new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Min.Z), tranform) + m_Position;
            Vector3 frd = Multiply(new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Min.Z), tranform) + m_Position;
            Vector3 brb = Multiply(new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 blt = Multiply(new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Max.Z), tranform) + m_Position;
            Vector3 flt = Multiply(new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Min.Z), tranform) + m_Position;

            #region WorldToScreen
            if (!WorldToScreen(fld, out fld) || !WorldToScreen(brt, out brt)
                || !WorldToScreen(bld, out bld) || !WorldToScreen(frt, out frt)
                || !WorldToScreen(frd, out frd) || !WorldToScreen(brb, out brb)
                || !WorldToScreen(blt, out blt) || !WorldToScreen(flt, out flt))
                return;
            #endregion

            #region DrawLines
            DrawLine(fld, flt, color);
            DrawLine(flt, frt, color);
            DrawLine(frt, frd, color);
            DrawLine(frd, fld, color);
            DrawLine(bld, blt, color);
            DrawLine(blt, brt, color);
            DrawLine(brt, brb, color);
            DrawLine(brb, bld, color);
            DrawLine(fld, bld, color);
            DrawLine(frd, brb, color);
            DrawLine(flt, blt, color);
            DrawLine(frt, brt, color);
            #endregion
        }

        private void DrawAABB(AxisAlignedBox aabb, Vector3 m_Position, float Yaw, Color color)
        {
            float cosY = (float)Math.Cos(Yaw);
            float sinY = (float)Math.Sin(Yaw);

            Vector3 fld = new Vector3(aabb.Min.Z * cosY - aabb.Min.X * sinY, aabb.Min.Y, aabb.Min.X * cosY + aabb.Min.Z * sinY) + m_Position; // 0
            Vector3 brt = new Vector3(aabb.Min.Z * cosY - aabb.Max.X * sinY, aabb.Min.Y, aabb.Max.X * cosY + aabb.Min.Z * sinY) + m_Position; // 1
            Vector3 bld = new Vector3(aabb.Max.Z * cosY - aabb.Max.X * sinY, aabb.Min.Y, aabb.Max.X * cosY + aabb.Max.Z * sinY) + m_Position; // 2
            Vector3 frt = new Vector3(aabb.Max.Z * cosY - aabb.Min.X * sinY, aabb.Min.Y, aabb.Min.X * cosY + aabb.Max.Z * sinY) + m_Position; // 3
            Vector3 frd = new Vector3(aabb.Max.Z * cosY - aabb.Min.X * sinY, aabb.Max.Y, aabb.Min.X * cosY + aabb.Max.Z * sinY) + m_Position; // 4
            Vector3 brb = new Vector3(aabb.Min.Z * cosY - aabb.Min.X * sinY, aabb.Max.Y, aabb.Min.X * cosY + aabb.Min.Z * sinY) + m_Position; // 5
            Vector3 blt = new Vector3(aabb.Min.Z * cosY - aabb.Max.X * sinY, aabb.Max.Y, aabb.Max.X * cosY + aabb.Min.Z * sinY) + m_Position; // 6
            Vector3 flt = new Vector3(aabb.Max.Z * cosY - aabb.Max.X * sinY, aabb.Max.Y, aabb.Max.X * cosY + aabb.Max.Z * sinY) + m_Position; // 7

            #region WorldToScreen
            if (!WorldToScreen(fld, out fld) || !WorldToScreen(brt, out brt)
                || !WorldToScreen(bld, out bld) || !WorldToScreen(frt, out frt)
                || !WorldToScreen(frd, out frd) || !WorldToScreen(brb, out brb)
                || !WorldToScreen(blt, out blt) || !WorldToScreen(flt, out flt))
                return;
            #endregion

            #region DrawLines
            DrawLine(fld, brt, color);
            DrawLine(brb, blt, color);
            DrawLine(fld, brb, color);
            DrawLine(brt, blt, color);

            DrawLine(frt, bld, color);
            DrawLine(frd, flt, color);
            DrawLine(frt, frd, color);
            DrawLine(bld, flt, color);

            DrawLine(frt, fld, color);
            DrawLine(frd, brb, color);
            DrawLine(brt, bld, color);
            DrawLine(blt, flt, color);
            #endregion
        }

        private void DrawInfo(int X, int Y, int W, int H)
        {
            DrawFillRect(X, Y, W, H, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, W - 2, H - 2, Color.Black);
            DrawLine(X + 1, Y + 25, X + W - 2, Y + 25, Color.Black);
            DrawFillRect(rect.Width / 2 + 191, rect.Height - 48, 282, 45, new Color(30, 30, 30, 130));
            DrawRect(rect.Width / 2 + 192, rect.Height - 47, 280, 43, Color.Black);
            DrawLine(rect.Width / 2 + 192, rect.Height - 20, rect.Width / 2 + 472, rect.Height - 20, Color.Black);
            DrawText(rect.Width / 2 + 197, rect.Height - 44, string.Format("Health: {0}/{1}", (int)localPlayer.Health, (int)localPlayer.MaxHealth), Color.White, true);
            DrawHealth(rect.Width / 2 + 197, rect.Height - 15, 270, 5, (int)localPlayer.Health, (int)localPlayer.MaxHealth);
            DrawText(X + 5, Y + 2, string.Format("AMMO: {0}/{1}", localPlayer.Ammo, localPlayer.AmmoClip), Color.White, true);
            int Accuracy = (int)(localPlayer.shotHit / (float)localPlayer.shotsFired * 100f);
            if (Accuracy < 0)
            {
                Accuracy = 0;
            }
            DrawText(X + 5, Y + 25, string.Format("ACCURACY: {0}%", Accuracy), Color.White, true);
        }

        private void DrawMenuInfo(int X, int Y, int W, int H)
        {
            DrawFillRect(X, Y, W, H, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, W - 2, H - 2, Color.Black);
            DrawText(X + 11, Y + 2, "DARK OVERLAY BY TEJISAV", Color.White, true, fontSmall);
            DrawLine(X +1, Y + 21, X + W - 2, Y + 21, Color.Black);
            DrawText(X + 3, Y + 22, "[INSERT] SHOW / HIDE MENU", Color.White, true, fontSmall);
        }

        private void DrawMainMenu(int X, int Y)
        {
            Color color = Color.BlueViolet;
            Color color1 = new Color(158, 197, 85, 255);
            DrawFillRect(X, Y, 200, 24 * MainMenuItems.MenuIndex.Count + 2, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, 200 - 2, 24 * MainMenuItems.MenuIndex.Count, Color.Black);
            for (int i = 1; i < MainMenuItems.MenuIndex.Count; i++)
            {
                DrawLine(X + 1, Y + 1 + 24 * i, (X + 1) + 198, Y + 1 + 24 * i, Color.Black);
            }
            for (int i = 0; i < MainMenuItems.MenuIndex.Count; i++)
            {
                if (i == 0)
                {
                    DrawText(X + 23, Y + 2 + 24 * i, MainMenuItems.MenuIndex[i].ItemName.PadLeft(14).ToUpper(), (selectedMainMenuIndex == i) ? color : Color.White, true, fontLarge);
                }
                else
                {
                    DrawText(X + 23, Y + 2 + 24 * i, MainMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + (MainMenuItems.MenuIndex[i].Enabled ? "<" : ">"), (selectedMainMenuIndex == i) ? color : (MainMenuItems.MenuIndex[i].Enabled) ? color1 : Color.White, true, fontLarge);
                }
                if (i != 0 && selectedMainMenuIndex == i)
                {
                    DrawFillRect(X + 5, Y + 6 + 24 * i, 14, 14, color);
                }
            }
            if (MainMenuItems.MenuIndex[1].Enabled)
            {
                DrawVisualMenu(X + 200, Y + 24);
            }
            else if (MainMenuItems.MenuIndex[2].Enabled)
            {
                DrawAimBotMenu(X + 200, Y + 24 * 2);
            }
            else if (MainMenuItems.MenuIndex[3].Enabled)
            {
                DrawRemovalMenu(X + 200, Y + 24 * 3);
            }
            else if (MainMenuItems.MenuIndex[4].Enabled)
            {
                DrawMiscMenu(X + 200, Y + 24 * 4);
            }
        }

        private void DrawVisualMenu(int X, int Y)
        {
            Color color = Color.BlueViolet;
            Color color1 = new Color(158, 197, 85, 255);
            DrawFillRect(X, Y, 180, 18 * VisualMenuItems.MenuIndex.Count + 2 + 6, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, 180 - 2, 18 * VisualMenuItems.MenuIndex.Count + 6, Color.Black);
            for (int i = 1; i < VisualMenuItems.MenuIndex.Count; i++)
            {
                DrawLine(X + 1, Y + 1 + 6 + 18 * i, (X + 1) + 178, Y + 1 + 6 + 18 * i, Color.Black);
            }
            for (int i = 0; i < VisualMenuItems.MenuIndex.Count; i++)
            {
                if (VisualMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    if (i == 0)
                    {
                        DrawText(X + 23, Y + 2 + 18 * i, VisualMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper(), (selectedVisualMenuIndex == i) ? color : Color.White, true, fontLarge);
                    }
                    else if (i > 1 && i < 11 && !VisualMenuItems.MenuIndex[1].Enabled)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, VisualMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + (VisualMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedVisualMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, VisualMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + (VisualMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedVisualMenuIndex == i) ? color : (VisualMenuItems.MenuIndex[i].Enabled) ? color1 : Color.White, true, fontSmall);
                    }
                }
                else
                {
                    if (i == 3 && (!VisualMenuItems.MenuIndex[1].Enabled || !VisualMenuItems.MenuIndex[2].Enabled))
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, VisualMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + VisualMenuItems.MenuIndex[i].ValueNames[VisualMenuItems.MenuIndex[i].Value] + "]", (selectedVisualMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, VisualMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + VisualMenuItems.MenuIndex[i].ValueNames[VisualMenuItems.MenuIndex[i].Value] + "]", (selectedVisualMenuIndex == i) ? color : color1, true, fontSmall);
                    }
                }
                if (selectedVisualMenuIndex == i)
                {
                    DrawFillRect(X + 5, Y + (i == 0 ? 4 : 7) + 2 + 18 * i, 14, 14, color);
                }
            }
        }

        private void DrawAimBotMenu(int X, int Y)
        {
            Color color = Color.BlueViolet;
            Color color1 = new Color(158, 197, 85, 255);
            DrawFillRect(X, Y, 240, 18 * AimbotMenuItems.MenuIndex.Count + 2 + 6, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, 240 - 2, 18 * AimbotMenuItems.MenuIndex.Count + 6, Color.Black);
            for (int i = 1; i < AimbotMenuItems.MenuIndex.Count; i++)
            {
                DrawLine(X + 1, Y + 1 + 6 + 18 * i, (X + 1) + 238, Y + 1 + 6 + 18 * i, Color.Black);
            }
            for (int i = 0; i < AimbotMenuItems.MenuIndex.Count; i++)
            {
                if (AimbotMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    if (i == 0)
                    {
                        DrawText(X + 23, Y + 2 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper(), (selectedAimbotMenuIndex == i) ? color : Color.White, true, fontLarge);
                    }
                    else if ((i > 1 && i < 14) && !AimbotMenuItems.MenuIndex[1].Enabled)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper() + "[" + (AimbotMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedAimbotMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper() + "[" + (AimbotMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedAimbotMenuIndex == i) ? color : (AimbotMenuItems.MenuIndex[i].Enabled) ? color1 : Color.White, true, fontSmall);
                    }
                }
                else
                {
                    if (i > 1 && i < 14 && !AimbotMenuItems.MenuIndex[1].Enabled)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper() + "[" + AimbotMenuItems.MenuIndex[i].ValueNames[AimbotMenuItems.MenuIndex[i].Value] + "]", (selectedAimbotMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else if (i == 8 && AimbotMenuItems.MenuIndex[6].Value == 2)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper() + "[" + AimbotMenuItems.MenuIndex[i].ValueNames[AimbotMenuItems.MenuIndex[i].Value] + "]", (selectedAimbotMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else if (i == 11 && !AimbotMenuItems.MenuIndex[10].Enabled)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper() + "[" + AimbotMenuItems.MenuIndex[i].ValueNames[AimbotMenuItems.MenuIndex[i].Value] + "]", (selectedAimbotMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, AimbotMenuItems.MenuIndex[i].ItemName.PadRight(17).ToUpper() + "[" + AimbotMenuItems.MenuIndex[i].ValueNames[AimbotMenuItems.MenuIndex[i].Value] + "]", (selectedAimbotMenuIndex == i) ? color : color1, true, fontSmall);
                    }
                }
                if (selectedAimbotMenuIndex == i)
                {
                    DrawFillRect(X + 5, Y + (i == 0 ? 4 : 7) + 2 + 18 * i, 14, 14, color);
                }
            }
            if (selectedAimbotMenuIndex == 8)
            {
                int num = AimbotMenuItems[AimbotMenuList.AIM_Fov].Value * 5 + 5;
                DrawFillCircle(rect.Center.X, rect.Center.Y, rect.Width / 100 * num, new Color(255, 255, 0, 60));
            }
        }

        private void DrawRemovalMenu(int X, int Y)
        {
            Color color = Color.BlueViolet;
            Color color1 = new Color(158, 197, 85, 255);
            DrawFillRect(X, Y, 200, 18 * RemovalMenuItems.MenuIndex.Count + 2 + 6, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, 200 - 2, 18 * RemovalMenuItems.MenuIndex.Count + 6, Color.Black);
            for (int i = 1; i < RemovalMenuItems.MenuIndex.Count; i++)
            {
                DrawLine(X + 1, Y + 1 + 6 + 18 * i, (X + 1) + 198, Y + 1 + 6 + 18 * i, Color.Black);
            }
            for (int i = 0; i < RemovalMenuItems.MenuIndex.Count; i++)
            {
                if (RemovalMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    if (i == 0)
                    {
                        DrawText(X + 23, Y + 2 + 18 * i, RemovalMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper(), (selectedRemovalMenuIndex == i) ? color : Color.White, true, fontLarge);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, RemovalMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + (RemovalMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedRemovalMenuIndex == i) ? color : (RemovalMenuItems.MenuIndex[i].Enabled) ? color1 : Color.White, true, fontSmall);
                    }
                }
                else
                {
                    if (i == 2 && RemovalMenuItems.MenuIndex[i].Value == 0)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, RemovalMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + RemovalMenuItems.MenuIndex[i].ValueNames[RemovalMenuItems.MenuIndex[i].Value] + "]", (selectedRemovalMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, RemovalMenuItems.MenuIndex[i].ItemName.PadRight(15).ToUpper() + "[" + RemovalMenuItems.MenuIndex[i].ValueNames[RemovalMenuItems.MenuIndex[i].Value] + "]", (selectedRemovalMenuIndex == i) ? color : color1, true, fontSmall);
                    }
                }
                if (selectedRemovalMenuIndex == i)
                {
                    DrawFillRect(X + 5, Y + (i == 0 ? 4 : 7) + 2 + 18 * i, 14, 14, color);
                }
            }
        }

        private void DrawMiscMenu(int X, int Y)
        {
            Color color = Color.BlueViolet;
            Color color1 = new Color(158, 197, 85, 255);
            DrawFillRect(X, Y, 250, 18 * MiscMenuItems.MenuIndex.Count + 2 + 6, new Color(30, 30, 30, 130));
            DrawRect(X + 1, Y + 1, 250 - 2, 18 * MiscMenuItems.MenuIndex.Count + 6, Color.Black);
            for (int i = 1; i < MiscMenuItems.MenuIndex.Count; i++)
            {
                DrawLine(X + 1, Y + 1 + 6 + 18 * i, (X + 1) + 248, Y + 1 + 6 + 18 * i, Color.Black);
            }
            for (int i = 0; i < MiscMenuItems.MenuIndex.Count; i++)
            {
                if (MiscMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    if (i == 0)
                    {
                        DrawText(X + 23, Y + 2 + 18 * i, MiscMenuItems.MenuIndex[i].ItemName.PadRight(20).ToUpper(), (selectedMiscMenuIndex == i) ? color : Color.White, true, fontLarge);
                    }
                    else if (i == 11 && !MiscMenuItems.MenuIndex[10].Enabled)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, MiscMenuItems.MenuIndex[i].ItemName.PadRight(20).ToUpper() + "[" + (MiscMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedMiscMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, MiscMenuItems.MenuIndex[i].ItemName.PadRight(20).ToUpper() + "[" + (MiscMenuItems.MenuIndex[i].Enabled ? "On" : "Off") + "]", (selectedMiscMenuIndex == i) ? color : (MiscMenuItems.MenuIndex[i].Enabled) ? color1 : Color.White, true, fontSmall);
                    }
                }
                else
                {
                    if (new[] { 1, 2, 3, 4 }.Contains(i) && MiscMenuItems.MenuIndex[i].Value == 0)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, MiscMenuItems.MenuIndex[i].ItemName.PadRight(20).ToUpper() + "[" + MiscMenuItems.MenuIndex[i].ValueNames[MiscMenuItems.MenuIndex[i].Value] + "]", (selectedMiscMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else if (i == 8 && !MiscMenuItems.MenuIndex[7].Enabled)
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, MiscMenuItems.MenuIndex[i].ItemName.PadRight(20).ToUpper() + "[" + MiscMenuItems.MenuIndex[i].ValueNames[MiscMenuItems.MenuIndex[i].Value] + "]", (selectedMiscMenuIndex == i) ? color : Color.White, true, fontSmall);
                    }
                    else
                    {
                        DrawText(X + 23, Y + 2 + 6 + 18 * i, MiscMenuItems.MenuIndex[i].ItemName.PadRight(20).ToUpper() + "[" + MiscMenuItems.MenuIndex[i].ValueNames[MiscMenuItems.MenuIndex[i].Value] + "]", (selectedMiscMenuIndex == i) ? color : color1, true, fontSmall);
                    }
                }
                if (selectedMiscMenuIndex == i)
                {
                    DrawFillRect(X + 5, Y + (i == 0 ? 4 : 7) + 2 + 18 * i, 14, 14, color);
                }
            }
        }

        private void DrawRadar(int X, int Y, int W, int H)
        {
            DrawFillRect(X, Y, W, H, new Color(25, 25, 25, 130));
            DrawRect(X + 1, Y + 1, W - 2, H - 2, Color.Black);
            DrawLine(X + W / 2, Y, X + W / 2, Y + H, Color.Black);
            DrawLine(X, Y + H / 2, X + W, Y + H / 2, Color.Black);
            float fovY = localPlayer.Fov.Y;
            fovY /= 1.34f;
            fovY -= (float)Math.PI / 2;
            float radarCenterX = X + W / 2;
            float radarCenterY = Y + H / 2;
            int dot = (int)Math.Sqrt((radarCenterX - radarCenterX) * (radarCenterX - radarCenterX) + (radarCenterY - radarCenterY - (float)(W / 2)) * (radarCenterY - radarCenterY - (float)(H / 2)));
            int fov_x = (int)(dot * (float)Math.Cos(fovY) + radarCenterX);
            Math.Sin(fovY);
            fovY += (float)Math.PI;
            int fov_x1 = (int)(dot * (float)Math.Cos(-(double)fovY) + radarCenterX);
            Math.Sin(-(double)fovY);
            DrawTriangle(new Vector2[]
			{
				new Vector2((int)radarCenterX, (int)radarCenterY),
				new Vector2(fov_x, Y),
				new Vector2(fov_x1, Y)
			}, new Color(255, 255, 255, 20));
            foreach (Player current in players)
            {
                if (current.IsValid())
                {
                    float r1 = localPlayer.Origin.Z - current.Origin.Z;
                    float r2 = localPlayer.Origin.X - current.Origin.X;
                    float x = r2 * (float)Math.Cos(-localPlayer.Yaw) - r1 * (float)Math.Sin(-localPlayer.Yaw);
                    float z = r2 * (float)Math.Sin(-localPlayer.Yaw) + r1 * (float)Math.Cos(-localPlayer.Yaw);
                    x *= RadarScale;
                    z *= RadarScale;
                    x += X + W / 2;
                    z += Y + H / 2;
                    Vector2 orgn = new Vector2(x, z);
                    Vector3 pos = current.Origin + current.ShootSpace * 10f;
                    r1 = localPlayer.Origin.Z - pos.Z;
                    r2 = localPlayer.Origin.X - pos.X;
                    x = r2 * (float)Math.Cos(-localPlayer.Yaw) - r1 * (float)Math.Sin(-localPlayer.Yaw);
                    z = r2 * (float)Math.Sin(-localPlayer.Yaw) + r1 * (float)Math.Cos(-localPlayer.Yaw);
                    x *= RadarScale;
                    z *= RadarScale;
                    x += X + W / 2;
                    z += Y + H / 2;
                    Vector2 temp1 = new Vector2(x - orgn.X, z - orgn.Y);
                    Vector2 temp;
                    Vector2.Normalize(ref temp1, out temp);
                    Vector2 enemyPositionRadar = new Vector2(orgn.X, orgn.Y);
                    double angleToRotate = Math.Atan2(0.0, 1.0) - Math.Atan2(temp.X, temp.Y);
                    angleToRotate *= (180 / Math.PI); // 57.295779513082323 Convert to degree
                    angleToRotate = 180 + angleToRotate; //this is needed as shit is inverted in bf
                    angleToRotate *= (Math.PI / 180); // 0.017453292519943295 Convert to radian
                    if (current.Distance >= 0f && current.Distance < W / (2 * RadarScale))
                    {
                        if (current.InVehicle)
                        {
                            if (current.IsDriver && current.Team != localPlayer.Team)
                            {
                                int index;
                                if (!RadarIcons.TryGetValue(current.VehicleName, out index))
                                {
                                    index = 3;
                                }
                                DrawSprite(new RectangleF((int)enemyPositionRadar.X - 15, (int)enemyPositionRadar.Y - 15, 30f, 30f), RadarBitmap, new RectangleF(index * 30, (current.Team == localPlayer.Team) ? 30 : 60, 30f, 30f), (float)angleToRotate);
                            }
                        }
                        else
                        {
                            DrawSprite(new RectangleF((int)enemyPositionRadar.X - 15, (int)enemyPositionRadar.Y - 15, 30f, 30f), RadarBitmap, new RectangleF((current.Team == localPlayer.Team) ? 30 : 60, 0f, 30f, 30f), (float)angleToRotate);
                        }
                    }
                }
            }
        }

        private void DrawNotification(int X, int Y, int W, int H, string Notification)
        {
            RoundedRectangle rect = new RoundedRectangle();
            rect.RadiusX = 4;
            rect.RadiusY = 4;
            rect.Rect = new RectangleF(X, Y, W, H);
            solidColorBrush.Color = new Color(196, 26, 31, 210);
            device.FillRoundedRectangle(ref rect, solidColorBrush);
            DrawText(X + 5, Y + 15, PadBoth(Notification,24), Color.White, true);
        } 

        private void DrawWarn(int X, int Y, int W, int H, string SpectatorName, string msg, int Slot)
        {
            if (Slot == 0)
            {
                DrawFillRect(X, Y, W, H, new Color(30, 30, 30, 130));
                DrawRect(X + 1, Y + 1, W - 2, H - 2, Color.Black);
                DrawText(X + 25, Y + 1, msg.PadLeft(9).ToUpper(), Color.White, true, fontLarge);
                DrawLine(X + 1, Y + 21, (X - 2) + W, Y + 21, Color.Black);
            }
            DrawText(X + 6, Y + 22 + (Slot * 14), (Slot + 1) + "." + SpectatorName, Color.White, true, fontSmall);
        }
        #endregion

        private void ReadSettings()
        {
            if (!File.Exists(SettingsPath))
                return;

            int VLine = 0, ALine = 0, RLine = 0, MLine = 0;
            string[] SettingsLine = File.ReadAllLines(SettingsPath);
            for(int i = 0; i < SettingsLine.Length; i++)
            {
                if(SettingsLine[i].Contains("[VISUAL]"))
                {
                    VLine = i;
                }
                else if(SettingsLine[i].Contains("[AIMBOT]"))
                {
                    ALine = i;
                }
                else if(SettingsLine[i].Contains("[REMOVAL]"))
                {
                    RLine = i;
                }
                else if (SettingsLine[i].Contains("[MISC]"))
                {
                    MLine = i;
                }
            }
            for(int i = VLine + 1; i < ALine; i++)
            {
                string[] parts = SettingsLine[i].Split('=');
                VisualMenuList EnumObject;
                if (Enum.TryParse(parts[0], true, out EnumObject))
                {
                    int EnumValue = (int)EnumObject;
                    if (VisualMenuItems.MenuIndex[EnumValue].itemtype == ItemType.Boolean)
                    {
                        VisualMenuItems.MenuIndex[EnumValue].Enabled = Convert.ToBoolean(parts[1]);
                    }
                    else
                    {
                        VisualMenuItems.MenuIndex[EnumValue].Value = Convert.ToInt32(parts[1]);
                    }
                }
            }
            for(int i = ALine + 1; i < RLine; i++)
            {
                string[] parts = SettingsLine[i].Split('=');
                AimbotMenuList EnumObject;
                if (Enum.TryParse(parts[0], true, out EnumObject))
                {
                    int EnumValue = (int)EnumObject;
                    if (AimbotMenuItems.MenuIndex[EnumValue].itemtype == ItemType.Boolean)
                    {
                        AimbotMenuItems.MenuIndex[EnumValue].Enabled = Convert.ToBoolean(parts[1]);
                    }
                    else
                    {
                        AimbotMenuItems.MenuIndex[EnumValue].Value = Convert.ToInt32(parts[1]);
                    }
                }
            }
            for(int i = RLine + 1; i < MLine; i++)
            {
                string[] parts = SettingsLine[i].Split('=');
                RemovalMenuList EnumObject;
                if (Enum.TryParse(parts[0], true, out EnumObject))
                {
                    int EnumValue = (int)EnumObject;
                    if (RemovalMenuItems.MenuIndex[EnumValue].itemtype == ItemType.Boolean)
                    {
                        RemovalMenuItems.MenuIndex[EnumValue].Enabled = Convert.ToBoolean(parts[1]);
                    }
                    else
                    {
                        RemovalMenuItems.MenuIndex[EnumValue].Value = Convert.ToInt32(parts[1]);
                    }
                }
            }
            for (int i = MLine + 1; i < SettingsLine.Length; i++)
            {
                string[] parts = SettingsLine[i].Split('=');
                MiscMenuList EnumObject;
                if (Enum.TryParse(parts[0], true, out EnumObject))
                {
                    int EnumValue = (int)EnumObject;
                    if (MiscMenuItems.MenuIndex[EnumValue].itemtype == ItemType.Boolean)
                    {
                        MiscMenuItems.MenuIndex[EnumValue].Enabled = Convert.ToBoolean(parts[1]);
                    }
                    else
                    {
                        MiscMenuItems.MenuIndex[EnumValue].Value = Convert.ToInt32(parts[1]);
                    }
                }
            }
        }

        private void SaveSettings()
        {
            string FileString = "";
            FileString += "[VISUAL]" + Environment.NewLine;
            for (int i = 1; i < VisualMenuItems.MenuIndex.Count; i++)
            {
                if (VisualMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    FileString += (VisualMenuList)i + "=" + Convert.ToString(VisualMenuItems.MenuIndex[i].Enabled) + Environment.NewLine;
                }
                else
                {
                    FileString += (VisualMenuList)i + "=" + VisualMenuItems.MenuIndex[i].Value + Environment.NewLine;
                }
            }
            FileString += "[AIMBOT]" + Environment.NewLine;
            for (int i = 1; i < AimbotMenuItems.MenuIndex.Count; i++)
            {
                if (AimbotMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    FileString += (AimbotMenuList)i + "=" + Convert.ToString(AimbotMenuItems.MenuIndex[i].Enabled) + Environment.NewLine;
                }
                else
                {
                    FileString += (AimbotMenuList)i + "=" + AimbotMenuItems.MenuIndex[i].Value + Environment.NewLine;
                }
            }
            FileString += "[REMOVAL]" + Environment.NewLine;
            for (int i = 1; i < RemovalMenuItems.MenuIndex.Count; i++)
            {
                if (RemovalMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    FileString += (RemovalMenuList)i + "=" + Convert.ToString(RemovalMenuItems.MenuIndex[i].Enabled) + Environment.NewLine;
                }
                else
                {
                    FileString += (RemovalMenuList)i + "=" + RemovalMenuItems.MenuIndex[i].Value + Environment.NewLine;
                }
            }
            FileString += "[MISC]" + Environment.NewLine;
            for (int i = 1; i < MiscMenuItems.MenuIndex.Count; i++)
            {
                if (MiscMenuItems.MenuIndex[i].itemtype == ItemType.Boolean)
                {
                    FileString += (MiscMenuList)i + "=" + Convert.ToString(MiscMenuItems.MenuIndex[i].Enabled) + Environment.NewLine;
                }
                else
                {
                    FileString += (MiscMenuList)i + "=" + MiscMenuItems.MenuIndex[i].Value + Environment.NewLine;
                }
            }
            File.WriteAllText(SettingsPath, FileString);
        }
    }
}
