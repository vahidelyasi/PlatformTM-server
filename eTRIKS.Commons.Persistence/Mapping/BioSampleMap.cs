using System.Data.Entity.ModelConfiguration;
using eTRIKS.Commons.Core.Domain.Model;

namespace eTRIKS.Commons.Persistence.Mapping
{
    public class BioSampleMap : EntityTypeConfiguration<Biosample>
    {
        public BioSampleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            this.Property(t => t.Id)
               .IsRequired();
               //.HasMaxLength(200);


            //this.Property(t => t.DataFile)
            //    .HasMaxLength(2000);

            //this.Property(t => t.ActivityId)
            //    .IsRequired()
            //    .HasMaxLength(200);

           
            // Table & Column Mappings
            this.ToTable("BioSamples_TBL");
            //this.Property(t => t.DomainId).HasColumnName("domainId");
            //this.Property(t => t.DataFile).HasColumnName("DataFile");
            //this.Property(t => t.ActivityId).HasColumnName("ActivityId");
            this.Property(t => t.Id).HasColumnName("BioSampleDBId");

            // Relationships
            //this.HasRequired(d => d.Activity)
            //    .WithMany(a => a.Datasets)
            //    .HasForeignKey(d => d.ActivityId);

            //this.HasRequired(d => d.Domain)
            //    .WithMany()
            //    .HasForeignKey(t => t.DomainId);



        }
    }
}
