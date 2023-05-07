namespace Lekha.GrainInterfaces
{
    [GenerateSerializer]
    public class BeginOrganizationExecutionRequest
    {
        [Id(0)]
        public Organization Organization { get; set; }
    }

    [GenerateSerializer]
    public class BeginOrganizationExecutionResponse
    {
        [Id(0)]
        public string? Result { get; set; }
        [Id(1)]
        public string? Error { get; set; }
    }
    [GenerateSerializer]
    public class BeginAccountExecutionRequest
    {
        [Id(0)]
        public Organization Organization { get; set; }
        [Id(1)]
        public Account Account { get; set; }
    }

    [GenerateSerializer]
    public class BeginAccountExecutionResponse
    {
        [Id(0)]
        public string? Result { get; set; }
        [Id(1)]
        public string? Error { get; set; }
    }
    [GenerateSerializer]
    public class BeginTaskGroupExecutionRequest
    {
        [Id(0)]
        public Organization Organization { get; set; }
        [Id(1)]
        public Account Account { get; set; }
        [Id(2)]
        public TaskGroupDefinition TaskGroupDefinition { get; set; }
    }

    [GenerateSerializer]
    public class BeginTaskGroupExecutionResponse
    {
        [Id(0)]
        public string? Result { get; set; }
        [Id(1)]
        public string? Error { get; set; }
    }

    [GenerateSerializer]
    public class OrganizationState
    {
        [Id(0)]
        public int SomeValue { get; set; }
    }

    public class TestMessage
    {

        public string id { get; set; }

        public string name { get; set; }
    }
}