using ACS_Yoda_Tweaks;
using FairyGUI;
using Light2D.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using UnityEngine;
using XiaWorld;
using XiaWorld.ThingStep;
using XiaWorld.UI.InGame;

public class MiracleMasterPanel //SearchMainPanel
{
	public UI_SearchMain UIInfo;
	private static Dictionary<string, string[]> MiracleCategories = new Dictionary<string, string[]>()
	{
		{ "Remolding", new string[] { "Healing_Charisma", "Healing_Intelligence", "Healing_Perception", "Healing_Physique" } },
		{ "Refinement", new string[] { "SoulCrystalYouPowerUp", "JieHuiDaoPowerUp", "SoulCrystalLingPowerUp", "LingStoneMake", "LingCrystalMake" } },
		{ "Weather", new string[] { "PrayForSun", "PrayForRain", "PrayForDustStorm", "PrayForLightningStorm", "PrayForFohn", "PrayForMiasma", "PrayForRainstorm", "PrayForDustStorm", "PrayForDustStorm" } },
		{ "Enemy", new string[] { "SeachSoul", "MakeSoulCrystal", "XinYinJue", "LingXiZhou", "WuYinKeShen" } },
		{ _otherSpells,  new string[0] },
	};
	public bool isShowing = false;
	private const string _otherSpells = "Other";
	private HashSet<string> CategorizedSpells;
	string selectedCategory = null;

	public MiracleMasterPanel(UI_SearchMain searchMain)
	{
		UIInfo = searchMain;
		UIInfo.m_n162.m_n5.onClick.Add(Hide);
		UIInfo.RemoveChild(UIInfo.m_n191); //elements
		UIInfo.RemoveChild(UIInfo.m_n170);//????
		UIInfo.m_n175.text = "Miracles (Yoda Mod)";
		UIInfo.m_n175.width += 80;
		UIInfo.RemoveChild(UIInfo.m_search); //searchbutton

		UIInfo.m_F.x -= 30;
		UIInfo.m_F.width += 40;

		UIInfo.m_F.m_title.onChanged.Add((EventCallback0)delegate
		{
			UpdateItemList();
		});
		var addHeight = 40 + UIInfo.m_n169.height;

		UIInfo.m_n169.height = UIInfo.m_n190.height = UIInfo.m_n163.height = UIInfo.m_n164.height = addHeight;
		UIInfo.m_n183.visible = false; //SelectItemRenderer;
		CategorizedSpells = new HashSet<string>(MiracleCategories.SelectMany(x => x.Value));
	}

	public void Show()
	{
		isShowing = UIInfo.visible = true;
		UIInfo.scale = Wnd_GameMain.Instance.SearchMainPanel.UIInfo.scale;

		selectedCategory = null;
		UIInfo.m_F.m_title.text = string.Empty;
		var categoryList = UIInfo.m_n164;
		categoryList.RemoveChildren(true);
		foreach (var category in MiracleCategories)
		{
			var btn = categoryList.AddItemFromPool() as GButton;
			btn.data = category.Key;
			btn.title = category.Key;
			btn.onClick.Remove(Category_Clicked);
			btn.onClick.Add(Category_Clicked);
		}
		UpdateItemList();

		UIInfo.m_n169.y = UIInfo.m_n190.y = UIInfo.m_n163.y = categoryList.y = UIInfo.m_F.m_title.y + UIInfo.m_F.m_title.actualHeight + 25;

	}

	private void UpdateItemList()
	{
		var searchText = UIInfo.m_F.m_title.text;
		UIInfo.m_n163.RemoveChildrenToPool();

		foreach (var spellButton in GetSpells().Where(x => (string.IsNullOrEmpty(searchText) || x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
													&& (selectedCategory == null
														|| (selectedCategory == _otherSpells && !CategorizedSpells.Contains(x.MagicDef.Name))
														|| MiracleCategories[selectedCategory].Contains(x.MagicDef.Name)))
										.OrderBy(x => x.Name))
		{
			var btn = UIInfo.m_n163.AddItemFromPool() as UI_SearchBtn;
			if (btn is null)
				continue;

			btn.data = spellButton;
			//SearchMainPanel SetBntInfo(int index, UI_SearchBtn bnt, Thing thing, bool inselect = false)
			//Panel_ThingInfo.UpdateMagicBnt
			RenderItem(btn);
		}
	}

	private void RenderItem(UI_SearchBtn btn)
	{
		ApplyButtonDefaults(btn);
		SpellItemInfo spellButton = (SpellItemInfo)btn.data;
		btn.m_n172.data = spellButton;
		btn.m_n172.m_n176.text = spellButton.Name; //title

		if (spellButton.MagicDef.CostLing > 0)
		{
			btn.m_n172.m_Ling.value = spellButton.Npc.ZhenQi;
			btn.m_n172.m_Ling.max = spellButton.MagicDef.CostLing;
		}

		if (spellButton.DisabledReason != null)
		{
			btn.m_Status.selectedIndex = 1;
			btn.m_n199.m_n200.text = spellButton.DisabledReason;
		}

		//btn.enabled = ;
		btn.grayed = !spellButton.btn.IsEnable(spellButton.Npc, null);
		
		btn.m_n172.onClick.Clear();
		btn.m_n172.onClick.Add(x =>
		{
			var eventData = (SpellItemInfo)((UI_S_Btn)x.sender).data;
			
			if(!eventData.Npc.IsSelectByPlayer)
				UILogicMode_Select.Instance.SelectThing(eventData.Npc);
			else
				MapCamera.Instance.LookKey(eventData.Npc.Key);

			if (eventData.btn.IsEnable(spellButton.Npc, null) && !eventData.btn.IsActed(eventData.Npc, null))
				eventData.btn.DoAct(eventData.Npc, null);
		});

		btn.m_n172.m_icon.visible = true;
		btn.m_n172.m_icon.url = spellButton.Icon;
	}

	private void ApplyButtonDefaults(UI_SearchBtn bnt)
	{
		bnt.m_Select.selectedIndex = 0;
		bnt.m_Status.selectedIndex = 0;
		bnt.m_buffs.RemoveChildrenToPool();
		bnt.m_buffs.ResizeToFit(0);
		bnt.m_buffs.visible = false;
		bnt.m_n172.m_gongelement.visible = false;
		bnt.m_n172.m_n185.visible = false;
	}

	private void Category_Clicked(EventContext context)
	{
		GButton btn = context.sender as GButton;
		var val = (string)btn.data;
		selectedCategory = val;
		UpdateItemList();
	}

	//private static HashSet<string> Never = new HashSet<string>() { "ClosedDoor", "ClosedDoor_Gong4", 
	//			"ClosedDoor_Gong1", "FenShenZhiShu", "RebornSelf", "AbsorbGong_9_2", "AbsorbGong_9_1", "AbsorbGong_9_3" };

	protected class SpellItemInfo
	{
		public Npc Npc;
		public MagicDef MagicDef;
		public NpcMagicBnt btn;
		public string Name => MagicDef.DisplayName + " \n" + Npc.GetName();
		public string Icon => MagicDef.Icon;
		public string DisabledReason;
	}

	private static IEnumerable<SpellItemInfo> GetSpells()
	{
		var npcs = ThingMgr.Instance.NpcList.Where(npc => npc.IsRealPlayerThing && !npc.IsVistor && npc.IsMagicRank
														&& !npc.FightBody.IsFighting
														&& !npc.IsDeath
														&& npc.IsAlive
														&& !npc.IsGod
														&& npc.IsSelectAble
														&& npc.PropertyMgr.Practice.Magics.Count > 0 && !npc.IsRent);

		foreach (var npc in npcs)
		{
			foreach (var spell in npc.PropertyMgr.Practice.Magics)
			{
				var result = new SpellItemInfo();
				result.Npc = npc;
				result.MagicDef = PracticeMgr.Instance.GetMagicDef(spell);
				result.btn = NpcMagicBnt.GetBntData(spell, npc);

				if (npc.HasSpecialFlag(g_emNpcSpecailFlag.FLAG_THUNDERING))
					result.DisabledReason = "Thundering";
				else if (npc.HasSpecialFlag(g_emNpcSpecailFlag.MapExploring))
					result.DisabledReason = "Adventuring";
				yield return result;
			}
		}
	}

	public void Hide()
	{
		isShowing = UIInfo.visible = false;
	}
}