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
                {
                    return new BaseResponse<TaskEntity>()
                    {
                        Description = "Задача с таким названием уже есть",
                        StatusCode = StatusCode.TaskIsHasAlready
                    };
                }

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
                return new BaseResponse<TaskEntity>()
                {
                    Description = "Задача создалась",
                    StatusCode = StatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TaskService.Create]: {ex.Message}");
                return new BaseResponse<TaskEntity>
                {
                    Description= $"{ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<IBaseResponse<bool>> EndTask(long id)
        {
            try
            {
                var task = await _taskRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
                if (task == null)
                    return new BaseResponse<bool>()
                    {
                        Description = "Задача не найдена",
                        StatusCode = StatusCode.TaskNotFoundry
                    };

                task.IsDone = true;

                await _taskRepository.Update(task);

                return new BaseResponse<bool>()
                {
                    Description = "Задача завершена",
                    StatusCode = StatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TaskService.GetTasks]: {ex.Message}");
                return new BaseResponse<bool>()
                {
                    Description = $"{ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
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

                return new BaseResponse<IEnumerable<TaskModel>>()
                {
                    Data = tasks,
                    StatusCode = StatusCode.Ok
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"[TaskService.GetTasks]: {ex.Message}");
                return new BaseResponse<IEnumerable<TaskModel>>()
                {
                    Description = $"{ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}
