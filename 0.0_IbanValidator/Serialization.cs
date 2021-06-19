using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IbanValidator
{
    public sealed partial class HoneyPot
    {
        public Result resultClass { get; set; }
        public HoneyPot(Iban iban) {
            resultClass = new Result(iban);
        }
    }

    public sealed partial class Result
    {
        public static string Result { get; set; }
        public static string Address { get; set; }
        public static string[] Results { get; set; }
        public ResultClass(Iban iban) {
            Result = iban.Status;
            Address = iban.Address;
            Checks = iban.Results;
        }
    }

    internal sealed static class Converter
    {
        public sealed static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                    new IsoDateTimeConverter {
                        DateTimeStyles = DateTimeStyles.AssumeUniversal
                    }
                },
        };
    }
}