using AttendanceManagementSystem.Data.Interfaces;
using AttendanceManagementSystem.Models.Entities;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Repositories.Implementations
{
    public class HolidayRepository : BaseRepository<Holiday>
    {
        public HolidayRepository(IMongoDbContext context) : base(context) { }

        public async Task<List<Holiday>> GetByYearAsync(int year)
        {
            return await _collection
                .Find(x => x.Year == year && !x.IsDeleted)
                .SortBy(x => x.Date)
                .ToListAsync();
        }
    }
}