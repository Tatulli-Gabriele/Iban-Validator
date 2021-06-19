using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IbanValidator
{
    sealed class Program
    {
        static private String _baseUrl = new String(
            "https://www.ibancalculator.com/iban_validieren.html?&tx_valIBAN_pi1%5Bfi%5D=fi&tx_valIBAN_pi1%5Biban%5D="    
        );

        static private String _outputPath = String.Empty;

        static internal async Task Main() => RunApp();

        static async Task RunApp()
        {
            Console.Write("Folder/File path: ");
            var path = Console.ReadLine();

            Console.Write("Output file path: ");
            _outputPath = Console.ReadLine();

            var list = new List<string>();

            if (Directory.Exists(path)) {
                Console.Write("Use recursive search? ([y/n]): ");
                var r = Console.ReadLine();

                if (r.Contains("y"))
                    foreach (string file in Files.FetchRecursively(path))
                        list.AddRange(await Iban.FetchAsync(file));
                else
                    foreach(string file in Directory.GetFiles(path))
                        list.AddRange(await Iban.FetchAsync(file));

                await Iban.ScrapeAsync(list);
            }
            else if (File.Exists(path)) {
                list.AddRange(await Iban.FetchAsync(path));
                await Iban.ScrapeAsync(list);
            }
            else {
                await Console.Error.WriteLineAsync(
                    "Wrong folder or file path, press any key and try again..."
                );
                Console.ReadKey();
            }
        }
    }
}