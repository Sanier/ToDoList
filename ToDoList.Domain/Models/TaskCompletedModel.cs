namespace ToDoList.Domain.Models
{
    public class TaskCompletedModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Created { get; set; }
    }
}
