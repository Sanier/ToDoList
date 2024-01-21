using ToDoList.Domain.Enum;

namespace ToDoList.Domain.Models
{
    public class CreateTaskModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }

        public void Validate()
        {
            if(string.IsNullOrWhiteSpace(Name))
                throw new ArgumentNullException(Name, "Укажите название задачи");

            if (string.IsNullOrWhiteSpace(Description))
                throw new ArgumentNullException(Description, "Укажите описание задачи");
        }
    }
}
