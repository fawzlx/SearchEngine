using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SearchEngine.Database.Entities;

public class InvertedIndexWordDocument
{
    public InvertedIndexWordDocument(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public InvertedIndexWordDocument(string key, object value)
    {
        Key = key;
        Value = JsonConvert.SerializeObject(value);
    }

    [Key]
    public int Id { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }
}