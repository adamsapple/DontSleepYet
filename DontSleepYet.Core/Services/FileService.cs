using System.Text;

using DontSleepYet.Core.Contracts.Services;

using Newtonsoft.Json;

namespace DontSleepYet.Core.Services;

public class FileService : IFileService
{
    public T Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        if (File.Exists(path))
        {
            mut.WaitOne();
            
            var json = File.ReadAllText(path);
            
            mut.ReleaseMutex();

            return JsonConvert.DeserializeObject<T>(json);
        }

        return default;
    }

    private readonly Mutex mut = new();

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonConvert.SerializeObject(content);

        mut.WaitOne();

        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
        
        mut.ReleaseMutex();
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }
}
