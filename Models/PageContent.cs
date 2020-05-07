using System.ComponentModel.DataAnnotations.Schema;
using EZms.Core.Attributes;
using Newtonsoft.Json;

namespace EZms.Core.Models
{
    public class PageContent<T> : Content where T : class, new()
    {
        [NotMapped]
        [IgnoreProperty]
        public T Model { get; set; }

        [IgnoreProperty]
        public override string ModelAsJson
        {
            get => Model == null ? null : JsonConvert.SerializeObject(Model, Formatting.None);
            set => Model = string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full
            });
        }
    }
}
