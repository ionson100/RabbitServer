using System.Threading.Tasks;
using RedisTr.utils;
using StackExchange.Redis;

namespace RedisTr.redis
{
    public class RedisConnectionSinglton: IRedisConnectionE
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly IServer _server=null;

        public RedisConnectionSinglton()
        {
            Task<ConnectionMultiplexer> redis = ConnectionMultiplexer.ConnectAsync(
                new ConfigurationOptions //host.docker.internal
                {
                    Password = Auth.Pwd,
                    EndPoints = {$"{Auth.Host}:6379"},
                    AbortOnConnectFail = false,
                });
            _connectionMultiplexer = redis.Result;
            _server = _connectionMultiplexer.GetServer($"{Auth.Host}:6379");
        }

        public ConnectionMultiplexer GConnectionMultiplexer()
        {
            return _connectionMultiplexer;
        }

        public IServer GetServer()
        {
            return _server;
        }
    }

    public interface IRedisConnectionE
    {
        ConnectionMultiplexer GConnectionMultiplexer();
        IServer GetServer();
    }
}
