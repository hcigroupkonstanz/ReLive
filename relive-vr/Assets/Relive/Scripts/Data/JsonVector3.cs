using UnityEngine;

namespace Relive.Data
{
    public class JsonVector3
    {
        public float x;
        public float y;
        public float z;


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
