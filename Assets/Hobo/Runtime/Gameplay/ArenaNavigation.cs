using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public sealed class ArenaNavigation
    {
        readonly CampaignModel model;
        public ArenaNavigation(CampaignModel session)
        {
            model = session;
        }

        public bool Free(Vector2 p, float radius = 12)
        {
            if (p.x < CampaignModel.Area.xMin + radius || p.x > CampaignModel.Area.xMax - radius || p.y < CampaignModel.Area.yMin + radius || p.y > CampaignModel.Area.yMax - radius)
                return false;
            foreach (var w in model.walls)
            {
                var r = new Rect(w.x - radius, w.y - radius, w.width + radius * 2, w.height + radius * 2);
                if (r.Contains(p))
                    return false;
            }

            return true;
        }

        public Vector2 Move(Vector2 p, Vector2 delta, float radius = 12)
        {
            int n = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / 4));
            delta /= n;
            for (int i = 0; i < n; i++)
            {
                if (model.Navigation.Free(p + new Vector2(delta.x, 0), radius))
                    p.x += delta.x;
                if (model.Navigation.Free(p + new Vector2(0, delta.y), radius))
                    p.y += delta.y;
            }

            return p;
        }

        public bool LOS(Vector2 a, Vector2 b)
        {
            int n = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(a, b) / 4));
            for (int i = 0; i <= n; i++)
            {
                var p = Vector2.Lerp(a, b, i / (float)n);
                foreach (var w in model.walls)
                    if (w.Contains(p))
                        return false;
            }

            return true;
        }

        public Vector2 Pursuit(Vector2 from, Vector2 target)
        {
            if (model.Navigation.LOS(from, target) && model.Navigation.Free(from + (target - from).normalized * 24))
                return (target - from).normalized;
            const int cols = 56, rows = 19;
            int Cell(Vector2 p) => Mathf.Clamp(Mathf.RoundToInt((p.y - 140) / 24), 0, rows - 1) * cols + Mathf.Clamp(Mathf.RoundToInt((p.x - 44) / 24), 0, cols - 1);
            Vector2 Point(int c) => new(44 + (c % cols) * 24, 140 + (c / cols) * 24);
            int start = Cell(from), end = Cell(target);
            var previous = new int[cols * rows];
            Array.Fill(previous, -1);
            var queue = new Queue<int>();
            queue.Enqueue(start);
            previous[start] = start;
            while (queue.Count > 0)
            {
                int a = queue.Dequeue();
                if (a == end)
                    break;
                foreach (int d in new[]{-1, 1, -cols, cols})
                {
                    int b = a + d;
                    if (b < 0 || b >= previous.Length || Mathf.Abs(a % cols - b % cols) > 1 || previous[b] != -1 || !model.Navigation.Free(Point(b), 14))
                        continue;
                    previous[b] = a;
                    queue.Enqueue(b);
                }
            }

            if (previous[end] < 0)
                return Vector2.zero;
            int step = end;
            while (previous[step] != start && step != start)
                step = previous[step];
            return (Point(step) - from).normalized;
        }
    }
}
