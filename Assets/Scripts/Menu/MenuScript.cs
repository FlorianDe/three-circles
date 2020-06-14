using Game;
using UnityEngine;

namespace Menu
{
    public class MenuScript : MonoBehaviour
    {
        public GameScript gameScript;

        public void StartGame()
        {
            gameScript.StartGame();
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}