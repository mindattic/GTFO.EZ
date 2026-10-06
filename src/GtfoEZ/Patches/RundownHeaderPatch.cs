using System;
using CellMenu;
using HarmonyLib;

namespace GtfoEZ.Patches;

/// <summary>
/// Appends a small confirmation line under the rundown title on the rundown-select screen, so
/// it's visible in-game without checking logs. Re-derives the base title from the original text
/// each call (splitting on "&lt;br&gt;") instead of blindly appending, since UpdateHeaderText can
/// fire more than once per screen and a naive append would stack duplicates.
/// </summary>
[HarmonyPatch(typeof(CM_PageRundown_New), nameof(CM_PageRundown_New.UpdateHeaderText))]
internal static class CM_PageRundown_New_UpdateHeaderText_Patch
{
    private static readonly string[] BreakTag = { "<br>" };

    [HarmonyPostfix]
    private static void Postfix(CM_PageRundown_New __instance)
    {
        var header = __instance.m_textRundownHeader;
        if (header == null) return;

        var title = header.text.Contains("<br>") ? header.text.Split(BreakTag, StringSplitOptions.None)[0] : header.text;
        header.SetText($"{title}<br><size=50%>GtfoEZ active - Goo goo. Gah gah.</size>");
    }
}
