using CampusRouteLab.Models;

namespace CampusRouteLab.Services;

public class StudentCatalogService : IStudentCatalogService
{
    private readonly Dictionary<string, Group> _groups;
    
    public StudentCatalogService()
    {
        _groups = new Dictionary<string, Group>
        {
            ["PI-101"] = new Group
            {
                Name = "PI-101",
                Students = new List<Student>
                {
                    new() { Id = 1, Name = "Иван Иванов", Email = "ivan@example.com" },
                    new() { Id = 2, Name = "Мария Петрова", Email = "maria@example.com" }
                }
            },
            ["PI-102"] = new Group
            {
                Name = "PI-102",
                Students = new List<Student>
                {
                    new() { Id = 1, Name = "Петр Сидоров", Email = "petr@example.com" },
                    new() { Id = 2, Name = "Анна Козлова", Email = "anna@example.com" },
                    new() { Id = 3, Name = "Дмитрий Смирнов", Email = "dmitry@example.com" }
                }
            }
        };
    }
    
    public Dictionary<string, Group> GetAllGroups() => _groups;
    public Group? GetGroup(string groupName) => _groups.GetValueOrDefault(groupName);
    public Student? GetStudent(string groupName, int studentId)
    {
        var group = GetGroup(groupName);
        return group?.Students.FirstOrDefault(s => s.Id == studentId);
    }
}