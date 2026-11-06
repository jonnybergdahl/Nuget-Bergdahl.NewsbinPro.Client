// newsbin: a command line client for Newsbin Pro, built on Bergdahl.NewsbinPro.Client.
//
//   newsbin [options] <command> [arguments]   run one command and exit
//   newsbin [options]                         interactive shell on one connection
//
// Run "newsbin --help" for the list of commands.

using Bergdahl.NewsbinPro.Cli;

return await App.RunAsync(args);
