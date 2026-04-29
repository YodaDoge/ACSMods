using FairyGUI;
using HarmonyLib;
using KTV;
using ModLoaderLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using XiaWorld;
using XiaWorld.LSBTree;
using XiaWorld.UI.InGame;
using static XiaWorld.MessageMgr;

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
			{42, "Back from adventure" },
			{22, "Unstable Mental State" },
			{54, "Unsuitable Grow Condition" },
			{21, "Mood deteriorating" },
		};

		public static void Save()
		{
			MLLMain.AddOrOverWriteSave(_info.Name, MessagesToIgnore);
		}

		public static void Load()
		{
			MessagesToIgnore = MLLMain.GetSaveOrDefault<HashSet<int>>(_info.Name) ?? new HashSet<int>();
		}

		[HarmonyPatch]
		public static class Patch
		{

			//[HarmonyPrefix]
			//[HarmonyPatch(typeof(MessageMgr), "AddMessage")]
			//public static bool FilterMessage(int msgid, List<Thing> things = null, string param = null, int brannum = -1, int targetkey = 0, int other = 0, string other2 = null, string tips = null, bool needUp = false)
			//{
			//	//ShowMessage("Added message " + msgid + " " + things?.FirstOrDefault()?.GetName());
			//	return !MessagesToIgnore?.Contains(msgid) != false;
			//}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_MessageBox), "OnRightClickItem")]
			public static void LogMsgId(Wnd_MessageBox __instance, EventContext context)
			{
				GObject gObject = context.sender as GObject;
				MessageMgr.MessageData messageData = gObject.data as MessageMgr.MessageData;
				MessageMgr.GetMessageInfo(messageData, out var attribute, out var _);
				//ShowMessage("Added message " + msgid + " " + things?.FirstOrDefault()?.GetName());
				AddLog($"{messageData._MessageID} things:{string.Join(" ", messageData._ThingID.Select(x => x.ToString()).ToArray())} rt:{attribute._ID} ob:{messageData._OnlyBox}");
			}


			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_MessageBox), "UpShow")]
			public static void FilterMessage(Wnd_MessageBox __instance)
			{
				try
				{
					//ShowMessage("Added message " + msgid + " " + things?.FirstOrDefault()?.GetName());
					List<MessageData> toRemove = new List<MessageData>();
					foreach (var item in MessageMgr.Instance.m_mapLevelMsg)
					{
						if (MessagesToIgnore?.Contains(item._AttributID) != true)
							continue;

						//AddLog($"UpShow "+ item._AttributID);
						MessageMgr.GetMessageInfo(item, out var attribute, out var _);
						item._OnlyBox = true; //to the shadow realm!
						if (attribute._RemoveType == MessageMgr.Message_RemoveType.Lookover)
						{
							toRemove.Add(item);
						}
					}
					toRemove.ForEach(MessageMgr.Instance.RemoveMessage);
				}
				catch (Exception ex)
				{
					ShowMessage(ex);
				}
			}

			static List<UI_item_msgshowevent> _customFilterUIElements = new List<UI_item_msgshowevent>();

			[HarmonyPostfix]
			[HarmonyPatch(typeof(Wnd_MsgShowConfig), "OnShowUpdate")]
			public static void AddCustomFilters(Wnd_MsgShowConfig __instance, UI_WindowMsgShowMgr ___UIInfo, params object[] objs)
			{
				try
				{
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
				Save();
			}
		}
	}
}