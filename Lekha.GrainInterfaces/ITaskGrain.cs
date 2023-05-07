namespace Lekha.GrainInterfaces
{
    public interface ITaskGrain : IGrainWithStringKey
    {
        ValueTask<string> SayHello(string greeting);
    }
}