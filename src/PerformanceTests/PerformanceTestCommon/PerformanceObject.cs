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
        }
    }
}
