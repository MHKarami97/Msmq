using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;

namespace Msmq
{
    internal class Program
    {
        private const bool ShowOnConsole = false;
        private static readonly List<string> Data = new List<string>();
        private const string QueuePath = @".\private$\asacepsecrlcmessage";
        private const string FilePath = "resultData.txt";

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
                var isQueueEmpty = IsQueueEmpty(_queue);

                if (isQueueEmpty)
                {
                    CheckDuplicates();
                }

                _queue.BeginReceive();
            }
        }

        private static bool IsQueueEmpty(MessageQueue queue)
        {
            using (var enumerator = queue.GetMessageEnumerator2())
            {
                return !enumerator.MoveNext();
            }
        }

        private static void DoProcessMessage(object input)
        {
            if (!(input is string msg))
            {
                throw new Exception("queue is not valid");
            }

            if (ShowOnConsole)
                Console.WriteLine(msg);

            var m = msg.Split(' ');

            Data.Add(m[0]);
        }

        private static void CheckDuplicates()
        {
            var duplicates = Data.GroupBy(x => x)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicates.Any())
            {
                duplicates.ForEach(Console.WriteLine);
            }

            WriteAllToFile();

            Console.WriteLine("queue is empty");
        }

        private static void WriteAllToFile()
        {
            File.WriteAllLines(FilePath, Data);
            Console.WriteLine("write to file finished");
        }
    }
}