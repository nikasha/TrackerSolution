using StorageService.Models;

namespace StorageService.Db
{
    public interface IVisitService
    {
        void SaveVisit(Visit visit);
    }
}