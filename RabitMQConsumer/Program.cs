using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using Microsoft.Office.Interop.Outlook;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Numerics;
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

               
                SendEmailOutlook("luizgustavomoura20@gmail.com", "Mensagem da Fila", message.ToString());
            };

            channel.BasicConsume(queue: "Tarefas", autoAck: true, consumer: consumer);

            Console.WriteLine("Consumer is running. Press any key to exit.");
            Console.ReadKey();
        }
        catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
        {
            Console.WriteLine("Erro ao conectar ao RabbitMQ: " + ex.Message);
        }
        catch (RabbitMQ.Client.Exceptions.ConnectFailureException ex)
        {
            Console.WriteLine("Falha na conexão com o RabbitMQ: " + ex.Message);
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex)
        {
            Console.WriteLine("Interrupção de operação no RabbitMQ: " + ex.Message);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine("Erro desconhecido: " + ex.Message);
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
        }
    }



}
