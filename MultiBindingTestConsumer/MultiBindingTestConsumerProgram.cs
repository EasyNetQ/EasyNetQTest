﻿using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;

namespace MultiBindingTestConsumer
{
    class MultiBindingConsumerProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Consumer...");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEasyNetQ("host=localhost").UseSystemTextJson();

            using var provider = serviceCollection.BuildServiceProvider();
            var bus = provider.GetRequiredService<IBus>();

            var advancedBus = bus.Advanced;
            var exchange = advancedBus.ExchangeDeclare(Messages.Constants.MultiBindingExchange, "topic");
            var queue = advancedBus.QueueDeclare("multibinding.test.consumer");

            // create 1000 bindings
            for (var i1 = 0; i1 < 10; i1++)
            {
                for (var i2 = 0; i2 < 10; i2++)
                {
                    for (var i3 = 0; i3 < 10; i3++)
                    {
                        advancedBus.Bind(exchange, queue, $"{i1}.{i2}.{i3}");
                    }
                }
            }

            advancedBus.Consume<Messages.TextMessage>(queue, async (msg, info) =>
            {
                Console.WriteLine($"Message: {msg.Body.Text}");
                await Task.Delay(10);
            });

            Console.WriteLine("Consumer started. Press Enter to quit");
            Console.ReadLine();
        }
    }
}
