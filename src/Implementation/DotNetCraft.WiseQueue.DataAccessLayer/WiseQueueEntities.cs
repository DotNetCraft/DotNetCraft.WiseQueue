using System.Data.Entity;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.DataAccessLayer
{
    public class WiseQueueEntities : DbContext
    {
        // Your context has been configured to use a 'WiseQueueEntities' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'DotNetCraft.WiseQueue.Core.WiseQueueEntities' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'WiseQueueEntities' 
        // connection string in the application configuration file.
        public WiseQueueEntities()
            : base("name=WiseQueueEntities")
        {
        }

        public WiseQueueEntities(string connectionString): base(connectionString)
        {            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<TaskInfo>().Property(p => p.RowVersion).IsConcurrencyToken();
        }


        public virtual DbSet<ServerInfo> ServerInfoSet { get; set; }

        public virtual DbSet<TaskInfo> TaskInfoSet { get; set; }

        public virtual DbSet<ScheduleInfo> ScheduleInfoSet { get; set; }
    }
}