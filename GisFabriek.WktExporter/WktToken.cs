using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GisFabriek.WktExporter
{
    internal class WktToken
    {
        internal WktToken(string s, int startIndex, int endIndex)
        {
            Text = s;
            StartIndex = startIndex;
            EndIndex = endIndex;

            //remove optional whitespace and/or parens at ends of token

            if (IsEmpty)
            {
                return;
            }

            while (char.IsWhiteSpace(Text[StartIndex]))
            {
                StartIndex++;
            }

            var removedLeadingParen = false;
            if (Text[StartIndex] == '(')
            {
                StartIndex++;
                removedLeadingParen = true;
            }

            while (char.IsWhiteSpace(Text[EndIndex]))
            {
                EndIndex--;
            }

            if (Text[EndIndex] == ')' && removedLeadingParen)
            {
                EndIndex--;
            }
        }

        internal string Text { get; }

        internal int StartIndex { get; }

        internal int EndIndex { get; }


        internal bool IsEmpty => StartIndex < 0 || EndIndex < StartIndex;

        internal IEnumerable<WktToken> Tokens
        {
            get
            { if (IsEmpty)
                {
                    yield break;
                }
                var currentStart = StartIndex;
                //currentStart may be a '(', do not let currentEnd go past it without nesting.
                var currentEnd = StartIndex;
                var nesting = 0;
                while (true)
                {
                    if (currentEnd >= EndIndex)
                    {
                        yield return new WktToken(Text, currentStart, EndIndex);
                        yield break;
                    }

                    if (Text[currentEnd] == '(')
                    {
                        nesting++;
                    }
                    if (Text[currentEnd] == ')')
                    {
                        nesting--;
                    }

                    if (nesting == 0 && Text[currentEnd] == ',')
                    {
                        yield return new WktToken(Text, currentStart, currentEnd - 1);
                        currentStart = currentEnd + 1;
                        while (currentStart < EndIndex && Char.IsWhiteSpace(Text[currentStart]))
                        {
                            currentStart++;
                        }
                        //currentStart may be a '(', do not let currentEnd go past it without nesting.
                        currentEnd = currentStart - 1;
                    }
                    currentEnd++;
                }
            }
        }

        internal IEnumerable<double> Coords
        {
            get
            {
                if (IsEmpty)
                {
                    return new double[0];
                }
                var text = Text.Substring(StartIndex, 1 + EndIndex - StartIndex);
                var words = text.Split((Char[])null, StringSplitOptions.RemoveEmptyEntries);
                return words.Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture));
            }
        }

        public override string ToString()
        {
            return Text.Substring(StartIndex, 1 + EndIndex - StartIndex);
        }
    }
}