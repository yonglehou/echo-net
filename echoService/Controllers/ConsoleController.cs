using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Linq;
using Microsoft.ServiceFabric.Data.Collections;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Data;
using Microsoft.WindowsAzure.Storage.Table;

using echoService.Model;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;

namespace echoService.Controllers
{
    public class ConsoleController : ApiController
    {
        // this is how the view will be structured
        // channels are composed of categories which are composed of messages that are in time order.
        private Dictionary<string, Dictionary<string, List<EchoMessage>>> _buffer;

        /// <summary>
        /// Use this Buffer for read operations, and use _buffer for the write operations.
        /// </summary>
        IReadOnlyDictionary<string, Dictionary<string, List<EchoMessage>>> Buffer
        {
            get
            {
                if (_buffer == null) FetchAll().Wait();
                return _buffer;
            }
        }


        /// <summary>
        /// Gets all channels, and all categories
        /// </summary>
        /// <returns></returns>
        public JsonResult<IReadOnlyDictionary<string, Dictionary<string, List<EchoMessage>>>> Get()
        {
            return Json(Buffer);
        }

        /// <summary>
        /// Get all categories in a channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public JsonResult<IEnumerable<EchoMessage>> Get(string channel)
        {
            // given only a channel (and no category) I will return a concatenated list of echo messages.
            if (Buffer.ContainsKey(channel))
            {
                IEnumerable<EchoMessage> allMessages = new List<EchoMessage>();

                foreach (var category in Buffer[channel])
                {
                    allMessages = allMessages.Concat(category.Value);
                }

                // in time ordering
                return Json<IEnumerable<EchoMessage>>(allMessages.OrderByDescending(x => x.TimeStamp));
            }

            return NoChannelError(channel);
        }

        /// <summary>
        /// Get all messages in a channel and a category.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        // GET echo 
        public JsonResult<IEnumerable<EchoMessage>> Get(string channel, string category)
        {
            
            // if we're lucky, we've got the channel on hand, otherwise, craft an apology.
            if (Buffer.ContainsKey(channel))
            {
                if (Buffer[channel].ContainsKey(category))
                {
                    return Json<IEnumerable<EchoMessage>>(Buffer[channel][category]);
                }

                return NoCategoryError(channel, category);
            }

            return NoChannelError(channel);
        }

        

        // echo/dotnet/noise?message=hi
        public JsonResult<string> Get(string channel, string category, [FromUri]string message)
        {
            var newMessage = FormatMessage(channel, category, message);

            Task.Factory.StartNew(async () => await StoreMessage(newMessage)); // "fire and forget" should be the protocol here

            return Json<string>(newMessage.FormattedMessage);
        }

        #region privates


        private void GuaranteeChannelCategoryExists(TableBatchOperation batchOperation, string channel, string category)
        {
            if (!Buffer.ContainsKey(channel))
            {
                _buffer.Add(channel, new Dictionary<string, List<EchoMessage>>());

                batchOperation.Add(TableOperation.InsertOrReplace(new ChannelMarkerTableEntity(channel)));
            }

            if (!Buffer[channel].ContainsKey(category))
            {
                _buffer[channel].Add(category, new List<EchoMessage>());

                batchOperation.Add(TableOperation.InsertOrReplace(new CategoryMarkerTableEntity(channel, category)));
            }
        }

        internal static EchoMessage FormatMessage(string channel, string category, string message)
        {
            var timestamp = DateTime.Now;
            return new EchoMessage()
            {
                TimeStamp = timestamp,
                Channel = channel,
                Category = category,
                FormattedMessage = $"{timestamp} - [{channel}, {category}] :: {message}"
            };
        }

        private JsonResult<IEnumerable<EchoMessage>> NoChannelError(string channel)
        {
            return Json<IEnumerable<EchoMessage>>(new[] { FormatMessage(channel, "error", $"Sorry, there is no channel with the name: '{channel}'") });
        }

        private JsonResult<IEnumerable<EchoMessage>> NoCategoryError(string channel, string category)
        {
            return Json<IEnumerable<EchoMessage>>(new[] { FormatMessage(channel, "error", $"Sorry, there is no category with the name: '{category}'") });
        }

        private async Task FetchAll()
        {
            var watch = Stopwatch.StartNew();
            _buffer = new Dictionary<string, Dictionary<string, List<EchoMessage>>>();
            // all data for now, I'll use this to debug.
            var table = ControllerContext.Configuration.Properties["Table"] as CloudTable;

            // first we retrieve the channel markers
            var result = (from x in table.CreateQuery<EchoTableEntity>()
                          where x.PartitionKey == "__echo_internal__"
                          select x).ToList();

            // then, we retrieve the data for each channel and serialize it in to our cache.
            foreach (var channel_marker in result)
            {
                try
                {
                    var marker = JsonConvert.DeserializeObject<Marker>(channel_marker.RowKey);

                    if (!_buffer.ContainsKey(marker.Channel))
                    {
                        _buffer.Add(marker.Channel, new Dictionary<string, List<EchoMessage>>());
                    }

                    var channelTableEntities = (from element in table.CreateQuery<EchoTableEntity>()
                                                where element.PartitionKey == marker.Channel
                                                select element).ToList();

                    // convert them to categorized messages and place them in the dictionary.
                    var categorizedMessages = channelTableEntities.Select(x => x.ToEchoMessage()).GroupBy(x => x.Category);

                    foreach (var category in categorizedMessages)
                    {
                        if (!_buffer[marker.Channel].ContainsKey(category.Key))
                        {
                            _buffer[marker.Channel].Add(category.Key, category.ToList());
                        }
                    }

                }
                catch (Exception ex) { ServiceEventSource.Current.Message($"{ex}"); }
            }

            ServiceEventSource.Current.Message($"all data retrieved and serialized in {watch.ElapsedMilliseconds} ms - goal is 500 ms");
        }

        private async Task StoreMessage(EchoMessage message)
        {
            try
            {
                var internalBatchOperation = new TableBatchOperation();
                var table = ControllerContext.Configuration.Properties["Table"] as CloudTable;

                GuaranteeChannelCategoryExists(internalBatchOperation, message.Channel, message.Category);

                _buffer[message.Channel][message.Category].Add(message);

                await table.ExecuteBatchAsync(internalBatchOperation); // internal data is on a separate partition.

                await table.ExecuteAsync(TableOperation.InsertOrMerge(new EchoTableEntity(message)));

                foreach (var result in await table.ExecuteBatchAsync(internalBatchOperation))
                {
                    ServiceEventSource.Current.Message($"Status code: {result.HttpStatusCode} - {Enum.GetName(typeof(HttpStatusCode), result.HttpStatusCode)}");
                }
            }
            catch (ArgumentException ex) // previously hit on: All entities in a given batch must have the same partition key.
            {
                ServiceEventSource.Current.Message($"Argument Exception : {ex?.ParamName} - {ex.Message}\r\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message($"Argument Exception : {ex.HResult} - {ex.Message}\r\n{ex.StackTrace}");
            }
        }

        #endregion
    }
}
