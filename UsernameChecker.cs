    class usernameHandler
    {
    static IDictionary<string, string> url = new Dictionary<string, string>()
        {
            {"1", "https://github.com/"},
            {"2", "https://www.snapchat.com/add/"},
            {"3", "https://leetcode.com/"},
            {"4", "https://www.codewars.com/users/"},
            {"5", "https://www.youtube.com/@" },
            {"6", "https://steamcommunity.com/id" },
            {"7", "https://vimeo.com/" },
            {"8", "https://www.wattpad.com/user/" },
            {"9", "https://soundcloud.com/" },
            {"10", "https://giphy.com/" }
        };

        static IDictionary<string, string> platform = new Dictionary<string, string>()
        {
            {"1", "Github"},
            {"2", "Snapchat"},
            {"3", "Leetcode"},
            {"4", "Codewars"},
            {"5", "YouTube" },
            {"6", "SteamID" },
            {"7", "Vimeo" },
            {"8", "Wattpad" },
            {"9", "Soundcloud" },
            {"10", "Giphy" }
        };

        static readonly HttpClient client = new HttpClient();
        
        static async Task check(String type)
        {

            Console.WriteLine("Would you like to create an output file? (Y/N)");

            bool output = Console.ReadLine().ToLower() == "y";

            string path = "username.txt";

            bool exists = File.Exists(path);

            if (!exists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("usernames.txt does not exist!");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("Would you like to create a template? (Y/N)");
                String template = Console.ReadLine();

                if(template.ToLower().Equals("y"))
                {
                    File.WriteAllText(path, "a\nb\nc");

                    if (File.Exists(path))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Template created!");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error creating template");
                    }

                }
                else
                {
                    Console.WriteLine("Exiting...");
                    Environment.Exit(0);

                }
            }

            var lines = File.ReadAllLines("username.txt");
            foreach (string line in lines)
            {

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Checking " + platform[type] + " username " + line + "...");
                    Console.ForegroundColor = ConsoleColor.White;

                    String final_url = url[type] + line;

                    var response = await client.GetAsync(final_url);
                    var resCode = response.StatusCode;

                    int res = (int)resCode;

                    if(res == 429)
                    {
                        while(res == 429)
                        {
                            Console.WriteLine("Rate limitation detected, sleeping for 10 seconds");
                            Thread.Sleep(10000);
                             response = await client.GetAsync(final_url);
                             resCode = response.StatusCode;
                             res = (int)resCode;
                        }
                    }
                    else if (res == 404)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(platform[type] + " username: " + line + " is not taken!");
                        if (output)
                        {
                            File.AppendAllText(platform[type] + ".txt", line + " -> Not Taken \n");
                            Console.WriteLine("Saved to " + platform[type] + ".txt");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (res == 200)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(platform[type] + " username: " + line + " is taken!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: " + resCode);
                        Console.ForegroundColor = ConsoleColor.White;
                    }




        }
            Console.WriteLine("would you like to check another platform? (Y/N)");
        }

        public void start()
        {
            Console.WriteLine("Select the Platform you want to check names for");
            Console.WriteLine("1. GitHub");
            Console.WriteLine("2. Snapchat");
            Console.WriteLine("3. Leetcode");
            Console.WriteLine("4. Codewars");
            Console.WriteLine("5. YouTube");
            Console.WriteLine("6. SteamID");
            Console.WriteLine("7. Vimeo");
            Console.WriteLine("8. Wattpad");
            Console.WriteLine("9. Soundcloud");
            Console.WriteLine("10. Giphy");
        
            String type = Console.ReadLine();

            if (type == "1" || type == "2" || type == "3" || type == "4" || type == "5" || type == "6" || type == "7" || type == "8" || type == "9" || type == "10")
            {
                check(type).Wait();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input!");
                Console.ForegroundColor = ConsoleColor.White;
                start();
            }

        }
    }
    
    class Program
    {
        
        static void  Main(string[] args)
        {
            usernameHandler usr = new usernameHandler();

            Console.WriteLine("--------------------------");
            Console.WriteLine("- Multi Username Checker -");
            Console.WriteLine("--------------------------");
            Console.WriteLine("");
            Console.WriteLine("Instructions:");
            Console.WriteLine("Placer a file called username.txt into the same directory as this file");
            Console.WriteLine("Put in all the usernames you want to check -> each username in a seperate line");
            Console.WriteLine("");


            while (true)
            {
                usr.start();
                String check = Console.ReadLine();
                if (check.ToLower().Equals("n"))
                {
                    break;
                }
            }
        }
        
    }
