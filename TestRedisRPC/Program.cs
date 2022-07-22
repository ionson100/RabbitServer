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
            ISubscriber sub = con.GetSubscriber();
            try
            {
                await sub.PerformerRpcAsync("chanel:100:23", (channel, value) =>
                {
                    return MyMethod(value);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            try
            {  Console.WriteLine("Запрос на исполнение: Message");
                var res= await sub.CallerRcpExtAsync("chanel:100:23", "Message",1000);
                Console.WriteLine($"Ответ получен:{res}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.Read();

        }

        static string MyMethod(string message)
        {
            return $"{message}-OK";
        }
    }
}
