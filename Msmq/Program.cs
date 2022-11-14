using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;

namespace Msmq
{
    internal class Program
    {
        private const bool ShowOnConsole = false;
        private static readonly List<string> Data = new List<string>();
        private const string QueuePath = @".\private$\asacepsecrlcmessage";

        private static MessageQueue _queue;

        public static void Main(string[] args)
        {
            Console.WriteLine("start reading");

            _queue = new MessageQueue(QueuePath)
            {
                Formatter = new XmlMessageFormatter(new[] { typeof(string) })
            };
            _queue.ReceiveCompleted += Queue_ReceiveCompleted;
            _queue.BeginReceive();

            Console.WriteLine("eny key to finish...");
            Console.ReadKey();
            Console.WriteLine("end");
        }

        private static void Queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (e == null || !e.AsyncResult.IsCompleted)
            {
                return;
            }

            try
            {
                var msg = e.Message.Body;

                if (msg == null)
                    return;

                DoProcessMessage(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------");
                Console.WriteLine(ex);
                Console.WriteLine("----------");
            }
            finally
            {
                _queue.BeginReceive();
            }
        }

        private static void DoProcessMessage(object input)
        {
            var msg = input as string;

            if (ShowOnConsole)
                Console.WriteLine(msg);

            Data.Add(msg);

            var duplicates = Data.GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
        }
    }
}