using System.Text;
using UnityEngine;

namespace ReLive.Entities
{
    public class TracedGameObject : TracedEntity
    {
        public bool ExportMesh = false;
        private int startIndex;

        protected override void InitializeEntity()
        {
            if (ExportMesh)
            {
                var mesh = MeshToObjString(entity.EntityId);
                entity.AttachContent("mesh.obj", "model", mesh);
            }

            entity.EntityType = EntityType.Object;
            entity.Space = EntitySpace.World;
            entity.ScheduleChanges();
        }

        // everything below is adapted from https://wiki.unity3d.com/index.php/ExportOBJ
        private string MeshToObjString(string objName)
        {
            StringBuilder meshString = new StringBuilder();

            meshString.Append("#" + objName + ".obj"
                                + "\n#" + System.DateTime.Now.ToLongDateString()
                                + "\n#" + System.DateTime.Now.ToLongTimeString()
                                + "\n#-------"
                                + "\n\n");

            Vector3 originalPosition = transform.position;
            transform.position = Vector3.zero;

            meshString.Append(ProcessTransform(transform));

            transform.position = originalPosition;

            return meshString.ToString();
        }

        private string ProcessTransform(Transform t)
        {
            StringBuilder meshString = new StringBuilder();

            meshString.Append("#" + t.name
                            + "\n#-------"
                            + "\n");

            meshString.Append("g ").Append(t.name).Append("\n");

            MeshFilter mf = t.GetComponent<MeshFilter>();
            if (mf)
                meshString.Append(MeshToString(mf, t));

            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);

                // avoid duplicates: stop if another (sub-)entity is detected
                if (child.GetComponent<TracedEntity>() == null)
                    meshString.Append(ProcessTransform(child));
            }

            return meshString.ToString();
        }

        private string MeshToString(MeshFilter mf, Transform t)
        {
            Vector3 s = t.localScale;
            Vector3 p = t.localPosition;
            Quaternion r = t.localRotation;


            int numVertices = 0;
            Mesh m = mf.sharedMesh;
            if (!m)
                return "####Error####";
            Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

            StringBuilder sb = new StringBuilder();

            foreach (Vector3 vv in m.vertices)
            {
                Vector3 v = t.TransformPoint(vv);
                numVertices++;
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");
            foreach (Vector3 nn in m.normals)
            {
                Vector3 v = r * nn;
                sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            for (int material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(mats[material].name).Append("\n");
                sb.Append("usemap ").Append(mats[material].name).Append("\n");

                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", triangles[i] + 1 + startIndex, triangles[i + 1] + 1 + startIndex, triangles[i + 2] + 1 + startIndex));
            }

            startIndex += numVertices;
            return sb.ToString();
        }

    }
}
