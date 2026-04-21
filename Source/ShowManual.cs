using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;
using static XiaWorld.JianghuMgr;

namespace ACS_Yoda_Tweaks
{
	public class ShowManual : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("ShowManual", "Flag NPC with Manual", false);

		private static bool _pauseAfterLoad = false;

		public ShowManual(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{

			[HarmonyPostfix]
			[HarmonyPatch(typeof(SearchMainPanel), "SetBntInfo")]
			public static void SetBntInfo(int index, UI_SearchBtn bnt, Thing thing, bool inselect = false)
			{
				if (!_info.Enabled)
					return;

				if (thing.ThingType != g_emThingType.Npc)
					return;
				try
				{
					if (thing is Npc npc)
					{
						if (npc.JiangHuSeed == 0 || (npc.IsPlayerThing && !npc.IsVistor) || npc.PropertyMgr?.Practice?.GongStateLevel < g_emGongStageLevel.Qi)
							return;


						var jnpc = JianghuMgr.Instance.GetKnowNpcData(npc.JiangHuSeed);
						
						if (jnpc == null || jnpc.KnowOther?.Contains(npc.JiangHuSeed + "_15") != true)
							bnt.m_n172.m_n176.text += " (秘籍)";

					}
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}



			}
		}
	}
}