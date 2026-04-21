using System;
using System.Text;
using UnityEngine;

namespace ACS_Yoda_Tweaks
{
	public abstract class Mod
	{
		public Mod(bool defaultEnabled)
		{
			Info.Enabled = defaultEnabled;
		}
		public static bool IsYodaMachine => ACS_Yoda_Tweaks.IsYodaMachine;

		public static void ShowMessage(string message, string title = null) => ACS_Yoda_Tweaks.ShowMessage(message);

		//private static StringBuilder Log = new StringBuilder();
		//public static void AddLog(string msg)
		//{
		//	if (Log.Length > 100)
		//		Log.Remove(0, Log.Length - 100);
		//}

		public static void ShowMessage(Exception ex)
		{
			ACS_Yoda_Tweaks.ShowMessage(ex.ToString());

			GUIUtility.systemCopyBuffer = ex.ToString();// Log.ToString();
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
