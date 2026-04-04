using System.Collections;
using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Owns the trigger collider, player detection, and optional debug visual.
    internal class CheckpointArea : MonoBehaviour
    {
        public float Radius;
        public Vector3 Offset;

        private CheckpointName _name;
        private Coroutine _distanceLogCoroutine;

        internal void Init(Checkpoint checkpoint)
        {
            _name = checkpoint.Name;
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
            if (other.CompareTag("Player")) {
                Plugin.Log.LogDebug($"Player entered checkpoint: {_name}");
                RaceManager.Instance?.OnPlayerEnteredCheckpoint(_name);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) {
                Plugin.Log.LogDebug($"Player exited checkpoint: {_name}");
            }
        }

        private void SpawnDebugSphere()
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0f, 0f, 0.3f);

            // Outer shell: standard winding, visible from outside.
            SpawnShell(mat, invertNormals: false);
            // Inner shell: inverted normals + winding, visible from inside.
            SpawnShell(mat, invertNormals: true);
        }

        private void SpawnShell(Material mat, bool invertNormals)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere.GetComponent<SphereCollider>());
            sphere.transform.SetParent(transform, worldPositionStays: false);
            sphere.transform.localScale = Vector3.one * Radius * 2f;
            sphere.GetComponent<MeshRenderer>().material = mat;

            if (!invertNormals) return;

            // filter.mesh creates a per-instance copy so we don't modify the shared primitive mesh.
            var mesh = sphere.GetComponent<MeshFilter>().mesh;

            var normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                var triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int tmp = triangles[i];
                    triangles[i] = triangles[i + 1];
                    triangles[i + 1] = tmp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }
    }
}
