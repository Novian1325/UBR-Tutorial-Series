using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Adds a Menu to the top of Unity to quickly switch between Scenes. NOTE: DOES NOT SAVE SCENE!
/// </summary>
public static class SceneMenu
{
    //TEMPLATE
    //[MenuItem("Scenes/Empire's Edge Galaxy Map")]
    //private static void LoadBoardScene()
    //{
    //    EditorSceneManager.OpenScene("Assets/Scenes/EmpiresEdge.unity");
    //}

    [MenuItem("SceneMenu/Workshop")]
    private static void LoadWorkshopScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Workshop.unity");
    }
    [MenuItem("SceneMenu/Network Test")]
    private static void LoadNetworkTestScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/NetworkTest.unity");
    }
    [MenuItem("SceneMenu/Start Master Server")]
    private static void LoadStartMasterServerScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Start Master Server.unity");
    }

}
