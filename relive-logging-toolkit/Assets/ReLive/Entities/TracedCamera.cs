using UniRx;
using UnityEngine;

namespace ReLive.Entities
{
    [RequireComponent(typeof(Camera))]
    public class TracedCamera : TracedEntity
    {
        protected override void InitializeEntity()
        {
            var camera = GetComponent<Camera>();

            entity.SetData("fov", camera.fieldOfView);
            camera
                .ObserveEveryValueChanged(c => c.fieldOfView)
                .TakeUntilDisable(this)
                .Skip(1) // ignore first - captured directly in entity
                .Subscribe(v => entity.AddTrace("fov", v));


            entity.SetData("aspect", camera.aspect);
            camera
                .ObserveEveryValueChanged(c => c.aspect)
                .TakeUntilDisable(this)
                .Skip(1) // ignore first - captured directly in entity
                .Subscribe(v => entity.AddTrace("aspect", v));

            entity.EntityType = EntityType.Camera;
            entity.Space = EntitySpace.World;
            entity.ScheduleChanges();
        }
    }
}
