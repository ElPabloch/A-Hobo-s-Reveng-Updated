using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public sealed class EnemySystem
    {
        readonly CampaignModel model;
        public EnemySystem(CampaignModel session)
        {
            model = session;
        }

        public void Tick(float deltaTime)
        {
            foreach (var enemy in model.State.enemies)
            {
                if (enemy.dead)
                    continue;
                enemy.flash = Mathf.Max(0, enemy.flash - deltaTime);
                enemy.animationRecovery = Mathf.Max(0, enemy.animationRecovery - deltaTime);
                enemy.cooldown -= deltaTime;
                var definition = Rules.Enemy(enemy.kind);
                var delta = model.State.hero - enemy.pos;
                if (enemy.windup > 0)
                {
                    enemy.windup -= deltaTime;
                    if (enemy.windup <= 0)
                    {
                        bool ranged = enemy.kind == "gunner" || enemy.kind == "thrower" || (enemy.kind == "boss" && enemy.hp <= 12);
                        enemy.animationRecovery = ranged ? .16f : .21f;
                        if (ranged)
                        {
                            model.Projectiles.Fire(enemy.pos, enemy.facing, enemy.kind == "thrower" ? "brick" : "bullet", true);
                            if (enemy.kind == "boss")
                            {
                                model.Projectiles.Fire(enemy.pos, Rotate(enemy.facing, .22f), "bullet", true);
                                model.Projectiles.Fire(enemy.pos, Rotate(enemy.facing, -.22f), "bullet", true);
                            }
                        }
                        else if (delta.magnitude <= definition.reach + 12 && Vector2.Dot(enemy.facing, delta.normalized) > .2f && model.Navigation.LOS(enemy.pos, model.State.hero))
                            model.Combat.Damage(enemy.kind == "rat" ? 1 : 2, enemy.pos, false);
                        enemy.cooldown = enemy.kind == "boss" ? .65f : .85f;
                    }
                }
                else
                {
                    enemy.facing = delta.normalized;
                    if (Mathf.Abs(enemy.facing.x) > Mathf.Abs(enemy.facing.y))
                        enemy.horizontalFacing = Mathf.Sign(enemy.facing.x);
                    float range = enemy.kind == "boss" && enemy.hp <= 12 ? 360 : definition.reach;
                    if (delta.magnitude <= range && model.Navigation.LOS(enemy.pos, model.State.hero))
                    {
                        if (enemy.cooldown <= 0)
                        {
                            enemy.windup = definition.windup;
                            enemy.animationFacing = new Vector2(enemy.horizontalFacing, 0f);
                            bool rangedAttack = enemy.kind == "gunner" || enemy.kind == "thrower"
                                || (enemy.kind == "boss" && enemy.hp <= 12);
                            enemy.animationAction = rangedAttack ? "throw" : "attack";
                        }
                    }
                    else
                    {
                        var direction = model.Navigation.Pursuit(enemy.pos, model.State.hero);
                        foreach (var other in model.State.enemies)
                            if (other != enemy && !other.dead && Vector2.Distance(enemy.pos, other.pos) < 28)
                                direction += (enemy.pos - other.pos).normalized * .65f;
                        enemy.pos = model.Navigation.Move(enemy.pos, direction.normalized * definition.speed * deltaTime, enemy.kind == "rat" ? 8 : 12);
                    }
                }
            }
        }

        static Vector2 Rotate(Vector2 d, float a) => new(d.x * Mathf.Cos(a) - d.y * Mathf.Sin(a), d.x * Mathf.Sin(a) + d.y * Mathf.Cos(a));
    }
}

