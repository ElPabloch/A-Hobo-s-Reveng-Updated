using System;
using UnityEngine;

namespace Hobo
{
    public sealed class InventorySystem
    {
        readonly CampaignModel model;
        public InventorySystem(CampaignModel session) => model = session;
        public void Interact()
        {
            var state = model.State;
            if (state.chapter == 7 && state.room == 2 && model.Clear && Vector2.Distance(state.hero, new(820, 330)) < 55)
            {
                model.mode = "won";
                return;
            }

            if (Vector2.Distance(state.hero, CampaignModel.Checkpoint) < 42)
            {
                if (model.shots.Count > 0)
                {
                    model.notice = "Сначала дождись, пока уляжется стрельба.";
                    return;
                }

                model.Saves.Save();
                return;
            }

            Item nearest = null;
            float distance = 43;
            foreach (var item in state.items)
            {
                float candidate = Vector2.Distance(item.pos, state.hero);
                if (candidate < distance && model.Navigation.LOS(state.hero, item.pos))
                {
                    nearest = item;
                    distance = candidate;
                }
            }

            if (nearest == null)
                return;
            string kind = nearest.kind;
            if (kind == "beer" && state.hp >= model.MaxHP)
            {
                model.notice = "Пока цел. Пиво подождёт.";
                return;
            }

            state.items.Remove(nearest);
            if (Array.IndexOf(Rules.Weapons, kind) >= 0)
            {
                if (state.weapon != "")
                    state.items.Add(new(state.weapon, nearest.pos));
                state.weapon = kind;
            }
            else if (kind == "beer")
                state.hp = Mathf.Min(model.MaxHP, state.hp + 2);
            else if (kind == "coat" || kind == "shoes")
            {
                if (state.outfit != "")
                    state.items.Add(new(state.outfit, nearest.pos));
                state.outfit = kind;
                state.hp = Mathf.Min(state.hp, model.MaxHP);
            }
            else
            {
                state.buff = kind;
                state.buffLeft = kind == "rage" ? 15 : kind == "speed" ? 20 : 5;
            }

            model.notice = Rules.Name(kind);
            model.Sound?.Invoke("pickup");
        }
    }
}
