using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedisRPC
{
    class InnerWorker<TIn,TOut>
    {
        private readonly ISubscriber _subscriber;
        private readonly RedisChannel _channel;
        private readonly CommandFlags _flags;


        public InnerWorker(ISubscriber subscriber, RedisChannel channel, CommandFlags flags)
        {
            _subscriber = subscriber;
            _channel = channel;
            _flags = flags;
        }

        public async Task<TOut> Call<T>(T param,int timeOut)
        {
            TaskWrapper wrapper = new TaskWrapper {Task = new Task(() => { })};
            var t = InnerCall(param, wrapper);
            IConnectionMultiplexer mp = _subscriber.Multiplexer;

            var server =mp.GetServer(mp.GetEndPoints()[0], new object());
            var b = await server.SubscriptionSubscriberCountAsync(_channel);
           //if (b == 0)
           //{
           //    throw new Exception("Исполнитель запроса не подключен");
           //}
           //
           //if (b > 1)
           //{
           //    throw new Exception($"Подключено несколько исполнителей запроса, не заню какой выбрать ({b})");
           //}

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
                    var tt = wrapper.ResultRedisValue.ToString();
                    return JsonConvert.DeserializeObject<TOut>(tt);
                }
                else
                {
                    wrapper.Action?.Invoke();
                   throw new Exception("Превышен лимит времени ожидания ответа от принтера");
                }
            }

        }

        private async Task InnerCall<T>( T param,TaskWrapper wrapper)
        {
            WrapperMessage<T> m = new WrapperMessage<T>(param, Utils.RandomString(12));
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
                    await _subscriber.PublishAsync(_channel, JsonConvert.SerializeObject(m));
                }
            }
        }
    }

    public class  WrapperMessage<TIn>
    {
        public readonly TIn Param;
        public readonly string Key;

        public WrapperMessage(TIn param, string key)
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
