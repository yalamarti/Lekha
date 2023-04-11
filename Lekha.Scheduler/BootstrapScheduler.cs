using System.Threading.Tasks;

namespace Lekha.Scheduler
{


    public class TaskGroupScheduleMonitor
    {
        public TaskGroupScheduleMonitor()
        {

        }

        public Task Start(string accountId, string taskGroupId, string taskGroupName)
        {
            // Check if an instance is already running with an ID: organizationId/organizationName/projectId/projectName/taskGroupId/taskGroupName
            // Start Orchestration instance 
            return Task.FromResult(true);
        }

        public Task OrchestrateTaskGroupMonitoring(string accountId, string taskGroupId, string taskGroupName)
        {
            // Trace operation
            // Get TaskGroup Schedule

            // Is it time to shutdown - then exit

            // Is it time to execute?
            //   Yes
            //      Check if an instance is already running with an ID: organizationId/organizationName/projectId/projectName/taskGroupId/taskGroupName
            //      Already running?
            //        Yes
            //           Queue up next execution?
            //             Is the queue full?
            //               No
            //                    Queue it up
            //
            //           Do Nothing
            //             return
            //        No
            //          Start sub-Orchestration 
            //   No
            //      Is one queued up to be executed?
            //          Yes
            //             Start sub-Orchestration 
            //      Wait till the next interval

            return Task.FromResult(true);
        }
    }


    public class TaskGroupExecutor
    {
        public TaskGroupExecutor()
        {

        }

        public Task Start(string accountId, string accountName, string taskGroupId, string taskGroupName)
        {
            // Check if an instance is already running with an ID: organizationId/organizationName/projectId/projectName/taskGroupId/taskGroupName

            // Already running?
            //   Yes
            //      Do Nothing
            //        return
            //   No
            //     Start Orchestration 

            return Task.FromResult(true);
        }

        public Task OrchestrateTaskGroupExecution(string organizationId, string organizationName, string accountId, string projectId, string projectName, string taskGroupId, string taskGroupName)
        {
            // Trace operation
            // Get TaskGroup Schedule

            // Is it time to shutdown - then exit

            // Is it time to execute?
            //   Yes
            //      Start TaskGroupExecution sub-orchestration
            //   No
            //      Wait till the next interval

            return Task.FromResult(true);
        }
    }
}
