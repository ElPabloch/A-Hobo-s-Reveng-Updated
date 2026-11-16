using UnityEngine;

namespace Hobo
{
    /// <summary>Updates player movement and action timers. Combat and inventory own their rules.</summary>
    public sealed class PlayerSystem
    {
        const float BaseMovementSpeed = 190f;
        const float BlockingSpeedMultiplier = 0.48f;
        const float SpeedBuffMultiplier = 1.45f;
        const float ShoesSpeedMultiplier = 1.15f;
        readonly CampaignModel model;

        public PlayerSystem(CampaignModel session)
        {
            model = session;
        }

        public void Tick(float deltaTime, Vector2 movement, Vector2 aim, bool block, bool attack, bool throwWeapon, bool interact)
        {
            UpdateTimers(deltaTime);
            UpdateMovement(deltaTime, movement, aim, block);
            ExecuteActions(block, attack, throwWeapon, interact);
        }

        void UpdateTimers(float deltaTime)
        {
            model.clock += deltaTime;
            model.State.elapsed += deltaTime;
            model.cooldown = Mathf.Max(0f, model.cooldown - deltaTime);
            model.hurt = Mathf.Max(0f, model.hurt - deltaTime);
            model.swing = Mathf.Max(0f, model.swing - deltaTime);
            model.throwAnimation = Mathf.Max(0f, model.throwAnimation - deltaTime);
            model.Combat.ReleaseThrowIfReady();
            model.State.buffLeft = Mathf.Max(0f, model.State.buffLeft - deltaTime);
            if (model.State.buffLeft == 0f)
                model.State.buff = "";
        }

        void UpdateMovement(float deltaTime, Vector2 movement, Vector2 aim, bool block)
        {
            if (aim.sqrMagnitude > 0.01f)
                model.facing = aim.normalized;
            if (Mathf.Abs(aim.x) > Mathf.Abs(aim.y))
                model.horizontalFacing = Mathf.Sign(aim.x);

            model.blocking = block;
            model.moving = movement.sqrMagnitude > 0.01f;
            float speed = GetMovementSpeed(block);
            Vector2 displacement = Vector2.ClampMagnitude(movement, 1f) * speed * deltaTime;
            model.State.hero = model.Navigation.Move(model.State.hero, displacement);
        }

        float GetMovementSpeed(bool block)
        {
            float speed = BaseMovementSpeed;
            if (block) speed *= BlockingSpeedMultiplier;
            if (model.State.buff == "speed") speed *= SpeedBuffMultiplier;
            if (model.State.outfit == "shoes") speed *= ShoesSpeedMultiplier;
            return speed;
        }

        void ExecuteActions(bool block, bool attack, bool throwWeapon, bool interact)
        {
            if (!block && attack) model.Combat.Attack();
            if (!block && throwWeapon) model.Combat.Throw();
            if (interact) model.Inventory.Interact();
        }
    }
}
