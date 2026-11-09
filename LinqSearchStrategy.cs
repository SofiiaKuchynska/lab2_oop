using System.Xml.Linq;

namespace XmlDormitoryApp;

public class LinqSearchStrategy : IXmlSearchStrategy
{
    public List<Student> Search(string filePath, SearchCriteria criteria)
    {
        var doc = XDocument.Load(filePath);
        var students = doc.Descendants("Student").Select(el => new Student
        {
            FullName = (string)el.Element("FullName") ?? "",
            Faculty = (string)el.Element("Faculty") ?? "",
            Course = int.TryParse((string)el.Element("Course"), out int c) ? c : 0,
            Room = int.TryParse((string)el.Element("Room"), out int r) ? r : 0,
            Department = (string)el.Element("Department") ?? "",
            CheckInDate = (string)el.Element("CheckInDate") ?? "",
            CheckOutDate = (string)el.Element("CheckOutDate") ?? ""
        });
        return students.Where(s => criteria.Matches(s)).ToList();
    }
}