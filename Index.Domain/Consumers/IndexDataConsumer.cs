 
using Index.Infrastructure;
using MassTransit;
using MessageBusDomainEvents;

namespace Index.Domain.Consumers
{
    public class IndexDataConsumer : IConsumer<IndexData>
    {
        private readonly IPublishEndpoint eventBus;
        private readonly IIndexer indexer;

        public IndexDataConsumer(IPublishEndpoint eventBus, IIndexer indexer)
        {
            this.eventBus = eventBus;
            this.indexer = indexer;
        }
        public async Task Consume(ConsumeContext<IndexData> context)
        {
            var data = context.Message;

            if (!string.IsNullOrEmpty(data.Name) && data.Value != null)
            {
                var resp = await indexer.Index(data.Name, data.ID.ToString(), data.Value);
                if (!resp)
                {
                    throw new IndexException($"Unable to create Index {data.ID}");
                }
            }
            await eventBus.Publish(new DataIndexed()
            {
                ID = context.Message.ID
            });
        }
    }
}
