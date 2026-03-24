using System.Text;
using System.Collections.ObjectModel;
using RandomStudentPickerWZ_28_4F.Models;
using RandomStudentPickerWZ_28_4F.Services;


namespace RandomStudentPickerWZ_28_4F.Views;

public partial class HomePage : ContentPage
{
    private readonly Random random = new();
    private readonly HomePageData data = new();

    private readonly ClassFileService classFileService = new();
    private readonly LuckyNumberService luckyNumberService = new();

    public HomePage()
    {
        InitializeComponent();

        ClassPicker.ItemsSource = data.Classes;
        ClassPicker.ItemDisplayBinding = new Binding("ClassName");

        ClassPicker.SelectedIndexChanged += OnClassChanged;

        LoadClasses();

        data.LuckyNumber = luckyNumberService.LoadLuckyNumber();
        LuckyNumberLabel.Text = $"Szczęśliwy numerek dziś: {data.LuckyNumber}";
    }

    private void LoadClasses()
    {
        data.Classes.Clear();

        var classes = classFileService.LoadClasses();

        foreach (var schoolClass in classes)
        {
            data.Classes.Add(schoolClass);
        }

        if (data.Classes.Count > 0 && ClassPicker.SelectedIndex == -1)
        {
            ClassPicker.SelectedIndex = 0;
        }
    }

    private void OnClassChanged(object? sender, EventArgs e)
    {
        if (ClassPicker.SelectedItem is not SchoolClass selectedClass)
        {
            return;
        }

        data.SelectedClass = selectedClass;
        StudentsView.ItemsSource = selectedClass.Students;

        DrawResult.Text = "";
    }

    private async void OnAddClassClicked(object sender, EventArgs e)
    {
        string className = (NewClassEntry.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(className))
        {
            await DisplayAlertAsync("Błąd", "Wpisz nazwę klasy", "OK");
            return;
        }

        bool exists = data.Classes.Any(c =>
            c.ClassName.Equals(className, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            await DisplayAlertAsync("Błąd", "Taka klasa już istnieje", "OK");
            return;
        }

        classFileService.AddClass(className);

        var newClass = new SchoolClass
        {
            ClassName = className
        };

        data.Classes.Add(newClass);

        NewClassEntry.Text = "";
        ClassPicker.SelectedItem = newClass;
    }

    private async void OnAddStudentClicked(object sender, EventArgs e)
    {
        if (data.SelectedClass is null)
        {
            await DisplayAlertAsync("Błąd", "Najpierw proszę wybrać klasę", "OK");
            return;
        }

        string numberText = (NewStudentNumber.Text ?? "").Trim();
        string nameOfStudent = (NewStudentName.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(numberText) || string.IsNullOrWhiteSpace(nameOfStudent))
        {
            await DisplayAlertAsync("Błąd", "Wpisz numer i imię z nazwiskiem", "OK");
            return;
        }

        if (!int.TryParse(numberText, out int studentNumber))
        {
            await DisplayAlertAsync("Błąd", "Numer musi być liczbą", "OK");
            return;
        }

        bool exist = data.SelectedClass.Students.Any(s => s.StudentsNumber == studentNumber);

        if (exist)
        {
            await DisplayAlertAsync("Błąd", "Uczeń z takim numerem już istnieje! :(", "OK");
            return;
        }

        data.SelectedClass.Students.Add(new Student
        {
            StudentsNumber = studentNumber,
            NameOfStudent = nameOfStudent,
            IsPresent = true
        });

        classFileService.SaveStudentsToFile(data.SelectedClass);

        NewStudentNumber.Text = "";
        NewStudentName.Text = "";
    }

    private void OnPresenceChanged(object sender, CheckedChangedEventArgs e)
    {
        if (data.SelectedClass is null)
        {
            return;
        }

        if (sender is CheckBox checkBox && checkBox.BindingContext is Student student)
        {
            student.IsPresent = e.Value;
            classFileService.SaveStudentsToFile(data.SelectedClass);
        }
    }

    private async void OnRemoveStudentClicked(object sender, EventArgs e)
    {
        if (data.SelectedClass is null)
        {
            return;
        }

        if (sender is Button button && button.BindingContext is Student student)
        {
            bool confirm = await DisplayAlertAsync(
                "Potwierdź",
                $"Czy jesteś pewny/pewna, że chcesz usunąć ucznia: {student.StudentsNumber} {student.NameOfStudent}?",
                "Tak",
                "Nie");

            if (!confirm)
            {
                return;
            }

            data.SelectedClass.Students.Remove(student);
            classFileService.SaveStudentsToFile(data.SelectedClass);
        }
    }

    private async void OnDrawClicked(object sender, EventArgs e)
    {
        if (data.SelectedClass is null)
        {
            await DisplayAlertAsync("Błąd", "Najpierw wybierz klasę", "OK");
            return;
        }

        var presentStudents = data.SelectedClass.Students
            .Where(s => s.IsPresent && s.StudentsNumber != data.LuckyNumber)
            .ToList();

        if (presentStudents.Count == 0)
        {
            await DisplayAlertAsync("Błąd", "Brak uczniów do losowania", "OK");
            return;
        }

        if (sender is Button drawButton)
        {
            drawButton.IsEnabled = false;
        }

        for (int i = 0; i < 15; i++)
        {
            int tempIndex = random.Next(0, presentStudents.Count);
            Student tempStudent = presentStudents[tempIndex];

            DrawResult.Text = $"Losowanie... {tempStudent.StudentsNumber} - {tempStudent.NameOfStudent}";
            await Task.Delay(80);
        }

        int finalIndex = random.Next(0, presentStudents.Count);
        Student drawn = presentStudents[finalIndex];

        DrawResult.Text = $"Wylosowano: {drawn.StudentsNumber} - {drawn.NameOfStudent}";

        if (sender is Button button)
        {
            button.IsEnabled = true;
        }
    }

}
