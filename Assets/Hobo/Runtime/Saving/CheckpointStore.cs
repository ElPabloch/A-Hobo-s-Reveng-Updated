using System;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public sealed class CheckpointStore
    {
        readonly CampaignModel model;
        public CheckpointStore(CampaignModel session) => model = session;
        public bool Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(model.savePath));
                string temporary = model.savePath + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(model.State, true));
                if (File.Exists(model.savePath))
                    File.Replace(temporary, model.savePath, null);
                else
                    File.Move(temporary, model.savePath);
                model.notice = "Чекушка выпита. Сохранено.";
                model.Sound?.Invoke("save");
                return true;
            }
            catch (Exception error)
            {
                model.notice = "Не удалось сохранить: " + error.Message;
                return false;
            }
        }

        public bool Load()
        {
            try
            {
                var candidate = JsonUtility.FromJson<SaveData>(File.ReadAllText(model.savePath));
                if (!Valid(candidate))
                    throw new InvalidDataException("Повреждённое сохранение");
                model.State = candidate;
                model.walls = Rules.Walls(candidate.chapter * 3 + candidate.room);
                model.shots.Clear();
                model.cooldown = model.hurt = model.swing = model.throwAnimation = 0;
                model.pendingThrowWeapon = "";
                model.blocking = false;
                model.mode = "play";
                model.revision++;
                return true;
            }
            catch (Exception error)
            {
                model.notice = "Не удалось загрузить: " + error.Message;
                return false;
            }
        }

        public bool Valid(SaveData data)
        {
            if (data == null || data.version != 1 || data.chapter < 0 || data.chapter >= model.content.chapters.Length || data.room < 0 || data.room >= 3)
                return false;
            if (data.hp < 1 || data.hp > (data.outfit == "coat" ? 6 : 4) || Array.IndexOf(Rules.Weapons, data.weapon) < 0)
                return false;
            if (data.enemies == null || data.items == null || data.blood == null || data.enemies.Count > 20 || data.items.Count > 500)
                return false;
            if (data.outfit != "" && data.outfit != "coat" && data.outfit != "shoes")
                return false;
            if (data.buff != "" && data.buff != "rage" && data.buff != "speed" && data.buff != "shield")
                return false;
            if (!float.IsFinite(data.buffLeft) || data.buffLeft < 0 || data.buffLeft > 20 || !float.IsFinite(data.elapsed) || data.elapsed < 0)
                return false;
            if (!Position(data.hero))
                return false;
            var oldWalls = model.walls;
            model.walls = Rules.Walls(data.chapter * 3 + data.room);
            bool valid = model.Navigation.Free(data.hero);
            model.walls = oldWalls;
            if (!valid)
                return false;
            foreach (var enemy in data.enemies)
            {
                if (enemy == null || !Position(enemy.pos) || !float.IsFinite(enemy.windup) || enemy.windup < 0 || enemy.windup > 2 || !float.IsFinite(enemy.cooldown) || !float.IsFinite(enemy.facing.x) || !float.IsFinite(enemy.facing.y))
                    return false;
                try
                {
                    var rule = Rules.Enemy(enemy.kind);
                    if (enemy.hp > rule.hp || (!enemy.dead && enemy.hp <= 0) || (enemy.dead && enemy.hp > 0))
                        return false;
                }
                catch
                {
                    return false;
                }
            }

            foreach (var item in data.items)
            {
                if (item == null || !Position(item.pos))
                    return false;
                if (!(Array.IndexOf(Rules.Weapons, item.kind) > 0 || Array.IndexOf(new[]{"beer", "rage", "speed", "shield", "coat", "shoes"}, item.kind) >= 0))
                    return false;
            }

            return true;
        }

        static bool Position(Vector2 position) => float.IsFinite(position.x) && float.IsFinite(position.y) && CampaignModel.Area.Contains(position);
    }
}
