using StorageService.Models;

namespace StorageService.Db
{
    public class VisitService(AppDbContext dbContext, ILogger<VisitService> logger) : IVisitService
    {
        private readonly AppDbContext _dbContext = dbContext;
        private readonly ILogger<VisitService> _logger = logger;

        public void SaveVisit(Visit visit)
        {
            try
            {
                _logger.LogInformation("Saving the visit into the database...");
                _dbContext.Visits.Add(visit);
                _dbContext.SaveChanges();
                _logger.LogInformation("The visit was saved into the database");
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error ocurred when trying to save the visit into the database");
                throw;
            }
        }
    }
}