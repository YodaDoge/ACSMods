using HarmonyLib;
using ModLoaderLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.SaveLoad;
using static XiaWorld.HumanoidEvolutionMgr;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H : Mod
	{
			private static Dictionary<string, string> FragAggName2AggDefName = new Dictionary<string, string>
			{
				{ "Scene", "AScene" },
				{ "Target", "ATarget" },
				{ "Emotion", "AEmotion" }
			};

		[HarmonyPatch]
		public static class Patch
		{
			static AutoThoughtsWindow _panelAutoThink;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "OnInit")]
			public static void OnInit(Wnd_A2HCreateAgg __instance)
			{
				if (!_info.Enabled)
					return;
				InitPanel(__instance);

			}

			private static void InitPanel(Wnd_A2HCreateAgg __instance)
			{
				if (_panelAutoThink != null)
					return;
				try
				{
					_panelAutoThink = new AutoThoughtsWindow();
					//TODO: move this into constructor
					__instance.AddChild(_panelAutoThink);
					_panelAutoThink.SetPosition(_panelAutoThink.position.x - (_panelAutoThink.size.x + 2), _panelAutoThink.y + 10, _panelAutoThink.z - 1);
					_panelAutoThink.AddCopyPasteButtons(__instance);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "ShowNpc")]
			public static void OpenWindow(Wnd_A2HCreateAgg __instance, Npc npc)
			{
				if (!_info.Enabled)
					return;
				InitPanel(__instance);
				InitNullLists(npc);
				try
				{

					//var saved = MLLMain.GetSaveOrDefault<Dictionary<int, List<string>>>(_info.Name);
					var selectedThoughts = AutoNPC.ContainsKey(npc.ID) ? AutoNPC[npc.ID] : new List<string>();
					_panelAutoThink.Update(npc, selectedThoughts);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
					KLog.Dbg(ex.ToString());
				}
			}


			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "OnHide")]
			public static void OnHide(Wnd_A2HCreateAgg __instance, Npc ___npc)
			{
				if (!_info.Enabled)
					return;

				AutoNPC[___npc.ID] = _panelAutoThink.GetCheckedThoughts(___npc);
				try
				{
					var isNew = !MLLMain.AddOrOverWriteSave(_info.Name, AutoNPC);

					ThinkIfYouCan(___npc);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
					KLog.Dbg(ex.ToString());
				}

			}
		}
	}
}
