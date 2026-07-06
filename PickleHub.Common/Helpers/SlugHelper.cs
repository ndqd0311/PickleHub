using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PickleHub.Common.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if( string.IsNullOrEmpty(input) ) 
                return string.Empty;

            var slug = input.ToLowerInvariant();
            slug = slug.Normalize(NormalizationForm.FormD);
            
            var chars = slug
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                        != UnicodeCategory.NonSpacingMark)
                .ToArray();
            slug = new string(chars).Normalize(NormalizationForm.FormC);

            slug = slug
                .Replace("đ", "d")
                .Replace("Đ", "d");

            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, @"-{2,}", "-").Trim('-');

            return slug;
        }
    }
}
