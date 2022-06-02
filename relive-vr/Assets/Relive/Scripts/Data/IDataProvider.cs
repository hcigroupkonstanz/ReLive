using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relive.Data
{
    public interface IDataProvider
    {
        Task<List<Session>> GetSessions();
        Task<LoadedSession> LoadSession(Session session);
        int GetProgress();
    }
}
