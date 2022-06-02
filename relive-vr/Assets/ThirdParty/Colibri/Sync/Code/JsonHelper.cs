using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace HCIKonstanz.Colibri.Sync
{
    public static class JsonExtensions
    {
        public static JArray ToJson(this Vector2 v) => new JArray { v.x, v.y };
        public static JArray ToJson(this Vector3 v) => new JArray { v.x, v.y, v.z };
        public static JArray ToJson(this Quaternion v) => new JArray { v.x, v.y, v.z, v.w };
        public static string ToJson(this Color c) => "#" + ColorUtility.ToHtmlStringRGBA(c);

        public static Vector2 ToVector2(this JToken val)
        {
            var vals = val.Select(x => (float)x).ToArray();
            return new Vector2(vals[0], vals[1]);
        }

        public static Vector3 ToVector3(this JToken val)
        {
            var vals = val.Select(x => (float)x).ToArray();
            return new Vector3(vals[0], vals[1], vals[2]);
        }

        public static Quaternion ToQuaternion(this JToken val)
        {
            var vals = val.Select(x => (float)x).ToArray();
            return new Quaternion(vals[0], vals[1], vals[2], vals[3]);
        }

        public static Color ToColor(this JToken val)
        {
            var c = new Color();
            ColorUtility.TryParseHtmlString(val.Value<string>(), out c);
            return c;
        }


        public static JToken ToJson(this object obj)
        {
            if (obj is bool)
                return new JValue((bool)obj);
            if (obj is int)
                return new JValue((int)obj);
            if (obj is float)
                return new JValue((float)obj);
            if (obj is string)
                return new JValue((string)obj);
            if (obj is Vector2 v2)
                return v2.ToJson();
            if (obj is Vector3 v3)
                return v3.ToJson();
            if (obj is Quaternion q)
                return q.ToJson();
            if (obj is Color c)
                return c.ToJson();

            if (obj is bool[])
                return new JArray((bool[])obj);
            if (obj is int[])
                return new JArray((int[])obj);
            if (obj is float[])
                return new JArray((float[])obj);
            if (obj is string[])
                return new JArray((string[])obj);
            if (obj is Vector2[] v2a)
                return new JArray(v2a.Select(x => x.ToJson()));
            if (obj is Vector3[] v3a)
                return new JArray(v3a.Select(x => x.ToJson()));
            if (obj is Quaternion[] qa)
                return new JArray(qa.Select(x => x.ToJson()));
            if (obj is Color[] ca)
                return new JArray(ca.Select(x => x.ToJson()));

            return new JValue("UNKNOWN TYPE");
        }
    }
}
