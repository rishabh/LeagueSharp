using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using LeagueSharp;
using LeagueSharp.Common;
using System.Linq;
using SharpDX;
using System.Drawing;
using System.Xml.Linq;

public class yetAnotherUdyr
{
    //Script Information

    static string versionNumber = "1.3.0";
    //Ease of use

    static readonly Obj_AI_Hero Player = ObjectManager.Player;
    static Menu Config;

    static Orbwalking.Orbwalker Orbwalker;
    static List<int> levelUpListPhoenix = new List<int> {3, 2, 1, 0, 3, 3, 1, 3, 1, 3, 1, 1, 2, 2, 2, 2, 0, 0};
    static List<int> levelUpListTiger = new List<int> {0, 2, 1, 0, 0, 2, 0, 1, 0, 2, 2, 2, 1, 1, 1, 3, 3, 3};

    //Spells
    static Spell Q;
    static Spell W;
    static Spell E;
    static Spell R;

    //Items
    //'Offensive - minus 25 range
    static Items.Item BilgeCut = new Items.Item(3144, 475);
    static Items.Item BoTRK = new Items.Item(3153, 425);
    static Items.Item RavHydra = new Items.Item(3074, 375);
    static Items.Item Tiamat = new Items.Item(3077, 375);
    static Items.Item RanOmen = new Items.Item(3143, 490);

    //'Defensive - minus 10 range
    static Items.Item LoTIS = new Items.Item(3190, 590);

    //Drawing
    static Dictionary<int, System.Drawing.Color> enemyColor = new Dictionary<int, System.Drawing.Color>();

    public static void Main(string[] arg)
    {
        CustomEvents.Game.OnGameLoad += Game_onGameLoad;
    }

    #region "Event Handlers"

    public static void Game_onGameLoad(System.EventArgs args)
    {
        Game.PrintChat("yetAnotherUdyr by FlapperDoodle, version: " + versionNumber);
        if (ObjectManager.Player.ChampionName != "Udyr")
        {
            Game.PrintChat("Please use Udyr~");
            return;
        }

        //Spell Initialize
        Q = new Spell(SpellSlot.Q, 200);
        W = new Spell(SpellSlot.W, 200);
        E = new Spell(SpellSlot.E, 200);
        R = new Spell(SpellSlot.R, 200);

        //Main Menu
        Config = new Menu("yA-Udyr", "yA-Udyr", true);

        //Orbwalker
        Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
        Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

        //Target Selector
        var targetSelector = new Menu("Target Selector", "Target Selector");
        TargetSelector.AddToMenu(targetSelector);
        Config.AddSubMenu(targetSelector);

        //Main
        Config.AddItem(new MenuItem("Combo Key", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
        Config.AddItem(new MenuItem("Style", "Style").SetValue(new StringList(new[] {"Phoenix", "Tiger"}, 0)));

        //Items
        Config.AddSubMenu(new Menu("Items", "Items"));
        //'Offensive
        Config.SubMenu("Items").AddSubMenu(new Menu("Offense", "Offense"));
        Config.SubMenu("Items")
            .SubMenu("Offense")
            .AddItem(new MenuItem("BilgeCut", "Bilgewater Cutlass").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("BoTRK", "BoT Ruined King").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("RavHydra", "Ravenous Hydra").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("RanOmen", "Randuin's Omen").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("Tiamat", "Tiamat").SetValue(true));
        //'Defensive
        Config.SubMenu("Items").AddSubMenu(new Menu("Defense", "Defense"));
        Config.SubMenu("Items").SubMenu("Defense").AddSubMenu(new Menu("LoT Iron Solari", "LoTIS-Menu"));
        ///LoT-IS
        Config.SubMenu("Items")
            .SubMenu("Defense")
            .SubMenu("LoTIS-Menu")
            .AddItem(new MenuItem("LoTIS", "Enabled").SetValue(true));
        Config.SubMenu("Items")
            .SubMenu("Defense")
            .SubMenu("LoTIS-Menu")
            .AddItem(new MenuItem("LoTIS-HP-%", "Use at HP %").SetValue(new Slider(40)));

        //Farm
        Config.AddSubMenu(new Menu("Farm", "Farm"));
        Config.SubMenu("Farm").AddItem(new MenuItem("Use-Q-Farm", "Use Q").SetValue(true));
        Config.SubMenu("Farm").AddItem(new MenuItem("Use-R-Farm", "Use R").SetValue(true));
        Config.SubMenu("Farm").AddItem(new MenuItem("Farm-Mana", "Mana Limit").SetValue(new Slider(20)));
        Config.SubMenu("Farm")
            .AddItem(new MenuItem("Farm Key", "Farm Key").SetValue(new KeyBind(86, KeyBindType.Press)));

        //Jungle Farm
        Config.AddSubMenu(new Menu("Jungle Farm", "Jungle Farm"));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Use-Q-Jungle", "Use Q").SetValue(true));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Use-R-Jungle", "Use R").SetValue(true));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Use-W-Jungle", "Use W").SetValue(true));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Jungle-Mana", "Mana Limit").SetValue(new Slider(20)));
        Config.SubMenu("Jungle Farm")
            .AddItem(new MenuItem("Jungle Farm Key", "Jungle Farm Key").SetValue(new KeyBind(67, KeyBindType.Press)));

        //Misc
        Config.AddSubMenu(new Menu("Misc", "Misc"));
        Config.SubMenu("Misc").AddItem(new MenuItem("Auto Level", "Auto Level").SetValue(true));
        Config.SubMenu("Misc").AddItem(new MenuItem("Stun Lock", "Stun Lock").SetValue(true));
        Config.SubMenu("Misc").AddItem(new MenuItem("Interrupt-Enemies", "Interrupt Enemies").SetValue(true));
        Config.SubMenu("Misc").AddItem(new MenuItem("Use-W-Turret", "Use W on Turret Cast").SetValue(true));

        //Drawing
        Config.AddSubMenu(new Menu("Drawing", "Drawing"));
        Config.SubMenu("Drawing").AddItem(new MenuItem("Draw", "Draw").SetValue(true));
        //'Enemy Status
        foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
        {
            //Add the enemy to our dictionary, so we can update the colors
            enemyColor.Add(enemy.NetworkId, System.Drawing.Color.Green);
            //Assign a menu to each enemy
            Config.SubMenu("Drawing").AddSubMenu(new Menu(enemy.ChampionName, enemy.ChampionName));
            Config.SubMenu("Drawing")
                .SubMenu(enemy.ChampionName)
                .AddItem(new MenuItem(enemy.NetworkId + "E", "Enabled").SetValue(true));
            Config.SubMenu("Drawing")
                .SubMenu(enemy.ChampionName)
                .AddItem(new MenuItem(enemy.NetworkId + "KC", "Killable Circle").SetValue(true));
            Config.SubMenu("Drawing")
                .SubMenu(enemy.ChampionName)
                .AddItem(new MenuItem(enemy.NetworkId + "HP", "HP").SetValue(false));
            Config.SubMenu("Drawing")
                .SubMenu(enemy.ChampionName)
                .AddItem(new MenuItem(enemy.NetworkId + "MP", "MP").SetValue(false));
            Config.SubMenu("Drawing")
                .SubMenu(enemy.ChampionName)
                .AddItem(
                    new MenuItem(enemy.NetworkId + "R", "Range").SetValue(
                        new StringList(new[] {"Basic", "Q", "W", "E", "R"})));
            Config.SubMenu("Drawing")
                .SubMenu(enemy.ChampionName)
                .AddItem(
                    new MenuItem(enemy.NetworkId + "RC", "Range Color").SetValue(new Circle(true,
                        System.Drawing.Color.Gray)));
        }
        Config.AddToMainMenu();

        //Handles
        Game.OnUpdate += Game_OnUpdate;
        CustomEvents.Unit.OnLevelUp += Unit_OnLevelUp;
        Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

        Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
    }

    // Protect from Turret
    private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
    {
        if (!Config.Item("Use-W-Turret").GetValue<bool>()) return;

        try
        {
            if (!sender.IsValid<Obj_AI_Turret>() || !args.Target.IsValid<Obj_AI_Hero>() || !args.Target.IsMe) return;

            W.Cast();
        }
        catch
        {
        }
    }


    // Interrupt Event, test
    private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
        Interrupter2.InterruptableTargetEventArgs args)
    {
        if (!Config.Item("Interrupt-Enemies").GetValue<bool>() || !sender.IsValidTarget(200) ||
            args.DangerLevel != Interrupter2.DangerLevel.High || !E.IsReady()) return;

        E.Cast();
        TargetSelector.SetTarget(sender);
    }

    //Orbwalk Events Here, farm goes here
    public static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
    {
        //if farm key is pressed
        if (Config.Item("Farm Key").GetValue<KeyBind>().Active && Orbwalking.CanMove(50) &&
            ((Player.Mana / Player.MaxMana) * 100) >= Config.Item("Farm-Mana").GetValue<Slider>().Value)
            Farm();
    }

    //Auto level up skill
    public static void Unit_OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
    {
        if (!sender.IsMe || !Config.Item("Auto Level").GetValue<bool>()) return;

        if (Config.Item("Style").GetValue<StringList>().SelectedIndex == 0)
            Player.Spellbook.LevelUpSpell((SpellSlot) levelUpListPhoenix[args.NewLevel - 1]);
        else
            Player.Spellbook.LevelUpSpell((SpellSlot) levelUpListTiger[args.NewLevel - 1]);
    }

    public static void Game_OnUpdate(EventArgs args)
    {
        if (Player.IsDead)
            return;

        //If combo key is pressed
        if (Config.Item("Combo Key").GetValue<KeyBind>().Active)
        {
            ComboIt();
        }

        //Defensive Items (currently only LoTIS)
        if (Config.Item("LoTIS").GetValue<bool>() && Items.HasItem(LoTIS.Id) && LoTIS.IsReady() &&
            ((Player.Health / Player.MaxHealth) * 100) <= Config.Item("LoTIS-HP-%").GetValue<Slider>().Value)
            LoTIS.Cast(Player);

        //If jungle key is pressed
        if (Config.Item("Jungle Farm Key").GetValue<KeyBind>().Active &&
            ((Player.Mana / Player.MaxMana) * 100) >= Config.Item("Jungle-Mana").GetValue<Slider>().Value)
            JungleFarm();

        //If drawing is on
        if (Config.Item("Draw").GetValue<bool>())
            UpdateIsKillable();
    }

    #endregion

    #region "Methods/Functions"

    public static void ComboIt()
    {
        //Create target
        var target = TargetSelector.GetTarget(600f, TargetSelector.DamageType.Magical);

        if (target == null)
            return;

        //Skill order sequence
        if (Player.Distance(target) < 300)
        {
            //If stun lock is on, the target doesn't have a stun buff, and the spell is ready, then cast bear stun
            if (Config.Item("Stun Lock").GetValue<bool>() && E.IsReady() && !target.HasBuff("udyrbearstuncheck"))
            {
                E.Cast();
                return;
            }

            if (Config.Item("Style").GetValue<StringList>().SelectedIndex == 0)
            {
                if (R.IsReady())
                    R.Cast();
                if (Q.IsReady())
                    Q.Cast();
                if (E.IsReady() && !target.HasBuff("udyrbearstuncheck"))
                    E.Cast();
                if (W.IsReady())
                    W.Cast();
            }
            else
            {
                if (E.IsReady() && !target.HasBuff("udyrbearstuncheck"))
                    E.Cast();
                if (Q.IsReady())
                    Q.Cast();
                if (W.IsReady())
                    W.Cast();
                if (R.IsReady())
                    R.Cast();
            }
        }

        //Do Attack Items
        if (Config.Item("BoTRK").GetValue<bool>() && Items.HasItem(BoTRK.Id) && BoTRK.IsReady())
            BoTRK.Cast(target);
        if (Config.Item("RavHydra").GetValue<bool>() && Items.HasItem(RavHydra.Id) && RavHydra.IsReady())
            RavHydra.Cast(target);
        if (Config.Item("BilgeCut").GetValue<bool>() && Items.HasItem(BilgeCut.Id) && BilgeCut.IsReady())
            BilgeCut.Cast(target);
        if (Config.Item("Tiamat").GetValue<bool>() && Items.HasItem(Tiamat.Id) && Tiamat.IsReady())
            Tiamat.Cast(target);
        if (Config.Item("RanOmen").GetValue<bool>() && Items.HasItem(RanOmen.Id) && RanOmen.IsReady() &&
            Player.Distance(target) <= 490)
            RanOmen.Cast(target);
    }

    public static void Farm()
    {
        var minions = MinionManager.GetMinions(Player.ServerPosition, 500f);

        if (minions.Count < 3)
            return;

        if (Config.Item("Use-R-Farm").GetValue<bool>() && R.IsReady())
            R.Cast();
        if (Config.Item("Use-Q-Farm").GetValue<bool>() && Q.IsReady())
            Q.Cast();
    }

    public static void JungleFarm()
    {
        var jungleMobs = MinionManager.GetMinions(Player.ServerPosition, 700, MinionTypes.All, MinionTeam.Neutral,
            MinionOrderTypes.MaxHealth);
        if (jungleMobs.Count == 0)
            return;

        if (Config.Item("Use-R-Jungle").GetValue<bool>() && R.IsReady())
            R.Cast();
        if (Config.Item("Use-Q-Jungle").GetValue<bool>() && Q.IsReady())
            Q.Cast();
        if (Config.Item("Use-W-Jungle").GetValue<bool>() && W.IsReady())
            W.Cast();
    }

    public static void UpdateIsKillable()
    {
        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
        {
            //Regular
            var totalDamage = Player.GetAutoAttackDamage(enemy);

            //Damage Spells
            if (Q.IsReady())
                totalDamage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            if (R.IsReady())
                totalDamage += Player.GetSpellDamage(enemy, SpellSlot.R);

            //Items
            if (BilgeCut.IsReady() && Config.Item("BilgeCut").GetValue<bool>())
                totalDamage += Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            if (BoTRK.IsReady() && Config.Item("BoTRK").GetValue<bool>())
                totalDamage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            if (RavHydra.IsReady() && Config.Item("RavHydra").GetValue<bool>())
                totalDamage += Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            if (Tiamat.IsReady() && Config.Item("Tiamat").GetValue<bool>())
                totalDamage += Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);


            var newEnemyHealth = ((enemy.Health - totalDamage) / enemy.MaxHealth);

            if (newEnemyHealth >= 0.66)
                enemyColor[enemy.NetworkId] = System.Drawing.Color.Green;
            else if (newEnemyHealth > 0.329 && newEnemyHealth < 0.66)
                enemyColor[enemy.NetworkId] = System.Drawing.Color.Yellow;
            else if (newEnemyHealth <= 0.39)
                enemyColor[enemy.NetworkId] = System.Drawing.Color.Red;
        }
    }

    #endregion
}