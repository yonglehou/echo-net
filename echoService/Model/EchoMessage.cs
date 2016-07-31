using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace echoService.Model
{
    public class EchoMessage
    {
        public string Channel { get; internal set; }
        public DateTimeOffset TimeStamp { get; internal set; } = DateTimeOffset.MinValue;
        public string Tags { get; internal set; }
        public string FormattedMessage { get; internal set; }
    }


}
