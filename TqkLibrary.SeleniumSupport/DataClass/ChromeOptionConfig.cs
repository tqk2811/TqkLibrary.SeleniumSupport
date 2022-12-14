using System.Collections.Generic;

namespace TqkLibrary.SeleniumSupport
{
    /// <summary>
    /// 
    /// </summary>
    public class ChromeOptionConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> UserAgents { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Arguments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ConfigValue> AdditionalCapabilitys { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> ExcludedArguments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ConfigValue> UserProfilePreferences { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ConfigValue
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }
    }
}