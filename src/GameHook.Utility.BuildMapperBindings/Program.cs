using GameHook.Application;
using GameHook.Domain;
using GameHook.Utility.BuildMapperBindings;

// Load XML file paths.
var mapperInputDirectoryPath = Path.GetFullPath($"{AppContext.BaseDirectory}../../../../../../mappers");
var typescriptOutputDirectoryPath = Path.GetFullPath($"{AppContext.BaseDirectory}../../../../../../bindings/src");
var filePaths = Directory.GetFiles(mapperInputDirectoryPath, "*.xml", SearchOption.AllDirectories);

foreach (var xmlFilePath in filePaths)
{
    try
    {
        var instance = new GameHookFakeInstance();
        var contents = await File.ReadAllTextAsync(xmlFilePath);
        var mapper = GameHookMapperXmlFactory.LoadMapperFromFile(instance, xmlFilePath, contents);

        // Create child directory if not exists.
        if (mapper.Metadata.GamePlatform.Any(x => char.IsLetter(x) == false && char.IsNumber(x) == false))
        {
            throw new Exception("Invalid characters in game platform.");
        }

        Directory.CreateDirectory(Path.Combine(typescriptOutputDirectoryPath, mapper.Metadata.GamePlatform));

        var tsDirectory = Path.Combine(typescriptOutputDirectoryPath, mapper.Metadata.GamePlatform);
        Directory.CreateDirectory(tsDirectory);

        var tsFilePath = Path.Combine(tsDirectory, $"{Path.GetFileNameWithoutExtension(xmlFilePath).ToPascalCase()}.ts");

        // Generate typescript bindings.
        var tsResult = TsGenerator.FromMapper(contents);
        await File.WriteAllTextAsync(tsFilePath, tsResult);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"*** ERROR parsing {xmlFilePath} ***");
        Console.WriteLine(ex);
    }
}

Console.WriteLine("Done");