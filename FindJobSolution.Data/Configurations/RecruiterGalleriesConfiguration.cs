﻿using FindJobSolution.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindJobSolution.Data.Configurations
{
    public class RecruiterGalleriesConfiguration : IEntityTypeConfiguration<RecruiterGalleries>
    {
        public void Configure(EntityTypeBuilder<RecruiterGalleries> builder)
        {
            builder.ToTable("RecruiterGalleries");
            
            builder.HasKey(x => x.RecruiterGalleriesId);

            builder.Property(x => x.RecruiterGalleriesId).UseIdentityColumn();

            builder.Property(x => x.src).IsRequired(true);

            builder.HasOne(x => x.Recruiter).WithMany(x => x.recruiterGalleries).HasForeignKey(x => x.RecruiterId);

            builder.Property(x => x.DateCreated);

            builder.Property(x => x.FileSize);

            builder.Property(x => x.Caption);
        }
    }
}
