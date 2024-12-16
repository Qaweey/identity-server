using AutoMapper.Configuration.Annotations;
using settl.identityserver.Domain.Shared.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace settl.identityserver.Domain.Shared
{
    public class Entity
    {
        [Key]
        [Ignore]
        public int Id { get; set; }

        public Entity()
        {
            CreatedOn = DateHelper.GetCurrentLocalTime();
            IsActive = true;
            IsDeleted = false;
        }

        [Ignore]
        [Column("created_on")]
        public DateTime CreatedOn { get; set; }

        [Column("updated_on")]
        public DateTime? UpdatedOn { get; set; }

        [Column("deleted_on")]
        public DateTime? DeletedOn { get; set; }

        [Column("active")]
        public bool IsActive { get; set; }

        [Column("deleted")]
        public bool IsDeleted { get; set; }
    }
}