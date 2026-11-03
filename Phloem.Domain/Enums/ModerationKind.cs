namespace Phloem.Domain.Enums;

/// <summary>What a moderation check is about.</summary>
public enum ModerationKind
{
    /// <summary>A dish's name and description (decides whether buyers see it).</summary>
    Dish = 1,

    /// <summary>A shop's name (advice for staff; a doubtful name goes to the queue).</summary>
    ShopName = 2,

    /// <summary>A dish name a seller suggested for the dish list (advice for staff).</summary>
    DishNameSuggestion = 3,

    /// <summary>A Contact us message (spam, urgent, summary).</summary>
    ContactMessage = 4
}
