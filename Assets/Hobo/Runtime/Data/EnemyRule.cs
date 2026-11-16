using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public struct EnemyRule
    {
        public int hp;
        public float speed, reach, windup;
        public string weapon;
        public EnemyRule(int h, float s, float r, float w, string a)
        {
            hp = h;
            speed = s;
            reach = r;
            windup = w;
            weapon = a;
        }
    }
}
