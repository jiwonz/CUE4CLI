using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Objects.Core.Misc;

class Program{
	private static async Task RunCommand(DefaultFileProvider provider, string[] arguments){
		string input;
		switch(arguments[0]) {
			case "ExtractFile":
				bool logOffOptimization = false;
				if (arguments.Length > 3){
					logOffOptimization = true;
				}
				for (int i = 1; i <= arguments.Length-1-1; i++){
					input = arguments[i];
					if(logOffOptimization==false) Console.Write("Getting File...");
					CUE4Parse.FileProvider.Objects.GameFile? value;
					provider.TryFindGameFile(input,out value);
					if(logOffOptimization==false) Console.Write("OK\n");
					if(logOffOptimization==false) Console.Write("Writing File...");
					await File.WriteAllBytesAsync(Path.Combine(arguments[^1], value.Name), await value!.ReadAsync());
					if(logOffOptimization==false) Console.Write("OK\n");
				}
				break;
			case "List":
				foreach (var path in provider.Files.Keys){
					if (path.StartsWith(arguments[1].ToLower())) {
						Console.WriteLine(path);
					}
				}
				break;
			case "SaveList":
				input = arguments[2];
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
				for (int i = 0; i < arguments.Length; i++){
					string arg = arguments[i];
					if (arg.StartsWith("\"") && arg.EndsWith("\"")){
						arguments[i] = arg.Substring(1, arg.Length - 2);
					}
				}
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
