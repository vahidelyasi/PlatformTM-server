﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformTM.Core.JoinEntities;
using PlatformTM.Data.Extensions;

namespace PlatformTM.Data.EntityConfigurations
{
    public class ObservationSynonymConfig : EntityTypeConfiguration<ObservationSynonym>
    {
        public override void Configure(EntityTypeBuilder<ObservationSynonym> builder)
        {
            builder
                .HasKey(t => new { t.ObservationId, t.QualifierId });

            builder
               .ToTable("Observation_Synonyms");

            builder
                .HasOne(dd => dd.Qualifier)
                .WithMany()
                .HasForeignKey(dd => dd.QualifierId);

            builder
                .HasOne(dd => dd.Observation)
                .WithMany(o=>o.Synonyms)
                .HasForeignKey(dd => dd.ObservationId);
        }
    }
}
