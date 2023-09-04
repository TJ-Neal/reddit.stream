using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Neal.Reddit.Core.Entities.Reddit;

[JsonConverter(typeof(JsonStringEnumConverterWithAttributeSupport))]
public enum Kind
{
    [EnumMember(Value = "")]
    Unknown,

    [EnumMember(Value = "t1")]
    Comment,

    [EnumMember(Value = "t2")]
    Account,

    [EnumMember(Value = "t3")]
    Link,

    [EnumMember(Value = "t4")]
    Message,

    [EnumMember(Value = "t5")]
    Subreddit,

    [EnumMember(Value = "t6")]
    Award,

    Listing
}
