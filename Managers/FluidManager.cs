using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScavPrototypeSexMod.Managers
{
    public class FluidManager
    {
        public static void Initialize()
        {
            Plugin.Log.LogInfo("FluidManager Initialized");
        }

        public static LiquidType AddLiquidType(LiquidType ltype, string localeName, string qualities, Color32? color, float? qualityAmt = 1f, bool? injectable = false, LiquidType.OnDrink onDrink = null, bool? healthUsable = false, LiquidType.OnHealthUse onhealthUse = null, float? valuePerLiter = 1f)
        {
            List<CraftingQuality> qualitiesList = new List<CraftingQuality>();
            qualitiesList.Add(new CraftingQuality(qualities, qualityAmt ?? 1f));

            ltype.localeName = localeName;
            ltype.color = color ?? Color.white;
            ltype.injectable = injectable ?? false;
            ltype.healthUsable = healthUsable ?? false;
            if (ltype.healthUsable)
            {
                ltype.onHealthUse = onhealthUse ?? null;
            }

            ltype.onDrink = onDrink ?? null;

            ltype.valuePerLiter = valuePerLiter ?? 1f;

            return ltype;
        }

        public static List<ItemInfo> liquids = new List<ItemInfo>()
        {
            new ItemInfo { fullName = "Lube", description = "Lubricant" },
            new ItemInfo { fullName = "Cum", description = "Semen" },
            new ItemInfo { fullName = "Urine", description = "Urine" },
            new ItemInfo { fullName = "Saliva", description = "Saliva" }
        };
        Color[] liquidColors = { Color.cyan, Color.white, Color.yellow, Color.gray };

        float[] liquidCapacities = { 500f, 100f, 250f, 10f };

        float[] liquidvalues = { 50f, 30f, 5f, 1f };

        public Item lube = ItemManager.AddItem("mfs.lube", "ScavPrototypeSexMod.Assets.kyjellylube.png", new Vector2(1.5f, 1.5f), 175, 200);

        public void RegisterFluids()
        {
            if (lube == null)
                lube = ItemManager.AddItem("mfs.lube", "ScavPrototypeSexMod.Assets.kyjellylube.png", new Vector2(1.5f, 1.5f), 175, 200);

            List<LiquidStack> lubeContents = new List<LiquidStack>();

            for (int i = 0; i < liquids.Count; i++)
            {
                var template = liquids[i];

                if (template == null || string.IsNullOrEmpty(template.fullName))
                {
                    Plugin.Log.LogWarning($"Skipping liquid at index {i}, template is null or missing fullName");
                    continue;
                }

                var liquidStack = new LiquidStack(template.fullName, liquidCapacities[i]);
                lubeContents.Add(liquidStack);

                var liquidItem = new LiquidItemInfo
                {
                    autoFill = false,
                    capacity = liquidCapacities[i],
                    defaultContents = new List<LiquidStack> { liquidStack }
                };

                // Assign info safely
                liquids[i] = ItemManager.AddItemInfo(
                    liquidItem,
                    name: template.fullName, // unique key per liquid
                    tags: "",
                    category: "water",
                    qualities: template.qualities?.FirstOrDefault()?.id ?? "utility",
                    desiredWearLimb: null,
                    wearSlotID: null,
                    description: template.description ?? "",
                    weight: 0.2f,
                    usable: true,
                    useAction: ItemManager.UseLube,
                    destroyAtZeroCond: false,
                    recmin: 5,
                    value: 50
                );

                // Add to registry safely
                var liquidType = AddLiquidType(
                    new LiquidType(),
                    localeName: template.fullName,
                    qualities: template.qualities?.FirstOrDefault()?.id ?? "utility",
                    color: liquidColors[i],
                    onDrink: ItemManager.ApplyLube,
                    valuePerLiter: liquidvalues[i]
                );

                if (!Liquids.Registry.ContainsKey(liquidType.localeName))
                {
                    Liquids.Registry.Add(liquidType.localeName, liquidType);
                }
            }

            var container = lube.gameObject.GetComponent<WaterContainerItem>();
            if (container == null)
                container = lube.gameObject.AddComponent<WaterContainerItem>();
            container.stack = lubeContents;
        }
    }
}
