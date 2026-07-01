using FairyGUI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using UnityEngine;
using XiaWorld;
using XiaWorld.UI.InGame;

namespace ACS_Yoda_Tweaks
{
	public class BranchDropDown : Mod
	{
		public override Meta Info => _info;
		private static Meta _info = new Meta("BranchDropDown", "Rightclick like its 2005", true);

		private static bool _pauseAfterLoad = false;

		public BranchDropDown(bool defaultEnabled) : base(defaultEnabled)
		{
			//XiaWorld.UI.InGame.UI_Panel_ThingInfo
		}

		[HarmonyPatch]
		public static class Patch
		{
		}
	}
}