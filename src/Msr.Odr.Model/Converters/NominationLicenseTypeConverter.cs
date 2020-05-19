using System;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;

namespace Msr.Odr.Model.Converters
{
    public class NominationLicenseTypeConverter : JsonConverter
    {
        private const NominationLicenseType DefaultType = NominationLicenseType.Standard;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            NominationLicenseType licenseType = (NominationLicenseType?)value ?? DefaultType;
            writer.WriteValue(licenseType.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return DefaultType;
            }

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing enum.");
            }

            string enumText = reader.Value.ToString();
            if (!Enum.TryParse(enumText, true, out NominationLicenseType licenseType))
            {
                throw new JsonSerializationException($"Error converting value {enumText} to type 'NominationLicenseType'.");
            }

            return licenseType;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}
