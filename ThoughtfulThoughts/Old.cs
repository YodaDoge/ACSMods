//using FairyGUI;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using XiaWorld;
//using XiaWorld.UI.InGame;

//namespace ThoughtfulThoughts
//{
//	[HarmonyPatch(typeof(Wnd_GameMain))]
//	public static class MainManager_Run_Patch
//	{
//		private static bool _didPatch = false;

//		[HarmonyPostfix]
//		[HarmonyPatch("OnInit")]
//		public static void OnInit_Postfix(Wnd_GameMain __instance)
//		{
//			if (_didPatch)
//				return;
//			_didPatch = true;

//			KLog.Dbg("Patching A2H manually:");


//			//TODO: plug below into a class thats not a generic as base
//			var harmony = new Harmony("YodaDoge.TT");

//			// Use DeclaredMethod to specifically target the implementation in Wnd_AnimalToHuman
//			var original = AccessTools.DeclaredMethod(typeof(Wnd_AnimalToHuman), "ShowNpc");

//			if (original == null)
//			{
//				// If DeclaredMethod fails, the method might actually be in the Base class
//				original = AccessTools.Method(typeof(Wnd_AnimalToHuman), "ShowNpc");
//			}

//			var postfix = AccessTools.Method(typeof(Patch_WndAnimalToHuman), nameof(Patch_WndAnimalToHuman.ShowNpc_PostfixShowNpc));

//			if (original != null && postfix != null)
//			{
//				var result = harmony.Patch(original, postfix: new HarmonyMethod(postfix));
//				KLog.Dbg("AH2 Patch: " + result.ToString());
//			}
//			else
//			{
//				if (original == null)
//					KLog.Dbg("A2H: Original null");

//				if (postfix == null)
//					KLog.Dbg("A2H: postfix null");
//			}
//		}
//	}

//	[HarmonyPatch(typeof(Wnd_AnimalToHuman))]
//	public static class RefreshTest
//	{
//		private static Npc _npc;

//		private static bool _didPatch = false;

//		[HarmonyPostfix]
//		[HarmonyPatch("Refresh")]
//		public static void ShowNpc_PostfixShowNpc()
//		{
//			KLog.Dbg("A2H Refresh");
//		}
//	}

//	//[HarmonyPatch(typeof(Wnd_AnimalToHuman))]
//	public static class Patch_WndAnimalToHuman
//	{
//		private static Npc _npc;

//		private static bool _didPatch = false;

//		//[HarmonyPostfix]
//		//[HarmonyPatch("ShowNpc")]
//		public static void ShowNpc_PostfixShowNpc(Npc npc)
//		{
//			KLog.Dbg("A2H ShowNPC");
//			_npc = npc;

//			if (_didPatch)
//				return;


//			_didPatch = true;

//			var instance = SingletonWindowEx<Wnd_AnimalToHuman, UI_AnimalToHuman>.Instance;
//			instance.UIInfo.m_thinks.itemRenderer += (idx, itm) => RenderFrag_Patch(instance, null, null, idx, itm);
//		}

//		[HarmonyPostfix]
//		[HarmonyPatch("RenderFrag")]
//		public static void RenderFrag_Patch(Wnd_AnimalToHuman __instance, Npc ___npc, List<HumanoidEvolutionMgr.ThinkFrag> ___thinks, int index, GObject item)
//		{
//			UI_A2HThink uiElement = item as UI_A2HThink;
//			HumanoidEvolutionMgr.ThinkFrag thinkFrag = ___thinks[(int)uiElement.data];
//			//var isLocked = ___thinks.Any(x => x.Fra
//			KLog.Dbg("A2H  " + thinkFrag.DescFormat + " Frags: " + string.Join(",", thinkFrag.frags.ToArray()));
//			//uiElement.text
//			//SNoSelectFrag sNoSelectFrag = noselectfrag[index];
//			//int idx = sNoSelectFrag.idx;
//			//HumanoidEvolutionMgr.ThinkFrag thinkFrag = thinks[idx];

//			//uI_A2HThink.m_desc.color = ___thinks.Any(x => x.fra
//		}

//		[HarmonyPostfix]
//		[HarmonyPatch("ClickLock")]
//		public static void ClickLock_Patch(Wnd_AnimalToHuman __instance, List<HumanoidEvolutionMgr.ThinkFrag> ___thinks, EventContext context)
//		{
//			GObject gObject = context.sender as GObject;
//			int index = (int)gObject.data;
//			UI_A2HThink uI_A2HThink = gObject.parent as UI_A2HThink;
//		}
//	}
//}
