using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public struct WeaponRule
    {
        public int damage;
        public float reach, delay;
        public WeaponRule(int d, float r, float t)
        {
            damage = d;
            reach = r;
            delay = t;
        }
    }
}
