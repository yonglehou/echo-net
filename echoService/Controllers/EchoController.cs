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

namespace echoService.Controllers
{
    public class EchoController : ApiController
    {
        // this is how the view will be structured
        // channel, category, then list of messages - time ordered?
        private Dictionary<string, Dictionary<string, List<EchoMessage>>> _buffer = new Dictionary<string, Dictionary<string, List<EchoMessage>>>();

        public JsonResult<IEnumerable<EchoMessage>> GetAsync(string channel)
        {         
            // given only a channel (and no category) I will return a concatenated list of echo messages.
            if(_buffer.ContainsKey(channel))
            {
                IEnumerable<EchoMessage> allMessages = new List<EchoMessage>();
                       
                foreach (var category in _buffer[channel])
                {
                    allMessages = allMessages.Concat(category.Value);
                }

                // in time ordering
                return Json<IEnumerable<EchoMessage>>(allMessages.OrderByDescending(x => x.TimeStamp));
            }

            return NoChannelError(channel);
        }

        public JsonResult<Dictionary<string, Dictionary<string, List<EchoMessage>>>> Get()
        {
            // all data for now, I'll use this to debug.
            return Json(_buffer);
        }

        // GET echo 
        public JsonResult<IEnumerable<EchoMessage>> Get(string channel, string category)
        {
            // if we're lucky, we've got the channel on hand, otherwise, craft an apology.
            if (_buffer.ContainsKey(channel))
            {
                if (_buffer[channel].ContainsKey(category))
                {
                    return Json<IEnumerable<EchoMessage>>(_buffer[channel][category]);
                }

                return NoCategoryError(channel, category);
            }

            // the apology
            return NoChannelError(channel);
        }

        /// <summary>
        /// This should remain an asynchronous task, even if I do not currently capitalize on that.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="category"></param>
        /// <param name="formattedMessage"></param>
        /// <returns></returns>
        private async Task StoreMessage(EchoMessage message)
        {
            var table = ControllerContext.Configuration.Properties["Table"] as CloudTable;

            GuaranteeChannelCategoryExists(message.Channel, message.Tags);

            _buffer[message.Channel][message.Tags].Add(message);   

            await table.ExecuteAsync(TableOperation.InsertOrMerge(new EchoTableEntity(message)));
        }

        // echo/dotnet/noise/"hello world!"
        public System.Web.Http.Results.JsonResult<string> Get(string channel, string category, string message)
        {
            var newMessage = FormatMessage(channel, category, message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StoreMessage(newMessage); // fire and forget should be the protocol here
#pragma warning restore CS4014

            return Json<string>(newMessage.FormattedMessage);
        }

        #region privates

        private TableBatchOperation _batchOperation = new TableBatchOperation();

        private void GuaranteeChannelCategoryExists(string channel, string category)
        {
            if (!_buffer.ContainsKey(channel))
            {
                _buffer.Add(channel, new Dictionary<string, List<EchoMessage>>());

                _batchOperation.Add(TableOperation.Insert(new ChannelMarkerTableEntity(channel)));
            }

            if (!_buffer[channel].ContainsKey(category))
            {
                _batchOperation.Add(TableOperation.Insert(new CategoryMarkerTableEntity(channel, category)));
                _buffer[channel].Add(category, new List<EchoMessage>());
            }
        }

        private bool GuaranteeCategoryExists(string channel, string category)
        {
            if (_buffer.ContainsKey(channel))
            {
                _buffer.Add(channel, new Dictionary<string, List<EchoMessage>>());

                if (!_buffer.ContainsKey(category))
                {
                    _buffer.Add(channel, new Dictionary<string, List<EchoMessage>>());
                }
            }
            return false;
        }

        internal static EchoMessage FormatMessage(string channel, string tags, string message)
        {
            var timestamp = DateTime.Now;
            return new EchoMessage()
            {
                TimeStamp = timestamp,
                Channel = channel,
                Tags = tags,
                FormattedMessage = $"{timestamp} - [{channel}, {tags}] :: {message}"
            };
        }

        private JsonResult<IEnumerable<EchoMessage>> NoChannelError(string channel)
        {
            return Json<IEnumerable<EchoMessage>>(new[] { FormatMessage(channel, "error", $"Sorry, there is no channel with the name: '{channel}'") });
        }

        private JsonResult<IEnumerable<EchoMessage>> NoCategoryError(string channel, string category)
        {
            return Json<IEnumerable<EchoMessage>>(new[] { FormatMessage(channel, "error", $"Sorry, there is no category with the name: '{channel}'") });
        }

        #endregion
    }
}
