using BepInEx;
using UnityEngine;
using GorillaNetworking;
using AntiIAuth;

namespace GorillaMedia
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            ConfigManager.LoadConfig(Config);
        }

        void Start()
        {
            GameObject MediaManager = new GameObject("MediaManager");
            MediaManager.AddComponent<MediaManager>();
            DontDestroyOnLoad(MediaManager);

            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
            NetworkSystem.Instance.OnJoinedRoomEvent += OnJoinedRoom;
        }

        void OnJoinedRoom()
        {
            CustomProperty.SetCustomNetworkProperty();
        }

        void OnPlayerSpawned()
        {
            GameObject UI = AssetManager.LoadObject<GameObject>("UI");
            //UI.transform.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find(
                 //"RigAnchor/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L"), false);
            
            UI.AddComponent<MediaControlUI>();
        }

        private Rect windowRect;
        private bool showMenu = true;
        private bool isScrubbing = false;
        private float scrubTime = 0f;

        private Texture2D bgTex, greenTex, grayTex, hoverBgTex;
        private GUIStyle windowStyle, titleStyle, artistStyle, timeStyle, buttonStyle;
        private bool stylesInitialized = false;
        private Texture2D playIcon, pauseIcon;
        private Texture2D spotifyIcon, appleIcon;

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D LoadTextureFromResource(string fileName)
        {
            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaMedia.Resources." + fileName);
            if (stream == null) return null;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);
            return tex;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            windowRect = new Rect(20, 20, 350, 90);

            bgTex = MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f, 1f));
            greenTex = MakeTex(1, 1, new Color(0.11f, 0.72f, 0.33f, 1f));
            grayTex = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f, 1f));
            hoverBgTex = MakeTex(1, 1, new Color(0.2f, 0.2f, 0.2f, 1f));

            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = bgTex;
            windowStyle.onNormal.background = bgTex;
            windowStyle.border = new RectOffset(0, 0, 0, 0);

            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.white;

            artistStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            artistStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);

            timeStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            timeStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.background = null;
            buttonStyle.hover.background = hoverBgTex;
            buttonStyle.active.background = grayTex;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 16;

            playIcon = AssetManager.LoadAsset<Texture2D>("play");
            pauseIcon = AssetManager.LoadAsset<Texture2D>("pause");
            
            spotifyIcon = LoadTextureFromResource("spotify.png");
            appleIcon = LoadTextureFromResource("apple.png");

            stylesInitialized = true;
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame) showMenu = !showMenu;
        }

        void OnGUI()
        {
            if (!showMenu || MediaManager.instance == null) return;

            InitStyles();
            windowRect = GUI.Window(0, windowRect, DrawWindow, "", windowStyle);
        }

        private Texture2D GetAppIcon(string sourceApp)
        {
            if (string.IsNullOrEmpty(sourceApp)) return null;
            sourceApp = sourceApp.ToLower();
            if (sourceApp.Contains("spotify")) return spotifyIcon;
            if (sourceApp.Contains("apple")) return appleIcon;
            return null;
        }

        private void DrawWindow(int windowID)
        {
            GUI.color = new Color(0.11f, 0.72f, 0.33f, 1f);
            GUI.DrawTexture(new Rect(0, 0, windowRect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, windowRect.height - 2, windowRect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, 0, 2, windowRect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(windowRect.width - 2, 0, 2, windowRect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (MediaManager.Icon != null)
                GUI.DrawTexture(new Rect(10, 10, 70, 70), MediaManager.Icon, ScaleMode.StretchToFill);

            Texture2D appIcon = GetAppIcon(MediaManager.SourceApp);
            if (appIcon != null)
                GUI.DrawTexture(new Rect(windowRect.width - 30, 10, 20, 20), appIcon, ScaleMode.StretchToFill);

            GUI.Label(new Rect(90, 10, 220, 30), MediaManager.Title, titleStyle);
            GUI.Label(new Rect(90, 35, 220, 20), MediaManager.Artist, artistStyle);

            float displayTime = isScrubbing ? scrubTime : MediaManager.ElapsedTime;
            displayTime = Mathf.Clamp(displayTime, MediaManager.StartTime, MediaManager.EndTime);

            float percent = Mathf.Clamp01((displayTime - MediaManager.StartTime) / Mathf.Max(1f, MediaManager.EndTime - MediaManager.StartTime));
            
            Rect sliderRect = new Rect(90, 65, 100, 15);
            GUI.DrawTexture(new Rect(sliderRect.x, sliderRect.y + 5, sliderRect.width, 4), grayTex);
            GUI.DrawTexture(new Rect(sliderRect.x, sliderRect.y + 5, sliderRect.width * percent, 4), greenTex);
            
            Rect thumbRect = new Rect(sliderRect.x + sliderRect.width * percent - 4, sliderRect.y + 3, 8, 8);
            GUI.DrawTexture(thumbRect, Texture2D.whiteTexture);

            if (Event.current.type == EventType.MouseDown && sliderRect.Contains(Event.current.mousePosition))
            {
                isScrubbing = true;
            }

            if (isScrubbing)
            {
                float mouseX = Event.current.mousePosition.x;
                float newPercent = Mathf.Clamp01((mouseX - sliderRect.x) / sliderRect.width);
                scrubTime = MediaManager.StartTime + newPercent * (MediaManager.EndTime - MediaManager.StartTime);

                if (Event.current.type == EventType.MouseUp)
                {
                    isScrubbing = false;
                    MediaManager.instance.SetPlaybackPosition(scrubTime);
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use();
                }
            }

            GUI.Label(new Rect(195, 60, 45, 20), $"{Mathf.Floor(displayTime / 60)}:{Mathf.Floor(displayTime % 60):00}", timeStyle);

            if (GUI.Button(new Rect(245, 53, 30, 30), "<", buttonStyle)) MediaManager.instance.PreviousTrack();
            
            Texture2D toggleIcon = MediaManager.Paused ? playIcon : pauseIcon;
            if (toggleIcon != null)
            {
                if (GUI.Button(new Rect(275, 53, 30, 30), toggleIcon, buttonStyle)) MediaManager.instance.PauseTrack();
            }
            else
            {
                if (GUI.Button(new Rect(275, 53, 30, 30), MediaManager.Paused ? ">" : "||", buttonStyle)) MediaManager.instance.PauseTrack();
            }
            
            if (GUI.Button(new Rect(305, 53, 30, 30), ">", buttonStyle)) MediaManager.instance.SkipTrack();

            GUI.DragWindow();
        }
    }
}
