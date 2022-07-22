using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisRPC
{
    class InnerPerformerWorker
    {
        public async Task Perform(ISubscriber subscriber, RedisChannel channel,
            Func<RedisChannel, RedisValue,string> handler)
        {
            await subscriber.SubscribeAsync(channel, async (channelE, message) =>
            {
                try
                {
                    string sd = message.ToString();
                    var m = JsonConvert.DeserializeObject<WrapperMessage>(message);
                    string  res=  handler.Invoke(channel,m.Param);
                    await subscriber.PublishAsync($"{channel}:{m.Key}", res);
                }
                catch (Exception e)
                {
                    throw new InnerExceptionRpc($"Произошло при сполениие процедуры: {e.Message}");
                }
              
            });
        }
    }
}
