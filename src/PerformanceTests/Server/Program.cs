using System;
using System.Configuration;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.Core.Domain.ServiceMessenger;
using DotNetCraft.Common.DataAccessLayer.UnitOfWorks.SimpleUnitOfWorks;
using DotNetCraft.Common.Domain.ServiceMessenger;
using DotNetCraft.Common.NLogger;
using DotNetCraft.Common.Utils.Logging;
using DotNetCraft.WiseQueue.Core.Caching;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Converters;
using DotNetCraft.WiseQueue.Core.Managers;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.DataAccessLayer;
using DotNetCraft.WiseQueue.DataAccessLayer.Repositories;
using DotNetCraft.WiseQueue.Domain.Common.Converters;
using DotNetCraft.WiseQueue.Domain.Server;
using DotNetCraft.WiseQueue.Domain.Server.Tasks;
using DotNetCraft.WiseQueue.MicrosoftExpressionCache;
using Ninject;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerConfiguration systemConfiguration = (ServerConfiguration)(dynamic)ConfigurationManager.GetSection("ServerConfiguration");
            Console.WriteLine(systemConfiguration.ToString());

            //We decided to use Ninject.
            IKernel kernel = new StandardKernel();

            //NLog
            LogManager.LoggerFactory = new CommonNLogLoggerFactory();

            //UseEntityFrameWork();
            kernel.Bind<IContextSettings>().ToConstant(systemConfiguration.SqlSettings).InSingletonScope();
            kernel.Bind<IDataContextFactory>().To<WiseQueueDataContextFactory>().InSingletonScope();

            //DataAccessLayer
            kernel.Bind<IServerRepository>().To<ServerRepository>().InSingletonScope();
            kernel.Bind<ITaskRepository>().To<TaskRepository>().InSingletonScope();

            //UnitOfWork
            kernel.Bind<IUnitOfWorkFactory>().To<UnitOfWorkFactory>().InSingletonScope();

            //Converters
            kernel.Bind<IExpressionConverter>().To<ExpressionConverter>().InSingletonScope();
            kernel.Bind<IJsonConverter>().ToConstructor(x => new JsonConverter()).InSingletonScope();
            //CachedExpressionCompiler
            kernel.Bind<ICachedExpressionCompiler>().To<CachedExpressionCompiler>().InSingletonScope();

            //TaskBuilder
            kernel.Bind<ITaskBuilder>().To<TaskBuilder>().InSingletonScope();

            //Managers' configuration
            kernel.Bind<ServerManagerConfiguration>().ToConstant(systemConfiguration.ServerManagerConfiguration).InSingletonScope();
            kernel.Bind<TaskManagerConfiguration>().ToConstant(systemConfiguration.TaskManagerConfiguration).InSingletonScope();
            kernel.Bind<TaskProcessingConfiguration>().ToConstant(systemConfiguration.TaskManagerConfiguration.TaskProcessingConfiguration).InSingletonScope();
            //Managers
            kernel.Bind<IServerManager>().To<ServerManager>().InSingletonScope();
            kernel.Bind<ITaskManager>().To<TaskManager>().InSingletonScope();
            kernel.Bind<ITaskProcessing>().To<TaskProcessing>().InSingletonScope();
            
            //ServiceMessageProcessor
            kernel.Bind<IServiceMessageProcessor>().To<ServiceMessageProcessor>().InSingletonScope();

            ITaskManager taskManager = kernel.Get<ITaskManager>();
            taskManager.Start();

            IServerManager serverManager = kernel.Get<IServerManager>();
            serverManager.Start();

            Console.ReadLine();
        }
    }
}
