using UnityEngine;

namespace DirectorsEye.VR
{
    public class ActorPlaceholder : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private MeshRenderer bodyRenderer;
        [SerializeField] private Color actorColor = Color.gray;
        [SerializeField] private string actorLabel = "Actor";

        [Header("Pose")]
        [SerializeField] private ActorPose currentPose = ActorPose.Standing;
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform bodyTransform;

        [Header("Marking")]
        [SerializeField] private GameObject floorMark;
        [SerializeField] private TextMesh labelText;
        [SerializeField] private int actorNumber = 1;

        private GrabbableObject _grabbable;

        public enum ActorPose
        {
            Standing,
            Sitting,
            Crouching,
            Lying,
            Walking,
            Running
        }

        void Awake()
        {
            _grabbable = GetComponent<GrabbableObject>();
            if (_grabbable == null)
            {
                _grabbable = gameObject.AddComponent<GrabbableObject>();
            }

            UpdateAppearance();
        }

        public void SetActorNumber(int number)
        {
            actorNumber = number;
            actorLabel = $"Actor {number}";
            UpdateAppearance();
        }

        public void SetColor(Color color)
        {
            actorColor = color;
            UpdateAppearance();
        }

        public void SetPose(ActorPose pose)
        {
            currentPose = pose;
            ApplyPose();
        }

        public void CyclePose()
        {
            currentPose = (ActorPose)(((int)currentPose + 1) % System.Enum.GetValues(typeof(ActorPose)).Length);
            ApplyPose();
        }

        private void UpdateAppearance()
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = actorColor;
            }

            if (labelText != null)
            {
                labelText.text = actorLabel;
            }

            if (floorMark != null)
            {
                var markRenderer = floorMark.GetComponent<Renderer>();
                if (markRenderer != null)
                {
                    markRenderer.material.color = actorColor;
                }
            }
        }

        private void ApplyPose()
        {
            if (bodyTransform == null) return;

            switch (currentPose)
            {
                case ActorPose.Standing:
                    bodyTransform.localPosition = Vector3.zero;
                    bodyTransform.localRotation = Quaternion.identity;
                    bodyTransform.localScale = Vector3.one;
                    break;

                case ActorPose.Sitting:
                    bodyTransform.localPosition = new Vector3(0, -0.4f, 0);
                    bodyTransform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;

                case ActorPose.Crouching:
                    bodyTransform.localPosition = new Vector3(0, -0.3f, 0);
                    bodyTransform.localScale = new Vector3(1, 0.7f, 1);
                    break;

                case ActorPose.Lying:
                    bodyTransform.localPosition = new Vector3(0, -0.7f, 0);
                    bodyTransform.localRotation = Quaternion.Euler(90, 0, 0);
                    break;

                case ActorPose.Walking:
                case ActorPose.Running:
                    bodyTransform.localPosition = Vector3.zero;
                    bodyTransform.localRotation = Quaternion.identity;
                    break;
            }

            Debug.Log($"[Actor] {actorLabel} pose: {currentPose}");
        }

        public ActorData GetActorData()
        {
            return new ActorData
            {
                label = actorLabel,
                number = actorNumber,
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                pose = currentPose.ToString(),
                color = ColorUtility.ToHtmlStringRGB(actorColor)
            };
        }

        public void LookAt(Vector3 target)
        {
            var lookDir = target - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        public void LookAtCamera(VirtualCameraRig camera)
        {
            if (camera != null)
            {
                LookAt(camera.transform.position);
            }
        }
    }

    [System.Serializable]
    public class ActorData
    {
        public string label;
        public int number;
        public Vector3 position;
        public Vector3 rotation;
        public string pose;
        public string color;
    }
}
