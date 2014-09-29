using System;
using Messages;
using NServiceBus;

class Program
{
    static string BasePath = "..\\..\\..\\storage";

    static void Main()
    {
        var busConfiguration = new BusConfiguration();
        busConfiguration.EndpointName("Sample.DataBus.Sender");
        busConfiguration.UseSerialization<JsonSerializer>();
        busConfiguration.FileShareDataBus(BasePath);
        busConfiguration.UsePersistence<InMemoryPersistence>();
        busConfiguration.EnableInstallers();
        var startableBus = Bus.Create(busConfiguration);
        using (var bus = startableBus.Start())
        {
            Start(bus);
        }
    }


    static void Start(IBus bus)
    {
        Console.WriteLine("Press 'Enter' to send a large message (>4MB)");
        Console.WriteLine("Press 'E' to send a message that will exceed the limit and throw");
        Console.WriteLine("To exit, press Ctrl + C");

        while (true)
        {
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.E)
            {
                SendMessageTooLargePayload(bus);
                continue;
            }

            if (key.Key == ConsoleKey.Enter)
            {
                SendMessageLargePayload(bus);
            }
        }
    }

    static void SendMessageLargePayload(IBus bus)
    {
        var message = new MessageWithLargePayload
        {
            SomeProperty = "This message contains a large blob that will be sent on the data bus",
            LargeBlob = new DataBusProperty<byte[]>(new byte[1024*1024*5]) //5MB
        };
        bus.Send("Sample.DataBus.Receiver",message);

        Console.WriteLine("Message sent, the payload is stored in: " + BasePath);
    }

    static void SendMessageTooLargePayload(IBus bus)
    {
        var message = new AnotherMessageWithLargePayload
        {
            LargeBlob = new byte[1024 * 1024 * 5] //5MB
        };
        bus.Send("Sample.DataBus.Receiver", message);
    }
}