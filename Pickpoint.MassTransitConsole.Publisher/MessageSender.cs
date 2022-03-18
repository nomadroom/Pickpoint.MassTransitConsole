﻿using Common;
using MassTransit;
using NLog;
using Pickpoint.MassTransitConsole.Publisher.Models;
using System.Diagnostics;

namespace Pickpoint.MassTransitConsole.Publisher
{
    internal class MessageSender
    {
        public MessageSender(Logger logger, IBusControl massTransitBusControl)
        {
            this.Logger = logger;
            this.MassTransitBusControl = massTransitBusControl;
        }

        internal Logger Logger { get; }
        internal IBusControl MassTransitBusControl { get; }

        public async Task Send(SendParams sendConfig)
        {
            try
            {
                this.Logger.Info("[*]The application for sending messages is running (Publisher)");

                // отправить сообщение потребителю (consumers)
                var endpoint = await this.MassTransitBusControl.GetSendEndpoint(new Uri("exchange:Consumer"));

                var timer = new Stopwatch();
                timer.Start();
                var messageNumber = sendConfig.MessageNumber;
                var intervalMilliseconds = (int)((double)messageNumber / sendConfig.SendIntervalSeconds * 1000); 
                // 1 сообщение 875 bytes (Статистика из кролика)
                for (int i = 0; i < messageNumber; i++)
                {
                    await endpoint.Send<ISendMessage>(new
                    {
                        Text = "123"
                    });

                    //Примерно 2 мс нужно для отправки сообщения. 
                    await Task.Delay(intervalMilliseconds);
                }
                timer.Stop();
                this.Logger.Info($"[*]Отпралено {messageNumber} сообщений за {(double)timer.ElapsedMilliseconds / 1000} секунд. Ожидаемое время ~ {sendConfig.SendIntervalSeconds} секунд.");
                this.Logger.Info($"[*]Размер сообщений составляет: {messageNumber * 875} bytes");
            }
            finally
            {
                await this.MassTransitBusControl.StopAsync();
            }
        }
    }
}
