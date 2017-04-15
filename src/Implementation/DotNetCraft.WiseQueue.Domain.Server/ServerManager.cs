using System;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.Specofications;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.Core.Domain.ServiceMessenger;
using DotNetCraft.Common.DataAccessLayer.Exceptions;
using DotNetCraft.Common.Domain.Management;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Managers;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.Core.ServiceMessages;
using DotNetCraft.WiseQueue.DataAccessLayer.Specifications;

namespace DotNetCraft.WiseQueue.Domain.Server
{
    public class ServerManager : BaseBackgroundManager<ServerManagerConfiguration>, IServerManager
    {
        private readonly IServiceMessageProcessor serviceMessageProcessor;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IContextSettings contextSettings;
        private readonly IServerRepository serverRepository;
        private int serverId;

        public ServerManager(IServiceMessageProcessor serviceMessageProcessor, IUnitOfWorkFactory unitOfWorkFactory, IServerRepository serverRepository, IContextSettings contextSettings, ServerManagerConfiguration serverManagerConfiguration)
            : base(serverManagerConfiguration)
        {
            if (serviceMessageProcessor == null)
                throw new ArgumentNullException(nameof(serviceMessageProcessor));
            if (unitOfWorkFactory == null)
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            if (contextSettings == null)
                throw new ArgumentNullException(nameof(contextSettings));
            if (serverRepository == null)
                throw new ArgumentNullException(nameof(serverRepository));

            this.serviceMessageProcessor = serviceMessageProcessor;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.contextSettings = contextSettings;
            this.serverRepository = serverRepository;
        }

        #region Overrides of BaseBackgroundManager<ServerManagerConfiguration>

        private void InsertNewServer(IUnitOfWork unitOfWork)
        {
            ServerInfo serverInfo = new ServerInfo
            {
                Name = Name,
                Description = "Some description",
                ServerExpiredTime = DateTime.UtcNow.Add(managerConfiguration.ServerHeartbeat)
            };
            unitOfWork.Insert(serverInfo);
            serverId = serverInfo.Id;
            ServerRegistrationMessage message = new ServerRegistrationMessage(serverId, this);
            serviceMessageProcessor.SendMessage(message);
        }

        private void UpdateServer(IUnitOfWork unitOfWork)
        {
            try
            {
                ServerInfo serverInfo = serverRepository.Get(serverId);
                serverInfo.ServerExpiredTime += managerConfiguration.ServerHeartbeat;
                unitOfWork.Update(serverInfo);
            }
            catch (EntityNotFoundException)
            {
                InsertNewServer(unitOfWork);
            }
        }

        private void DeleteExpiredServers(IUnitOfWork unitOfWork)
        {
            ISpecificationRequest<ServerInfo> specification = new SimpleSpecificationRequest<ServerInfo>();
            specification.Specification = new ExpiredServerSpecification();
            var servers = serverRepository.GetBySpecification(specification);
            foreach (ServerInfo serverInfo in servers)
            {
                unitOfWork.Delete(serverInfo);
            }
        }

        protected override void OnBackroundExecution()
        {
            using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
            {
                if (serverId == 0)
                {
                    InsertNewServer(unitOfWork);
                }
                else
                {
                    UpdateServer(unitOfWork);
                }

                DeleteExpiredServers(unitOfWork);
                unitOfWork.Commit();
            }
        }        

        protected override void OnBackroundException(Exception exception)
        {
            serverId = 0;

            ServerRegistrationMessage message = new ServerRegistrationMessage(serverId, this);
            serviceMessageProcessor.SendMessage(message);

            base.OnBackroundException(exception);
        }

        #endregion

        #region Overrides of BaseBackgroundManager<ServerManagerConfiguration>

        public override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (serverId > 0)
                    {
                        using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
                        {
                            unitOfWork.Delete<ServerInfo>(serverId);
                        }
                        serverId = 0;
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
