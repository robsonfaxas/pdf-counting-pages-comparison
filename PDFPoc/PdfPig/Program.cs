using System;
using System.Diagnostics;
using System.IO;
using UglyToad.PdfPig;  // install via NuGet: PdfPig
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Exceptions;

namespace PdfPigPageCounter
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
                    Console.WriteLine("File not found: " + pdfPath);
                    continue;
                }

                // Basic file validation
                if (!ValidatePdfFile(pdfPath))
                {
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
                catch (PdfDocumentFormatException ex)
                {
                    Console.WriteLine($"Error: The file is not a valid PDF - {ex.Message}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"I/O error while reading the file - {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error counting pages: {ex.Message}");
                    Console.WriteLine($"Error type: {ex.GetType().Name}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner error: {ex.InnerException.Message}");
                    }
                }
            }

            Console.WriteLine($"\n{'=',-50}");
            Console.WriteLine("Processing completed for all files.");
        }

        static bool ValidatePdfFile(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                Console.WriteLine($"File size: {FormatBytes(fileInfo.Length)}");

                // Check if the file is not empty
                if (fileInfo.Length == 0)
                {
                    Console.WriteLine("Error: The PDF file is empty.");
                    return false;
                }

                // Check the first bytes to confirm it's a PDF
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[5];
                    int bytesRead = fileStream.Read(header, 0, 5);

                    if (bytesRead < 5)
                    {
                        Console.WriteLine("Error: File too small to be a valid PDF.");
                        return false;
                    }

                    string headerString = System.Text.Encoding.ASCII.GetString(header);
                    if (!headerString.StartsWith("%PDF-"))
                    {
                        Console.WriteLine($"Error: File does not have a valid PDF header. Found: {headerString}");
                        return false;
                    }

                    Console.WriteLine($"Valid PDF header found: {headerString}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating file: {ex.Message}");
                return false;
            }
        }

        static int GetPageCount(string path)
        {
            try
            {
                using (PdfDocument document = PdfDocument.Open(path))
                {
                    Console.WriteLine($"PDF Version: {document.Version}");
                    return document.NumberOfPages;
                }
            }
            catch (Exception ex)
            {
                // Re-throw with more context
                throw new Exception($"Failed to read PDF with PdfPig: {ex.Message}", ex);
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