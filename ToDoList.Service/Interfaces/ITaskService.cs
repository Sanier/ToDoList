using ToDoList.Domain.Entity;
using ToDoList.Domain.Filters.Task;
using ToDoList.Domain.Models;
using ToDoList.Domain.Response;

namespace ToDoList.Service.Interfaces
{
    public interface ITaskService
    {
        Task<IBaseResponse<TaskEntity>> Create(CreateTaskModel model);

        Task<IBaseResponse<bool>> EndTask(long id);

        Task<IBaseResponse<IEnumerable<TaskModel>>> GetTasks(TaskFilter filter);
    }
}