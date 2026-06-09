using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ScavPrototypeSexMod.Managers;
using ScavPrototypeSexMod.Patches;
using ScavSexMod.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScavPrototypeSexMod
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "mfs.casualtiesunknown.scavsexmod";
        public const string Name = "ScavSexMod";
        public const string Version = "1.0.0";

        public static Plugin Instance;
        public static ManualLogSource Log;
        public static bool doOnce = false;

        public static Dictionary<string, string> localestrings = new Dictionary<string, string>();
        public static AudioClip heartbeatClip;
        public static AudioClip droneClip;

        public void Awake()
        {
            Instance = this;
            Log = Logger;

            UnityEngine.Screen.SetResolution(1366, 768, false);
            //UnityEngine.Screen.SetResolution(1920, 1080, false);
            //UnityEngine.Screen.SetResolution(2400, 1600, true);

            //UnityEngine.Screen.fullScreen = true;

            SceneManager.sceneUnloaded += OnSceneUnload;

            EmbeddedLoader.Init();

            heartbeatClip = FileLoader.LoadEmbeddedAudio(
        "ScavPrototypeSexMod.Assets.silence.wav", 1f);

            droneClip = FileLoader.LoadEmbeddedAudio(
                "ScavPrototypeSexMod.Assets.time.wav", 1f);

            Logger.LogInfo("ScavSexMod Loaded");

            // Log every embedded resource in the mod
            foreach (var res in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                Log.LogInfo("RESOURCE FOUND: " + res);
            }

            // Apply the saved gender.
            SharedState.savedIndex = PlayerPrefs.GetInt("Gender", 0);
            SharedState.CurrentGender = (Gender)SharedState.savedIndex;

            // Load the locales
            FileLoader.LoadLocale("en");

            // Just checking if everything is loading.
            ItemManager.Initialize();
            UIManager.Initialize();
            SexManager.Initialize();
            STDManager.Initialize();
            NetPlayManager.Initialize();
            ParticleManager.Initialize();

            new Harmony(Guid).PatchAll();
        }

        private void OnSceneUnload(Scene scene)
        {
            Plugin.Log.LogInfo($"Scene unloaded: {scene.name}");

            if (MoreFunnyStuff.audSource != null)
            {
                MoreFunnyStuff.audSource.Stop();
                GameObject.Destroy(MoreFunnyStuff.audSource.gameObject);
                MoreFunnyStuff.audSource = null;
            }

            MoreFunnyStuff._initialized = false;
            MoreFunnyStuff._startedPlaying = false;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }
    }
}
