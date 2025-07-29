using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using PlatformPlatform.DeveloperCli.Installation;
using PlatformPlatform.DeveloperCli.Utilities;
using Spectre.Console;

namespace PlatformPlatform.DeveloperCli.Commands;

/// <summary>
///     Command to manage Aspire AppHost lifecycle - start, stop, and monitor the application host.
/// </summary>
public class WatchCommand : Command
{
    private const int AspirePort = 9001;
    private const int DashboardPort = 9097;
    private const int ResourceServicePort = 9098;

    public WatchCommand() : base("watch", "Manages Aspire AppHost operations")
    {
        AddOption(new Option<bool>(["--force"], "Force start a fresh Aspire AppHost instance, stopping any existing one"));
        AddOption(new Option<bool>(["--stop"], "Stop any running Aspire AppHost instance without starting a new one"));
        AddOption(new Option<bool>(["--attach", "-a"], "Keep the CLI process attached to the Aspire process"));
        AddOption(new Option<bool>(["--detach", "-d"], "Run the Aspire process in detached mode (background)"));
        AddOption(new Option<string?>(["--public-url"], "Set the PUBLIC_URL environment variable for the app (e.g., https://example.ngrok-free.app)"));

        Handler = CommandHandler.Create<bool, bool, bool, bool, string?>(Execute);
    }

    private static void Execute(bool force, bool stop, bool attach, bool detach, string? publicUrl)
    {
        Prerequisite.Ensure(Prerequisite.Dotnet, Prerequisite.Node, Prerequisite.Docker);

        var isRunning = IsAspireRunning();

        if (stop)
        {
            StopAspire();
            return;
        }

        // Validate that either --attach or --detach is specified (but not both)
        if (attach == detach)
        {
            AnsiConsole.MarkupLine("[red]You must specify either --attach (-a) or --detach (-d) mode.[/]");
            Environment.Exit(1);
        }

        if (isRunning)
        {
            if (!force)
            {
                AnsiConsole.MarkupLine($"[yellow]Aspire AppHost is already running on port {AspirePort}. Use --force to force a fresh start or --stop to stop it.[/]");
                Environment.Exit(1);
            }

            StopAspire();
        }

        StartAspireAppHost(attach, publicUrl);
    }

    private static bool IsAspireRunning()
    {
        // Check the main Aspire port
        if (Configuration.IsWindows)
        {
            // Windows: Check all Aspire ports
            var aspirePortsToCheck = new[] { AspirePort, DashboardPort, ResourceServicePort };
            foreach (var port in aspirePortsToCheck)
            {
                var portCheckCommand = $"""powershell -Command "Get-NetTCPConnection -LocalPort {port} -State Listen -ErrorAction SilentlyContinue" """;
                var result = ProcessHelper.StartProcess(portCheckCommand, redirectOutput: true, exitOnError: false);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    return true;
                }
            }
        }
        else
        {
            // macOS/Linux: Original logic - only check main port
            var portCheckCommand = $"lsof -i :{AspirePort} -sTCP:LISTEN -t";
            var result = ProcessHelper.StartProcess(portCheckCommand, redirectOutput: true, exitOnError: false);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return true;
            }
        }

        // Also check if there are any dotnet watch processes running AppHost
        if (Configuration.IsWindows)
        {
            // Check if any dotnet.exe processes are running with AppHost in the command line
            var watchProcesses = ProcessHelper.StartProcess("""powershell -Command "Get-Process dotnet -ErrorAction SilentlyContinue | Where-Object {$_.CommandLine -like '*watch*AppHost*'} | Select-Object Id" """, redirectOutput: true, exitOnError: false);
            return !string.IsNullOrWhiteSpace(watchProcesses) && watchProcesses.Contains("Id");
        }
        else
        {
            var watchProcesses = ProcessHelper.StartProcess("pgrep -f dotnet.*watch.*AppHost", redirectOutput: true, exitOnError: false);
            return !string.IsNullOrWhiteSpace(watchProcesses);
        }
    }

    private static void StopAspire()
    {
        AnsiConsole.MarkupLine("[blue]Stopping Aspire AppHost and all related services...[/]");

        if (Configuration.IsWindows)
        {
            // Step 1: Kill processes using our ports
            var ports = new[] { AspirePort, DashboardPort, ResourceServicePort };
            foreach (var port in ports)
            {
                ProcessHelper.StartProcess($"""cmd /c "netstat -ano | findstr :{port} | findstr LISTENING" """, redirectOutput: true, exitOnError: false);
                ProcessHelper.StartProcess($"""cmd /c "for /f "tokens=5" %a in ('netstat -aon ^| findstr :{port} ^| findstr LISTENING') do taskkill /f /pid %a" """, redirectOutput: true, exitOnError: false);
            }

            // Step 2: Kill dotnet watch processes
            // This approach finds all dotnet.exe processes and terminates them
            // The key insight is that when dotnet watch is waiting, it's still a dotnet.exe process
            ProcessHelper.StartProcess("""taskkill /F /IM dotnet.exe""", redirectOutput: true, exitOnError: false);

            // Also kill any remaining Aspire-related processes by searching for command line
            ProcessHelper.StartProcess("""wmic process where "name='dotnet.exe' and commandline like '%watch%'" delete""", redirectOutput: true, exitOnError: false);
            ProcessHelper.StartProcess("""wmic process where "commandline like '%AppHost%'" delete""", redirectOutput: true, exitOnError: false);
            ProcessHelper.StartProcess("""wmic process where "commandline like '%Aspire%'" delete""", redirectOutput: true, exitOnError: false);

            // Step 3: Kill specific Aspire-related processes
            var processesToKill = new[] { "PlatformPlatform.AppHost", "Aspire.Dashboard", "dcp", "dcpproc" };
            foreach (var processName in processesToKill)
            {
                ProcessHelper.StartProcess($"taskkill /F /IM {processName}.exe", redirectOutput: true, exitOnError: false);
            }
        }
        else
        {
            // Kill all dotnet watch processes that are running AppHost
            // This handles both absolute and relative paths
            ProcessHelper.StartProcess("pkill -9 -f dotnet.*watch.*AppHost", redirectOutput: true, exitOnError: false);

            // Kill all processes that contain our application folder path
            // This catches any process started from our directory
            ProcessHelper.StartProcess($"pkill -9 -f {Configuration.ApplicationFolder}", redirectOutput: true, exitOnError: false);

            // Kill Aspire-specific processes (Dashboard, DCP, etc.)
            ProcessHelper.StartProcess("pkill -9 -if aspire", redirectOutput: true, exitOnError: false);
            ProcessHelper.StartProcess("pkill -9 -f dcp", redirectOutput: true, exitOnError: false);

            // Kill processes by project names in case they're running from different locations
            // Find all subdirectories in the application folder and kill matching processes
            var applicationProjects = Directory.GetDirectories(Configuration.ApplicationFolder)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name));

            foreach (var projectName in applicationProjects)
            {
                ProcessHelper.StartProcess($"pkill -9 -f {projectName}", redirectOutput: true, exitOnError: false);
            }
        }

        // Wait a moment for processes to terminate
        Thread.Sleep(TimeSpan.FromSeconds(2));

        // On Windows, do one final check and cleanup of any remaining processes on our ports
        if (Configuration.IsWindows)
        {
            var ports = new[] { AspirePort, DashboardPort, ResourceServicePort };
            foreach (var port in ports)
            {
                ProcessHelper.StartProcess($"""cmd /c "for /f "tokens=5" %a in ('netstat -aon ^| findstr :{port} ^| findstr LISTENING') do taskkill /f /pid %a" """, redirectOutput: true, exitOnError: false);
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        AnsiConsole.MarkupLine("[green]Aspire AppHost stopped successfully.[/]");
    }

    private static void StartAspireAppHost(bool attach, string? publicUrl)
    {
        AnsiConsole.MarkupLine($"[blue]Starting Aspire AppHost in {(attach ? "attached" : "detached")} mode...[/]");

        if (publicUrl is not null)
        {
            AnsiConsole.MarkupLine($"[blue]Using PUBLIC_URL: {publicUrl}[/]");

            // Check if this is an ngrok URL and start ngrok if needed
            if (publicUrl.Contains(".ngrok-free.app", StringComparison.OrdinalIgnoreCase) ||
                publicUrl.Contains(".ngrok.io", StringComparison.OrdinalIgnoreCase))
            {
                StartNgrokIfNeeded(publicUrl);
            }
        }

        var appHostProjectPath = Path.Combine(Configuration.ApplicationFolder, "AppHost", "AppHost.csproj");
        var command = $"dotnet watch --non-interactive --project {appHostProjectPath}";

        if (!attach && Configuration.IsWindows)
        {
            // For Windows in detached mode, use "start" command to truly detach
            var detachedCommand = $"cmd /c start \"Aspire AppHost\" /min {command}";
            if (publicUrl is not null)
            {
                ProcessHelper.StartProcess($"{detachedCommand} --environment PUBLIC_URL={publicUrl}", Configuration.ApplicationFolder, waitForExit: false);
            }
            else
            {
                ProcessHelper.StartProcess(detachedCommand, Configuration.ApplicationFolder, waitForExit: false);
            }

            // Give it a moment to start
            Thread.Sleep(2000);
            AnsiConsole.MarkupLine("[green]Aspire AppHost started in detached mode.[/]");
        }
        else
        {
            // Attached mode or non-Windows
            if (publicUrl is not null)
            {
                ProcessHelper.StartProcess(command, Configuration.ApplicationFolder, waitForExit: attach, environmentVariables: ("PUBLIC_URL", publicUrl));
            }
            else
            {
                ProcessHelper.StartProcess(command, Configuration.ApplicationFolder, waitForExit: attach);
            }
        }
    }

    private static void StartNgrokIfNeeded(string publicUrl)
    {
        // First check if ngrok is installed
        var ngrokVersion = ProcessHelper.StartProcess("ngrok version", redirectOutput: true, exitOnError: false);
        if (!ngrokVersion.Contains("ngrok version", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[yellow]Ngrok is not installed. Please install ngrok from https://ngrok.com/download[/]");
            AnsiConsole.MarkupLine("[yellow]Continuing without ngrok tunnel...[/]");
            return;
        }

        // Extract the subdomain from the URL
        var uri = new Uri(publicUrl);
        var subdomain = uri.Host.Split('.')[0];

        // Check if ngrok is already running
        var isNgrokRunning = false;

        if (Configuration.IsWindows)
        {
            var ngrokProcesses = ProcessHelper.StartProcess("""tasklist /FI "IMAGENAME eq ngrok.exe" """, redirectOutput: true, exitOnError: false);
            isNgrokRunning = ngrokProcesses.Contains("ngrok.exe");
        }
        else
        {
            var ngrokProcesses = ProcessHelper.StartProcess("pgrep -f ngrok", redirectOutput: true, exitOnError: false);
            isNgrokRunning = !string.IsNullOrEmpty(ngrokProcesses);
        }

        if (isNgrokRunning)
        {
            AnsiConsole.MarkupLine("[yellow]Ngrok is already running.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[blue]Starting ngrok tunnel...[/]");

        // Start ngrok in detached mode
        var ngrokCommand = $"ngrok http --url={subdomain}.ngrok-free.app https://localhost:9000";

        if (Configuration.IsWindows)
        {
            ProcessHelper.StartProcess($"start /B {ngrokCommand}", waitForExit: false);
        }
        else
        {
            // Use shell to handle backgrounding properly
            ProcessHelper.StartProcess($"sh -c \"{ngrokCommand} > /dev/null 2>&1 &\"", waitForExit: false);
        }

        AnsiConsole.MarkupLine("[green]Ngrok tunnel started successfully.[/]");
    }
}
