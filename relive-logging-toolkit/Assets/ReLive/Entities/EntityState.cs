using ReLive.Core;
using System;
using UnityEngine;

namespace ReLive.Entities
{
    public enum StateStatus { Active, Inactive, Deleted }

    public class EntityState : State
    {
        public StateStatus Status { set => SetData("status", Enum.GetName(typeof(StateStatus), value).ToLower()); }
        public Vector3 Position { set => SetData("position", value); }
        public Vector3 Rotation { set => SetData("position", value); }
        public Vector3 Scale { set => SetData("position", value); }
        public Color Color { set => SetData("color", ColorUtility.ToHtmlStringRGB(value)); }

        public EntityState()
        {
            StateType = StateType.Entity;
        }
    }
}
