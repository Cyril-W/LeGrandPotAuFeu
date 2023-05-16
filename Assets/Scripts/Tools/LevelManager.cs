using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    [SerializeField] ThirdPersonController tPC;
    [SerializeField] float minYPlayerPos = -50f;

    void FixedUpdate() {
        if (GroupManager.Instance != null && GroupManager.Instance.GetPlayerPosition().y <= minYPlayerPos) {
            ReloadScene();
        }
    }

    public void ApplicationQuit() {
        Debug.Log("Exiting the game");
        Application.Quit();
    }

    public void ReloadScene() {
        Debug.Log("Reloading the scene");
        var scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
