using FairyGUI;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class MapSeedFix : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("SeedFix", "", true);

		private static bool _pauseAfterLoad = false;

		public MapSeedFix(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameDIY), "_RefreshEnv")]
			public static void OnShowUpdate(Wnd_GameDIY __instance, UI_WindowGameDiy ___UIInfo)
			{
				___UIInfo.m_Evn.m_n239.title = World.Instance.map.Seed;
			}

			//[HarmonyPrefix]
			//[HarmonyPatch(typeof(Wnd_GameDIY), "CopyEditSeed")]
			//public static bool CopyEditSeed(Wnd_GameDIY __instance, UI_WindowGameDiy ___UIInfo, Dictionary<string, GList> ___ConstractEnvList, ref string __result)
			//{
			//	EnvSeed envSeed = new EnvSeed();
			//	envSeed.Seed = World.Instance.map.Seed;
			//	envSeed.EnvC = new HashSet<string>();
			//	foreach (KeyValuePair<string, GList> constractEnv in ___ConstractEnvList)
			//	{
			//		GList value = constractEnv.Value;
			//		ImmortalConstraintDef def = DifficultyMgr.IConstraints.GetDef(constractEnv.Key);
			//		List<int> selection = value.GetSelection();
			//		for (int i = 0; i < selection.Count; i++)
			//		{
			//			string item = def.Selector[selection[i]];
			//			envSeed.EnvC.Add(item);
			//		}
			//	}
			//	__result = JsonConvert.SerializeObject(envSeed, GameWatch.JsonSetting);
			//	return false;
			//}

			//[HarmonyPrefix]
			//[HarmonyPatch(typeof(Wnd_GameDIY), "PasteEditSeed")]
			//public static bool CopyEditSeed(Wnd_GameDIY __instance, Dictionary<string, GList> ___ConstractEnvList, UI_WindowGameDiy ___UIInfo, string rs)
			//{
			//	EnvSeed envSeed = JsonConvert.DeserializeObject<EnvSeed>(rs, GameWatch.JsonSetting);
			//	if (envSeed.EnvC == null)
			//	{
			//		envSeed = null;
			//	}
			//	___UIInfo.m_Evn.m_n239.title = envSeed.Seed;
			//	foreach (KeyValuePair<string, GList> constractEnv in ___ConstractEnvList)
			//	{
			//		GList value = constractEnv.Value;
			//		ImmortalConstraintDef def = DifficultyMgr.IConstraints.GetDef(constractEnv.Key);
			//		value.ClearSelection();
			//		foreach (string item in envSeed.EnvC)
			//		{
			//			int num = def.Selector.IndexOf(item);
			//			if (num >= 0)
			//			{
			//				value.AddSelection(num, scrollItToView: false);
			//			}
			//		}
			//	}
			//	return false;
			//}

			//private class EnvSeed
			//{
			//	public string Seed;

			//	public HashSet<string> EnvC;
			//}
		}
	}
}