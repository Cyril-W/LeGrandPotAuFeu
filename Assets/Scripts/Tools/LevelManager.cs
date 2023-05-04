using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    [SerializeField] ThirdPersonController tPC;
    [SerializeField] float minYPlayerPos = -50f;

    Transform player;

    void OnValidate() {
        if (tPC == null) { tPC = FindObjectOfType<ThirdPersonController>(true); }
    }

    void Start() {
        if (tPC == null) { tPC = FindObjectOfType<ThirdPersonController>(true); }
        if (tPC != null && player == null) { player = tPC.transform; }
    }

    void FixedUpdate() {
        if (player != null && player.position.y <= minYPlayerPos) {
            ReloadScene();
        }
    }

    public void ApplicationQuit() {
        Application.Quit();
    }

    public void ReloadScene() {
        var scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
