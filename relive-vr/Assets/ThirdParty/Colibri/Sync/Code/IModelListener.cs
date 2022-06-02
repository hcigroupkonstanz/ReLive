using Newtonsoft.Json.Linq;

namespace HCIKonstanz.Colibri.Sync
{
    public interface IModelListener
    {
        string GetChannelName();
        void OnSyncInitialized(JArray initialObjects);
        void OnSyncUpdate(JObject updatedObject);
        void OnSyncDelete(JObject deletedObject);
    }
}
