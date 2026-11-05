using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using DefensesExpanded.Adapters;
using DefensesExpanded.Core;
using HarmonyLib;

namespace DefensesExpanded;

[BepInPlugin(Guid, "Defenses Expanded", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.defensesexpanded";
    private const string Version = "0.1.1";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var uninstallPrep = Config.Bind("Uninstall", "UninstallPrep", false,
            "Turn this on before you remove Defenses Expanded. Once a save is loaded, every Concrete Barrier, Barbed Wire, Spike Plate, Nail Gun Turret or Tesla Coil Package becomes the package of the game's defense it is based on (Sandbags, Spike Barrier, Mechanical Chainsaw or Electric Trap), in your bags, in storage and on the floor, in every place the save holds. Placed and packed defenses are always saved as those game defenses, so they need nothing. Then save the game, quit, and remove DefensesExpanded.dll. LogOutput.log lists what was changed. Turning this off again does not turn the packages back.\n" +
            "移除防御扩展模组之前请先打开此项。读取存档后，背包、储物和地上的每个混凝土路障、铁丝网、钉板、射钉炮塔或特斯拉线圈包裹都会变成它所仿照的原版防御设施（沙包、尖刺障碍、机械电锯或电网）的包裹，存档里的每个地点都一样。已安装和已打包的防御设施本来就按这些原版设施保存，不受影响。然后存档、退出游戏，再删除 DefensesExpanded.dll。改了哪些东西写在 LogOutput.log 里。再关掉此项，包裹也不会变回来。");

        foreach (var kind in DefenseRows.All) BindSettings(kind);

        OptionalLive.Watch(uninstallPrep, isOn =>
        {
            UninstallPrep.Set(isOn);
            if (isOn) Log.LogInfo("Uninstall prep is on: load a save to turn the defense packages into the game's own defense packages");
        });

        var tables = new DefenseTables(message => Log.LogInfo(message));
        var harmony = new Harmony(Guid);
        var iconPlaced = false;

        SavePatches.Apply(harmony, message => Log.LogWarning(message));
        FxPatches.Apply(harmony, message => Log.LogWarning(message));
        DefenseModel.Apply(harmony, message => Log.LogInfo(message));

        DefenseTicker.Log = message => Log.LogError(message);
        UninstallPrep.Log = message => Log.LogInfo(message);
        DefenseTicker.Check = now =>
        {
            if (!iconPlaced)
            {
                iconPlaced = true;
                Log.LogInfo(DefenseIcon.Place());
            }

            tables.Tick(now);
            UninstallPrep.Tick(now);
        };
        AddComponent<DefenseTicker>();
        Log.LogInfo($"Defenses Expanded {Version} loaded");
    }

    /// <summary>
    /// One section of settings per defense, with only the settings its role uses. Every one applies live: the
    /// defense's rows are rewritten when one changes, and the defenses already placed take the new numbers.
    /// </summary>
    private void BindSettings(DefenseKind kind)
    {
        var section = kind.Name.Replace(" ", "");
        var name = kind.Name;
        var zh = kind.Texts.First(text => text.Key == kind.TextKey("Name")).Zh;
        var d = kind.Defaults;
        var entries = new List<ConfigEntryBase>();

        ConfigEntry<T> Bind<T>(string key, T value, string en, string chinese, AcceptableValueBase? range = null)
        {
            var entry = Config.Bind(section, key, value, new ConfigDescription(en + "\n" + chinese, range));

            entries.Add(entry);

            return entry;
        }

        var health = Bind("Health", d.Health,
            $"How much damage a {name} takes before it breaks. The game's Sandbags have 600 and its other defenses 200.",
            $"{zh}被打坏前能承受多少伤害。原版沙包是 600，其他原版防御设施是 200。",
            new AcceptableValueRange<int>(1, 20000));
        var materials = Bind("Materials", d.Materials,
            $"What crafting a {name} Package takes, written as item id:count pairs split by commas. 20004 is Scrap Metal, 20104 Wire, 20210 Electronic Components, 20361 Iron Rod and 20362 Iron Parts. Up to {MaterialList.MostKinds} different items. A list the game cannot use (an unknown item, or the same materials as another recipe) falls back to the default, and LogOutput.log says why.",
            $"制作{zh}包裹所需的材料，写成「物品id:数量」，用逗号隔开。20004 是铁皮，20104 是铁丝，20210 是电器元件，20361 是铁棍，20362 是铁制零件。最多 {MaterialList.MostKinds} 种物品。游戏用不了的清单（物品不存在，或和别的配方材料相同）会改用默认值，原因写在 LogOutput.log 里。");
        var repairPerPart = Bind("RepairPerPart", d.RepairPerPart,
            $"How much health one Repair Parts puts back on a {name}. The game's Sandbags get 200 and its Spike Barrier 50.",
            $"每个维修零件能为{zh}恢复多少耐久。原版沙包是 200，尖刺障碍是 50。",
            new AcceptableValueRange<int>(1, 5000));
        var repairCost = Bind("RepairCostRatio", d.RepairCostRatio,
            $"How many Repair Parts fixing a {name} from broken takes, as a share of what crafting it takes: 0.5 is half. The game uses 0.5 for its Sandbags and 0.8 for its Mechanical Chainsaw.",
            $"把完全损坏的{zh}修好要用多少维修零件，按制作材料的比例算：0.5 表示一半。原版沙包是 0.5，机械电锯是 0.8。",
            new AcceptableValueRange<float>(0f, 2f));
        var siege = Bind("SiegePoints", d.SiegePoints,
            $"How much each {name} adds to the size of the next siege: the more defenses you build, the more zombies come. The game counts its Sandbags as 1 and its Mechanical Chainsaw as 6.",
            $"每个{zh}让下一次围攻增加多少规模：防御设施越多，来的丧尸越多。原版沙包算 1，机械电锯算 6。",
            new AcceptableValueRange<int>(0, 50));

        ConfigEntry<int>? damage = null, maxTargets = null, slowPercent = null, powerCost = null;
        ConfigEntry<float>? interval = null, range = null, slowSeconds = null, powerPerHit = null, shutsDown = null;

        if (kind.Attacks)
        {
            damage = Bind("Damage", d.Damage,
                $"How much damage a {name} does to each zombie it strikes. The game's Electric Trap does 2 and its Mechanical Chainsaw 12.",
                $"{zh}每次对命中的每只丧尸造成多少伤害。原版电网是 2，机械电锯是 12。",
                new AcceptableValueRange<int>(0, 500));
            interval = Bind("AttackInterval", d.AttackInterval,
                $"Seconds between a {name}'s strikes.",
                $"{zh}两次攻击之间隔几秒。",
                new AcceptableValueRange<float>(0.1f, 60f));
            range = Bind("AttackRange", d.AttackRange,
                $"How far from a {name} a zombie can be and still be struck, in meters.",
                $"丧尸离{zh}多远以内会被攻击，单位是米。",
                new AcceptableValueRange<float>(0.1f, 10f));
            maxTargets = Bind("MaxTargets", d.MaxTargets,
                $"How many zombies a {name} strikes at once.",
                $"{zh}一次最多攻击几只丧尸。",
                new AcceptableValueRange<int>(1, 50));
        }

        if (kind.Slows)
        {
            slowPercent = Bind("SlowPercent", d.SlowPercent,
                $"How much slower a zombie the {name} strikes walks, in percent. 0 does not slow them. The game's Electric Trap slows by 50.",
                $"被{zh}击中的丧尸走路慢多少，单位是百分比。0 表示不减速。原版电网是 50。",
                new AcceptableValueRange<int>(0, 90));
            slowSeconds = Bind("SlowSeconds", d.SlowSeconds,
                $"How many seconds the slow from a {name} lasts.",
                $"{zh}的减速效果持续几秒。",
                new AcceptableValueRange<float>(0f, 60f));
        }

        if (kind.Powered)
        {
            powerCost = Bind("PowerCost", d.PowerCost,
                $"Power a {name} draws. Without power it does not strike. 0 lets it work with no power at all. The game's Electric Trap draws 3 and its Mechanical Chainsaw 5.",
                $"{zh}的耗电量。没有电时不会攻击。0 表示完全不用电也能工作。原版电网耗电 3，机械电锯耗电 5。",
                new AcceptableValueRange<int>(0, 50));
            powerPerHit = Bind("PowerPerHit", d.PowerPerHit,
                $"Extra power each strike of a {name} uses up.",
                $"{zh}每次攻击额外消耗的电量。",
                new AcceptableValueRange<float>(0f, 50f));
            shutsDown = Bind("ShutsDownBelow", d.ShutsDownBelow,
                $"A {name} whose health falls below this share stops working until it is repaired: 0.5 is half its health. 0 keeps it working until it breaks. The game's Mechanical Chainsaw uses 0.5.",
                $"{zh}的耐久低于这个比例时会停止工作，直到修好为止：0.5 表示一半耐久。0 表示一直工作到损坏。原版机械电锯是 0.5。",
                new AcceptableValueRange<float>(0f, 0.95f));
        }

        OptionalLive.Watch(() => CurrentSettings.Set(kind, new DefenseSettings
        {
            Health = health.Value,
            Materials = materials.Value,
            RepairPerPart = repairPerPart.Value,
            RepairCostRatio = repairCost.Value,
            SiegePoints = siege.Value,
            Damage = damage?.Value ?? 0,
            AttackInterval = interval?.Value ?? 0f,
            AttackRange = range?.Value ?? 0f,
            MaxTargets = maxTargets?.Value ?? 0,
            SlowPercent = slowPercent?.Value ?? 0,
            SlowSeconds = slowSeconds?.Value ?? 0f,
            PowerCost = powerCost?.Value ?? 0,
            PowerPerHit = powerPerHit?.Value ?? 0f,
            ShutsDownBelow = shutsDown?.Value ?? 0f,
        }), entries.ToArray());
    }
}
