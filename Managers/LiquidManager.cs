using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ScavPrototypeSexMod.Managers
{
    public static class LiquidManager
    {
        public static void Initialize()
        {
            Plugin.Log.LogInfo("UIManager Initialized");
        }

        public static void drinkCum(float ml, Body body)
        {
            float num = ml * 0.001f;
            body.Drink(num * 90f);
            body.Eat(30f * num, 4f * num);
            body.temperature -= num * 2.5f;
            body.happiness += 5f * num;
        }

        public static LiquidType AddLiquidInfo(LiquidType __instance, string Lname, string qualities = null, float? qualityAmt = 1f, bool? localeFromItem = false, Color? Lcolor = null, bool? healthUsable = false, bool? injectable = false, float? valuePerLiter = 0f, float? injectionSickness = 0f, LiquidType.OnDrink onDrink = null, LiquidType.OnHealthUse onHealthUse = null)
        {
            // Do stuff
            List<CraftingQuality> qualitiesList = new List<CraftingQuality>();
            qualitiesList.Add(new CraftingQuality(qualities ?? null, qualityAmt ?? 1f));

            __instance.localeName = Lname;
            __instance.localeFromItem = localeFromItem ?? false;
            __instance.qualities = qualitiesList;

            __instance.color = Lcolor ?? new Color32(255, 255, 255, 1);
            __instance.healthUsable = healthUsable ?? false;
            __instance.injectable = injectable ?? false;
            if (__instance.injectable)
            {
                __instance.injectionSickness = injectionSickness ?? 0f;
            }

            __instance.valuePerLiter = valuePerLiter ?? 0f;
            __instance.onDrink = onDrink ?? null;
            __instance.onHealthUse = onHealthUse ?? null;

            return __instance;
        }

        public static GameObject AddLiquid(string gameObjName)
        {
            GameObject liquidGO = new GameObject(gameObjName);
            ParticleSystem ps = liquidGO.AddComponent<ParticleSystem>();

            return liquidGO;
        }

        public static IEnumerator RegisterLiquids(FluidManager __instance, LiquidType lt)
        {
            yield return null;

            var info = new LiquidType();
            LiquidType liqtype = AddLiquidInfo(info, "cum", null, null, false, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false, false, 13f, 2f, drinkCum, null);
            Liquids.Registry.Add(liqtype.localeName, liqtype);
            var cum = AddLiquid("cum");


            Plugin.Log.LogInfo("Registered cum liquid");
        }

    }
}
