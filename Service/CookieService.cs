using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Specialized;
using System.Text.Json;
using static System.Collections.Specialized.BitVector32;

public class CookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // JsonSerializerOptions를 클래스 필드로 선언
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        Converters = { new OrderedDictionaryConverter() } // 커스텀 컨버터 추가
    };

    public CookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // 쿠키 설정
    public bool SetCookie(string key, object value, TimeSpan? expireTime)
    {
        if (string.IsNullOrEmpty(key) || value == null)
        {
            return false;
        }
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // HTTPS 환경이 아니라면 false
            SameSite = SameSiteMode.Lax,
            Expires = expireTime.HasValue && expireTime.Value > TimeSpan.Zero
                ? DateTime.UtcNow.Add(expireTime.Value)
                : DateTime.UtcNow.AddDays(1) // 기본 만료 시간 1일
        };
        try
        {
            string serializedValue;

            if (value is string stringValue)
            {
                serializedValue = stringValue; // 문자열 그대로 사용
            }
            else if (value is int intValue)
            {
                serializedValue = intValue.ToString(); // 정수 -> 문자열
            }
            else if (value is List<string> listStringValue)
            {
                serializedValue = JsonSerializer.Serialize(listStringValue);
            }
            else
            {
                serializedValue = JsonSerializer.Serialize(value); // JSON으로 직렬화
            }

            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, serializedValue, cookieOptions);
            return true; // 성공
        }
        catch
        {
            return false; // 실패
        }
    }

    // 쿠키 가져오기
    public T GetCookie<T>(string key)
    {
        if (!_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(key, out string cookieValue))
        {
            return default;
        }
        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)cookieValue; // 문자열 그대로 반환
            }
            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(cookieValue, out int intValue))
                {
                    return (T)(object)intValue; // 문자열 -> 정수 변환
                }
            }
            return JsonSerializer.Deserialize<T>(cookieValue); // JSON 역직렬화
        }

        catch (JsonException jsonEx)
        {
            Console.WriteLine($"1-3 JSON 역직렬화 오류: {jsonEx.Message} (쿠키 키: '{key}', 값: '{cookieValue}')"); // JSON 관련 오류 로그
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"1-4 알 수 없는 오류: {ex.Message} (쿠키 키: '{key}', 값: '{cookieValue}')"); // 기타 오류 로그
            return default;
        }
    }

    // 쿠키 삭제
    public bool DeleteCookie(string key)
    {
        if (string.IsNullOrEmpty(key)) return false; // 유효하지 않은 키

        try
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(key);
            return true; // 성공
        }
        catch
        {
            return false; // 실패
        }
    }

    public bool SetCookieDictionary(string cookieName, int type, TimeSpan expireTime, int key, int? boardIdx = null)
    {
        // 1. 쿠키에서 데이터 가져오기
        var data = GetCookieDictionary(cookieName, 0) as OrderedDictionary ?? new OrderedDictionary();

        string keyString = key.ToString(); // int 키를 문자열로 변환

        boardIdx ??= -1; // 2. boardIdx가 null이면 -1로 설정

        if (type == 0) // Section 처리
        {
            if (boardIdx != -1)  return false;

            if (!data.Contains(keyString)) data.Add(keyString, new List<int>());
            else  MoveKeyToFront(data, keyString);

            return SetCookie(cookieName, data, expireTime);
        }
        else if (type == 1) // Board 처리
        {
            string boardIdxString = boardIdx.Value.ToString();


            if (!data.Contains(keyString))  return false; 

            MoveKeyToFront(data, keyString);

            var boards = data[keyString] as List<string>; // 값은 항상 List<string>로 처리
            if (boards == null)
            {
                boards = JsonSerializer.Deserialize<List<string>>(JsonSerializer.Serialize(data[keyString])) ?? new List<string>();
                data[keyString] = boards;
            }

            if (boards.Contains(boardIdxString)) boards.Remove(boardIdxString);

            boards.Insert(0, boardIdxString);

            return SetCookie(cookieName, data, expireTime);
        }

        return false; // 잘못된 type 처리
    }

    private void MoveKeyToFront(OrderedDictionary data, string key)
    {
        var currentIndex = data.Keys.Cast<string>().ToList().IndexOf(key); // 키를 문자열로 검색

        if (currentIndex == 0) return;

        var value = data[key]; // 기존 값을 저장
        data.Remove(key);      // 기존 키 삭제
        data.Insert(0, key, value); // 0번 위치에 다시 삽입
    }

    public object GetCookieDictionary(string cookieName, int type)
    {
        if (!_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(cookieName, out string serializedData))  return null;

        var data = JsonSerializer.Deserialize<OrderedDictionary>(serializedData, _jsonSerializerOptions);

        switch (type)
        {
            case 0: // 전체 OrderedDictionary 반환
                return data;

            case 1: // 0번째 Key 반환
                if (data.Count == 0) return null; // 데이터가 없으면 null 반환
                var firstKey = data.Keys.Cast<object>().First();
                return firstKey;

            case 2: // 모든 Key 반환
                var allKeys = data.Keys.Cast<object>().ToList();
                return allKeys;

            case 3: 
                if (data.Count == 0) return null;
                var firstKeytype3 = data.Keys.Cast<object>().First();
                var firstValue = data[firstKeytype3]; 
                return new { Key = firstKeytype3, Value = firstValue };

            default:
                throw new ArgumentException($"잘못된 type 값: {type}"); // 잘못된 type 처리
        }
    }

}
