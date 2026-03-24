using RandomStudentPickerWZ_28_4F.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RandomStudentPickerWZ_28_4F.Services
{
    public class ClassFileService
    {
        private readonly string classesDirectory;

        public ClassFileService()
        {
            classesDirectory = Path.Combine(FileSystem.AppDataDirectory, "Classes");
            Directory.CreateDirectory(classesDirectory);
        }

        private string GetClassPath(string className)
        {
            return Path.Combine(classesDirectory, $"{className}.txt");
        }

        public List<SchoolClass> LoadClasses()
        {
            var result = new List<SchoolClass>();

            var files = Directory.GetFiles(classesDirectory, "*.txt")
                                 .OrderBy(f => f);

            foreach (var file in files)
            {
                string className = Path.GetFileNameWithoutExtension(file);

                var students = new ObservableCollection<Student>(LoadStudentsFromFile(className));

                result.Add(new SchoolClass
                {
                    ClassName = className,
                    Students = students
                });
            }

            return result;
        }

        public List<Student> LoadStudentsFromFile(string className)
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
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split("|");
                if (parts.Length != 3)
                    continue;

                bool parsedStudentNumber = int.TryParse(parts[0], out int studentNumber);
                if (!parsedStudentNumber)
                    continue;

                bool parsedPresence = bool.TryParse(parts[2], out bool isPresent);
                if (!parsedPresence)
                    continue;

                result.Add(new Student
                {
                    StudentsNumber = studentNumber,
                    NameOfStudent = parts[1].Trim(),
                    IsPresent = isPresent
                });
            }

            return result.OrderBy(s => s.StudentsNumber).ToList();
        }

        public void SaveStudentsToFile(SchoolClass schoolClass)
        {
            string path = GetClassPath(schoolClass.ClassName);

            var lines = schoolClass.Students
                .OrderBy(s => s.StudentsNumber)
                .Select(s => $"{s.StudentsNumber}|{s.NameOfStudent}|{s.IsPresent}")
                .ToArray();

            File.WriteAllLines(path, lines, Encoding.UTF8);
        }

        public void AddClass(string className)
        {
            string path = GetClassPath(className);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "", Encoding.UTF8);
            }
        }
    }
}
