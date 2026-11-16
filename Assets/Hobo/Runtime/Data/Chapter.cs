using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    [Serializable]
    public class Chapter
    {
        public string title, place, story, end, floor, accent, wall, sign;
        public Room[] rooms;
    }
}
