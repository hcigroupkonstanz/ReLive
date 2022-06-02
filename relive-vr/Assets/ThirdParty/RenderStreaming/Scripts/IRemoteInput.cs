using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.RenderStreaming
{
    public interface IRemoteInput
    {
        void SetInput(IInput input);
    }
}
