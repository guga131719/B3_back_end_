
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LocalizeApi.Models;
using System.Linq;
using LocalizesApi.Services;


using MassTransit;
using AlunosApi.Models;
using MassTransit.Transports;
using TarefaApi.RabbitMQ;

namespace LocalizeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefasController : ControllerBase
    {
        private readonly ITarefaService _TarefaService;
        private readonly IBusControl _bus;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRabitMQProducer _rabitMQProducer;
        public TarefasController(ITarefaService TarefaService, IBusControl bus, IPublishEndpoint publishEndpoint, IRabitMQProducer rabitMQProducer)
        {
            _TarefaService = TarefaService;
            _bus = bus;
            _publishEndpoint = publishEndpoint;
            _rabitMQProducer = rabitMQProducer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tarefa>>> Index()
        {
            try
            {
                var Tarefas = await _TarefaService.GetTarefas();
                return Ok(Tarefas);
            }
            catch
            {
                //return BadRequest("Request inválido");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Erro ao acessar dados de Tarefas");
            }
        }

        [HttpGet("{id:int}", Name = "GetTarefa")]
        public async Task<ActionResult<Tarefa>> Details(int id)
        {
            var Tarefa = await _TarefaService.GetTarefa(id);

            if (Tarefa == null)
                return NotFound($"Tarefa com id= {id} não encontrado");

            return Ok(Tarefa);
        }

        [HttpGet("TarefasPorStatus")]
        public async Task<ActionResult<IEnumerable<Tarefa>>> GetTarefaPorStatus([FromQuery] string status)
        {
            var Tarefas = await _TarefaService.GetTarefaByStatus(status);

            if (Tarefas == null)
                return NotFound($"Não existem Tarefas com status = {status}");

            return Ok(Tarefas);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(Tarefa Tarefa)
        {
            try
            {

                var TarefaRertorno= await _TarefaService.GetTarefa(Tarefa.Id);            


                if (TarefaRertorno == null)
                {
                    await _TarefaService.CreateTarefa(Tarefa);

                    var TarefaRertornoDataCreate = await _TarefaService.GetTarefa(Tarefa.Id);
                    _rabitMQProducer.SendProductMessage(TarefaRertornoDataCreate);

                    return Ok(Tarefa);
                }
                else {

                    await _TarefaService.UpdateTarefa(Tarefa);

                    var TarefaRertornoDataUpdate = await _TarefaService.GetTarefa(Tarefa.Id);
                    _rabitMQProducer.SendProductMessage(TarefaRertornoDataUpdate); 

                    return Ok(Tarefa);

                }

              

            }
            catch (Exception ex)
            {
                return BadRequest("Request inválido" + ex.ToString());
            }
        }
   


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var Tarefa = await _TarefaService.GetTarefa(id);
                if (Tarefa != null)
                {
                    await _TarefaService.DeleteTarefa(Tarefa);
              
                    _rabitMQProducer.SendProductMessage("Delete id Tarefa"+ id.ToString());

                    return Ok(Tarefa);
                }
                else
                {
                    return NotFound($"Tarefa com id= {id} não encontrado");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
