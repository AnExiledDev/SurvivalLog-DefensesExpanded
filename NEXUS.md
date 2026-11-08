Name: Defenses Expanded
Summary: Adds five new defenses to build: a Concrete Barrier, Barbed Wire, a Spike Plate, a powered Nail Gun Turret and a Tesla Coil. Each has its own model and icon and unlocks at a Defense skill level. You can change health, damage, range, timing, slowing, power, repairs, siege size, unlock level and materials, and one setting lets you remove it safely.

Description (BBCode):
[center][img]https://sldb.core-directive.com/media/save/3-remove-with-care.png[/img][/center]

Defenses Expanded adds five new defenses to build against the siege in Survival Log.

[size=5]Main features[/size]
[list]
[*][b]Concrete Barrier[/b]: a heavy wall of cast concrete. It blocks zombies like Sandbags, with 1500 health against their 600, and each Repair Parts puts back 300.[/*]
[*][b]Barbed Wire[/b]: zombies can push through it, but it cuts up to 6 at once for 1 damage every 2 seconds and slows them by 40% for 4 seconds.[/*]
[*][b]Spike Plate[/b]: laid flat on the ground. Zombies walk over it and take 3 damage every 2 seconds, up to 6 at once.[/*]
[*][b]Nail Gun Turret[/b]: needs power. It turns to face one zombie up to 4 m away and fires for 8 damage every 1.5 seconds, and stops below half health until it is repaired.[/*]
[*][b]Tesla Coil[/b]: needs power. Every 3 seconds it shocks up to 8 zombies within 3 m for 18 damage and slows them for 2 seconds.[/*]
[*][b]Zombies go for them[/b]: zombies attack them just like Sandbags and the rest, Repair Parts fix them, and they count toward the size of the next siege. Each has its own model and icon.[/*]
[*][b]Craft them[/b]: the recipes show at the workbench once you have any of the game's defense recipes, and unlock with your Defense skill: the Concrete Barrier, Barbed Wire and Spike Plate at level 2, the Nail Gun Turret at level 3 and the Tesla Coil at level 5. They take Scrap Metal, Wire, Iron Rod and Electronic Components.[/*]
[*][b]Set your own numbers[/b]: health, damage, timing, range, targets, slowing, power, repairs, siege size, unlock level and materials are all settings on the Mod Manager's Mods screen, and a change reaches the defenses already standing.[/*]
[*][b]Safe to remove[/b]: the game saves each defense as the game's defense it is based on, so a save still loads without the mod. See Uninstall below for the one step to take first.[/*]
[*][b]English and Chinese[/b]: every name and description comes in both.[/*]
[/list]

[size=5]Recipes[/size]
The recipes show at the workbench once you have any of the game's defense recipes. These are the defaults, and you can change both the materials and the level.
[list]
[*]Concrete Barrier: 4 Scrap Metal and 1 Iron Rod, Defense level 2.[/*]
[*]Barbed Wire: 3 Scrap Metal and 2 Wire, Defense level 2.[/*]
[*]Spike Plate: 2 Scrap Metal and 1 Iron Rod, Defense level 2.[/*]
[*]Nail Gun Turret: 1 Electronic Components, 1 Iron Rod and 3 Scrap Metal, Defense level 3.[/*]
[*]Tesla Coil: 2 Electronic Components, 3 Wire and 2 Scrap Metal, Defense level 5.[/*]
[/list]

[size=5]Known issues[/size]
The new defenses can't be dyed. Each one uses the sounds of the game's defense it is based on.

[center][img]https://sldb.core-directive.com/img/mods-banner.jpg[/img]
[size=4][url=https://www.nexusmods.com/survivallog/mods/73]Get Mod Manager[/url] to set up all your mods in game  |  [url=https://sldb.core-directive.com/en/]Open the Survival Log wiki[/url] for items, recipes and maps[/size][/center]

[size=5]Requirements[/size]
[list]
[*]Survival Log 1.1.20719. A game update can break it until the mod is updated.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]. Install it first.[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.19.0 or newer. Defenses Expanded does not load without it. It lets settings changed on the Mod Manager's Mods screen take effect without restarting the game.[/*]
[/list]

[size=5]Installation[/size]
[list=1]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i](if no other mods were installed before, start the game once so BepInEx finishes setup, then quit)[/i].[/*]
[*]Install [url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] the same way: download its zip and extract it into the game folder.[/*]
[*]Download the zip from the Files tab.[/*]
[*]Extract it into the game folder (the folder with [font=Courier New]SurvivalLog.exe[/font]). The zip holds a BepInEx folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\DefensesExpanded.dll[/code][/*]
[/list]

[size=5]Uninstall[/size]
A defense package in a bag or in storage would be lost without the mod, so first turn on [font=Courier New]UninstallPrep[/font] and load your save: every defense package becomes the package of the game's defense it is based on. Save and quit, then delete [font=Courier New]DefensesExpanded.dll[/font] from the [font=Courier New]BepInEx\plugins[/font] folder. Do this for each save you want to keep.

[size=5]Configuration[/size]
To change settings, edit [font=Courier New]BepInEx\config\com.dev.survivallog.defensesexpanded.cfg[/font] while the game is closed, or do it in game on the Mod Manager's Mods screen (see [url=https://github.com/AnExiledDev/SurvivalLog-DefensesExpanded/blob/main/CONFIG.md]CONFIG.md[/url] for every setting). Every setting takes effect at once.

[size=5]Feedback[/size]
Found a bug or have an idea? Come say hi on the [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url], or post in this page's Posts tab. For bugs, please include [font=Courier New]BepInEx\LogOutput.log[/font].
To reach me directly, message [b]exiled.dev[/b] on Discord, you can find me in the [url=https://discord.gg/MZGg4gQAUe]official Survival Log Discord[/url]. I'm also on X as [url=https://x.com/AnExiledDev]@AnExiledDev[/url].

[size=5]Source and license[/size]
Open source under MIT: [url=https://github.com/AnExiledDev/SurvivalLog-DefensesExpanded]github.com/AnExiledDev/SurvivalLog-DefensesExpanded[/url]

[line]
[size=5]简体中文[/size]

防御扩展为《Survival Log》新增五种抵御围攻的防御设施。

[size=5]主要功能[/size]
[list]
[*][b]混凝土路障[/b]：浇筑混凝土制成的厚重路障。和沙包一样挡住丧尸，但耐久 1500（沙包 600），每个维修零件修复 300。[/*]
[*][b]铁丝网[/b]：丧尸能挤过去，但它每 2 秒割伤范围内最多 6 只丧尸，造成 1 点伤害，并让它们减速 40%，持续 4 秒。[/*]
[*][b]钉板[/b]：平铺在地上。丧尸走过时每 2 秒受到 3 点伤害，一次最多 6 只。[/*]
[*][b]射钉炮塔[/b]：需要供电。会转向 4 米内的一只丧尸，每 1.5 秒射击一次，造成 8 点伤害；耐久低于一半时停止工作，修好后恢复。[/*]
[*][b]特斯拉线圈[/b]：需要供电。每 3 秒电击 3 米内最多 8 只丧尸，造成 18 点伤害并让它们减速 2 秒。[/*]
[*][b]丧尸会攻击它们[/b]：和沙包等防御设施一样，丧尸会攻击它们，维修零件可以修复，也会计入下一次围攻的规模。每种都有自己的模型和图标。[/*]
[*][b]可以制作[/b]：学会任意一个游戏自带的防御设施配方后，配方就会出现在工作台上，并随防御技能等级解锁：混凝土路障、铁丝网和钉板 2 级，射钉炮塔 3 级，特斯拉线圈 5 级。材料是铁皮、铁丝、铁棍和电器元件。[/*]
[*][b]数值随你设置[/b]：耐久、伤害、间隔、范围、目标数、减速、耗电、维修、围攻规模、解锁等级和材料都可以在 Mod Manager 的「模组」界面里设置，改动对已经摆好的防御设施也生效。[/*]
[*][b]可以放心移除[/b]：游戏会把每种防御设施存成它所依据的游戏防御设施，所以移除本模组后存档仍能读取。移除前要先做一步，见下方「卸载」。[/*]
[*][b]中英双语[/b]：所有名称和说明都有中文和英文。[/*]
[/list]

[size=5]配方[/size]
学会任意一个游戏自带的防御设施配方后，配方就会出现在工作台上。以下是默认值，材料和等级都可以修改。
[list]
[*]混凝土路障：4 个铁皮和 1 根铁棍，防御 2 级。[/*]
[*]铁丝网：3 个铁皮和 2 卷铁丝，防御 2 级。[/*]
[*]钉板：2 个铁皮和 1 根铁棍，防御 2 级。[/*]
[*]射钉炮塔：1 个电器元件、1 根铁棍和 3 个铁皮，防御 3 级。[/*]
[*]特斯拉线圈：2 个电器元件、3 卷铁丝和 2 个铁皮，防御 5 级。[/*]
[/list]

[size=5]已知问题[/size]
新防御设施不能染色，每种都使用所依据的游戏防御设施的音效。

[url=https://www.nexusmods.com/survivallog/mods/73]获取 Mod Manager[/url]，在游戏里设置所有模组。[url=https://sldb.core-directive.com/zh/]打开 Survival Log 维基[/url]，物品、配方、地图一查就有。

[size=5]前置需求[/size]
[list]
[*]Survival Log 1.1.20719。游戏更新后，在模组更新前可能无法使用。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/12]BepInEx for Survival Log[/url]，请先安装。[/*]
[*][url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url] 0.19.0 或更新版本。没有它本模组不会加载。它让在 Mod Manager 的「模组」界面改动的设置无需重启游戏即可生效。[/*]
[/list]

[size=5]安装[/size]
[list=1]
[*]安装 [url=https://www.nexusmods.com/survivallog/mods/12]BepInEx[/url] [i]（如果之前没有装过其他模组，先启动一次游戏，让 BepInEx 完成初始化，然后退出）[/i]。[/*]
[*]用同样的方式安装 [url=https://www.nexusmods.com/survivallog/mods/61]Exiled's Toolkit[/url]：下载它的压缩包并解压到游戏目录。[/*]
[*]在「文件」页下载压缩包。[/*]
[*]把压缩包解压到游戏目录（[font=Courier New]SurvivalLog.exe[/font] 所在的文件夹）。压缩包里带有 BepInEx 文件夹，DLL 会自动放到正确位置。如果提示合并文件夹，选择合并。解压后应为：
[code]C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\DefensesExpanded.dll[/code][/*]
[/list]

[size=5]卸载[/size]
没有本模组时，背包和储物里的防御设施包裹会丢失，所以请先打开 [font=Courier New]UninstallPrep[/font] 并读取存档：所有防御设施包裹都会变成它所依据的游戏防御设施的包裹。存档并退出，再从 [font=Courier New]BepInEx\plugins[/font] 文件夹中删除 [font=Courier New]DefensesExpanded.dll[/font]。每个想保留的存档都要这样做一次。

[size=5]配置[/size]
可以在游戏内 Mod Manager 的「模组」界面修改，也可以在游戏关闭时编辑 [font=Courier New]BepInEx\config\com.dev.survivallog.defensesexpanded.cfg[/font]。配置文件中每项设置都有中文说明，完整说明见 [url=https://github.com/AnExiledDev/SurvivalLog-DefensesExpanded/blob/main/CONFIG.md]CONFIG.md[/url]（英文）。所有设置都即时生效。

[size=5]反馈[/size]
发现 bug 或有好点子？欢迎来 [url=https://discord.gg/trTsyxUFqg]Core Directive Discord[/url] 打个招呼，或在本页的「Posts」标签里发帖。报告 bug 时，请附上 [font=Courier New]BepInEx\LogOutput.log[/font]。
想直接联系我，可以在 Discord 上给 [b]exiled.dev[/b] 发消息，我在 [url=https://discord.gg/MZGg4gQAUe]Survival Log 官方 Discord[/url] 里。也可以在 X 上找我：[url=https://x.com/AnExiledDev]@AnExiledDev[/url]。

[size=5]源代码与许可[/size]
基于 MIT 许可开源：[url=https://github.com/AnExiledDev/SurvivalLog-DefensesExpanded]github.com/AnExiledDev/SurvivalLog-DefensesExpanded[/url]
