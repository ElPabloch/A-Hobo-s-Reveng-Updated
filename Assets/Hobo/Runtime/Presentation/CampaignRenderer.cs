using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hobo
{
    public sealed class CampaignRenderer : MonoBehaviour
    {
        CampaignModel model;
        readonly Dictionary<string, Sprite> sprites = new();
        readonly List<SpriteRenderer> pool = new();
        int cursor, revision = -1;
        Transform environment, actors;
        Material material;
        public void Initialize(CampaignModel session)
        {
            model = session;
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            actors = new GameObject("Actors").transform;
            actors.SetParent(transform);
        }

        public static Vector3 World(Vector2 position) => GameCoordinates.ToWorld(position);
        static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }

        Sprite GetSprite(string atlas, int column, int row, int height)
        {
            string key = $"{atlas}:{column}:{row}:{height}";
            if (sprites.TryGetValue(key, out var sprite))
                return sprite;
            var texture = Resources.Load<Texture2D>("Art/" + atlas);
            if (texture == null)
                throw new Exception("Missing atlas: " + atlas);
            bool playerAtlas = atlas.StartsWith("player_") || atlas.StartsWith("enemy_") || atlas.StartsWith("boss_");
            int width = playerAtlas ? 35 : 32;
            var pivot = playerAtlas ? new Vector2(.5f, 0f) : height == 48 ? new Vector2(.5f, .125f) : new Vector2(.5f, .5f);
            if (playerAtlas) texture.filterMode = FilterMode.Point;
            sprite = Sprite.Create(texture, new Rect(column * width, texture.height - (row + 1) * height, width, height), pivot, 16, 0, SpriteMeshType.FullRect);
            sprites.Add(key, sprite);
            return sprite;
        }

        SpriteRenderer DrawSprite(string atlas, Vector2 position, int column = 0, int row = 0, int height = 48, int order = 0, Color? color = null, bool flip = false)
        {
            SpriteRenderer renderer;
            if (cursor == pool.Count)
            {
                renderer = new GameObject("Sprite").AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(actors);
                renderer.sharedMaterial = material;
                pool.Add(renderer);
            }
            else
                renderer = pool[cursor];
            cursor++;
            renderer.gameObject.SetActive(true);
            renderer.sprite = GetSprite(atlas, column, row, height);
            renderer.transform.position = World(position);
            renderer.transform.localScale = Vector3.one;
            renderer.transform.rotation = Quaternion.identity;
            renderer.sortingOrder = order;
            renderer.color = color ?? Color.white;
            renderer.flipX = flip;
            return renderer;
        }

        void Tile(string atlas, Vector2 position, int column, int row = 0, int order = -1000, Color? color = null, Vector3? scale = null)
        {
            var renderer = new GameObject(atlas).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(environment);
            renderer.sharedMaterial = material;
            renderer.sprite = GetSprite(atlas, column, row, 32);
            renderer.transform.position = World(position);
            renderer.transform.localScale = scale ?? Vector3.one;
            renderer.sortingOrder = order;
            renderer.color = color ?? Color.white;
        }

        void BuildRoom()
        {
            if (environment)
                Destroy(environment.gameObject);
            environment = new GameObject("Room").transform;
            environment.SetParent(transform);
            for (int y = 144; y <= 592; y += 64)
                for (int x = 32; x <= 1376; x += 64)
                    Tile("sewer_tiles", new(x, y), 0, color: model.State.chapter == 0 ? Color.white : Color.Lerp(Color.white, Hex(model.Chapter.floor), .4f));
            for (int x = 32; x <= 1376; x += 64)
            {
                Tile("sewer_tiles", new(x, 112), 1);
                Tile("sewer_tiles", new(x, 560), 2, color: Hex("72877c"));
                Tile("sewer_tiles", new(x, 128), 4, order: -900);
            }

            foreach (var wall in model.walls)
                for (float y = wall.y; y < wall.yMax; y += 32)
                    Tile("sewer_tiles", new(wall.center.x, y + 16), 1, order: (int)wall.yMax, color: Hex(model.Chapter.wall), scale: new Vector3(wall.width / 64, .5f, 1));
            Tile("environment_props", new(82, 180), 0, order: 180);
            Tile("environment_props", new(1308, 180), 3, order: 180);
            Tile("environment_props", new(1288, 510), 1, order: 510);
            Tile("environment_props", new(160, 165), 2, 1, 165);
        }

        void Actor(string atlas, Vector2 position, Vector2 facing, bool walking, int pose = -1, Color? color = null)
        {
            int row = Mathf.Abs(facing.x) > Mathf.Abs(facing.y) ? 1 : facing.y < 0 ? 2 : 0;
            int frame = pose >= 0 ? pose : walking ? (int)(model.clock * 9) % 4 : 0;
            DrawSprite(atlas, position, frame, row, atlas == "rat" ? 24 : 48, (int)position.y, color, facing.x < 0 && row == 1);
        }

        void DrawPlayer()
        {
            Vector2 facing = model.throwAnimation > 0 ? model.throwFacing
                : model.swing > 0 ? model.attackFacing : model.facing;
            string direction = Mathf.Abs(facing.x) > Mathf.Abs(facing.y)
                ? (facing.x < 0 ? "left" : "right")
                : (facing.y < 0 ? "up" : "down");
            string action = model.throwAnimation > 0 ? "throw" : model.swing > 0 ? "attack" : "walk";
            int frame;
            if (action == "throw")
                frame = Mathf.Clamp((int)((.24f - model.throwAnimation) / .24f * 6), 0, 5);
            else if (action == "attack")
                frame = AttackAnimationFrame(model.swing);
            else
                frame = model.moving ? (int)(model.clock * 10) % 6 : 0;
            Color tint = model.hurt > 0 && ((int)(model.clock * 18) % 2 == 0)
                ? new Color(1, .5f, .5f, .55f) : Color.white;
            DrawSprite($"player_{action}_{direction}", model.State.hero, frame, 0, 35, (int)model.State.hero.y, tint);
            if (model.State.weapon != "" || model.pendingThrowWeapon != "")
                DrawHeldWeapon(direction, action, frame);
        }

        static int AttackAnimationFrame(float remaining)
        {
            float elapsed = CombatSystem.AttackAnimationDuration - remaining;
            if (elapsed < .025f) return 0;
            if (elapsed < .050f) return 1;
            if (elapsed < .090f) return 2;
            if (elapsed < .125f) return 3;
            if (elapsed < .170f) return 4;
            if (elapsed < .215f) return 5;
            if (elapsed < .250f) return 6;
            return 7;
        }

        void DrawHeldWeapon(string direction, string action, int frame)
        {
            bool attack = action == "attack";
            Vector2[] hands = direction switch
            {
                "down" => attack ? new[] { new Vector2(22, 22), new Vector2(22, 19), new Vector2(24, 17), new Vector2(25, 20), new Vector2(24, 17), new Vector2(22, 22) } : new[] { new Vector2(22, 22), new Vector2(21, 21), new Vector2(21, 21), new Vector2(22, 22), new Vector2(22, 21), new Vector2(22, 21) },
                "left" => attack ? new[] { new Vector2(12, 15), new Vector2(9, 14), new Vector2(6, 14), new Vector2(5, 15), new Vector2(8, 16), new Vector2(10, 16), new Vector2(13, 16), new Vector2(12, 15) } : new[] { new Vector2(16, 21), new Vector2(11, 19), new Vector2(10, 19), new Vector2(16, 21), new Vector2(13, 19), new Vector2(11, 19) },
                "right" => attack ? new[] { new Vector2(22, 15), new Vector2(25, 14), new Vector2(28, 14), new Vector2(29, 15), new Vector2(26, 16), new Vector2(24, 16), new Vector2(21, 16), new Vector2(22, 15) } : new[] { new Vector2(17, 21), new Vector2(22, 19), new Vector2(23, 19), new Vector2(17, 21), new Vector2(20, 19), new Vector2(22, 19) },
                "up" => attack ? new[] { new Vector2(22, 22), new Vector2(22, 19), new Vector2(24, 17), new Vector2(25, 20), new Vector2(24, 17), new Vector2(22, 22) } : new[] { new Vector2(22, 22), new Vector2(21, 21), new Vector2(21, 21), new Vector2(22, 22), new Vector2(22, 21), new Vector2(22, 21) },
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
            if (action == "throw")
            {
                hands = direction switch
                {
                    "right" => new[] { new Vector2(21, 13), new Vector2(20, 9), new Vector2(26, 12), new Vector2(28, 15), new Vector2(24, 18), new Vector2(22, 21) },
                    "left" => new[] { new Vector2(13, 13), new Vector2(14, 9), new Vector2(8, 12), new Vector2(6, 15), new Vector2(10, 18), new Vector2(12, 21) },
                    _ => new[] { new Vector2(22, 14), new Vector2(22, 10), new Vector2(23, 13), new Vector2(24, 17), new Vector2(23, 20), new Vector2(22, 22) }
                };
            }
            Vector2 hand = hands[frame];
            Vector2 offset = new((hand.x - 17.5f) * 2f, (hand.y - 35f) * 2f);
            float side = direction == "left" ? -1f : 1f;
            float[] swing = direction == "left" || direction == "right"
                ? new[] { -75f, -45f, -15f, 5f, 35f, 65f, -40f, -75f }
                : new[] { -75f, -45f, -15f, 35f, 65f, -75f };
            float angle = attack ? swing[frame] : -75f;
            Vector2 blade = new(side * Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            string heldWeapon = action == "throw" ? model.pendingThrowWeapon : model.State.weapon;
            if (heldWeapon == "")
                return;
            var weapon = DrawSprite("props", model.State.hero + offset + blade * 10f,
                Prop(heldWeapon), 0, 32, (int)model.State.hero.y + 1);
            weapon.transform.localScale = Vector3.one * .45f;
            weapon.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(blade.y, blade.x) * Mathf.Rad2Deg);
        }

        static readonly string[] EnemyVariants = { "scrapper", "drifter", "raider", "worker" };
        static readonly Vector2[] EnemyAttackHands = { new(22,15), new(25,14), new(28,14), new(29,15), new(26,16), new(24,16), new(21,16), new(22,15) };
        static readonly Vector2[] EnemyThrowHands = { new(21,13), new(20,9), new(26,12), new(28,15), new(24,18), new(22,21) };
        static readonly float[] EnemyWeaponAngles = { -75f, -45f, -15f, 5f, 35f, 65f, -40f, -75f };

        void DrawCommonEnemy(Enemy enemy, int variantIndex)
        {
            string variant = enemy.kind == "cop" || enemy.kind == "gunner" ? enemy.kind : EnemyVariants[variantIndex % EnemyVariants.Length];
            DrawAnimatedEnemy(enemy, "enemy_" + variant, 1f);
        }

        void DrawBoss(Enemy enemy)
        {
            DrawAnimatedEnemy(enemy, "boss_" + enemy.kind, enemy.kind == "boss" ? 1.65f : 1.35f);
        }

        void DrawAnimatedEnemy(Enemy enemy, string atlas, float scale)
        {
            bool acting = !enemy.dead && (enemy.windup > 0 || enemy.animationRecovery > 0);
            Vector2 facing = acting ? enemy.animationFacing : enemy.facing;
            if (acting && facing.sqrMagnitude < .01f)
                facing = new Vector2(enemy.facing.x < 0 ? -1f : 1f, 0f);
            string direction = acting ? (facing.x < 0 ? "left" : "right")
                : Mathf.Abs(facing.x) > Mathf.Abs(facing.y) ? (facing.x < 0 ? "left" : "right")
                : facing.y < 0 ? "up" : "down";
            string action = acting ? (enemy.animationAction == "throw" ? "throw" : "attack") : "walk";
            int frame = enemy.dead ? 0 : (int)(model.clock * 10f) % 6;
            if (acting)
            {
                // Hold preparation during wind-up, then show impact and recovery after release.
                float releaseTime = action == "throw" ? .08f : .09f;
                float duration = action == "throw" ? .24f : CombatSystem.AttackAnimationDuration;
                float elapsed = enemy.windup > 0 ? (1f - enemy.windup / Rules.Enemy(enemy.kind).windup) * releaseTime : duration - enemy.animationRecovery;
                frame = action == "throw" ? Mathf.Clamp((int)(elapsed / .04f), 0, 5) : AttackAnimationFrame(duration - elapsed);
            }
            Color tint = enemy.dead ? Hex("756463") : enemy.flash > 0 ? Hex("ffb8a0") : enemy.windup > 0 ? Hex("ffcc8b") : Color.white;
            var renderer = DrawSprite($"{atlas}_{action}_{direction}", enemy.pos, frame, 0, 35, (int)enemy.pos.y - (enemy.dead ? 1 : 0), tint);
            renderer.transform.localScale = Vector3.one * scale;
            if (enemy.dead) renderer.transform.rotation = Quaternion.Euler(0, 0, 90);
            else DrawEnemyHeldWeapon(enemy, direction, action, frame, scale, tint);
        }

        void DrawEnemyHeldWeapon(Enemy enemy, string direction, string action, int frame, float scale, Color tint)
        {
            string weaponKind = Rules.Enemy(enemy.kind).weapon;
            if (weaponKind == "" || (enemy.kind == "thrower" && action == "throw" && enemy.windup <= 0)) return;
            Vector2 hand = action == "attack" ? EnemyAttackHands[frame] : action == "throw" ? EnemyThrowHands[frame]
                : direction == "down" || direction == "up" ? new Vector2(22,21) : new Vector2(21,20);
            if (direction == "left") hand.x = 34f - hand.x;
            Vector2 offset = new((hand.x - 17.5f) * 2f * scale, (hand.y - 35f) * 2f * scale);
            float angle = action == "attack" ? EnemyWeaponAngles[frame] : -75f;
            float side = direction == "left" ? -1f : 1f;
            Vector2 blade = new(side * Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            var weapon = DrawSprite("props", enemy.pos + offset + blade * (10f * scale), Prop(weaponKind), 0, 32, (int)enemy.pos.y + 1, tint);
            weapon.transform.localScale = Vector3.one * (.45f * scale);
            weapon.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(blade.y, blade.x) * Mathf.Rad2Deg);
        }

        static int Prop(string kind) => kind switch
        {
        "beer" => 0, "rage" => 0, "speed" => 0, "shield" => 0, "brick" => 2, "pipe" => 3, "bat" => 4, "knife" => 5, "whisky" => 6, _ => 1
        }

        ;
        public void Refresh()
        {
            if (revision != model.revision)
            {
                BuildRoom();
                revision = model.revision;
            }

            cursor = 0;
            foreach (var position in model.State.blood)
            {
                var stain = DrawSprite("sewer_tiles", position, 2, 0, 32, -800, Hex("682e35"));
                stain.transform.localScale = new(.45f, .18f, 1);
            }

            foreach (var item in model.State.items)
            {
                var color = item.kind == "rage" ? Hex("e898a5") : item.kind == "speed" ? Hex("a5d9be") : item.kind == "shield" ? Hex("f0cf85") : Color.white;
                if (item.kind == "coat" || item.kind == "shoes")
                    DrawSprite(item.kind == "coat" ? "enemy_cop_walk_down" : "player_walk_down",
                        item.pos, height: 35, order: (int)item.pos.y).transform.localScale = Vector3.one * .45f;
                else
                    DrawSprite("props", item.pos, Prop(item.kind), 0, 32, (int)item.pos.y, color);
            }

            DrawSprite("props", CampaignModel.Checkpoint, 1, 0, 32, 510);
            int enemyIndex = 0;
            foreach (var enemy in model.State.enemies)
            {
                int variantIndex = enemyIndex++;
                if (enemy.kind == "boss" || enemy.kind == "elite")
                {
                    DrawBoss(enemy);
                    continue;
                }

                if (enemy.kind == "hobo" || enemy.kind == "thug" || enemy.kind == "knife" || enemy.kind == "thrower" || enemy.kind == "cop" || enemy.kind == "gunner")
                {
                    DrawCommonEnemy(enemy, variantIndex);
                    continue;
                }

                // Human enemies are handled above; only the rat uses the legacy-sized atlas.
                string atlas = "rat";
                if (enemy.dead)
                {
                    var corpse = DrawSprite(atlas, enemy.pos, 0, 0, atlas == "rat" ? 24 : 48, (int)enemy.pos.y - 1, Hex("756463"));
                    corpse.transform.rotation = Quaternion.Euler(0, 0, 90);
                    corpse.transform.localScale = Vector3.one * .8f;
                    continue;
                }

                Actor(atlas, enemy.pos, enemy.facing, enemy.windup <= 0, enemy.windup > 0 ? 0 : -1, enemy.flash > 0 ? Hex("ffb8a0") : enemy.windup > 0 ? Hex("ffcc8b") : Color.white);
            }

            DrawPlayer();
            foreach (var shot in model.shots)
            {
                var renderer = DrawSprite("props", shot.pos, shot.kind == "bullet" ? 5 : Prop(shot.kind), 0, 32, 700, shot.hostile ? Hex("ffcc7b") : Color.white);
                if (shot.kind == "bullet")
                    renderer.transform.localScale = new(.5f, .25f, 1);
                renderer.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(shot.velocity.y, shot.velocity.x) * Mathf.Rad2Deg);
            }

            if (model.State.chapter == 7 && model.State.room == 2 && model.Clear)
                DrawSprite("props", new(1268, 330), 6, 0, 32, 330);
            for (int i = cursor; i < pool.Count; i++)
                pool[i].gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            foreach (var sprite in sprites.Values)
                Destroy(sprite);
            if (material)
                Destroy(material);
        }
    }
}
