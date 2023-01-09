/*
 * Copyright (c) 2021 Alisha Taylor
 * Open source software. Licensed under the MIT license: https://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Predictive
{
    internal static class ExtensionCore
    {
        internal static bool IsNumeric(this string str)
        {
            return Regex.IsMatch(str, @"^\d+$");
        }

        internal static bool IsNotEmpty(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        internal static bool IsEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        internal static bool IsNotEmpty<T>(this T[] array)
        {
            if (array == null) return false;
            return array.Length > 0;
        }

        internal static bool IsEmpty<T>(this T[] array)
        {
            if (array == null) return true;
            return array.Length == 0;
        }

        internal static bool IsNotEmpty<T>(this List<T> list)
        {
            if (list == null) return false;
            return list.Count > 0;
        }

        internal static bool IsEmpty<T>(this List<T> list)
        {
            if (list == null) return true;
            return list.Count == 0;
        }

        internal static bool ContainsXx(this string str, string substring)
        {
            if (str == null || substring.IsEmpty()) return false;
            return str.ToLower().Contains(substring.ToLower());
        }

        internal static string[] IntersectXx(this IEnumerable<string> array1, IEnumerable<string> array2)
        {
            List<string> commonArray = new();
            foreach (string item1 in array1)
            {
                foreach (string item2 in array2)
                {
                    if (item1.EqualsXx(item2))
                    {
                        commonArray.Add(item1);
                    }
                }
            }
            return commonArray.ToArray();
        }

        internal static bool ContainsXx(this IEnumerable<string> array, string str)
        {
            if (array == null) return false;
            foreach (string item in array)
            {
                if (string.Equals(item, str, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        internal static bool RemoveXx(this List<string> array, string str)
        {
            if (array == null) return false;
            foreach (string item in array)
            {
                if (string.Equals(item, str, StringComparison.OrdinalIgnoreCase))
                {
                    array.Remove(item);
                    return true;
                }
            }
            return false;
        }

        internal static string RemoveAll(this string str, string str1, string str2, string str3 = null)
        {
            if (str == null) return null;
            string ret = str.Replace(str1, "").Replace(str2, "");
            if (str3 != null) ret = ret.Replace(str3 ?? "", "");
            return ret;
        }

        internal static string RemoveAllXx(this string str, IEnumerable<string> strArray)
        {
            if (str == null) return null;
            if (strArray == null) return str;

            foreach (string strArr in strArray)
            {
                str = str.RemoveXx(strArr, StringComparison.OrdinalIgnoreCase);
            }
            return str;
        }

        internal static string RemoveXx(this string str, string str2, StringComparison comp)
        {
            if (str == null) return null;
            if (str2 == null) return str;

            int idx = str.IndexOf(str2, comp);
            if (idx < 0) return str;
            string str3 = str.Remove(idx, str2.Length);
            return str3;
        }

        internal static IEnumerable<string> RemoveAll(this IEnumerable<string> strArray, IEnumerable<string> strArray2)
        {
            foreach (string str in strArray2)
            {
                if (strArray.Any(x => x.Equals(str)))
                {
                    strArray.ToList().Remove(str);
                }
            }
            return strArray;
        }

        internal static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            HashSet<TKey> seenKeys = new();
            foreach (TSource source1 in source)
            {
                TSource element = source1;
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
            }
        }

        internal static string SubstringX(this string str, int startPos, int endPos)
        {
            return str[startPos..endPos];
        }

        internal static string SeparateNonEmptyStrings(this IEnumerable<string> array, string separator)
        {
            IEnumerable<string> arr = array.Where(x => x.IsNotEmpty());
            string str = string.Join(separator, arr);
            return str;
        }

        internal static bool EqualsXx(this string str, string str2)
        {
            return str.Equals(str2, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsAny(this IEnumerable<string> strArray, IEnumerable<string> strArray2)
        {
            List<string> list = new();
            list.AddRange(strArray2);
            foreach (string listItem in list)
            {
                if (strArray.Any(x => x.Equals(listItem)))
                    return true;
            }
            return false;
        }

        internal static bool EqualsAny(this string str, IEnumerable<string> array)
        {
            bool equalToAny = array.Any(x => x.Equals(str));
            return equalToAny;
        }

        internal static bool EqualsAnyXx(this string str, IEnumerable<string> array)
        {
            bool equalToAny = array.Any(x => x.Equals(str, StringComparison.OrdinalIgnoreCase));
            return equalToAny;
        }

        internal static bool EqualsAnyXx(this string str, string str1, string str2 = null, string str3 = null, string str4 = null, string str5 = null, string str6 = null)
        {
            List<string> list = new();
            if (str1 != null) list.Add(str1);
            if (str2 != null) list.Add(str2);
            if (str3 != null) list.Add(str3);
            if (str4 != null) list.Add(str4);
            if (str5 != null) list.Add(str5);
            if (str6 != null) list.Add(str6);
            bool equalToAny = list.Any(x => x.Equals(str, StringComparison.OrdinalIgnoreCase));
            return equalToAny;
        }

        internal static bool EqualsAny(this string str, string str1, string str2 = null, string str3 = null, string str4 = null, string str5 = null, string str6 = null)
        {
            List<string> list = new();
            if (str1 != null) list.Add(str1);
            if (str2 != null) list.Add(str2);
            if (str3 != null) list.Add(str3);
            if (str4 != null) list.Add(str4);
            if (str5 != null) list.Add(str5);
            if (str6 != null) list.Add(str6);
            bool equalToAny = list.Any(x => x.Equals(str));
            return equalToAny;
        }

        internal static string RemoveNewlines(this string str)
        {
            string[] lines = str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string str2 = string.Join("", lines);
            return str2;
        }


        internal static bool StartsWithAny(this IEnumerable<string> strArray, string str1, string str2 = null, string str3 = null, string str4 = null, string str5 = null, bool caseInsensitive = false)
        {
            List<string> list = new();
            if (str1 != null) list.Add(str1);
            if (str2 != null) list.Add(str2);
            if (str3 != null) list.Add(str3);
            if (str4 != null) list.Add(str4);
            if (str5 != null) list.Add(str5);
            foreach (string listItem in list)
            {
                if (caseInsensitive)
                {
                    if (strArray.Any(x => x.ToLower().StartsWith(listItem.ToLower()))) return true;
                }
                else
                {
                    if (strArray.Any(x => x.StartsWith(listItem))) return true;
                }
            }
            return false;
        }

        internal static bool StartsWithAny(this string str, string str1, string str2 = null, string str3 = null, string str4 = null, string str5 = null, bool caseInsensitive = false)
        {
            List<string> list = new();
            if (str1 != null) list.Add(str1);
            if (str2 != null) list.Add(str2);
            if (str3 != null) list.Add(str3);
            if (str4 != null) list.Add(str4);
            if (str5 != null) list.Add(str5);
            foreach (string listItem in list)
            {
                if (caseInsensitive)
                {
                    if (str.ToLower().StartsWith(listItem.ToLower())) return true;
                }
                else
                {
                    if (str.StartsWith(listItem)) return true;
                }
            }
            return false;
        }

        internal static bool StartsWithAny(this string str, IEnumerable<string> strArray, bool caseInsensitive = false)
        {
            foreach (string arrayItem in strArray)
            {
                if (caseInsensitive)
                {
                    if (str.ToLower().StartsWith(arrayItem.ToLower())) return true;
                }
                else
                {
                    if (str.StartsWith(arrayItem)) return true;
                }
            }
            return false;
        }

        internal static bool StartsWithXx(this string str, string str2, bool caseInsensitive = false)
        {
            return str.ToLower().StartsWith(str2.ToLower());
        }

        internal static bool ContainsOnly(this string str, IEnumerable<string> strArray)
        {
            foreach (char ch in str)
            {
                if (!strArray.Contains(ch.ToString())) return false;
            }
            return true;
        }

        internal static bool ContainsOnly(this string[] strArr, IEnumerable<string> strArray)
        {
            foreach (string str in strArr)
            {
                if (!strArray.Contains(str.ToString())) return false;
            }
            return true;
        }

        internal static bool ContainsAny(this string str, IEnumerable<string> strArray)
        {
            foreach (string arrayItem in strArray)
            {
                if (str.Contains(arrayItem))
                    return true;
            }
            return false;
        }

        internal static bool ContainsAnyXx(this string str, IEnumerable<string> strArray)
        {
            foreach (string arrayItem in strArray)
            {
                if (str.ContainsXx(arrayItem))
                    return true;
            }
            return false;
        }

        internal static bool ContainsAny(this string str, string str1, string str2 = null, string str3 = null, string str4 = null, string str5 = null, bool caseInsensitive = false)
        {
            if (str.IsEmpty()) return false;

            List<string> list = new();
            if (str1 != null) list.Add(str1);
            if (str2 != null) list.Add(str2);
            if (str3 != null) list.Add(str3);
            if (str4 != null) list.Add(str4);
            if (str5 != null) list.Add(str5);
            foreach (string listItem in list)
            {
                if (caseInsensitive)
                {
                    if (str.ToLower().Contains(listItem.ToLower())) return true;
                }
                else
                {
                    if (str.Contains(listItem)) return true;
                }
                
            }
            return false;
        }

        internal static bool ContainsAny(this IEnumerable<string> strArray, string str1, string str2 = null, string str3 = null, string str4 = null, string str5 = null, bool caseInsensitive = false)
        {
            List<string> list = new();
            if (str1 != null) list.Add(str1);
            if (str2 != null) list.Add(str2);
            if (str3 != null) list.Add(str3);
            if (str4 != null) list.Add(str4);
            if (str5 != null) list.Add(str5);
            foreach (string listItem in list)
            {
                if (caseInsensitive)
                {
                    if (strArray.Any(x => x.ToLower().Contains(listItem.ToLower()))) return true;
                }
                else
                {
                    if (strArray.Any(x => x.Contains(listItem))) return true;
                }
            }
            return false;
        }

        internal static bool ContainsAny(this IEnumerable<string> strArray, IEnumerable<string> strArray2)
        {
            List<string> list = new();
            list.AddRange(strArray2);
            foreach (string listItem in list)
            {
                if (strArray.Any(x => x.Contains(listItem)))
                    return true;
            }
            return false;
        }

        internal static bool ContainsAll(this IEnumerable<string> strArray, IEnumerable<string> strArray2, bool caseInsensitive = false)
        {
            foreach (string str2 in strArray2)
            {
                if (caseInsensitive)
                {
                    if (!strArray.ContainsXx(str2)) return false;
                }
                else
                {
                    if (!strArray.Contains(str2)) return false;
                }
            }
            return true;
        }

        internal static bool Contains(this IEnumerable<XElement> elementArray, XElement element)
        {
            foreach (XElement ElementArrayItem in elementArray)
            {
                if (XElement.DeepEquals(ElementArrayItem, element)) return true;
            }
            return false;
        }

        internal static bool IsSubDir(this string subDir, string parentDir)
        {
            return ((subDir.ToLower().TrimEnd('\\') + '\\').StartsWith(parentDir.ToLower().TrimEnd('\\') + '\\'));
        }

        internal static void AddIfNotExist(this List<string> strArray, string str)
        {
            if (strArray.Contains(str)) return;
            strArray.Add(str);
        }

        internal static void RemoveIfExist(this List<string> strArray, string str)
        {
            if (!strArray.Contains(str)) return;
            strArray.Remove(str);
        }

        internal static int IndexOf(this IEnumerable<string> strArr, string str)
        {
            return Array.IndexOf(strArr.ToArray(), str);
        }

        internal static bool EqualsIgnoreDiacritics(this string str1, string str2, bool caseInsensitive = true)
        {
            if (str1 == null) return str2 == null;

            if (caseInsensitive)
            {
                return String.Compare(str1, str2, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0;
            }
            else
            {
                return String.Compare(str1, str2, CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0;
            }
        }

        internal static string Escape(this string str)
        {
            try { return Uri.EscapeDataString(str); }
            catch { return ""; }
        }

        internal static void SaveToFile(this XDocument xdoc, string filePath)
        {
            XmlWriterSettings xws = new()
            {
                Encoding = new UTF8Encoding(false),
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n",
                OmitXmlDeclaration = false,
                Indent = true
            };
            using XmlWriter xmlWriter = XmlWriter.Create(filePath, xws);
            xdoc.Save(xmlWriter);    // UTF-8 no BOM (XDocument.Parse will fail if BOM present).
        }

        internal static string ToString(this XDocument xdoc)
        {
            XmlWriterSettings xws = new()
            {
                Encoding = new UTF8Encoding(false),  // UTF-8 no BOM (XDocument.Parse will fail if BOM present).
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n",
                OmitXmlDeclaration = false,
                Indent = true
            };
            using Utf8StringWriter sw = new();
            using (XmlWriter xmlWriter = XmlWriter.Create(sw, xws))
            {
                xdoc.Save(xmlWriter);
            }
            string str = sw.ToString();
            return sw.ToString();
        }

        internal static string[] GetDataLines(this string content)
        {
            return content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim(new[] { '\t', ' ' }).ToLower()).Where(x => !x.StartsWith("//")).ToArray();
        }

        internal static string[] GetCommentLines(this string content)
        {
            List<string> commentLines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim(new[] { '\t', ' ' })).Where(x => x.StartsWith("//")).ToList();
            // Add trailing blank line.
            commentLines.Add("");
            return commentLines.ToArray();
        }


        internal class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}
