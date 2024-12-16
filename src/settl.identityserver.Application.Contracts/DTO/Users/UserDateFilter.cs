using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;

namespace settl.identityserver.Application.Contracts.DTO.Users
{
    public class UserDateFilter
    {
        public int? Days { get; set; }

        /// <summary>
        /// 2021-02-01
        /// </summary>
        [SwaggerParameter("Example: 2021-01-01")]
        public string StartDate { get; set; } = "";

        [SwaggerParameter("Example: 2021-01-31")]
        public string EndDate { get; set; } = "";
    }

    public class UsersDateFilterResponse
    {
        public List<UserDTO> Users { get; set; }
        public List<UserDTO> PreviousUsers { get; set; }
    }
}