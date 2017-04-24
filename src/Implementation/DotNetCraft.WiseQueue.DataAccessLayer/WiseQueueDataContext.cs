using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks;
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

        protected override ICollection<TEntity> OnExecuteQuery<TEntity>(string query, DataBaseParameter[] args)
        {
            ICollection<TEntity> result;
            
            if (args != null && args.Length > 0)
            {
                object[] parameters = new SqlParameter[args.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    DataBaseParameter dataBaseParameter = args[i];
                    string parameterName = dataBaseParameter.ParameterName;

                    Type argumentType = dataBaseParameter.ParameterValue.GetType();

                    SqlDbType dbType = SqlDbType.Int;

                    if (argumentType == typeof(string))
                        dbType = SqlDbType.NVarChar;
                    if (argumentType == typeof(DateTime))
                        dbType = SqlDbType.DateTime;

                    parameters[i] = new SqlParameter(parameterName, dbType)
                    {
                        Value = dataBaseParameter.ParameterValue
                    };
                }
                
                result = dbContext.Database.SqlQuery<TEntity>(query, parameters).ToList();
            }
            else
            {
                result = dbContext.Database.SqlQuery<TEntity>(query).ToList();
            }

            return result;
        }

        protected override void OnBeginTransaction()
        {
            dbContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted);
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
