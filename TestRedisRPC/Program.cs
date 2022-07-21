using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisRPC;
using StackExchange.Redis;

namespace TestRedisRPC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Task<ConnectionMultiplexer> redis = ConnectionMultiplexer.ConnectAsync(
                new ConfigurationOptions //host.docker.internal
                {
                    
                    EndPoints = { $"localhost:6379" },
                    AbortOnConnectFail = false,
                });
            var con=  redis.Result;
            var connectionCounters = con.GetCounters().Subscription;
            ISubscriber sub = con.GetSubscriber();
           var res= await sub.CallerRcpExtAsync<string,string>("asas", "asas");
           int dd = 1;

        }
    }
}
