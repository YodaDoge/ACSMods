using ACS_Yoda_Tweaks.AutoA2H;
using FairyGUI;
using HarmonyLib;
using ModLoaderLite;
using ModLoaderLite.Config;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;
using static ACS_Yoda_Tweaks.Mod;
using static CSCallLua;
using static System.Net.WebRequestMethods;

namespace ACS_Yoda_Tweaks
{
	public class ACS_Yoda_Tweaks
	{
		private static List<Meta> mods;

		private static List<Meta> GetDefaults() => new List<Meta>()
		{
			//Pure fixes 
			//new SpiritAnimalPlayFix().Info,
			//new ShowArtifactCraftingResult().Info, 
			//new LookForDummy().Info,
			
			//QoL
			//new A2H_SortOrder().Info, 
			//new HandworkPriority().Info,
			//new CopyBuildThing(true).Info,
			new ReactiveDisciples(true).Info,
			new MindfulDresser(true).Info,
			new CultivationTweaks(true).Info,


			//default off
			new MasterNoBreakGuard(false).Info,
			new AutoPause(false).Info,
			new OneClickInterrogate(false).Info,
			new AmbientLightMod(false).Info,
			new A2H(false).Info,
			new SmeltManual(false).Info,
			new FogRemover(false).Info,
			new Everywhere(false).Info,
			new EmptyPrio(false).Info,
			new ShowManual(false).Info,
			new PolicyEventTimer(false).Info,
		};

		private const string ConfigName = "ACS_Yoda_Tweaks";
		public static bool IsYodaMachine => Environment.MachineName == "YODADOGE";
		public static string ModName = "Yoda's Tweaks and Fixes";
		public static string RootWorkshopUrl = @"https://steamcommunity.com/sharedfiles/filedetails/?id=";
		public static string HarmonyConflictReadme = @"https://github.com/YodaDoge/ACSMods?tab=readme-ov-file#harmony-warning";

		public static void OnInit()
		{
			WarnIfHarmonyConflict();

		}

		public static void OnLoad()
		{
			//loading another save might reset the subscription. manual unsubscribe to avoid double subscribe
			Configuration.Unsubscribe(OnSave);
			Configuration.Subscribe(OnSave);

			mods = GetDefaults();
			LoadSavedConfig();
		}

		public static void ShowMessage(string text, string title = null)
		{
			var msg = Wnd_Message.Show(text, title: title ?? ModName, txt: text, bnt: 1, mode: 0);
		}

		private static void ShowConflictMessage(string location)
		{
			if (location == null)
				location = typeof(Harmony).Assembly.Location;

			var txtWarn = $"Outdated Harmony version\nOpen Readme?";

			var modId = Path.GetDirectoryName(location).Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
			var workShopURL = RootWorkshopUrl + modId;
			if (!int.TryParse(modId, out int result))
				workShopURL = location;

			var msg = Wnd_Message.Show("Prompt", title: ModName, txt: txtWarn, bnt: 2, mode: 0, act: x =>
			{
				if (x == "1")
				{
					Application.OpenURL(HarmonyConflictReadme);
					GUIUtility.systemCopyBuffer = workShopURL;
					MainManager.Instance.Pause();
				}
			});
		}

		private static void WarnIfHarmonyConflict()
		{
			try
			{
				var usedHarmonyAssembly = typeof(Harmony).Assembly.GetName();
				if (usedHarmonyAssembly.Version < new Version(2, 2, 1, 0))
				{
					var location = typeof(Harmony).Assembly.Location;
					ShowConflictMessage(location);
					KLog.Dbg("Outdated Harmony at " + location);
				}
			}
			catch (Exception ex)
			{
				KLog.Dbg(ex.ToString());
			}
		}

		public static void OnSave()
		{
			//this is an in memory save. Actually persistence only if user saves the game.
			foreach (var item in mods)
			{
				var checkState = Configuration.GetCheckBox(ConfigName, item.Name);
				KLog.Dbg($"Saved {item.Name} enabled {checkState}");
				item.Enabled = checkState;
			}
			MLLMain.AddOrOverWriteSave(ConfigName, mods.ToDictionary(key => key.Name, va => va.Enabled));

			bool v = MLLMain.AddOrOverWriteSave(A2H.Name, A2H.AutoNPC);

		}

		private static void LoadSavedConfig()
		{
			Dictionary<string, bool> config = MLLMain.GetSaveOrDefault<Dictionary<string, bool>>(ConfigName) ?? new Dictionary<string, bool>();

			foreach (var mod in mods)
			{
				if (config.TryGetValue(mod.Name, out bool enabled))
				{
					mod.Enabled = enabled;
				}
				else if(IsYodaMachine)
					mod.Enabled = true;

				//add does nothing if checkbox already exists
				Configuration.AddCheckBox(ConfigName, mod.Name, mod.Description, mod.Enabled);

				Configuration.SetCheckBox(ConfigName, mod.Name, mod.Enabled);
			}

			var a2h = MLLMain.GetSaveOrDefault<Dictionary<int, List<string>>>(AutoA2H.A2H.Name);
			A2H.InitNpcCache(a2h);
		}
	}
}
