using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static XiaWorld.JianghuMgr;

namespace ACS_Yoda_Tweaks
{
	[HarmonyPatch]
	public static class IndexOfRedirectPatch
	{
		public static int LowerIndexOf(string instance, string searchString)
		{
			return instance.IndexOf(searchString, StringComparison.OrdinalIgnoreCase);
		}
		
		static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(SearchMainPanel), "UpdateList");
			yield return AccessTools.Method(typeof(SearchMainPanel), "CheckNormalItems");
			yield return AccessTools.Method(typeof(SearchMainPanel), "CheckNoneItems");
			yield return AccessTools.Method(typeof(SearchMainPanel), "CheckThing");

		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var targetMethod = AccessTools.Method(typeof(string), nameof(string.IndexOf), new[] { typeof(string) });
			var replacementMethod = AccessTools.Method(typeof(IndexOfRedirectPatch), nameof(LowerIndexOf));

			foreach (var instruction in instructions)
			{
				if (instruction.Calls(targetMethod))
				{
					// Swap the 'call' to string.IndexOf with a 'call' to MyCustomIndexOf
					yield return new CodeInstruction(OpCodes.Call, replacementMethod);
				}
				else
				{
					yield return instruction;
				}
			}
		}

		//[HarmonyPostfix]
		//[HarmonyPatch(typeof(SearchMainPanel), "SetBntInfo")]
		//public static void SetBntInfo(SearchMainPanel __instance, string ___searchtxt)
		//{
		//	var txtSearch = __instance.UIInfo.m_F.m_title;
		//	txtSearch.onChanged.Clear();
		//	txtSearch.onChanged.Add(() =>
		//	{
		//		var search2 = __instance.UIInfo.m_F.title;
		//		if (___searchtxt?.ToLower() != search2.ToLower())
		//		{
		//			searchtxt = txtSearch;
		//			UpdateList();
		//			UpdateType();
		//		}
		//	});
		//}

		//[HarmonyPrefix]
		//[HarmonyPatch(typeof(SearchMainPanel), "UpdateList")]
		//public static void SetBntInfo(SearchMainPanel __instance, ref string ___searchtxt)
		//{
		//	___searchtxt = __instance.UIInfo.m_F.title?.ToLower();
		//}
	}
}