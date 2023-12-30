using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Database.Entities;

public class InvertedIndexWordDocument
{
    public InvertedIndexWordDocument(string key, string value)
    {
        Key = key;
        Value = value;
    }

    [Key]
    public int Id { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }
}