namespace JCFruit.WeebChat.Server.ServerEvents
{
    public class UserEvent<T>
    {
        public string UserId { get; set; }

        public T Body { get; set; }
    }
}
