using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Objects.Core.Misc;

class Program
{
    static async Task Main(string[] args)
    {
		if (args.Length < 1){
			Console.WriteLine("Usage:\n\tcue4cli.exe <Paks Directory> <AES Key>");
			return;
		}

		Console.Write("Reading Paks...");
		var provider = new DefaultFileProvider(args[0], SearchOption.TopDirectoryOnly, true, new VersionContainer(EGame.GAME_UE4_LATEST));
		provider.Initialize();
		if (args.Length > 1) provider.SubmitKey(new FGuid(), new FAesKey(args[1]));
		Console.Write("OK\n");

		bool run = true;
		while (run) {
			Console.Write("> ");
			string? command = Console.ReadLine();
			string[] arguments = command!.Split(" ");
			switch(arguments[0]) {
				case "ExtractFile":
					Console.Write("Getting File...");
					CUE4Parse.FileProvider.Objects.GameFile? value;
					provider.TryFindGameFile(arguments[1]!,out value);
					Console.Write("OK\n");
					Console.Write("Writing File...");
					await File.WriteAllBytesAsync(arguments[2]!, await value!.ReadAsync());
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
					Console.Write("Writing List Into "+arguments[2]+"...");
					using (StreamWriter writer = new StreamWriter(arguments[2])){
						foreach (var path in provider.Files.Keys){
							if (path.StartsWith(arguments[1].ToLower())) {
								writer.WriteLine(path);
							}
						}
					}
					Console.Write("OK\n");
					break;
				case "Quit":
					run = false;
					break;
				default:
					Console.WriteLine("Invalid command"+" '"+arguments[0]+"'");
					Console.WriteLine("Available commands: ExtractFile <GameFile Path> <Output File>, List <Directory Path>, SaveList <Directory Path> <Output File> Quit");
					break;
			}
		}
	}
}
