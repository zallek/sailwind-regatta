using System.Collections;
using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Position and size are driven by the standard Transform (localPosition = offset, localScale = radius * 2).
    // The SphereCollider (radius 0.5) and debug shells inherit scale automatically.
    internal class CheckpointArea : MonoBehaviour
    {
        private Checkpoint _checkpoint;

        internal void Init(Checkpoint checkpoint)
        {
            _checkpoint = checkpoint;
            transform.localPosition = checkpoint.Offset;
            transform.localScale = Vector3.one * checkpoint.Radius * 2f;

            // radius = 0.5 on a unit sphere; world radius = 0.5 * localScale = checkpoint.Radius.
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;

            if (Plugin.ShowCheckpointZones.Value)
                SpawnDebugSpheres();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                RaceManager.Instance?.OnPlayerEnteredCheckpoint(_checkpoint);
            }
        }

        private void SpawnDebugSpheres()
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(1f, 0f, 0f, 0.3f);

            SpawnShell(mat, invertNormals: false);
            SpawnShell(mat, invertNormals: true);
        }

        private void SpawnShell(Material mat, bool invertNormals)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere.GetComponent<SphereCollider>());
            sphere.transform.SetParent(transform, worldPositionStays: false);
            sphere.GetComponent<MeshRenderer>().material = mat;

            if (!invertNormals) return;

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
