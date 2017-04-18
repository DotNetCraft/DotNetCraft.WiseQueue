using System;
using System.Data.Entity;
using System.Linq;
using DotNetCraft.Common.DataAccessLayer;
using DotNetCraft.WiseQueue.Core.Configurations;

namespace DotNetCraft.WiseQueue.DataAccessLayer
{
    public class WiseQueueDataContext : BaseDataContext
    {
        private readonly DbContext dbContext;

        public WiseQueueDataContext(WiseQueueDataContextFactory owner, SqlSettings sqlSettings): base(owner)
        {
            dbContext = new WiseQueueEntities(sqlSettings.ConnectionString);
        }

        private void LifeHach()
        {
            //The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            //for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            //Make sure the provider assembly is available to the running application. 
            //See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.

            //Stupid life hack - makes dbContext creation working.
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        /// <summary>
        /// Occurs when object is disposing.
        /// </summary>
        /// <param name="isDisposing">Flag shows that object is disposing or not.</param>
        protected override void OnDataContextDisposing(bool isDisposing)
        {
            if (isDisposing)
            {
                dbContext.Dispose();
            }
        }

        protected override IQueryable<TEntity> OnGetCollectionSet<TEntity>()
        {
            IQueryable<TEntity> result = dbContext.Set<TEntity>();
            return result;
        }

        protected override void OnInsert<TEntity>(TEntity entity, bool assignIdentifier = true)
        {
            var dbSet = dbContext.Set<TEntity>();
            dbSet.Add(entity);
            if (assignIdentifier)
                dbContext.SaveChanges(); //TODO: Can be a problem if a lot of items are inserted
        }

        protected override void OnUpdate<TEntity>(TEntity entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;            
        }

        protected override void OnDelete<TEntity>(object entityId)
        {
            var entity = Activator.CreateInstance<TEntity>(); //TODO: Where is ID???
            OnDelete(entity);
        }

        protected override void OnDelete<TEntity>(TEntity entity)
        {
            var dbSet = dbContext.Set<TEntity>();
            dbSet.Attach(entity);
            dbSet.Remove(entity);
        }

        protected override void OnBeginTransaction()
        {
            dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
        }

        protected override void OnCommit()
        {
            dbContext.SaveChanges();
            dbContext.Database.CurrentTransaction.Commit();
        }

        protected override void OnRollBack()
        {
            dbContext.Database.CurrentTransaction.Rollback();
        }
    }
}
