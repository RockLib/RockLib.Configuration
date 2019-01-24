using System.Collections.Generic;

namespace RockLib.Configuration.MessagingProvider
{
    /// <summary>
    /// Singleton implementation of the <see cref="ISettingFilter"/> interface
    /// that always returns <see langword="true"/>.
    /// </summary>
    public class NullSettingFilter : ISettingFilter
    {
        private NullSettingFilter() {}

        /// <summary>
        /// Gets the instance of <see cref="NullSettingFilter"/>.
        /// </summary>
        public static NullSettingFilter Instance { get; } = new NullSettingFilter();

        /// <summary>
        /// Always returns true.
        /// </summary>
        /// <param name="setting">Ignored.</param>
        /// <param name="messageHeaders">Ignored.</param>
        /// <returns><see langword="true"/>.</returns>
        public bool ShouldProcessSettingChange(string setting, IReadOnlyDictionary<string, object> messageHeaders) => true;
    }
}
