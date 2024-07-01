﻿using System.Collections;

namespace GameHook.Domain.Models;

public class ByteArrayProperty
{
    private IEnumerable<int>? _byteArray;
    public List<string> EditableArray { get; private set; } = [];
    public ByteArrayProperty(IEnumerable<int>? byteArray)
    {
        _byteArray = byteArray;
        CreateEditableArray();
    }

    private void CreateEditableArray()
    {
        if (_byteArray is null || !_byteArray.Any())
        {
            EditableArray = [];
            return;
        }
        EditableArray = _byteArray
            .Select(b => b.ToString("X2"))
            .ToList();
    }
    public IEnumerable<int>? GetByteArray()
        => _byteArray;

    public void UpdateByteArray()
    {
        //Should we allow users to create a new _byteArray? Need to look into this more
        if(_byteArray is null || !_byteArray.Any())
            return;
        try
        {
            _byteArray = EditableArray.Select(bS => 
                    int.Parse(bS, System.Globalization.NumberStyles.HexNumber))
                .AsEnumerable();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public static ByteArrayProperty? FromString(string? byteString)
    {
        if (string.IsNullOrWhiteSpace(byteString))
            return null;
        try
        {
            return new ByteArrayProperty(byteString.Split(' ')
                .Select(bS => int.Parse(bS, System.Globalization.NumberStyles.HexNumber)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    public override string ToString()
    {
        return _byteArray?
            .Select(b => b.ToString("X2"))
            .ToString() ?? "";
    }
}