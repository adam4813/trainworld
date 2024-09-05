using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;
    
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        newGameButton.onClick.AddListener(OnNewGame);
        loadGameButton.onClick.AddListener(OnLoadGame);
        quitButton.onClick.AddListener(Application.Quit);
    }

    private void OnLoadGame()
    {
        audioSource.Play();
        throw new System.NotImplementedException();
    }

    private void OnNewGame()
    {
        audioSource.Play();
        SceneManager.LoadScene(1);
    }
}
