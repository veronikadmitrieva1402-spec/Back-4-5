using CampusRouteLab.Models;

namespace CampusRouteLab.Services;

public interface IStudentCatalogService
{
    Dictionary<string, Group> GetAllGroups();
    Group? GetGroup(string groupName);
    Student? GetStudent(string groupName, int studentId);
}