using System;
using System.Configuration;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.DataAccessLayer.UnitOfWorks.SimpleUnitOfWorks;
using DotNetCraft.Common.NLogger;
using DotNetCraft.Common.Utils.Logging;
using DotNetCraft.WiseQueue.Core.Caching;
using DotNetCraft.WiseQueue.Core.Converters;
using DotNetCraft.WiseQueue.Core.Managers;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.DataAccessLayer;
using DotNetCraft.WiseQueue.DataAccessLayer.Repositories;
using DotNetCraft.WiseQueue.Domain.Client;
using DotNetCraft.WiseQueue.Domain.Common.Converters;
using DotNetCraft.WiseQueue.MicrosoftExpressionCache;
using Ninject;
using PerformanceTestCommon;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] {"20", "Client1"};
            }
            int count = Convert.ToInt32(args[0]);
            string clientName = args[1];

            ClientConfiguration systemConfiguration = (ClientConfiguration)(dynamic)ConfigurationManager.GetSection("ClientConfiguration");

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

            //Managers' configuration
            //TODO: Client manager configuration
            //Client manager
            kernel.Bind<IClientManager>().To<ClientManager>().InSingletonScope();

            for (int i = 0; i < count; i++)
            {

                //Insert a new task.
                IClientManager clientManager = kernel.Get<IClientManager>();
                int taskId = clientManager.StartTask(() => new PerformanceObject().Execute(clientName));
            }            
        }
    }
}
