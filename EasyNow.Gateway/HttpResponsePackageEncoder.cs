using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public class HttpResponsePackageEncoder:IPackageEncoder<HttpResponse>
    {
        public int Encode(IBufferWriter<byte> writer, HttpResponse pack)
        {
            foreach (var memory in pack.Data)
            {
                writer.Write(memory.Span);
            }

            return (int)pack.Data.Length;
        }
    }
}