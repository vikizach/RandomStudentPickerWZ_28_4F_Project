using System;
using System.Collections.Generic;
using System.Text;

namespace RandomStudentPickerWZ_28_4F.Models
{
    public class Student
    {
        public int StudentsNumber {  get; set; }
        public string NameOfStudent { get; set; } = "";
        public bool IsPresent { get; set; } = true;
    }
}
