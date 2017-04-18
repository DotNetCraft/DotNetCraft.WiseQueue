using System;
using System.Configuration;
using System.Threading;
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
using DotNetCraft.WiseQueue.Domain.Client;
using DotNetCraft.WiseQueue.Domain.Common.Converters;
using DotNetCraft.WiseQueue.Domain.Common.Schedules;
using DotNetCraft.WiseQueue.Domain.Server;
using DotNetCraft.WiseQueue.Domain.Server.Tasks;
using DotNetCraft.WiseQueue.MicrosoftExpressionCache;
using Ninject;

namespace DotNetCraft.WiseQueue.Server
{
    class Program
    {

        private class MyClass
        {
            public void Test(string msg)
            {
                Console.WriteLine("MSG: {0}", msg);
            }

            public void VerySlowTask(string msg, CancellationToken cancellationToken)
            {
                while (true)
                {
                    Console.WriteLine("MSG: {0}", msg);
                    Thread.Sleep(1000);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Cancelling...");
                        break;
                    }
                    //cancellationToken.ThrowIfCancellationRequested();
                }
                Console.WriteLine("The VerySlowTask has been cancelled.");
            }

            public void ExceptionTask()
            {
                throw new NotImplementedException();
            }
        }


        static void Main(string[] args)
        {
            SystemConfiguration systemConfiguration = (SystemConfiguration)(dynamic)ConfigurationManager.GetSection("SystemConfiguration");
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
            kernel.Bind<IScheduleRepository>().To<ScheduleRepository>().InSingletonScope();

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
            //Client manager
            kernel.Bind<IClientManager>().To<ClientManager>().InSingletonScope();

            //ServiceMessageProcessor
            kernel.Bind<IServiceMessageProcessor>().To<ServiceMessageProcessor>().InSingletonScope();

            //Insert a new task.
            IClientManager clientManager = kernel.Get<IClientManager>();
            int taskId = clientManager.StartTask(() => new MyClass().Test("Hi"), new IntervalSchedule());
            //taskId = clientManager.StartTask(systemConfiguration.ServerManagerConfiguration.Queues[0], () => new MyClass().Test("Hi there"));
            //int taskId = clientManager.StartTask(systemConfiguration.ServerManagerConfiguration.Queues[1], () => new MyClass().VerySlowTask("Slow task", CancellationToken.None));

            ITaskManager taskManager = kernel.Get<ITaskManager>();
            taskManager.Start();

            IServerManager serverManager = kernel.Get<IServerManager>();
            serverManager.Start();

            Console.WriteLine("Press enter to stop task");
            Console.ReadLine();
            clientManager.StopTask(taskId);
            
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
