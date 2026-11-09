using System.Xml;

namespace XmlDormitoryApp;

public class DomSearchStrategy : IXmlSearchStrategy
{
    public List<Student> Search(string filePath, SearchCriteria criteria)
    {
        var results = new List<Student>();
        var doc = new XmlDocument();
        doc.Load(filePath);
        XmlNodeList nodes = doc.SelectNodes("//Student");
        if (nodes == null) return results;
        foreach (XmlNode node in nodes)
        {
            var student = new Student
            {
                FullName = node["FullName"]?.InnerText ?? "",
                Faculty = node["Faculty"]?.InnerText ?? "",
                Course = int.TryParse(node["Course"]?.InnerText, out int c) ? c : 0,
                Room = int.TryParse(node["Room"]?.InnerText, out int r) ? r : 0,
                Department = node["Department"]?.InnerText ?? "",
                CheckInDate = node["CheckInDate"]?.InnerText ?? "",
                CheckOutDate = node["CheckOutDate"]?.InnerText ?? ""
            };

            if (criteria.Matches(student))
            {
                results.Add(student);
            }
        }

        return results;
    }
}