using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class MainMenuView: MonoBehaviour, IScreen
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _quitButton;

        public event Action<bool> SwitchScreen = delegate {  };

        public void ChangeScreenState(bool state)
        {
            gameObject.SetActive(state);
        }

        private void Awake()
        {
            _playButton.onClick.AddListener(SwitchToLobby);
            _quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
        }

        private void SwitchToLobby()
        {
            SwitchScreen.Invoke(true);
            ChangeScreenState(false);
        }

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