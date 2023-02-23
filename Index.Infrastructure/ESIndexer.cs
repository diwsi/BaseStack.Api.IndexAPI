using Elasticsearch.Net;
using Nest;

namespace Index.Infrastructure
{
    public class ESIndexer : IIndexer
    {
        private readonly ElasticClient client;

        public ESIndexer(ElasticClient client)
        {
            this.client = client;


        }

        public async Task<bool> Index(string indexName, string id, object data)
        {
            await InitIndex(indexName);
            var resp = client.LowLevel.Index<StringResponse>(indexName, id, data.ToString());

            return resp.Success;
        }


        public async Task InitIndex(string indexName)
        {
            var resp = await client.Indices.ExistsAsync(indexName);
            if (!resp.Exists)
            {
                var indexResp = await client.Indices.CreateAsync(indexName);
                if (indexResp != null && !indexResp.IsValid)
                {
                    throw new IndexException($"Unable to create index '${indexName}'  EXP: {indexResp?.ServerError?.Error?.Reason}");
                }
            }
        }

    }
}
