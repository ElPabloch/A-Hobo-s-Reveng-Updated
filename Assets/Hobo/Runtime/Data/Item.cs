using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    [Serializable]
    public class Item
    {
        public string kind;
        public Vector2 pos;
        public Item(string k, Vector2 p)
        {
            kind = k;
            pos = p;
        }
    }
}
