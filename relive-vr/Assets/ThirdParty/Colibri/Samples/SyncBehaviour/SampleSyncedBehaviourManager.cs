using HCIKonstanz.Colibri.Sync;

namespace HCIKonstanz.Colibri.Samples
{
    public class SampleSyncedBehaviourManager : SyncedBehaviourManager<SampleSyncedBehaviour>
    {
        public override string Channel => "SampleSyncedBehaviour";
    }
}
