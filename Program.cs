using Microsoft.Extensions.Configuration;
using System.Diagnostics;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

var action = true;
do
{
    var selectedBrowser = configuration[$"Browser:Default"];
    if (selectedBrowser == null)
    {
        // Browser Select
        Console.WriteLine("Which browser would you like to use?");
        Console.WriteLine();
        Console.WriteLine($"0 - Blisk");
        Console.WriteLine($"1 - Opera");

        var browserIndexText = Console.ReadLine();
        if (!int.TryParse(browserIndexText, out var browserIndex))
        {
            Console.Clear();
            Console.WriteLine("Please enter only number of browser!");
            Console.WriteLine();
            continue;
        }

        selectedBrowser = browserIndex == 0 ? "Blisk" : "Opera";
    }

    var browserPathConfig = configuration[$"Browser:{selectedBrowser}"]
        ?? throw new NullReferenceException("Browser program path not found!");

    // Check Profile Data
    var profilePathConfig = configuration["Profile"]
        ?? throw new NullReferenceException("Profile path not found!");

    var profilePath = Path.Combine(profilePathConfig, selectedBrowser.ToLower());

    // Profile Selection
    Console.WriteLine("Which profile would you like to use?");
    Console.WriteLine();

    Console.WriteLine($"0 - Create new one!");
    Console.WriteLine($"1 - Default");

    // Check exists profiles
    var profiles = Array.Empty<DirectoryInfo>();
    if (Directory.Exists(profilePath))
    {
        var profileDirectory = new DirectoryInfo(profilePath);
        profiles = profileDirectory.GetDirectories()
            .OrderBy(x => x.CreationTime)
            .ToArray();
        for (int i = 0; i < profiles.Length; i++)
        {
            var browser = profiles[i];
            Console.WriteLine($"{i + 2} - {browser.Name}");
        }
    }

    var selectedIndex = Console.ReadLine();
    if (!int.TryParse(selectedIndex, out var index))
    {
        Console.Clear();
        Console.WriteLine("Please enter only number of profile!");
        Console.WriteLine();
        continue;
    }

    // Open browser with default profile
    if (index == 1)
    {
        action = RunBrowser(browserPathConfig);
        continue;
    }

    // Create new profile
    if (index == 0)
    {
        Console.Clear();
        Console.WriteLine("Please enter the browser name.");
        var browserName = Console.ReadLine()?.Trim()?.ToLower();
        if (string.IsNullOrEmpty(browserName))
        {
            Console.Clear();
            Console.WriteLine("Please enter the valid name for browser!");
            Console.WriteLine();
            continue;
        }

        // Open browser
        action = RunBrowser(browserPathConfig, Path.Combine(profilePath, browserName));
        continue;
    }

    // Check browser number
    if (index > profiles.Length + 1)
    {
        Console.Clear();
        Console.WriteLine("Please enter a number for the existing browser!");
        Console.WriteLine();
        continue;
    }

    // Open browser
    action = RunBrowser(browserPathConfig, profiles[index - 2].FullName);
} while (action);

static bool RunBrowser(string browserPath, string? path = null)
{
    var startInfo = new ProcessStartInfo(browserPath);
    if (path != null)
        startInfo.Arguments = $"--user-data-dir={path}";

    _ = Process.Start(startInfo);

    return !true;
}