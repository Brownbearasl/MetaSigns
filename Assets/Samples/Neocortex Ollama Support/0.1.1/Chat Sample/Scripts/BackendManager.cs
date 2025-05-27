using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NPOI.XSSF.UserModel;
using UnityEngine;

public static class BackendManager
{
    private static readonly string ExcelPath = Path.Combine(Application.dataPath, "Resources", "Concepts.xlsx");

    public static (string subjectName, string[] conceptWords) GetRandomSubjectAndConcepts()
    {
        try
        {
            using (FileStream file = new FileStream(ExcelPath, FileMode.Open, FileAccess.Read))
            {
                var workbook = new XSSFWorkbook(file);

                // Pick a random sheet
                int sheetCount = workbook.NumberOfSheets;
                int randomSheetIndex = UnityEngine.Random.Range(0, sheetCount);
                var sheet = workbook.GetSheetAt(randomSheetIndex);
                string subjectName = sheet.SheetName;

                // Read words from column A
                List<string> words = new List<string>();
                for (int row = 0; row <= sheet.LastRowNum; row++)
                {
                    var cell = sheet.GetRow(row)?.GetCell(0);
                    if (cell != null)
                    {
                        string word = cell.ToString().Trim();
                        if (!string.IsNullOrEmpty(word))
                            words.Add(word);
                    }
                }

                if (words.Count < 3)
                    throw new Exception($"Sheet '{subjectName}' must have at least 3 words.");

                // Pick 3 random words
                var selectedWords = words.OrderBy(_ => UnityEngine.Random.value).Take(3).ToArray();

                return (subjectName, selectedWords);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading Excel file: " + e.Message);
            return ("UnknownSubject", new[] { "default1", "default2", "default3" });
        }
    }
}
