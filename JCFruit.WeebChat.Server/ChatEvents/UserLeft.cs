namespace JCFruit.WeebChat.Server.ChatEvents
{
    public class UserLeft
    {
        public LeftReason Reason { get; }

        public UserLeft(LeftReason reason)
        {
            Reason = reason;
        }
    }

    public enum LeftReason
    {
        Voluntarily = 0,
        Inactivity = 1,
        Kicked = 2,
        Disconnected = 4
    }
}
