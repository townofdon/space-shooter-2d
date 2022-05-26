using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using Audio;
using Core;
using Game;
using Enemies;
using Player;

namespace UI {

    public class MainMenu : MonoBehaviour {

        [Header("State")]
        [Space]
        [SerializeField] GameStateSO gameState;
        [SerializeField] PlayerStateSO playerState;

        [Header("Menus")]
        [Space]
        [SerializeField] GameObject canvasLevelStart;
        [SerializeField] GameObject canvasChooseDifficultyMenu;
        [SerializeField] GameObject canvasChooseShipMenu;

        [Header("LevelStart")]
        [Space]
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI subTitle;
        [SerializeField] GameObject startButton;
        [SerializeField] GameObject quitButton;

        [Header("ChooseDifficulty")]
        [Space]
        [SerializeField] Button easyButton;
        [SerializeField] Button mediumButton;
        [SerializeField] Button hardButton;
        [SerializeField] Button insaneButton;

        [Header("ChooseShip")]
        [Space]
        [SerializeField] Button redShipButton;
        [SerializeField] Button yellowShipButton;
        [SerializeField] Button blueShipButton;
        [SerializeField] Button greenShipButton;
        [SerializeField] Transform playerExitTarget;
        [SerializeField] float timeDelayFirstLevel = 3f;

        [Header("Timing")]
        [Space]
        [SerializeField] float delayShowTitle = 1.5f;
        [SerializeField] float delayShowSubtitle = 0.5f;
        [SerializeField] float delayShowButtons = 0.5f;
        [SerializeField] float timeCycleColors = 1.5f;

        [Header("Colors")]
        [Space]
        [SerializeField] Gradient cycleGradient;
        [SerializeField] Color buttonActiveColor;
        [SerializeField] Color buttonInactiveColor;


        Timer colorCycler = new Timer(TimerDirection.Increment);
        Timer colorFinalizer = new Timer(TimerDirection.Increment, TimerStep.DeltaTime, 0.2f);

        Color initialTitleColor;
        Color initialSubtitleColor;

        UIButton uiButtonStart;
        UIButton uiButtonQuit;

        UIButton uiButtonEasy;
        UIButton uiButtonMedium;
        UIButton uiButtonHard;
        UIButton uiButtonInsane;

        UIButton uiButtonRedShip;
        UIButton uiButtonYellowShip;
        UIButton uiButtonBlueShip;
        UIButton uiButtonGreenShip;

        bool everFocused = false;

        Coroutine ieStart;

        public void OnSelectLevelStart(BaseEventData eventData) {
            OnFocusSound();
            HandleSelectLevelStart(eventData);
        }

        public void OnSelectChooseDifficulty(BaseEventData eventData) {
            OnFocusSound();
            HandleSelectChooseDifficulty(eventData);
        }

        public void OnSelectChooseShip(BaseEventData eventData) {
            OnFocusSound();
            HandleSelectChooseShip(eventData);
        }

        public void OnStart() {
            GotoChooseDifficultyMenu();
        }

        public void OnQuit() {
            Application.Quit();
        }

        public void OnChooseEasy() {
            GameManager.current.SetDifficulty(GameDifficulty.Easy);
            GotoChooseShipMenu();
        }

        public void OnChooseMedium() {
            GameManager.current.SetDifficulty(GameDifficulty.Medium);
            GotoChooseShipMenu();
        }

        public void OnChooseHard() {
            GameManager.current.SetDifficulty(GameDifficulty.Hard);
            GotoChooseShipMenu();
        }

        public void OnChooseInsane() {
            GameManager.current.SetDifficulty(GameDifficulty.Insane);
            GotoChooseShipMenu();
        }

        public void OnChooseRedShip() {
            GameManager.current.SetPlayerShipColor(PlayerShipColor.Red);
            GotoFirstLevel();
        }

        public void OnChooseYellowShip() {
            GameManager.current.SetPlayerShipColor(PlayerShipColor.Yellow);
            GotoFirstLevel();

        }

        public void OnChooseBlueShip() {
            GameManager.current.SetPlayerShipColor(PlayerShipColor.Blue);
            GotoFirstLevel();
        }

        public void OnChooseGreenShip() {
            GameManager.current.SetPlayerShipColor(PlayerShipColor.Green);
            GotoFirstLevel();
        }

        void Awake() {
            initialTitleColor = title.color;
            initialSubtitleColor = subTitle.color;

            title.color = new Color(0, 0, 0, 0);
            subTitle.color = new Color(0, 0, 0, 0);

            uiButtonStart = new UIButton(startButton.GetComponent<Button>());
            uiButtonQuit = new UIButton(quitButton.GetComponent<Button>());
            uiButtonEasy = new UIButton(easyButton);
            uiButtonMedium = new UIButton(mediumButton);
            uiButtonHard = new UIButton(hardButton);
            uiButtonInsane = new UIButton(insaneButton);
            uiButtonRedShip = new UIButton(redShipButton);
            uiButtonBlueShip = new UIButton(blueShipButton);
            uiButtonYellowShip = new UIButton(yellowShipButton);
            uiButtonGreenShip = new UIButton(greenShipButton);
        }

        void Start() {
            AudioManager.current.PlayTrack("starlord-main-theme");
            GameManager.current.NewGame();

            startButton.SetActive(false);
            quitButton.SetActive(false);

            canvasLevelStart.SetActive(true);
            canvasChooseDifficultyMenu.SetActive(false);
            canvasChooseShipMenu.SetActive(false);


            if (ieStart != null) StopCoroutine(ieStart);
            ieStart = StartCoroutine(IStart());
        }

        void GotoChooseDifficultyMenu() {
            AudioManager.current.PlaySound("MenuSelect");
            everFocused = false;
            canvasLevelStart.SetActive(false);
            canvasChooseDifficultyMenu.SetActive(true);
            mediumButton.Select();
        }

        void GotoChooseShipMenu() {
            AudioManager.current.PlaySound("MenuSelect");
            everFocused = false;
            canvasChooseShipMenu.SetActive(true);
            canvasChooseDifficultyMenu.SetActive(false);
            redShipButton.Select();
        }

        void GotoFirstLevel() {
            AudioManager.current.PlaySound("MenuSelect");
            canvasChooseShipMenu.SetActive(false);
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            spawner.StopBattle();
            spawner.DestroyAllEnemiesPresent(true);
            AudioManager.current.CueTrack("starlord-transition");
            GameManager.current.RespawnPlayerShip(playerExitTarget);
            AudioManager.current.PlaySound("ship-whoosh");
            StartCoroutine(IGotoFirstLevel());
        }

        void OnFocusSound() {
            if (everFocused) AudioManager.current.PlaySound("MenuFocus");
            everFocused = true;
        }

        void HandleSelectLevelStart(BaseEventData eventData) {
            if (eventData.selectedObject == startButton) {
                SelectButton(uiButtonStart);
                DeselectButton(uiButtonQuit);
            } else {
                SelectButton(uiButtonQuit);
                DeselectButton(uiButtonStart);
            }
        }

        void HandleSelectChooseDifficulty(BaseEventData eventData) {
            if (eventData.selectedObject == easyButton.gameObject) {
                SelectButton(uiButtonEasy);
                DeselectButton(uiButtonMedium);
                DeselectButton(uiButtonHard);
                DeselectButton(uiButtonInsane);
            } else if (eventData.selectedObject == mediumButton.gameObject) {
                DeselectButton(uiButtonEasy);
                SelectButton(uiButtonMedium);
                DeselectButton(uiButtonHard);
                DeselectButton(uiButtonInsane);
            } else if (eventData.selectedObject == hardButton.gameObject) {
                DeselectButton(uiButtonEasy);
                DeselectButton(uiButtonMedium);
                SelectButton(uiButtonHard);
                DeselectButton(uiButtonInsane);
            } else {
                DeselectButton(uiButtonEasy);
                DeselectButton(uiButtonMedium);
                DeselectButton(uiButtonHard);
                SelectButton(uiButtonInsane);
            }
        }

        void HandleSelectChooseShip(BaseEventData eventData) {
            if (eventData.selectedObject == redShipButton.gameObject) {
                SelectButton(uiButtonRedShip);
                DeselectButton(uiButtonYellowShip);
                DeselectButton(uiButtonBlueShip);
                DeselectButton(uiButtonGreenShip);
            } else if (eventData.selectedObject == yellowShipButton.gameObject) {
                DeselectButton(uiButtonRedShip);
                SelectButton(uiButtonYellowShip);
                DeselectButton(uiButtonBlueShip);
                DeselectButton(uiButtonGreenShip);
            } else if (eventData.selectedObject == blueShipButton.gameObject) {
                DeselectButton(uiButtonRedShip);
                DeselectButton(uiButtonYellowShip);
                SelectButton(uiButtonBlueShip);
                DeselectButton(uiButtonGreenShip);
            } else {
                DeselectButton(uiButtonRedShip);
                DeselectButton(uiButtonYellowShip);
                DeselectButton(uiButtonBlueShip);
                SelectButton(uiButtonGreenShip);
            }
        }

        void SelectButton(UIButton uiButton) {
            uiButton.SetTextColorInherit();
        }

        void DeselectButton(UIButton uiButton) {
            uiButton.SetTextColorInitial();
        }

        IEnumerator IStart() {
            yield return new WaitForSeconds(delayShowTitle);
            yield return ICycleTextColor(title, initialTitleColor);
            yield return new WaitForSeconds(delayShowSubtitle);
            yield return ICycleTextColor(subTitle, initialSubtitleColor);
            yield return new WaitForSeconds(delayShowButtons);

            startButton.SetActive(true);
            quitButton.SetActive(true);
            uiButtonStart.button.Select();
        }

        IEnumerator ICycleTextColor(TextMeshProUGUI elem, Color finalColor) {
            colorCycler.SetDuration(timeCycleColors);
            colorCycler.Start();

            while (colorCycler.active) {
                elem.color = cycleGradient.Evaluate(colorCycler.value);
                colorCycler.Tick();
                yield return null;
            }

            colorFinalizer.SetDuration(0.1f);
            colorFinalizer.Start();
            Color temp = elem.color;

            while (colorFinalizer.active) {
                elem.color = Color.Lerp(temp, finalColor, colorFinalizer.value);
                colorFinalizer.Tick();
                yield return null;
            }

            elem.color = finalColor;
        }

        IEnumerator IGotoFirstLevel() {
            yield return new WaitForSeconds(timeDelayFirstLevel);
            GameManager.current.GotoNextLevel(GameManager.current.difficulty == GameDifficulty.Insane ? 1 : 0);
        }
    }
}

