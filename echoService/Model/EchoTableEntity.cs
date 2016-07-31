using echoService.Controllers;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace echoService.Model
{
    public class ChannelMarkerTableEntity : EchoTableEntity
    {
        public ChannelMarkerTableEntity(string channel) : base(EchoController.FormatMessage(channel, "_dev_static_channel_marker", "channel_marker"))
        {
            RowKey = DateTimeOffset.MinValue.Ticks.ToString();
        }
    }

    public class CategoryMarkerTableEntity : EchoTableEntity
    {
        public CategoryMarkerTableEntity(string channel, string category) : base(EchoController.FormatMessage(channel, "_dev_static_channel_category", category))
        {
            RowKey = DateTimeOffset.MinValue.Ticks.ToString();
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
        public string Tags { get; set; }

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
            Tags = message.Tags;
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
                Tags = entity.Tags,
                FormattedMessage = entity.FormattedMessage
            };
        }
    }
}
