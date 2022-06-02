using System.Collections.Generic;

namespace LineBalancing.Interfaces
{
    public interface ICrud<T> where T : class
    {
        List<T> GetAll();
        List<T> GetAllById(int id);
        T GetById(int id);
        bool SaveUpdate(T data);
        bool Delete(T data);
    }
}
