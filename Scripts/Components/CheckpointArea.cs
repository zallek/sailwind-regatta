using UnityEngine;

namespace SailwindRegatta
{
    // Injected at runtime as a child of a Port GameObject.
    // Position and size are driven by the standard Transform (localPosition = offset, localScale = radius * 2).
    // The SphereCollider (radius 0.5) inherits scale automatically.
    internal class CheckpointArea : MonoBehaviour
    {
        private Checkpoint _checkpoint;

        internal void Init(Checkpoint checkpoint)
        {
            _checkpoint = checkpoint;
            transform.localPosition = checkpoint.Offset;
            transform.localScale = new Vector3(checkpoint.Radius * 2f, 50f, checkpoint.Radius * 2f);

            // SphereCollider radius = 0.5 on this scaled object → world radius = 0.5 * localScale = checkpoint.Radius.
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;

#if DEBUG
            SpawnDebugDisk();
#endif
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                RaceManager.Instance?.OnPlayerEnteredCheckpoint(_checkpoint);
            }
        }

#if DEBUG
        private void SpawnDebugDisk()
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Plugin.Log.LogWarning("CheckpointArea: Shader 'Sprites/Default' not found; skipping debug disk.");
                return;
            }
            var mat = new Material(shader);
            mat.color = new Color(1f, 0f, 0f, 0.3f);

            // Map world sea level (Y=0) into this object's local space so the disk sits flat on the water.
            var seaLevelLocal = transform.InverseTransformPoint(new Vector3(transform.position.x, 0f, transform.position.z));

            SpawnDiskShell(mat, seaLevelLocal, invertNormals: false);
            SpawnDiskShell(mat, seaLevelLocal, invertNormals: true);
        }

        private void SpawnDiskShell(Material mat, Vector3 localPosition, bool invertNormals)
        {
            var disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(disk.GetComponent<CapsuleCollider>());
            disk.transform.SetParent(transform, worldPositionStays: false);
            disk.transform.localPosition = localPosition;
            // Parent scale = Radius*2 (uniform). Cylinder primitive has radius 0.5 at localScale 1,
            // so localScale.x/z = 1 → world radius = Radius*2 * 1 * 0.5 = Radius. ✓
            // localScale.y very small to flatten the cylinder into a disk.
            disk.transform.localScale = new Vector3(1f, 0.01f, 1f);
            disk.GetComponent<MeshRenderer>().material = mat;

            if (!invertNormals)
                return;

            var mesh = disk.GetComponent<MeshFilter>().mesh;

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
#endif
    }
}
