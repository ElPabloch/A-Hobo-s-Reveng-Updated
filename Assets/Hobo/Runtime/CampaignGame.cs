using System;
using System.IO;
using UnityEngine;

namespace Hobo
{
    public sealed class CampaignGame : MonoBehaviour
    {
        [NonSerialized]
        public CampaignModel model;
        public Camera gameCamera;
        public TMPro.TMP_FontAsset font;
        PlayerInputReader input;
        CampaignRenderer view;
        CampaignUI ui;
        GameAudio audio;
        void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            var content = JsonUtility.FromJson<CampaignContent>(Resources.Load<TextAsset>("Campaign").text.TrimStart('﻿'));
            model = new CampaignModel(content, Path.Combine(Application.persistentDataPath, "campaign_unity_v1.json"));
            input = new PlayerInputReader();
            audio = gameObject.AddComponent<GameAudio>();
            audio.Initialize();
            model.Sound = audio.Play;
            view = gameObject.AddComponent<CampaignRenderer>();
            view.Initialize(model);
            ui = gameObject.AddComponent<CampaignUI>();
            ui.Initialize(model, font, audio);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "--arena-preview") >= 0)
            {
                model.NewGame();
                model.mode = "play";
            }
        }

        void Update()
        {
            var frame = input.Read(gameCamera, model.State.hero, model.facing);
            if (frame.ToggleFullscreen)
                Screen.fullScreen = !Screen.fullScreen;
            if (frame.TogglePause)
                model.TogglePause();
            model.Tick(Time.deltaTime, frame.Movement, frame.Aim, frame.Block, frame.Attack, frame.Throw, frame.Interact);
            FollowPlayer();
            view.Refresh();
            ui.Refresh();
        }

        void FollowPlayer()
        {
            if (!gameCamera) return;
            var position = gameCamera.transform.position;
            float centerX = Mathf.Clamp(model.State.hero.x, 480f, CampaignModel.Area.xMax + 32f - 480f);
            position.x = centerX / GameCoordinates.SimulationPixelsPerWorldUnit;
            gameCamera.transform.position = position;
        }

        void OnDestroy()
        {
            if (model != null)
                model.Sound = null;
        }
    }
}
