using HCIKonstanz.Colibri.Networking;
using HCIKonstanz.Colibri.Sync;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

public class SampleObservableModel : ObservableModel<SampleObservableModel>
{
    private string _testString = "default value";
    public string TestString
    {
        get { return _testString; }
        set
        {
            if (_testString != value)
            {
                _testString = value;
                TriggerLocalChange("TestString");
            }
        }
    }


    private Vector3 _position;
    public Vector3 Position
    {
        get { return _position; }
        set
        {
            if (_position != value)
            {
                _position = value;
                TriggerLocalChange("Position");
            }
        }
    }


    private Quaternion _rotation;
    public Quaternion Rotation
    {
        get { return _rotation; }
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                TriggerLocalChange("Rotation");
            }
        }
    }




    public override JObject ToJson()
    {
        return new JObject
        {
            { "Id", Id },
            { "TestString", _testString },
            { "Position", new JArray { _position.x, _position.y, _position.z } },
            { "Rotation", new JArray { _rotation.x, _rotation.y, _rotation.z, _rotation.w } }
        };
    }

    protected override void ApplyRemoteUpdate(JObject updates)
    {
        var testString = updates["TestString"];
        if (testString != null)
            _testString = testString.Value<string>();

        var position = updates["Position"];
        if (position != null)
        {
            var vals = position.Select(i => (float)i).ToArray();
            _position = new Vector3(vals[0], vals[1], vals[2]);
        }

        var rotation = updates["Rotation"];
        if (rotation != null)
        {
            var vals = rotation.Select(i => (float)i).ToArray();
            _rotation = new Quaternion(vals[0], vals[1], vals[2], vals[3]);
        }
    }
}
