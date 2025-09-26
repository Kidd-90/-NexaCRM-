using System;
using System.ComponentModel.DataAnnotations;

namespace NexaCRM.Services.Admin.Validation;

/// <summary>
/// Validation attribute that rejects repeated or sequential character runs beyond a configured length.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DisallowSequentialCharactersAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisallowSequentialCharactersAttribute"/> class.
    /// </summary>
    /// <param name="sequenceLength">Minimum length of a repeated or sequential run that should be rejected.</param>
    public DisallowSequentialCharactersAttribute(int sequenceLength = 3)
    {
        if (sequenceLength < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(sequenceLength), sequenceLength, "Sequence length must be at least 2.");
        }

        SequenceLength = sequenceLength;
    }

    /// <summary>
    /// Gets the minimum length of a repeated or sequential character run that triggers validation failure.
    /// </summary>
    public int SequenceLength { get; }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text)
        {
            return ValidationResult.Success;
        }

        var candidate = text.Trim();
        if (candidate.Length < SequenceLength)
        {
            return ValidationResult.Success;
        }

        for (var index = 0; index <= candidate.Length - SequenceLength; index++)
        {
            var segment = candidate.AsSpan(index, SequenceLength);
            if (ContainsRepeatedCharacters(segment) || ContainsSequentialCharacters(segment))
            {
                var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(errorMessage);
            }
        }

        return ValidationResult.Success;
    }

    private static bool ContainsRepeatedCharacters(ReadOnlySpan<char> segment)
    {
        for (var index = 1; index < segment.Length; index++)
        {
            if (segment[index] != segment[0])
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsSequentialCharacters(ReadOnlySpan<char> segment)
    {
        var ascending = true;
        var descending = true;

        for (var index = 1; index < segment.Length; index++)
        {
            var previous = segment[index - 1];
            var current = segment[index];

            if (!char.IsLetterOrDigit(previous) || !char.IsLetterOrDigit(current))
            {
                ascending = false;
                descending = false;
                continue;
            }

            var normalizedPrevious = char.ToLowerInvariant(previous);
            var normalizedCurrent = char.ToLowerInvariant(current);
            var difference = normalizedCurrent - normalizedPrevious;

            if (difference != 1)
            {
                ascending = false;
            }

            if (difference != -1)
            {
                descending = false;
            }
        }

        return ascending || descending;
    }
}
