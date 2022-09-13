using Microsoft.WindowsAzure.Storage.Table;

namespace WatchFunctions.Model
{
    public class UserEntity : TableEntity
    {
        public byte[] Password { get; set; } 
        public byte[] Salt { get; set; }
        public string Email { get; set; }
        public string SessionToken { get; set; }
        public string LastAccess { get; set; }
        public string PublicKey { get; set; }

        public UserEntity() { }
    }
}
