using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

public class yetAnotherDiana
{

    //Script Information

    private static string versionNumber = "1.0.0.1";

    //Ease of use
    private static Obj_AI_Hero Player = ObjectManager.Player;
    private static Menu Config;

    private static Orbwalking.Orbwalker _orbwalker;
    private static List<int> levelUpList = new List<int> { 0, 1, 0, 2, 0, 3, 0, 1, 0, 1, 3, 1, 1, 2, 2, 3, 2, 2 };

    //Spells
    private static Spell _q;
    private static Spell _w;
    private static Spell _e;
    private static Spell _r;

    //Items
    ///Offensive - minus 25 range
    private static Items.Item BilgeCut = new Items.Item(3144, 475);
    private static Items.Item BoTRK = new Items.Item(3153, 425);
    private static Items.Item RavHydra = new Items.Item(3074, 375);
    private static Items.Item Tiamat = new Items.Item(3077, 375);
    private static Items.Item Dfg;

    ///Defensive - minus 10 range
    private static Items.Item LoTIS = new Items.Item(3190, 590);
    private static Items.Item Zhonya = new Items.Item(3157, 10);
    private static Items.Item RanOmen = new Items.Item(3143, 490);

    //Ignite
    private static SpellSlot Ignite;
    private static SpellSlot Smite;

    //Drawing
    private static Dictionary<string, System.Drawing.Color> enemyColor = new Dictionary<string, System.Drawing.Color>();

    public static void Main(string[] arg)
    {
        CustomEvents.Game.OnGameLoad += Game_onGameLoad;
    }

    // First Start
    static void QuickStart()
	{
		if (!Player.Gold.Equals (475f) && Game.Time > 80f)
			return;

		if (Smite == SpellSlot.Unknown)
		{	switch (Config.Item ("Quick Start").GetValue<StringList> ().SelectedIndex) {
			//Quick Start for Summoner's Rift
			case 1:
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.	Struct (3340, Player.NetworkId)).Send (); //Warding Totem
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2003, Player.NetworkId)).Send (); //Health Potion
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2004, Player.NetworkId)).Send (); //Mana Potion
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2004, Player.NetworkId)).Send (); //Mana Potion
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2041, Player.NetworkId)).Send (); //Crystalline Flask
				break;
			case 2:
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (3340, Player.NetworkId)).Send (); //Warding Totem
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2041, Player.NetworkId)).Send (); //Crystalline Flask
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2003, Player.NetworkId)).Send (); //Health Potion
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2003, Player.NetworkId)).Send (); //Health Potion
				Packet.C2S.BuyItem.Encoded (new Packet.C2S.BuyItem.Struct (2003, Player.NetworkId)).Send (); //Health Potion
				break;
			}
		}
        if (Config.Item("Quick Start").GetValue<StringList>().SelectedIndex == 3 && Smite != SpellSlot.Unknown)
        {
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(3340, Player.NetworkId)).Send(); //Warding Totem
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(1039, Player.NetworkId)).Send(); //Hunter's Machete
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(2003, Player.NetworkId)).Send(); //Health Potion
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(2003, Player.NetworkId)).Send(); //Health Potion
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(2003, Player.NetworkId)).Send(); //Health Potion
            Packet.C2S.BuyItem.Encoded(new Packet.C2S.BuyItem.Struct(2003, Player.NetworkId)).Send(); //Health Potion
        }

        if (!Config.Item("Auto Level").GetValue<bool>())
            return;

        Player.Spellbook.LevelUpSpell((SpellSlot)levelUpList[Player.Level - 1]);
    }

    #region "Event Handlers"

    //OnGameLoad
    public static void Game_onGameLoad(EventArgs args)
    {
        Game.PrintChat("yetAnotherDiana by FlapperDoodle, version: " + versionNumber);
        if (ObjectManager.Player.ChampionName != "Diana")
        {
            Game.PrintChat("Please use Diana~");
            return;
        }

        //Spell Initialization
        _q = new Spell(SpellSlot.Q, 900);
        _q.SetSkillshot(0.35f, 180f, 1800f, false, SkillshotType.SkillshotCircle);
        _w = new Spell(SpellSlot.W, 240); //-10 range
        _e = new Spell(SpellSlot.E, 450);
        _r = new Spell(SpellSlot.R, 825);

        Ignite = Player.GetSpellSlot("SummonerDot");
        Smite = Player.GetSpellSlot("SummonerSmite");
        //Main Menu
        Config = new Menu("yA-Diana", "yA-Diana", true);

        //Orbwalker
        Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
        _orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

        //Target Selector
        var targetSelector = new Menu("Target Selector", "Target Selector");
        SimpleTs.AddToMenu(targetSelector);
        Config.AddSubMenu(targetSelector);

        //Combo
        Config.AddSubMenu(new Menu("Combo", "Combo"));
        Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Key", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press))); //Spacebar
        Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Use-W", "Use W").SetValue(true));
        Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Use-E", "Use E").SetValue(true));
        Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Jump-To-Target", "Jump To Target").SetValue(new StringList(new[] {
			"Killable",
			"On",
			"Off"
		})));

        //Killsteal
        Config.AddSubMenu(new Menu("Killsteal", "Killsteal"));
        Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Enabled", "Enabled").SetValue(true));
        Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-Q", "Use Q").SetValue(true));
        Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-R", "Use R").SetValue(true));
        Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-Ignite", "Use Ignite").SetValue(true));

        //Harass
        Config.AddSubMenu(new Menu("Harass", "Harass"));
        Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Key", "Harass Key").SetValue(new KeyBind(67, KeyBindType.Press))); //C
        Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Use-Q", "Use Q").SetValue(true));
        Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Use-W", "Use W").SetValue(true));
        Config.SubMenu("Harass").AddItem(new MenuItem("Harass-MoveTo", "Move To Mouse").SetValue(true));

        //Farm
        Config.AddSubMenu(new Menu("Farm", "Farm"));
        Config.SubMenu("Farm").AddItem(new MenuItem("Farm-Key", "Farm Key").SetValue(new KeyBind(86, KeyBindType.Press))); // V
        Config.SubMenu("Farm").AddItem(new MenuItem("Use-Q-Farm", "Use Q").SetValue(true));
        Config.SubMenu("Farm").AddItem(new MenuItem("Use-W-Farm", "Use W").SetValue(true));
        Config.SubMenu("Farm").AddItem(new MenuItem("Farm-Mana", "Mana Limit").SetValue(new Slider(20)));

        //Jungle Farm
        Config.AddSubMenu(new Menu("Jungle Farm", "Jungle Farm"));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Jungle-Farm-Key", "Jungle-Farm-Key").SetValue(new KeyBind(86, KeyBindType.Press))); //V
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Use-Q-Jungle", "Use Q").SetValue(true));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Use-W-Jungle", "Use W").SetValue(true));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Use-R-Jungle", "Use R").SetValue(new StringList(new[] {
			"With Q",
			"On",
			"Off"
		})));
        Config.SubMenu("Jungle Farm").AddItem(new MenuItem("Jungle-Mana", "Mana Limit").SetValue(new Slider(20)));

        //Items	
        Config.AddSubMenu(new Menu("Items", "Items"));
        Config.SubMenu("Items").AddItem(new MenuItem("Items-Enabled", "Enabled").SetValue(true));
        //'Offensive
        Config.SubMenu("Items").AddSubMenu(new Menu("Offense", "Offense"));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("BilgeCut", "Bilgewater Cutlass").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("BotRK", "Bot Ruined King").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("DFG", "Deathfire Grasp").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("RavHydra", "Ravenous Hydra").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("RanOmen", "Randuin's Omen").SetValue(true));
        Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("Tiamat", "Tiamat").SetValue(true));

        //'Defensive	
        Config.SubMenu("Items").AddSubMenu(new Menu("Defense", "Defense"));
        //>LoTIS
        Config.SubMenu("Items").SubMenu("Defense").AddSubMenu(new Menu("LoT Iron Solari", "LoTIS-Menu"));
        Config.SubMenu("Items").SubMenu("Defense").SubMenu("LoTIS-Menu").AddItem(new MenuItem("LoTIS", "Enabled").SetValue(true));
        Config.SubMenu("Items").SubMenu("Defense").SubMenu("LoTIS-Menu").AddItem(new MenuItem("LoTIS-HP-%", "HP %").SetValue(new Slider(40)));
        //>Zhonya's
        Config.SubMenu("Items").SubMenu("Defense").AddSubMenu(new Menu("Zhonya's Hourglass", "Zhonya-Menu"));
        Config.SubMenu("Items").SubMenu("Defense").SubMenu("Zhonya-Menu").AddItem(new MenuItem("Zhonya", "Enabled").SetValue(true));
        Config.SubMenu("Items").SubMenu("Defense").SubMenu("Zhonya-Menu").AddItem(new MenuItem("Zhonya-HP-%", "HP %").SetValue(new Slider(15)));

        //Misc
        Config.AddSubMenu(new Menu("Misc", "Misc"));
        Config.SubMenu("Misc").AddItem(new MenuItem("Auto Level", "Auto Level").SetValue(true));
        Config.SubMenu("Misc").AddItem(new MenuItem("Packet Casting", "Packet Casting").SetValue(true));
        Config.SubMenu("Misc").AddItem(new MenuItem("Quick Start", "Quick Start").SetValue(new StringList(new[] {
			"Off",
			"CF,HP,2*MP",
			"CF,3*HP",
			"Jungle: HM,4*HP"
		})));

        //Drawing
        Config.AddSubMenu(new Menu("Drawing", "Drawing"));
        Config.SubMenu("Drawing").AddItem(new MenuItem("Draw", "Draw").SetValue(true));
        Config.SubMenu("Drawing").AddItem(new MenuItem("Draw R", "Draw R").SetValue(new Circle(true, System.Drawing.Color.DeepPink)));
        //>Enemy Status
        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
        {
            //Add the enemy to our dictionary, so we can update the colors
            enemyColor.Add(enemy.ChampionName, System.Drawing.Color.Green);
            //Assign a menu to each enemy
            Config.SubMenu("Drawing").AddSubMenu(new Menu(enemy.ChampionName, enemy.ChampionName));
            Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(new MenuItem(enemy.ChampionName + "E", "Enabled").SetValue(true));
            Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(new MenuItem(enemy.ChampionName + "KC", "Killable Circle").SetValue(true));
            Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(new MenuItem(enemy.ChampionName + "HP", "HP").SetValue(true));
            Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(new MenuItem(enemy.ChampionName + "MP", "MP").SetValue(true));
            Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(new MenuItem(enemy.ChampionName + "R", "Range").SetValue(new StringList(new[] {
				"Basic",
				"Q",
				"W",
				"E",
				"R"
			})));
            Config.SubMenu("Drawing").SubMenu(enemy.ChampionName).AddItem(new MenuItem(enemy.ChampionName + "RC", "Range Color").SetValue(new Circle(true, System.Drawing.Color.Gray)));
        }

        //Interrupt
        Config.AddSubMenu(new Menu("Interrupt", "Interrupt"));
        Config.SubMenu("Interrupt").AddItem(new MenuItem("Interrupt-Enabled", "Enabled").SetValue(new StringList(new[] {
			"Off in Combo",
			"On",
			"Off"
		},1)));
        Config.SubMenu("Interrupt").AddItem(new MenuItem("Interrupt-Use-E", "Use E").SetValue(true));
        Config.SubMenu("Interrupt").AddItem(new MenuItem("Interrupt-Use-R", "Use R").SetValue(new StringList(new[] {
			"Danger",
			"On",
			"Off"
		})));

        // Map Specific
        if (Utility.Map.GetMap()._MapType == Utility.Map.MapType.SummonersRift)
        {
            Dfg = new Items.Item(3128, 750);
            //>Jungle Jump
            Config.SubMenu("Jungle Farm").AddSubMenu(new Menu("Jungle Jump", "Jungle Jump"));
            Config.SubMenu("Jungle Farm").SubMenu("Jungle Jump").AddItem(new MenuItem("Jungle-Jump-Key", "Jungle Jump").SetValue(new KeyBind(71, KeyBindType.Press))); //G
            Config.SubMenu("Jungle Farm").SubMenu("Jungle Jump").AddItem(new MenuItem("Jungle-Draw-Spots", "Draw Spots").SetValue(new Circle(true, System.Drawing.Color.Blue)));
            Config.SubMenu("Jungle Farm").SubMenu("Jungle Jump").AddItem(new MenuItem("Jungle-MoveTo", "Move To Mouse").SetValue(true));
        }
        else
            Dfg = new Items.Item(3188, 750);

        //Gap Closer
        Config.AddSubMenu(new Menu("Gap Closer", "Gap Closer"));
        Config.SubMenu("Gap Closer").AddItem(new MenuItem("GapCloser-Use-W", "Use W").SetValue(true));

        Config.AddToMainMenu();

        //Handles
        Game.OnGameUpdate += Game_OnGameUpdate;
        Drawing.OnDraw += Drawing_OnDraw;
        CustomEvents.Unit.OnLevelUp += Unit_OnLevelUp;
        //Farm
        Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        //Double-Check
        Game.OnGameProcessPacket += GameOnOnGameProcessPacket;
        Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        //Interrupt
        Interrupter.OnPossibleToInterrupt += InterrupterOnPossibleToInterrupt;
        //Gap Closer
        AntiGapcloser.OnEnemyGapcloser += AntiGapcloserOnOnEnemyGapcloser;

        //FirstStart
        QuickStart();
    }

    #region "Regular Events"

    //Interrupt + GapCloser
    static void AntiGapcloserOnOnEnemyGapcloser(ActiveGapcloser gapcloser)
    {
		if (Config.Item("GapCloser-Use-W").GetValue<bool>() && _w.IsReady())
            _w.Cast();
    }

    static void InterrupterOnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
    {
        if (Config.Item("Interrupt-Enabled").GetValue<StringList>().SelectedIndex == 2 || Player.HasBuff("Recall"))
            return;
        Console.WriteLine("Interrupter: " + spell.ChampionName);
		if (Config.Item("Interrupt-Use-E").GetValue<bool>() && _e.IsReady() && Player.Distance(unit) <= _e.Range)
            _e.Cast();
		else if (_r.IsReady() && Player.Distance(unit) <= _r.Range && (Config.Item("Interrupt-Use-R").GetValue<StringList>().SelectedIndex == 1 || (Config.Item("Interrupt-Use-R").GetValue<StringList>().SelectedIndex == 0 && spell.DangerLevel == InterruptableDangerLevel.High)))
            _r.Cast(unit, Config.Item("Packet Casting").GetValue<bool>());
    }

    //Increase chance of casting Q before R hits
    static void GameOnOnGameProcessPacket(GamePacketEventArgs args)
    {
		if (args.PacketData[0] == Packet.C2S.Cast.Header && _q.IsReady())
        {
            var decoded = Packet.C2S.Cast.Decoded(args.PacketData);
            if (decoded.SourceNetworkId == Player.NetworkId && decoded.Slot == SpellSlot.R && InMisayaCombo)
            {
                Console.WriteLine("Packet Cast");
                _q.Cast(ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(decoded.TargetNetworkId), Config.Item("Packet Casting").GetValue<bool>());
            }
        }
    }

    static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
    {
		if (!sender.IsMe || args.SData.Name != "DianaTeleport" || !InMisayaCombo || Player.Distance((Obj_AI_Base)args.Target) > 400 || !_q.IsReady())
            return;
        Console.WriteLine("Process Spell");
        _q.Cast((Obj_AI_Base)args.Target, Config.Item("Packet Casting").GetValue<bool>());
    }

    //Orbwalk Events Here, farm goes here
    static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
    {
        //if farm key is pressed
        if (Config.Item("Farm-Key").GetValue<KeyBind>().Active && Orbwalking.CanMove(35) && (Player.Mana / Player.MaxMana * 100) >= Config.Item("Farm-Mana").GetValue<Slider>().Value)
        {

            var minions = MinionManager.GetMinions(Player.ServerPosition, _w.Range + 100);
            if (minions.Count < 3)
                return;

            Farm();
        }


    }

    //Auto level up skill
    static void Unit_OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
    {
        if (!sender.IsMe || !Config.Item("Auto Level").GetValue<bool>())
            return;
        Player.Spellbook.LevelUpSpell((SpellSlot)levelUpList[args.NewLevel - 1]);
    }

    #endregion

    //GameOnGameUpdate
    static void Game_OnGameUpdate(EventArgs args)
    {
        if (Player.IsDead)
            return;

        if (Config.Item("Killsteal-Enabled").GetValue<bool>() && !(Config.Item("Combo-Key").GetValue<KeyBind>().Active && Config.Item("Interrupt-Enabled").GetValue<StringList>().SelectedIndex == 0))
            Killsteal();

        // Define Target
        var target = SimpleTs.GetTarget(_r.Range * 2 + 10, SimpleTs.DamageType.Magical);
        if (target != null)
        {
            //Double Check
			if (InMisayaCombo && Player.Distance(target) <= 500 && _q.IsReady())
            {
                _q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
            }

            //If combo key is pressed
            if (Config.Item("Combo-Key").GetValue<KeyBind>().Active)
            {
                ComboIt(target);
            }
            else if (Config.Item("Harass-Key").GetValue<KeyBind>().Active)
            {
				if (Config.Item("Harass-MoveTo").GetValue<bool>())
					MoveTo(Game.CursorPos);
                Harass(target);
            }
        }
        //Defensive Items
        if (Config.Item("LoTIS").GetValue<bool>() && LoTIS.IsReady() && ((Player.Health / Player.MaxHealth) * 100) <= Config.Item("LoTIS-HP-%").GetValue<Slider>().Value)
            Items.UseItem(LoTIS.Id, Player);
        if (Config.Item("Zhonya").GetValue<bool>() && Zhonya.IsReady() && (Player.Health / Player.MaxHealth) * 100 <= Config.Item("Zhonya-HP-%").GetValue<Slider>().Value)
            Items.UseItem(Zhonya.Id, Player);

        //Jungle-Farm
        if (Config.Item("Jungle-Farm-Key").GetValue<KeyBind>().Active && ((Player.Mana / Player.MaxMana) * 100) >= Config.Item("Jungle-Mana").GetValue<Slider>().Value)
            JungleFarm();

        //Farm
        if (Config.Item("Farm-Key").GetValue<KeyBind>().Active && Config.Item("Use-Q-Farm").GetValue<bool>() && (Player.Mana / Player.MaxMana * 100) >= Config.Item("Farm-Mana").GetValue<Slider>().Value && _q.IsReady())
        {
            Console.WriteLine("Farm Key");
            var tuple = getBestQPosFarm();
            if (tuple.Item1 > 2)
                _q.Cast(tuple.Item2, Config.Item("Packet Casting").GetValue<bool>());
        }

        //JungleJump
        if (Config.Item("Jungle-Jump-Key").GetValue<KeyBind>().Active)
        {
            if (Config.Item("Jungle-MoveTo").GetValue<bool>())
                MoveTo(Game.CursorPos);

            JungleJump();
        }
        //If drawing is on
        if (Config.Item("Draw").GetValue<bool>())
            UpdateIsKillable();
    }

    #endregion

    #region "Methods/Functions"

    //<-----------Combo---------->

    public static void ComboIt(Obj_AI_Hero target)
    {

        // Jump To Target
        if ((Config.Item("Combo-Jump-To-Target").GetValue<StringList>().SelectedIndex == 0 && ReturnComboDamage(target, 0) > target.Health) || //If Killable & target is killable
            (Config.Item("Combo-Jump-To-Target").GetValue<StringList>().SelectedIndex == 1)) //If On
            JumpToTarget(target);

        // Cast the Main Combo
        MainCombo(target);

        //Do Attack Items
        if (Config.Item("DFG").GetValue<bool>() & Dfg.IsReady())
            Dfg.Cast(target);
        if (Config.Item("BoTRK").GetValue<bool>() && BoTRK.IsReady())
            BoTRK.Cast(target);
        if (Config.Item("RavHydra").GetValue<bool>() && RavHydra.IsReady())
			Items.UseItem(RavHydra.Id, Player);
        if (Config.Item("BilgeCut").GetValue<bool>() && BilgeCut.IsReady())
            BilgeCut.Cast(target);
        if (Config.Item("Tiamat").GetValue<bool>() && Tiamat.IsReady())
			Items.UseItem(Tiamat.Id, Player);
        if (Config.Item("RanOmen").GetValue<bool>() && RanOmen.IsReady() && Player.Distance(target) <= 490)
            Items.UseItem(RanOmen.Id, Player);

    }

    //> Jump To Target
    public static bool JumpToTargetFlag;
    public static int JumpToTargetTick;

    public static void JumpToTarget(Obj_AI_Base target)
    {
        if (Environment.TickCount > JumpToTargetTick + 1000)
            JumpToTargetFlag = false;

		if (JumpToTargetFlag)
        {
            foreach (var minion in MinionManager.GetMinions(Player.Position, _r.Range - 10, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).Where(minion => minion.HasBuff("dianamoonlight") && minion.Distance(target) <= _r.Range - 10))
            {
                _r.CastOnUnit(minion, Config.Item("Packet Casting").GetValue<bool>());
                JumpToTargetFlag = false;
                break;
            }
        }

        //Do Combo
        // Jump To Target if in range
		if (_q.IsReady() && _r.IsReady() && (Player.Distance(target) <= _r.Range * 2 - 20) && (Player.Distance(target) > _r.Range + 10))
        {
            foreach (var minion in MinionManager.GetMinions(Player.Position, _r.Range - 10, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).Where(minion => _q.GetDamage(minion) < HealthPrediction.GetHealthPrediction(minion, 25) && minion.Distance(target) <= _r.Range - 15))
            {
                _q.Cast(minion, Config.Item("Packet Casting").GetValue<bool>());
                JumpToTargetFlag = true;
                break;
            }
        }
    }

    //>Main Combo
    static int MisayaTick;
    static bool CheckFlag;
    static bool InMisayaCombo;

    public static void MainCombo(Obj_AI_Base target)
    {
        if (InMisayaCombo)
            Console.WriteLine(Player.Distance(target));
        if (Environment.TickCount > MisayaTick + 1000)
        {
            InMisayaCombo = false;
            CheckFlag = false;
        }

		if (_r.IsReady() && Player.Distance(target) <= _r.Range)
        {

			if (_q.IsReady() && Player.Mana > Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + Player.Spellbook.GetSpell(SpellSlot.R).ManaCost)
            {
                if (Player.Distance(target) > 500)
                {
                    Console.WriteLine("First R Cast");
                    _r.Cast(target, Config.Item("Packet Casting").GetValue<bool>());

					for (var i = 0; i < 150; i++)
                    {
						if(_q.IsReady())
                        	_q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                        if (!_q.IsReady())
                            Console.WriteLine("Not Ready");
                    }

                    InMisayaCombo = true;
                    MisayaTick = Environment.TickCount;
                }
                else
                {
                    _q.Cast(target.Position, Config.Item("Packet Casting").GetValue<bool>());
                    CheckFlag = true;
                }
            }
            else if (_r.IsReady() && Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time >= 0.6)
            {
                Console.WriteLine("Second R Cast");
                _r.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
            }
            if (CheckFlag && _r.IsReady() && target.HasBuff("dianamoonlight", true))
            {
                _r.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                CheckFlag = false;
            }
        }
        if (_q.IsReady())
        {
            if (InMisayaCombo && Player.Distance(target) <= 500)
            {
                Console.WriteLine("hit");
                _q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                InMisayaCombo = false;
            }
            else if ((_q.GetDamage(target) - 10 >= target.Health && Player.Distance(target) <= _q.Range - 10 && Player.Distance(target) >= 125) || //If you can kill target with Q OR
                           (!_r.IsReady() && //If R isn't ready AND
                      ((Player.Distance(target) > 450 && Player.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time > Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown - 0.7) || //If R will not be ready by the time your Q will be available OR
                      ((Player.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time) * 1.5 > Player.Spellbook.GetSpell(SpellSlot.Q).Cooldown) || Player.Spellbook.GetSpell(SpellSlot.R).State != SpellState.Ready)))
            { //If R won't be ready for a while
                _q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
            }
        }
        if (Config.Item("Combo-Use-E").GetValue<bool>() && _e.IsReady() && Player.Distance(target) < _e.Range - 15 && Player.Distance(target) > 165 && target.IsFacing(Player))
            _e.Cast();
        if (Config.Item("Combo-Use-W").GetValue<bool>() && _w.IsReady() && Player.Distance(target) < _w.Range)
            _w.Cast();
    }
    //<--------------------------->

    //<-----------Farm------------>
    public static void Farm()
    {
        if (Config.Item("Use-W-Farm").GetValue<bool>() && _w.IsReady())
            _w.Cast();
    }

    static Tuple<int, Vector3> getBestQPosFarm()
    {
        Tuple<int, Vector3> bestSoFar = Tuple.Create(0, ObjectManager.Player.Position);
        var laneMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range);

        laneMinions.Reverse();
        Console.WriteLine("-------------");
        foreach (var minion in laneMinions)
        {
            Console.WriteLine(Player.Distance(minion));
            var hitCount = getMinionsHit(minion);
            if (hitCount > bestSoFar.Item1)
            {
                bestSoFar = Tuple.Create(hitCount, minion.ServerPosition);
            }
        }
        return bestSoFar;
    }

    private static int getMinionsHit(Obj_AI_Base target)
    {
        var laneMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range);

        return laneMinions.Count(minion => Vector3.Distance(minion.ServerPosition, target.ServerPosition) <= 190);
    }

    public static void JungleFarm()
    {
        var jungleMobs = MinionManager.GetMinions(Player.ServerPosition, 400, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
        if (jungleMobs.Count == 0)
            return;

        var jungleMob = jungleMobs.Last();

		if (Config.Item("Use-W-Jungle").GetValue<bool>() && _w.IsReady() && ((Player.Health / Player.MaxHealth * 100 < 10) || (jungleMobs.Count > 1)))
			_w.Cast(Config.Item("Packet Casting").GetValue<bool>());

        if (Player.GetAutoAttackDamage(jungleMob) * 3 > jungleMob.Health)
            return;

		if (Config.Item("Use-Q-Jungle").GetValue<bool>() && _q.IsReady())
        {
            _q.Cast(jungleMob.Position);
			if (Config.Item("Use-R-Jungle").GetValue<StringList>().SelectedIndex == 0 && _r.IsReady() && jungleMob.HasBuff("dianamoonlight", true))
            {
                _r.CastOnUnit(jungleMob);
            }
        }
		if (Config.Item("Use-R-Jungle").GetValue<StringList>().SelectedIndex == 1 && _r.IsReady())
            _r.CastOnUnit(jungleMob);
    }
    //<--------------------------->

    //<---------Killsteal--------->
    static bool CheckFlagKS;
    static int KillstealTick;

    public static void Killsteal()
    {
        if (Environment.TickCount > KillstealTick + 1000)
            CheckFlagKS = false;


		foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget(_r.Range)))
        {
            if (CheckFlagKS && target.HasBuff("dianamoonlight", true))
            {
                _r.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                CheckFlagKS = false;
            }

            if (Player.GetAutoAttackDamage(target) * 2 > target.Health && Player.Distance(target) < 125)
            {
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                return;
            }

            if (Config.Item("Killsteal-Use-Q").GetValue<bool>() && _q.IsReady() && _q.GetDamage(target) - 10 >= target.Health && Player.Distance(target) <= _q.Range - 10)
            {
                _q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
            }
			else if (!Config.Item("Killsteal-Use-R").GetValue<bool>() || (Utility.UnderTurret(target,true) && (Player.Health / Player.MaxHealth < 0.6)))
            {
                return;
            }
            else if (_r.IsReady() && _r.GetDamage(target) - 10 >= target.Health && Player.Distance(target) <= _r.Range - 5)
            {
                _r.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
            }
            else if (!Config.Item("Killsteal-Use-Q").GetValue<bool>())
            {
                return;
            }
            else if (_q.IsReady() && _r.IsReady() && Player.Distance(target) <= _r.Range - 10)
            {
                if (_q.GetDamage(target) + _r.GetDamage(target) - 10 >= target.Health)
                {
                    _r.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                    _q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                }
                else if (_q.GetDamage(target) + (_r.GetDamage(target) * 2) - 10 >= target.Health)
                {
                    _q.Cast(target, Config.Item("Packet Casting").GetValue<bool>());
                    CheckFlagKS = true;
                    KillstealTick = Environment.TickCount;
                }
            }

            if (Config.Item("Killsteal-Use-Ignite").GetValue<bool>() && Ignite != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(Ignite) == SpellState.Ready && Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) - 10 >= target.Health)
                Player.SummonerSpellbook.CastSpell(Ignite, target);

        }
    }
    //<--------------------------->

    //<-----------Other----------->

    #region "Jungle Escape Dictionary"

    static Dictionary<float, Vector3> jungleLocations = new Dictionary<float, Vector3> {
		//Blue, Bottom
		//Golems
		{ 125f, new Vector3 (7916.842f, 2533.963f, 54.2764f) }, //SmallGolem
		{ 125.1f, new Vector3 (8216.842f, 2533.963f, 54.2764f) }, //Golem
		//Lizard
		{ 180.3f, new Vector3 (7375f, 3835f, 57.01306f) }, //LizardElder, YoungLizard, YoungLizard
		//Wraith Camp
		{ 200f, new Vector3 (6555f, 5239f, 57.46352f) }, //Wraith

		//Blue, Top
		//Wolves
		{ 185f, new Vector3 (3447f, 6265f, 55.61018f) }, //GiantWolf, Wolf, Wolf
		//Golem
		{ 182f, new Vector3 (3585f, 7645f, 54.39365f) }, //AncientGolem, YoungLizard, YoungLizard
		//Wraith
		{ 181f, new Vector3 (1674f, 8207f, 54.92368f) }, //GreatWraith

		//Purple, Bottom
		//Wolves
		{ 186.1f, new Vector3 (10609f, 8089f, 65.31285f) }, //GiantWolf, Wolf, Wolf
		//Golem
		{ 236f, new Vector3 (10463f, 6751f, 54.8691f) }, //AncientGolem, YoungLizard, YoungLizard
		//Wraith
		{ 156f, new Vector3 (12337f, 6263f, 54.81839f) }, //GreatWraith

		//Purple, Top
		//Golems
		{ 125.2f, new Vector3 (5846.097f, 11914.81f, 39.58729f) }, //SmallGolem
		{ 125.3f, new Vector3 (6140.464f, 11935.47f, 39.59138f) }, //Golem
		//Lizard
		{ 180.2f, new Vector3 (6547f, 10621f, 54.635f) }, //LizardElder, YoungLizard, YoungLizard
		//Wraith Camp
		{ 200.1f, new Vector3 (7459f, 9225f, 55.50208f) }, //Wraith

		//Pits
		//Dragon
		{ 159.9f, new Vector3 (9459.52f, 4193.03f, -60.59203f) },
		//Worm
		{ 159.98f, new Vector3 (4600.495f, 10250.46f, -63.07223f) }
	};



    #endregion

    static bool jungleEscapeFlag;
    static int jungleEscapeTick;
    static void JungleJump()
    {
        if (Environment.TickCount > jungleEscapeTick + 1000)
            jungleEscapeFlag = false;

        foreach (var campRadius in jungleLocations.Keys.Where(campRadius => Game.CursorPos.To2D().Distance(jungleLocations[campRadius].To2D()) <= campRadius &&
                    Player.Distance(jungleLocations[campRadius]) <= _r.Range + (campRadius / 2) && Player.Distance(jungleLocations[campRadius]) >= campRadius + (campRadius * 0.75) &&
                    _q.IsReady() && (_r.IsReady() || Player.Spellbook.GetSpell(SpellSlot.R).CooldownExpires - Game.Time <= 0.15)))
        {
            _q.Cast(jungleLocations[campRadius].To2D(), Config.Item("Packet Casting").GetValue<bool>());
            jungleEscapeFlag = true;
            jungleEscapeTick = Environment.TickCount;
        }

        if (!jungleEscapeFlag) return;

        var mobs = MinionManager.GetMinions(Player.Position, _r.Range + 100, MinionTypes.All, MinionTeam.Neutral);
        var casted = false;
        foreach (var mob in mobs.Where(mob => mob.HasBuff("dianamoonlight", true)))
        {
            if (casted && Player.Distance(mob) <= mob.BoundingRadius + 50)
            {
                jungleEscapeFlag = false;
                break;
            }
            _r.Cast(mob, Config.Item("Packet Casting").GetValue<bool>());
            casted = true;
        }
    }

    static void Harass(Obj_AI_Base target)
    {
        if (_q.IsReady() && Player.Distance(target) < _q.Range - 25)
            _q.CastIfHitchanceEquals(target, HitChance.VeryHigh, Config.Item("Packet Casting").GetValue<bool>());
        if (_w.IsReady() && Player.Distance(target) < _w.Range - 15)
			_w.Cast(Config.Item("Packet Casting").GetValue<bool>());
    }

    static void MoveTo(Vector3 position)
    {
        var pointToMoveTo = Player.ServerPosition + 250 * (position.To2D() - Player.ServerPosition.To2D()).Normalized().To3D();
        Player.IssueOrder(GameObjectOrder.MoveTo, pointToMoveTo);
    }

    public static double ReturnComboDamage(Obj_AI_Base target, int qCount = 1, int rCount = 1)
    {
        var totalDamage = Player.GetAutoAttackDamage(target);

        //Damage Spells
        if (_q.IsReady())
            totalDamage += Player.GetSpellDamage(target, SpellSlot.Q) * qCount;
        if (_r.IsReady())
            totalDamage += Player.GetSpellDamage(target, SpellSlot.R) * rCount;
        if (_w.IsReady())
            totalDamage += Player.GetSpellDamage(target, SpellSlot.W);

        //Items
        if (Dfg.IsReady() && Config.Item("DFG").GetValue<bool>())
            totalDamage += Player.GetItemDamage(target, Damage.DamageItems.Dfg);
        if (BilgeCut.IsReady() && Config.Item("BilgeCut").GetValue<bool>())
            totalDamage += Player.GetItemDamage(target, Damage.DamageItems.Bilgewater);
        if (BoTRK.IsReady() && Config.Item("BoTRK").GetValue<bool>())
            totalDamage += Player.GetItemDamage(target, Damage.DamageItems.Botrk);
        if (RavHydra.IsReady() && Config.Item("RavHydra").GetValue<bool>())
            totalDamage += Player.GetItemDamage(target, Damage.DamageItems.Hydra);
        if (Tiamat.IsReady() && Config.Item("Tiamat").GetValue<bool>())
            totalDamage += Player.GetItemDamage(target, Damage.DamageItems.Hydra);

        return totalDamage;

    }
    //<--------------------------->

    #endregion

    #region "Drawing"
    static void Drawing_OnDraw(EventArgs args)
    {
        if (!Config.Item("Draw").GetValue<bool>() || Player.IsDead)
            return;

        if (Config.Item("Draw R").GetValue<Circle>().Active)
            Utility.DrawCircle(Player.Position, Player.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], Config.Item("Draw R").GetValue<Circle>().Color);

        if (Config.Item("Jungle-Draw-Spots").GetValue<Circle>().Active)
            foreach (var camp in jungleLocations.Where(camp => _r.IsReady() && _q.IsReady() && Player.Distance(camp.Value) <= 1500))
                Utility.DrawCircle(camp.Value, camp.Key, Config.Item("Jungle-Draw-Spots").GetValue<Circle>().Color, 2, 22);

        foreach (var enemyVisible in ObjectManager.Get<Obj_AI_Hero>().Where(enemyVisible => enemyVisible.IsValidTarget() && Config.Item(enemyVisible.ChampionName + "E").GetValue<bool>()))
        {
            //Regular drawing
			if (Config.Item(enemyVisible.ChampionName + "KC").GetValue<bool>())
                Utility.DrawCircle(enemyVisible.Position, 60, enemyColor[enemyVisible.ChampionName], 2, 15);
            if (Config.Item(enemyVisible.ChampionName + "HP").GetValue<bool>())
                Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] - 40, Drawing.WorldToScreen(enemyVisible.Position)[1] - 100, System.Drawing.Color.Red, Convert.ToInt32(enemyVisible.Health / enemyVisible.MaxHealth * 100) + "%");
            if (Config.Item(enemyVisible.ChampionName + "MP").GetValue<bool>())
                Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 10, Drawing.WorldToScreen(enemyVisible.Position)[1] - 100, System.Drawing.Color.Violet, Convert.ToInt32(enemyVisible.Mana / enemyVisible.MaxMana * 100) + "%");
            Console.WriteLine(enemyVisible.ChampionName);
            //If color is off, then go to the next enemy that is visible
            if (!Config.Item(enemyVisible.ChampionName + "RC").GetValue<Circle>().Active)
                continue;
            //Range of Skills
            switch (Config.Item(enemyVisible.ChampionName + "R").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    Utility.DrawCircle(enemyVisible.Position, enemyVisible.BasicAttack.CastRange[0], Config.Item(enemyVisible.ChampionName + "RC").GetValue<Circle>().Color, 1, 22);
                    break;
                case 1:
                    Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange[0], Config.Item(enemyVisible.ChampionName + "RC").GetValue<Circle>().Color, 1, 22);
                    break;
                case 2:
                    Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.W).SData.CastRange[0], Config.Item(enemyVisible.ChampionName + "RC").GetValue<Circle>().Color, 1, 22);
                    break;
                case 3:
                    Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.E).SData.CastRange[0], Config.Item(enemyVisible.ChampionName + "RC").GetValue<Circle>().Color, 1, 22);
                    break;
                case 4:
                    Utility.DrawCircle(enemyVisible.Position, enemyVisible.Spellbook.GetSpell(SpellSlot.R).SData.CastRange[0], Config.Item(enemyVisible.ChampionName + "RC").GetValue<Circle>().Color, 1, 22);
                    break;
            }
        }
    }

    public static void UpdateIsKillable()
    {
        foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target.IsValidTarget()))
        {
            //Regular
            var totalDamage = ReturnComboDamage(target);

            var newEnemyHealth = ((target.Health - totalDamage) / target.MaxHealth);

            if (newEnemyHealth >= 0.66)
                enemyColor[target.ChampionName] = System.Drawing.Color.Green;
            else if (newEnemyHealth > 0.329 && newEnemyHealth < 0.66)
                enemyColor[target.ChampionName] = System.Drawing.Color.Yellow;
            else if (newEnemyHealth <= 0.39)
                enemyColor[target.ChampionName] = System.Drawing.Color.Red;
        }
    }

    #endregion

}
