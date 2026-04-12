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
			private static Dictionary<string, string> F2A = new Dictionary<string, string>
			{
				{ "Scene", "AScene" },
				{ "Target", "ATarget" },
				{ "Emotion", "AEmotion" }
			};

		[HarmonyPatch]
		public static class Patch
		{
			private static Npc _npc;
			private static int _lastFrame;
			private static HashSet<ThinkFrag> _think2Consider = new HashSet<ThinkFrag>();
			private static AnimalToHuman A2H => _npc.A2H;
			private static HashSet<string> _ConsiderableFrag = new HashSet<string>();
			private static string _ConsiderFrag;
			static AutoThoughtsWindow _configArea;
			static HumanoidEvolutionMgr HMgr => HumanoidEvolutionMgr.Instance;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "OnInit")]
			public static void OnInit(Wnd_A2HCreateAgg __instance)
			{
				if (!_info.Enabled)
					return;

				try
				{
					var x = IManagerModule_LoopInterval<HumanoidEvolutionMgr>.Instance.Fragments;
					_configArea = new AutoThoughtsWindow();

					__instance.AddChild(_configArea);
					_configArea.SetPosition(_configArea.position.x - (_configArea.size.x + 2), _configArea.y + 30, _configArea.z);
					_configArea.AddCopyPasteButtons(__instance);
				}
				catch (Exception ex)
				{
					ShowMessage(ex.ToString());
				}

			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "ShowNpc")]
			public static void OpenWindow(Wnd_A2HCreateAgg __instance, Npc npc)
			{
				if (!_info.Enabled)
					return;

				try
				{
					var saved = MLLMain.GetSaveOrDefault<Dictionary<int, List<string>>>(_info.Name);
					var selectedThoughts = AutoNPC.ContainsKey(npc.ID) ? AutoNPC[npc.ID] : new List<string>();
					_configArea.Update(npc, selectedThoughts);
				}
				catch (Exception ex)
				{
					ShowMessage(ex.Message);
					KLog.Dbg(ex.ToString());
				}
			}


			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_A2HCreateAgg), "OnHide")]
			public static void OnHide(Wnd_A2HCreateAgg __instance, Npc ___npc)
			{
				if (!_info.Enabled)
					return;

				AutoNPC[___npc.ID] = _configArea.GetCheckedThoughts(___npc);
				try
				{
					var isNew = !MLLMain.AddOrOverWriteSave(_info.Name, AutoNPC);

					ThinkIfYouCan(___npc);
				}
				catch (Exception ex)
				{
					ShowMessage(ex.ToString());
					KLog.Dbg(ex.ToString());
				}

			}
		}
	}
}
