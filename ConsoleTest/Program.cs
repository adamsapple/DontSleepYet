using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var counter_WorkingSet = new PerformanceCounter("Process", "Working Set", "firefox");

            var counter_PageFaults = new PerformanceCounter("Process", "Page Faults/sec", "firefox");

            var myCounterProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var myCounterMemory = new PerformanceCounter("Memory", "Available MBytes", string.Empty);
            var myCounterPagingFile = new PerformanceCounter("Paging File", "% Usage", "_Total");
            var myCounterMemoryPage = new PerformanceCounter("Memory", "Pages/Sec", string.Empty);

            while (true)

            {

                Console.WriteLine("----------------");

                Console.WriteLine("    Working Set : " + counter_WorkingSet.NextValue() / 1024 / 1024 / 1024);

                Console.WriteLine("Page Faults/sec : " + counter_PageFaults.NextValue());

                Console.WriteLine("% Processor Time: " + myCounterProcessor.NextValue());
                Console.WriteLine("% Available GBytes: " + Math.Round(myCounterMemory.NextValue() / 1024, 1));
                Console.WriteLine("% Paging File: " + myCounterPagingFile.NextValue());
                Console.WriteLine("myCounterMemoryPage: " + myCounterMemoryPage.NextValue());

                Thread.Sleep(1000);
            }
        }
    }
}
