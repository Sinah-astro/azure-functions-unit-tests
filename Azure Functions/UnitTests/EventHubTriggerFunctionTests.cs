using Xbehave;
using System.Text;
using EventData = Azure.Messaging.EventHubs.EventData;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Azure_Functions.Functions;
using System;
using Shouldly;

namespace AzureFunction.Tests
{
    public class EventGridTriggerFunctionTests
    {
        [Scenario]
        [Example("some data")]
        public void EventHubTriggerFunction_WhenThereIsOneValidEvent_ProcessesSuccessfully(string data)
        {
            Mock<ILogger> logger = null;
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(data));
            EventData[] eventDatas = null;
            string output = null;
            StringWriter outputWriter = null;

            "Given an event is initiated".x(() =>
            {
                eventDatas = new[]
                {
                    eventData
                };

                logger = new Mock<ILogger>();

                outputWriter = new StringWriter();
                Console.SetOut(outputWriter);
            });

            "When it is received by the handler".x(async ()
                =>
            {
                await EventHubTriggerFunction.Run(eventDatas, logger.Object);
            });

            "Then it should be executed successfully".x(()
                =>
            {
                logger.Verify(
                    x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) =>
                            v.ToString().StartsWith($"C# Event Hub trigger function processed a message:")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

                output = outputWriter.ToString().Trim();
                output.ShouldBe("Executed successfully with no exceptions!");
            });
        }
    }
}