using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SailwindRegatta
{
    internal class ShipModelItem : PickupableItem
    {
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            holdDistance = 0.9f;
        }

        public override void OnPickup()
        {
            _rb.isKinematic = true;
        }

        public override void OnDrop()
        {
            _rb.isKinematic = false;
        }

        internal void BuildVisual(BoatRefs boatRefs)
        {
            var root = new GameObject("ShipModelVisual");
            root.transform.SetParent(transform, worldPositionStays: false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * GetScaleForBoat(boatRefs);
            CopyVisualHierarchy(boatRefs.boatModel, root.transform);

            // StartCoroutine(DisableChildrenOneByOne(root.transform));
        }

        private IEnumerator DisableChildrenOneByOne(Transform parent)
        {
            // Iterate through all children of this transform
            // Note: We use a loop because transform is an IEnumerable
            int childCount = parent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);

                NotificationUi.instance.ShowNotification($"Disabling child at index: {i} (Name: {child.name})", 2f);

                child.gameObject.SetActive(false);

                yield return new WaitForSeconds(2);
            }

            Debug.Log("All children have been disabled.");
        }

        private static void CopyVisualHierarchy(Transform source, Transform destParent)
        {
            foreach (Transform child in source)
            {
                if (!child.gameObject.activeSelf)
                    continue;

                var blacklistedChildren = new List<string> { "walk_col", "walk_col (player)", "hull damage tex" };
                if (blacklistedChildren.Contains(child.name))
                    continue;

                var go = new GameObject(child.name);
                go.transform.SetParent(destParent, worldPositionStays: false);
                go.transform.localPosition = child.localPosition;
                go.transform.localRotation = child.localRotation;
                go.transform.localScale = child.localScale;

                CopyMeshRenderers(child.gameObject, go);
                CopyVisualHierarchy(child, go.transform);
            }
        }

        // Usefull gameobject
        // structure_container (71)

        private static void CopyMeshRenderers(GameObject source, GameObject dest)
        {
            var mr = source.GetComponent<MeshRenderer>();
            var mf = source.GetComponent<MeshFilter>();
            if (mr != null && mf != null && mf.sharedMesh != null)
            {
                dest.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
                dest.AddComponent<MeshRenderer>().sharedMaterials = mr.sharedMaterials;
                return;
            }

            var smr = source.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
            {
                var baked = new Mesh();
                smr.BakeMesh(baked);
                dest.AddComponent<MeshFilter>().sharedMesh = baked;
                dest.AddComponent<MeshRenderer>().sharedMaterials = smr.sharedMaterials;
            }
        }

        private static float GetScaleForBoat(BoatRefs boatRefs)
        {
            var saveObj = boatRefs.GetComponentInParent<SaveableObject>();
            if (saveObj == null)
                return 0.02f;
            switch (saveObj.sceneIndex)
            {
                case 10:
                    return 0.018f; // Dhow
                case 20:
                    return 0.016f; // Sanbuq
                case 40:
                    return 0.013f; // Coq
                case 50:
                    return 0.012f; // Brig
                case 80:
                    return 0.014f; // Junk
                case 90:
                    return 0.017f; // Kakam
                case 153:
                    return 0.010f; // Clipper
                case 160:
                    return 0.016f; // Sloop
                default:
                    return 0.015f;
            }
        }
    }
}
