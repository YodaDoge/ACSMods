using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static XiaWorld.AuctionData;

namespace ACS_Yoda_Tweaks
{
	public class InstantSearch : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("InstantSearch", "", true);

		private static bool _pauseAfterLoad = false;

		public InstantSearch(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_CangJingGeWindow), "OnInit")]
			public static void PavillionSearchWhileTyping(Wnd_CangJingGeWindow __instance, UI_CangJingGeWindow ___UIInfo)
			{
				___UIInfo.m_F.m_title.onChanged.Add(x => ___UIInfo.m_search.onClick.Call());
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_RemoteStorage), "OnInit")]
			public static void UniverseSearchWhileTyping(Wnd_RemoteStorage __instance)
			{
				__instance.UIInfo.m_n8.onChanged.Add(x => __instance.UIInfo.m_n10.onClick.Call());
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_RemoteStorage), "OnShowUpdate")]
			public static void ShowItemsOnUniverseOpen(Wnd_RemoteStorage __instance, params object[] objs)
			{
				__instance.UIInfo.m_n8.text = string.Empty;
				__instance.UIInfo.m_n10.onClick.Call();
				__instance.UIInfo.m_n8.RequestFocus();
				SelectAllText(__instance.UIInfo.m_n8);
			}

			private static void SelectAllText(GTextInput textField)
			{
				textField.SetSelection(0, textField.text?.Length ?? 0);
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_RemoteStorage), "RefreshData")]
			public static bool FixNullErrorInGetOrCreate(Wnd_RemoteStorage __instance, ref List<string> ___Items, Dictionary<string, List<string>> ___kvs)
			{
				___Items.Clear();
				___kvs.Clear();
				foreach (KeyValuePair<string, string> item in ThingMgr.RemoteItemType)
				{
					if (item.Value == null)
						continue;
					___kvs.GetOrCreate(item.Value).Add(item.Key);
					___Items.Add(item.Key);
				}
				return false;
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_RemoteStorage), "ShowType")]
			public static bool CaseInsensitiveSearch(Wnd_RemoteStorage __instance, ref string ____ShowType, ref List<string> ___Items, ref List<string> ___ShowingItems, string ___seachkey, Dictionary<string, List<string>> ___kvs, string t)
			{
				____ShowType = t;
				List<string> list = (___ShowingItems = ((t != null) ? ___kvs.SafeGet(t) : ___Items));
				string searchval = __instance.UIInfo.m_n8.text;
				if (!string.IsNullOrEmpty(searchval))
				{
					___ShowingItems = new List<string>(list);
					___ShowingItems.RemoveAll(delegate (string name)
					{
						ThingDef def = ThingMgr.Instance.GetDef(g_emThingType.Item, name);
						return def.ThingName.IndexOf(searchval, StringComparison.OrdinalIgnoreCase) < 0;
					});
				}
				if (list != null)
				{
					__instance.UIInfo.m_n5.numItems = ___ShowingItems.Count;
				}
				else
				{
					__instance.UIInfo.m_n5.numItems = 0;
				}
				return false;
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_RemoteStorage), "ClickOnItem")]
			public static bool ClickOnItem(Wnd_RemoteStorage __instance, Thing ___from, EventContext context)
			{
				bool ctrlClick = UnityEngine.Input.GetKey(KeyCode.LeftControl);
				bool shiftClick = UnityEngine.Input.GetKey(KeyCode.LeftShift);
				bool altClick = UnityEngine.Input.GetKey(KeyCode.LeftAlt);

				if (!shiftClick && !ctrlClick && !altClick)
					return true;

				UI_Bnt_EquipItem uI_Bnt_EquipItem = context.data as UI_Bnt_EquipItem;
				string item = uI_Bnt_EquipItem.data as string;
				ThingDef def = ThingMgr.Instance.GetDef(g_emThingType.Item, item);
				var spaceRing = World.Instance.map.SpaceRing;
				int itemCount = spaceRing.GetItemCount(item);

				if (itemCount <= 0)
				{
					return true;
				}
				int num = shiftClick ? itemCount : altClick ? Math.Min(itemCount, 10) : 1;
				spaceRing.TakeOut(item, num, (___from != null) ? ___from.Key : 0, ___from.Pos);
				return false;
			}


			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_Message), "ShowInputSlider")]
			public static void FocusNumericInputOnPrompShow(Wnd_Message __result, string tit, int bnt, Action<float> act, bool modal = false, Func<float, string> Desc = null, Func<float, bool> OkCon = null, float max = 100f, bool intmode = false)
			{
				__result.UIInfo.m_n45.value = 1;
				__result.UIInfo.m_sliderv.m_title.text = "1";
				__result.UIInfo.m_sliderv.m_title.onChanged?.Call();
				//__result.UIInfo.m_sliderv.m_title.RequestFocus();
				SelectAllText(__result.UIInfo.m_sliderv.m_title);
			}
		}
	}
}