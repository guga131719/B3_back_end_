using LocalizeApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocalizesApi.Services
{
    public interface ITarefaService
    {
        Task<IEnumerable<Tarefa>> GetTarefas();
        Task<Tarefa> GetTarefa(int id);
        Task<IEnumerable<Tarefa>> GetTarefaByStatus(string Status);
        Task CreateTarefa(Tarefa aluno);
        Task UpdateTarefa(Tarefa aluno);
        Task DeleteTarefa(Tarefa aluno);

        Task CreateTarefaByStatus(string Status);

     
    }
}
