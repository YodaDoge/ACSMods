using FairyGUI;
using FairyGUI.Utils;
using HarmonyLib;
using rail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class BranchDropDown : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("BranchDropDown", "Rightclick like its 2005", true);

		private static bool _pauseAfterLoad = false;

		public BranchDropDown(bool defaultEnabled) : base(defaultEnabled)
		{
			//XiaWorld.UI.InGame.UI_Panel_ThingInfo  m_atkPanel = (GGroup)GetChildAt(47);
		}

		[HarmonyPatch]
		public static class Patch
		{

			[HarmonyPostfix]
			[HarmonyPatch(typeof(UI_Panel_ThingInfo), "ConstructFromXML")]
			public static void AddComboBox(UI_Panel_ThingInfo __instance, XML xml)
			{
				UI_ComboBox cbx = UI_ComboBox.CreateInstance();
				cbx.name = _Name;
				cbx.items = new string[] { "None", "Test" };
				cbx.values = new string[] { "None", "Test" };
				cbx.selectedIndex = 0;
				//cbx.z = __instance.z - 1;
				cbx.x -= 61;
				cbx.y -= 30;
				cbx.onChanged.Add(AssignBranch);

				__instance.AddChild(cbx);
				cbx.width = cbx.minWidth = cbx.maxWidth  = 67;
				cbx.fontsize = cbx.titleFontSize = 8;
				cbx.UpdateDropdownList();
			}

			private static void AssignBranch(EventContext context)
			{
				try
				{
					var cbx = context.sender as UI_ComboBox;
					if (cbx.value != null)
					{

						int id = int.Parse(cbx.value);
						if (id > 0)
						{
							var x = new List<int>(SchoolMgr.Instance.GetTang(id).Npcs);
							x.Add(_lastNpc.ID);
							SchoolMgr.Instance.SetTangNpcs(id, x);
						}
					}
					else
						SchoolMgr.Instance.TangRemoveNpc(_lastNpc.TangJoined, _lastNpc);

					EventMgr.Instance.EventTrigger(g_emEvent.UpdateSchool, null);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}

			}

			private readonly static string _Name = "YodaDoge.BranchSelect";
			private static Npc _lastNpc;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Panel_ThingInfo), "UpdateThings")]
			public static void Blergh(Panel_ThingInfo __instance, UI_Panel_ThingInfo ___Panel)
			{
				_lastNpc = null;
				var cbx = ___Panel.GetChild(_Name) as UI_ComboBox;
				cbx.visible = false;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Panel_ThingInfo), "UpdateThing")]
			public static void Reset(Panel_ThingInfo __instance, UI_Panel_ThingInfo ___Panel, Thing t = null, AreaBase a = null)
			{
				if (_lastNpc != t)
					_lastNpc = null;
				var cbx = ___Panel.GetChild(_Name) as UI_ComboBox;
				cbx.visible = t is Npc npc && npc.IsRealPlayerThing && GameWatch.Instance.Mode != g_emGameMode.RPG && SchoolMgr.Instance.Tangs?.Count > 0 && npc.IsSmartRace;
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Panel_ThingInfo), "ShowNpc")]
			public static void UpdateBranches(Panel_ThingInfo __instance, UI_Panel_ThingInfo ___Panel, Npc npc)
			{
				var cbx = ___Panel.GetChild(_Name) as UI_ComboBox;
				if (!cbx.visible || _lastNpc == npc)
					return;
				cbx.enabled = SchoolMgr.Instance.Tangs?.Any(x => x.Master == npc.ID) != true && SchoolMgr.Instance.MasterID != npc.ID;
				if (!cbx.enabled)
				{
					cbx.items = new[] { "Master" };
					return;
				}

				_lastNpc = npc;
				cbx.items = new[] { "None" }.Concat(SchoolMgr.Instance.Tangs.Select(x => x.Name)).ToArray();

				cbx.values = new string[] { "0" }.Concat(SchoolMgr.Instance.Tangs.Select(x => x.Id.ToString())).ToArray();
				cbx.value = npc.TangJoined.ToString();
			}

			//UILogicMgr
			//after select => refresh __Status of CBX
			// Loook at to find
			//use post of method from UILogic after things are assigned
			//[HarmonyPostfix]
			//[HarmonyPatch(typeof(UILogicMode_Select), nameof(UILogicMode_Select.OnClickMap))]
			//static void OnMapClick_Prefix(UILogicMode_Select __instance, Vector3 v, int key, int bnt)
			//{
			//	bool showCombobox = __instance.SelectThings?.Count == 1;
			//	//toogle visible 
			//	if (__instance.SelectThings[0] is Npc npc)
			//	{
			//		bool hideBranch = !npc.IsRealPlayerThing;
			//		if (hideBranch)
			//		{
			//			//.visible = false;
			//			return;
			//		}
			//	}
			//}
		}
	}
}