using Azure;
using Azure.Data.Tables;
using Lekha.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lekha.Infrastructure
{
    public interface IBlobClientService<T>
    {
        Task Upload(string containerName, string blobName, Stream stream);
    }

    public class TableDataModel
    {
        // Captures all of the TaskGroup properties -- Status, etc
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public DateTimeOffset? Timestamp { get; set; }
        public string Etag { get; set; }

        public object this[string name]
        {
            get => (ContainsProperty(name)) ? _properties[name] : null;
            set => _properties[name] = value;
        }

        public ICollection<string> PropertyNames => _properties.Keys;
        public int PropertyCount => _properties.Count;
        public bool ContainsProperty(string name) => _properties.ContainsKey(name);
    }

    public class TaskGroupExecutionInputModel
    {
        public string AccountId { get; set; }
        public string TaskGroupId { get; set; }
        public int? Status { get; set; }
    }

    public class TaskGroupScheduleRunDataModel : TableDataModel
    {
        // Captures all of the TaskGroup properties -- Status, etc
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        public string AccountId { get; set; }
        public string TaskGroupId { get; set; }
    }

    public class TaskGroupExecutionDataModel : TableDataModel
    {
        public string AccountId { get; set; }
        public string TaskGroupId { get; set; }
        public string TaskGroupScheduleRunId { get; set; }
    }

    public class TaskGroupFilterResultsInputModel : IValidatableObject
    {
        public string PartitionKey { get; set; }
        public string RowKeyDateStart { get; set; }
        public string RowKeyTimeStart { get; set; }
        public string RowKeyDateEnd { get; set; }
        public string RowKeyTimeEnd { get; set; }
        public int? Status { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }

    public interface ITaskGroupExecutionStatusTableClientService
    {
        IEnumerable<TaskGroupExecutionDataModel> GetAllRows();
        IEnumerable<TaskGroupExecutionDataModel> GetFilteredRows(TaskGroupFilterResultsInputModel inputModel);
        Task<Response> InsertTableEntity(TaskGroupExecutionInputModel model);
        Task<Response> UpsertTableEntity(TaskGroupExecutionInputModel model);
        Task<Response> RemoveEntity(TaskGroupExecutionInputModel model);
    }

    /// <summary>
    /// Reference: https://docs.microsoft.com/en-us/azure/cosmos-db/table/create-table-dotnet?tabs=azure-portal%2Cvisual-studio
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskGroupExecutionTableClientService : ITaskGroupExecutionStatusTableClientService
    {
        public string[] EXCLUDE_TABLE_ENTITY_KEYS = { "PartitionKey", "RowKey", "odata.etag", "Timestamp" };

        private readonly ApplicationContext appContext;
        private readonly IConfiguration configuration;
        private readonly ILogger<TaskGroupExecutionTableClientService> logger;
        private TableClient tableClient;
        public TaskGroupExecutionTableClientService(ApplicationContext appContext,
                                                    TableClient tableClient,
                                                    IConfiguration configuration,
                                                    ILogger<TaskGroupExecutionTableClientService> logger)
        {
            this.appContext = appContext;
            this.tableClient = tableClient;
            this.configuration = configuration;
            this.logger = logger;
        }

        private TaskGroupExecutionDataModel MapTableEntity(TableEntity entity)
        {
            TaskGroupExecutionDataModel dataModel = new TaskGroupExecutionDataModel();

            dataModel.Timestamp = entity.Timestamp;
            dataModel.Etag = entity.ETag.ToString();

            var measurements = entity.Keys.Where(key => !EXCLUDE_TABLE_ENTITY_KEYS.Contains(key));
            foreach (var key in measurements)
            {
                dataModel[key] = entity[key];
            }
            return dataModel;
        }

        public IEnumerable<TaskGroupExecutionDataModel> GetAllRows() 
        {
            Pageable<TableEntity> entities = tableClient.Query<TableEntity>();

            return entities.Select(e => MapTableEntity(e));
        }

        public IEnumerable<TaskGroupExecutionDataModel> GetFilteredRows(TaskGroupFilterResultsInputModel inputModel)
        {
            List<string> filters = new List<string>();

            if (!String.IsNullOrEmpty(inputModel.PartitionKey))
                filters.Add($"PartitionKey eq '{inputModel.PartitionKey}'");
            if (!String.IsNullOrEmpty(inputModel.RowKeyDateStart) && !String.IsNullOrEmpty(inputModel.RowKeyTimeStart))
                filters.Add($"RowKey ge '{inputModel.RowKeyDateStart} {inputModel.RowKeyTimeStart}'");
            if (!String.IsNullOrEmpty(inputModel.RowKeyDateEnd) && !String.IsNullOrEmpty(inputModel.RowKeyTimeEnd))
                filters.Add($"RowKey le '{inputModel.RowKeyDateEnd} {inputModel.RowKeyTimeEnd}'");
            if (inputModel.Status.HasValue)
                filters.Add($"Status eq {inputModel.Status.Value}");

            string filter = String.Join(" and ", filters);

            Pageable<TableEntity> entities = tableClient.Query<TableEntity>(filter);

            return entities.Select(e => MapTableEntity(e));
        }
        public Task<Response> InsertTableEntity(TaskGroupExecutionInputModel model)
        {
            TableEntity entity = new TableEntity();

            entity.PartitionKey = model.AccountId;
            entity.RowKey = model.TaskGroupId;

            // The other values are added like a items to a dictionary
            entity["Status"] = model.Status;

            return tableClient.AddEntityAsync(entity);
        }

        public Task<Response> UpsertTableEntity(TaskGroupExecutionInputModel model)
        {
            TableEntity entity = new TableEntity();

            entity.PartitionKey = model.AccountId;
            entity.RowKey = model.TaskGroupId;

            // The other values are added like a items to a dictionary
            entity["Status"] = model.Status;

            return tableClient.UpsertEntityAsync(entity);
        }

        public Task<Response> RemoveEntity(TaskGroupExecutionInputModel model)
        {
            return tableClient.DeleteEntityAsync(model.AccountId, model.TaskGroupId);
        }
    }
}
