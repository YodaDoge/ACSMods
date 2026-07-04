using HarmonyLib;
using ModLoaderLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.SaveLoad;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks.AutoA2H
{
	public partial class A2H : Mod
	{
		[HarmonyPatch]
		public static class Patch
		{
			static AutoThoughtsWindow _panelAutoThink;
			private static string PanelName;

			private static void InitPanel(Wnd_A2HCreateAgg __instance, Npc npc, List<string> selectedThoughts)
			{
				var panelAutoThink = _panelAutoThink; // private variable to make findref stuff easier
				if (_panelAutoThink == null || string.IsNullOrEmpty(PanelName) || __instance.GetChild(PanelName) == null) 
				{
					panelAutoThink = new AutoThoughtsWindow();
					PanelName = panelAutoThink.name;
					__instance.AddChild(panelAutoThink);
					panelAutoThink.SetPosition(panelAutoThink.position.x - (panelAutoThink.size.x + 2), panelAutoThink.y + 10, panelAutoThink.z - 1);
				}
				panelAutoThink.Update(npc, selectedThoughts);
				_panelAutoThink = panelAutoThink;
			}

			private static bool _hadError = false;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "ShowNpc")]
			public static void OpenWindow(Npc npc)
			{
				if (!_info.Enabled)
					return;

				try
				{
					AddLog("ShowNpc");
					_hadError = false;
					InitNullLists(npc);
					var window = SingletonWindowEx<Wnd_A2HCreateAgg, UI_A2HCreateAgg>.Instance;
					//var saved = MLLMain.GetSaveOrDefault<Dictionary<int, List<string>>>(_info.Name);
					var selectedThoughts = AutoNPC.ContainsKey(npc.ID) ? AutoNPC[npc.ID] : new List<string>();
					InitPanel(window, npc, selectedThoughts);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
					KLog.Dbg(ex.ToString());
					_hadError = true;
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "OnHide")]
			public static void OnHide(Wnd_A2HCreateAgg __instance, Npc ___npc)
			{
				if (!_info.Enabled || _hadError || _panelAutoThink == null)
					return;

				try
				{
					AutoNPC[___npc.ID] = _panelAutoThink.GetCheckedThoughts(___npc);
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
