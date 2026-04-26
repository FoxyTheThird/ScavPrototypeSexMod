using ScavSexMod.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ScavPrototypeSexMod.SharedState;

namespace ScavPrototypeSexMod.Managers
{
    public static class ItemManager
    {
        public static void Initialize()
        {
            Plugin.Log.LogInfo("ItemManager Initialized");
        }

        // Types of Crafting Qualities:
        // cutting
        // hammering
        // meat
        // produce
        // rippable
        // flammable
        // nails
        // firestarter
        public static ItemInfo AddItemInfo(ItemInfo __instance, string name, string tags, string category, string qualities, string desiredWearLimb, string wearSlotID, string description = "", float? weight = 1f, float? slotRot = 0f, float? qualityAmt = 1f, bool? scaleWeightWithCond = false, bool? destroyAtZeroCond = false, int? recmin = 1, int? value = 0, bool? ignoreDepression = false, byte? decayInfo = 0, float? decayMinutes = 0f, bool? usable = false, bool? combinable = false, bool? usableOnLimb = false, bool? usableWithLMB = false, ItemInfo.Use useAction = null, ItemInfo.UseLimb useLimbAction = null, bool? autoAttack = false, bool? onlyHoldInHands = false, bool? wearable = false, float? wearableArmor = 1f, float? wearableLossMult = 0f, float? wearableIsolation = 0f, int? wearableVisOffset = 5, float? jumpMult = 0f, float? rotSpeed = 0f)
        {
            List<CraftingQuality> qualitiesList = new List<CraftingQuality>();
            qualitiesList.Add(new CraftingQuality(qualities ?? null, qualityAmt ?? 1f));
            // oh btw inner crafting quality is optional, can also use string ^

            Recognition rec = new Recognition(recmin ?? 1);

            // Required
            __instance.fullName = name;
            __instance.description = description ?? "";
            __instance.tags = tags;
            __instance.category = category;
            __instance.qualities = qualitiesList;
            __instance.rec = rec;

            // Optionals
            __instance.weight = weight ?? 1f;
            __instance.slotRotation = slotRot ?? 0f;
            __instance.scaleWeightWithCondition = scaleWeightWithCond ?? false;
            __instance.destroyAtZeroCondition = destroyAtZeroCond ?? false;
            __instance.value = value ?? 0;
            __instance.ignoreDepression = ignoreDepression ?? false;

            __instance.decayInfo = decayInfo ?? 0;
            if (__instance.decayInfo != 0)
            {
                __instance.decayMinutes = decayMinutes ?? 0f;
                __instance.rotSpeed = rotSpeed ?? 0f;
            }


            __instance.usable = usable ?? false;
            __instance.combineable = combinable ?? false;
            if (__instance.usable == true)
            {
                __instance.usableWithLMB = usableWithLMB ?? false;
                __instance.useAction = useAction ?? null;
                __instance.useLimbAction = useLimbAction ?? null;
                __instance.usableOnLimb = usableOnLimb ?? false;
                __instance.autoAttack = autoAttack ?? false;
                __instance.onlyHoldInHands = onlyHoldInHands ?? false;
            }

            __instance.wearable = wearable ?? false;
            if (__instance.wearable)
            {
                if (string.IsNullOrEmpty(desiredWearLimb))
                    throw new ArgumentException("desiredWearLimb must be set when wearable is true");

                if (string.IsNullOrEmpty(wearSlotID))
                    throw new ArgumentException("wearSlotID must be set when wearable is true");

                __instance.desiredWearLimb = desiredWearLimb;
                __instance.wearSlotId = wearSlotID;
                __instance.wearableArmor = wearableArmor ?? 1f;
                __instance.wearableHitDurabilityLossMultiplier = wearableLossMult ?? 1f;
                __instance.wearableIsolation = wearableIsolation ?? 0f;
                __instance.wearableVisualOffset = wearableVisOffset ?? 5;
                __instance.jumpHeightMultChange = jumpMult ?? 0f;
            }

            return __instance;
        }


        public static Item AddItem(string itemName, string sprite, Vector2 colliderSize, float size, int sortOrder)
        {
            GameObject itemGO = new GameObject(itemName);
            Item newItem = itemGO.AddComponent<Item>();

            Rigidbody2D rb = itemGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.sleepMode = RigidbodySleepMode2D.StartAwake;

            SpriteRenderer sr = itemGO.AddComponent<SpriteRenderer>();
            BoxCollider2D col = itemGO.AddComponent<BoxCollider2D>();
            sr.sprite = FileLoader.LoadEmbeddedSprite(sprite, size);
            itemGO.layer = LayerMask.NameToLayer("Item");
            sr.sortingLayerName = "Default";
            sr.sortingOrder = sortOrder;
            col.size = colliderSize;

            newItem.rb = rb;

            SharedState.ModPrefabs[itemName] = itemGO;

            return newItem;
        }

        public static Item sextoy;
        public static Item viagra;
        public static Item condom;

        public static void RegisterItems()
        {
            // Smaller ppu number means bigger
            sextoy = AddItem("mfs.sextoy", "ScavPrototypeSexMod.Assets.example.png", new Vector2(1.5f, 1.5f), 100, 0);
            viagra = AddItem("mfs.vyagra", "ScavPrototypeSexMod.Assets.vyagra.png", new Vector2(1f, 1.5f), 400, 200);
            string[] condomcolors = new string[]
            {
                "redcondom",
                "silvercondom",
                "goldcondom",
                "pinkcondom"
            };
            int rand = UnityEngine.Random.Range(0, condomcolors.Length);
            condom = AddItem("mfs.condom", "ScavPrototypeSexMod.Assets." + condomcolors[rand] + ".png", new Vector2(1.5f, 1.5f), 275, 200);

            var sextoyinfo = new ItemInfo();
            var sextoyitem = AddItemInfo(sextoyinfo, name: "sextoy", tags: "", category: "utility", qualities: "hammering",
                description: "A sex toy.", desiredWearLimb: null, wearSlotID: null, value: 50, usable: true,
                usableWithLMB: true, usableOnLimb: true, useAction: TestItemUsed, useLimbAction: TestItemUsedLimb);

            var viagrainfo = new ItemInfo();
            var viagraitem = AddItemInfo(viagrainfo, name: "vyagra", tags: "", qualities: "rippable",
                description: "Prescription for a specific naughty bit.", desiredWearLimb: null, wearSlotID: null,
                value: 15, category: "drug", slotRot: 0f, usable: true, combinable: true, weight: 0.2f,
                scaleWeightWithCond: true, useAction: UseViagra);

            var condominfo = new ItemInfo();
            var condomitem = AddItemInfo(condominfo, name: "condom", tags: "", qualities: "rippable",
                description: "A condom used to negate risk of STDs.", desiredWearLimb: null, wearSlotID: null,
                weight: 0.15f, value: 30, slotRot: 0f, usable: true, useAction: UseCondom, destroyAtZeroCond: true, category: "utility", recmin: 5);

            var lubeinfo = new LiquidItemInfo();
            var lubeitem = AddItemInfo(lubeinfo, name: "lube?", tags: "", category: "water", qualities: null,
                description: "May or may not be lube?", desiredWearLimb: null, wearSlotID: null,
                weight: 0.75f, value: 10, slotRot: 0f, usable: true, useAction: UseLube, destroyAtZeroCond: false, recmin: 5);

            FluidManager fluidManager = new FluidManager();
            fluidManager.RegisterFluids();

            lubeinfo.capacity = fluidManager.liquidCapacities.Sum();

            var items = new List<Item> { sextoy, viagra, condom, fluidManager.lube };
            var infos = new List<ItemInfo> { sextoyitem, viagraitem, condomitem, lubeitem };

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                ItemInfo info = infos[i];

                item.id = item.name;
                info.SetTags();
                Item.GlobalItems.Add(item.id, info);

                Plugin.Log.LogInfo("Registered item " + item.name + " with id: " + item.id + "!");
            }

            var container = fluidManager.lube.gameObject.AddComponent<WaterContainerItem>();
            container.stack = fluidManager.lubeContents;
        }

        // For the viagra
        public static void UseViagra(Body body, Item item)
        {
            int chance = UnityEngine.Random.RandomRange(1, 250);
            item.condition -= 0.3f;
            if (SharedState.vCoroutine != null)
            {
                SharedState.pc.StopCoroutine(SharedState.vCoroutine);
            }

            SharedState.vCoroutine = SharedState.pc.StartCoroutine(ViagraRoutine(body, chance));
        }

        private static IEnumerator ViagraRoutine(Body body, int chance)
        {
            float duration = 60f;

            while (duration > 0f)
            {
                body.bloodViscous = Mathf.Max(body.bloodViscous - 0.15f * Time.deltaTime, -25f);
                body.forcedSleepQuality = Body.SleepQuality.Bad;

                if (chance == 49)
                {
                    body.sicknessAmount = Mathf.Min(body.sicknessAmount + Time.deltaTime * 0.15f, 25f);
                    body.averagePain = Mathf.Min(body.averagePain + Time.deltaTime * 0.15f, 10f);
                }
                else if (chance == 2)
                {
                    body.sicknessAmount = Mathf.Min(body.sicknessAmount + Time.deltaTime * 0.15f, 95f);
                    if (body.sicknessAmount >= 75)
                    {
                        body.vomiter.Vomit();
                        body.consciousness = Mathf.Max(body.consciousness - Time.deltaTime * 0.10f, 65f);
                        body.averagePain = Mathf.Min(body.averagePain + Time.deltaTime * 0.15f, 15f);
                    }
                }

                if (SharedState.CurrentGender == SharedState.Gender.Male || SharedState.CurrentGender == SharedState.Gender.Intersex)
                {
                    SharedState.Hardness = Mathf.Min(SharedState.Hardness + Time.deltaTime * 0.25f, 1f);
                    if (SharedState.Hardness > 0.95f)
                    {
                        body.wetness = Mathf.Min(body.wetness + Time.deltaTime * 0.25f, 30f);
                        SharedState.Horniness = Mathf.Min(SharedState.Horniness + Time.deltaTime * 1f, 100f);
                    }
                }
                else if (SharedState.CurrentGender == SharedState.Gender.Female)
                {
                    body.wetness = Mathf.Min(body.wetness + Time.deltaTime * 0.15f, 30f);
                }
                else
                {
                    body.talker.Talk("Why did I do that?...");
                    body.wetness = Mathf.Min(body.wetness + Time.deltaTime * 0.15f, 15f);
                    duration = 0f;
                }

                duration -= Time.deltaTime;
                yield return null;
            }
        }

        // For the condom
        // TODO: Dropping the condom after activating the coroutine destroys the condom lol
        public static void UseCondom(Body body, Item item)
        {
            // Put protection on the player!
            if (SharedState.CurrentGender == Gender.Male || SharedState.CurrentGender == Gender.Intersex)
            {
                item.condition -= 1f;
                SharedState.WearingCondom = true;
            }
            else if (SharedState.CurrentGender == Gender.Female)
            {
                body.talker.Talk("I can't use this... but maybe someone else can?", null, true, false);
            }
            else
            {
                body.talker.Talk("I don't have any use for this. Maybe I should sell it.", null, true, false);
            }
        }

        public static void UseLube(Body body, Item item)
        {
            item.GetComponent<WaterContainerItem>().Drink(body, 10f, "drink");
            Plugin.Log.LogWarning("Yummers....");
        }

        public static void ApplyLube(float ml, Body body)
        {
            ml -= 10f;

            Plugin.Log.LogWarning("DO the thing here.");

            body.talker.TalkDelayed(0.5f, "That tasted terrible...", null, true, false);
        }

        // For the sex toy
        public static void TestItemUsed(Body body, Item item)
        {
            Plugin.Log.LogInfo("Yeah you used me, now what? lol");
            // Not the way to go here.
            SharedState.stdTypes["Syphilis"] = true;
        }

        public static void TestItemUsedLimb(Limb limb, Item item)
        {
            Plugin.Log.LogInfo("Applied to limb: " + limb.name);
        }
    }

}
