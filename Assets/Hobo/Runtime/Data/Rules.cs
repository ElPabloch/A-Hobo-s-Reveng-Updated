using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public static class Rules
    {
        public static readonly string[] Weapons = {"", "knife", "pipe", "bat", "brick"};
        public static WeaponRule Weapon(string k) => k switch
        {
        "knife" => new(1, 46, .20f), "pipe" => new(1, 80, .38f), "bat" => new(2, 76, .55f), "brick" => new(1, 42, .36f), _ => new(1, 46, .30f)}

        ;
        public static EnemyRule Enemy(string k) => k switch
        {
        "rat" => new(1, 98, 33, .55f, ""), "hobo" => new(3, 73, 42, .62f, "brick"), "thug" => new(4, 78, 64, .70f, "pipe"), "knife" => new(3, 108, 40, .50f, "knife"), "thrower" => new(3, 65, 260, .90f, "brick"), "cop" => new(5, 77, 65, .70f, "bat"), "gunner" => new(4, 57, 390, 1.10f, ""), "elite" => new(12, 64, 78, .95f, "bat"), "boss" => new(24, 68, 95, .95f, "bat"), _ => throw new ArgumentException(k)}

        ;
        public static string Name(string k) => k switch
        {
        "" => "Кулаки", "knife" => "Нож", "pipe" => "Арматура", "bat" => "Бита", "brick" => "Кирпич", "beer" => "Побрика-7 · HP +2", "rage" => "Исели-9 · урон +2", "speed" => "Быстрое · скорость", "shield" => "Бессмертное · защита", "coat" => "Форма · максимум HP +2", "shoes" => "Кроссовки · скорость +15%", _ => k
        }

        ;
        public static Rect[] Walls(int i) => (i % 6)switch
        {
        0 => new[]{new Rect(340, 190, 48, 120), new Rect(565, 375, 48, 120)}, 1 => new[]{new Rect(320, 250, 100, 42), new Rect(560, 400, 110, 42)}, 2 => new[]{new Rect(430, 175, 50, 100), new Rect(430, 435, 50, 95)}, 3 => new[]{new Rect(300, 200, 50, 210), new Rect(570, 300, 50, 205)}, 4 => new[]{new Rect(340, 210, 60, 55), new Rect(550, 290, 65, 60), new Rect(350, 440, 60, 55)}, _ => new[]{new Rect(360, 240, 50, 65), new Rect(570, 420, 50, 65)}}

        ;
    }
}
