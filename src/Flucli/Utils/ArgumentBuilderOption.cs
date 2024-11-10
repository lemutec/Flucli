namespace Flucli.Utils;

public class ArgumentBuilderOption
{
    /// <summary>
    /// Default Quote
    /// </summary>
    public const string Quote = "\"";

    /// <summary>
    /// <seealso cref="QuoteReplace.DoubleQuote"/>
    /// </summary>
    public const string DoubleQuote = "\"";

    /// <summary>
    /// <seealso cref="QuoteReplace.BackSlashQuote"/>
    /// </summary>
    public const string BackSlashQuote = "\\\"";

    /// <summary>
    /// Single instance of this class.
    /// </summary>
    public static ArgumentBuilderOption Default { get; } = new();

    /// <summary>
    /// Options for handling separator within argument strings.
    /// </summary>
    public string Separator { get; set; } = " ";

    /// <summary>
    /// Options for handling quotes within argument strings.
    /// </summary>
    public QuoteReplace QuoteReplace { get; set; } = QuoteReplace.None;

    /// <summary>
    /// Some issues to be fixed here about url scheme.
    /// Which argument Starts with `https://` or `http://` should be added quote.
    /// </summary>
    public bool IsQuoteScheme { get; set; } = true;
}

/// <summary>
/// Options for handling quotes within argument strings.
/// </summary>
public enum QuoteReplace
{
    /// <summary>
    /// No special handling for quotes
    /// </summary>
    None,

    /// <summary>
    /// Replace each quote with two quotes
    /// </summary>
    DoubleQuote,

    /// <summary>
    /// Escape each quote with a backslash
    /// </summary>
    BackSlashQuote,
}
