using System;

namespace PerformanceTestCommon
{
    public class PerformanceObject
    {
        public void Execute(string clientName)
        {
            using (PerformanceTestModel context = new PerformanceTestModel())
            {
                MyEntity myEntity = new MyEntity
                {
                    ClientName = clientName,
                };
                context.MyEntities.Add(myEntity);
                context.SaveChanges();
            }

            //throw new Exception("Just an exception");
        }
    }
}
