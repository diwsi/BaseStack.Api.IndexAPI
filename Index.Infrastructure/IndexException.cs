using Exceptions;

namespace Index.Infrastructure
{
    public class IndexException:BaseException
    {
        public IndexException(string message):base(message)
        {

        }
    }
}
