//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using RedisTr.Controllers;
//using RedisTr.models;
//using RedisTr.utils;
//using StackExchange.Redis;
//
//namespace RedisTr.redis
//{
//    public class RedisWorker
//    {
//        private readonly IRedisConnectionE _redisConnectionE;
//        private readonly MDataIn _mDataIn;
//        private readonly MDataOut _mDataOut;
//        private readonly string _rabBody;
//        public static string MyHost = "host.docker.internal";
//
//        public RedisWorker(IRedisConnectionE redisConnectionE, MDataIn mDataIn, MDataOut mDataOut, string rabBody)
//        {
//            _redisConnectionE = redisConnectionE;
//            _mDataIn = mDataIn;
//            _mDataOut = mDataOut;
//            _rabBody = rabBody;
//        }
//        public async Task Run(TaskWrapper wrapper)
//        {
//            ISubscriber sub = _redisConnectionE.GConnectionMultiplexer().GetSubscriber();
//            WrapperMessage m = new WrapperMessage { Param = _rabBody, Key = utils.Utils.RandomString(12) };
//            try
//            {
//                await sub.SubscribeAsync($"{Utils.ChanelName(_mDataIn)}:{m.Key}", (channel, message) =>
//                {
//                    sub.UnsubscribeAsync(channel);
//                    _mDataOut.SetErrorData(ErrorCode.PrintError, message);
//                    _mDataOut.InnerRunner.IsError = false;
//                  
//                    wrapper.Task.Start();
//
//                });
//                wrapper.Action = () => { sub.UnsubscribeAsync(Utils.ChanelName(_mDataIn)); };
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                wrapper.Exception = e;
//            }
//            finally
//            {
//                if (wrapper.Exception == null)
//                { 
//                    await sub.PublishAsync(Utils.ChanelName(_mDataIn), JsonConvert.SerializeObject(m));
//                }
//            }
//            
//
//        }
//    }
//
//    public class WrapperMessage
//    {
//        public string Key { get; set; }
//        public string Param { get; set; }
//
//    }
//}
//