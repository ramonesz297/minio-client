namespace Minio.Client.Http
{
    public readonly struct MultipartCopyRequest
    {
        public MultipartCopyRequest(CopyRequest request, string uploadId = null, Part? part = null)
        {
            Request = request;
            UploadId = uploadId;
            Part = part;
        }

        public CopyRequest Request { get; }

        public string UploadId { get; }

        public Part? Part { get; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(UploadId) || !Part.HasValue)
            {
                return "";
            }
            return $"?uploadId={UploadId}&partNumber={Part.Value.PartId}";
        }
    }
}