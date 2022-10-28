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
    public class CvConfiguration : IEntityTypeConfiguration<Cv>
    {
        public void Configure(EntityTypeBuilder<Cv> builder)
        {
            builder.ToTable("Cv");

            builder.HasKey(x => x.CvId);

            builder.Property(x => x.CvId).UseIdentityColumn();

            builder.Property(x => x.Name);

            builder.Property(x => x.fileType);

            builder.Property(x => x.FileSize);

            builder.Property(x => x.Timespan);
            
            builder.Property(x => x.ViewCount).IsRequired().HasDefaultValue(0);

            builder.HasOne(x => x.JobSeeker).WithMany(x=>x.Cvs).HasForeignKey(x=>x.JobSeekerId);
        }
    }
}