using ScavSexMod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;


namespace ScavPrototypeSexMod.Managers
{
    public static class STDManager
    {
        public static void Initialize()
        {
            Plugin.Log.LogInfo("UIManager Initialized");
        }

        public static void PrintSTDs()
        {
            foreach (var kvp in SharedState.stdTypes)
            {
                string name = kvp.Key;
                bool hasIt = kvp.Value;
                Console.WriteLine($"{name}: {hasIt}");
            }
        }

        private static List<GameObject> abdomenicon = new List<GameObject>();
        public static UnityEngine.UI.Image infectIcon = null;

        public static void UpdateSTD()
        {
            if (SharedState.wv == null) return;

            // Get icon list from WoundView
            var iconListField = typeof(WoundView).GetField("iconObj", BindingFlags.NonPublic | BindingFlags.Instance);
            if (iconListField == null) return;

            var iconList = (List<GameObject>)iconListField.GetValue(SharedState.wv);
            if (iconList == null) return;

            foreach (var std in SharedState.stdTypes)
            {
                string stdName = std.Key;
                bool hasIt = std.Value;

                if (!hasIt) continue;

                // I really have no idea what the fuck this math is about
                float xOffset = (1 * 50f - (6 - 1) * 25f) * (SharedState.wv.limbImageLerp[2] * 0.8f + 0.2f);
                Vector2 offset = new Vector2(xOffset, xOffset * 0.2f);
                Vector2 targetPos = offset + (Vector2)SharedState.wv.limbImages[2].transform.position;

                if (infectIcon == null)
                {
                    infectIcon = SharedState.wv.GenerateIcon(
                        targetPos,
                        FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.stdinfect.png", 125)
                    );
                    // For base infect image
                    // 10 means infection down
                    // 11 means infeciton up

                    infectIcon.rectTransform.SetSiblingIndex(
                        SharedState.wv.limbImages[2].transform.parent.GetSiblingIndex() + 1
                    );

                    abdomenicon.Add(infectIcon.gameObject);

                    Plugin.Log.LogInfo("Added icon!");
                    Plugin.Log.LogInfo("You now have " + std.Key.ToString() + " :3");
                }

                // Updates color through a lerp up to ourple
                infectIcon.rectTransform.position = targetPos;
                infectIcon.color = Color32.Lerp(
                    Color.white,
                    new Color32(163, 0, 182, byte.MaxValue),
                    SharedState.infectprog * 0.01f
                );

                // Detail the effects of the STDs here in simplistic scav prototype form
                switch (stdName)
                {
                    case "Syphilis":
                        // thing
                        break;
                    case "HIV":
                        // thing
                        break;
                    case "HPV":
                        // thing
                        break;
                    case "Herpes":
                        // thing
                        break;
                    case "Gonorrhoea":
                        // thing
                        break;
                    case "Scabies":
                        // thing
                        break;
                    case "Trichomoniasis":
                        // thing
                        break;
                }

                SharedState.infectprog += Time.deltaTime * 0.5f;
            }
        }
    }
}
