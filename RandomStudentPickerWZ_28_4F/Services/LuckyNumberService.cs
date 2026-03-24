using System;
using System.Collections.Generic;
using System.Text;

namespace RandomStudentPickerWZ_28_4F.Services
{
    public class LuckyNumberService
    {
        private readonly string luckyNumberFile;
        private readonly Random random = new();

        public LuckyNumberService()
        {
            luckyNumberFile = Path.Combine(FileSystem.AppDataDirectory, "lucky.txt");
        }

        public int LoadLuckyNumber()
        {
            if (!File.Exists(luckyNumberFile))
            {
                return GenerateLuckyNumber();
            }

            var lines = File.ReadAllLines(luckyNumberFile);

            if (lines.Length != 2)
            {
                return GenerateLuckyNumber();
            }

            bool parsedDate = DateTime.TryParse(lines[0], out DateTime savedDate);
            bool parsedNumber = int.TryParse(lines[1], out int savedNumber);

            if (!parsedDate || !parsedNumber)
            {
                return GenerateLuckyNumber();
            }

            if (savedDate.Date == DateTime.Today)
            {
                return savedNumber;
            }

            return GenerateLuckyNumber();
        }

        private int GenerateLuckyNumber()
        {
            int luckyNumber = random.Next(1, 31);

            File.WriteAllLines(luckyNumberFile, new string[]
            {
                DateTime.Today.ToString("yyyy-MM-dd"),
                luckyNumber.ToString()
            });

            return luckyNumber;
        }
    }
}
