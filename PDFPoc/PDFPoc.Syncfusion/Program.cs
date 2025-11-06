using System;
using System.Diagnostics;
using System.IO;
using Syncfusion.Pdf.Parsing;  // install via NuGet: Syncfusion.Pdf.Net.Core (or similar)

namespace PdfPageCountApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string basePath = @"C:\Users\rfaxas\Downloads\";
            string[] pdfFiles = { "200mb.pdf", "1_GB.pdf", "3GB.pdf" };

            foreach (string fileName in pdfFiles)
            {
                string pdfPath = Path.Combine(basePath, fileName);

                Console.WriteLine($"\n{'=',-50}");
                Console.WriteLine($"Processing: {fileName}");
                Console.WriteLine($"{'=',-50}");

                if (!System.IO.File.Exists(pdfPath))
                {
                    Console.WriteLine($"File not found: {pdfPath}");
                    continue;
                }

                try
                {
                    Console.WriteLine($"Reading PDF: {pdfPath}");

                    // Force garbage collection before measurement
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    long memoryBefore = GC.GetTotalMemory(false);
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    int pageCount = GetPageCount(pdfPath);

                    stopwatch.Stop();
                    long memoryAfter = GC.GetTotalMemory(false);
                    long memoryUsed = memoryAfter - memoryBefore;

                    Console.WriteLine($"Total pages: {pageCount}");
                    Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
                    Console.WriteLine($"Memory used: {FormatBytes(memoryUsed)}");
                    Console.WriteLine($"Total memory after operation: {FormatBytes(memoryAfter)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error counting pages: {ex.Message}");
                }
            }

            Console.WriteLine($"\n{'=',-50}");
            Console.WriteLine("Processing completed for all files.");
        }

        static int GetPageCount(string path)
        {
            // Load document in read-only mode
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(path))
            {
                return loadedDocument.Pages.Count;
            }
        }

        static string FormatBytes(long bytes)
        {
            if (bytes < 0) return "0 B";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}