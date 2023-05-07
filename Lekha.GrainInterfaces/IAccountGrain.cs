namespace Lekha.GrainInterfaces
{
    public interface IAccountGrain : IGrainWithStringKey
    {
        ValueTask<BeginAccountExecutionResponse> BeginTaskGroupExecution(BeginAccountExecutionRequest beginTaskGroupExecutionRequest);
    }
}