using System;
using System.Globalization;
using System.Text;

namespace AuthApi.Helper
{
    public class StringHelper
    {
        public static string RemoveAccents(string input)
        {
            return new string(input
                .Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }
    }
}