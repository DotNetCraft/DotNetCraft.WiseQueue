using System;
using System.Reflection;
using DotNetCraft.WiseQueue.Core.Converters;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Domain.Server.Tasks
{
    public class TaskBuilder: ITaskBuilder
    {
        private readonly IExpressionConverter expressionConverter;
        private readonly IJsonConverter jsonConverter;

        public TaskBuilder(IExpressionConverter expressionConverter, IJsonConverter jsonConverter)
        {
            if (expressionConverter == null)
                throw new ArgumentNullException(nameof(expressionConverter));
            if (jsonConverter == null)
                throw new ArgumentNullException(nameof(jsonConverter));

            this.expressionConverter = expressionConverter;
            this.jsonConverter = jsonConverter;
        }

        #region Implementation of ITaskBuilder

        public IRunningTask Build(TaskInfo taskInfo)
        {
            string instanceTypeJson = jsonConverter.ConvertFromJson<string>(taskInfo.InstanceType);
            Type instanceType = Type.GetType(instanceTypeJson, throwOnError: true, ignoreCase: true);
            Type[] argumentTypes = jsonConverter.ConvertFromJson<Type[]>(taskInfo.ParametersTypes);
            MethodInfo method = expressionConverter.GetNonOpenMatchingMethod(instanceType, taskInfo.Method, argumentTypes);

            string[] serializedArguments = jsonConverter.ConvertFromJson<string[]>(taskInfo.Arguments);

            object[] arguments = expressionConverter.DeserializeArguments(method, serializedArguments);


            //TODO: Activate and run task (Smart logic)
            var instance = Activator.CreateInstance(instanceType);

            return new RunningTask(taskInfo, instance, method, argumentTypes, arguments);
        }

        #endregion
    }
}
