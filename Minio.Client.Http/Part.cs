namespace Minio.Client.Http
{
    public readonly struct Part
    {
        public Part(long from, long to, int partId)
        {
            From = from;
            To = to;
            PartId = partId;
        }

        public long From { get; }

        public long To { get; }

        public int PartId { get; }
    }
}