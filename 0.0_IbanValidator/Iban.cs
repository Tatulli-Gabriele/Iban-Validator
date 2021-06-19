using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IbanValidator
{
    sealed class Iban
    {
        public static string Status { get; set; }
        public static string Address { get; set; }
        public static string[] Results { get; set; }
        public Iban(string status, string address, string[] results) {
            Status = status;
            Address = address;
            Results = results;
        }

        public static sealed class Regex
        {
            public static Regex Results = new Regex(
                @"<td valign=""top""><img src=""data:image\/gif;base64,(?:[A-Za-z0-9+\/]{4})*(?:[A-Za-z0-9+\/]{2}==|[A-Za-z0-9+\/]{3}=)"" alt=""\+""><\/td><td><p>([a-zA-Z0-9_.\- :()]*)"
            );

            public static Regex Address = new Regex(
                @"^.*([A-Z]{2}[ \-]?[0-9]{2}(?=(?:[ \-]?[A-Z0-9]){9,30})(?:[ \-]?[A-Z0-9]{3,9}){2,7}[ \-]?[A-Z0-9]{1,3}?).*$"
            );

            public static Regex Status = new Regex(
                @"<fieldset><legend>Result<\/legend><p><b>(.*)<\/b><\/p>"
            );
        }

        public static async string ParseAsync(Iban iban)
        {
            var jsonResult = string.Empty;
            var HoneyPot = new HoneyPot(iban);
            
            try
            {
                jsonResult = JsonConvert.SerializeObject(
                    HoneyPot,
                    Converter.Settings
                );
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(
                    ex.Message
                );
            }

            return jsonResult;
        }

        static async Task ScrapeAsync(List<String> ibans)
        {
            using (var client = new HttpClient()) {
                foreach (var iban in ibans) {
                    var scrapedData = String.Empty;
                    
                    try {
                        scrapedData = await client.GetStringAsync(
                            _baseUrl + iban
                        );
                    }
                    catch {
                        await Console.Error.WriteLineAsync(
                            "Request Failed, Trying again." + Environment.NewLine +
                            "Config: { Attempts: 20, Cooldown: 5000 }"
                        );

                        for(var i = 0; i < 20; i++) {
                            if (!string.IsNullOrEmpty(scrapedData)) {
                                try {
                                    scrapedData = await client.GetStringAsync(
                                        _baseUrl + iban
                                    );
                                }
                                catch {}
                                Thread.Sleep(5000);
                            }
                        }
                    }
                    
                    if(Iban.Regex.Status.IsMatch(scrapedData)) {
                        var status = Iban.Regex.Status.Match(scrapedData).Groups[1].Value;
                        var matches = Iban.Regex.Results.Matches(scrapedData);
                        var json = await ParseAsync(new Iban(
                                results: () => {
                                    for (int i = 0; i<=3; i++) {
                                        yield return matches[i].Groups[1].Value;
                                    }
                                },
                                status: status,
                                iban: iban
                            )
                        );

                        if (!File.Exists(outputdir))
                            await File.WriteAllTextAsync(
                                outputdir,
                                json
                            );
                        else
                            await File.WriteAllTextAsync(
                                outputdir, await File.ReadAllTextAsync(outputdir)
                                    .Status.Replace("}}", "},\n") + json.Replace(
                                        "{\"statusClass\":",
                                        "\"statusClass\":"
                                    )
                                );
                    }
                    Thread.Sleep(1500);
                }
            }
        }

        static async List<String> FetchAsync(string filePath)
        {
            var ibans = new List<String>();
            var data = await File.ReadAllLinesAsync(filePath);

            foreach(var ln in data)
                if(_addressRegex.IsMatch(ln))
                    ibans.Add(_addressRegex.Match(ln)
                            .Groups[1]
                            .Value
                        );

            return ibans;
        }
    }
}