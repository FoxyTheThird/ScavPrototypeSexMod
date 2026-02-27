using BepInEx;
using JetBrains.Annotations;
using ScavSexMod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScavPrototypeSexMod.Managers
{
    public static class MoodleHandler
    {
        // This is something...
        private static readonly Dictionary<Tuple<string, string>, Tuple<int, bool>> moodleMetadata = new Dictionary<Tuple<string, string>, Tuple<int, bool>>()
        {
            { Tuple.Create("hornymoodle", "hornymoodle.png"), Tuple.Create(0, false) },
            { Tuple.Create("horniermoodle", "horniermoodle.png"), Tuple.Create(1, false) },
            { Tuple.Create("evenhorniermoodle", "evenhorniermoodle.png"), Tuple.Create(2, false) },
            { Tuple.Create("horniestmoodle", "horniestmoodle.png"), Tuple.Create(3, true) },
            { Tuple.Create("protectionmoodle", "protectionmoodle.png"), Tuple.Create(0, false) }
        };

        public static void Initialize(MoodleManager mm)
        {
            Plugin.Log.LogInfo("Initializing moodles...");

            foreach (var entry in moodleMetadata)
            {
                string iconName = entry.Key.Item1;
                string fileName = entry.Key.Item2;

                Sprite sprite = FileLoader.LoadEmbeddedSprite(
                    $"ScavPrototypeSexMod.Assets.{fileName}", 125);

                // Register the icon
                mm.icons.Add(iconName, sprite);

                Plugin.Log.LogInfo($"Registered moodle icon: {iconName}");
            }

            Plugin.Log.LogInfo("All custom moodles registered!");
        }

        public static void AddMoodle(MoodleManager __instance, string iconName)
        {
            Tuple<string, string> keyEntry = null;
            foreach (var k in moodleMetadata.Keys)
            {
                if (k.Item1 == iconName)
                {
                    keyEntry = k;
                    break;
                }
            }

            if (keyEntry == null)
            {
                Plugin.Log.LogError("Moodle key '" + iconName + "' not found in moodleMetadata!");
                return;
            }

            Tuple<int, bool> meta = moodleMetadata[keyEntry];
            int intensity = meta.Item1;
            bool critical = meta.Item2;

            string name = FileLoader.GetLocale(iconName.Replace("moodle", ""));
            string desc = FileLoader.GetLocale(iconName.Replace("moodle", "") + "Desc");

            __instance.AddMoodle(intensity, iconName, name, desc, critical, false);
        }
    }
}
