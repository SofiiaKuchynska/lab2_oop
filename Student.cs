namespace XmlDormitoryApp;

public class Student
{
    public string FullName { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public int Course { get; set; }
    public int Room { get; set; }
    public string Department { get; set; } = string.Empty;
    public string CheckInDate { get; set; } = string.Empty;
    public string CheckOutDate { get; set; } = string.Empty;

    public Student() { }

    public Student(string fullName, string faculty, int course, int room, string department, string checkInDate, string checkOutDate)
    {
        FullName = fullName;
        Faculty = faculty;
        Course = course;
        Room = room;
        Department = department;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }
}