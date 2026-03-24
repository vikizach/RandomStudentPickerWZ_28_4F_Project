
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace RandomStudentPickerWZ_28_4F.Models
{
    public class HomePageData
    {
        public ObservableCollection<SchoolClass> Classes { get; set; } = new();
        public SchoolClass? SelectedClass { get; set; }
        public string? SelectedClassName { get; set; }
        public int LuckyNumber { get; set; } = -1;
    }
}