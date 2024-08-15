using Shared;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;


namespace Workers
{

    public class CodeRunner
    {
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("redis-server:6379"); // Update Redis connection string if necessary
        private static readonly IDatabase db = redis.GetDatabase();

        private static readonly Dictionary<string, string> Extensions = new()
        {
            { "cpp", "cpp" },
            { "c", "c" },
            { "java", "java" },
            { "python", "txt" },
            {"javascript","js" },
            { "csharp", "cs" },
            {"typescript", "ts" }
        };

        public static async Task RunCodeAsync(ApiBody apiBody)
        {
            try
            {
                await db.StringSetAsync(apiBody.Folder, "Processing");

                var filePath = Path.Combine("temp", apiBody.Folder, $"source.{Extensions[apiBody.Lang]}");
                Console.WriteLine(filePath);
                var command = $"python3 run.py {filePath} {apiBody.Lang} {apiBody.TimeOut}";

                // Create output.txt file
                var outputFilePath = Path.Combine("temp", apiBody.Folder, "output.txt");
                await File.WriteAllTextAsync(outputFilePath, string.Empty);
                Console.WriteLine("Output.txt created!");

                // Execute command
                var output = await ExecuteCommandAsync(command);

                // Read output file
                var data = await File.ReadAllTextAsync(outputFilePath);
                var result = new
                {
                    output = data,
                    stderr = output.stderr,
                    status = output.stdout,
                    submission_id = apiBody.Folder
                };

                Console.WriteLine(result);

                // Delete folder
                DeleteFolder(Path.Combine("temp", apiBody.Folder));

                await db.StringSetAsync(apiBody.Folder, JsonSerializer.Serialize(result), TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static async Task CreateFilesAsync(ApiBody apiBody)
        {
            try
            {
                Console.WriteLine("Creating file from consumer");
                var folderPath = Path.Combine("temp", apiBody.Folder);
                Directory.CreateDirectory(folderPath);

                await File.WriteAllTextAsync(Path.Combine(folderPath, "input.txt"), apiBody.Input);
                await File.WriteAllTextAsync(Path.Combine(folderPath, $"source.{Extensions[apiBody.Lang]}"), apiBody.Input);

                await RunCodeAsync(apiBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static async Task<(string stdout, string stderr)> ExecuteCommandAsync(string command)
        {
            var processInfo = new ProcessStartInfo("bash", $"-c \"{command}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = new Process { StartInfo = processInfo };
            process.Start();

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            return (stdout, stderr);
        }
    }

}
