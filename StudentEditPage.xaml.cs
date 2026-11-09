using System;
using Microsoft.Maui.Controls;

namespace XmlDormitoryApp;

public partial class EditStudentPage : ContentPage
{
    public Student StudentData { get; private set; }
    public bool IsDeleteRequested { get; private set; }
    private readonly bool _isNew;

    public EditStudentPage(Student? student = null, bool isNew = false)
    {
        InitializeComponent();
        _isNew = isNew;

        if (student != null)
        {
            StudentData = new Student(
                student.FullName,
                student.Faculty,
                student.Course,
                student.Room,
                student.Department,
                student.CheckInDate,
                student.CheckOutDate
            );

            FullNameEntry.Text = student.FullName;
            FacultyEntry.Text = student.Faculty;
            CourseEntry.Text = student.Course > 0 ? student.Course.ToString() : string.Empty;
            RoomEntry.Text = student.Room > 0 ? student.Room.ToString() : string.Empty;
            DeptEntry.Text = student.Department;
            CheckInEntry.Text = student.CheckInDate;
            CheckOutEntry.Text = student.CheckOutDate;
        }
        else
        {
            StudentData = new Student();
        }

        PageHeaderLabel.Text = _isNew ? "Новий запис студента" : "Редагування студента";
        DeleteBtn.IsVisible = !_isNew;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            await DisplayAlertAsync("Помилка", "ПІБ студента є обов'язковим!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(FacultyEntry.Text))
        {
            await DisplayAlertAsync("Помилка", "Вкажіть факультет!", "OK");
            return;
        }

        if (!int.TryParse(CourseEntry.Text, out int course) || course
< 1 || course > 6)
        {
            await DisplayAlertAsync("Помилка", "Курс має бути числом від 1 до 6!", "OK");
            return;
        }

        if (!int.TryParse(RoomEntry.Text, out int room) || room
    < 100 || room > 925)
        {
            await DisplayAlertAsync("Помилка", "Номер кімнати має бути від 100 до 925!", "OK");
            return;
        }

        string checkIn = CheckInEntry.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(checkIn) && !DateTime.TryParse(checkIn, out _))
        {
            await DisplayAlertAsync("Помилка", "Невірний формат дати поселення (наприклад, 2025-08-28)!", "OK");
            return;
        }

        string checkOut = CheckOutEntry.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(checkOut) && !DateTime.TryParse(checkOut, out _))
        {
            await DisplayAlertAsync("Помилка", "Невірний формат дати виселення (наприклад, 2027-06-30)!", "OK");
            return;
        }

        StudentData.FullName = FullNameEntry.Text.Trim();
        StudentData.Faculty = FacultyEntry.Text.Trim();
        StudentData.Course = course;
        StudentData.Room = room;
        StudentData.Department = DeptEntry.Text?.Trim() ?? string.Empty;
        StudentData.CheckInDate = checkIn;
        StudentData.CheckOutDate = checkOut;

        await Navigation.PopModalAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Підтвердження", $"Ви дійсно хочете видалити студента {StudentData.FullName}?", "Так, видалити", "Ні");
        if (!confirm) return;

        IsDeleteRequested = true;
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        StudentData = null!;
        await Navigation.PopModalAsync();
    }
}