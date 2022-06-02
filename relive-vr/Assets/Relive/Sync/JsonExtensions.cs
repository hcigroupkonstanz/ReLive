using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Relive.Sync
{
    public static class JsonExtensions
    {
        // Buffer sized as recommended by Bradley Grainger, https://faithlife.codes/blog/2012/06/always-wrap-gzipstream-with-bufferedstream/
        // Do not use a buffer larger than 85,000 bytes since objects larger than that go on the large object heap.  See:
        // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap
        // Renting a larger array would also be an option, see https://docs.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1.rent?view=netcore-3.1
        const int BufferSize = 8192;

        public static void SerializeToFileCompressed(object value, string path, JsonSerializerSettings settings)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                SerializeCompressed(value, fs, settings);
        }

        public static void SerializeCompressed(object value, Stream stream, JsonSerializerSettings settings)
        {
            using (var compressor = new GZipStream(stream, CompressionMode.Compress))
            using (var writer = new StreamWriter(compressor, Encoding.UTF8, BufferSize))
            {
                var serializer = JsonSerializer.CreateDefault(settings);
                serializer.Serialize(writer, value);
            }
        }

        public static T DeserializeFromFileCompressed<T>(string path, JsonSerializerSettings settings = null)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return DeserializeCompressed<T>(fs, settings);
        }

        public static T DeserializeCompressed<T>(Stream stream, JsonSerializerSettings settings = null)
        {
            using (var compressor = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(compressor))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = JsonSerializer.CreateDefault(settings);
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}
