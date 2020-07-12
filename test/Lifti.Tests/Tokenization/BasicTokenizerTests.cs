﻿using FluentAssertions;
using Lifti.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Lifti.Tests.Tokenization
{
    public abstract class BasicTokenizerTests
    {
        protected BasicTokenizer sut = new BasicTokenizer();

        [Fact]
        public void ShouldReturnNoTokensForEmptyString()
        {
            var output = this.sut.Process(string.Empty).ToList();

            output.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnNoTokensForNullString()
        {
            var output = this.sut.Process((string)null).ToList();

            output.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnNoTokensForNullStringEnumerable()
        {
            var output = this.sut.Process((IEnumerable<string>)null).ToList();

            output.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnNoTokensForStringContainingJustWordBreakCharacters()
        {
            var output = this.sut.Process(" \t\r\n\u2028\u2029\u000C").ToList();

            output.Should().BeEmpty();
        }

        protected void WithConfiguration(bool splitOnPunctuation = true, char[] additionalSplitChars = null, bool caseInsensitive = false, bool accentInsensitive = false)
        {
            ((IConfiguredBy<TokenizationOptions>)this.sut).Configure(
                new TokenizationOptions(TokenizerKind.PlainText)
                {
                    SplitOnPunctuation = splitOnPunctuation,
                    AdditionalSplitCharacters = additionalSplitChars ?? Array.Empty<char>(),
                    CaseInsensitive = caseInsensitive,
                    AccentInsensitive = accentInsensitive
                });
        }

        public class WithNoPreprocessors : BasicTokenizerTests
        {
            [Fact]
            public void ShouldReturnSingleTokenForStringContainingOnlyOneWord()
            {
                var output = this.sut.Process("test").ToList();

                output.Should().BeEquivalentTo(new[]
                {
                    new Token("test", new TokenLocation(0, 0, 4))
                });
            }

            [Fact]
            public void ShouldReturnSeparateTokensForWordsSeparatedByNonBreakSpace()
            {
                var output = this.sut.Process("test split").ToList();

                output.Should().BeEquivalentTo(new[]
                {
                    new Token("test", new TokenLocation(0, 0, 4)),
                    new Token("split", new TokenLocation(1, 5, 5))
                });
            }

            [Fact]
            public void ShouldReturnSingleTokenForStringContainingOnlyOneWordEnclosedWithWordBreaks()
            {
                var output = this.sut.Process(" test\r\n").ToList();

                output.Should().BeEquivalentTo(new[]
                {
                    new Token("test", new TokenLocation(0, 1, 4))
                });
            }

            [Fact]
            public void WhenSplittingAtPunctuation_ShouldTokenizeAtWordBreaksAndPunctuation()
            {
                this.WithConfiguration();

                var input = "Test string (with punctuation) with test spaces";

                var output = this.sut.Process(input).ToList();

                output.Should().BeEquivalentTo(new[]
                {
                    new Token("Test", new TokenLocation(0, 0, 4)),
                    new Token("string", new TokenLocation(1, 5, 6)),
                    new Token("with", new TokenLocation(2, 13, 4), new TokenLocation(4, 31, 4)),
                    new Token("punctuation", new TokenLocation(3, 18, 11)),
                    new Token("test", new TokenLocation(5, 36, 4)),
                    new Token("spaces", new TokenLocation(6, 41, 6))
                });
            }

            [Fact]
            public void WhenNotSplittingAtPunctuation_ShouldTokenizeAtWordBreaksOnly()
            {
                this.WithConfiguration(splitOnPunctuation: false);

                var input = "Test string (with punctuation) with test spaces";

                var output = this.sut.Process(input).ToList();

                output.Should().BeEquivalentTo(new[]
                {
                    new Token("Test", new TokenLocation(0, 0, 4)),
                    new Token("string", new TokenLocation(1, 5, 6)),
                    new Token("(with", new TokenLocation(2, 12, 5)),
                    new Token("punctuation)", new TokenLocation(3, 18, 12)),
                    new Token("with", new TokenLocation(4, 31, 4)),
                    new Token("test", new TokenLocation(5, 36, 4)),
                    new Token("spaces", new TokenLocation(6, 41, 6))
                });
            }

            [Fact]
            public void WhenSplittingOnAdditionalCharacters_ShouldTokenizeAtWordBreaksAndAdditionalCharacters()
            {
                this.WithConfiguration(splitOnPunctuation: false, additionalSplitChars: new[] { '@', '¬' });

                var input = "Test@string¬with custom\tsplits";

                var output = this.sut.Process(input).ToList();

                output.Should().BeEquivalentTo(new[]
                {
                    new Token("Test", new TokenLocation(0, 0, 4)),
                    new Token("string", new TokenLocation(1, 5, 6)),
                    new Token("with", new TokenLocation(2, 12, 4)),
                    new Token("custom", new TokenLocation(3, 17, 6)),
                    new Token("splits", new TokenLocation(4, 24, 6))
                });
            }

            public class WithAllInsensitivityProcessors : BasicTokenizerTests
            {
                public WithAllInsensitivityProcessors()
                {
                    this.WithConfiguration(caseInsensitive: true, accentInsensitive: true);
                }

                [Fact]
                public void ShouldReturnSingleTokenForStringContainingOnlyOneWord()
                {
                    var output = this.sut.Process("test").ToList();

                    output.Should().BeEquivalentTo(new[]
                    {
                        new Token("TEST", new TokenLocation(0, 0, 4))
                    });
                }

                [Fact]
                public void ProcessingEnumerableSetOfOneWordStrings_ShouldReturnSingleTokenForEachWithContinuingIndexAndOffset()
                {
                    var output = this.sut.Process(new[] { "test", "test2", "test3" }).ToList();

                    output.Should().BeEquivalentTo(new[]
                    {
                        new Token("TEST", new TokenLocation(0, 0, 4)),
                        new Token("TEST2", new TokenLocation(1, 4, 5)),
                        new Token("TEST3", new TokenLocation(2, 9, 5))
                    });
                }

                [Fact]
                public void ProcessingEnumerableContainingMultipleWordStrings_ShouldReturnTokensWithContinuingIndexAndOffset()
                {
                    var output = this.sut.Process(new[] { "test", "test2 and test3", "test4" }).ToList();

                    output.Should().BeEquivalentTo(new[]
                    {
                        new Token("TEST", new TokenLocation(0, 0, 4)),
                        new Token("TEST2", new TokenLocation(1, 4, 5)),
                        new Token("AND", new TokenLocation(2, 10, 3)),
                        new Token("TEST3", new TokenLocation(3, 14, 5)),
                        new Token("TEST4", new TokenLocation(4, 19, 5))
                    });
                }

                [Fact]
                public void ShouldReturnSingleTokenForStringContainingOnlyOneWordEnclosedWithWordBreaks()
                {
                    var output = this.sut.Process(" test\r\n").ToList();

                    output.Should().BeEquivalentTo(new[]
                    {
                        new Token("TEST", new TokenLocation(0, 1, 4))
                    });
                }

                [Fact]
                public void WhenSplittingAtPunctuation_ShouldTokenizeAtWordBreaksAndPunctuation()
                {
                    var input = "Træ træ moo moǑ";

                    var output = this.sut.Process(input).ToList();

                    output.OrderBy(o => o.Value[0]).Should().BeEquivalentTo(new[]
                    {
                        new Token("MOO", new TokenLocation(2, 8, 3), new TokenLocation(3, 12, 3)),
                        new Token("TRAE", new TokenLocation(0, 0, 3), new TokenLocation(1, 4, 3))
                    });
                }
            }
        }
    }
}