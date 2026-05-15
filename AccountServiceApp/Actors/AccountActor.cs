using Dapr.Actors;
using Dapr.Actors.Runtime;
using System.Numerics;
using System.Threading.Tasks;
using AccountActorInterfaces;
public class AccountActor : Actor, IAccountActor
{
    // Note 1: For simplicity, we are using the actor ID as the key
    //          in the state store to hold the balance.
    // Note 2: In a real application, you would want to handle errors and
    //          ensure that both operations succeed or fail together

    public AccountActor(ActorHost host) : base(host) { }


    public async Task Withdraw(int amount)
    {
        Console.WriteLine($"AccountService: For Actor Id: {this.Id}");
        Console.WriteLine($"AccountService: Withdraw() is called with amount: {amount}");
        var key = this.Id.GetId(); // use actor ID as Redis key
        var balance = await StateManager.TryGetStateAsync<int>(key);
        var current = balance.HasValue ? balance.Value : 0;
        var newBalance = current - amount;
        await StateManager.SetStateAsync(key, newBalance);
        Console.WriteLine($"AccountService: Updated Balance of {this.Id} is : {newBalance}");
    }

    public async Task Deposit(int amount)
    {
        Console.WriteLine($"AccountService: For Actor Id: {this.Id}");
        Console.WriteLine($"AccountService: Deposit() called with amount: {amount}");
        var key = this.Id.GetId();
        var balance = await StateManager.TryGetStateAsync<int>(key);
        var current = balance.HasValue ? balance.Value : 0;
        var newBalance = current + amount;
        await StateManager.SetStateAsync(key, newBalance);
        Console.WriteLine($"AccountService: Updated Balance of {this.Id} is : {newBalance}");
    }

    public async Task<int> GetBalance()
    {
        var key = this.Id.GetId();
        var balance = await StateManager.TryGetStateAsync<int>(key);
        return balance.HasValue ? balance.Value : 0;
    }
}
