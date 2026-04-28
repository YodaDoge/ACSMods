using FairyGUI;
using HarmonyLib;
using KTV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class ImmortalSaveLoad : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("ImmortalSaveLoad", "Immortal Save/Load", false);

		public ImmortalSaveLoad(bool defaultEnabled) : base(defaultEnabled)
		{
		}

		[HarmonyPatch]
		public static class Patch
		{
			private static PopupMenu MainMenu;

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_GameMain), "OnInit")]
			public static void AddSaveLoadQuitButtons(Wnd_GameMain __instance, PopupMenu ___MainMenu)
			{
				MainMenu = ___MainMenu;
				if (_info.Enabled)
					AddSaveLoadQuitButtons();	
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(GameWatch), "GetAutoSaveTime")]
			public static void UseAutoSaveTimeInImmortal(GameWatch __instance, ref float __result)
			{
				if (_info.Enabled)
					__result = GlobleDataMgr.Instance.GetFloat("AutoSave", 20f);
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_Setting), "OnShowUpdate")]
			public static void EnableAutoSaveSetting(Wnd_Setting __instance, UI_WindowSetting ___UIInfo, params object[] objs)
			{
				___UIInfo.m_save.enabled = true;
			}

			private static List<GButton> _btn = new List<GButton>();

			private static void AddSaveLoadQuitButtons()
			{
				if (World.Instance.GameMode == g_emGameMode.HardCore)
				{
					//save
					_btn.Add(MainMenu.AddItem(TFMgr.Get("存档"), (EventCallback0)delegate
					{
						Wnd_Save.Instance.ShowSaveWnd(0);
					}));
					//load
					_btn.Add(MainMenu.AddItem(TFMgr.Get("读档"), (EventCallback0)delegate
					{
						Wnd_Save.Instance.ShowSaveWnd(1);
					}));
					//quit w/o saving
					_btn.Add(MainMenu.AddItem(TFMgr.Get("退出游戏"), (EventCallback0)delegate
					{
						Wnd_Message.Show(TFMgr.Get("退出将会丢失所有未保存的进度，确定要退出吗？"), 2, delegate (string r)
						{
							if (r == "1")
							{
								GameWatch.Instance.TrueQuit = true; //without this it will still autosave due to quitapplication routine
								GameWatch.QuitApplication();
							}
						}, modal: true, null, 0, 0, string.Empty);
					}));
				}
			}
		}
	}
}