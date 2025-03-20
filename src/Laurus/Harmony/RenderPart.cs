using System.Diagnostics;
using HarmonyLib;
using XRL.World.Parts;

[HarmonyPatch(typeof(Render))]
public static class Patch_Render_ProcessRenderString
{
        private static void LogLocal(string s)
    {
        if (LaurusHarmonyConfigHandler.LOGGING_HARMONY_RENDER)
        {
            LL.Info("[Harmony] "+s, LogCategory.Harmony);
        }
    }

    static Patch_Render_ProcessRenderString()
    {
        LogLocal("Harmony patch for Render.ProcessRenderString loaded.");
    }

    [HarmonyPrefix]
    [HarmonyPatch("ProcessRenderString")]
    public static bool Prefix(ref string what, ref string __result)
    {
        LogLocal($"[Laurus] ProcessRenderString called with input: '{what ?? "null"}'");

        if (string.IsNullOrEmpty(what))
        {
            __result = string.Empty;
            
            // Capture stack trace for debugging null cases
            var stackTrace = new StackTrace(true);
            LogLocal($"[Laurus] NULL DETECTED in ProcessRenderString! Stack trace:\n{stackTrace}");
            
            return false; // Skip original method execution
        }

        if (what.Length > 1)
        {
            if (int.TryParse(what, out int charCode))
            {
                __result = ((char)charCode).ToString();
            }
            else
            {
                __result = what;
            }
            return false;
        }

        switch (what)
        {
            case "&":
                __result = "&&";
                return false;
            case "^":
                __result = "^^";
                return false;
        }

        __result = what;
        return false;
    }
}
