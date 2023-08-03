using LocalizeApi.Context;
using LocalizeApi.Models;
using LocalizesApi.Services;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;




namespace LocalizeApi.Services
{
    public class TarefasService : ITarefaService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;




        public TarefasService(AppDbContext context)
        {
            _context = context;
          
        }

        public async Task<IEnumerable<Tarefa>> GetTarefas()
        {
            try
            {
                return await _context.Tarefas.ToListAsync();
            }
            catch 
            {
                throw;
            }
        }

        public async Task CreateTarefa(Tarefa Tarefa)
        {
            _context.Tarefas.Add(Tarefa);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTarefa(Tarefa Tarefa)
        {

            var existingTarefa = await _context.Tarefas.FindAsync(Tarefa.Id);
            _context.Entry(existingTarefa).CurrentValues.SetValues(Tarefa);
            _context.Update(existingTarefa);
            await _context.SaveChangesAsync();
        }

        public async Task<Tarefa> GetTarefa(int id)
        {
            Tarefa Tarefa = await _context.Tarefas.FindAsync(id);
            return Tarefa;
        }

        public async Task DeleteTarefa(Tarefa Tarefa)
        {
            _context.Tarefas.Remove(Tarefa);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Tarefa>> GetTarefaByStatus(string Status)
        {
            if (!string.IsNullOrWhiteSpace(Status))
            {
                var tarefas = await _context.Tarefas.Where(n => n.Description.Contains(Status)).ToListAsync();
                return tarefas;
            }
            else
            {
                var tarefas = await GetTarefas();
                return tarefas;
            }
        }


        public async Task CreateTarefaByStatus(string Status)
        {

            Random random = new Random();
            int randomNumber = random.Next();


            var Tarefa = new Tarefa
            {
                Status = Status,
                Id= randomNumber

            };

            // Realizar a solicitação de extração na ReceitaWS
            var options = new RestClientOptions("https://receitaws.com.br")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/v1/Status/"+ Status, Method.Get);
            request.AddHeader("Accept", "application/json");
            var body = @"";
            request.AddParameter("text/plain", body, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);


            if (response.IsSuccessStatusCode)
            {
                Tarefa.Description = response.Content;
            }
            else
            {
                // Tratar o caso de erro na chamada à API ReceitaWS
                // Exemplo: logar o erro, lançar uma exceção, etc.
                throw new Exception("Erro ao consultar a ReceitaWS");
            }


            _context.Tarefas.Add(Tarefa);
            await _context.SaveChangesAsync();
        }

    }
}
