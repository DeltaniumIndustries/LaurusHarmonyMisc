using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using HarmonyLib;

[HarmonyPatch(typeof(BlueprintScrollerController))]
public static class BlueprintScrollerControllerPatch
{

    private static void LogLocal(string s)
    {
        if (LaurusHarmonyConfigHandler.LOGGING_HARMONY_BPSC)
        {
            LL.Info("[Harmony] "+s, LogCategory.Harmony);
        }
    }
    [HarmonyPrefix, HarmonyPatch("ResetCaches")]
    public static void ResetCaches_Prefix(BlueprintScrollerController __instance)
    {
        LogLocal("Resetting BlueprintScrollerController caches.");
        __instance.scrollDataCache?.Clear();
        if (__instance.Blueprints != null)
        {
            __instance.Blueprints.Clear();
        }
    }

    [HarmonyPrefix, HarmonyPatch("UpdateDataFilter")]
    public static bool UpdateDataFilter_Prefix(
        BlueprintScrollerController __instance,
        string ObjectFilter,
        HashSet<string> SecondaryFilter)
    {
        if (__instance.scrollDataCache == null || __instance.myScroller == null)
        {
            LogLocal("UpdateDataFilter aborted: missing components.");
            return false;
        }

        LogLocal($"Updating BlueprintScrollerController filter: '{ObjectFilter}' with {SecondaryFilter?.Count ?? 0} secondary filters.");
        var filter = string.IsNullOrEmpty(ObjectFilter) ? string.Empty : ObjectFilter.ToUpperInvariant();
        var blueprints = __instance.Blueprints;

        if (blueprints != null)
        {
            __instance.scrollDataCache.Clear();
            foreach (var blueprint in blueprints)
            {
                if (blueprint?.Name == null) continue;
                if ((filter.Length == 0 || blueprint.Name.ToUpperInvariant().Contains(filter)) &&
                    (SecondaryFilter == null || SecondaryFilter.Contains(blueprint.Name)))
                {
                    if (!__instance.scrollDataCache.TryGetValue(blueprint.Name, out var blueprintScrollerData))
                    {
                        blueprintScrollerData = new BlueprintScrollerData(blueprint.Name);
                        __instance.scrollDataCache[blueprint.Name] = blueprintScrollerData;
                    }
                }
            }
        }
        __instance.myScroller.ReloadData();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch("UpdateSelected")]
    public static bool UpdateSelected_Prefix(BlueprintScrollerController __instance)
    {
        if (__instance.myScroller == null)
        {
            LogLocal("UpdateSelected aborted: myScroller is null.");
            return false;
        }
        LogLocal("Refreshing BlueprintScroller active cells.");
        __instance.myScroller.RefreshActiveCellViews();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch("Start")]
    public static void Start_Prefix(BlueprintScrollerController __instance)
    {
        if (__instance.myScroller == null)
        {
            LogLocal("Start aborted: myScroller is null.");
            return;
        }
        LogLocal("Assigning BlueprintScrollerController delegate.");
        __instance.myScroller.Delegate = __instance;
    }

    [HarmonyPrefix, HarmonyPatch("GetNumberOfCells")]
    public static bool GetNumberOfCells_Prefix(BlueprintScrollerController __instance, ref int __result)
    {
        __result = __instance.scrollDataCache?.Count ?? 0;
        LogLocal($"GetNumberOfCells returning {__result}.");
        return false;
    }

    [HarmonyPrefix, HarmonyPatch("GetCellViewSize")]
    public static bool GetCellViewSize_Prefix(ref float __result)
    {
        __result = 24f;
        return false;
    }

    [HarmonyPrefix, HarmonyPatch("GetCellView")]
    public static bool GetCellView_Prefix(
        BlueprintScrollerController __instance,
        EnhancedScroller scroller,
        int dataIndex,
        int cellIndex,
        ref EnhancedScrollerCellView __result)
    {
        if (scroller == null || __instance.animalCellViewPrefab == null || __instance.scrollDataCache == null)
        {
            LogLocal($"GetCellView aborted: invalid parameters (dataIndex: {dataIndex}).");
            return false;
        }

        var blueprintKeys = new List<string>(__instance.scrollDataCache.Keys);
        if (dataIndex < 0 || dataIndex >= blueprintKeys.Count) return false;

        var blueprintName = blueprintKeys[dataIndex];
        if (!__instance.scrollDataCache.TryGetValue(blueprintName, out var blueprintData)) return false;

        var cellView = scroller.GetCellView(__instance.animalCellViewPrefab) as BlueprintCellView;
        if (cellView != null)
        {
            cellView.SetData(blueprintData);
            __result = cellView;
            LogLocal($"GetCellView successfully returned cell for {blueprintName}.");
        }
        return false;
    }
}
