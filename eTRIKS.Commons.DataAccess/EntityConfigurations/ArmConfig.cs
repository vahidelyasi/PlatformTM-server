using eTRIKS.Commons.Core.Domain.Model.DesignElements;
using eTRIKS.Commons.DataAccess.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eTRIKS.Commons.DataAccess.EntityConfigurations
{
    public class ArmConfig : EntityTypeConfiguration<Arm>
    {
        public override void Configure(EntityTypeBuilder<Arm> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(2000);

            builder
                .Property(t => t.Code)
                .IsRequired();
            //.HasMaxLength(200);

            // Table & Column Mappings
            builder.ToTable("Arm_TBL");
            builder.Property(t => t.Id).HasColumnName("ArmId");
            builder.Property(t => t.Name).HasColumnName("ArmName");
            builder.Property(t => t.Code).HasColumnName("ArmCode");

            //CONSIDER WHEN M-2-M realtionships are supported again

            //builder.HasMany(t => t.Studies)
            //    .WithMany(t => t.Arms)
            //    .Map(mc =>
            //    {
            //        mc.ToTable("Study_Arms");
            //        mc.MapLeftKey("ArmId");
            //        mc.MapRightKey("StudyId");

            //    });
        }
    }
}
