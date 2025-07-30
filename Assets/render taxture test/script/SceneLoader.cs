using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadSceneAsync("scene_room_01", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("scene_room_02", LoadSceneMode.Additive);
    }


}
