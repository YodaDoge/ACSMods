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
using ACS_Yoda_Tweaks.AutoA2H;
using rail;

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
			new WorkerAutoEquip(true).Info,

			//default off
			new MasterNoBreakGuard(false).Info,
			new AutoPause(false).Info,
			new OneClickInterrogate(false).Info,
			new AmbientLightMod(false).Info,
		};

		private const string ConfigName = "ACS_Yoda_Tweaks";
		private const string ModName = "Yoda's Tweaks and Fixes";

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

		private static void ShowConflictMessage()
		{
			var location = typeof(Harmony).Assembly.Location;

			var txtWarn = $"Outdated Harmony version\nOpen Readme?";

			var modId = Path.GetDirectoryName(location).Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
			var workShopURL = RootWorkshopUrl + modId;
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

		public static string RootWorkshopUrl = @"https://steamcommunity.com/sharedfiles/filedetails/?id=";
		public static string HarmonyConflictReadme = @"https://github.com/YodaDoge/ACSMods?tab=readme-ov-file#harmony-warning";

		private static void WarnIfHarmonyConflict()
		{
			try
			{
				var usedHarmonyAssembly = typeof(Harmony).Assembly.GetName();
				if (usedHarmonyAssembly.Version < new Version(2, 2, 1, 0))
				{
					var location = typeof(Harmony).Assembly.Location;
					ShowConflictMessage();
					KLog.Dbg("Outdated Harmony at " + location);
				}
			}
			catch (Exception ex)
			{
				KLog.Dbg(ex.ToString());
			}
		}

		public static void ShowMessage(string text)
		{
			var msg = Wnd_Message.Show(text, title: "Harmony Conflict", txt: text, bnt: 1, mode: 0);
		}

		public static void OnSave()
		{
			//this is an in memory save. Actually persistance only if user saves the game.
			foreach (var item in mods)
			{
				var checkState = Configuration.GetCheckBox(ConfigName, item.Name);
				KLog.Dbg($"Saved {item.Name} enabled {checkState}");
				item.Enabled = checkState;
			}

			MLLMain.AddOrOverWriteSave(ConfigName, mods.ToDictionary(key => key.Name, va => va.Enabled));

			bool v = MLLMain.AddOrOverWriteSave(A2H.Name, A2H.AutoNPC);
			foreach (var item in A2H.AutoNPC)
			{
				KLog.Dbg("saved a2h " + item.Key + " vals: " + string.Join(",", item.Value.ToArray()));
			}

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

				//add does nothing if checkbox already exists
				Configuration.AddCheckBox(ConfigName, mod.Name, mod.Description, mod.Enabled);

				Configuration.SetCheckBox(ConfigName, mod.Name, mod.Enabled);
			}
			var a2h = MLLMain.GetSaveOrDefault<Dictionary<int, List<string>>>(AutoA2H.A2H.Name);
			if (a2h != null)
			{
				foreach (var item in a2h)
				{
					KLog.Dbg("loaded a2h " + item.Key + " vals: " + string.Join(",", item.Value.ToArray()));
				}
			}
			A2H.InitNpcCache(a2h);
		}
		protected static bool IsYodaMachine => Environment.MachineName == "YODADOGE";
	}

	public abstract class Mod
	{
		public Mod(bool defaultEnabled)
		{
			Info.Enabled = defaultEnabled;
		}

		protected static bool IsYodaMachine => A2H.IsYodaMachine;

		public static void ShowMessage(string message) => ACS_Yoda_Tweaks.ShowMessage(message);

		public abstract Meta Info { get; }
		public class Meta
		{
			public string Name { get; set; }
			public string Description { get; set; }

			public static bool LogStateChange = false;

			protected bool _enabled;
			public bool Enabled
			{
				get => _enabled;
				set
				{
					var last = _enabled;
					_enabled = value;

					if (last != _enabled)
					{
						if (LogStateChange)
						{
							string state = value ? "enabled" : "disabled";
							KLog.Dbg($"YodaDoge Tweak {Name} changed to {state}");
						}
						OnEnableChanged?.Invoke(this);
					}
				}
			}

			Action<Meta> OnEnableChanged;

			public Meta(string name, string description, bool enabled, Action<Meta> enableToggled)
				: this(name, description, enabled)
			{
				OnEnableChanged = enableToggled;
			}

			public Meta(string name, string description, bool enabled)
			{
				Name = name;
				Description = description;
				Enabled = enabled;
			}
		}
	}
}
