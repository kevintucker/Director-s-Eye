using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using DirectorsEye.VR;

namespace DirectorsEye.Core
{
    [Serializable]
    public class ShotCapturedEvent : UnityEvent<ShotData> { }

    public class ShotCaptureManager : MonoBehaviour
    {
        public static ShotCaptureManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string projectName = "Untitled Project";
        [SerializeField] private string outputDirectory;

        [Header("References")]
        [SerializeField] private VirtualCameraRig activeCamera;

        [Header("Events")]
        public ShotCapturedEvent OnShotCaptured;
        public UnityEvent<List<ShotData>> OnShotListUpdated;

        private List<ShotData> _shots = new List<ShotData>();
        private int _shotCounter = 1;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(Application.persistentDataPath, "DirectorsEye", "Shots");
            }

            Directory.CreateDirectory(outputDirectory);
        }

        public void SetActiveCamera(VirtualCameraRig camera)
        {
            activeCamera = camera;
        }

        public ShotData CaptureShot(string description = "")
        {
            if (activeCamera == null)
            {
                Debug.LogWarning("[ShotCapture] No active camera set");
                return null;
            }

            var frame = activeCamera.CaptureFrame();
            var cameraMetadata = activeCamera.GetMetadata();
            var actors = FindAllActors();

            var shot = new ShotData
            {
                shotId = $"shot_{_shotCounter:D3}",
                shotNumber = _shotCounter,
                description = description,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                cameraPosition = cameraMetadata.position,
                cameraRotation = cameraMetadata.rotation,
                focalLength = cameraMetadata.focalLength,
                fieldOfView = cameraMetadata.fieldOfView,
                aspectRatio = cameraMetadata.aspectRatio,
                actors = actors,
                framePath = SaveFrame(frame, _shotCounter)
            };

            _shots.Add(shot);
            _shotCounter++;

            OnShotCaptured?.Invoke(shot);
            OnShotListUpdated?.Invoke(_shots);

            Debug.Log($"[ShotCapture] Captured {shot.shotId}: {shot.description}");
            return shot;
        }

        public void CaptureShot()
        {
            CaptureShot("");
        }

        private List<ActorData> FindAllActors()
        {
            var actors = new List<ActorData>();
            var actorObjects = FindObjectsOfType<ActorPlaceholder>();

            foreach (var actor in actorObjects)
            {
                actors.Add(actor.GetActorData());
            }

            return actors;
        }

        private string SaveFrame(Texture2D frame, int shotNumber)
        {
            var filename = $"{projectName}_shot_{shotNumber:D3}.png";
            var path = Path.Combine(outputDirectory, filename);

            var pngData = frame.EncodeToPNG();
            File.WriteAllBytes(path, pngData);

            Destroy(frame);

            return path;
        }

        public void ExportShotList(string format = "json")
        {
            var exportPath = Path.Combine(outputDirectory, $"{projectName}_shotlist.{format}");

            switch (format.ToLower())
            {
                case "json":
                    ExportAsJson(exportPath);
                    break;
                case "csv":
                    ExportAsCsv(exportPath);
                    break;
                default:
                    ExportAsJson(exportPath);
                    break;
            }

            Debug.Log($"[ShotCapture] Exported shot list to {exportPath}");
        }

        private void ExportAsJson(string path)
        {
            var export = new ShotListExport
            {
                projectName = projectName,
                exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                totalShots = _shots.Count,
                shots = _shots
            };

            var json = JsonUtility.ToJson(export, true);
            File.WriteAllText(path, json);
        }

        private void ExportAsCsv(string path)
        {
            using var writer = new StreamWriter(path);
            
            writer.WriteLine("Shot,Description,Focal Length,FOV,Aspect Ratio,Camera X,Camera Y,Camera Z,Actors,Frame Path");
            
            foreach (var shot in _shots)
            {
                var actorNames = string.Join("; ", shot.actors.ConvertAll(a => a.label));
                writer.WriteLine($"{shot.shotNumber},\"{shot.description}\",{shot.focalLength}mm,{shot.fieldOfView:F1},{shot.aspectRatio},{shot.cameraPosition.x:F2},{shot.cameraPosition.y:F2},{shot.cameraPosition.z:F2},\"{actorNames}\",{shot.framePath}");
            }
        }

        public List<ShotData> GetAllShots() => _shots;
        
        public void ClearShots()
        {
            _shots.Clear();
            _shotCounter = 1;
            OnShotListUpdated?.Invoke(_shots);
        }

        public void DeleteShot(string shotId)
        {
            var shot = _shots.Find(s => s.shotId == shotId);
            if (shot != null)
            {
                if (File.Exists(shot.framePath))
                {
                    File.Delete(shot.framePath);
                }
                _shots.Remove(shot);
                OnShotListUpdated?.Invoke(_shots);
            }
        }
    }

    [Serializable]
    public class ShotData
    {
        public string shotId;
        public int shotNumber;
        public string description;
        public string timestamp;
        public Vector3 cameraPosition;
        public Vector3 cameraRotation;
        public float focalLength;
        public float fieldOfView;
        public string aspectRatio;
        public List<ActorData> actors;
        public string framePath;
    }

    [Serializable]
    public class ShotListExport
    {
        public string projectName;
        public string exportDate;
        public int totalShots;
        public List<ShotData> shots;
    }
}
