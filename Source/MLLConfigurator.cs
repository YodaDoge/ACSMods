using FairyGUI;
using HarmonyLib;
using ModLoaderLite;
using ModLoaderLite.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XiaWorld;
using static ACS_Yoda_Tweaks.Mod;
using static CSCallLua;

namespace ACS_Yoda_Tweaks
{
	public class ACS_Yoda_Tweaks
	{
		//static EventCallback0 eventHandleConfig = new EventCallback0(HandleConfig);
		private static List<Meta> mods = new List<Meta>()
		{
			new SpiritAnimalPlayFix().Info,
			new ShowArtifactCraftingResult().Info,
			new HandworkPriority().Info,
			new LookForDummy().Info,
			new WorkerAutoEquip().Info,
			new MasterNoBreakGuard().Info,
			new AutoPause().Info,
			new OneClickInterrogate().Info,
			new AmbientLightMod().Info,
		};

		private const string ConfigName = "ACS_Yoda_Tweaks";

		public static void OnInit()
		{
			KLog.Dbg("OnInit YodaDoge Tweaks and Fixes");
			Configuration.Subscribe(ConfigUpdate);
			var usedHarmonyAssembly = typeof(Harmony).Assembly;
			KLog.Dbg($"Harmony version {usedHarmonyAssembly.GetName().Version} location: {usedHarmonyAssembly.Location}");
		}

		public static void OnLoad()
		{
			KLog.Dbg("OnLoad YodaDoge Tweaks and Fixes Harmony is using Version ");
			LoadConfig();
			WarnHarmonyConflict();
		}

		private static void WarnHarmonyConflict()
		{
			try
			{
				var directory = new DirectoryInfo(KLog.logFilePath);
				var newestFile = directory.GetFiles()
										  .OrderByDescending(f => f.LastWriteTime)
										  .FirstOrDefault();
				using (var fs = new FileStream(newestFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var reader = new StreamReader(fs))
				{
					string txt = reader.ReadToEnd();
					if (txt.Contains("Patching exception"))
					{

						//TODO: show a warning
					}
				}
			}
			catch (Exception ex)
			{
				KLog.Dbg(ex.ToString());
			}

		}

		public static void OnSave()
		{
			ConfigUpdate();
		}

		private static void ConfigUpdate()
		{
			KLog.Dbg("ConfigUpdate YodaDoge Tweaks and Fixes");
			foreach (var item in mods)
			{
				var checkState = Configuration.GetCheckBox(ConfigName, item.Name);
				KLog.Dbg($"Saved {item.Name} enabled {checkState}");
				item.Enabled = checkState;
			}
			MLLMain.AddOrOverWriteSave(ConfigName, mods.ToDictionary(key => key.Name, va => va.Enabled));
		}

		private static void LoadConfig()
		{
			Dictionary<string, bool> config = MLLMain.GetSaveOrDefault<Dictionary<string, bool>>(ConfigName) ?? new Dictionary<string, bool>();

			foreach (var mod in mods)
			{
				if (config.TryGetValue(mod.Name, out bool enabled))
				{
					KLog.Dbg($"Loaded {mod.Name}: {enabled}");
					mod.Enabled = enabled;
				}
				Configuration.AddCheckBox(ConfigName, mod.Name, mod.Description, mod.Enabled);
			}
		}


	}

	public abstract class Mod
	{
		public abstract Meta Info { get; }
		public class Meta
		{
			public string Name { get; set; }
			public string Description { get; set; }

			private bool _enabled;
			public bool Enabled
			{
				get => _enabled;
				set
				{
					var last = _enabled;
					_enabled = value;

					if (last != _enabled)
					{
						KLog.Dbg($"YodaDoge Tweaks: {Name} enabled {value}");
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
