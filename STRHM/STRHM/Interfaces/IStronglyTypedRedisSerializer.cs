namespace STRHM.Interfaces
{
    public interface IStronglyTypedRedisSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string value);
    }
}
