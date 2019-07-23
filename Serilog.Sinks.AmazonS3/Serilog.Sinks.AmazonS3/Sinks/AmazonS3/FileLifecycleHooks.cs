// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileLifecycleHooks.cs" company="Hämmer Electronics">
// The project is licensed under the GNU GENERAL PUBLIC LICENSE, Version 3, 29 June 2007
// </copyright>
// <summary>
//   Defines the FileLifecycleHooks type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.AmazonS3
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    /// <summary>   This class enables hooking into log file lifecycle events. </summary>
    public abstract class FileLifecycleHooks
    {
        /// <summary>
        ///     Initialize or wrap the <paramref name="underlyingStream" /> opened on the log file. This
        ///     can be used to write file headers, or wrap the stream in another that adds buffering,
        ///     compression, encryption, etc. The underlying file may or may not be empty when this
        ///     method is called.
        /// </summary>
        ///
        /// <remarks>
        ///     A value must be returned from overrides of this method. Serilog will flush and/or dispose
        ///     the returned value, but will not dispose the stream initially passed in unless it is
        ///     itself returned.
        /// </remarks>
        ///
        /// <param name="underlyingStream"> The underlying <see cref="Stream" /> opened on the log file. </param>
        /// <param name="encoding">         The encoding to use when reading/writing to the stream. </param>
        ///
        /// <returns>
        ///     The <see cref="Stream" /> Serilog should use when writing events to the log file.
        /// </returns>

        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public virtual Stream OnFileOpened(Stream underlyingStream, Encoding encoding)
        {
            return underlyingStream;
        }
    }
}