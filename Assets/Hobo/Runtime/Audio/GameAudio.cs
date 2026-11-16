using System.Collections.Generic;
using UnityEngine;

namespace Hobo
{
    public sealed class GameAudio : MonoBehaviour
    {
        AudioSource source;
        readonly Dictionary<string, AudioClip> clips = new();
        public float Volume { get; private set; } = .35f;
        public void ToggleMute() => Volume = Volume > 0 ? 0 : .35f;
        public void Initialize()
        {
            source = gameObject.AddComponent<AudioSource>();
            foreach (string name in new[]{"hit", "swing", "block", "shoot", "pickup", "save"})
                clips.Add(name, CreateEffect(name));
        }

        static AudioClip CreateEffect(string name)
        {
            int count = name == "save" ? 6600 : 3300;
            float frequency = name == "hit" ? 85 : name == "block" ? 180 : name == "shoot" ? 65 : 440;
            var samples = new float[count];
            var random = new System.Random(42);
            for (int i = 0; i < count; i++)
            {
                float envelope = 1 - i / (float)count;
                float wave = Mathf.Sin(2 * Mathf.PI * frequency * i / 22050f) * envelope;
                if (name == "hit" || name == "shoot")
                    wave = wave * .35f + ((float)random.NextDouble() * 2 - 1) * .35f * envelope;
                samples[i] = wave * .25f;
            }

            var clip = AudioClip.Create(name, count, 1, 22050, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void Play(string name)
        {
            if (clips.TryGetValue(name, out var clip))
                source.PlayOneShot(clip, Volume);
        }

        void OnDestroy()
        {
            foreach (var clip in clips.Values)
                Destroy(clip);
        }
    }
}
