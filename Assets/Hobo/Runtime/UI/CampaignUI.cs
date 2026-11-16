using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace Hobo
{
    public sealed class CampaignUI : MonoBehaviour
    {
        CampaignModel model;
        TMP_FontAsset font;
        GameAudio audio;
        Canvas canvas;
        GameObject panel, eventSystem;
        TextMeshProUGUI hud, hint, toast, title, body;
        readonly List<UnityEngine.UI.Button> buttons = new();
        string shown = "";
        public void Initialize(CampaignModel session, TMP_FontAsset textFont, GameAudio gameAudio)
        {
            model = session;
            font = textFont;
            audio = gameAudio;
            Build();
        }

        static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }

        RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new(0, 1);
            rect.anchoredPosition = new(position.x, -position.y);
            rect.sizeDelta = size;
            return rect;
        }

        TextMeshProUGUI Label(string name, Transform parent, Vector2 position, Vector2 size, float fontSize, Color color)
        {
            var label = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        void Build()
        {
            var root = new GameObject("HUD", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new(960, 640);
            scaler.matchWidthOrHeight = 1;
            eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            hud = Label("HUD", root.transform, new(28, 14), new(905, 90), 20, Hex("e7cea0"));
            hint = Label("Controls", root.transform, new(28, 589), new(905, 25), 15, Hex("adc4b8"));
            toast = Label("Notice", root.transform, new(28, 614), new(905, 24), 15, Hex("e9c77d"));
            var backdrop = Rect("Menu", root.transform, Vector2.zero, new(960, 640));
            panel = backdrop.gameObject;
            backdrop.anchorMin = Vector2.zero;
            backdrop.anchorMax = Vector2.one;
            backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
            panel.AddComponent<UnityEngine.UI.Image>().color = new(.025f, .04f, .05f, .97f);
            var content = Rect("MenuContent", backdrop, new(0, 0), new(960, 640));
            content.anchorMin = content.anchorMax = content.pivot = new(.5f, .5f);
            content.anchoredPosition = Vector2.zero;
            title = Label("Heading", content, new(110, 90), new(740, 70), 36, Hex("e8c57a"));
            body = Label("Description", content, new(110, 175), new(740, 180), 21, Hex("c9d2c5"));
            for (int index = 0; index < 4; index++)
            {
                var rect = Rect("Action" + index, content, new(110, 360 + index * 54), new(740, 44));
                var background = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
                background.color = Hex("283c3b");
                var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = background;
                var colors = button.colors;
                colors.highlightedColor = Hex("d8c097");
                colors.selectedColor = Hex("d8c097");
                button.colors = colors;
                Label("Label", rect, new(16, 6), new(710, 32), 22, Hex("e8ddc7"));
                buttons.Add(button);
            }
        }

        void Button(int index, string label, Action action, bool enabled = true)
        {
            var button = buttons[index];
            button.interactable = enabled;
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TextMeshProUGUI>().text = label;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
        }

        public void Refresh()
        {
            var state = model.State;
            hud.text = $"{model.Chapter.place} / {state.room + 1}:3\nHP {state.hp}/{model.MaxHP}    {Rules.Name(state.weapon)}    {(state.outfit == "" ? "Старая куртка" : Rules.Name(state.outfit))}\n{(state.buff == "" ? "" : Rules.Name(state.buff) + " · " + Mathf.CeilToInt(state.buffLeft) + " сек")}";
            hint.text = model.Clear ? "Проход справа открыт →   E — предмет / чекушка" : "WASD — движение   ЛКМ — удар   Пробел — блок   ПКМ — бросок   E — взять   Esc — пауза";
            toast.text = model.notice;
            panel.SetActive(model.mode != "play");
            if (shown == model.mode)
                return;
            shown = model.mode;
            foreach (var button in buttons)
                button.gameObject.SetActive(false);
            switch (model.mode)
            {
                case "menu":
                    title.text = "A HOBO'S REVENG";
                    body.text = "СМОЛЬНОГОРСК-42 / 1990-е\nОдна бутылка. Очень длинная дорога домой.\nРанняя Unity-версия · 8 глав / 24 комнаты";
                    Button(0, "Новая игра", () => model.NewGame());
                    Button(1, "Продолжить с чекушки", () => model.Load(), File.Exists(model.savePath));
                    Button(2, "Управление и звук", () => model.mode = "settings");
                    Button(3, "Выйти", () => Application.Quit());
                    break;
                case "story":
                    title.text = $"{state.chapter + 1}. {model.Chapter.title}";
                    body.text = model.Chapter.story;
                    Button(0, "Пора возвращать своё", () => model.mode = "play");
                    break;
                case "pause":
                    title.text = "ПЕРЕКУР";
                    body.text = "Город подождёт. Бутылка — тем более.\nСохранение — у чекушки на уровне.";
                    Button(0, "Продолжить", () => model.mode = "play");
                    Button(1, "Загрузить чекушку", () => model.Load(), File.Exists(model.savePath));
                    Button(2, "В меню", () => model.mode = "menu");
                    break;
                case "dead":
                    title.text = "ПРИЛЁГ ОТДОХНУТЬ";
                    body.text = "Два серьёзных удара — и всё.\nБлок держит удары спереди. От пуль спасает укрытие.";
                    Button(0, "Вернуться к чекушке", () =>
                    {
                        if (!model.Load())
                            model.NewGame();
                    });
                    Button(1, "Новая игра", () => model.NewGame());
                    Button(2, "В меню", () => model.mode = "menu");
                    break;
                case "won":
                    title.text = "СВОЁ НЕ ОТДАЮТ";
                    body.text = $"Jopa Denni снова дома.\nПобеждено: {state.kills}. Время: {state.elapsed / 60:0} мин.\nКонец ранней версии.";
                    Button(0, "В меню", () => model.mode = "menu");
                    break;
                case "settings":
                    title.text = "УПРАВЛЕНИЕ";
                    body.text = "WASD / левый стик — движение. Мышь / правый стик — цель.\nЛКМ / RB — удар. Пробел / LB — блок. ПКМ / X — бросок.\nE / A — взять. Esc / Start — пауза. F11 — полный экран.";
                    Button(0, "Эффекты: включить / выключить", () => audio.ToggleMute());
                    Button(1, "Назад", () => model.mode = "menu");
                    break;
            }

            if (panel.activeSelf)
                foreach (var button in buttons)
                    if (button.gameObject.activeSelf && button.interactable)
                    {
                        button.Select();
                        break;
                    }
        }

        void OnDestroy()
        {
            if (canvas)
                Destroy(canvas.gameObject);
            if (eventSystem)
                Destroy(eventSystem);
        }
    }
}
