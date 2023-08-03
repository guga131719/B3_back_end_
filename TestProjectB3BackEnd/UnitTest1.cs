using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using LocalizeApi.Controllers;
using LocalizesApi.Services;
using AlunosApi.Models;
using MassTransit;
using LocalizeApi.Models;
using TarefaApi.RabbitMQ;

namespace MeuProjetoTeste
{
    [TestClass]
    public class TarefasControllerTests
    {
        private TarefasController _controller;
        private Mock<ITarefaService> _tarefaServiceMock;
        private Mock<IBusControl> _busControlMock;
        private Mock<IPublishEndpoint> _publishEndpointMock;
        private Mock<IRabitMQProducer> _rabitMQProducerMock;

        [TestInitialize]
        public void Setup()
        {
            _tarefaServiceMock = new Mock<ITarefaService>();
            _busControlMock = new Mock<IBusControl>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _rabitMQProducerMock = new Mock<IRabitMQProducer>();

            _controller = new TarefasController(
                _tarefaServiceMock.Object,
                _busControlMock.Object,
                _publishEndpointMock.Object,
                _rabitMQProducerMock.Object
            );
        }

        [TestMethod]
        public void CreateOrUpdate_ValidTarefa_ReturnsOkResult()
        {
            // Arrange
            var tarefa = new Tarefa { Description = "Tarefa Teste", Status = "Fazendo", Date = DateTime.Now };
            _tarefaServiceMock.Setup(service => service.GetTarefa(tarefa.Id)).ReturnsAsync((Tarefa)null);

            // Act
            var result = _controller.CreateOrUpdate(tarefa).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _tarefaServiceMock.Verify(service => service.CreateTarefa(tarefa), Times.Once);
            _rabitMQProducerMock.Verify(producer => producer.SendProductMessage(tarefa), Times.Once);
        }

        [TestMethod]
        public void CreateOrUpdate_ExistingTarefa_ReturnsOkResult()
        {
            // Arrange
            var tarefa = new Tarefa { Id = 3, Description = "Tarefa Teste", Status = "Fazendo", Date = DateTime.Now.AddDays(3) };
            _tarefaServiceMock.Setup(service => service.GetTarefa(tarefa.Id)).ReturnsAsync(tarefa);

            // Act
            var result = _controller.CreateOrUpdate(tarefa).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _tarefaServiceMock.Verify(service => service.UpdateTarefa(tarefa), Times.Once);
            _rabitMQProducerMock.Verify(producer => producer.SendProductMessage(It.IsAny<Tarefa>()), Times.Never);
        }

        // Add more test methods for other scenarios
    }
}
