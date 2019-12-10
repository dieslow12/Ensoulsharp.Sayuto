using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using EnsoulSharp;
using SharpDX;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp.SDK.MenuUI.Values;
using Keys = System.Windows.Forms.Keys;
using SPrediction;
using DaoHungAIO.Helpers;
using EnsoulSharp.SDK.Events;
using Geometry = EnsoulSharp.SDK.Geometry;
using Utility = EnsoulSharp.SDK.Utility;


namespace DaoHungAIO.Champions
{
    class Zoe
    {
        private Menu config;
        private Spell q, w, e, r;
        private int qWidth = 58;
        private AIHeroClient Player = ObjectManager.Player;
        private bool qCanCast = true;
        private bool rAfterQ = false;
        private bool rBeforeQ = false;

        private Menu combo = new Menu("combo", "Combo");
        private MenuBool qcombo = new MenuBool("qcombo", "Use Q");
        private MenuBool wcombo = new MenuBool("wcombo", "Use W");
        private MenuBool ecombo = new MenuBool("ecombo", "Use E");
        private MenuBool rcombo = new MenuBool("rcombo", "Use R");

        private Menu harass = new Menu("harass", "Harass");
        private MenuBool qharass = new MenuBool("qharass", "Use Q");
        private MenuBool wharass = new MenuBool("wharass", "Use W");
        private MenuBool eharass = new MenuBool("eharass", "Use E");
        private MenuBool rharass = new MenuBool("rharass", "Use R");

        private Menu clear = new Menu("clear", "Clear(Not Implement)");
        private MenuBool qclear = new MenuBool("qclear", "Use Q");
        private MenuBool eclear = new MenuBool("eclear", "Use E");

        private Menu lasthit = new Menu("lasthit", "Lasthit(Not Implement)");
        private MenuBool qlasthit = new MenuBool("qlasthit", "Use Q");
        private MenuBool elasthit = new MenuBool("elasthit", "Use E");

        private Menu misc = new Menu("misc", "Misc(Not Implement)");

        private Menu draw = new Menu("draw", "Draw");
        private MenuBool qrange = new MenuBool("qrange", "Q max range");
        private MenuBool erange = new MenuBool("erange", "E range");
        private MenuBool rrange = new MenuBool("rrange", "R range");

        public Zoe()
        {
            q = new Spell(SpellSlot.Q, 800);
            w = new Spell(SpellSlot.W);
            e = new Spell(SpellSlot.E, 800);
            r = new Spell(SpellSlot.R, 575);

            q.SetSkillshot(.25f, qWidth, 900, true, SkillshotType.Line);

            e.SetSkillshot(.25f, 100, 900, true, SkillshotType.Line);

            config = new Menu("Zoe", "DH.Zoe", true);
            combo.Add(qcombo);
            combo.Add(wcombo);
            combo.Add(ecombo);
            combo.Add(rcombo);
            config.Add(combo);

            config.Add(harass);
            harass.Add(qharass);
            harass.Add(wharass);
            harass.Add(eharass);
            harass.Add(rharass);

            config.Add(clear);
            clear.Add(qclear);
            clear.Add(eclear);

            config.Add(lasthit);
            lasthit.Add(qlasthit);
            lasthit.Add(elasthit);

            config.Add(misc);

            config.Add(draw);
            draw.Add(qrange);
            draw.Add(erange);
            draw.Add(rrange);

            config.Attach();

            Tick.OnTick += OnTick;
            Drawing.OnDraw += OnDraw;
            GameObject.OnCreate += OnCreate;
        }

        private void OnCreate(GameObject sender, EventArgs args)
        {
           if(sender.Name.Contains("Zoe") && sender.Name.Contains("Mis_Linger"))
            {
                Utility.DelayAction.Add(800, () => {
                    if(q.Name == "ZoeQ")
                    {
                        return;
                    }
                    var target = TargetSelector.GetTarget(q.Range);
                    if(target != null)
                    {
                        q.Cast(target.Position);
                    }
                    if(target== null)
                    {
                        var minion = GameObjects.GetMinions(q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health).FirstOrDefault();
                        if(minion  != null)
                        {
                            q.Cast(minion);
                        } else
                        {
                            var jungle = GameObjects.GetJungles(q.Range, JungleType.All, JungleOrderTypes.Health).FirstOrDefault();
                            if (jungle != null)
                            {
                                q.Cast(jungle);
                            }
                        }
                        
                    }
                });
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (q.IsReady() && qrange.Enabled)
            {
                if (r.IsReady())
                {
                    Render.Circle.DrawCircle(Player.Position, q.Range + r.Range - 150, Color.Gray, 1);
                }
                else
                {
                    Render.Circle.DrawCircle(Player.Position, q.Range - 50, Color.Gray, 1);
                }
            }
            if (e.IsReady() && erange.Enabled)
            {
                    Render.Circle.DrawCircle(Player.Position, e.Range, Color.Gray, 1);
            }
            if (r.IsReady() && rrange.Enabled)
            {
                    Render.Circle.DrawCircle(Player.Position, r.Range, Color.Gray, 1);
            }
        }

        private void OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    break;
                case OrbwalkerMode.LastHit:
                    break;
            }
        }

        private void CastQInstance(AttackableUnit target)
        {
            q.Cast((AIBaseClient)target);
        }
        private void Combo()
        {
            var target = TargetSelector.GetTarget(q.Range - 50);

            if (r.IsReady() && rcombo.Enabled)
            {
                if(target == null)
                {
                    target = TargetSelector.GetTarget(r.Range + r.Range - 150);

                    if (target == null)
                    {
                        return;
                    } else
                    {
                        rAfterQ = true;
                    }
                } else
                {
                    rBeforeQ = true;
                }
            }
            if (target == null)
            {
                return;
            }
            if (q.IsReady() && qcombo.Enabled)
            {
                CastQ(target);
                rBeforeQ = false;
                rAfterQ = false;
            }
            if (e.IsReady() && ecombo.Enabled)
            {
                e.Cast(target);
            }
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(q.Range - 50);

            if (r.IsReady() && rcombo.Enabled)
            {
                if (target == null)
                {
                    target = TargetSelector.GetTarget(r.Range + r.Range - 150);

                    if (target == null)
                    {
                        return;
                    }
                    else
                    {
                        rAfterQ = true;
                    }
                }
                else
                {
                    rBeforeQ = true;
                }
            }
            if (target == null)
            {
                return;
            }
            if (q.IsReady() && qharass.Enabled)
            {
                CastQ(target);

                rBeforeQ = false;
                rAfterQ = false;
            }
            if (e.IsReady() && eharass.Enabled)
            {
                e.Cast(target);
            }
        }

        private void CastQ(AttackableUnit target)
        {
            if (q.Name == "ZoeQ")
            {
                var playerPos = Player.Position.ToVector2();

                if (rBeforeQ)
                {
                    playerPos = Player.Position.Extend(target.Position, -r.Range).ToVector2();
                }
                var pos = playerPos + 565;
                List<Vector2> posCanCast = new List<Vector2>();
                for (var angle = 0f; angle < 359; angle += 15)
                {
                    var newPos = pos.RotateAroundPoint(playerPos, (float)(Math.PI * angle / 180.0));
                    if (!q.CheckCollision(newPos))
                    {
                        posCanCast.Add(newPos);
                    }
                    //q.Cast(newPos);
                }
                if (posCanCast.Count() == 0)
                {
                    return;
                }
                var point = posCanCast.Where(o => o.Distance(target) > target.Distance(Player) && !SPrediction.Collision.CheckCollision(o, target.Position.ToVector2(), qWidth, .25f, 900))
                    .OrderByDescending(o => o.Distance(target)).FirstOrDefault();
                if (point != null)
                {
                    if (rBeforeQ)
                    {

                        //Game.Print("pos:" + point);
                        var posCastR = Player.Position.Extend(target.Position, -r.Range);
                        r.Cast(posCastR);
                        rBeforeQ = false;
                        Utility.DelayAction.Add((int)(q.Range * 1000 / 900 + Game.Ping), () =>
                        {

                            q.Cast(point);
                            Utility.DelayAction.Add((int)(q.Range * 1000 / 900 + Game.Ping), () =>
                            {
                                qCanCast = true;
                            });
                        });
                    }
                    else
                    {

                        q.Cast(point);
                        qCanCast = false;
                        Utility.DelayAction.Add((int)(q.Range * 1000 / 900 + Game.Ping), () =>
                        {
                            if (rAfterQ)
                            {
                                var posCastR = Player.Position.Extend(target.Position, r.Range);
                                r.Cast(posCastR);
                                rAfterQ = false;
                            }
                            qCanCast = true;
                        });
                    }
                }
                else
                {
                    q.Cast((AIBaseClient)target);
                }
            } else if(qCanCast)
            {
                q.CastIfHitchanceMinimum((AIBaseClient)target, HitChance.Low);
            }


        }
    }
}
