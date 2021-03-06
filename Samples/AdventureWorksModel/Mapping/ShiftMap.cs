using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace AdventureWorksModel
{
    public class ShiftMap : EntityTypeConfiguration<Shift>
    {
        public ShiftMap()
        {
            // Primary Key
            this.HasKey(t => t.ShiftID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Shift", "HumanResources");
            this.Property(t => t.ShiftID).HasColumnName("ShiftID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Times.StartTime).HasColumnName("StartTime");
            this.Property(t => t.Times.EndTime).HasColumnName("EndTime");
            this.Property(t => t.ModifiedDate).HasColumnName("ModifiedDate");
        }
    }
}
