using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Objects.Core.Misc;
using System.Linq;
using System.IO;

class Program{
	private static async Task RunCommand(DefaultFileProvider provider, string[] arguments){
		switch(arguments[0]) {
			case "ExtractFile":
				string output = arguments[2];
				if (output.StartsWith("\"") && output.EndsWith("\"")){
					output = output.Substring(1, output.Length - 2);
				}
				Console.Write("Getting File...");
				CUE4Parse.FileProvider.Objects.GameFile? value;
				provider.TryFindGameFile(arguments[1]!,out value);
				Console.Write("OK\n");
				Console.Write("Writing File...");
				await File.WriteAllBytesAsync(output!, await value!.ReadAsync());
				Console.Write("OK\n");
				break;
			case "List":
				foreach (var path in provider.Files.Keys){
					if (path.StartsWith(arguments[1].ToLower())) {
						Console.WriteLine(path);
					}
				}
				break;
			case "SaveList":
				string input = arguments[2];
				if (input.StartsWith("\"") && input.EndsWith("\"")){
					input = input.Substring(1, input.Length - 2);
				}
				Console.Write("Writing List Into "+input+"...");
				using (StreamWriter writer = new StreamWriter(input)){
					foreach (var path in provider.Files.Keys){
						if (path.StartsWith(arguments[1].ToLower())) {
							writer.WriteLine(path);
						}
					}
				}
				Console.Write("OK\n");
				break;
			default:
				Console.WriteLine("Invalid command"+" '"+arguments[0]+"'");
				Console.WriteLine("Available commands: ExtractFile <GameFile Path> <Output File>, List <Directory Path>, SaveList <Directory Path> <Output File> Quit");
				break;
		}
	}
    static async Task Main(string[] args){
		if (args.Length < 2){
			Console.WriteLine("Usage:\n\tcue4cli.exe <Command> <Paks Directory> <AES Key> <Arguments>\n\nCommands:\n\tCLI\tUses interactive Command Line Interface\n\tExtractFile\tExtracts GameFile by path\n\tList\tLists GameFile paths in directory path\n\tSaveList\tSaves the list of GameFile paths in directory path");
			return;
		}

		Console.Write("Reading Paks...");
		var provider = new DefaultFileProvider(args[1], SearchOption.TopDirectoryOnly, true, new VersionContainer(EGame.GAME_UE4_LATEST));
		provider.Initialize();
		if (args.Length > 2) provider.SubmitKey(new FGuid(), new FAesKey(args[2]));
		Console.Write("OK\n");

		if (args[0] == "CLI") {
			bool run = true;
			while (run) {
				Console.Write("> ");
				string? command = Console.ReadLine();
				string[] arguments = command!.Split(" ");
				switch(arguments[0]){
					case "Quit":
						run = false;
						break;
					default:
						await RunCommand(provider,arguments);
						break;
				}
			}
		} else {
			string[] cmd = {args[0]};
			await RunCommand(provider,cmd.Concat(args.Skip(3).ToArray()).ToArray());
		}
	}
}
