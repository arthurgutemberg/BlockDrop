using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelector : MonoBehaviour
{
    public string classicScene = "Main";     // nome da cena atual do jogo
    public string arcadeScene = "Arcade";    // nova cena que criaremos

    public void LoadClassic()
    {
        SceneManager.LoadScene(classicScene);
    }

    public void LoadArcade()
    {
        SceneManager.LoadScene(arcadeScene);
    }
}