
namespace CCDemo.Tests
{
    using UnityEngine.TestTools;

    #if UNITY_EDITOR
    public class GameTestPrebuildSetup : IPrebuildSetup
    {
        public void Setup()
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
        }
    }
    #endif
}
