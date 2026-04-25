using FairyGUI;
using HarmonyLib;
using KTV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using static XiaWorld.OutspreadMgr;

namespace ACS_Yoda_Tweaks
{

	//public class MessageIgnore : Mod
	//{
		//public override Meta Info => _info;
		//private static Meta _info = new Meta("MessageIgnore", "Agency Fixes", false);

		//private static bool _pauseAfterLoad = false;

		//public MessageIgnore(bool defaultEnabled) : base(defaultEnabled)
		//{

		//}
		//public static HashSet<int> MessagesToIgnore = new HashSet<int>();

		//[HarmonyPrefix]
		//[HarmonyPatch(typeof(MessageMgr), "AddMessage")]
		//public static bool FilterMessage(int msgid)
		//{
		//	return !MessagesToIgnore.Contains(msgid);
		//}
	//}
}