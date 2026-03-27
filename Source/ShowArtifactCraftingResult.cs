using ACS_Yoda_Tweaks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using XiaWorld;
using XiaWorld.UI.InGame;
using static ACS_Yoda_Tweaks.Mod;
using static XiaWorld.MessageMgr;


public class ShowArtifactCraftingResult : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("ArtifactCraftingResultMessageFix", "Fix Artifact crafting message", true);

	[HarmonyPatch]
	public static class Patch
	{
		private static HashSet<int> ArtifactCraftingMessageIds = new HashSet<int>
				{
					38, //success 
					68 //godSuccess
				};

		[HarmonyPrefix]
		[HarmonyPatch(typeof(MessageMgr), "AddMessage")]
		public static bool AddMessage(int msgid, List<Thing> things = null, string param = null, int brannum = -1, int targetkey = 0, int other = 0, string other2 = null, string tips = null, bool needUp = false)
		{
			if (!_info.Enabled) return true;

			bool isBrokenArtifactCraftingMessage = ArtifactCraftingMessageIds.Contains(msgid) && targetkey != 0 && things != null && things.Any() && things.FirstOrDefault() is Npc maker;
			if (!isBrokenArtifactCraftingMessage)
				return true;
			maker = (Npc)things.FirstOrDefault();
			var artifact = maker.map.Things.GetThingsAtGrid(targetkey).Where(x => x.GetName() == param).FirstOrDefault();

			MessageMgr.Instance.AddMessage(msgid, things, param, -1, 0, artifact?.ID ?? other, other2, tips, true); // the 0 is important. without this override will be called again and we cant tell we already did our work
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(MessageMgr), "GetMessageAttribute")]
		public static void ChangeMsgType(MessageAttribute __result, int id)
		{
			if (!_info.Enabled) return;

			if (ArtifactCraftingMessageIds.Contains(id))
			{
				__result._RemoveType = Message_RemoveType.Lookover;
				KLog.Dbg($"YodaDoge: changed crafting message type");
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Wnd_MessageBox), "LookAtMsgThings")]
		public static bool AddMessage(UI_MessageItem _MessageItem)
		{
			if (!_info.Enabled) return true;

			if (!(_MessageItem.data is MessageMgr.MessageData messageData))
			{
				return true;
			}

			MessageMgr.GetMessageInfo(messageData, out var attribute, out var _);

			if (ArtifactCraftingMessageIds.Contains(messageData._MessageID))
				return true;

			var thing2 = ThingMgr.Instance.FindThingByID(messageData._Other) as ItemThing;
			if (thing2 != null)
			{
				int locationID = thing2.ID;
				if (thing2.InWhoseBag > 0)
					locationID = thing2.InWhoseBag;
				if (thing2.InWhoseHand > 0)
					locationID = thing2.InWhoseHand;

				if (UILogicMgr.Instance.GetCurMode().Mode != g_emUILogicMode.IndividualCommand)
				{
					UILogicMgr.Instance.BasicMode.SelectThing(thing2);
				}
				MapCamera.Instance.LookKey(thing2.Key);
				return false;
			}
			return true;
		}
	}
}
