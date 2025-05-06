using System.Globalization;

namespace PokeAByte.Domain.Models;

public class ByteArrayProperty
{
    private IEnumerable<int>? _byteArray;
    public IReadOnlyCollection<int>? ByteArray => _byteArray?.ToList().AsReadOnly();
    public List<string> EditableArray { get; private set; } = [];
    public ByteArrayProperty(IEnumerable<int>? byteArray, int? length)
    {
        _byteArray = byteArray;
        CreateEditableArray(length);
    }

    private void CreateEditableArray(int? length)
    {
        var len = length ?? 1;
        if (_byteArray is null || !_byteArray.Any())
        {
            EditableArray = [];
            return;
        }
        EditableArray = _byteArray
            .Select(b => b.ToString("X2"))
            .ToList();
        if (EditableArray.Count < len)
        {
            var lenRemaining = len - EditableArray.Count;
            var range = new List<string>();
            for (var i = 0; i < lenRemaining; i++)
            {
                range.Add(new string(""));
            }
            EditableArray.AddRange(range);
        }
    }
    public void UpdateByteArray()
    {
        //Should we allow users to create a new _byteArray? Need to look into this more
        if (_byteArray is null || !_byteArray.Any())
            return;
        try
        {
            _byteArray = EditableArray.Select(bS =>
                    int.TryParse(bS, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var bi) ?
                        bi : 0)
                .AsEnumerable();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    /*public static ByteArrayProperty? FromString(string? byteString)
    {
        if (string.IsNullOrWhiteSpace(byteString))
            return null;
        try
        {
            return new ByteArrayProperty(byteString.Split(' ')
                .Select(bS => int.Parse(bS, NumberStyles.HexNumber)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }*/
    public override string ToString()
    {
        var ba = _byteArray?
            .Select(b => b.ToString("X2")).ToList() ?? [];
        return string.Join(' ', ba);
    }
}