namespace Evolve.Utilities
{
    /// <summary>
    /// [os].[version]-[architecture]-[additional qualifiers]
    /// </summary>
    public class RID
    {
        private string _runtime;

        public RID(string runtime)
        {
            _runtime = runtime;
        }

        public string OS
        {
            get
            {
                if (string.IsNullOrEmpty(_runtime))
                {
                    return "";
                }

                string os_version_part = _runtime.Split('-')[0];
                return os_version_part.Split('.')[0];
            }
        }

        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_runtime))
                {
                    return "";
                }

                string os_version_part = _runtime.Split('-')[0];
                int dotIndex = os_version_part.IndexOf('.');
                if (dotIndex == -1)
                {
                    return "";
                }

                return os_version_part.Substring(startIndex: dotIndex + 1);
            }
        }

        public string Architecture
        {
            get
            {
                if (string.IsNullOrEmpty(_runtime))
                {
                    return "";
                }

                string[] parts = _runtime.Split('-');
                if (parts.Length < 2)
                {
                    return "";
                }

                return parts[1];
            }
        }

        public override string ToString()
        {
            return $"RID: [{_runtime}] OS: [{OS}], Version: [{Version}], Architecture: [{Architecture}]";
        }
    }
}
