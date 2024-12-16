using System;

namespace settl.identityserver.Application.Contracts.DTO
{
    public class EntityDTO
    {
        public DateTime CreatedOn { get; set; }
    }

    public class SettlBaseApiDTO
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }
    }
}