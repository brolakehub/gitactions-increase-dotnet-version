namespace IncreaseDotNetProjectVersion
{
    internal class Program
    {
        private static readonly string[] SupportedChanges = new string[] { "major", "minor", "patch", "prepatch" };

        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine($"Expected 2 arguments, but got {args.Length}");
                return -1;
            }

            if (!SupportedChanges.Contains(args[0].ToLower()))
            {
                Console.Error.WriteLine("Expected as first argument, to be one of the following: major | minor | patch | prepatch");
                return -1;
            }

            if (!File.Exists(args[1]))
            {
                Console.Error.WriteLine($"Could not find the csproj file at location {args[1]}");
                return -1;
            }

            if (args[1].Split('.').Last() != "csproj")
            {
                Console.Error.WriteLine("Expected .csproj file as second argument.");
                return -1;
            }

            string? tempLine;
            int[] projVersion = new int[] { 0, 0, 0, 0 };
            using (FileStream inputStream = File.OpenRead(args[1]))
            {
                using (StreamReader reader = new StreamReader(inputStream))
                {
                    using (StreamWriter writer = File.AppendText(args[1] + ".new"))
                    {
                        while ((tempLine = reader.ReadLine()) != null)
                        {
                            if (tempLine.Contains("<Version>"))
                            {
                                string[] entireLine = tempLine.Split("<Version>");
                                string textVersion = entireLine[1].Split("</Version>")[0];
                                foreach (var item in textVersion.Split('.').Select((value, index) => new {value, index}))
                                {
                                    if (item.index != 2)
                                    {
                                        projVersion[item.index] = Convert.ToInt32(item.value);
                                    }
                                    else
                                    {
                                        projVersion[item.index] = Convert.ToInt32(item.value.Split('-')[0]);
                                    }
                                }

                                switch (args[0])
                                {
                                    case "major":
                                        projVersion[0] += 1;
                                        projVersion[1] = 0;
                                        projVersion[2] = 0;
                                        projVersion[3] = 0;
                                        break;
                                    case "minor":
                                        projVersion[1] += 1;
                                        projVersion[2] = 0;
                                        projVersion[3] = 0;
                                        break;
                                    case "patch":
                                        projVersion[2] += 1;
                                        projVersion[3] = 0;
                                        break;
                                    case "prepatch":
                                        projVersion[3] += 1;
                                        break;
                                }

                                writer.Write(entireLine[0] + $"<Version>{projVersion[0]}.{projVersion[1]}.{projVersion[2]}");
                                if (projVersion[3] != 0)
                                {
                                    writer.Write($"-next.{projVersion[3]}");
                                }
                                writer.WriteLine("</Version>");
                            }
                            else
                            {
                                writer.WriteLine(tempLine);
                            }
                        }
                    }
                }
            }
            File.Delete(args[1]);
            File.Move(args[1] + ".new", args[1]);
            return 0;
        }
    }
}