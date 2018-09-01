namespace Evolve.Cli
{
    internal enum Command
    {
        //Lowercase because of some parsing issue with CommandLineParser library
        //https://github.com/commandlineparser/commandline/issues/198
        migrate,
        erase,
        repair
    }
}
