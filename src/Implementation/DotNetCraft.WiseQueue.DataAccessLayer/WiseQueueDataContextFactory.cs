using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.DataAccessLayer;
using DotNetCraft.WiseQueue.Core.Configurations;

namespace DotNetCraft.WiseQueue.DataAccessLayer
{
    public class WiseQueueDataContextFactory: DataContextFactory<WiseQueueDataContext, SqlSettings>, IDataContextFactory
    {
        #region Overrides of DataContextFactory<WiseQueueDataContext,SqlSettings>

        protected override WiseQueueDataContext OnCreateDataContext(SqlSettings sqlSettings)
        {
            WiseQueueDataContext result = new WiseQueueDataContext(this, sqlSettings);
            return result;
        }

        #endregion
    }
}
