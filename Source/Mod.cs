using System;

namespace ACS_Yoda_Tweaks
{
	public abstract class Mod
	{
		public Mod(bool defaultEnabled)
		{
			Info.Enabled = defaultEnabled;
		}
		protected static bool IsYodaMachine => ACS_Yoda_Tweaks.IsYodaMachine;

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
							OnEnableChanged?.Invoke(this);
						}
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
