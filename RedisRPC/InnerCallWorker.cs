using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisRPC
{
    class InnerCallWorker
    {
        private readonly ISubscriber _subscriber;
        private readonly RedisChannel _channel;
        private readonly CommandFlags _flags;


        public InnerCallWorker(ISubscriber subscriber, RedisChannel channel, CommandFlags flags)
        {
            _subscriber = subscriber;
            _channel = channel;
            _flags = flags;
        }

        public async Task<string> Call(string param,int timeOut)
        {
            TaskWrapper wrapper = new TaskWrapper {Task = new Task(() => { })};
          
            IConnectionMultiplexer mp = _subscriber.Multiplexer;

            var ee = InnerCall(param, wrapper);
            ee.Wait();
            if (wrapper.Exception != null)
            {
                wrapper.Action?.Invoke();
                throw wrapper.Exception;
            }
            else
            {
                if (wrapper.Task.Wait(timeOut))
                {
                    var tt = wrapper.ResultRedisValue;
                    return tt;
                }
                else
                {
                    wrapper.Action?.Invoke();
                   throw new InnerExceptionRpc("Превышен лимит времени ожидания ответа от принтера");
                }
            }

        }

        private async Task InnerCall( string param,TaskWrapper wrapper)
        {
            WrapperMessage m = new WrapperMessage(param, Utils.RandomString(12));
            try
            {
                await _subscriber.SubscribeAsync($"{_channel}:{m.Key}", (channel, message) =>
                {
                    _subscriber.UnsubscribeAsync(channel);
                    wrapper.ResultRedisValue = message;
                    wrapper.Task.Start();

                });
                wrapper.Action = () => { _subscriber.UnsubscribeAsync(_channel); };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                wrapper.Exception = e;
            }
            finally
            {
                if (wrapper.Exception == null)
                {
                    await _subscriber.PublishAsync(_channel, JsonConvert.SerializeObject(m,Formatting.None));
                }
            }
        }
    }

    public class  WrapperMessage
    {
        public readonly string Param;
        public readonly string Key;

        public WrapperMessage(string param, string key)
        {
            Param = param;
            Key = key;
        }
    }
    public class TaskWrapper
    {
        public Task Task { get; set; } = new Task(() => { });
        public Action Action { get; set; }
        public Exception Exception { get; set; }
        public RedisValue ResultRedisValue { get; set; }
    }
}
