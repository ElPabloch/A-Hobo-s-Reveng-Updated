using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    [Serializable]
    public class Enemy
    {
        public string kind;
        public Vector2 pos, facing = Vector2.left;
        public int hp;
        public float cooldown = 1, windup, flash;
        [NonSerialized] public float animationRecovery;
        [NonSerialized] public Vector2 animationFacing = Vector2.left;
        [NonSerialized] public float horizontalFacing = -1f;
        [NonSerialized] public string animationAction = "attack";
        public bool dead;
        public Enemy(string k, Vector2 p)
        {
            kind = k;
            pos = p;
            hp = Rules.Enemy(k).hp;
        }
    }
}
