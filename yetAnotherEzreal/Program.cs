﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;
using SharpDX;

internal class yetAnotherEzreal
{
	private static Spell _q;
	private static Spell _w;
	private static Spell _e;
	private static Spell _r;


	//Script Information
	private const string VersionNumber = "1.0.0.0";

	//Ease of use
	private static Obj_AI_Hero Player = ObjectManager.Player;
	private static Menu Config;

	private static Orbwalking.Orbwalker _orbwalker;

	private static SpellSlot Ignite;

	private static bool CheckFlag;
	//Items
	///Offensive - minus 25 range
	private static Dictionary<string, Items.Item> OffensiveItems = new Dictionary<string, Items.Item>()
	{
		{"Dfg", new Items.Item(3188, 750)},
		{"Bilgewater", new Items.Item(3144, 475)},
		{"Botrk",new Items.Item(3153, 425)},
		{"Hexgun", new Items.Item(3146, 675)},
		{"RanOmen", new Items.Item(3143, 490)}
	};

	///Defensive - minus 10 range
	private static Items.Item LoTIS = new Items.Item(3190, 590);
	private static Items.Item Zhonya = new Items.Item(3157, 10);

	//Drawing
	private static Vector3 CastPosition;

	//Priority
	private static string[] ChampionPriority =
    {
        "Akali", "Diana", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce", "Kassadin",
        "Kayle", "Kha'Zix", "Lissandra", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Vladimir", "Yasuo",
        "Zilean", "Ahri", "Anivia", "Annie", "Ashe", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
        "Ezreal", "Graves", "Jinx", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "LeBlanc", "Lucian",
        "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon", "Teemo",
        "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath", "Zed",
        "Ziggs","Gangplank","Ryze","Shaco", "Akali", "Diana", "Fiddlesticks", "Fizz"
    };

	private static string[] WPriority =
    {
	"Akali", "Diana", "Fiddlesticks", "Fiora", "Fizz", "Heimerdinger", "Jayce",
        "Kayle", "Kha'Zix", "Mordekaiser", "Nidalee", "Riven", "Shaco", "Yasuo", "Ashe", "Caitlyn", "Corki", "Draven",
        "Graves", "Jinx", "Karthus", "Katarina", "Kennen", "KogMaw", "Lucian",
        "MasterYi", "MissFortune", "Quinn", "Sivir", "Talon", "Teemo",
        "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Zed",
        "Ziggs","Gangplank", "Shaco", "Akali", "Diana", "Fizz"
    };

	private static void Main(string[] args)
	{
		CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
	}

	private static void Game_OnGameLoad(EventArgs args)
	{
		Game.PrintChat("yetAnotherEzreal by FlapperDoodle, version: " + VersionNumber);
		if (ObjectManager.Player.ChampionName != "Ezreal")
		{
			Game.PrintChat("Please use Ezreal~");
			return;
		}

		_q = new Spell(SpellSlot.Q, 1150);
		_q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

		_w = new Spell(SpellSlot.W, 950);
		_w.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

		_e = new Spell(SpellSlot.E, 490);

		_r = new Spell(SpellSlot.R);
		_r.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

		Ignite = Player.GetSpellSlot("SummonerDot");

		//Main Menu
		Config = new Menu("yA-Ezreal", "yA-Ezreal", true);
		//Orbwalker
		Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
		_orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

		//Target Selector
		var targetSelector = new Menu("Target Selector", "Target Selector");
		TargetSelector.AddToMenu(targetSelector);
		Config.AddSubMenu(targetSelector);

		//Combo
		Config.AddSubMenu(new Menu("Combo", "Combo"));
		Config.SubMenu("Combo")
			.AddItem(new MenuItem("Combo-Key", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press))); //Spacebar
		Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Use-Q", "Use Q").SetValue(true));
		Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Use-W", "Use W").SetValue(true));
		Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Use-E-W", "Use E For W Boost").SetValue(true));
		Config.SubMenu("Combo").AddItem(new MenuItem("Combo-Use-R", "Use R").SetValue(true));

		//Farm
		Config.AddSubMenu(new Menu("Farm", "Farm"));
		Config.SubMenu("Farm").AddItem(new MenuItem("Farm-Use-Q", "Use Smart Q for LH").SetValue(true));
		Config.SubMenu("Farm").AddItem(new MenuItem("Farm-Use-Q-Select", "LaneClear/Mixed: Use Q Only On").SetValue(new StringList(new[] { "Off", "Siege/Super", "All" }, 1)));
		Config.SubMenu("Farm").AddItem(new MenuItem("Farm-Mana", "Minimum Mana %").SetValue(new Slider(30)));

		//Killsteal
		Config.AddSubMenu(new Menu("Killsteal", "Killsteal"));
		Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Enabled", "Enabled").SetValue(true));
		Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-Q", "Use Q").SetValue(true));
		Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-W", "Use W").SetValue(true));
		Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-Botrk", "Use Bot Ruined King").SetValue(true));
		Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-Bilgewater", "Use Bilgewater Cutlass").SetValue(true));
		Config.SubMenu("Killsteal").AddItem(new MenuItem("Killsteal-Use-Ignite", "Use Ignite").SetValue(true));
		//>R on Recalling Enemies
		Config.SubMenu("Killsteal").AddSubMenu(new Menu("Use R on Recalling Enemies", "Killsteal-R-Snipe"));
		Config.SubMenu("Killsteal")
			.SubMenu("Killsteal-R-Snipe")
			.AddItem(new MenuItem("Killsteal-R-Snipe-Enabled", "Enabled").SetValue(true));
		Config.SubMenu("Killsteal")
			.SubMenu("Killsteal-R-Snipe")
			.AddItem(new MenuItem("Killsteal-R-Snipe-Range", "Range").SetValue(new Slider(3000, 2750, 4000)));
		Config.SubMenu("Killsteal")
			.SubMenu("Killsteal-R-Snipe")
			.AddItem(
				new MenuItem("Killsteal-R-Snipe-Delay", "Delay before Recall finishes").SetValue(new Slider(900, 750, 1500)));

		//Harass
		Config.AddSubMenu(new Menu("Harass", "Harass"));
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Key", "Harass Key").SetValue(new KeyBind(67, KeyBindType.Press))); // C
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Toggle", "Harass Always On").SetValue(false));
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Use-Q", "Use Q").SetValue(true));
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Use-W", "Use W").SetValue(true));
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Use-E", "Use E For Better Q Location").SetValue(true));
		Config.SubMenu("Harass").AddSubMenu(new Menu("Champions To Harass", "Harass-Champions"));
		Config.SubMenu("Harass").SubMenu("Harass-Champions").AddItem(new MenuItem("Harass-Champions-Disabled", "Harass All Champions").SetValue(false));
		foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
		{
			Config.SubMenu("Harass").SubMenu("Harass-Champions").AddItem(new MenuItem("Harass-" + enemy.ChampionName, enemy.ChampionName)
				.SetValue(ChampionPriority.Contains(enemy.ChampionName)));
		}
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Mana-Q", "Minimum Mana % for Q").SetValue(new Slider(30)));
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Mana-W", "Minimum Mana % for W").SetValue(new Slider(30)));
		Config.SubMenu("Harass").AddItem(new MenuItem("Harass-Mana-E", "Minimum Mana % for E").SetValue(new Slider(40)));

		//Items
		Config.AddSubMenu(new Menu("Items", "Items"));
		Config.SubMenu("Items").AddItem(new MenuItem("Items-Enabled", "Enabled").SetValue(true));
		//'Offensive
		Config.SubMenu("Items").AddSubMenu(new Menu("Offense", "Offense"));
		Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("Bilgewater", "Bilgewater Cutlass").SetValue(true));
		Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("Botrk", "Bot Ruined King").SetValue(true));
		Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("Dfg", "Deathfire Grasp").SetValue(true));
		Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("Hexgun", "Hextech Gunblade").SetValue(true));
		Config.SubMenu("Items").SubMenu("Offense").AddItem(new MenuItem("RanOmen", "Randuin's Omen").SetValue(true));

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
		Config.SubMenu("Misc").AddItem(new MenuItem("Misc-Use-W", "Use W on Ally vs. Turret (WIP)").SetValue(false));

		//Drawing
		Config.AddSubMenu(new Menu("Drawing", "Drawing"));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-Enabled", "Enabled").SetValue(true));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-Q-Range", "Draw Q Range").SetValue(new Circle(true, System.Drawing.Color.DeepPink)));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-Q-Ready", "Draw Q Only When Ready").SetValue(false));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-W-Range", "Draw W Range").SetValue(new Circle(true, System.Drawing.Color.DeepSkyBlue)));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-W-Ready", "Draw W Only When Ready").SetValue(true));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-E-Range", "Draw E Range").SetValue(new Circle(true, System.Drawing.Color.Lime)));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-E-Ready", "Draw E Only When Ready").SetValue(true));
		Config.SubMenu("Drawing").AddItem(new MenuItem("Drawing-E-Position", "Draw E Position").SetValue(true));

		// Map Specific
		if (Utility.Map.GetMap().Type == Utility.Map.MapType.SummonersRift || Utility.Map.GetMap().Type == Utility.Map.MapType.HowlingAbyss)
		{
			OffensiveItems["Dfg"] = new Items.Item(3128, 750);
		}

		Config.AddToMainMenu();

		//Event Handlers
		Game.OnGameUpdate += Game_OnGameUpdate;
		Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;

		//Recall
		Obj_AI_Base.OnTeleport += Obj_AI_Base_OnTeleport;

		//Orbwalking
		Orbwalking.AfterAttack += Orbwalking_AfterAttack;

		//Drawing
		Drawing.OnDraw += Drawing_OnDraw;

	}


	static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
	{
		if (!CheckFlag || sender == null || !sender.IsValid || !sender.IsMe || args.SData.Name != "EzrealEssenceFluxMissile" || !_e.IsReady()) return;

		_e.Cast(Player.Position.Extend(args.End, _e.Range));
		CheckFlag = false;
	}
	static bool ShouldWait()
	{
		return
		ObjectManager.Get<Obj_AI_Minion>()
			.Any(
				minion =>
					minion.IsValidTarget(_q.Range) && minion.Team != GameObjectTeam.Neutral &&
					(Config.Item("Farm-Use-Q-Select").GetValue<StringList>().SelectedIndex == 2 ||
					(Config.Item("Farm-Use-Q-Select").GetValue<StringList>().SelectedIndex == 1 &&
					(minion.SkinName == "SRU_ChaosMinionSiege" | minion.SkinName == "SRU_OrderMinionSiege" |
					minion.SkinName == "SRU_ChaosMinionSuper" | minion.SkinName == "SRU_OrderMinionSuper"))) &&
					HealthPrediction.LaneClearHealthPrediction(
						minion, (int)((Player.AttackDelay * 1000) * 2f), 0) <=
					_q.GetDamage(minion));
	}

	static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
	{
		if (!Config.Item("Farm-Use-Q").GetValue<bool>() || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None) return;

		foreach (var minionGoingToDie in MinionManager.GetMinions(_q.Range).Where(minion => target.NetworkId != minion.NetworkId && _q.IsReady()
			&& HealthPrediction.GetHealthPrediction(minion, (int)((Player.AttackDelay * 1000) * 2.65f + Game.Ping / 2), 0) <= 0 && _q.GetDamage(minion) >= minion.Health))
		{
			var qHit = _q.GetPrediction(minionGoingToDie);
			if (qHit.Hitchance >= HitChance.High)
			{
				_q.Cast(qHit.CastPosition);
				break;
			}
		}
	}


	private static Vector3 rCastVector;
	private static bool rFlag;
	private static int rNetworkID;
	private static float rCastAtTime;

	static void Obj_AI_Base_OnTeleport(GameObject sender, GameObjectTeleportEventArgs args)
	{
		var recallPacket = Packet.S2C.Teleport.Decoded(sender, args);
		if (rFlag || recallPacket.Type != Packet.S2C.Teleport.Type.Recall) return;

		var hero = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recallPacket.UnitNetworkId);

		if (recallPacket.Status == Packet.S2C.Teleport.Status.Start && hero.IsValid &&
			hero.IsValidTarget(Config.Item("Killsteal-R-Snipe-Range").GetValue<Slider>().Value) &&
			_r.GetDamage(hero) > hero.Health + 50)
		{
			rCastVector = hero.Position;
			rNetworkID = recallPacket.UnitNetworkId;
			rCastAtTime = recallPacket.Start + recallPacket.Duration -
						  Config.Item("Killsteal-R-Snipe-Delay").GetValue<Slider>().Value - TravelTime(hero);
			if (rCastAtTime < Environment.TickCount)
			{
				//Can't make it in time
				Console.WriteLine("Too early");
				rFlag = false;
			}
			else
			{
				rFlag = true;
			}
		}
		else if ((recallPacket.Status == Packet.S2C.Teleport.Status.Abort || recallPacket.Status == Packet.S2C.Teleport.Status.Finish)
			&& recallPacket.UnitNetworkId == rNetworkID)
		{
			Console.WriteLine("Abort");
			rFlag = false;
		}
	}

	//How long it takes for ult to get to target
	private static float TravelTime(Obj_AI_Hero target)
	{
		return ((Player.ServerPosition.Distance(target.Position) / _r.Speed + _r.Delay) * 1000);
	}

	private static void Game_OnGameUpdate(EventArgs args)
	{
		if (Player.IsDead) return;

		//Recall Snipe
		if (rFlag && Config.Item("Killsteal-R-Snipe-Enabled").GetValue<bool>())
		{
			if (Environment.TickCount >= rCastAtTime)
			{
				Console.WriteLine("Casted At: " + Environment.TickCount);
				_r.Cast(rCastVector);
				rFlag = false;
			}
		}

		//Defensive Items
		if (Config.Item("LoTIS").GetValue<bool>() && Items.HasItem(LoTIS.Id) && LoTIS.IsReady()
			&& ((Player.Health / Player.MaxHealth) * 100) <= Config.Item("LoTIS-HP-%").GetValue<Slider>().Value)
			Items.UseItem(LoTIS.Id, Player);

		if (Config.Item("Zhonya").GetValue<bool>() && Items.HasItem(Zhonya.Id) && Zhonya.IsReady()
			&& (Player.Health / Player.MaxHealth) * 100 <= Config.Item("Zhonya-HP-%").GetValue<Slider>().Value
			&& (ObjectManager.Get<Obj_AI_Hero>().Any(enemy => enemy.IsEnemy && enemy.IsValidTarget(1600)) || Player.HasBuffOfType(BuffType.Damage) || Player.HasBuffOfType(BuffType.Poison)))
			Items.UseItem(Zhonya.Id, Player);

		if (Config.Item("Drawing-E-Position").GetValue<bool>() && Config.Item("Drawing-Enabled").GetValue<bool>())
			CastPosition = GetELandingPosition();

		if (Config.Item("Farm-Use-Q-Select").GetValue<StringList>().SelectedIndex != 0 && _q.IsReady() &&
			(Player.Mana / Player.MaxMana) * 100 >= Config.Item("Farm-Mana").GetValue<Slider>().Value &&
			(_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear || _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed) && !ShouldWait())
			Farm();

		if (Config.Item("Killsteal-Enabled").GetValue<bool>())
			KillSteal();

		if ((Config.Item("Harass-Key").GetValue<KeyBind>().Active || Config.Item("Harass-Toggle").GetValue<bool>()))
			Harass();

		var target = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Magical, false);

		if (!target.IsValidTarget()) return;

		if (Config.Item("Combo-Key").GetValue<KeyBind>().Active)
			Combo(target);

	}

	private static void Combo(Obj_AI_Hero target)
	{
		if (Config.Item("Combo-Use-Q").GetValue<bool>() && _q.IsReady())
		{
			var qHit = _q.GetPrediction(target);
			if (qHit.Hitchance >= HitChance.VeryHigh)
			{
				_q.Cast(target);
			}
		}

		if (Config.Item("Combo-Use-W").GetValue<bool>() && _w.IsReady() && Player.Distance(target) < _w.Range)
		{
			var wHit = _w.GetPrediction(target);
			if (wHit.Hitchance >= HitChance.VeryHigh)
			{
				_w.Cast(wHit.CastPosition);
				if (Config.Item("Combo-Use-E-W").GetValue<bool>() && _e.IsReady())
				{
					var landingPosition = Player.Position.Extend(wHit.CastPosition, _e.Range);

					if (!landingPosition.UnderTurret(true) && !landingPosition.IsWall() && //Check if landing position is definitely not a wall or a turret
						ObjectManager.Get<Obj_AI_Hero>().Count(enemy => enemy.IsEnemy && enemy.IsValidTarget(600, true, target.Position)) - 1 //The number of enemies, other than the target, near the target
						<= ObjectManager.Get<Obj_AI_Hero>().Count(ally => ally.IsAlly && !ally.IsDead && ally.Distance(target) < 600) && //The number of allies near the target
						(!target.IsMelee() || //If target is range, then just go ahead and land
						 (target.IsMelee() && (landingPosition.Distance(target.Position) + Player.BoundingRadius + 50 > target.AttackRange + target.BoundingRadius)))) //If target is melee, make sure you don't land in its AA range
					{
						CheckFlag = true;
					}
				}
			}
		}


		if (Config.Item("Combo-Use-R").GetValue<bool>() && _r.IsReady())
		{
			var rHit = _r.GetPrediction(target);
			if (rHit.Hitchance >= HitChance.Dashing)
			{
				_r.Cast(target);
			}
		}

		CastOffensiveItems(target);
	}

	private static void CastOffensiveItems(Obj_AI_Hero target)
	{
		foreach (var item in OffensiveItems.Where(item => Config.Item(item.Key).GetValue<bool>() && Items.HasItem(item.Value.Id) && item.Value.IsReady()))
		{
			if (item.Key == "RanOmen" && Player.Distance(target) <= 490)
				Items.UseItem(item.Value.Id, target);
			else
				Items.UseItem(item.Value.Id, target);
		}
	}

	private static void Harass()
	{
		foreach (var enemyInRange in
					ObjectManager.Get<Obj_AI_Hero>()
						.Where(
							enemy =>
								enemy.IsValidTarget(_q.Range) &&
								Config.Item("Harass-" + enemy.ChampionName).GetValue<bool>()))
		{
			var qPred = _q.GetPrediction(enemyInRange);
			var manaPercent = (Player.Mana / Player.MaxMana) * 100;

			if (Config.Item("Harass-Use-Q").GetValue<bool>() && _q.IsReady() && manaPercent >= Config.Item("Harass-Mana-Q").GetValue<Slider>().Value && qPred.Hitchance >= HitChance.VeryHigh)
				_q.Cast(qPred.CastPosition);

			if (Config.Item("Harass-Use-W").GetValue<bool>() && _w.IsReady() && manaPercent >= Config.Item("Harass-Mana-W").GetValue<Slider>().Value && Player.Distance(enemyInRange) < _w.Range)
				_w.Cast(enemyInRange);

			if (!_e.IsReady() || !_q.IsReady() || qPred.Hitchance > HitChance.Impossible || Player.Distance(enemyInRange) > _q.Range ||
				Player.Mana <= (Player.Spellbook.GetSpell(SpellSlot.E).ManaCost + Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost) ||
				!Config.Item("Harass-Use-E").GetValue<bool>() ||
				manaPercent < Config.Item("Harass-Mana-E").GetValue<Slider>().Value) return;

			var c = new Geometry.Circle(Player.Position.To2D(), 490);
			var point = c.ToPolygon().Points.OrderByDescending(vector2 => vector2.Distance(enemyInRange.Position.To2D())).Reverse();

			for (var i = 1; i < 3; i++)
			{
				var pointTo = point.ElementAt(i).To3D();
				if (pointTo.IsWall() || pointTo.UnderTurret(true) ||
					ObjectManager.Get<Obj_AI_Hero>().Any(enemy => enemy != enemyInRange && enemy.IsValidTarget(300, true, pointTo))) continue;

				var qNewPred = new PredictionInput
				{
					Unit = enemyInRange,
					Radius = _q.Width,
					From = pointTo,
					Aoe = false,
					Delay = 0.5f,
					Range = _q.Range,
					Speed = _q.Speed,
					Type = SkillshotType.SkillshotLine,
					Collision = true
				};

				var qHit = Prediction.GetPrediction(qNewPred);

				if (qHit.Hitchance < HitChance.VeryHigh || !_e.IsReady() || !_q.IsReady()) continue;

				_e.Cast(pointTo);
				break;
			}
		}
	}

	private static void Farm()
	{
		foreach (var enemyMinion in MinionManager.GetMinions(_q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth)
			.Where(minion => minion.IsValidTarget(_q.Range) &&
			(Config.Item("Farm-Use-Q-Select").GetValue<StringList>().SelectedIndex == 2 ||
			(Config.Item("Farm-Use-Q-Select").GetValue<StringList>().SelectedIndex == 1 &&
			(minion.SkinName == "SRU_ChaosMinionSiege" | minion.SkinName == "SRU_OrderMinionSiege" |
					minion.SkinName == "SRU_ChaosMinionSuper" | minion.SkinName == "SRU_OrderMinionSuper")))))
		{
			var qHit = _q.GetPrediction(enemyMinion);
			if (qHit.Hitchance >= HitChance.High)
			{
				_q.Cast(qHit.CastPosition);
				break;
			}
		}
	}

	private static void KillSteal()
	{
		foreach (var enemyInRange in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(_q.Range)))
		{
			var qDamage = _q.GetDamage(enemyInRange);
			var wDamage = _w.GetDamage(enemyInRange);

			if (Config.Item("Killsteal-Use-Q").GetValue<bool>() && _q.IsReady() &&
				qDamage >= enemyInRange.Health)
			{
				var qHit = _q.GetPrediction(enemyInRange);
				if (qHit.Hitchance >= HitChance.VeryHigh)
					_q.Cast(enemyInRange);
			}
			else if (Config.Item("Killsteal-Use-W").GetValue<bool>() && _w.IsReady() && Player.Distance(enemyInRange) < _w.Range &&
					 wDamage >= enemyInRange.Health)
			{
				var wHit = _w.GetPrediction(enemyInRange);
				if (wHit.Hitchance >= HitChance.VeryHigh)
					_w.Cast(enemyInRange);
			}
			else if (Config.Item("Killsteal-Use-Bilgewater").GetValue<bool>() && Items.HasItem(OffensiveItems["Bilgewater"].Id) && OffensiveItems["Bilgewater"].IsReady() &&
					 Player.GetItemDamage(enemyInRange, Damage.DamageItems.Bilgewater) > enemyInRange.Health)
			{
				Items.UseItem(OffensiveItems["Bilgewater"].Id, enemyInRange);
			}
			else if (Config.Item("Killsteal-Use-Botrk").GetValue<bool>() && Items.HasItem(OffensiveItems["Botrk"].Id) && OffensiveItems["Botrk"].IsReady() &&
					 Player.GetItemDamage(enemyInRange, Damage.DamageItems.Botrk) > enemyInRange.Health)
			{
				Items.UseItem(OffensiveItems["Botrk"].Id, enemyInRange);
			}
			else if (Config.Item("Killsteal-Use-Q").GetValue<bool>() && Config.Item("Killsteal-Use-W").GetValue<bool>() &&
					 _q.IsReady() && _w.IsReady() && Player.Distance(enemyInRange) < _w.Range &&
					 (qDamage + wDamage > enemyInRange.Health))
			{
				var wHit = _w.GetPrediction(enemyInRange);
				var qHit = _q.GetPrediction(enemyInRange);

				if (wHit.Hitchance < HitChance.VeryHigh || qHit.Hitchance < HitChance.VeryHigh) continue;
				_q.Cast(enemyInRange);
				_w.Cast(enemyInRange);
			}
			else if (Config.Item("Killsteal-Use-Ignite").GetValue<bool>() && Ignite != SpellSlot.Unknown &&
					 Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready && Player.GetSummonerSpellDamage(enemyInRange, Damage.SummonerSpell.Ignite) > enemyInRange.Health)
			{
				Player.Spellbook.CastSpell(Ignite, enemyInRange);
			}
		}
	}

	static void Drawing_OnDraw(EventArgs args)
	{
		if (Player.IsDead || !Config.Item("Drawing-Enabled").GetValue<bool>())
			return;

		if (Config.Item("Drawing-E-Position").GetValue<bool>())
			DrawE();

		if (Config.Item("Drawing-Q-Range").GetValue<Circle>().Active || (Config.Item("Drawing-Q-Ready").GetValue<bool>() && _q.IsReady()))
			Utility.DrawCircle(Player.Position, _q.Range, Config.Item("Drawing-Q-Range").GetValue<Circle>().Color, 7, 20);

		if (Config.Item("Drawing-W-Range").GetValue<Circle>().Active || (Config.Item("Drawing-W-Ready").GetValue<bool>() && _w.IsReady()))
			Utility.DrawCircle(Player.Position, _w.Range, Config.Item("Drawing-W-Range").GetValue<Circle>().Color, 7, 20);

		if (Config.Item("Drawing-E-Range").GetValue<Circle>().Active || (Config.Item("Drawing-E-Ready").GetValue<bool>() && _e.IsReady()))
			Utility.DrawCircle(Player.Position, _e.Range, Config.Item("Drawing-E-Range").GetValue<Circle>().Color, 7, 20);

	}


	//Credits to Honda (I think)
	private static Vector3 GetELandingPosition()
	{
		Vector3 castPoint = Player.Distance(Game.CursorPos) <= _e.Range
			? Game.CursorPos
			: Player.Position.Extend(Game.CursorPos, _e.Range);

		for (var i = 0; i < 500; i += 9)
			for (double j = 0; j < 2 * Math.PI + 0.2; j += 0.2)
			{
				var c = new Vector3((castPoint.X + i * (float)Math.Cos(j)), castPoint.Y + i * (float)Math.Sin(j), castPoint.Z);
				if (!c.IsWall())
					return c;
			}

		return castPoint;
	}

	private static void DrawE()
	{
		var myPos = Player.Position;
		var direction = (CastPosition - myPos);
		var directionPerpendicular = new Vector3(-direction.Y, direction.Z, direction.X).Normalized();

		var readyColor = _e.IsReady() ? System.Drawing.Color.Green : System.Drawing.Color.Red;

		Drawing.DrawLine(Drawing.WorldToScreen(myPos + directionPerpendicular), Drawing.WorldToScreen(CastPosition), 2, readyColor);
		Utility.DrawCircle(CastPosition, 50, readyColor, 5, 15);
	}

	private static double ComboDamage(Obj_AI_Hero target)
	{
		return Player.GetAutoAttackDamage(target) * 2 + OffensiveItems.Where(ranomen => ranomen.Key != "RanOmen").Sum(item => (item.Value.IsOwned() && item.Value.IsReady()) ? Player.GetItemDamage(target, (Damage.DamageItems)Enum.Parse(typeof(Damage.DamageItems), item.Key)) : 0);
	}
}

