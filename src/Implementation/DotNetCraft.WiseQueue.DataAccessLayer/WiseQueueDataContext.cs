using System;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using DotNetCraft.Common.DataAccessLayer;
using DotNetCraft.WiseQueue.Core.Configurations;

namespace DotNetCraft.WiseQueue.DataAccessLayer
{
    public class WiseQueueDataContext : BaseDataContext

    {
        private readonly DbContext dbContext;
        private TransactionScope transactionScope;

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
                if (transactionScope != null)
                {
                    transactionScope.Dispose();
                    transactionScope = null;
                }
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
            var dbSet = dbContext.Set<TEntity>();

            dbContext.Entry(entity).State = EntityState.Modified;
            //dbSet.Attach(entity);
        }

        protected override void OnDelete<TEntity>(object entityId)
        {
            var entity = Activator.CreateInstance<TEntity>();
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
            transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions {IsolationLevel = IsolationLevel.RepeatableRead});
        }

        protected override void OnCommit()
        {
            dbContext.SaveChanges();
            transactionScope.Complete();
        }

        protected override void OnRollBack()
        {
            transactionScope.Dispose();
            transactionScope = null;
        }
    }
}
