using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisRPC
{
    public static class ExtensionSubscriber
    {
        public static void PerformerRpc(this ISubscriber subscriber,RedisChannel channel, Action<RedisChannel, RedisValue> handler,
            CommandFlags flags = CommandFlags.None)
        {

        }
        public static async Task<TOut> CallerRcpExtAsync<TIn,TOut>(this ISubscriber subscriber, RedisChannel channel,TIn param,int timeOut=1000, 
            CommandFlags flags = CommandFlags.None)
        {
            InnerWorker<TIn,TOut> innerWorker=new InnerWorker<TIn,TOut>(subscriber,channel,flags);
            try
            { 
                var dd= await innerWorker.Call(param,timeOut);
                return dd;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new TaskCanceledException(e.Message,e);

            }
           
           // dd.Wait();
            //return new Task<TOut>(() => default);
            throw new Exception();
        }

    }
}
