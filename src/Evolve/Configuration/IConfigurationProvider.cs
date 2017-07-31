namespace Evolve.Configuration
{
    /// <summary>
    ///     A strategy for loading Evolve main configuration.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        ///     Overload default <paramref name="configuration"/> with values defined in the file located to <paramref name="evolveConfigurationPath"/>
        /// </summary>
        /// <param name="evolveConfigurationPath"> Full path to the Evolve configuration file. </param>
        /// <param name="configuration"> Default Evolve configuration. </param>
        /// <param name="environmentName"> The environment is typically set to one of Development, Staging, or Production. Optional. </param>
        void Configure(string evolveConfigurationPath, IEvolveConfiguration configuration, string environmentName);
    }
}
