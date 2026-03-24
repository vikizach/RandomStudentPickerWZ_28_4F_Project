using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace RandomStudentPickerWZ_28_4F.Models
{
    public class SchoolClass
    {
        public string ClassName { get; set; } = "";
        public ObservableCollection<Student> Students { get; set; } = new();
    }
}
