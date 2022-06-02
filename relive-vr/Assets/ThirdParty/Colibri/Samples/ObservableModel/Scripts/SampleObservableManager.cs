using HCIKonstanz.Colibri.Sync;

namespace HCIKonstanz.Colibri.Samples
{
    public class SampleObservableManager : ObservableManager<SampleObservableModel>
    {
        // Channel *must* start with GROUP<number>, may only contain [A-Z][a-z][0-9][-_.] (i.e. must be valid URL)
        protected override string Channel => "GROUP123-mysyncobject";
    }
}
