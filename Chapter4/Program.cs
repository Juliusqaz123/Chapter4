﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Chapter4
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"c:\temp\test.txt";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            FileInfo fileInfo = new FileInfo(path);

            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            Console.ReadLine();
        }

        private static void ListDirectories(DirectoryInfo directoryInfo,
            string searchPattern, int maxLevel, int currentLevel)
        {
            if (currentLevel >= maxLevel)
            {
                return;
            }

            string indent = new string('-', currentLevel);

            try
            {
                DirectoryInfo[] subDirectories = directoryInfo.GetDirectories(searchPattern);

                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    Console.WriteLine(indent + subDirectory.Name);
                    ListDirectories(subDirectory, searchPattern, maxLevel, currentLevel + 1);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // You don't have access to his folder.
                Console.WriteLine(indent + "Can't access: " + directoryInfo.Name);
            }
            catch (DirectoryNotFoundException)
            {
                // The folder is removed while iterating
                Console.WriteLine(indent + "Can't find: " + directoryInfo.Name);
                return;
            }
        }
    }
}
