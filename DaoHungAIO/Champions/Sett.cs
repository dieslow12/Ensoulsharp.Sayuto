using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using static EnsoulSharp.SDK.Geometry;
using Utility = EnsoulSharp.SDK.Utility;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Color = System.Drawing.Color;

namespace DaoHungAIO.Champions
{
    class Sett
    {
        private static Spell q, w, e, r;
        private static Menu menu, combo, harass, misc, draw;
        private static AIHeroClient Player = ObjectManager.Player;

        #region
        private static readonly MenuBool Qcombo = new MenuBool("qcombo", "[Q] on Combo");
        private static readonly MenuBool QAA = new MenuBool("QAA", "^ After auto attack", false);
        private static readonly MenuBool Wcombo = new MenuBool("wcombo", "[W] on Combo");
        private static readonly MenuSlider Wminimum = new MenuSlider("Wminimum", "^ minimum % mana for use(0 = disable)");
        private static readonly MenuBool Ecombo = new MenuBool("ecombo", "[E] on Combo");
        private static readonly MenuBool Estun = new MenuBool("Estun", "^ must stun");
        private static readonly MenuBool Rcombo = new MenuBool("Rcombo", "[R] on Combo");
        private static readonly MenuSlider RcomboMinHit = new MenuSlider("RcomboMinHit", "^ minimum hit", 0, 0, 4);

        private static readonly MenuBool Qharass = new MenuBool("qharass", "[Q] on Harass");
        private static readonly MenuBool QAAH = new MenuBool("QAAH", "^ After auto attack", false);
        private static readonly MenuBool Wharass = new MenuBool("Wharass", "[W] on Harass");
        private static readonly MenuBool Eharass = new MenuBool("eharass", "[E] on Harass");

        private static readonly MenuSlider MiscAutoW = new MenuSlider("MiscAutoW", "Auto W when mana higher than", 95);
        private static readonly MenuBool MiscEDash = new MenuBool("MiscEDash", "Auto E on Dash");
        private static readonly MenuBool MiscECC = new MenuBool("MiscECC", "Auto E On CC");
        private static readonly MenuBool MiscQOnE = new MenuBool("MiscQOnE", "Auto Q if E hit");


        private static readonly MenuBool DrawW = new MenuBool("DrawW", "W range");
        private static readonly MenuBool DrawE = new MenuBool("DrawE", "E range");
        private static readonly MenuBool DrawR = new MenuBool("DrawR", "R range");
        private static readonly List<BuffType> CCList = new List<BuffType>() { BuffType.Blind, BuffType.Fear, BuffType.Knockback, BuffType.Knockup, BuffType.Sleep, BuffType.Stun, BuffType.Taunt, BuffType.Suppression, BuffType.Slow };
        #endregion
        public Sett()
        {
            q = new Spell(SpellSlot.Q, 0);
            w = new Spell(SpellSlot.W, 780);
            e = new Spell(SpellSlot.E, 480);
            r = new Spell(SpellSlot.R, 400);

            w.SetSkillshot(.75f, 160, 1200, false, SkillshotType.Cone, HitChance.Medium);
            e.SetSkillshot(.1f, 350, 3000, false, SkillshotType.Line);
            r.SetTargetted(0, float.MaxValue);

            menu = new Menu("Sett", "DH.Sett", true);
            combo = new Menu("Combo", "Combo");
            harass = new Menu("Harass", "Harass");
            draw = new Menu("draw", "Draw");

            combo.Add(Qcombo);
            combo.Add(QAA);
            combo.Add(Wcombo);
            combo.Add(Wminimum);
            combo.Add(Ecombo);
            combo.Add(Estun);
            combo.Add(Rcombo);
            combo.Add(RcomboMinHit);

            harass.Add(Qharass);
            harass.Add(QAAH);
            harass.Add(Wharass);
            harass.Add(Eharass);


            draw.Add(DrawW);
            draw.Add(DrawE);
            draw.Add(DrawR);

            menu.Add(combo);
            menu.Add(harass);
            menu.Add(draw);
            menu.Attach();


            Game.OnUpdate += OnTick;
            //Game.OnWndProc += OnWndProc;


            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAction += OnAction;
        }

        private void OnAction(object sender, OrbwalkerActionArgs args)
        {
            if(args.Type == OrbwalkerType.AfterAttack)
            {
                if(Orbwalker.ActiveMode == OrbwalkerMode.Harass)
                {
                    if (QAAH.Enabled && q.IsReady() && args.Target is AIHeroClient)
                    {
                        q.Cast();
                        Orbwalker.ResetAutoAttackTimer();
                    }
                }
                if(Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    if(QAA.Enabled && q.IsReady() && args.Target is AIHeroClient)
                    {
                        q.Cast();
                        Orbwalker.ResetAutoAttackTimer();
                    }
                }
            }
        }
               
        private void OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case (OrbwalkerMode.Combo):
                    DoCombo();
                    break;
                case OrbwalkerMode.Harass:
                    DoHarass();
                    break;
            }
        }


        private void DoHarass()
        {
            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsValidTarget(e.Range) || target.HaveSpellShield())
            {
                target = TargetSelector.GetTargets(e.Range).Where(t => !t.HaveSpellShield()).OrderByDescending(t => TargetSelector.GetPriority(t)).FirstOrDefault();
            }

            if (target == null)
            {
                return;
            }
            if (!Qharass.Enabled && !Eharass.Enabled && !Wharass.Enabled)
            {
                return;
            }
            if (target.IsValidTarget(e.Range) && Eharass.Enabled && e.IsReady())
            {
                e.Cast(target, true);
            }
            if (target.IsValidTarget(w.Range) && Wharass.Enabled && w.IsReady())
            {
                w.Cast(target, true);
            }
            if (target.IsValidTarget(q.Range) && Qharass.Enabled && q.IsReady())
            {
                if(!QAAH.Enabled)
                {
                    q.Cast();
                }
            }
        }

        private void DoCombo()
        {
            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsValidTarget(e.Range) || target.HaveSpellShield())
            {
                target = TargetSelector.GetTargets(e.Range).Where(t => !t.HaveSpellShield()).OrderByDescending(t => TargetSelector.GetPriority(t)).FirstOrDefault();
            }

            if (target == null)
            {
                return;
            }
            if (!Qcombo.Enabled && !Wcombo.Enabled && !Ecombo.Enabled && !Rcombo.Enabled)
            {
                return;
            }
            if (target.IsValidTarget(e.Range) && Ecombo.Enabled && e.IsReady())
            {
                if (Estun.Enabled)
                {
                    CastEIfStun(target);
                } else
                {
                    e.Cast(target, true);
                }
                
            }
            if (target.IsValidTarget(200) &&Qcombo.Enabled && q.IsReady() && !QAA.Enabled)
            {
                q.Cast();
            }
            if (target.IsValidTarget(r.Range) && Rcombo.Enabled && r.IsReady() && Rhit(target) >= RcomboMinHit.Value)
            {
                r.Cast(target);
            }

            if (w.IsReady() && Player.Mana >= Wminimum.Value)
            {
                w.Cast(target);
            }
        }

        private int Rhit(AIHeroClient target)
        {
            var point = target.Position.Extend(Player.Position, -400);
            return HeroManager.Enemies.Where(e => e.Distance(point) <= 580).Count() - 1;
        }

        private void CastEIfStun(AIHeroClient target)
        {

            var point = Player.Position.Extend(target.Position, -480);
            Geometry.Rectangle rect = new Geometry.Rectangle(Player.Position, point, 170);
            if (GameObjects.AttackableUnits.Where(e => e != target && (e is AIMinionClient || e is AIHeroClient) && e.IsEnemy && e.IsValidTarget(510) && rect.IsInside(e.Position.ToVector2())).Count() > 0) {
                e.Cast(target.Position);
            }
            
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            //var target = TargetSelector.GetTarget(480);
            //if(target != null)
            //{
            //    var point = Player.Position.Extend(target.Position, -480);
            //    Geometry.Rectangle rect = new Geometry.Rectangle(Player.Position, point, 170);
            //    if (GameObjects.AttackableUnits.Where(e => e != target && (e is AIMinionClient || e is AIHeroClient) && e.IsEnemy && e.IsValidTarget(510) && rect.IsInside(e.Position.ToVector2())).Count() > 0)
            //    {
            //        rect.Draw(Color.Green);
            //    } else
            //    {
            //        rect.Draw(Color.Red);

            //    }
            //}
            
            if (DrawW.Enabled && w.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, w.Range, Color.Pink, 1);
            }
            if (DrawE.Enabled && e.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, e.Range, Color.Pink, 1);
            }
            if (DrawR.Enabled && r.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, r.Range, Color.Pink, 1);
            }
        }
    }
}
