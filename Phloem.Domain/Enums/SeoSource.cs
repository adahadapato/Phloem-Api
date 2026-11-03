namespace Phloem.Domain.Enums;

/// <summary>Who wrote a page's search engine text.</summary>
public enum SeoSource
{
    /// <summary>The AI. Rewritten by itself when the page changes (e.g. a shop adds dishes).</summary>
    Ai = 1,

    /// <summary>A staff member. Never overwritten by the AI unless staff press "Write with AI" again.</summary>
    Staff = 2
}
