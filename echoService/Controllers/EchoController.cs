using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace echoService.Controllers
{
    public class EchoController : ApiController
    {
        static Dictionary<string, List<string>> _buffer = new Dictionary<string, List<string>>();

        public EchoController()
        {
            
        }

        public JsonResult<Dictionary<string, List<string>>> Get()
        {
            // all channels
            return Json(_buffer);
        }

        // GET echo 
        public JsonResult<IEnumerable<string>> Get(string channel)
        {
            // if we're lucky, we've got the channel on hand, otherwise, craft an apology.
            if (_buffer.ContainsKey(channel))
            {
                return Json<IEnumerable<string>>(_buffer[channel]);
            }

            // the apology
            return Json<IEnumerable<string>>(new[] { FormatMessage("error", $"Sorry, there is no channel by the name {channel}.") });
        }

        private static string FormatMessage(string channel, string message)
        {
            return $"{DateTime.Now} - [{channel}] :: {message}";
        }


        private async Task AddToChannel(string channel, string message)
        {
            if (!_buffer.ContainsKey(channel))
            {
                _buffer.Add(channel, new List<string>());
            }
        }

        // GET echo?message=Hello%20World 
        public System.Web.Http.Results.JsonResult<string> Get(string channel, string message)
        {
            string newMessage = FormatMessage(channel, message);

            AddToChannel(channel, newMessage); // fire and forget.

            return Json<string>(newMessage);
        }
        
    }
}
