using System.Collections.Generic;

namespace TqkLibrary.SeleniumSupport
{
  internal class ChromeOptionConfig
  {
    public List<string> UserAgents { get; set; }
    public List<string> Arguments { get; set; }
    public List<ConfigValue> AdditionalCapabilitys { get; set; }
    public List<string> ExcludedArguments { get; set; }
    public List<ConfigValue> UserProfilePreferences { get; set; }
  }

  internal class ConfigValue
  {
    public string Name { get; set; }
    public string Value { get; set; }
  }
}