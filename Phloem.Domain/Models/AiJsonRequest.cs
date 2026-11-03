namespace Phloem.Domain.Models;

/// <summary>One question for an AI job.</summary>
/// <param name="PromptKey">Which job (a key from <see cref="Constants.AiPrompts"/>).</param>
/// <param name="Input">What the job works on, usually JSON. People's text must be inside JSON values (never pasted
/// into the instructions), so text like "ignore your instructions" stays data.</param>
/// <param name="Subject">What it's about, for the log, e.g. "Shop Mama's Kitchen" or "Prompt seo.shop".</param>
public sealed record AiJsonRequest(string PromptKey, string Input, string? Subject = null);
