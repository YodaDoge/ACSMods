using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	public abstract class Mod
	{
		public Mod(bool defaultEnabled)
		{
			Info.Enabled = defaultEnabled;
		}
		public static bool IsYodaMachine => ACS_Yoda_Tweaks.IsYodaMachine;
		public static bool OnSchoolMap => World.Instance.GameMode == g_emGameMode.Normal || World.Instance.GameMode == g_emGameMode.HardCore;

		public static void ShowMessage(string message, string title = null) => ACS_Yoda_Tweaks.ShowMessage(message);

		private const int LogLength = 20;
		private static Queue<string> Log = new Queue<string>();

		public static void AddLog(string msg, params object[] fmt) => AddLog(string.Format(msg, fmt));
		public static void AddLog(string msg)
		{
			while (Log.Count >= LogLength)
				Log.Dequeue();
			Log.Enqueue(msg);
		}

		public static void ShowLog(string msg = "")
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var item in Log.Reverse())
			{
				stringBuilder.AppendLine(item);
			}

			if (!string.IsNullOrEmpty(msg))
			{
				stringBuilder.AppendLine("__________");
				stringBuilder.AppendLine(msg);
			}
			ShowMessage(stringBuilder.ToString());
		}
		private static HashSet<int> _once = new HashSet<int>();

		protected static void Once(string s)
		{
			Once(() => ShowMessage(s), s.GetHashCode());
		}

		protected static bool Once(int key)
		{
			if (!_once.Contains(key))
			{
				_once.Add(key);
				return true;
			}
			return false;
		}

		protected static void Once(Action value, int key = 55)
		{
			int val = key;// ?? value.GetHashCode();
			if (!_once.Contains(val))
			{
				_once.Add(val);
				value.Invoke();
			}
		}

		public virtual void OnSave()
		{
		}

		public virtual void OnLoad()
		{
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
