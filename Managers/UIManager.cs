using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using ScavSexMod.Helpers;
using TMPro;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace ScavPrototypeSexMod.Managers
{
    public static class UIManager
    {
        public static void Initialize()
        {
            Plugin.Log.LogInfo("UIManager Initialized");
        }
        
        private static readonly string[] Options =
        {
            "Male",
            "Female",
            "Intersex",
            "Non-Binary"
        };

        private const string PREF_KEY = "GenderSelection";

        // Creates the gender radio check boxes on the main menu
        public static void CreateGenderRadios()
        {
            var canvas = GameObject.Find("Canvas")?.transform;
            if (canvas == null) return;

            var root = new GameObject("GenderOptions", typeof(RectTransform));
            root.transform.SetParent(canvas, false);

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(25, -225);

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperLeft;

            var group = root.AddComponent<ToggleGroup>();
            group.allowSwitchOff = false;

            int savedIndex = PlayerPrefs.GetInt(PREF_KEY, 0);
            SharedState.CurrentGender = (SharedState.Gender)savedIndex;

            for (int i = 0; i < Options.Length; i++)
            {
                int index = i;

                var toggleGO = new GameObject(Options[i], typeof(RectTransform), typeof(Image), typeof(Toggle), typeof(HorizontalLayoutGroup));

                toggleGO.transform.SetParent(root.transform, false);

                var rowRect = toggleGO.GetComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(220, 26);

                var rowImage = toggleGO.GetComponent<Image>();
                rowImage.color = new Color(0, 0, 0, 0);

                var hLayout = toggleGO.GetComponent<HorizontalLayoutGroup>();
                hLayout.spacing = 6;
                hLayout.childAlignment = TextAnchor.MiddleLeft;
                hLayout.childControlHeight = false;
                hLayout.childControlWidth = false;
                hLayout.childForceExpandWidth = false;

                var toggle = toggleGO.GetComponent<Toggle>();
                toggle.group = group;

                var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
                box.transform.SetParent(toggleGO.transform, false);

                var boxRect = box.GetComponent<RectTransform>();
                boxRect.sizeDelta = new Vector2(18, 18);

                var boxImage = box.GetComponent<Image>();
                boxImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

                toggle.targetGraphic = boxImage;

                var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
                check.transform.SetParent(box.transform, false);

                var checkRect = check.GetComponent<RectTransform>();
                checkRect.anchorMin = Vector2.zero;
                checkRect.anchorMax = Vector2.one;
                checkRect.offsetMin = Vector2.zero;
                checkRect.offsetMax = Vector2.zero;

                var checkImage = check.GetComponent<Image>();
                checkImage.color = Color.green;

                toggle.graphic = checkImage;

                var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelGO.transform.SetParent(toggleGO.transform, false);

                var text = labelGO.GetComponent<TextMeshProUGUI>();
                text.text = Options[i];
                text.fontSize = 22;
                text.alignment = TextAlignmentOptions.MidlineLeft;

                toggle.isOn = index == savedIndex;

                toggle.onValueChanged.AddListener(isOn =>
                {
                    if (!isOn) return;

                    SharedState.CurrentGender = (SharedState.Gender)index;
                    Plugin.Log.LogInfo(SharedState.CurrentGender.ToString());
                    PlayerPrefs.SetInt(PREF_KEY, index);
                    PlayerPrefs.Save();
                });
            }
        }

        // Creates the Sex Button on the trader menu
        public static IEnumerator CreateTraderSexButton(PlayerCamera cam)
        {
            yield return null;

            GameObject tradeMenu = cam.tradeMenu.gameObject;

            if (tradeMenu.transform.Find("SexModImage") != null)
                yield break;

            // Create object
            GameObject imgGO = new GameObject("SexModImage");
            imgGO.transform.SetParent(tradeMenu.transform, false);

            Image img = imgGO.AddComponent<Image>();
            img.sprite = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.sex.png");

            Button btn = imgGO.AddComponent<Button>();
            btn.image = img;
            btn.interactable = true;
            btn.enabled = true;

            RectTransform rt = img.rectTransform;
            // Putting the pivot point directly in the center of the button
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // offset relative to radialMenu
            rt.anchoredPosition = new Vector2(100f, -170f);
            rt.sizeDelta = new Vector2(128, 128);
            rt.localScale = Vector3.one;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                cam.StartCoroutine(SexManager.TriggerSex(cam));
            });
        }

        // Creates the horny stat in the wound view and the gender.
        public static IEnumerator InitSexModWoundView(PlayerCamera cam)
        {
            GameObject woundView = cam.woundView;

            // Gender stuffs
            if (SharedState.genderRoot == null)
            {
                RectTransform GenderRT = woundView.transform.Find("StatMenu").GetComponent<RectTransform>();

                GameObject GenderGO = new GameObject("GenRoot");
                SharedState.genderRoot = GenderGO;
                GenderGO.transform.SetParent(GenderRT, false);

                Image genimg = GenderGO.AddComponent<Image>();
                genimg.sprite = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets." + SharedState.CurrentGender.ToString().ToLower() + "symbol.png");
                genimg.raycastTarget = true;

                RectTransform imgRT = genimg.rectTransform;
                imgRT.anchorMin = new Vector2(1f, 1f);
                imgRT.anchorMax = new Vector2(1f, 1f);
                imgRT.pivot = new Vector2(1f, 1f);

                UITooltip tooltip = genimg.gameObject.AddComponent<UITooltip>();
                tooltip.skipLocale = true;
                tooltip.tipName = "Gender";

                switch (SharedState.CurrentGender)
                {
                    case SharedState.Gender.Male:
                        imgRT.sizeDelta = new Vector2(250, 250);
                        imgRT.anchoredPosition = new Vector2(-195f, -315f);
                        imgRT.localScale = new Vector3(0.15f, 0.15f, 1f);
                        tooltip.tipDesc = "You are a " + SharedState.CurrentGender.ToString() + ".";
                        break;
                    case SharedState.Gender.Female:
                        imgRT.sizeDelta = new Vector2(250, 250);
                        imgRT.anchoredPosition = new Vector2(-185f, -310f);
                        imgRT.localScale = new Vector3(0.20f, 0.20f, 1f);
                        tooltip.tipDesc = "You are a " + SharedState.CurrentGender.ToString() + ".";
                        break;
                    case SharedState.Gender.Intersex:
                        imgRT.sizeDelta = new Vector2(250, 340);
                        imgRT.anchoredPosition = new Vector2(-195f, -310f);
                        imgRT.localScale = new Vector3(0.13f, 0.13f, 1f);
                        tooltip.tipDesc = "You are " + SharedState.CurrentGender.ToString() + ".";
                        break;
                    case SharedState.Gender.NonBinary:
                        imgRT.sizeDelta = new Vector2(142, 250);
                        imgRT.anchoredPosition = new Vector2(-200f, -315f);
                        imgRT.localScale = new Vector3(0.15f, 0.15f, 1f);
                        tooltip.tipDesc = "You are " + SharedState.CurrentGender.ToString() + ".";
                        break;
                }
            }


            // For Horny text
            if (SharedState.CurrentGender != SharedState.Gender.NonBinary)
            {
                if (SharedState.horninessRoot == null)
                {
                    Transform statMenu = null;
                    yield return new WaitUntil(() =>
                    {
                        statMenu = woundView.transform.Find("StatMenu");
                        return statMenu != null;
                    });

                    RectTransform rootRT = statMenu.GetComponent<RectTransform>();
                    TextMeshProUGUI TextMeshGUI = statMenu.transform.Find("Image/OxyText").GetComponent<TextMeshProUGUI>();

                    // IMAGE
                    GameObject imgGO = new GameObject("SexModStatRoot");
                    SharedState.horninessRoot = imgGO;
                    imgGO.transform.SetParent(rootRT, false);
                    Image img = imgGO.AddComponent<Image>();
                    img.sprite = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.hornystat.png");
                    img.raycastTarget = true;

                    // Tooltip
                    UITooltip tooltip = img.gameObject.AddComponent<UITooltip>();
                    tooltip.skipLocale = true;
                    tooltip.tipName = "Horniness";
                    tooltip.tipDesc = "Represents sexual tension. Decreases over time or through relief.";

                    // Image RectTransform fills parent
                    RectTransform imgRT = img.rectTransform;
                    imgRT.anchorMin = new Vector2(1f, 1f);
                    imgRT.anchorMax = new Vector2(1f, 1f);
                    imgRT.pivot = new Vector2(1f, 1f);

                    imgRT.anchoredPosition = new Vector2(-30f, -170f);
                    imgRT.sizeDelta = new Vector2(154f, 50f);
                    imgRT.localScale = new Vector3(0.67f, 0.67f, 1f);

                    // TEXT
                    GameObject txtGO = new GameObject("Text");
                    txtGO.transform.SetParent(imgRT, false);
                    SharedState.horninessText = txtGO.AddComponent<TextMeshProUGUI>();
                    SharedState.horninessText.alignment = TextAlignmentOptions.Center;
                    SharedState.horninessText.color = TextMeshGUI.color;
                    SharedState.horninessText.fontSize = 35f;
                    SharedState.horninessText.font = TextMeshGUI.font;

                    RectTransform txtRT = SharedState.horninessText.rectTransform;
                    txtRT.anchorMin = new Vector2(0.5f, 0.75f);
                    txtRT.anchorMax = new Vector2(0.5f, 0f);
                }
            }
        }

        // Makes the masturbate button in the workout list UI
        public static IEnumerator InitSexModWorkoutList(PlayerCamera cam)
        {
            yield return null;

            RectTransform rootRT = cam.woundView.gameObject.transform.Find("WorkoutsList").GetComponent<RectTransform>();

            TextMeshProUGUI textmesh = cam.woundView.gameObject.transform.Find("WorkoutsList").Find("Plank").Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

            if (SharedState.CurrentGender == SharedState.Gender.NonBinary)
                yield break;

            if (SharedState.masturbateButton != null)
            {
                SharedState.masturbateButton.SetActive(true);
                yield break;
            }

            if (!rootRT)
            {
                Plugin.Log.LogError("RootRT is not defined.");
                yield break;
            }

            Plugin.Log.LogInfo("Creating masturbate button");

            SharedState.masturbateButton = new GameObject("Masturbate");
            SharedState.masturbateButton.transform.SetParent(rootRT.transform, false);

            Image img = SharedState.masturbateButton.AddComponent<Image>();
            img.sprite = FileLoader.LoadEmbeddedSprite("ScavPrototypeSexMod.Assets.boxofdoom.png", 200);

            Button btn = SharedState.masturbateButton.AddComponent<Button>();
            btn.targetGraphic = img;

            UITooltip uiTool = SharedState.masturbateButton.AddComponent<UITooltip>();
            uiTool.tipName = "Masturbate";
            uiTool.tipDesc = "Relieve yourself; Decreases horniness, gives RES experience. Tires you out quick.";
            uiTool.skipLocale = true;

            RectTransform btnrect = SharedState.masturbateButton.GetComponent<RectTransform>();
            btnrect.anchorMin = new Vector2(1.003f, 1.23f);
            btnrect.anchorMax = new Vector2(1.003f, 1.23f);
            btnrect.pivot = new Vector2(1f, 1f);
            btnrect.sizeDelta = new Vector2(textmesh.rectTransform.sizeDelta.x + 30f, textmesh.rectTransform.sizeDelta.y);

            GameObject txtGO = new GameObject("Text");
            txtGO.transform.SetParent(SharedState.masturbateButton.transform, false);

            TextMeshProUGUI txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.text = "Masturbate";
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = textmesh.fontSize;
            txt.font = textmesh.font;

            RectTransform txtRT = txt.rectTransform;
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = Vector2.zero;
            txtRT.offsetMax = Vector2.zero;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (SharedState.mCoroutine != null)
                {
                    SharedState.pc.StopCoroutine(SharedState.mCoroutine);
                }

                SharedState.mCoroutine = SharedState.pc.StartCoroutine(SexManager.Masturbate(cam));
            });
        }

        public static void UpdateHorninessUI()
        {
            if (SharedState.horninessText != null)
                SharedState.horninessText.text = $"{SharedState.Horniness:F0}%";
        }
    }

}
