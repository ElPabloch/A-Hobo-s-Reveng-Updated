using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    [Serializable]
    public class Shot
    {
        public string kind;
        public Vector2 pos, velocity;
        public bool hostile;
        public float life = 2;
    }
}
