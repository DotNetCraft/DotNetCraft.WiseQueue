using System;
using System.Configuration;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.Core.Domain.Management;
using DotNetCraft.Common.Core.Domain.ServiceMessenger;
using DotNetCraft.Common.DataAccessLayer.UnitOfWorks.SimpleUnitOfWorks;
using DotNetCraft.Common.Domain.ServiceMessenger;
using DotNetCraft.Common.NLogger;
using DotNetCraft.Common.Utils.Logging;
using DotNetCraft.WiseQueue.Core.Caching;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Managers;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.DataAccessLayer;
using DotNetCraft.WiseQueue.DataAccessLayer.Repositories;
using DotNetCraft.WiseQueue.Domain.Server;
using DotNetCraft.WiseQueue.MicrosoftExpressionCache;
using Ninject;

namespace DotNetCraft.WiseQueue.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            SystemConfiguration systemConfiguration = (SystemConfiguration)(dynamic)ConfigurationManager.GetSection("SystemConfiguration");
            Console.WriteLine(systemConfiguration.ToString());

            //We decided to use Ninject.
            IKernel kernel = new StandardKernel();

            //NLog
            LogManager.LoggerFactory = new CommonNLogLoggerFactory();

            //UseEntityFrameWork();
            kernel.Bind<IContextSettings>().ToConstant(systemConfiguration.SqlSettings);
            kernel.Bind<IDataContextFactory>().To<WiseQueueDataContextFactory>();

            //AMS.DataAccessLayer
            kernel.Bind<IServerRepository>().To<ServerRepository>();
            //AMS.BusinessLayer
            //kernel.Bind<IAccountBusiness>().To<AccountBusiness>();
            //Mapper
            //kernel.Bind<IDotNetCraftMapper>().To<DotNetCraftAutoMapper>();

            //UnitOfWork
            kernel.Bind<IUnitOfWorkFactory>().To<UnitOfWorkFactory>();

            //Managers
            kernel.Bind<ServerManagerConfiguration>().ToConstant(systemConfiguration.ServerManagerConfiguration);
            kernel.Bind<TaskManagerConfiguration>().ToConstant(systemConfiguration.TaskManagerConfiguration);
            kernel.Bind<IServerManager>().To<ServerManager>();
            kernel.Bind<ITaskManager>().To<TaskManager>();

            //ServiceMessageProcessor
            kernel.Bind<IServiceMessageProcessor>().To<ServiceMessageProcessor>();

            //CachedExpressionCompiler
            kernel.Bind<ICachedExpressionCompiler>().To<CachedExpressionCompiler>();

            ITaskManager taskManager = kernel.Get<ITaskManager>();
            taskManager.Start();

            IServerManager serverManager = kernel.Get<IServerManager>();
            serverManager.Start();

            Console.ReadLine();
        }
    }
}
