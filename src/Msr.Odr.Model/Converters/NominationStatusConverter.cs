// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;

namespace Msr.Odr.Model.Converters
{
    public class NominationStatusConverter : JsonConverter
    {
        private const NominationStatus DefaultStatus = NominationStatus.PendingApproval;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            NominationStatus status = (NominationStatus?) value ?? DefaultStatus;
            writer.WriteValue(status.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return DefaultStatus;
            }

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing enum.");
            }

            string enumText = reader.Value.ToString().Replace(" ", string.Empty);
            if (!Enum.TryParse(enumText, true, out NominationStatus status))
            {
                throw new JsonSerializationException($"Error converting value {enumText} to type 'NominationStatus'.");
            }

            return status;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}
