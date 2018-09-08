namespace STRHM.Interfaces
{
    public interface IStronglyTypedRedisSerializer
    {
        string Serialize(object obj);
        string Serialize(object obj, string dateTimeFormat);
        T Deserialize<T>(string value);
        T Deserialize<T>(string value, string dateTimeFormat);
    }
}
