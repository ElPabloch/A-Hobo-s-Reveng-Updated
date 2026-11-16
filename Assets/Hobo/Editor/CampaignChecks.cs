using System;
using System.IO;
using UnityEngine;

namespace Hobo.Editor
{
    public static class CampaignChecks
    {
        public static string Run()
        {
            int count = 0;
            void Check(bool ok, string label)
            {
                count++;
                if (!ok)
                    throw new Exception("CHECK FAILED: " + label);
            }

            var content = JsonUtility.FromJson<CampaignContent>(Resources.Load<TextAsset>("Campaign").text.TrimStart('\uFEFF'));
            var path = Path.Combine(Application.temporaryCachePath, "hobo-migration-test.json");
            var m = new CampaignModel(content, path);
            m.NewGame();
            m.mode = "play";
            Check(m.State.hp == 4, "base HP");
            m.blocking = true;
            m.facing = Vector2.right;
            m.Damage(2, m.State.hero + Vector2.right * 20, false);
            Check(m.State.hp == 4, "front block");
            m.Damage(2, m.State.hero - Vector2.right * 20, false);
            Check(m.State.hp == 2, "rear bypass");
            m.hurt = 0;
            m.Damage(2, m.State.hero + Vector2.right * 20, true);
            Check(m.State.hp == 0, "bullet bypass and two hits");
            m.NewGame();
            m.mode = "play";
            m.State.enemies.Clear();
            var target = new Enemy("hobo", m.State.hero + Vector2.right * 60);
            m.State.enemies.Add(target);
            m.Attack();
            Check(target.hp == 3, "fist range");
            m.cooldown = 0;
            m.State.weapon = "pipe";
            m.Attack();
            Check(target.hp == 2, "long weapon range");
            m.cooldown = 0;
            m.State.buff = "rage";
            m.State.buffLeft = 15;
            m.Attack();
            Check(target.dead, "rage damage");
            Check(m.State.items.FindAll(i => i.kind == "brick" && i.pos == target.pos).Count == 1, "weapon drop once");
            m.Hit(target, 10);
            Check(m.State.kills == 1, "no duplicate kill");
            m.cooldown = 0;
            m.State.weapon = "knife";
            m.Throw();
            Check(m.State.weapon == "" && m.shots.Count == 1, "throw consumes held weapon");
            m.Throw();
            Check(m.shots.Count == 1, "throw no duplication");
            m.NewGame();
            m.State.hp = 2;
            m.State.items.Clear();
            m.State.items.Add(new("beer", m.State.hero));
            m.Interact();
            Check(m.State.hp == 4 && m.State.items.Count == 0, "beer heal two");
            m.State.items.Add(new("beer", m.State.hero));
            m.Interact();
            Check(m.State.items.Count == 1, "beer retained at full hp");
            m.State.items.Clear();
            m.State.items.Add(new("coat", m.State.hero));
            m.Interact();
            Check(m.MaxHP == 6 && m.State.hp == 4, "coat no free heal");
            m.State.hp = 6;
            m.State.items.Add(new("shoes", m.State.hero));
            m.Interact();
            Check(m.MaxHP == 4 && m.State.hp == 4, "unequip clamps HP");
            m.NewGame();
            m.State.hero = CampaignModel.Checkpoint;
            Check(m.Save(), "first save");
            m.State.hp = 2;
            Check(m.Save(), "atomic replacement");
            m.State.hp = 4;
            Check(m.Load() && m.State.hp == 2, "restore hp");
            Check(m.Valid(m.State), "own save valid");
            m.State.hero = new(float.NaN, 0);
            Check(!m.Valid(m.State), "reject nonfinite position");
            Check(m.Load(), "valid reload");
            m.State.items.Add(new("fake", m.State.hero));
            Check(!m.Valid(m.State), "unknown item rejected");
            File.WriteAllText(path, "bad json");
            Check(!m.Load(), "corrupt save handled");
            m.NewGame();
            m.mode = "play";
            m.State.enemies.Clear();
            m.State.buff = "rage";
            m.State.buffLeft = .02f;
            m.Tick(.04f, Vector2.zero, Vector2.right, false, false, false, false);
            Check(m.State.buff == "", "buff expiry");
            m.State.buff = "shield";
            m.State.buffLeft = 5;
            m.mode = "pause";
            m.Tick(.05f, Vector2.right, Vector2.right, false, false, false, false);
            Check(m.State.buffLeft == 5, "pause stops buff");
            m.NewGame();
            var p = m.Move(new(310, 240), Vector2.right * 100);
            Check(p.x < 340, "movement cannot tunnel");
            Check(!m.LOS(new(300, 240), new(420, 240)), "walls stop line of sight");
            for (int c = 0; c < 8; c++)
                for (int r = 0; r < 3; r++)
                {
                    m.State.chapter = c;
                    m.State.room = r;
                    m.Setup();
                    Check(m.Free(m.State.hero), $"hero spawn {c}:{r}");
                    foreach (var e in m.State.enemies)
                        Check(m.Free(e.pos, e.kind == "rat" ? 8 : 12), $"enemy spawn {c}:{r}");
                    foreach (var i in m.State.items)
                        Check(m.Free(i.pos, 2), $"item spawn {c}:{r}");
                    Check(m.Pursuit(m.State.hero, new(880, 330)).sqrMagnitude > .1f, $"reachable exit {c}:{r}");
                }

            m.NewGame();
            m.mode = "play";
            m.State.enemies.Clear();
            m.State.hero = new(880, 330);
            m.Tick(.016f, Vector2.zero, Vector2.right, false, false, false, false);
            Check(m.State.room == 1, "room transition");
            m.State.chapter = 7;
            m.State.room = 2;
            m.Setup();
            m.State.enemies.Clear();
            m.State.hero = new(820, 330);
            m.Interact();
            Check(m.mode == "won", "final whisky ending");
            m.NewGame();
            m.mode = "play";
            m.State.buff = "shield";
            m.State.buffLeft = 100;
            for (int i = 0; i < 1200; i++)
                m.Tick(1 / 60f, Vector2.zero, Vector2.right, true, false, false, false);
            foreach (var e in m.State.enemies)
                Check(m.Free(e.pos, e.kind == "rat" ? 8 : 12), "AI remains navigable");
            var report = $"PASS: {count} campaign assertions; Unity {Application.unityVersion}; {DateTime.UtcNow:O}";
            Directory.CreateDirectory("QA");
            File.WriteAllText("QA/model-checks.txt", report);
            Debug.Log(report);
            return report;
        }
    }
}
