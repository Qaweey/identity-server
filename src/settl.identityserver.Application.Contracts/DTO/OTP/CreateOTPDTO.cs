﻿using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.OTP
{
    public class CreateOTPDTO
    {
        [Required]
        public string OTPNumber { get; set; }

        public string Code { get; set; }

        [MinLength(11)]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}