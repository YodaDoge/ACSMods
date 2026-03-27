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
			new WorkerAutoEquip(true).Info,

			//default off
			new MasterNoBreakGuard(false).Info,
			new AutoPause(false).Info,
			new OneClickInterrogate(false).Info,
			new AmbientLightMod(false).Info,
		};

		private const string ConfigName = "ACS_Yoda_Tweaks";

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

		private static void WarnIfHarmonyConflict()
		{
			try
			{
				var usedHarmonyAssembly = typeof(Harmony).Assembly.GetName();
				if (usedHarmonyAssembly.Version < new Version(2, 2, 1, 0))
				{
					var location = typeof(Harmony).Assembly.Location;
					GUIUtility.systemCopyBuffer = location.ToString();
					var txtWarn = $"Outdated Harmony version {usedHarmonyAssembly.Version} Filepath was copied to clipboard";
					KLog.Dbg(txtWarn + " " + location);
					var msg = Wnd_Message.Show("Prompt", title: "Harmony Conflict", txt: txtWarn, bnt: 1, mode: 0);
				}
			}
			catch (Exception ex)
			{
				KLog.Dbg(ex.ToString());
			}
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
		}
	}

	public abstract class Mod
	{
		public Mod(bool defaultEnabled)
		{
			Info.Enabled = defaultEnabled;
		}

		public abstract Meta Info { get; }
		public class Meta
		{
			public string Name { get; set; }
			public string Description { get; set; }

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
						string state = value ? "enabled" : "disabled";
						KLog.Dbg($"YodaDoge Tweak {Name} changed to {state}");
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
