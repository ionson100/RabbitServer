using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RedisTr.models;
using RedisTr.redis;
using RedisTr.utils;

namespace RedisTr.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiCore : ControllerBase
    {
        private readonly IRedisConnectionE _connectionFactory;

        public ApiCore(IRedisConnectionE connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        [HttpPost]
        public async Task<MDataOut> PostCore([FromBody] MDataIn mDataIn)
        { 
            MDataOut mDataOut=new MDataOut(mDataIn);
            var server = _connectionFactory.GetServer();
            var b = await server.SubscriptionSubscriberCountAsync(Utils.ChanelName(mDataIn));
            if (b == 0)
            {
                mDataOut.SetErrorData(ErrorCode.PrintError,
                    $"Печатающее устройство не подключено {Utils.ErrorPostfix(mDataIn)} ");
                return mDataOut;
            }

            if (b > 1)
            {
                mDataOut.SetErrorData(ErrorCode.PrintError, $"Печатающx устройств больше чем одно {b} {Utils.ErrorPostfix(mDataIn)}");
                return mDataOut;
            }

           
            try
            {
                 mDataIn.Validate();
               
            }
            catch (Exception e)
            {
                mDataOut.SetErrorData(ErrorCode.PrintError, $"{e.Message} {Utils.ErrorPostfix(mDataIn)}");
                return mDataOut;
            }
            
             

           
            var r = new StreamReader(Request.Body);
            r.BaseStream.Seek(0, SeekOrigin.Begin);
            var  rawBody=await r.ReadToEndAsync();

            if (string.IsNullOrEmpty(rawBody)) throw new Exception("Не могу олучить тело запроса");
            RedisWorker redisWorker = new RedisWorker(_connectionFactory, mDataIn, mDataOut, rawBody);

            TaskWrapper wrapper = new TaskWrapper();

            var ee = redisWorker.Run(wrapper);
            ee.Wait();

            if (wrapper.Exception != null)
            {
                mDataOut.SetErrorData(ErrorCode.PrintError, wrapper.Exception.Message);
                wrapper.Action?.Invoke();
                return mDataOut;
            }
            else
            {
                if (wrapper.Task.Wait(1000))
                {
                    Console.WriteLine($" ############ {mDataOut.ErrorText}");
                }
                else
                {
                    wrapper.Action?.Invoke();
                    mDataOut.SetErrorData(ErrorCode.PrintError, "Превышен лимит времени ожидания ответа от принтера");
                }
                return mDataOut;
            }
           

        }
    }

    public class TaskWrapper
    {
        public Task Task { get; set; }=new Task(() => { });
        public Action Action { get; set; }
        public Exception Exception { get; set; } 
    }

}
