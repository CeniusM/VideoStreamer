

namespace VidoeStreaming
{
    internal class QuickFixes
    {
        /// <summary>
        /// When using ASCII_bad_apple the split lines have 2 separate line on it
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static List<string> FixDoubleLine(List<string> file)
        {
            int Hundredth = file.Count / 100;
            int NormalLen = file[0].Length;

            List<string> lines = new List<string>();
            for (int i = 0; i < file.Count; i++)
            {
                //if (!file.Contains("SPIT"))
                //    lines.Add(file[i]);
                //else
                //{
                //    lines.AddRange(file[i].Split("SPLIT"));
                //}

                if (file[i].Length == NormalLen)
                    lines.Add(file[i]);
                else
                {
                    string[] foo = file[i].Split("SPLIT");
                    foo[0] += '\0';
                    lines.AddRange(foo);
                }



                if (i % Hundredth == 0)
                    Console.WriteLine(i / Hundredth + "/100");
            }
            return lines;
        }
    }
}
