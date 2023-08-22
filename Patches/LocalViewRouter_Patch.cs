using HarmonyLib;
using Kitchen;
using KitchenLib.Utils;
using UnityEngine;

namespace KitchenToggleAct.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ViewType view_type, ref GameObject __result)
        {
            if (view_type == ViewType.Player && __result != null && __result.GetComponent<ToggleActIndicatorView>() == null)
            {
                Main.LogInfo("Updating Player Prefab");
                PlayerView playerView = __result.GetComponent<PlayerView>();
                ToggleActIndicatorView indicatorView = __result.AddComponent<ToggleActIndicatorView>();
                indicatorView.PlayerView = playerView;
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
                MaterialUtils.ApplyMaterial(indicator, MaterialUtils.GetExistingMaterial("Indicator Light On"));
                indicator.transform.SetParent(__result.transform);
                indicator.transform.Reset();
                indicator.transform.localScale = Vector3.one * 0.05f;
                indicator.transform.localPosition = Vector3.up * 1.5f;
                indicator.SetActive(false);
                indicatorView.Indicator = indicator;
            }
        }
    }
}
