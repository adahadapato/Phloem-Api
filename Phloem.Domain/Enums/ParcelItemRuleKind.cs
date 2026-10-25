namespace Phloem.Domain.Enums;

/// <summary>Whether an item on the "what can be sent" list is refused, or allowed only with a condition.</summary>
/// <remarks>The numbers are stored in the database: only add new values at the end.</remarks>
public enum ParcelItemRuleKind
{
    /// <summary>Never carried (e.g. weapons, cash).</summary>
    NotAllowed = 1,

    /// <summary>Carried only as the rule's detail says (e.g. "dried fish: sealed, at most 2 kg").</summary>
    Restricted = 2
}
