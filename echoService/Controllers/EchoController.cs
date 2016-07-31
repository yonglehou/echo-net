using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Linq;

namespace echoService.Controllers
{
    public class EchoController : ApiController
    {
        public static Dictionary<string, Dictionary<string, List<EchoMessage>>> _buffer = new Dictionary<string, Dictionary<string, List<EchoMessage>>>();

        public struct EchoMessage
        {
            public DateTime timestamp;
            public string channel;
            public string category;
            public string formattedMessage;
        }

        public JsonResult<IEnumerable<EchoMessage>> Get(string channel)
        {
            // given only a channel (and no category) I will return a concatenated list of echo messages.
            if (_buffer.ContainsKey(channel))
            {
                IEnumerable<EchoMessage> allMessages = new List<EchoMessage>();
                foreach(var category in _buffer[channel])
                {
                    allMessages = allMessages.Concat(category.Value);
                }

                return Json<IEnumerable<EchoMessage>>(allMessages.OrderByDescending(x => x.timestamp));
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
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task StoreMessage(EchoMessage message)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            GuaranteeChannelCategoryExists(message.channel, message.category);
            _buffer[message.channel][message.category].Add(message);   
        }

        // GET echo?message=Hello%20World 
        public System.Web.Http.Results.JsonResult<string> Get(string channel, string category, string message)
        {
            var newMessage = FormatMessage(channel, category, message);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            StoreMessage(newMessage); // fire and forget should be the protocol here, even if I do not currently do that.
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return Json<string>(newMessage.formattedMessage);
        }

        #region privates


        private void GuaranteeChannelCategoryExists(string channel, string category)
        {
            if (!_buffer.ContainsKey(channel))
            {
                _buffer.Add(channel, new Dictionary<string, List<EchoMessage>>());
            }

            if (!_buffer[channel].ContainsKey(category))
            {
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

        private static EchoMessage FormatMessage(string channel, string category, string message)
        {
            var timestamp = DateTime.Now;
            return new EchoMessage()
            {
                timestamp = timestamp,
                channel = channel,
                category = category,
                formattedMessage = $"{timestamp} - [{channel}, {category}] :: {message}"
            };
        }

        private JsonResult<IEnumerable<EchoMessage>> NoChannelError(string channel)
        {
            return Json<IEnumerable<EchoMessage>>(new[] { FormatMessage(channel, "error", $"Sorry, there is no channel with the name {channel}.") });
        }

        private JsonResult<IEnumerable<EchoMessage>> NoCategoryError(string channel, string category)
        {
            return Json<IEnumerable<EchoMessage>>(new[] { FormatMessage(channel, "error", $"Sorry, there is no category with the name {channel}.") });
        }

        #endregion
    }
}
