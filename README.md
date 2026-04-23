# Yoda's Tweaks and Fixes

Application restart required after enabling this mod! <br/>
Can be added/removed from ongoing saves.

## Fixes
* Spirit Animal now generates Leisure when performing any Playing actions or interactions with Disciples.
* Artifact Crafting message will not be removed when the Artifact is hauled
* Outers will now search for training Dummies more diligently. 
* Tier 9+ clothing will be ignored when mindful dessing is active.
* Ingame Copy function (see Hotkeys) will now place the selected building and material in your hand. 
* ESC -> Difficulty -> Environment; now shows the correct map seed
* Universe Storage now shows items after opening
* Universe Storage search is now case-insensitive

## Default Features
* **Handworking Station Priority** <br/> Handworking will now be performed at the same priority as Stonecutting while respecting the priority setting on the workbench. 
* **Smarter Dressers*** <br/> Mindful Dressers will seek out trinkets. 
<br/> Outers equip Handkerchiefs, Bells and Talismans such as SpiritTravel, Cleansing, Agility...
<br/> Xiandao will equip Scented Sachets and Dice
* **Flag NPC with Manual** <br/> In the Search Panel, NPC's which still carry a Friendship manual have (秘籍) added to their name
* **Smarter Bedroom Selection** <br/> Workers will prefer beds in rooms with multiple beds and vice versa. Rooms with a single bed will be named after their residents.
* **Reactive Disciples*** <br/> Leisure and cultivation activities will immediately abort when receiving an order. <br/>
Most tasks will be interrupted to perform a miracle or talk to an npc.
* **UI Improvements** 
<br/>- Many Windows have their Input fields automatically focused
<br/>- Searching is now case-insensitive (Manual pavillion, Universe, Search Panel) 
<br/>- Manual Pavillion and Mini-Universe will perform the search while typing 
<br/>- Items in Mini-Universe can be removed via shift+click (all), ctrl+click (10), alt+click (1)
<br/>- On Adventures with a single Disciple interactions will skip the character selection prompt
<br/>- Animal thoughts are now sorted by Type → Level → Name. Memorized shards use a bold font
* **(Beta) Meditation and Cultivation Tweaks** *<br/>
Meditation now counts for Spirit Root Sympathy. <br/>
Meditation is automatically cancelled upon dropping to 50 Mental State or less <br/>
Inners on a balanced cultivation plan maintain at least 50 mental state with fun activities.<br/>
They cultivate until they reach a bottleneck; afterwards, they switch to practicing for XP and Qi-regen. <br/> 
Meditation will automatically be used when available. <br/>
**Elysium Compatible**: This logic is naturally bypassed if the Disciple is managed by Elysium

*= Can be disabled in Mod settings

## Optional Features
Following Features must be enabled in the MLL menu (Esc => MLL)

* **Auto Pause on Load**
* **Guard Disabled for Mentors** 
* **One Click Interrogate Extended** <br/>
  Almost identical to the [original](https://steamcommunity.com/sharedfiles/filedetails/?id=2856326732&searchtext=Interrogate) Mod.
  <br/>Interrogation includes all NPCs known to the player.
* **Brighter Daylight** <br/>
  Days will be brighter, creating a stronger contrast between day and night.
* **Smelt after Transcribe** <br/>
  Manuals will be consumed on sucessful transcription.
* **Remove Map Fog** <br/>
  Disables Map fog on home and adventure Maps.
* **Map Wide Branch Area** <br/>
  Branch area bonus will be applied to their respective disciples at all time.<br/> One such area per branch must exist on the map for this to work.
* **Recruit without Priorities** <br/>
  Freshly accepted Disciples will have no work priorities and have hard working enabled.
* **Animal Autothink** <br/>
  You can define which thought shards an animal should use. <br/>
  It will automatically handle thinking, memorizing, studying and forming thoughts.

SourceCode and Readme: https://github.com/YodaDoge/ACSMods/

## Harmony warning 
If an outdated Harmony has been loaded you will see a warning dialog. <br/>
Using an outdated Harmony version can lead to loss of progress. <br/>
This mod is not the cause of this, it only warns you about the conflict. <br/>
Either disable the mod that provides an outdated harmony version, or delete the harmony file of the conflicting mod manually (not recommended). <br/>
Most commonly this is caused by [SkillLevelInRecruitment](https://steamcommunity.com/sharedfiles/filedetails/?id=2364620535) providing an outdated harmony version. <br/>
I recommend using [Iguanas Overhaul](https://github.com/iguanacore/iguana_acs_functions/) instead. <br/>
If the warning message opened this page for you, the steam-workshop link of the offending Mod has been copied to your clipboard <br/>
