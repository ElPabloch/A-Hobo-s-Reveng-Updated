using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    [Serializable]
    public class SaveData
    {
        public int version = 1, chapter, room, hp = 4, kills;
        public Vector2 hero;
        public string weapon = "", outfit = "", buff = "";
        public float buffLeft, elapsed;
        public List<Enemy> enemies = new();
        public List<Item> items = new();
        public List<Vector2> blood = new();
    }
}
