using FairyGUI;
using FairyGUI.Utils;
using HarmonyLib;
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

			[HarmonyPatch(typeof(UI_Panel_ThingInfo), "ConstructFromXML")]
			public static void AddComboBox(UI_Panel_ThingInfo __instance, XML xml)
			{
				
				UI_ComboBox cbx = UI_ComboBox.CreateInstance();
				cbx.name = "YodaDoge.BranchSelect";
				cbx.items = new string[] { "None", "Test" };
				cbx.values = new string[] { "None", "Test" };
				cbx.selectedIndex = 0;
				//cbx.z = __instance.z - 1;
				cbx.x -= 50;
				cbx.onChanged.Add(e =>
				{

				});
				
				__instance.AddChild(cbx);
				cbx.dropdown.minWidth = 50;
				cbx.UpdateDropdownList();
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