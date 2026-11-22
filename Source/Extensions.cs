using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XiaWorld;

namespace ACS_Yoda_Tweaks
{
	internal static class Extensions
	{
		public static bool IsAlchemyBuilding(this BuildingThing b)
		{
			return b.def.Name == "DanStove";
		}

	}
}
