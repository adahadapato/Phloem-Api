namespace Phloem.Domain.Enums;

/// <summary>Where a moderation check is.</summary>
public enum ModerationState
{
    /// <summary>Waiting for the AI (the background checker takes it within about a minute).</summary>
    WaitingForAi = 1,

    /// <summary>In the Moderation queue: a person decides.</summary>
    WaitingForPerson = 2,

    /// <summary>Finished by the AI.</summary>
    DoneByAi = 3,

    /// <summary>Finished by a person.</summary>
    DecidedByStaff = 4,

    /// <summary>The text changed (or was deleted) before the check finished; a newer check replaces it.</summary>
    Replaced = 5
}
