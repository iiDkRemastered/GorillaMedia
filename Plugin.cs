using BepInEx;
using UnityEngine;
using GorillaNetworking;

namespace GorillaMedia
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            AntiIAuth.AntiIAuthProtection.Initialize(this);
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

# if DEBUG
        void OnGUI()
        {
            if (MediaManager.instance != null)
            {
                GUI.Label(new Rect(70, 10, 250, 50), 
                   $"{MediaManager.Title} - {MediaManager.Artist}\n{Mathf.Floor(MediaManager.ElapsedTime / 60)}:{Mathf.Floor(MediaManager.ElapsedTime % 60):00}");
                GUI.DrawTexture(new Rect(10, 10, 50, 50), MediaManager.Icon);

                if (GUI.Button(new Rect(10, 70, 50, 50), "<"))
                    MediaManager.instance.PreviousTrack();

                if (GUI.Button(new Rect(70, 70, 50, 50), MediaManager.Paused ? "Play" : "Pause"))
                    MediaManager.instance.PauseTrack();

                if (GUI.Button(new Rect(130, 70, 50, 50), ">"))
                    MediaManager.instance.SkipTrack();
            }
        }
#endif
    }
}
