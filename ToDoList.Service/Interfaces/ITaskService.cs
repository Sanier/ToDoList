using ToDoList.Domain.Entity;
using ToDoList.Domain.Models;
using ToDoList.Domain.Response;

namespace ToDoList.Service.Interfaces
{
    public interface ITaskService
    {
        Task<IBaseResponse<TaskEntity>> Create(CreateTaskModel model);
    }
}