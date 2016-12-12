using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Threading;

namespace FileWatcher
{
    public class Watcher
    {
        public static string[] args;
        public static FileSystemWatcher watcher;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            args = System.Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program.
            if (args.Length != 4)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: Watcher.exe [watching_directory] [backup_directory] [send_directory]");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = args[1];
            watcher.IncludeSubdirectories = false;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
              // Only watch text files.
            watcher.Filter = "*.csv";

            // Add event handlers.
        //    watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
           // watcher.Deleted += new FileSystemEventHandler(OnChanged);
         //   watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            Console.WriteLine("Press \'q\' to quit the programm.");
            while (Console.Read() != 'q') ;
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
      
            bool flag_d = true;
            int tries = 0;

            do
            {
                 try
                 {
                      if (tries == 60) 
                      {
                           Console.WriteLine("Can't handle a source file {0}. File is in use by another process", e.FullPath);
                           break;
                      }
                      Thread.Sleep(1000); //Wait some time
                      // Specify what is done when a file is changed, created, or deleted.
                      Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            
                      string old_file_path = args[2]; //path for backup a source file
                      string new_file_path = args[3]; //path for new converted file
                      string full_path_copy = old_file_path + e.Name;
                      // Copy a source file to backup directory.
                 
                      File.Copy(e.FullPath, full_path_copy, true);
                      Console.WriteLine("Copy {0} to path {1}", e.FullPath, full_path_copy);
                      
                      //Create a copy of a source file with another code page.
                      Encoding srcEncodingFormat = Encoding.UTF8;
                      Encoding dstEncodingFormat = Encoding.GetEncoding("windows-1251");
                      byte[] originalByteString = File.ReadAllBytes(e.FullPath);
                      byte[] convertedByteString = Encoding.Convert(srcEncodingFormat, dstEncodingFormat, originalByteString);

                      // string full_path_copy = old_file_path + f_name;

                      using (FileStream fs = File.Create(Path.Combine(new_file_path, e.Name)))
                      {
                           fs.Write(convertedByteString, 0, convertedByteString.Length);
                           fs.Close();
                      }
                      //Delete a source file from directory
                    
                      File.Delete(e.FullPath);
                      Console.WriteLine("Delete file {0}", e.FullPath);
                      flag_d = false;
                 }

                 catch (IOException ex_d)
                 {
                      Console.WriteLine("IOException ex_d: {0}", ex_d.Message);
                      flag_d = true;
                 }

                 tries++;

            } while (flag_d); 
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }


    class Program
    {

        static void Main(string[] args)
        {
            Watcher.Run();
            //   Console.WriteLine("Press \'q\' to quit the sample.");
            //   while (Console.Read() != 'q') ;             
            // Keep the console window open in debug mode.
            //            Console.WriteLine("Press any key to exit.");
            //    System.Console.ReadKey();
        }
    }
}
