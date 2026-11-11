using ACS_Yoda_Tweaks;
using FairyGUI;
using FairyGUI.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiaWorld;
using XiaWorld.Fight;
using XiaWorld.UI.InGame;
public partial class MiracleMaster : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("MiracleMaster", "Miracle Overview UI button bottom left", true);

	public MiracleMaster(bool defaultEnabled) : base(defaultEnabled)
	{
	}

	[HarmonyPatch]
	public static partial class Patch
	{
		private const string btnName = "BtnMiracleMaster";

		[HarmonyPostfix]
		[HarmonyPatch(typeof(UI_WindowGameMain), "ConstructFromXML")]
		public static void AddMiracleUI(UI_WindowGameMain __instance, XML xml)
		{
			try
			{
				var btn = UI_WndBnt_MenuBuild.CreateInstance();
				btn.name = btnName;
				__instance.AddChildAt(btn, __instance.GetChildIndex(__instance.m_producebnt) + 1); //determines layer position so its behind popups!
				btn.m_title.text = "Miracles";
				btn.icon = PracticeMgr.Instance.GetMagicDef("Healing_Damage").Icon;
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}
		}
		private static bool init = false;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
		public static void InitMiracleButton(Wnd_GameMain __instance)
		{
			try
			{
				var btn = (UI_WndBnt_MenuBuild)__instance.UIInfo.GetChild(btnName);
				btn.visible = true;// _info.Enabled;
			

				var refButton = __instance.UIInfo.m_producebnt;
				btn.x = refButton.position.x + refButton.actualWidth + 10;
				btn.y = refButton.position.y + 1;

				btn.m_icon.scale = __instance.SearchMainPanel.UIInfo.scale;
				btn.m_icon.x -= 5;
				btn.m_icon.y -= 2;
				
				var window = UI_SearchMain.CreateInstance();
				window.name = "MiracleWIndow";
				__instance.AddChild(window);
				window.visible = false;
				var searchPanelRef = __instance.UIInfo.m_n1501;
				window.position = searchPanelRef.position;
				window.height = searchPanelRef.height;
				window.width = searchPanelRef.width;

				miraclePanel = new MiracleMasterPanel(window);
				btn.visible = Mod.OnSchoolMap;
				btn.onClick.Clear();
				btn.onClick.Add(x => ToggleWindow(miraclePanel));
			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}
		}
		static MiracleMasterPanel miraclePanel;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(XiaWorld.UILogicMode_Select), "Esc_Global")]
		public static bool HideOnEsc()
		{
			if (miraclePanel.isShowing)
			{
				miraclePanel.Hide();
				return false;
			}
			return true;
		}

		private static void ToggleWindow(MiracleMasterPanel wndMain)
		{
			try
			{
				if (wndMain.isShowing)
					wndMain.Hide();
				else
					wndMain.Show();

			}
			catch (Exception ex)
			{
				ShowMessage(ex);
			}

		}
	}
}