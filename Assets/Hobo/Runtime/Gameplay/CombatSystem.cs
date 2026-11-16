using UnityEngine;

namespace Hobo
{
    public sealed class CombatSystem
    {
        public const float AttackAnimationDuration = .30f;
        readonly CampaignModel model;
        public CombatSystem(CampaignModel session) => model = session;
        public void Attack()
        {
            if (model.cooldown > 0)
                return;
            var weapon = Rules.Weapon(model.State.weapon);
            model.cooldown = weapon.delay;
            model.swing = AttackAnimationDuration;
            model.attackFacing = new Vector2(model.horizontalFacing, 0f);
            model.Sound?.Invoke("swing");
            foreach (var enemy in model.State.enemies)
            {
                if (enemy.dead)
                    continue;
                var offset = enemy.pos - model.State.hero;
                if (offset.magnitude <= weapon.reach && Vector2.Dot(model.facing, offset.normalized) > .25f && model.Navigation.LOS(model.State.hero, enemy.pos))
                    Hit(enemy, weapon.damage + (model.State.buff == "rage" ? 2 : 0));
            }
        }

        public void Hit(Enemy enemy, int damage)
        {
            if (enemy.dead)
                return;
            enemy.hp -= damage;
            enemy.flash = .15f;
            model.Sound?.Invoke("hit");
            if (enemy.kind != "boss" && enemy.kind != "elite")
            {
                enemy.windup = 0;
                enemy.cooldown = .4f;
            }

            if (enemy.hp > 0)
                return;
            enemy.dead = true;
            model.State.kills++;
            model.State.blood.Add(enemy.pos);
            string weapon = Rules.Enemy(enemy.kind).weapon;
            if (weapon != "")
                model.State.items.Add(new Item(weapon, enemy.pos));
        }

        public void Damage(int amount, Vector2 source, bool bullet)
        {
            if (model.hurt > 0 || model.State.buff == "shield")
                return;
            bool frontal = Vector2.Dot(model.facing, (source - model.State.hero).normalized) > .2f;
            if (model.blocking && !bullet && frontal)
            {
                model.Sound?.Invoke("block");
                return;
            }

            model.State.hp = Mathf.Max(0, model.State.hp - amount);
            model.hurt = .7f;
            model.Sound?.Invoke("hit");
        }

        public void Throw()
        {
            if (model.State.weapon == "" || model.cooldown > 0)
                return;
            model.pendingThrowWeapon = model.State.weapon;
            model.throwFacing = model.facing;
            model.State.weapon = "";
            model.cooldown = .3f;
            model.throwAnimation = .24f;
        }

        public void ReleaseThrowIfReady()
        {
            if (model.pendingThrowWeapon == "" || model.throwAnimation > .16f)
                return;
            model.Projectiles.Fire(model.State.hero, model.throwFacing, model.pendingThrowWeapon, false);
            model.pendingThrowWeapon = "";
        }
    }
}
