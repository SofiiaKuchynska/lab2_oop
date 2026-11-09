using System.Xml;

namespace XmlDormitoryApp;

public class SaxSearchStrategy : IXmlSearchStrategy
{
    public List<Student> Search(string filePath, SearchCriteria criteria)
    {
        var results = new List<Student>();

        using var reader = XmlReader.Create(filePath);

        Student current = null;
        string currentTag = "";
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                currentTag = reader.Name;
                if (currentTag == "Student")
                {
                    current = new Student();
                }
            }
            else if (reader.NodeType == XmlNodeType.Text && current != null)
            {
                switch (currentTag)
                {
                    case "FullName": current.FullName = reader.Value; break;
                    case "Faculty": current.Faculty = reader.Value; break;
                    case "Course": if (int.TryParse(reader.Value, out int c)) current.Course = c; break;
                    case "Room": if (int.TryParse(reader.Value, out int r)) current.Room = r; break;
                    case "Department": current.Department = reader.Value; break;
                    case "CheckInDate": current.CheckInDate = reader.Value; break;
                    case "CheckOutDate": current.CheckOutDate = reader.Value; break;
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Student" && current != null)
            {
                if (criteria.Matches(current))
                {
                    results.Add(current);
                }
                current = null;
            }
        }

        return results;
    }
}