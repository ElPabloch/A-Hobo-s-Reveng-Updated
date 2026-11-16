using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public sealed class ProjectileSystem
    {
        readonly CampaignModel model;
        public ProjectileSystem(CampaignModel session)
        {
            model = session;
        }

        public void Fire(Vector2 pos, Vector2 direction, string kind, bool hostile)
        {
            model.shots.Add(new Shot{pos = pos + direction * 19, velocity = direction * (kind == "bullet" ? 440 : 350), kind = kind, hostile = hostile});
            if (kind == "bullet")
                model.Sound?.Invoke("shoot");
        }

        public void Tick(float dt)
        {
            for (int i = model.shots.Count - 1; i >= 0; i--)
            {
                var p = model.shots[i];
                p.life -= dt;
                bool hit = false;
                int n = Mathf.Max(1, Mathf.CeilToInt(p.velocity.magnitude * dt / 4));
                for (int j = 0; j < n && !hit; j++)
                {
                    var next = p.pos + p.velocity * dt / n;
                    if (!model.Navigation.Free(next, 2))
                    {
                        hit = true;
                        break;
                    }

                    p.pos = next;
                    if (p.hostile && Vector2.Distance(p.pos, model.State.hero) < 15)
                    {
                        model.Combat.Damage(p.kind == "bullet" ? 2 : 1, p.pos - p.velocity.normalized * 16, p.kind == "bullet");
                        hit = true;
                    }
                    else if (!p.hostile)
                        foreach (var e in model.State.enemies)
                            if (!e.dead && Vector2.Distance(p.pos, e.pos) < (e.kind == "rat" ? 15 : 20))
                            {
                                model.Combat.Hit(e, 1);
                                hit = true;
                                break;
                            }
                }

                if (hit || p.life <= 0)
                {
                    if (p.kind != "bullet")
                        model.State.items.Add(new(p.kind, p.pos));
                    model.shots.RemoveAt(i);
                }
            }
        }
    }
}
