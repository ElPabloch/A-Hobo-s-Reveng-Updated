using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hobo
{
    public class CampaignModel
    {
        readonly RoomSetup roomSetup = new();
        public readonly CampaignContent content;
        public SaveData State = new();
        public Rect[] walls;
        public List<Shot> shots = new();
        public Vector2 facing = Vector2.right;
        public Vector2 attackFacing = Vector2.right;
        public float horizontalFacing = 1f;
        public Vector2 throwFacing = Vector2.right;
        public string pendingThrowWeapon = "";
        public float cooldown, hurt, swing, throwAnimation, clock;
        public bool blocking, moving;
        public string mode = "menu", notice = "";
        public int revision;
        public Action<string> Sound;
        public string savePath;
        public static readonly Rect Area = new(32, 128, 1344, 448), Exit = new(1314, 293, 62, 80);
        public static readonly Vector2 Checkpoint = new(125, 510);
        public int MaxHP => State.outfit == "coat" ? 6 : 4;
        public Chapter Chapter => content.chapters[State.chapter];
        public bool Clear => State.enemies.TrueForAll(e => e.dead);
        public ArenaNavigation Navigation { get; }

        public CombatSystem Combat { get; }

        public InventorySystem Inventory { get; }

        public CheckpointStore Saves { get; }

        public EnemySystem Enemies { get; }

        public ProjectileSystem Projectiles { get; }

        public PlayerSystem Player { get; }

        public CampaignModel(CampaignContent c, string path)
        {
            content = c;
            savePath = path;
            Navigation = new(this);
            Combat = new(this);
            Inventory = new(this);
            Saves = new(this);
            Enemies = new(this);
            Projectiles = new(this);
            Player = new(this);
            Setup();
        }

        public void NewGame()
        {
            State = new SaveData();
            Setup();
            mode = "story";
        }

        public void TogglePause()
        {
            if (mode == "play")
                mode = "pause";
            else if (mode == "pause")
                mode = "play";
        }

        public void Setup()
        {
            walls = Rules.Walls(State.chapter * 3 + State.room);
            roomSetup.Populate(State, Chapter);
            shots.Clear();
            cooldown = hurt = swing = throwAnimation = 0;
            blocking = false;
            facing = Vector2.right;
            attackFacing = facing;
            horizontalFacing = 1f;
            throwFacing = facing;
            pendingThrowWeapon = "";
            revision++;
        }

        public void Tick(float dt, Vector2 move, Vector2 aim, bool block, bool attack, bool toss, bool interact)
        {
            if (mode != "play")
                return;
            dt = Mathf.Clamp(dt, 0, .05f);
            Player.Tick(dt, move, aim, block, attack, toss, interact);
            Enemies.Tick(dt);
            Projectiles.Tick(dt);
            if (State.hp <= 0)
                mode = "dead";
            if (mode == "play" && Clear && Exit.Contains(State.hero) && !(State.chapter == 7 && State.room == 2))
            {
                State.room++;
                if (State.room == 3)
                {
                    State.room = 0;
                    State.chapter++;
                    mode = "story";
                }

                Setup();
            }
        }

        // Facade keeps command callers and regression checks independent from service composition.
        public bool Free(Vector2 p, float radius = 12) => Navigation.Free(p, radius);
        public Vector2 Move(Vector2 p, Vector2 delta, float radius = 12) => Navigation.Move(p, delta, radius);
        public bool LOS(Vector2 a, Vector2 b) => Navigation.LOS(a, b);
        public Vector2 Pursuit(Vector2 from, Vector2 target) => Navigation.Pursuit(from, target);
        public void Attack() => Combat.Attack();
        public void Hit(Enemy enemy, int damage) => Combat.Hit(enemy, damage);
        public void Damage(int amount, Vector2 source, bool bullet) => Combat.Damage(amount, source, bullet);
        public void Throw() => Combat.Throw();
        public void Interact() => Inventory.Interact();
        public bool Save() => Saves.Save();
        public bool Load() => Saves.Load();
        public bool Valid(SaveData data) => Saves.Valid(data);
    }
}
