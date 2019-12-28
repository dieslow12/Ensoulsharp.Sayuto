using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keys = System.Windows.Forms.Keys;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Utility = EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using SharpDX;
using EnsoulSharp.SDK.Utility;
using Color = System.Drawing.Color;
using SPrediction;
using DaoHungAIO.Helpers;

namespace DaoHungAIO.Champions
{
    internal class Azir
    {
        private Spell Q, Q2, W, W2, E, R, R2;
        private List<Spell> SpellList = new List<Spell>();
        private Menu menu;
        private AIHeroClient Player = ObjectManager.Player;
        public Azir()
        {
            LoadSpells();
            LoadMenu();
            Game.OnUpdate += Game_OnGameUpdateEvent;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnInterrupterSpell += Interrupter_OnPosibleToInterruptEvent;
            Gapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloserEvent;
            //GameObject.OnCreate += GameObject_OnCreateEvent;
            //AIBaseClient.OnProcessSpellCast += AIBaseClient_OnProcessSpellCastEvent;
            //GameObject.OnDelete += GameObject_OnDeleteEvent;
            //AIBaseClient.OnIssueOrder += ObjAiHeroOnOnIssueOrderEvent;
            //Spellbook.OnUpdateChargedSpell += Spellbook_OnUpdateChargedSpellEvent;
            //Spellbook.OnCastSpell += SpellbookOnOnCastSpell;
            //Spellbook.OnStopCast += SpellbookOnOnStopCast;
            AIHeroClient.OnProcessSpellCast += AIBaseClient_OnProcessSpellCast;
            Orbwalker.OnAction += OnAttack;
        }

        private static AIHeroClient _insecTarget;
        private Vector3 _rVec;


        private void AntiGapcloser_OnEnemyGapcloserEvent(
    AIHeroClient sender,
    Gapcloser.GapcloserArgs args
)
        {
            AntiGapcloser_OnEnemyGapcloser(sender, args);
        }



        private void Interrupter_OnPosibleToInterruptEvent(
    AIHeroClient sender,
    Interrupter.InterruptSpellArgs args
)
        {
            Interrupter_OnPosibleToInterrupt(sender, args);
        }

 

        private void Game_OnGameUpdateEvent(EventArgs args)
        {
            /*
            if (LagManager.Enabled && Player.ChampionName.ToLower() != "azir" && Player.ChampionName.ToLower() != "lucian")
                if (!LagManager.ReadyState)
                    return;*/

            //check if player is dead
            if (Player.IsDead && Player.CharacterName.ToLower() != "karthus") return;

            Game_OnGameUpdate(args);
        }



//        private void GameObject_OnCreateEvent(
//    GameObject sender,
//    EventArgs args
//)
//        {
//            GameObject_OnCreate(sender, args);
//        }



//        private void GameObject_OnDeleteEvent(GameObject sender, EventArgs args)
//        {
//            GameObject_OnDelete(sender, args);
//        }


        //private void Obj_AI_Base_OnProcessSpellCastEvent(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        //{
        //    Obj_AI_Base_OnProcessSpellCast(unit, args);
        //}


        //private void AfterAttackEvent(AttackableUnit unit, AttackableUnit target)
        //{
        //    AfterAttack(unit, target);
        //}



        //private void BeforeAttackEvent(xSaliceWalker.BeforeAttackEventArgs args)
        //{
        //    BeforeAttack(args);
        //}


        //private void BeforeAttackEvent(Orbwalking.BeforeAttackEventArgs args)
        //{
        //    BeforeAttack(args);
        //}


        //private void ObjAiHeroOnOnIssueOrderEvent(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        //{
        //    ObjAiHeroOnOnIssueOrder(sender, args);
        //}


        //private void Spellbook_OnUpdateChargedSpellEvent(Spellbook sender, SpellbookUpdateChargedSpellEventArgs args)
        //{
        //    Spellbook_OnUpdateChargedSpell(sender, args);
        //}





        private void LoadSpells()
        {
            //intalize spell
            Q = new Spell(SpellSlot.Q, 950);
            Q2 = new Spell(SpellSlot.Q, 2000);
            W = new Spell(SpellSlot.W, 450);
            W2 = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 2000);
            R = new Spell(SpellSlot.R, 450);
            R2 = new Spell(SpellSlot.R);

            Q.SetSkillshot(0, 80, 1600, false, SkillshotType.Circle);
            Q2.SetSkillshot(0, 80, 1600, false, SkillshotType.Circle);
            E.SetSkillshot(0.25f, 100, 1200, false, SkillshotType.Line);
            R.SetSkillshot(0.5f, 700, 1400, false, SkillshotType.Line);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

        }

        private void LoadMenu()
        {
            menu = new Menu("Azir", "DH.Azir", true);
            var key = new Menu("Key", "Key");
            {
                key.Add(new MenuKeyBind("ComboActive", "Combo!", Keys.Space, KeyBindType.Press));
                key.Add(new MenuKeyBind("HarassActive", "Harass!", Keys.C, KeyBindType.Press));
                key.Add(new MenuKeyBind("HarassActiveT", "Harass (toggle)!", Keys.N, KeyBindType.Toggle));
                key.Add(new MenuKeyBind("LaneClearActive", "Farm!", Keys.V, KeyBindType.Press));
                key.Add(new MenuKeyBind("escape", "Escape", Keys.Z, KeyBindType.Press));
                key.Add(new MenuKeyBind("insec", "Insec Selected target", Keys.J, KeyBindType.Press));
                key.Add(new MenuKeyBind("qeCombo", "Q->E stun Nearest target", Keys.V, KeyBindType.Press));
                key.Add(new MenuKeyBind("qMulti", "Q if 2+ Soilder", Keys.I, KeyBindType.Toggle));
                //add to menu
                menu.Add(key);
            }

            //Spell Menu
            var spell = new Menu("Spell", "Spell");
            {

                var qMenu = new Menu("QSpell", "QSpell");
                {
                    qMenu.Add(new MenuBool("qOutRange", "Only Use When target out of range"));
                    spell.Add(qMenu);
                }
                //W Menu
                var wMenu = new Menu("WSpell", "WSpell");
                {
                    wMenu.Add(new MenuBool("wAtk", "Always Atk Enemy"));
                    spell.Add(wMenu);
                }
                //E Menu
                var eMenu = new Menu("ESpell", "ESpell");
                {
                    eMenu.Add(new MenuBool("eKill", "If Killable Combo").SetValue(false));
                    eMenu.Add(new MenuBool("eKnock", "Always Knockup/DMG").SetValue(false));
                    eMenu.Add(new MenuSlider("eHP", "if HP >").SetValue(new Slider(100)));
                    spell.Add(eMenu);
                }
                //R Menu
                var rMenu = new Menu("RSpell", "RSpell");
                {
                    rMenu.Add(new MenuSlider("rHP", "if HP <").SetValue(new Slider(20)));
                    rMenu.Add(new MenuBool("rWall", "R Enemy Into Wall"));
                    spell.Add(rMenu);
                }
                menu.Add(spell);
            }

            //Combo menu:
            var combo = new Menu("Combo", "Combo");
            {
                combo.Add(new MenuBool("UseQCombo", "Use Q"));
                combo.Add(new MenuBool("UseWCombo", "Use W"));
                combo.Add(new MenuBool("UseECombo", "Use E"));
                combo.Add(new MenuBool("UseRCombo", "Use R"));
                //combo.Add(HitChanceManager.AddHitChanceMenuCombo(true, false, false, false));
                menu.Add(combo);
            }

            //Harass menu:
            var harass = new Menu("Harass", "Harass");
            {
                harass.Add(new MenuBool("UseQHarass", "Use Q"));
                harass.Add(new MenuBool("UseWHarass", "Use W"));
                harass.Add(new MenuBool("UseEHarass", "Use E").SetValue(false));
                harass.Add(new MenuSlider("manaHarass", "Mana Min").SetValue(new Slider(60)));
                //harass.Add(HitChanceManager.AddHitChanceMenuHarass(true, false, false, false));
                //ManaManager.AddManaManagertoMenu(harass, "Harass", 60);
                menu.Add(harass);
            }

            //killsteal
            var killSteal = new Menu("KillSteal", "KillSteal");
            {
                killSteal.Add(new MenuBool("smartKS", "Use Smart KS System"));
                killSteal.Add(new MenuBool("eKS", "Use E KS").SetValue(false));
                killSteal.Add(new MenuBool("wqKS", "Use WQ KS"));
                killSteal.Add(new MenuBool("qeKS", "Use WQE KS").SetValue(false));
                killSteal.Add(new MenuBool("rKS", "Use R KS"));
                menu.Add(killSteal);
            }

            //farm menu
            var farm = new Menu("Farm", "Farm");
            {
                farm.Add(new MenuBool("UseQFarm", "Use Q").SetValue(false));
                farm.Add(new MenuSlider("qFarm", "Only Q if > minion").SetValue(new Slider(3, 0, 5)));
                farm.Add(new MenuSlider("manaFarm", "Mana Min").SetValue(new Slider(60)));
                //ManaManager.AddManaManagertoMenu(farm, "Farm", 50);
                menu.Add(farm);
            }

            //Misc Menu:
            var misc = new Menu("Misc", "Misc");
            {
                //misc.Add(AoeAddHitChanceMenuCombo(false, false, false, true));
                misc.Add(new MenuBool("UseInt", "Use E to Interrupt"));
                misc.Add(new MenuBool("UseGap", "Use R for GapCloser"));
                //misc.Add(new MenuSlider("escapeDelay", "Escape Delay Decrease").SetValue(new Slider(0, 0, 300)));
                menu.Add(misc);
            }

            //Drawings menu:
            var draw = new Menu("Drawings", "Drawings");
            {
                draw.Add(new MenuBool("QRange", "Q range"));
                draw.Add(new MenuBool("WRange", "W range"));
                draw.Add(new MenuBool("ERange", "E range"));
                draw.Add(new MenuBool("RRange", "R range"));             

                menu.Add(draw);
            }
            var credit = new Menu("Credits", "Credits");
            credit.Add(new Menu("xSalice", "xSalice"));
            credit.Add(new Menu("Kortaru", "Kortaru"));
            menu.Add(credit);
            menu.Attach();
        }

        private float GetComboDamage(AIBaseClient enemy)
        {
            if (enemy == null)
                return 0;

            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (soilderCount() > 0 || W.IsReady())
            {
                //damage += AzirManager.GetAzirAaSandwarriorDamage(enemy);
            }

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            //damage = ItemManager.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<MenuBool>(), menu.Item("UseWCombo").GetValue<MenuBool>(),
                menu.Item("UseECombo").GetValue<MenuBool>(), menu.Item("UseRCombo").GetValue<MenuBool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<MenuBool>(), menu.Item("UseWHarass").GetValue<MenuBool>(),
                menu.Item("UseEHarass").GetValue<MenuBool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !(menu.Item("manaHarass").GetValue<MenuSlider>() < Player.ManaPercent))
                return;

            var qTarget = TargetSelector.GetTarget(Q.Range);
            var soilderTarget = TargetSelector.GetTarget(1200);
            var dmg = GetComboDamage(soilderTarget);

            if (soilderTarget == null || qTarget == null)
                return;

            //R
            if (useR && R.IsReady() && ShouldR(qTarget) && Player.Distance(qTarget.Position) < R.Range)
                R.Cast(qTarget);

            //W
            if (useW && W.IsReady() && useQ)
            {
                CastW(qTarget);
            }

            //Q
            if (useQ && Q.IsReady())
            {
                CastQ(qTarget, source);
                return;
            }

            //items
            //if (source == "Combo")
            //{
            //    ItemManager.Target = soilderTarget;

            //    //see if killable
            //    if (dmg > soilderTarget.Health - 50)
            //        ItemManager.KillableTarget = true;

            //    ItemManager.UseTargetted = true;

            //}

            //E
            if (useE && (E.IsReady()))
            {
                CastE(soilderTarget);
            }
        }

        private bool WallStun(AIHeroClient target)
        {
            var pushedPos = R.GetPrediction(target).UnitPosition;

            if (Util.IsPassWall(Player.Position, pushedPos))
                return true;

            return false;
        }

        private void SmartKs()
        {
            if (!menu.Item("smartKS").GetValue<MenuBool>())
                return;

            foreach (AIHeroClient target in ObjectManager.Get<AIHeroClient>().Where(x => x.IsValidTarget(1200) && !x.HasBuffOfType(BuffType.Invulnerability)).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    //R
                    if ((Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 20 && Player.Distance(target.Position) < R.Range && menu.Item("rKS").GetValue<MenuBool>())
                    {
                        R.Cast(target);
                    }

                    if (soilderCount() < 1 && !W.IsReady())
                        return;

                    //WQ
                    if ((Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health + 20 && menu.Item("wqKS").GetValue<MenuBool>())
                    {
                        CastW(target);
                    }

                    //qe
                    if ((Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.E)) > target.Health + 20 && Player.Distance(target.Position) < Q.Range && menu.Item("qeKS").GetValue<MenuBool>())
                    {
                        CastQe(target, "Null");
                    }

                }
            }
        }

        private void AIBaseClient_OnProcessSpellCast(AIBaseClient unit, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!unit.IsMe)
                return;

            if (args.SData.Name == "AzirQ")
            {
                Q.LastCastAttemptT = Variables.TickCount + 250;
                _rVec = Player.Position;
            }

            if (args.SData.Name == "AzirE" && (Q.IsReady()))
            {
                if (Variables.TickCount - E.LastCastAttemptT < 0)
                {
                    Q2.Cast(Game.CursorPos);
                }
            }
        }

        private void Escape()
        {
            Vector3 wVec = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * W.Range;

            if ((E.IsReady()))
            {
                if (soilderCount() < 1 && W.IsReady())
                    W.Cast(wVec);
                else if (soilderCount() < 1 && !W.IsReady())
                    return;

                if (GetNearestSoilderToMouse() == null)
                    return;

                var nearSlave = GetNearestSoilderToMouse();

                if ((E.IsReady()) &&
                    Player.Distance(Game.CursorPos) > Game.CursorPos.Distance(nearSlave.Position))
                {
                    E.Cast(nearSlave.Position);
                    E.LastCastAttemptT = Variables.TickCount + 250;
                }
                else if (W.IsReady())
                {
                    W.Cast(wVec);
                }
            }

        }

        private GameObject GetNearestSoilderToMouse()
        {
            var soilder = AzirManager.AllSoldiers.OrderBy(x => Game.CursorPos.Distance(x.Position));

            if (soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private void CastQe(AIHeroClient target, string source)
        {
            if (target == null)
                return;

            if (W.IsReady())
            {
                Vector3 wVec = Player.Position + Vector3.Normalize(target.Position - Player.Position) * W.Range;

                var qPred = Util.GetP(wVec, Q, target, W.Delay + Q.Delay, true);

                if ((Q.IsReady()) && (E.IsReady()) && Player.Distance(target.Position) < Q.Range - 75 && qPred.Hitchance >= Q.MinHitChance)
                {
                    var vec = target.Position - Player.Position;
                    var castBehind = qPred.CastPosition + Vector3.Normalize(vec) * 75;

                    W.Cast(wVec);
                    Utility.DelayAction.Add((int)W.Delay + 100, () => Q2.Cast(castBehind));
                    Utility.DelayAction.Add((int)(W.Delay + Q.Delay) + 100, () => E.Cast(castBehind));
                }
            }

            if(source == "insec")
            {
                if (!target.IsValidTarget(R.Range)){
                    return;
                }
                var turret = GameObjects.AllyTurrets.Where(t => t.Distance(target) <= t.GetRealAutoAttackRange() + 150 + 450).FirstOrDefault();
                var allys = GameObjects.AllyHeroes.Where(a => a.DistanceToPlayer() > a.Distance(target)).OrderByDescending(a => a.Health);
                var allyHealthTop = allys.FirstOrDefault();

                if(allyHealthTop != null)
                {
                    if (turret == null)
                    {
                        R.Cast(allyHealthTop.Position);
                    } else
                    {
                        var allyUnderTurret = allys.Where(a => a.UnderAllyTurret()).OrderByDescending(a => a.Health).FirstOrDefault();
                        if(allyUnderTurret != null)
                        {
                            R.Cast(allyUnderTurret.Position);
                        } else
                        {
                            R.Cast(turret.Position);
                        }
                    }
                } else if(turret != null)
                {
                    R.Cast(turret.Position);
                }
            }
        }

        private void Insec()
        {
            var target = _insecTarget;

            if (target == null)
                return;

            CastQe(target, "insec");
        }

        private void CastW(AIHeroClient target)
        {
            if (target == null || Player.Distance(Prediction.GetFastUnitPosition(target, W.Delay)) < W2.Range)
                return;

            if (Q.IsReady())
            {
                W.Cast(Player.Position.ToVector2().Extend(target.Position.ToVector2(), W.Range));
            }
        }

        private void OnAttack(
    Object sender,
    OrbwalkerActionArgs args
) //(AttackableUnit unit, AttackableUnit target)
        {
            if (!menu.Item("ComboActive").GetValue<MenuKeyBind>().Active || !W.IsReady())
                return;
            if(args.Type != OrbwalkerType.OnAttack)
            {
                return;
            }
            var target = args.Target;
            var unit = sender;
            if (target == null || unit == null)
                return;

            if (unit is AIHeroClient && target is AIBaseClient)
            {
                if (Player.Distance(Prediction.GetFastUnitPosition((AIHeroClient)target, W.Delay)) <
                    W2.Range)
                    W.Cast(Prediction.GetFastUnitPosition((AIHeroClient)target, W.Delay));
            }
        }

        private void CastQ(AIHeroClient target, string source)
        {
            if (soilderCount() < 1)
                return;

            var slaves = AzirManager.AllSoldiers.ToList();

            foreach (var slave in slaves)
            {
                if (Player.Distance(target.Position) < Q.Range && ShouldQ(target, slave))
                {

                    Q.UpdateSourcePosition(slave.Position, Player.Position);
                    var qPred = Q.GetPrediction(target);

                    if (Q.IsReady() && Player.Distance(target.Position) < Q.Range && qPred.Hitchance >= Q.MinHitChance)
                    {
                        Q.Cast(qPred.CastPosition);
                        return;
                    }
                }
            }
        }

        private void CastE(AIHeroClient target)
        {
            if (soilderCount() < 1)
                return;

            var slaves = AzirManager.AllSoldiers.ToList();

            foreach (var slave in slaves)
            {
                if (target != null && Player.Distance(slave.Position) < E.Range)
                {
                    var ePred = E.GetPrediction(target);
                    Object[] obj = Util.VectorPointProjectionOnLineSegment(Player.Position.ToVector2(), slave.Position.ToVector2(), ePred.UnitPosition.ToVector2());
                    var isOnseg = (bool)obj[2];
                    var pointLine = (Vector2)obj[1];

                    if (E.IsReady() && isOnseg && pointLine.Distance(ePred.UnitPosition.ToVector2()) < E.Width && ShouldE(target))
                    {
                        E.Cast(slave.Position);
                        return;
                    }
                }
            }
        }

        private bool ShouldQ(AIHeroClient target, GameObject slave)
        {

            if (soilderCount() < 2 && menu.Item("qMulti").GetValue<MenuKeyBind>().Active)
                return false;

            if (!menu.Item("qOutRange").GetValue<MenuBool>())
                return true;

            var slaves = AzirManager.AllSoldiers.Where(s => s.Distance(target) < 250).FirstOrDefault();
            if (slaves == null)
                return true;

            if (!target.InAutoAttackRange())
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.Q) > target.Health + 10)
                return true;


            return false;
        }
        private bool ShouldE(AIHeroClient target)
        {
            if (menu.Item("eKnock").GetValue<MenuBool>())
                return true;

            if (menu.Item("eKill").GetValue<MenuBool>() && GetComboDamage(target) > target.Health + 15)
                return true;

            if (menu.Item("eKS").GetValue<MenuBool>() && Player.GetSpellDamage(target, SpellSlot.E) > target.Health + 10)
                return true;

            //hp 
            var hp = menu.Item("eHP").GetValue<MenuSlider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (hpPercent > hp)
                return true;

            return false;
        }

        private bool ShouldR(AIHeroClient target)
        {
            if (Player.GetSpellDamage(target, SpellSlot.R) > target.Health - 150)
                return true;

            var hp = menu.Item("rHP").GetValue<MenuSlider>().Value;
            if (Player.HealthPercent < hp)
                return true;

            if (WallStun(target) && GetComboDamage(target) > target.Health / 2 && menu.Item("rWall").GetValue<MenuBool>())
            {
                return true;
            }

            return false;
        }

        private void AutoAtk()
        {
            if (soilderCount() < 1)
                return;

            var soilderTarget = TargetSelector.GetTarget(1000);

            if (soilderTarget == null)
                return;

            AttackTarget(soilderTarget);
        }

        private int soilderCount()
        {
            return AzirManager.AllSoldiers.Count();
        }


        private void AttackTarget(AIHeroClient target)
        {
            if (soilderCount() < 1)
                return;

            var tar = getNearestSoilderToEnemy(target);
            if (tar != null && Player.Distance(tar.Position) < 800)
            {
                if (target != null && target.Distance(tar.Position) <= 350)
                {
                    Orbwalker.Orbwalk(target, Game.CursorPos);
                }
            }

        }

        private GameObject getNearestSoilderToEnemy(AIBaseClient target)
        {
            var soilder = AzirManager.AllSoldiers.ToList().OrderBy(x => target.Distance(x.Position));

            if (soilder.FirstOrDefault() != null)
                return soilder.FirstOrDefault();

            return null;
        }

        private void Farm()
        {
            if (!(menu.Item("manaFarm").GetValue<MenuSlider>() < Player.ManaPercent))
                return;

            var allMinionsQ = GameObjects.GetMinions(Player.Position, Q.Range + Q.Width);
            var allMinionsW = GameObjects.GetMinions(Player.Position, W.Range);

            var useQ = menu.Item("UseQFarm").GetValue<MenuBool>();
            var min = menu.Item("qFarm").GetValue<MenuSlider>().Value;


            if (useQ && (Q.IsReady()))
            {
                int hit;
                if (soilderCount() > 0)
                {
                    var slaves = AzirManager.AllSoldiers.ToList();
                    foreach (var slave in slaves)
                    {
                        foreach (var enemy in allMinionsQ)
                        {
                            hit = 0;
                            Q.UpdateSourcePosition(slave.Position, Player.Position);
                            var prediction = Q.GetPrediction(enemy);

                            if (Q.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                            {
                                hit += allMinionsQ.Count(enemy2 => enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady());
                                if (hit >= min)
                                {
                                    if (Q.IsReady())
                                    {
                                        Q.Cast(prediction.CastPosition);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                if (W.IsReady())
                {
                    var wpred = W.GetCircularFarmLocation(allMinionsW);
                    if (wpred.MinionsHit > 0)
                        W.Cast(wpred.Position);

                    foreach (var enemy in allMinionsQ)
                    {
                        hit = 0;
                        Q.UpdateSourcePosition(Player.Position, Player.Position);
                        var prediction = Q.GetPrediction(enemy);

                        if (Q.IsReady() && Player.Distance(enemy.Position) <= Q.Range)
                        {
                            hit += allMinionsQ.Count(enemy2 => enemy2.Distance(prediction.CastPosition) < 200 && Q.IsReady());
                            if (hit >= min)
                            {
                                if (Q.IsReady())
                                {
                                    Q.Cast(prediction.CastPosition);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            SmartKs();

            if (menu.Item("escape").GetValue<MenuKeyBind>().Active)
            {
                Orbwalker.Orbwalk(null, Game.CursorPos);
                Escape();
            }
            else if (menu.Item("ComboActive").GetValue<MenuKeyBind>().Active)
            {
                Combo();
            }
            else if (menu.Item("insec").GetValue<MenuKeyBind>().Active)
            {
                Orbwalker.Orbwalk(null, Game.CursorPos);

                _insecTarget = TargetSelector.SelectedTarget;

                if (_insecTarget != null)
                {
                    if (_insecTarget.HasBuffOfType(BuffType.Knockup) || _insecTarget.HasBuffOfType(BuffType.Knockback))
                        if (Player.Distance(_insecTarget) < 200)
                            R2.Cast(_rVec);

                    Insec();
                }
            }
            else if (menu.Item("qeCombo").GetValue<MenuKeyBind>().Active)
            {
                var soilderTarget = TargetSelector.GetTarget(900);

                Orbwalker.Orbwalk(null, Game.CursorPos);
                CastQe(soilderTarget, "Null");
            }
            else
            {
                if (menu.Item("LaneClearActive").GetValue<MenuKeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("HarassActive").GetValue<MenuKeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT").GetValue<MenuKeyBind>().Active)
                    Harass();

                if (menu.Item("wAtk").GetValue<MenuBool>())
                    AutoAtk();
            }
                
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<MenuBool>();
                if (menuItem.Enabled)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, System.Drawing.Color.Red, 1);
            }
            if (menu.Item("QRange").GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red, 1);
        }

        private void AntiGapcloser_OnEnemyGapcloser(
    AIHeroClient sender,
    Gapcloser.GapcloserArgs args
)
        {
            if (!menu.Item("UseGap").GetValue<MenuBool>()) return;

            if (R.IsReady() && sender.IsValidTarget(R.Range))
                R.Cast(sender);
        }

        private void Interrupter_OnPosibleToInterrupt(
    AIHeroClient sender,
    Interrupter.InterruptSpellArgs args
)
        {
            if (!menu.Item("UseInt").GetValue<MenuBool>()) return;

            if (Player.Distance(sender.Position) < R.Range && R.IsReady())
            {
                R.Cast(sender);
            }
        }

        //private void GameObject_OnCreate(GameObject sender, EventArgs args)
        //{
        //    AzirManager.Obj_OnCreate(sender, args);
        //}

        //private void GameObject_OnDelete(GameObject sender, EventArgs args)
        //{
        //    AzirManager.OnDelete(sender, args);
        //}
    }
    internal static class AzirManager
    {
        private static List<AIMinionClient> _soldiers = new List<AIMinionClient>();
        private static Dictionary<int, string> Animations = new Dictionary<int, string>();
        private const bool DrawSoldiers = true;

        public static List<AIMinionClient> ActiveSoldiers
        {
            get { return _soldiers.Where(s => s.IsValid && !s.IsDead && !s.IsMoving && (!Animations.ContainsKey((int)s.NetworkId) || Animations[(int)s.NetworkId] != "Inactive")).ToList(); }
        }

        public static List<AIMinionClient> AllSoldiers2
        {
            get { return _soldiers.Where(s => s.IsValid && !s.IsDead).ToList(); }
        }

        public static List<AIMinionClient> AllSoldiers
        {
            get { return _soldiers.Where(s => s.IsValid && !s.IsDead && !s.IsMoving).ToList(); }
        }

        static AzirManager()
        {
            AIMinionClient.OnCreate += AIMinionClient_OnCreate;
            AIMinionClient.OnDelete += AIMinionClient_OnDelete;
            AIMinionClient.OnPlayAnimation += AIMinionClient_OnPlayAnimation;

            if (DrawSoldiers)
            {
                Drawing.OnDraw += Drawing_OnDraw;
            }
        }

        static void AIMinionClient_OnPlayAnimation(GameObject sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (sender is AIMinionClient && ((AIMinionClient)sender).IsSoldier())
            {
                Animations[(int)sender.NetworkId] = args.Animation;
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var soldier in ActiveSoldiers)
            {
                Render.Circle.DrawCircle(soldier.Position, 320, System.Drawing.Color.FromArgb(150, System.Drawing.Color.Yellow));
            }
        }

        private static bool IsSoldier(this AIMinionClient soldier)
        {
            return soldier.IsAlly && String.Equals(soldier.CharacterName, "azirsoldier", StringComparison.InvariantCultureIgnoreCase);
        }

        static void AIMinionClient_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender is AIMinionClient && ((AIMinionClient)sender).IsSoldier())
            {
                _soldiers.Add((AIMinionClient)sender);
            }
        }

        static void AIMinionClient_OnDelete(GameObject sender, EventArgs args)
        {
            _soldiers.RemoveAll(s => s.NetworkId == sender.NetworkId);
            Animations.Remove((int)sender.NetworkId);
        }
    }
    }