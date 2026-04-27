namespace CampusRouteLab.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class Group
{
    public string Name { get; set; } = string.Empty;
    public List<Student> Students { get; set; } = new();
}