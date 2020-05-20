using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace DatasetUtil.Components
{
    public static class Utils
    {
        public static bool IsConfigFile(string fileName)
        {
            return IsMatchingFileName(fileName, Constants.ConfigFileName);
        }

        public static bool IsMatchingFileName(string fileName, string baseName)
        {
            return Path.GetFileName(fileName).IsMatchingText(baseName);
        }

        public static IEnumerable<T> Tap<T>(this IEnumerable<T> items, Action<T> handleItem)
        {
            foreach (var item in items)
            {
                handleItem(item);
                yield return item;
            }
        }

        public static bool IsMatchingText(this string text1, string text2)
        {
            return string.Compare(text1, text2, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
