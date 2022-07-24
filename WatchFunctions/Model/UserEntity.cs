using Microsoft.WindowsAzure.Storage.Table;

namespace WatchFunctions.Model
{
    public class UserEntity : TableEntity
    {
        public string Handle { get; set; }
        public byte[] Password { get; set; } 
        public byte[] Salt { get; set; }
        public string Email { get; set; }

        public UserEntity() { }
    }
}
