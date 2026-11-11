using ACS_Yoda_Tweaks;
using FairyGUI;
using HarmonyLib;
using Light2D.Examples;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiaWorld;
using XiaWorld.Fight;
using static ACS_Yoda_Tweaks.Mod;


public class NoClickSocial : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("NoClickSocial", "No click Favor, Interrogation and drop manual button", false);

	public NoClickSocial(bool defaultEnabled) : base(defaultEnabled)
	{
		if (IsYodaMachine)
			_info.Enabled = true;
		AddLog("Set to enabled " + IsYodaMachine);
	}

	[HarmonyPatch]
	public static class Patch
	{
		private static Npc _target;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Wnd_JianghuTalk), "Talk")]
		public static void Postfix(Npc leftnpc, Npc rightnpc)
		{
			if (!_info.Enabled)
				return;
			try
			{
				_target = leftnpc;
				JianghuMgr.Instance.KnowNpcAllData(leftnpc);
				var targetdata = JianghuMgr.Instance.GetKnowNpcData(leftnpc.JiangHuSeed);
				targetdata.favour = 100f;
				targetdata.UnlockHeart();

				foreach (KeyValuePair<int, JianghuMgr.JHNpcData> knowNpcDatum in JianghuMgr.Instance.KnowNpcData)
				{
					JianghuMgr.Instance.KnowNpcAllData(knowNpcDatum.Key);
				}
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}

		}

		public static void DropManual(Npc target)
		{
			JianghuNpcDef targetdef = JianghuMgr.Instance.GetJHNpcDataBySeed(_target.JiangHuSeed);
			if (targetdef.Esoterica != null)
			{
				ItemThing itemThing = null;
				if (!string.IsNullOrEmpty(targetdef.Esoterica.Name))
				{
					itemThing = ThingMgr.Instance.AddSysEsotericaItem(0, targetdef.Esoterica.Name, null);
				}
				else if (targetdef.Esoterica.Type != 0)
				{
					JianghuNpcSeed jianghuNpcSeed = JianghuMgr.JHNpcLocaltionSeed2Data(target.JiangHuSeed);
					itemThing = ThingMgr.Instance.AddEsotericaItemCustomize(0, null, target.PropertyMgr.SuffixName, target.PropertyMgr.Practice.Gong.ElementKind, targetdef.Esoterica.Type, (float)jianghuNpcSeed.l * 0.25f, 1, (g_emGongStageLevel)jianghuNpcSeed.l);
				}

				if (itemThing != null)
				{
					itemThing.SetCamp(g_emFightCamp.Player);
					World.Instance.map.DropItem(itemThing, target.Key, beFound: true, needcarry: true, noself: true, needclick: true, 30f);
					string key = $"{_target.JiangHuSeed}_{(int)g_emJHNpcDataType.Esoterica}";
					JianghuMgr.Instance.GetKnowNpcData(target.JiangHuSeed).AddKnowKey(key);
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Wnd_JianghuTalk), "OnShowUpdate")]
		public static void ShowOrUpdate(Wnd_JianghuTalk __instance, object[] objs)
		{
			if (!_info.Enabled)
			{
				HideManualButton(__instance);
				return;
			}

			try
			{
				var npc = _target;
				if (npc.JiangHuSeed == 0 || (npc.IsPlayerThing && !npc.IsVistor) || npc.PropertyMgr?.Practice?.GongStateLevel < g_emGongStageLevel.Qi)
				{
					HideManualButton(__instance);
					return;
				}

				var jnpc = JianghuMgr.Instance.GetKnowNpcData(npc.JiangHuSeed);
				if (jnpc == null || jnpc.KnowOther?.Contains(npc.JiangHuSeed + "_15") != true)
				{
					ShowManualButton(__instance);
				}
				else
					HideManualButton(__instance);
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}

		}

		private const string DropManualButton = "btnGivManual";
		private static void ShowManualButton(Wnd_JianghuTalk instance)
		{
			var btn = instance.UIInfo.GetChild(DropManualButton) as GButton;
			if (btn == null)
			{
				btn = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
				btn.name = DropManualButton;
				btn.title = btn.text = "DropManual";
				instance.UIInfo.AddChild(btn);
				btn.onClick.Add(x =>
				{
					DropManual();
					btn.visible = false;
				});
			}
			btn.LeftBottom();
			btn.visible = true;
		}

		private static void DropManual() => DropManual(_target);
		private static void HideManualButton(Wnd_JianghuTalk instance)
		{
			var btn = instance.UIInfo.GetChild(DropManualButton) as GButton;
			if (btn == null)
				return;
			btn.visible = false;
		}
	}
}