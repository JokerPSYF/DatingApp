using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Converters
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string DateFormat = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                DateOnly.ParseExact(reader.GetString()!, DateFormat, CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            //writer.WriteValue(value.ToString(TimeFormat, CultureInfo.InvariantCulture));
            writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
    }

}
