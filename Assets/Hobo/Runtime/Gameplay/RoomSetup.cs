using UnityEngine;

namespace Hobo
{
    /// <summary>
    /// Populates a fresh room from campaign data. This is the current prototype placement policy;
    /// replace this class when authored level layouts are introduced, not the combat systems.
    /// </summary>
    public sealed class RoomSetup
    {
        static readonly Vector2 PlayerSpawn = new(130, 340);
        static readonly Vector2[] EnemySpawns =
        {
            new(735, 240),
            new(745, 450),
            new(650, 330),
            new(950, 230),
            new(1080, 430),
            new(1190, 300),
            new(900, 510),
            new(1240, 480)
        };

        static readonly string[] RoomBuffs = { "rage", "speed", "shield" };

        public void Populate(SaveData state, Chapter chapter)
        {
            state.hero = PlayerSpawn;
            state.enemies.Clear();
            state.items.Clear();
            state.blood.Clear();

            SpawnEnemies(state, chapter.rooms[state.room]);
            PlaceSupplies(state);
        }

        static void SpawnEnemies(SaveData state, Room room)
        {
            for (int index = 0; index < room.enemies.Length; index++)
            {
                Vector2 position = EnemySpawns[index % EnemySpawns.Length];
                state.enemies.Add(new Enemy(room.enemies[index], position));
            }

            // Reinforcements use existing enemy types and have distinct spawn points.
            int additional = Mathf.Min(3, 20 - state.enemies.Count);
            for (int index = 0; index < additional; index++)
            {
                string kind = index == 0 ? "hobo" : "rat";
                state.enemies.Add(new Enemy(kind, EnemySpawns[(room.enemies.Length + index) % EnemySpawns.Length]));
            }
        }

        static void PlaceSupplies(SaveData state)
        {
            string weapon = state.room switch
            {
                0 => "brick",
                1 => "pipe",
                _ => "bat"
            };

            string buff = RoomBuffs[(state.chapter + state.room) % RoomBuffs.Length];
            state.items.Add(new Item("beer", new Vector2(210, 450)));
            state.items.Add(new Item(weapon, new Vector2(235, 220)));
            state.items.Add(new Item(buff, new Vector2(490, 340)));

            if (state.room == 1)
            {
                string outfit = state.chapter % 2 == 0 ? "shoes" : "coat";
                state.items.Add(new Item(outfit, new Vector2(200, 380)));
            }
        }
    }
}
