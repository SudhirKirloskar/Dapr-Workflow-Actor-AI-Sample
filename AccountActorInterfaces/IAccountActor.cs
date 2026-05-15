using Dapr.Actors;
using System.Threading.Tasks;

namespace AccountActorInterfaces
{
    public interface IAccountActor : IActor
    {
        Task Withdraw(int amount);
        Task Deposit(int amount);
        Task<int> GetBalance();
    }
}
