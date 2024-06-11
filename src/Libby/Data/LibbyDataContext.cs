using Libby.Data.Models;
using Libby.Data.Sagas;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Libby.Data;

public sealed class LibbyDataContext(DbContextOptions<LibbyDataContext> options) : DbContext(options)
{
    public DbSet<Library> Libraries => Set<Library>();

    public DbSet<LibraryScan> LibraryScans => Set<LibraryScan>();

    public DbSet<LibraryItem> LibraryItems => Set<LibraryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LibraryScan>()
            .HasKey(ls => ls.CorrelationId);

        modelBuilder.Entity<LibraryItem>()
            .Property(mi => mi.FfprobeDate)
            .HasColumnName("ffprobe_date");

        modelBuilder.Entity<LibraryItem>()
            .Property(mi => mi.FFprobeError)
            .HasColumnName("ffprobe_error");

        modelBuilder.Entity<LibraryItem>()
            .Property(mi => mi.FfprobeJson)
            .HasColumnName("ffprobe_json");

        var converter = new ValueConverter<byte[]?, long>(
            v => v == null ? 0 : BitConverter.ToInt64(v, 0),
            v => BitConverter.GetBytes(v));

        modelBuilder.AddInboxStateEntity(c =>
        {
            c.ToTable("mt_inbox_states");

            c.Property(p => p.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .HasConversion(
                    converter,
                    new ValueComparer<byte[]>(
                        (obj, otherObj) => ReferenceEquals(obj, otherObj),
                        obj => obj.GetHashCode(),
                        obj => obj));
        });

        modelBuilder.AddOutboxMessageEntity(c => c.ToTable("mt_outbox_messages"));

        modelBuilder.AddOutboxStateEntity(c =>
        {
            c.ToTable("mt_outbox_states");

            c.Property(p => p.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .HasConversion(
                    converter,
                    new ValueComparer<byte[]>(
                        (obj, otherObj) => ReferenceEquals(obj, otherObj),
                        obj => obj.GetHashCode(),
                        obj => obj));
        });
    }
}
