using System;
using System.Threading.Tasks;

namespace IAutoM8.WebSockets.Stores.Interfaces
{
    public interface ILoginSocketStore
    {
        Task LogOff(Guid id);
    }
}
