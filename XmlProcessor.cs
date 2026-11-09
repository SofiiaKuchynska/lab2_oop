namespace XmlDormitoryApp;

public class XmlProcessor
{
    private IXmlSearchStrategy _strategy;
    public XmlProcessor(IXmlSearchStrategy defaultStrategy)
    {
        _strategy = defaultStrategy;
    }
    public void SetStrategy(IXmlSearchStrategy strategy)
    {
        _strategy = strategy;
    }
    public List<Student> Search(string filePath, SearchCriteria criteria)
    {
        return _strategy.Search(filePath, criteria);
    }
    public List<string> GetAvailableAttributes(string filePath)
    {
        return new List<string>
        {
            "FullName",
            "Faculty",
            "Course",
            "Room",
            "Department",
            "CheckInDate",
            "CheckOutDate"
        };
    }
}