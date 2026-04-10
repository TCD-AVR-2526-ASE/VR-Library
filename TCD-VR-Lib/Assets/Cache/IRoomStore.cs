using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRoomStore
{
    Task<List<RoomData>> LoadAll();
    Task SaveAll(List<RoomData> rooms);
}
