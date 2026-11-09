using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Maui.Controls;

namespace XmlDormitoryApp;

public partial class DormitoryView : ContentPage
{
    private string _xmlFilePath = string.Empty;
    private string _xslFilePath = string.Empty;
    private string _htmlFilePath = string.Empty;

    private XmlProcessor _processor;
    private XmlTransformer _transformer;

    // Списки для звичайного та каскадного пошуку
    private List<Student> _allStudents = new List<Student>();
    private List<Student> _displayedStudents = new List<Student>();
    private List<string> _activeFilterSteps = new List<string>();

    public DormitoryView()
    {
        InitializeComponent();

        _processor = new XmlProcessor(new LinqSearchStrategy());
        _transformer = new XmlTransformer();
        pickerAttribute.SelectedIndex = 0;

        _xslFilePath = Path.Combine(FileSystem.AppDataDirectory, "transform.xsl");
        _htmlFilePath = Path.Combine(FileSystem.AppDataDirectory, "dormitory.html");

        EnsureXslFileExists();
    }

    private void EnsureXslFileExists()
    {
        if (!File.Exists(_xslFilePath))
        {
            File.WriteAllText(_xslFilePath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
  <xsl:template match=""/"">
    <html>
      <head>
        <meta charset=""utf-8"" />
        <title>Список мешканців гуртожитку</title>
        <style>
          body { font-family: sans-serif; background: #F8FAFC; padding: 20px; }
          table { border-collapse: collapse; width: 100%; background: white; border-radius: 8px; }
          th { background: #EDE9FE; color: #4C1D95; padding: 10px; text-align: left; }
          td { border-bottom: 1px solid #E2E8F0; padding: 10px; }
        </style>
      </head>
      <body>
        <h2>Мешканці гуртожитку</h2>
        <table>
          <tr><th>ПІБ</th><th>Факультет</th><th>Курс</th><th>Кімната</th><th>Кафедра</th></tr>
          <xsl:for-each select=""Dormitory/Student"">
            <tr>
              <td><xsl:value-of select=""FullName""/></td>
              <td><xsl:value-of select=""Faculty""/></td>
              <td><xsl:value-of select=""Course""/></td>
              <td><xsl:value-of select=""Room""/></td>
              <td><xsl:value-of select=""Department""/></td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>");
        }
    }

    private void RefreshStrategy()
    {
        if (rbLinq.IsChecked) _processor.SetStrategy(new LinqSearchStrategy());
        else if (rbDom.IsChecked) _processor.SetStrategy(new DomSearchStrategy());
        else if (rbSax.IsChecked) _processor.SetStrategy(new SaxSearchStrategy());
    }

    // СИНХРОНІЗАЦІЯ ЗМІН ІЗ ПОТОЧНИМ XML-ФАЙЛОМ НА ДИСКУ
    private void SaveAllToCurrentFile()
    {
        if (string.IsNullOrEmpty(_xmlFilePath)) return;

        var doc = new XDocument(
            new XElement("Dormitory",
                _allStudents.Select(s => new XElement("Student",
                    new XElement("FullName", s.FullName),
                    new XElement("Faculty", s.Faculty),
                    new XElement("Course", s.Course),
                    new XElement("Room", s.Room),
                    new XElement("Department", s.Department),
                    new XElement("CheckInDate", s.CheckInDate),
                    new XElement("CheckOutDate", s.CheckOutDate)
                ))
            )
        );
        doc.Save(_xmlFilePath);
    }

    // ПІДГРУЗКА УНІКАЛЬНИХ ЗНАЧЕНЬ ІЗ ВІДКРИТОГО ФАЙЛУ
    private void UpdateAvailableValues()
    {
        if (_allStudents == null || _allStudents.Count == 0)
        {
            pickerSearchValue.ItemsSource = null;
            return;
        }

        string selectedUa = pickerAttribute.SelectedItem?.ToString() ?? "ПІБ";

        var values = _allStudents
            .Select(s => selectedUa switch
            {
                "ПІБ" => s.FullName,
                "Факультет" => s.Faculty,
                "Курс" => s.Course > 0 ? s.Course.ToString() : string.Empty,
                "Номер кімнати" => s.Room > 0 ? s.Room.ToString() : string.Empty,
                "Кафедра" => s.Department,
                "Дата поселення" => s.CheckInDate,
                _ => string.Empty
            })
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        pickerSearchValue.ItemsSource = values;
        pickerSearchValue.SelectedIndex = -1;
    }

    private void OnAttributeChanged(object? sender, EventArgs e)
    {
        UpdateAvailableValues();
    }

    private void OnSearchValuePicked(object? sender, EventArgs e)
    {
        if (pickerSearchValue.SelectedItem is string val && !string.IsNullOrEmpty(val))
        {
            entrySearchValue.Text = val;
        }
    }

    // 1. ВИБІР ВХІДНОГО XML-ФАЙЛУ
    public async void OnSelectFileClicked(object sender, EventArgs e)
    {
        try
        {
            var customXmlType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".xml" } },
                { DevicePlatform.macOS, new[] { "xml" } },
                { DevicePlatform.Android, new[] { "text/xml", "application/xml" } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть XML-файл гуртожитку",
                FileTypes = customXmlType
            });

            if (result != null)
            {
                _xmlFilePath = result.FullPath;
                RefreshStrategy();
                _allStudents = _processor.Search(_xmlFilePath, new SearchCriteria("", ""));
                _displayedStudents = new List<Student>(_allStudents);
                _activeFilterSteps.Clear();
                UpdateAvailableValues();
                UpdateTable();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }

    // 2. ФІЛЬТРАЦІЯ (ЗВИЧАЙНА ТА КАСКАДНА)
    public void OnSearchClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_xmlFilePath) || !File.Exists(_xmlFilePath))
        {
            DisplayAlertAsync("Увага", "Будь ласка, спочатку відкрийте XML-файл!", "OK");
            return;
        }

        RefreshStrategy();

        string selectedUa = pickerAttribute.SelectedItem?.ToString() ?? "Факультет";
        string attr = selectedUa switch
        {
            "ПІБ" => "FullName",
            "Факультет" => "Faculty",
            "Курс" => "Course",
            "Номер кімнати" => "Room",
            "Кафедра" => "Department",
            "Дата поселення" => "CheckInDate",
            _ => selectedUa
        };

        string val = entrySearchValue.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(val))
        {
            DisplayAlertAsync("Підказка", "Оберіть або введіть значення для пошуку", "OK");
            return;
        }

        var criteria = new SearchCriteria(attr, val);

        if (cbCascade.IsChecked)
        {
            _displayedStudents = _displayedStudents.Where(s => criteria.Matches(s)).ToList();
            _activeFilterSteps.Add($"{selectedUa}='{val}'");
        }
        else
        {
            _displayedStudents = _processor.Search(_xmlFilePath, criteria);
            _activeFilterSteps.Clear();
            _activeFilterSteps.Add($"{selectedUa}='{val}'");
        }

        UpdateTable();
    }

    // 3. СКИДАННЯ ФІЛЬТРІВ
    public void OnClearClicked(object sender, EventArgs e)
    {
        entrySearchValue.Text = string.Empty;
        pickerSearchValue.SelectedIndex = -1;
        _activeFilterSteps.Clear();
        _displayedStudents = new List<Student>(_allStudents);
        UpdateTable();
    }

    private void UpdateTable()
    {
        collectionStudents.ItemsSource = null;
        collectionStudents.ItemsSource = _displayedStudents;
        labelCount.Text = $"Записів: {_displayedStudents.Count}";
        labelActiveFilters.Text = _activeFilterSteps.Count > 0 ? string.Join(" ➔ ", _activeFilterSteps) : "Немає";
    }

    // 4. ЗБЕРЕЖЕННЯ ВІДФІЛЬТРОВАНОГО XML
    public async void OnSaveFilteredClicked(object sender, EventArgs e)
    {
        if (_displayedStudents.Count == 0)
        {
            await DisplayAlertAsync("Увага", "Немає записів для збереження!", "OK");
            return;
        }

        string fileName = await DisplayPromptAsync("Збереження XML", "Введіть ім'я нового файлу:", initialValue: "filtered_dormitory.xml");
        if (string.IsNullOrWhiteSpace(fileName)) return;

        if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) fileName += ".xml";

        string targetDir = Path.GetDirectoryName(_xmlFilePath);
        if (string.IsNullOrEmpty(targetDir)) targetDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string fullPath = Path.Combine(targetDir, fileName);

        var doc = new XDocument(
            new XElement("Dormitory",
                _displayedStudents.Select(s => new XElement("Student",
                    new XElement("FullName", s.FullName),
                    new XElement("Faculty", s.Faculty),
                    new XElement("Course", s.Course),
                    new XElement("Room", s.Room),
                    new XElement("Department", s.Department),
                    new XElement("CheckInDate", s.CheckInDate),
                    new XElement("CheckOutDate", s.CheckOutDate)
                ))
            )
        );

        doc.Save(fullPath);
        await DisplayAlertAsync("Успіх", $"Файл успішно збережено за адресою:\n{fullPath}", "OK");
    }

    // 5. XSLT-ТРАНСФОРМАЦІЯ В HTML
    public async void OnTransformClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_xmlFilePath))
        {
            await DisplayAlertAsync("Увага", "Спочатку відкрийте файл!", "OK");
            return;
        }

        try
        {
            _transformer.TransformToHtml(_xmlFilePath, _xslFilePath, _htmlFilePath);
            await DisplayAlertAsync("Успіх XSLT", $"HTML-звіт згенеровано за адресою:\n{_htmlFilePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Помилка", ex.Message, "OK");
        }
    }

    // 6. ДОДАВАННЯ СТУДЕНТА ЧЕРЕЗ EditStudentPage
    public async void OnAddStudentClicked(object sender, EventArgs e)
    {
        var editPage = new EditStudentPage(student: null, isNew: true);
        editPage.Disappearing += (s, args) =>
        {
            if (editPage.StudentData != null && !string.IsNullOrWhiteSpace(editPage.StudentData.FullName))
            {
                _allStudents.Add(editPage.StudentData);
                _activeFilterSteps.Clear();
                _displayedStudents = new List<Student>(_allStudents);
                SaveAllToCurrentFile();
                UpdateAvailableValues();
                UpdateTable();
            }
        };
        await Navigation.PushModalAsync(editPage);
    }

    // 7. РЕДАГУВАННЯ ЧЕРЕЗ EditStudentPage
    public async void OnEditStudentClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Student student)
        {
            var editPage = new EditStudentPage(student: student, isNew: false);
            editPage.Disappearing += (s, args) =>
            {
                if (editPage.IsDeleteRequested)
                {
                    _allStudents.Remove(student);
                    _displayedStudents.Remove(student);
                    SaveAllToCurrentFile();
                    UpdateAvailableValues();
                    UpdateTable();
                }
                else if (editPage.StudentData != null)
                {
                    int idxAll = _allStudents.IndexOf(student);
                    if (idxAll >= 0) _allStudents[idxAll] = editPage.StudentData;

                    int idxDisp = _displayedStudents.IndexOf(student);
                    if (idxDisp >= 0) _displayedStudents[idxDisp] = editPage.StudentData;

                    SaveAllToCurrentFile();
                    UpdateAvailableValues();
                    UpdateTable();
                }
            };
            await Navigation.PushModalAsync(editPage);
        }
    }

    public async void OnDeleteStudentClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Student student)
        {
            bool confirm = await DisplayAlertAsync("Видалення", $"Видалити студента {student.FullName}?", "Так", "Ні");
            if (confirm)
            {
                _allStudents.Remove(student);
                _displayedStudents.Remove(student);
                SaveAllToCurrentFile();
                UpdateAvailableValues();
                UpdateTable();
            }
        }
    }

    // 8. ДОВІДКА ТА ВИХІД
    public async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync("Про програму",
            "Лабораторна робота №2 з ООП\n" +
            "Тема: «Аналіз XML-документів» (Варіант: Гуртожиток)\n" +
            "Студентка: Кучинська Софія\n" +
            "Курс: 2 | Група: К28\n" +
            "Рік: 2026\n\n" +
            "Реалізовано: патерн Strategy (LINQ to XML, DOM, SAX), XSLT-трансформацію, каскадну фільтрацію та експорт результатів.",
            "Зрозуміло");
    }

    public async void OnExitClicked(object sender, EventArgs e)
    {
        bool exit = await DisplayAlertAsync("Підтвердження", "Ви дійсно бажаєте вийти з програми?", "Так", "Ні");
        if (exit) Application.Current?.Quit();
    }
}