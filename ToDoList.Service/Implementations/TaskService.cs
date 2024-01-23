using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoList.DAL.Interfaces;
using ToDoList.Domain.Entity;
using ToDoList.Domain.Enum;
using ToDoList.Domain.Extensions;
using ToDoList.Domain.Filters.Task;
using ToDoList.Domain.Models;
using ToDoList.Domain.Response;
using ToDoList.Service.Interfaces;

namespace ToDoList.Service.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly IBaseRepository<TaskEntity> _taskRepository;
        private ILogger<TaskService> _logger;

        public TaskService(IBaseRepository<TaskEntity> taskRepository, ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _logger = logger;
        }   

        public async Task<IBaseResponse<TaskEntity>> Create(CreateTaskModel model)
        {
            try
            {
                model.Validate();

                _logger.LogInformation($"Запрос на создание задачи - {model.Name}");

                var task = await _taskRepository.GetAll()
                    .Where(x => x.Created.Date == DateTime.Today)
                    .FirstOrDefaultAsync(x => x.Name == model.Name);
                
                if (task != null)
                    return OutputProcessing<TaskEntity>("Задача с таким названием уже есть", StatusCode.TaskIsHasAlready);

                task = new TaskEntity()
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsDone = false,
                    Priority = model.Priority,
                    Created = DateTime.Now,
                };

                await _taskRepository.Create(task);

                _logger.LogInformation($"Задача создалась: {task.Name} {task.Created}");
                await _taskRepository.Update(task);
                return OutputProcessing<TaskEntity>("Задача создалась", StatusCode.Ok);
            }
            catch (Exception ex)
            {
                return HandleException<TaskEntity>(ex, "TaskService.Create");
            }
        }

        public async Task<IBaseResponse<bool>> EndTask(long id)
        {
            try
            {
                var task = await _taskRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
                
                if (task == null)
                    return OutputProcessing<bool>("Задача не найдена", StatusCode.TaskNotFoundry);

                task.IsDone = true;

                await _taskRepository.Update(task);
                return OutputProcessing<bool>("Задача завершена", StatusCode.Ok);
            }
            catch (Exception ex)
            {
                return HandleException<bool>(ex, "TaskService.EndTask");
            }
        }

        public async Task<IBaseResponse<IEnumerable<TaskCompletedModel>>> GetCompletedTasks()
        {
            try
            {
                var tasks = await _taskRepository.GetAll()
                    .Where(x => x.IsDone)
                    .Select(x => new TaskCompletedModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description
                    })
                    .ToListAsync();

                return OutputProcessing<IEnumerable<TaskCompletedModel>>(tasks, StatusCode.Ok);
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<TaskCompletedModel>>(ex, "TaskService.GetCompletedTasks");
            }
        }

        public async Task<IBaseResponse<IEnumerable<TaskModel>>> GetTasks(TaskFilter filter)
        {
            try
            {
                var tasks = await _taskRepository.GetAll()
                    .Where(x => !x.IsDone)
                    .WhereIf(!string.IsNullOrWhiteSpace(filter.Name), x => x.Name == filter.Name)
                    .WhereIf(filter.Priority.HasValue, x => x.Priority == filter.Priority)
                    .Select(x => new TaskModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        IsDone = x.IsDone == true ? "Готова" : "Не готова",
                        Priority = x.Priority.GetDisplayName(),
                        Created = x.Created.ToLongDateString()
                    })
                    .ToListAsync();
                return OutputProcessing<IEnumerable<TaskModel>>(tasks, StatusCode.Ok);
            }
            catch(Exception ex)
            {
                return HandleException<IEnumerable<TaskModel>>(ex, "TaskService.GetTasks");
            }
        }

        #region Private Method

        /// <summary>
        /// To optimize and simplify re-output
        /// </summary>
        /// <typeparam name="TResponse">Any type parameter</typeparam>
        /// <param name="description">Output text</param>
        /// <param name="statusCode">StatusCode</param>
        /// <returns>new BaseResponse<TResponse></returns>
        private BaseResponse<TResponse> OutputProcessing<TResponse>(string description, StatusCode statusCode)
        {
            return new BaseResponse<TResponse>()
            {
                Description = $"{description}",
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// To optimize and simplify re-output
        /// </summary>
        /// <typeparam name="TResponse">Any type parameter</typeparam>
        /// <param name="task"></param>
        /// <param name="statusCode">StatusCode</param>
        /// <returns>new BaseResponse<TResponse></returns>
        private BaseResponse<TResponse> OutputProcessing<TResponse>(TResponse task, StatusCode statusCode)
        {
            return new BaseResponse<TResponse>()
            {
                Data = task,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Merges exception output
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="ex">Passed Exception</param>
        /// <param name="getNameMethod">Method name</param>
        /// <returns>new BaseResponse<TResponse></returns>
        private BaseResponse<TResponse> HandleException<TResponse>(Exception ex, string nameMethod)
        {
            _logger.LogError(ex, $"[{nameMethod}]: {ex.Message}");
            return OutputProcessing<TResponse>(ex.Message, StatusCode.InternalServerError);
        }

        #endregion Private Method
    }
}
