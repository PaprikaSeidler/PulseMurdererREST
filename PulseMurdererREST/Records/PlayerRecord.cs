using PulseMurdererV3;

namespace PulseMurdererREST.Records
{
    public record PlayerRecord(int? Id, string? Name, string? Avatar, bool IsMurderer  );

    public static class RecordHelper 
    {
        public static Player ConvertPlayerRecord(PlayerRecord record)
        {
            if (record.Id == null)
            {
                throw new ArgumentNullException("" + record.Id);
            }
            if (record.Name == null)
            {
                throw new ArgumentNullException("" + record.Name);
            }
            return new Player
            {
                Id = (int)record.Id,
                Name = record.Name,
                Avatar = record.Avatar,
                IsMurderer = record.IsMurderer
            };
        }
    }
}
