using System.Threading.Tasks;
using FinTech.Application.Services;

namespace FinTech.Application.Interfaces.Services
{
    public interface IJournalEntryService
    {
        Task<JournalEntryDto> CreateJournalEntryAsync(JournalEntryDto journalEntryDto);
        Task<JournalEntryDto> GetJournalEntryByIdAsync(int id);
        Task<bool> PostJournalEntryAsync(int id);
        Task<bool> VoidJournalEntryAsync(int id, string reason);
    }
}