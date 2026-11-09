namespace XmlDormitoryApp;

public interface IXmlSearchStrategy
{
    List<Student> Search(string filePath, SearchCriteria criteria);
}