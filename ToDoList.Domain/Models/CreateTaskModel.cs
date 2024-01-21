using ToDoList.Domain.Enum;

namespace ToDoList.Domain.Models
{
    public class CreateTaskModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
    }
}
