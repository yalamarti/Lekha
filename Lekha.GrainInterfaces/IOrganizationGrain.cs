namespace Lekha.GrainInterfaces
{
    public interface IOrganizationGrain : IGrainWithStringKey
    {
        ValueTask<BeginOrganizationExecutionResponse> BeginTaskGroupExecution(BeginOrganizationExecutionRequest beginTaskGroupExecutionRequest);
    }
}