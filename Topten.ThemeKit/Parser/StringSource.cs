using System;
using System.IO;
using System.Text;

namespace Topten.ThemeKit
{
    /// <summary>
    /// Provides the contents of a text file, along with its filename
    /// </summary>
    public class StringSource
    {
        /// <summary>
        /// Constructs a string source for a file
        /// </summary>
        /// <param name="filename">The name of the file to read</param>
        /// <returns>A StringSource for the specified file</returns>
        public static StringSource FromFile(string filename)
        {
            // Make sure path is fully qualified
            filename = Path.GetFullPath(filename);

            // Open string source
            return new StringSource(
                File.ReadAllText(filename),
                filename
                );
        }

        /// <summary>
        /// Constructs a string source for a file, resolving the full
        /// path from a base directory.
        /// </summary>
        /// <param name="baseDirectory">The base directory</param>
        /// <param name="filename">The file name, which if not fully qualified will be resolved to the base directory</param>
        /// <returns>A StringSource for the specified file</returns>
        public static StringSource FromFile(string baseDirectory, string filename)
        {
            // Make sure path is fully qualified
            filename = Path.GetFullPath(filename, baseDirectory);

            // Open string source
            return new StringSource(
                File.ReadAllText(filename),
                filename
                );
        }

        /// <summary>
        /// Construct a string source from a string
        /// </summary>
        /// <param name="str">The content of the file</param>
        /// <param name="filename">The filename associated with the string</param>
        public StringSource(string str, string filename)
        {
            // Remove BOM
            if (str.Length > 0 && (str[0] == 0xFFFE || str[0] == 0xFEFF))
                str = str.Substring(1);

            _str = str;
            _pos = 0;
            _startPos = 0;
            _stopPos = str.Length;
            _filename = filename;

            // Skip BOM if exists
            if (!Skip((char)0xFFFE))
                Skip((char)0xFEFF);
        }


        /// <summary>
        /// Construct a string source from a substring of another string
        /// </summary>
        /// <param name="str">The string content</param>
        /// <param name="startPos">The starting offset</param>
        /// <param name="length">The length of the slice</param>
        /// <param name="filename">The associated filename</param>
        public StringSource(string str, int startPos, int length, string filename = null)
        {
            _str = str;
            _pos = startPos;
            _startPos = startPos;
            _stopPos = startPos + length;
            _filename = filename;
        }

        /// <summary>
        /// Gets the source text for this StringSource
        /// </summary>
        public string SourceText
        {
            get { return _str.Substring(_startPos, _stopPos - _startPos); }
        }

        /// <summary>
        /// Creates a new StringSource from a section of this one
        /// </summary>
        /// <param name="from"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public StringSource CreateEmbeddedSource(int from, int length)
        {
            return new StringSource(_str, from, length, _filename);
        }

        /// <summary>
        /// Creates a SourcePosition for a location
        /// </summary>
        /// <param name="position">The character offset of the position</param>
        /// <returns>A SourcePosition describing the position</returns>
        public SourcePosition CreatePosition(int position)
        {
            return new SourcePosition(this, position);
        }

        /// <summary>
        /// Creates a SourcePosition for a location based on a line number
        /// </summary>
        /// <param name="line">The line number</param>
        /// <returns>A SourcePosition describing the position</returns>
        public SourcePosition CreateLinePosition(int line)
        {
            return new SourcePosition(this, LineNumbers.ToFileOffset(line, 0));
        }

        /// <summary>
        /// Creates a position marker for the EOF of this StringSource
        /// </summary>
        /// <returns>A SourcePosition describing the EOF</returns>
        public SourcePosition CreateEndPosition()
        {
            return CreatePosition(_stopPos);
        }

        /// <summary>
        /// Creates a SourcePosition for the current read position
        /// </summary>
        /// <returns>A SourcePosition</returns>
        public SourcePosition CapturePosition()
        {
            return new SourcePosition(this, _pos);
        }

        /// <summary>
        /// Gets the filename associated with this StringSource
        /// </summary>
        public string FileName => _filename;

        /// <summary>
        /// True if the current position is at EOF
        /// </summary>
        public bool EOF => _pos >= _stopPos;

        /// <summary>
        /// The current character 
        /// </summary>
        public char Current => _pos < _stopPos ? _str[_pos] : '\0';

        /// <summary>
        /// The current position 
        /// </summary>
        public int Position
        {
            get => _pos;
            set
            {
                System.Diagnostics.Debug.Assert(value >= _startPos && value <= _stopPos);
                _pos = value;
            }
        }

        /// <summary>
        /// The remaining text (handy for watching in debugger) 
        /// </summary>
        public string Remaining => _str.Substring(_pos);

        /// <summary>
        /// The character at offset from current 
        /// </summary>
        /// <param name="offset">The offset</param>
        /// <returns>The character at that offset</returns>
        public char CharAt(int offset)
        {
            var pos = _pos + offset;
            return pos < _stopPos ? _str[pos] : '\0';
        }

        /// <summary>
        /// Move the current position by a number of characters
        /// </summary>
        /// <param name="deltaChars"></param>
        public void Move(int deltaChars)
        {
            _pos += deltaChars;
            if (_pos < 0)
                _pos = 0;
            if (_pos > _stopPos)
                _pos = _stopPos;
        }

        /// <summary>
        /// Move to the next character (if available) 
        /// </summary>
        public void Next()
        {
            if (_pos < _stopPos)
                _pos++;
        }

        /// <summary>
        /// Move to the previous character 
        /// </summary>
        public void Previous()
        {
            if (_pos > 0)
                _pos--;
        }

        /// <summary>
        /// Return the rest of the string and move to EOF
        /// </summary>
        /// <returns></returns>
        public string SkipRemaining()
        {
            var str = Remaining;
            _pos = _stopPos;
            return str;
        }

        /// <summary>
        /// True if the current position is at the end of a line
        /// </summary>
        public bool EOL
        {
            get
            {
                if (_pos >= _stopPos)
                    return true;

                return _str[_pos] == '\r' || _str[_pos] == '\n';
            }
        }

        /// <summary>
        /// Move the current position to the end of the current line
        /// </summary>
        /// <returns>True if the current position moved</returns>
        public bool SkipToEOL()
        {
            // Skip to end of line
            int start = _pos;
            while (!EOF && _str[_pos] != '\r' && _str[_pos] != '\n')
                _pos++;
            return _pos > start;
        }

        /// <summary>
        /// Move the current position over the CR/LF at the current position
        /// </summary>
        /// <returns>True if the current position moved</returns>
        public bool SkipEOL()
        {
            int oldPos = _pos;
            if (_pos < _stopPos && _str[_pos] == '\r')
                _pos++;
            if (_pos < _stopPos && _str[_pos] == '\n')
                _pos++;
            return _pos > oldPos;
        }

        /// <summary>
        /// Move the current position to the next line
        /// </summary>
        /// <returns>True if the current position moved</returns>
        public bool SkipToNextLine()
        {
            int start = _pos;
            SkipToEOL();
            SkipEOL();
            return _pos > start;
        }

        /// <summary>
        /// Skip over any whitespace on the current line
        /// </summary>
        /// <returns>True if the current position moved</returns>
        public bool SkipLinespace()
        {
            if (_pos >= _stopPos)
                return false;
            if (!IsLineSpace(_str[_pos]))
                return false;

            _pos++;
            while (_pos < _stopPos && IsLineSpace(_str[_pos]))
                _pos++;

            return true;

        }

        /// <summary>
        /// Skip over any whitespace at the current position
        /// </summary>
        /// <returns>True if the current position moved</returns>
        public bool SkipWhitespace()
        {
            if (_pos >= _stopPos)
                return false;
            if (!char.IsWhiteSpace(_str[_pos]))
                return false;

            _pos++;
            while (_pos < _stopPos && char.IsWhiteSpace(_str[_pos]))
                _pos++;

            return true;
        }

        /// <summary>
        /// Check if the current position in the string matches a substring (case sensitive)
        /// </summary>
        /// <param name="str">The string to check for</param>
        /// <returns>True if the current position matches the specified string</returns>
        public bool DoesMatch(string str)
        {
            if (_pos + str.Length > _stopPos)
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (_str[_pos + i] != str[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the current position in the string matches a substring (case insensitive)
        /// </summary>
        /// <param name="str">The string to check for</param>
        /// <returns>True if the current position matches the specified string</returns>
        public bool DoesMatchI(string str)
        {
            if (_pos + str.Length > _stopPos)
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (char.ToLowerInvariant(_str[_pos + i]) != char.ToLowerInvariant(str[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Skip forward until a particular string is matched (case sensitive)
        /// </summary>
        /// <param name="str">The string to search for</param>
        /// <returns>True if the string was found</returns>
        public bool SkipUntil(string str)
        {
            while (_pos < _stopPos)
            {
                if (DoesMatch(str))
                    return true;
                _pos++;
            }
            return false;
        }

        /// <summary>
        /// Skip forward until a particular string is matched (case insensitive)
        /// </summary>
        /// <param name="str">The string to search for</param>
        /// <returns>True if the string was found</returns>
        public bool SkipUntilI(string str)
        {
            while (_pos < _stopPos)
            {
                if (DoesMatchI(str))
                    return true;
                _pos++;
            }
            return false;
        }

        /// <summary>
        /// Skip characters matching predicate 
        /// </summary>
        /// <param name="predicate">The predicate callback</param>
        /// <returns>True any characters were skipped</returns>
        public bool Skip(Func<char, bool> predicate)
        {
            if (_pos >= _stopPos || !predicate(_str[_pos]))
                return false;

            _pos++;
            while (_pos < _stopPos && predicate(_str[_pos]))
                _pos++;
            return true;
        }

        /// <summary>
        /// Skip one instance of the specified character (case sensitive)
        /// </summary>
        /// <param name="ch">The character to skip</param>
        /// <returns>True if the character was skipped</returns>
        public bool Skip(char ch)
        {
            if (_pos >= _stopPos)
                return false;

            if (_str[_pos] != ch)
                return false;

            _pos++;
            return true;
        }

        /// <summary>
        /// Skip one instance of the specified character (case insensitive)
        /// </summary>
        /// <param name="ch">The character to skip</param>
        /// <returns>True if the character was skipped</returns>
        public bool SkipI(char ch)
        {
            if (_pos >= _stopPos)
                return false;

            if (char.ToUpperInvariant(_str[_pos]) != char.ToUpperInvariant(ch))
                return false;

            _pos++;
            return true;
        }

        /// <summary>
        /// Skip a string (case sensitive) 
        /// </summary>
        /// <param name="str">The string to skip</param>
        /// <returns>True if the string was found at the current position</returns>
        public bool Skip(string str)
        {
            if (DoesMatch(str))
            {
                _pos += str.Length;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Skip a string (case insensitive) 
        /// </summary>
        /// <param name="str">The string to skip</param>
        /// <returns>True if the string was found at the current position</returns>
        public bool SkipI(string str)
        {
            if (DoesMatchI(str))
            {
                _pos += str.Length;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Extract text from the specified position to the current position 
        /// </summary>
        /// <param name="fromPosition">The start position of the string to extract</param>
        /// <returns>The extracted string</returns>
        public string Extract(int fromPosition)
        {
            return _str.Substring(fromPosition, _pos - fromPosition);
        }

        /// <summary>
        /// Extract text from the specified range
        /// </summary>
        /// <param name="fromPosition">The start position (inclusive)</param>
        /// <param name="toPosition">The end position (exlusive)</param>
        /// <returns>The extracted string</returns>
        public string Extract(int fromPosition, int toPosition)
        {
            return _str.Substring(fromPosition, toPosition - fromPosition);
        }

        /// <summary>
        /// Skip characters matching predicate and return the matched characters
        /// </summary>
        /// <param name="predicate">The predicate callback</param>
        /// <returns>The skipped characters</returns>
        public string SkipAndExtract(Func<char, bool> predicate)
        {
            int pos = _pos;
            if (!Skip(predicate))
                return null;
            return Extract(pos);
        }

        LineNumbers _lineNumbers;
        internal LineNumbers LineNumbers
        {
            get
            {
                if (_lineNumbers == null)
                    _lineNumbers = new LineNumbers(_str);
                return _lineNumbers;
            }
        }

        /// <summary>
        /// Extract a line
        /// </summary>
        /// <param name="line">The zero based index of the line to extract</param>
        /// <returns>The extracted line text</returns>
        public string ExtractLine(int line)
        {
            int linePos = LineNumbers.ToFileOffset(line, 0);
            int nextLinePos = LineNumbers.ToFileOffset(line + 1, 0);
            if (nextLinePos > 1 && _str[nextLinePos-1] == '\n')
                nextLinePos--;
            if (nextLinePos > 1 && _str[nextLinePos-1] == '\r')
                nextLinePos--;
            return _str.Substring(linePos, nextLinePos - linePos);
        }

        StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Process characters from the current position through a callback 
        /// building up a string in a StringBuilder.
        /// </summary>
        /// <param name="callback">The process callback</param>
        /// <returns>The contents of the built string</returns>
        public string Process(Func<char, StringBuilder, bool> callback)
        {
            _sb.Length = 0;
            while (_pos < _stopPos && callback(_str[_pos], _sb))
            {
                _pos++;
            }
            return _sb.ToString();
        }

        static bool IsLineSpace(char ch)
        {
            return ch == ' ' || ch == '\t';
        }


        string _str;
        int _pos;
        int _startPos;
        int _stopPos;
        string _filename;


    }


    /// <summary>
    /// Represents a position within a StringSource
    /// with helpers to map back to the source itself and the line
    /// number and character offset
    /// </summary>
    public class SourcePosition
    {
        /// <summary>
        /// Constructs a new SourcePosition
        /// </summary>
        /// <param name="source">The StringSource</param>
        /// <param name="pos">The position in the file</param>
        public SourcePosition(StringSource source, int pos)
        {
            Source = source;
            Position = pos;
        }

        /// <summary>
        /// The StringSource of this position
        /// </summary>
        public StringSource Source;

        /// <summary>
        /// The character offset into the file of the position
        /// </summary>
        public int Position;

        /// <summary>
        /// The zero-based line number of the position
        /// </summary>
        public int LineNumber
        {
            get
            {
                if (_lineNumber < 0)
                {
                    Source.LineNumbers.FromFileOffset(Position, out _lineNumber, out _charPosition);
                }
                return _lineNumber;
            }
        }

        /// <summary>
        /// The zero based character offset within the line
        /// </summary>
        public int CharacterPosition
        {
            get
            {
                if (_lineNumber < 0)
                {
                    Source.LineNumbers.FromFileOffset(Position, out _lineNumber, out _charPosition);
                }
                return _charPosition;
            }
        }

        int _lineNumber = -1;
        int _charPosition = -1;
    }

    internal static partial class StringSourceUtils
    {
        public static string Describe(this SourcePosition pos)
        {
            if (pos == null)
                return "<unknown>";
            else
                return $"{pos.Source.FileName}({pos.LineNumber + 1},{pos.CharacterPosition + 1})";
        }
    }
}
