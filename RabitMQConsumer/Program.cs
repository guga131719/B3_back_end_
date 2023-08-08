using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using EASendMail;

class Program
{
    static void Main()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare("Tarefas", exclusive: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Tasks message received: {message}" + message.ToString());

       
                SendLogToRabbitMQ("ok "+message);

             
                SendEmailOutlook("luizgustavomoura20@gmail.com", "Mensagem da Fila", message.ToString());
            };

            channel.BasicConsume(queue: "Tarefas", autoAck: true, consumer: consumer);

            Console.WriteLine("Consumer is running. Press any key to exit.");
            Console.ReadKey();
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Erro : " + ex.Message);
            SendLogToRabbitMQ("Erro : " + ex.Message.ToString());
        }
    }

  

    static void SendEmailOutlook(string destinatarioEmail, string assunto, string mensagem)
    {
        try
        {
            SmtpMail oMail = new SmtpMail("TryIt");
            oMail.From = "uizgustavomoura20@outlook.com";
            oMail.To = destinatarioEmail;
            oMail.Subject = assunto;
            oMail.TextBody = mensagem;
            SmtpServer oServer = new SmtpServer("smtp.office365.com");

            oServer.User = "uizgustavomoura20@outlook.com";
            oServer.Password = "guga131719";
            oServer.Port = 587;
            oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

            Console.WriteLine("Starting evio de email TLS...");

            EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
            oSmtp.SendMail(oServer, oMail);

            Console.WriteLine("OK  ao enviar o e-mail");
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Erro ao enviar o e-mail: " + ex.Message);
            SendLogToRabbitMQ("Erro ao enviar o e-mail: "+ ex.Message.ToString());
        }
    }

    static void SendLogToRabbitMQ(string logMessage)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare("logs_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(logMessage);
        channel.BasicPublish(exchange: "", routingKey: "logs_queue", basicProperties: null, body: body);
        Console.WriteLine($"Sent log to RabbitMQ: {logMessage}");
    }
}
