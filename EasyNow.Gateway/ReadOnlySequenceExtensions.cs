using System.Buffers;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public static class ReadOnlySequenceExtensions
    {
        public static ReadOnlySequence<byte> ConcactSequence(this ReadOnlySequence<byte> first,
            ReadOnlySequence<byte> second, bool cloneSecond = true)
        {
            if (cloneSecond)
            {
                second=new ReadOnlySequence<byte>(second.ToArray());
            }
            SequenceSegment head = first.Start.GetObject() as SequenceSegment;
            SequenceSegment tail = first.End.GetObject() as SequenceSegment;
            
            if (head == null)
            {
                foreach (var segment in first)
                {                
                    if (head == null)
                        tail = head = new SequenceSegment(segment);
                    else
                        tail = tail.SetNext(segment);
                }
            }

            foreach (var segment in second)
            {
                if (tail == null)
                {
                    head=tail=new SequenceSegment(segment);
                }
                else
                {
                    tail = tail.SetNext(segment);
                }
            }

            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }
    }
}