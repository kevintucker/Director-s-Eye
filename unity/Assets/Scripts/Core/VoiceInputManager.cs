using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace DirectorsEye.Core
{
    [Serializable]
    public class TranscriptionCompleteEvent : UnityEvent<string> { }

    public class VoiceInputManager : MonoBehaviour
    {
        public static VoiceInputManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string whisperEndpoint = "http://localhost:8000/api/transcribe";
        [SerializeField] private int sampleRate = 16000;
        [SerializeField] private int maxRecordingSeconds = 30;
        [SerializeField] private float silenceThreshold = 0.01f;
        [SerializeField] private float silenceDuration = 1.5f;

        [Header("Events")]
        public UnityEvent OnRecordingStarted;
        public UnityEvent OnRecordingStopped;
        public TranscriptionCompleteEvent OnTranscriptionComplete;
        public UnityEvent<string> OnTranscriptionFailed;

        private AudioClip _recordingClip;
        private bool _isRecording;
        private float _silenceTimer;
        private string _microphoneDevice;

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
            }
        }

        void Start()
        {
            if (Microphone.devices.Length > 0)
            {
                _microphoneDevice = Microphone.devices[0];
                Debug.Log($"[VoiceInput] Using microphone: {_microphoneDevice}");
            }
            else
            {
                Debug.LogError("[VoiceInput] No microphone found!");
            }
        }

        void Update()
        {
            if (_isRecording)
            {
                CheckForSilence();
            }
        }

        public void StartRecording()
        {
            if (_isRecording || string.IsNullOrEmpty(_microphoneDevice))
                return;

            _recordingClip = Microphone.Start(_microphoneDevice, false, maxRecordingSeconds, sampleRate);
            _isRecording = true;
            _silenceTimer = 0f;
            
            OnRecordingStarted?.Invoke();
            Debug.Log("[VoiceInput] Recording started");
        }

        public void StopRecording()
        {
            if (!_isRecording)
                return;

            var position = Microphone.GetPosition(_microphoneDevice);
            Microphone.End(_microphoneDevice);
            _isRecording = false;

            OnRecordingStopped?.Invoke();
            Debug.Log("[VoiceInput] Recording stopped");

            if (position > 0)
            {
                StartCoroutine(ProcessRecording(position));
            }
        }

        public void ToggleRecording()
        {
            if (_isRecording)
                StopRecording();
            else
                StartRecording();
        }

        private void CheckForSilence()
        {
            var position = Microphone.GetPosition(_microphoneDevice);
            if (position < sampleRate / 10) return;

            var samples = new float[sampleRate / 10];
            _recordingClip.GetData(samples, position - samples.Length);

            float maxAmplitude = 0f;
            foreach (var sample in samples)
            {
                var abs = Mathf.Abs(sample);
                if (abs > maxAmplitude) maxAmplitude = abs;
            }

            if (maxAmplitude < silenceThreshold)
            {
                _silenceTimer += Time.deltaTime;
                if (_silenceTimer >= silenceDuration)
                {
                    StopRecording();
                }
            }
            else
            {
                _silenceTimer = 0f;
            }
        }

        private IEnumerator ProcessRecording(int sampleCount)
        {
            Debug.Log("[VoiceInput] Processing recording...");

            var samples = new float[sampleCount];
            _recordingClip.GetData(samples, 0);

            var wavData = ConvertToWav(samples, sampleRate);
            
            yield return StartCoroutine(SendToWhisper(wavData));
        }

        private byte[] ConvertToWav(float[] samples, int sampleRate)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            var byteRate = sampleRate * 2;
            var blockAlign = 2;
            var subChunk2Size = samples.Length * 2;
            var chunkSize = 36 + subChunk2Size;

            // RIFF header
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(chunkSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)16);

            // data subchunk
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(subChunk2Size);

            foreach (var sample in samples)
            {
                var intSample = (short)(sample * 32767f);
                writer.Write(intSample);
            }

            return stream.ToArray();
        }

        private IEnumerator SendToWhisper(byte[] wavData)
        {
            var form = new WWWForm();
            form.AddBinaryData("audio", wavData, "recording.wav", "audio/wav");

            using var request = UnityWebRequest.Post(whisperEndpoint, form);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[VoiceInput] Transcription failed: {request.error}");
                OnTranscriptionFailed?.Invoke(request.error);
                yield break;
            }

            var response = JsonUtility.FromJson<TranscriptionResponse>(request.downloadHandler.text);
            
            if (!string.IsNullOrEmpty(response.text))
            {
                Debug.Log($"[VoiceInput] Transcription: {response.text}");
                OnTranscriptionComplete?.Invoke(response.text);
            }
            else
            {
                OnTranscriptionFailed?.Invoke("Empty transcription");
            }
        }

        [Serializable]
        private class TranscriptionResponse
        {
            public string text;
            public float confidence;
        }
    }
}
