namespace XmlDormitoryApp;

public class SearchCriteria
{
    public string AttributeName { get; set; } = string.Empty;
    public string SearchValue { get; set; } = string.Empty;

    public SearchCriteria() { }

    public SearchCriteria(string attributeName, string searchValue)
    {
        AttributeName = attributeName;
        SearchValue = searchValue;
    }
    public bool Matches(Student student)
    {
        if (string.IsNullOrWhiteSpace(SearchValue)) return true;

        string target = AttributeName switch
        {
            "FullName" => student.FullName,
            "Faculty" => student.Faculty,
            "Course" => student.Course.ToString(),
            "Room" => student.Room.ToString(),
            "Department" => student.Department,
            "CheckInDate" => student.CheckInDate,
            "CheckOutDate" => student.CheckOutDate,
            _ => string.Empty
        };

        return target.Contains(SearchValue.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}