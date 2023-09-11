using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neal.Reddit.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverterWithAttributeSupport))]
public enum MonitorTypes
{
    [EnumMember(Value = "")]
    Unknown,

    [EnumMember(Value = "None")]
    None,

    [EnumMember(Value = "AfterStartOnly")]
    AfterStartOnly,

    [EnumMember(Value = "All")]
    All,
}
