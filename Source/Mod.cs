using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
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

		private const int LogLength = 5;
		private static Queue<string> Log = new Queue<string>(LogLength);

		public static void AddLog(string msg, params string[] fmt) => AddLog(string.Format(msg, fmt));
		public static void AddLog(string msg)
		{
			while (Log.Count >= LogLength)
				Log.Dequeue();
			Log.Enqueue(msg);
		}

		public static void ShowLog(string msg)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (Log.Count > 0)
			{
				stringBuilder.Append(Log.Dequeue());
			}
			stringBuilder.AppendLine("__________");
			stringBuilder.AppendLine(msg);
			ShowMessage(stringBuilder.ToString());
		}

		public static void ShowMessage(Exception ex)
		{
			ShowLog(ex.ToString());

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
