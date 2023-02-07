using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public sealed class MainMenuView: MonoBehaviour, IScreen
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        public event Action<bool> SwitchScreen = delegate {  };

        /// <summary>
        /// Change screen state on switch screen event
        /// </summary>
        /// <param name="state"></param>
        public void ChangeScreenState(bool state)
        {
            gameObject.SetActive(state);
        }

        /// <summary>
        /// Add subscribers to buttons
        /// </summary>
        private void Awake()
        {
            _playButton.onClick.AddListener(SwitchToLobby);
            _quitButton.onClick.AddListener(QuitGame);
        }

        /// <summary>
        /// Remove subscribers from buttons
        /// </summary>
        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Go from main menu to lobby
        /// </summary>
        private void SwitchToLobby()
        {
            SwitchScreen.Invoke(true);
            ChangeScreenState(false);
        }

        /// <summary>
        /// Close application
        /// </summary>
        private void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}