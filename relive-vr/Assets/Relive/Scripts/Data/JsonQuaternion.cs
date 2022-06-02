using UnityEngine;

namespace Relive.Data
{
    public class JsonQuaternion
    {
        public float x;
        public float y;
        public float z;

        public float? w;


        public Quaternion GetQuaternion()
        {
            if (w.HasValue)
            {
                return new Quaternion(x, y, z, w.Value);
            }
            else
            {
                return Quaternion.Euler(GetVector3());
            }
        }

        public Vector3 GetVector3()
        {
            return new Vector3(x, y, z);
        }

        public override string ToString()
        {
            return x + ", " + y + "," + z;
        }
    }
}
