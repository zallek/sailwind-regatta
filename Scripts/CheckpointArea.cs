using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Owns the trigger collider, player detection, and optional debug visual.
    internal class CheckpointArea : MonoBehaviour
    {
        public float Radius;
        public Vector3 Offset;

        private Port _port;

        internal void Init(RaceCheckpoint checkpoint, Port port)
        {
            _port = port;
            Radius = checkpoint.Radius;
            Offset = checkpoint.Offset;
            transform.localPosition = Offset;

            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = Radius;

            if (Plugin.ShowCheckpointZones.Value)
                SpawnDebugSphere();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                RaceManager.Instance?.OnPlayerEnteredPort(_port);
        }

        private void SpawnDebugSphere()
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere.GetComponent<SphereCollider>());
            sphere.transform.SetParent(transform, worldPositionStays: false);
            sphere.transform.localScale = Vector3.one * Radius * 2f;

            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0f, 0f, 0.3f);
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            sphere.GetComponent<MeshRenderer>().material = mat;
        }
    }
}
