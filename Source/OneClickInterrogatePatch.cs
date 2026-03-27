using ACS_Yoda_Tweaks;
using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using XiaWorld;
using static ACS_Yoda_Tweaks.Mod;


public class OneClickInterrogate : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("OneClickInterrogateUpgrade", "Interrogate about all known NPC", false);

	[HarmonyPatch]
	public static class Patch
	{
		[HarmonyPatch(typeof(Wnd_JianghuTalk), "OnInit")]
		public static void Postfix(ref Wnd_JianghuTalk __instance)
		{
			if (!_info.Enabled) return;

			try
			{
				Wnd_JianghuTalk talkWindow = __instance;
				if (talkWindow.UIInfo.GetChild("OneClick.Interrogate") == null)
				{
					GButton gButton = (GButton)UIPackage.CreateObjectFromURL("ui://ncbwb41mv9j6ah");
					gButton.name = "OneClick.Interrogate";
					gButton.title = "Interrogate";
					gButton.text = "Interrogate";
					talkWindow.UIInfo.AddChild(gButton);
					gButton.onClick.Add((EventCallback0)delegate
					{
						Npc npc = (Npc)Traverse.Create(talkWindow).Field("player").GetValue();
						Npc npc2 = (Npc)Traverse.Create(talkWindow).Field("target").GetValue();
						int num = (int)Traverse.Create(talkWindow).Field("targetseed").GetValue();
						int num2 = 0;
						foreach (KeyValuePair<int, JianghuMgr.JHNpcData> knowNpcDatum in JianghuMgr.Instance.KnowNpcData)
						{
							int key = knowNpcDatum.Key;
							if (key != num)
							{
								for (g_emJHNpcDataType g_emJHNpcDataType = g_emJHNpcDataType.Feature; g_emJHNpcDataType <= g_emJHNpcDataType.Hobby3; g_emJHNpcDataType++)
								{
									bool flag = JianghuMgr.Instance.IsKnowNpc(key, g_emJHNpcDataType);
									bool flag2 = JianghuMgr.Instance.CheckNpcKnowOther(num, key, g_emJHNpcDataType);
									if (!flag && flag2)
									{
										JianghuMgr.Instance.UnLockNpcDataKnow(key, g_emJHNpcDataType);
										num2++;
									}
								}
							}
						}
						string t = string.Format("{0}{1}{2}{3}{4}{5}", npc.GetName(), " interrogates ", npc2.GetName(), " about all of their fellow sect members and learned ", num2, " things.");
						talkWindow.SetTxt(t);
					});
				}
				else
				{
					GButton gButton = (GButton)talkWindow.UIInfo.GetChild("OneClick.Interrogate");
				}
			}
			catch (Exception ex)
			{
				KLog.Dbg("[OCI] error" + ex.ToString());
			}
		}
	}
}