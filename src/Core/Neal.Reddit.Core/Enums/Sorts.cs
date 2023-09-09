using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neal.Reddit.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverterWithAttributeSupport))]
public enum Sorts
{
    [EnumMember(Value = "")]
    Unknown,

    [EnumMember(Value = "default")]
    Default,

    [EnumMember(Value = "gold")]
    Gold,

    [EnumMember(Value = "new")]
    New,

    [EnumMember(Value = "popular")]
    Popular
}
