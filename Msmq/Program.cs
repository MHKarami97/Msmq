using System;
using System.Collections.Generic;
using System.Messaging;

namespace Msmq
{
    internal class Program
    {
        private const bool ShowOnConsole = false;
        private static readonly List<string> Data = new List<string>();
        private const string QueuePath = @".\private$\asacepsecpersistrlcmessagefailed";

        public static void Main(string[] args)
        {
            Console.WriteLine("start reading");

            var queue = new MessageQueue(QueuePath);
            queue.Formatter = new BinaryMessageFormatter();
            queue.ReceiveCompleted += Queue_ReceiveCompleted;
            queue.BeginReceive();

            Console.WriteLine("eny key to finish...");
            Console.ReadKey();
            Console.WriteLine("end");
        }

        private static void Queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (!(sender is MessageQueue queue))
            {
                throw new Exception("queue is not valid");
            }

            try
            {
                var msg = queue.EndReceive(e.AsyncResult);

                if (msg == null)
                    return;

                var message = msg.Body;

                DoProcessMessage(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                queue.BeginReceive();
            }
        }

        private static void DoProcessMessage(object input)
        {
            var msg = input as string;

            if (ShowOnConsole)
                Console.WriteLine(msg);

            Data.Add(msg);
        }
    }
}