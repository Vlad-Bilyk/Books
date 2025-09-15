using Books.Core.Models.DTO;

namespace Books.Core.Interfaces;

public interface IFilterReader
{
    Filter Read(string filterPath);
}
