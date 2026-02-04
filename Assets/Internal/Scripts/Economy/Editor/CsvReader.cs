using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Internal.Scripts.Economy.Editor
{
    public static class CsvReader
    {
        public static List<string[]> ReadFile(string path)
        {
            string content = File.ReadAllText(path, Encoding.UTF8);
            return Read(content);
        }

        public static List<string[]> Read(string content)
        {
            List<string[]> rows = new List<string[]>();
            List<string> currentRow = new List<string>();
            StringBuilder field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                    continue;
                }

                if (c == ',')
                {
                    currentRow.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    currentRow.Add(field.ToString());
                    field.Clear();
                    rows.Add(currentRow.ToArray());
                    currentRow.Clear();
                    continue;
                }

                field.Append(c);
            }

            if (inQuotes || field.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(field.ToString());
                rows.Add(currentRow.ToArray());
            }

            return rows;
        }
    }
}
