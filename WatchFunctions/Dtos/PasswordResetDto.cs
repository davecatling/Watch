namespace WatchFunctions.Dtos
{
    public class PasswordResetDto
    {
        public string ChannelNumber { get; set; }
        public string Handle { get; set; }
        public string Password { get; set; }
        public byte[] Signature { get; set; }
    }
}
