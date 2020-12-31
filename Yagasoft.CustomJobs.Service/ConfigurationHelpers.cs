#region Imports

using System.Configuration;
using System.Linq;
using Yagasoft.Libraries.Common;

#endregion

namespace Yagasoft.CustomJobs.Service
{
	/// <summary>
	/// ConfigHelpers
	/// </summary>
	public static class ConfigHelpers
	{
		/// <summary>
		/// Gets the specified key from the config file.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <exception cref="ConfigurationErrorsException">Missing '{keyString}' key value from configuration.</exception>
		public static string Get(string key)
		{
			ValidateKeys(key);

			var value = ConfigurationManager.AppSettings[key];

			if (value.IsEmpty())
			{
				throw new ConfigurationErrorsException($"Missing '{key}' key value in configuration.");
			}

			return value;
		}

		/// <summary>
		/// Validates the keys exit in the config file.
		/// </summary>
		/// <param name="keys">The keys.</param>
		/// <exception cref="ConfigurationErrorsException">Missing '{missingKeys.StringAggregate(",")}' keys from configuration.</exception>
		public static void ValidateKeys(params string[] keys)
		{
			var missingKeys = keys.Except(ConfigurationManager.AppSettings.AllKeys).ToList();

			if (missingKeys.Any())
			{
				throw new ConfigurationErrorsException(
					$"Missing '{missingKeys.StringAggregate(",")}' keys from configuration.");
			}
		}
	}
}
