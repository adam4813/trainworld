using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;
    // Start is called before the first frame update
    void Start()
    {
        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(OnLoadGame);
        quitButton.onClick.AddListener(Application.Quit);
        
    }

    private void OnLoadGame()
    {
        throw new System.NotImplementedException();
    }

    private void OnNewGame()
    {
        SceneManager.LoadScene(1);
    }
}
