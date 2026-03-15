using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DirectorsEye.Core
{
    [Serializable]
    public class SceneGeneratedEvent : UnityEvent<GameObject> { }

    public class SceneGenerationManager : MonoBehaviour
    {
        public static SceneGenerationManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string backendUrl = "http://localhost:8000";
        [SerializeField] private Transform sceneRoot;
        [SerializeField] private Material defaultEnvironmentMaterial;

        [Header("Events")]
        public SceneGeneratedEvent OnSceneGenerated;
        public UnityEvent OnGenerationStarted;
        public UnityEvent<string> OnGenerationFailed;

        private MarbleAPIClient _marbleClient;
        private GameObject _currentScene;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _marbleClient = new MarbleAPIClient(backendUrl);
            
            if (sceneRoot == null)
            {
                var rootObj = new GameObject("GeneratedSceneRoot");
                sceneRoot = rootObj.transform;
            }
        }

        public async Task GenerateSceneFromPrompt(string prompt, SceneStyle style = null)
        {
            OnGenerationStarted?.Invoke();
            Debug.Log($"[DirectorsEye] Generating scene: {prompt}");

            try
            {
                if (_currentScene != null)
                {
                    Destroy(_currentScene);
                }

                var request = new SceneGenerationRequest
                {
                    prompt = prompt,
                    style = style ?? SceneStyle.Cinematic,
                    outputFormat = "glb"
                };

                var sceneData = await _marbleClient.GenerateEnvironment(request);
                _currentScene = await LoadSceneFromData(sceneData);
                _currentScene.transform.SetParent(sceneRoot);

                OnSceneGenerated?.Invoke(_currentScene);
                Debug.Log("[DirectorsEye] Scene generation complete");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DirectorsEye] Generation failed: {ex.Message}");
                OnGenerationFailed?.Invoke(ex.Message);
            }
        }

        public async Task RefineScene(string refinementPrompt)
        {
            if (_currentScene == null)
            {
                Debug.LogWarning("[DirectorsEye] No scene to refine");
                return;
            }

            OnGenerationStarted?.Invoke();
            Debug.Log($"[DirectorsEye] Refining scene: {refinementPrompt}");

            try
            {
                var request = new SceneRefinementRequest
                {
                    refinementPrompt = refinementPrompt,
                    preserveLayout = true
                };

                var sceneData = await _marbleClient.RefineEnvironment(request);
                
                Destroy(_currentScene);
                _currentScene = await LoadSceneFromData(sceneData);
                _currentScene.transform.SetParent(sceneRoot);

                OnSceneGenerated?.Invoke(_currentScene);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DirectorsEye] Refinement failed: {ex.Message}");
                OnGenerationFailed?.Invoke(ex.Message);
            }
        }

        private async Task<GameObject> LoadSceneFromData(SceneData data)
        {
            // GLB/GLTF loading - using GLTFUtility or similar
            // For hackathon MVP, we'll use a simplified loader
            var sceneObj = new GameObject("GeneratedEnvironment");
            
            if (data.meshData != null)
            {
                var meshFilter = sceneObj.AddComponent<MeshFilter>();
                var meshRenderer = sceneObj.AddComponent<MeshRenderer>();
                
                meshFilter.mesh = MeshSerializer.Deserialize(data.meshData);
                meshRenderer.material = defaultEnvironmentMaterial;
                
                if (data.textureData != null)
                {
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(data.textureData);
                    meshRenderer.material.mainTexture = texture;
                }
            }

            // Add collider for spatial interaction
            var collider = sceneObj.AddComponent<MeshCollider>();
            collider.sharedMesh = sceneObj.GetComponent<MeshFilter>()?.mesh;

            return sceneObj;
        }

        public void ClearScene()
        {
            if (_currentScene != null)
            {
                Destroy(_currentScene);
                _currentScene = null;
            }
        }
    }

    [Serializable]
    public class SceneStyle
    {
        public string name;
        public string mood;
        public string lighting;
        public float detailLevel;

        public static SceneStyle Cinematic => new SceneStyle
        {
            name = "cinematic",
            mood = "dramatic",
            lighting = "volumetric",
            detailLevel = 0.8f
        };

        public static SceneStyle Noir => new SceneStyle
        {
            name = "noir",
            mood = "moody",
            lighting = "high_contrast",
            detailLevel = 0.7f
        };
    }
}
