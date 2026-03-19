using System.Text;
using System.Collections.ObjectModel;
using RandomStudentPickerWZ_28_4F.Models;


namespace RandomStudentPickerWZ_28_4F.Views;

public partial class HomePage : ContentPage
{
	private readonly Random random = new Random();

	private readonly string classesDirectory;

	private int luckyNumber = -1;

	private DateTime luckyDate;

	private string luckyNumberFile;

	private ObservableCollection<string> classes = new();

	private ObservableCollection<Student> students = new();

	public HomePage()
	{
		InitializeComponent();

		classesDirectory = Path.Combine(FileSystem.AppDataDirectory, "Classes");

		Directory.CreateDirectory(classesDirectory);

		ClassPicker.ItemsSource = classes;
		StudentsView.ItemsSource = students;

		ClassPicker.SelectedIndexChanged += OnClassChanged;

		LoadClasses();

		luckyNumberFile = Path.Combine(FileSystem.AppDataDirectory, "lucky.txt");

		LoadLuckyNumber();

		LuckyNumberLabel.Text = $"Szczęśliwy numerek dziś: {luckyNumber}";
	}

	private string GetClassPath(string className)
	{
		return Path.Combine(classesDirectory, $"{className}.txt");
	}

	private void LoadClasses()
	{
		classes.Clear();

		var files = Directory.GetFiles(classesDirectory, "*.txt").OrderBy(f => f);

		foreach (var file in files)
		{
			string className = Path.GetFileNameWithoutExtension(file);

			classes.Add(className);
		}

		if (classes.Count > 0 && ClassPicker.SelectedIndex == -1)
		{
			ClassPicker.SelectedIndex = 0;
		}
	}

	private List<Student> LoadStudentsFromFile(string className)
	{
		var result = new List<Student>();

		string path = GetClassPath(className);

		if (!File.Exists(path))
		{
			return result;
		}

		var lines = File.ReadAllLines(path, Encoding.UTF8);

		foreach (var line in lines)
		{
			if (string.IsNullOrWhiteSpace(line)) { continue; }

			var parts = line.Split("|");
			if (parts.Length != 3) { continue; }

			bool parsedStudentNumber = int.TryParse(parts[0], out int studentNumber);

			if (!parsedStudentNumber) { continue; }

			bool parsedPresence = bool.TryParse(parts[2], out bool isPresent);
			if(!parsedPresence) { continue; }

			var studentName = new Student
			{
				StudentsNumber = studentNumber,
				NameOfStudent = parts[1].Trim(),
				IsPresent = isPresent
			};

			result.Add(studentName);
		}
		return result.OrderBy(s => s.StudentsNumber).ToList();
	}

	private void SaveStudentsToFile(string className)
	{
		string path = GetClassPath(className);

		var lines = students.OrderBy(s => s.StudentsNumber)
			.Select(s => $"{s.StudentsNumber}|{s.NameOfStudent}|{s.IsPresent}").ToArray();

		File.WriteAllLines(path, lines, Encoding.UTF8);
	}

	private void OnClassChanged(object? sender, EventArgs e)
	{
		string? className = ClassPicker.SelectedItem as string;

		if (string.IsNullOrEmpty(className)) { return; }

		students.Clear();

		var loadStudents = LoadStudentsFromFile(className);

		foreach (var student in loadStudents)
		{
			students.Add(student);
		}

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

		string path = GetClassPath(className);

		if (!File.Exists(path))
		{
			File.WriteAllText(path, "", Encoding.UTF8);
		}

		NewClassEntry.Text = "";

		LoadClasses();

		ClassPicker.SelectedItem = className;
	}

	private async void OnAddStudentClicked(object sender, EventArgs e)
	{
		string? className = ClassPicker.SelectedItem as string;

		if (string.IsNullOrEmpty(className))
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

		bool parsed = int.TryParse(numberText, out int studentNumber);

		if (!parsed) 
		{
			await DisplayAlertAsync("Błąd", "Numer musi być liczbą", "OK");
			return;
		}

		bool exist = students.Any(s => s.StudentsNumber == studentNumber);

		if (exist) 
		{
			await DisplayAlertAsync("Błąd", "Uczeń z takim numerem już istnieje! :(", "OK");
			return;
		}

		students.Add(new Student
		{
            StudentsNumber = studentNumber,
			NameOfStudent = nameOfStudent,
			IsPresent = true
		});

		SaveStudentsToFile(className);

		NewStudentNumber.Text = "";
		NewStudentName.Text = "";

	}

	private void OnPresenceChanged(object sender, CheckedChangedEventArgs e)
	{
		string? className = ClassPicker.SelectedItem as string;

		if (string.IsNullOrEmpty(className))
		{
			return;
		}
		
		if(sender is CheckBox checkBox && checkBox.BindingContext is Student student)
		{
			student.IsPresent = e.Value;
			SaveStudentsToFile(className);
		}

	}

    private async void OnRemoveStudentClicked(object sender, EventArgs e)
	{
		string? className = ClassPicker.SelectedItem as string;

		if(string.IsNullOrWhiteSpace(className))
		{
			return;
		}

		if(sender is Button button && button.BindingContext is Student student)
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

			students.Remove(student);
			SaveStudentsToFile(className);
		}
	}

	private async void OnDrawClicked(object sender, EventArgs e)
	{
		var presentStudents = students
			.Where(s => s.IsPresent == true && s.StudentsNumber != luckyNumber)
			.ToList();

		if(presentStudents.Count == 0)
		{
			await DisplayAlertAsync("Błąd", "Brak uczniów do losowania", "OK");
			return;
		}

		if(sender is Button drawButton)
		{
			drawButton.IsEnabled = false;
		}

		for(int i = 0; i < 15; i++)
		{
			int tempIndex = random.Next(0, presentStudents.Count);
			Student tempStudent = presentStudents[tempIndex];

			DrawResult.Text = $"Losowanie...{tempStudent.StudentsNumber} - {tempStudent.NameOfStudent}";

			await Task.Delay(80);
		}

		int finalIndex = random.Next(0, presentStudents.Count);
		Student drawn = presentStudents[finalIndex];

		DrawResult.Text = $"Wylosowano: {drawn.StudentsNumber} - {drawn.NameOfStudent}";

		if(sender is Button button)
		{
			button.IsEnabled = true;
		}
	}

    private void GenerateLuckyNumber()
    {
        luckyNumber = random.Next(1, 31);

        luckyDate = DateTime.Today;

        File.WriteAllLines(luckyNumberFile, new string[]{
            luckyDate.ToString("yyyy-MM-dd"),
            luckyNumber.ToString()
        });
    }

    private void LoadLuckyNumber()
	{
		if (!File.Exists(luckyNumberFile))
		{
            GenerateLuckyNumber();
            return;
		}

		var lines = File.ReadAllLines(luckyNumberFile);

		if(lines.Length != 2)
		{
            GenerateLuckyNumber();
            return;
		}

		bool parsedDate = DateTime.TryParse(lines[0], out DateTime savedDate);

		bool parsedNumber = int.TryParse(lines[1], out int savedNumber);

		if(!parsedDate || !parsedNumber)
		{
			GenerateLuckyNumber();
            return;
		}

		if(savedDate.Date == DateTime.Today)
		{
			luckyDate = savedDate;
			luckyNumber = savedNumber;
		}
		else
		{
			GenerateLuckyNumber();

        }
	}

}