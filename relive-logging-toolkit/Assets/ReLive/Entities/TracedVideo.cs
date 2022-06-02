using ReLive.Entities;

public class TracedVideo : TracedEntity
{
    public string VideoAddress;
    public float FieldOfView;
    public int ResolutionWidth;
    public int ResolutionHeight;

    protected override void InitializeEntity()
    {
        entity.Space = EntitySpace.World;
        entity.EntityType = EntityType.Video;

        entity.SetData("fov", FieldOfView);
        entity.SetData("resolutionWidth", ResolutionWidth);
        entity.SetData("resolutionHeight", ResolutionHeight);

        entity.AttachContent("rtsp", "rtsp", VideoAddress);
    }
}
