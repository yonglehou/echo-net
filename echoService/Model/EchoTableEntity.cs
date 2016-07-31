using echoService.Controllers;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace echoService.Model
{
    public struct Marker
    {
        public string Type { get; set; }
        public string Channel { get; set; }
        public string Name { get; set; }
    }

    public class ChannelMarkerTableEntity : EchoTableEntity
    {
        public ChannelMarkerTableEntity(string channel) : base(EchoController.FormatMessage(channel, "dev_static_channel_marker", channel))
        {
            PartitionKey = "__echo_internal__";
            RowKey = JsonConvert.SerializeObject(new Marker
            {
                Type = "channel_marker",
                Channel = channel,
                Name = channel
            });
        }
    }

    public class CategoryMarkerTableEntity : EchoTableEntity
    {
        public CategoryMarkerTableEntity(string channel, string category) : base(EchoController.FormatMessage(channel, "dev_static_channel_category", category))
        {
            PartitionKey = "__echo_internal__";
            RowKey = JsonConvert.SerializeObject(new Marker
            {
                Type = "category_marker",
                Channel = channel,
                Name = category
            });
        }
    }

    /// <summary>
    /// PartitionKey = EchoMessage.channel
    /// RowKey = EchoMessage.timestamp
    /// </summary>
    public class EchoTableEntity : TableEntity
    {
        public string Channel { get; set; }
        public DateTimeOffset EchoTimestamp { get; set; }
        public string Category { get; set; }

        public string FormattedMessage { get; set; }

        public EchoTableEntity()
        {

        }

        public EchoTableEntity(EchoMessage message)
        {
            PartitionKey = message.Channel;
            RowKey = message.TimeStamp.Ticks.ToString();

            EchoTimestamp = message.TimeStamp;
            Channel = message.Channel;
            FormattedMessage = message.FormattedMessage;
            Category = message.Category;
        }
    }

    public static class EchoTableEntityExtensions
    {
        public static EchoMessage ToEchoMessage(this EchoTableEntity entity)
        {
            return new EchoMessage()
            {
                Channel = entity.Channel,
                TimeStamp = entity.EchoTimestamp,
                Category = entity.Category,
                FormattedMessage = entity.FormattedMessage
            };
        }
    }
}
