using System.Collections;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;

// OrderedDictionary를 JSON으로 직렬화/역직렬화하기 위한 커스텀 컨버터 클래스
public class OrderedDictionaryConverter : JsonConverter<OrderedDictionary>
{
    // JSON -> OrderedDictionary 역직렬화 (JSON 문자열을 OrderedDictionary 객체로 변환)
    public override OrderedDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 새 OrderedDictionary 생성
        var dictionary = new OrderedDictionary();

        // JSON 데이터를 Dictionary<string, object>로 역직렬화
        var jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);

        // Dictionary<string, object> 데이터를 OrderedDictionary로 변환
        foreach (var kvp in jsonDictionary)
        {
            dictionary.Add(kvp.Key, kvp.Value); // Key와 Value를 OrderedDictionary에 추가
        }

        return dictionary; // 완성된 OrderedDictionary 반환
    }

    // OrderedDictionary -> JSON 직렬화 (OrderedDictionary 객체를 JSON 문자열로 변환)
    public override void Write(Utf8JsonWriter writer, OrderedDictionary value, JsonSerializerOptions options)
    {
        // OrderedDictionary 데이터를 Dictionary<string, object>로 변환
        var dictionary = new Dictionary<string, object>();

        foreach (DictionaryEntry entry in value)
        {
            // OrderedDictionary의 Key와 Value를 Dictionary<string, object>로 복사
            dictionary.Add(entry.Key.ToString(), entry.Value);
        }

        // Dictionary<string, object>를 JSON으로 직렬화
        JsonSerializer.Serialize(writer, dictionary, options);
    }
}
