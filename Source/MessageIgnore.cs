using FairyGUI;
using HarmonyLib;
using KTV;
using ModLoaderLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class MessageIgnore : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("MessageIgnore", "Agency Fixes", false);

		private static bool _pauseAfterLoad = false;

		public MessageIgnore(bool defaultEnabled) : base(defaultEnabled)
		{

		}
		public static HashSet<int> MessagesToIgnore = null;

		public static Dictionary<int, string> Messages = new Dictionary<int, string>()
		{
			{42, "Back from adventure" }, //48 = old
			{22, "Unstable Mental State" },
			{54, "Unsuitable Grow Condition" },
			{21, "Mood deteriorating" },
		};

		public static void Save()
		{
			if (MessagesToIgnore != null)
				MLLMain.AddOrOverWriteSave(_info.Name, MessagesToIgnore);
		}

		public static HashSet<int> Load()
		{
			return MLLMain.GetSaveOrDefault<HashSet<int>>(_info.Name);
		}

		[HarmonyPatch]
		public static class Patch
		{
			[HarmonyPrefix]
			[HarmonyPatch(typeof(MessageMgr), "AddMessage")]
			public static bool FilterMessage(int msgid, List<Thing> things = null, string param = null, int brannum = -1, int targetkey = 0, int other = 0, string other2 = null, string tips = null, bool needUp = false)
			{
				//ShowMessage("Added message " + msgid + " " + things?.FirstOrDefault()?.GetName());
				return !MessagesToIgnore?.Contains(msgid) != false;
			}

			static List<UI_item_msgshowevent> _customFilterUIElements = new List<UI_item_msgshowevent>();


			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_MsgShowConfig), "OnShowUpdate")]
			public static void AddCustomFilters(Wnd_MsgShowConfig __instance, UI_WindowMsgShowMgr ___UIInfo, params object[] objs)
			{
				try
				{
					if (MessagesToIgnore == null)
						MessagesToIgnore = MLLMain.GetSaveOrDefault<HashSet<int>>(_info.Name) ?? new HashSet<int>();
					foreach (var item in Messages)
					{
						UI_item_msgshowevent uI_item_msgshowevent = ___UIInfo.m_n137.AddItemFromPool() as UI_item_msgshowevent;
						uI_item_msgshowevent.m_title.text = item.Value;
						//uI_item_msgshowevent.m_title.tooltips = sEventCongfig.Value.Desc;
						uI_item_msgshowevent.m_Select.selectedIndex = MessagesToIgnore.Contains(item.Key) ? 1 : 0;
						uI_item_msgshowevent.data = item.Key;
						_customFilterUIElements.Add(uI_item_msgshowevent);
					}
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}

			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(Wnd_MsgShowConfig), "OnHide")]
			public static void RemoveCustomFilterUI(Wnd_MsgShowConfig __instance, UI_WindowMsgShowMgr ___UIInfo)
			{
				foreach (var uI_item_msgshowevent in _customFilterUIElements)
				{
					int msgId = (int)uI_item_msgshowevent.data;
					if (uI_item_msgshowevent.m_Select.selectedIndex == 1)
						MessagesToIgnore.Add(msgId);
					else
						MessagesToIgnore.Remove(msgId);
					___UIInfo.m_n137.RemoveChild(uI_item_msgshowevent);
				}
				_customFilterUIElements.Clear();
				var isNew = !MLLMain.AddOrOverWriteSave(_info.Name, MessagesToIgnore);
			}
		}
	}
}