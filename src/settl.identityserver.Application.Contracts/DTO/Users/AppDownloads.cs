namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class AppDownloads
    {
        public Downloads Agents { get; set; }
        public Downloads Consumers { get; set; }
    }

    public class Downloads
    {
        public int AppStore { get; set; }
        public int PlayStore { get; set; }
    }
}