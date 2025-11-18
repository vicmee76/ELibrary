using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELibrary.Infrastructure.Models
{
    internal class DoaSearchResponseModel
    {
        public List<DoaResults> doaResults {  get; set; }
    }


    internal class DoaResults
    {
        [JsonProperty("uuid")]
        public string Uuid;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("handle")]
        public string Handle;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("expand")]
        public List<string> Expand;

        [JsonProperty("lastModified")]
        public string LastModified;

        [JsonProperty("parentCollection")]
        public object ParentCollection;

        [JsonProperty("parentCollectionList")]
        public object ParentCollectionList;

        [JsonProperty("parentCommunityList")]
        public object ParentCommunityList;

        [JsonProperty("bitstreams")]
        public object Bitstreams;

        [JsonProperty("withdrawn")]
        public string Withdrawn;

        [JsonProperty("archived")]
        public string Archived;

        [JsonProperty("link")]
        public string Link;

        [JsonProperty("metadata")]
        public List<DoaMetaData> Metadata;
    }



    internal class DoaMetaData
    {
        [JsonProperty("key")]
        public string Key;

        [JsonProperty("value")]
        public object Value;

        [JsonProperty("language")]
        public string Language;

        [JsonProperty("schema")]
        public string Schema;

        [JsonProperty("element")]
        public string Element;

        [JsonProperty("qualifier")]
        public string Qualifier;

        [JsonProperty("code")]
        public string Code;
    }
}
