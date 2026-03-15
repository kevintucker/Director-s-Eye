using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DirectorsEye.Core
{
    [Serializable]
    public class SceneGenerationRequest
    {
        public string prompt;
        public SceneStyle style;
        public string outputFormat;
    }

    [Serializable]
    public class SceneRefinementRequest
    {
        public string refinementPrompt;
        public bool preserveLayout;
    }

    [Serializable]
    public class SceneData
    {
        public byte[] meshData;
        public byte[] textureData;
        public string metadata;
    }

    [Serializable]
    public class APIResponse
    {
        public bool success;
        public string error;
        public string sceneDataBase64;
        public string textureDataBase64;
        public string metadata;
    }

    public class MarbleAPIClient
    {
        private readonly string _baseUrl;
        private string _sessionId;

        public MarbleAPIClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _sessionId = Guid.NewGuid().ToString();
        }

        public async Task<SceneData> GenerateEnvironment(SceneGenerationRequest request)
        {
            var endpoint = $"{_baseUrl}/api/generate";
            var json = JsonUtility.ToJson(new GenerateRequestPayload
            {
                prompt = request.prompt,
                style_name = request.style?.name ?? "cinematic",
                style_mood = request.style?.mood ?? "dramatic",
                style_lighting = request.style?.lighting ?? "natural",
                detail_level = request.style?.detailLevel ?? 0.8f,
                output_format = request.outputFormat ?? "glb",
                session_id = _sessionId
            });

            return await PostRequest(endpoint, json);
        }

        public async Task<SceneData> RefineEnvironment(SceneRefinementRequest request)
        {
            var endpoint = $"{_baseUrl}/api/refine";
            var json = JsonUtility.ToJson(new RefineRequestPayload
            {
                refinement_prompt = request.refinementPrompt,
                preserve_layout = request.preserveLayout,
                session_id = _sessionId
            });

            return await PostRequest(endpoint, json);
        }

        private async Task<SceneData> PostRequest(string url, string jsonBody)
        {
            using var request = new UnityWebRequest(url, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"API request failed: {request.error}");
            }

            var response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
            
            if (!response.success)
            {
                throw new Exception($"API error: {response.error}");
            }

            return new SceneData
            {
                meshData = Convert.FromBase64String(response.sceneDataBase64),
                textureData = !string.IsNullOrEmpty(response.textureDataBase64) 
                    ? Convert.FromBase64String(response.textureDataBase64) 
                    : null,
                metadata = response.metadata
            };
        }

        [Serializable]
        private class GenerateRequestPayload
        {
            public string prompt;
            public string style_name;
            public string style_mood;
            public string style_lighting;
            public float detail_level;
            public string output_format;
            public string session_id;
        }

        [Serializable]
        private class RefineRequestPayload
        {
            public string refinement_prompt;
            public bool preserve_layout;
            public string session_id;
        }
    }

    public static class MeshSerializer
    {
        public static Mesh Deserialize(byte[] data)
        {
            // Simplified mesh deserialization for hackathon
            // In production, use GLTFUtility or UnityGLTF
            var mesh = new Mesh();
            
            // Parse GLB/GLTF binary data
            // This is a placeholder - actual implementation would use a GLTF library
            Debug.Log("[MeshSerializer] Deserializing mesh data...");
            
            return mesh;
        }
    }
}
