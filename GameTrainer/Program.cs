using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Memory;

public static class Game
{
    public static string skills = "Borderlands2.exe + 0x0165DE08,194,14,14,274";
    public static string token  = "Borderlands2.exe + 0x0169F868,34,308,3C8,298,2F0";
}

class Trainer
{
    
    [DllImport("user32.dll")]
    public static extern int GetAsyncKeyState(Keys vKeys);

    public static Mem memory = new Mem();
    public static int procID;

    public static void init()
    {
        Console.WriteLine("Waiting for game to start...");

        // get process id from process name
        int processID = memory.GetProcIdFromName("Borderlands2");

        // while proc id is not 0 get process id again
        while (processID == 0)
        {
            processID = memory.GetProcIdFromName("Borderlands2");
            Thread.Sleep(1000);
        }

        Console.WriteLine("Game process found!");

        procID = processID;

    }

    static void Main(string[] args)
    {

        init();

        // open process
        memory.OpenProcess(procID);

        // check if spacebar is pressed 

        while (true)
        {

            // unlimited skills
            memory.WriteMemory(Game.skills, "int", "50");

            // unlimited Torgue token 
            memory.WriteMemory(Game.token, "int", "999");

            Thread.Sleep(100);

        }

    }
}

