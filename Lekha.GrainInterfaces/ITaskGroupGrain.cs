namespace Lekha.GrainInterfaces
{
    public interface ITaskGroupGrain : IGrainWithStringKey
    {
        ValueTask<BeginTaskGroupExecutionResponse> BeginExecution(BeginTaskGroupExecutionRequest beginTaskGroupExecutionRequest);
    }
}