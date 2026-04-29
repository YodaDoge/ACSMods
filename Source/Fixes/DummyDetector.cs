using ACS_Yoda_Tweaks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;
using static ACS_Yoda_Tweaks.Mod;

public class DummyDetector : Mod
{
	public override Meta Info => _info;
	private static Meta _info = new Meta("LookForDummy", "Training Dummy searchrange increase", true);

	public DummyDetector(bool defaultEnabled) : base(defaultEnabled)
	{
	}

	[HarmonyPatch]
	public static class Patch
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(JobBasePractice), "GetToilList")]
		public static bool Prefix(ref List<ToilBase> __result, JobBasePractice __instance, ref bool ___freepractice)
		{
			if (!_info.Enabled) return true;
			var npc = __instance.Worker;
			List<ToilBase> list = new List<ToilBase>();
			__result = list;
			var freepractice = ___freepractice;
			bool flag = false;
			
			if (npc.InBuilding == null || __instance.Worker.InBuilding.TagData.CheckTag("BasePractice") <= 0)
			{
				//AddLog($"{npc.ToString()} Searching");
				BuildingThing buildingThing = __instance.Worker.map.Things.FindBuilding(__instance.Worker, 200, "BasePractice", 0, needworkspace: true, issort: true);
				if (buildingThing != null)
				{
					flag = true;
					int num = buildingThing.CheckWorkSpace();
					list.Add(new ToilLockWorkSpace(buildingThing, num));
					list.Add(ToilGoto.GotoThing(buildingThing, g_emPathEndMode.Touch, num));
					list.Add(new ToilJump2Building(buildingThing, num));
				}
				//AddLog($"{npc.ToString()} found Dummy "+flag);
			}
			if (__instance.Worker.InBuilding != null && __instance.Worker.InBuilding.TagData.CheckTag("BasePractice") > 0)
			{
				flag = true;
			}
			if (!flag)
			{
				int num2 = 0;
				List<int> activityScope = __instance.Worker.map.GetActivityScope();
				num2 = activityScope[World.RandomRange(0, activityScope.Count)];
				if (__instance.Worker.CheckKeyStayOK(num2) && GridMgr.Inst.sqrDistance(num2, __instance.Worker.Key) <= 200 && __instance.Worker.map.CheckPath(__instance.Worker, num2) != 0)
				{
					list.Add(ToilGoto.GotoGrid(num2, g_emPathEndMode.OnPos));
				}
			}
			list.Add(new ToilBasePractice(freepractice));

			return false;
		}
	}
}
