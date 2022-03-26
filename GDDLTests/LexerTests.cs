using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using GDDL.Parsing;

namespace GDDL.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void LexesIntegers()
        {
            Assert.AreEqual(TokenInt("1"), LexSingle("1"));
            Assert.AreEqual(TokenInt("-1"), LexSingle("-1"));
            Assert.AreEqual(TokenHexInt("0x1"), LexSingle("0x1"));
            Assert.AreEqual(TokenHexInt("-0x1"), LexSingle("-0x1"));
        }

        [TestMethod]
        public void LexesFloats()
        {
            Assert.AreEqual(TokenFloat("1.0"), LexSingle("1.0"));
            Assert.AreEqual(TokenFloat("1."), LexSingle("1."));
            Assert.AreEqual(TokenFloat(".1"), LexSingle(".1"));
            Assert.AreEqual(TokenFloat("1.0e1"), LexSingle("1.0e1"));
            Assert.AreEqual(TokenFloat("1.0e+1"), LexSingle("1.0e+1"));
            Assert.AreEqual(TokenFloat("1.0e-1"), LexSingle("1.0e-1"));
            Assert.AreEqual(TokenFloat("1.e1"), LexSingle("1.e1"));
            Assert.AreEqual(TokenFloat(".1e1"), LexSingle(".1e1"));

            Assert.AreEqual(TokenFloat("-1.0"), LexSingle("-1.0"));
            Assert.AreEqual(TokenFloat("-1."), LexSingle("-1."));
            Assert.AreEqual(TokenFloat("-.1"), LexSingle("-.1"));
            Assert.AreEqual(TokenFloat("-1.0e1"), LexSingle("-1.0e1"));
            Assert.AreEqual(TokenFloat("-1.0e+1"), LexSingle("-1.0e+1"));
            Assert.AreEqual(TokenFloat("-1.0e-1"), LexSingle("-1.0e-1"));
            Assert.AreEqual(TokenFloat("-1.e1"), LexSingle("-1.e1"));
            Assert.AreEqual(TokenFloat("-.1e1"), LexSingle("-.1e1"));

            Assert.AreEqual(TokenFloat("+1.0"), LexSingle("+1.0"));
            Assert.AreEqual(TokenFloat("+1."), LexSingle("+1."));
            Assert.AreEqual(TokenFloat("+.1"), LexSingle("+.1"));
            Assert.AreEqual(TokenFloat("+1.0e1"), LexSingle("+1.0e1"));
            Assert.AreEqual(TokenFloat("+1.0e+1"), LexSingle("+1.0e+1"));
            Assert.AreEqual(TokenFloat("+1.0e-1"), LexSingle("+1.0e-1"));
            Assert.AreEqual(TokenFloat("+1.e1"), LexSingle("+1.e1"));
            Assert.AreEqual(TokenFloat("+.1e1"), LexSingle("+.1e1"));

            Assert.AreEqual(TokenFloat(".Inf"), LexSingle(".Inf"));
            Assert.AreEqual(TokenFloat("-.Inf"), LexSingle("-.Inf"));
            Assert.AreEqual(TokenFloat("+.Inf"), LexSingle("+.Inf"));
            Assert.AreEqual(TokenFloat(".NaN"), LexSingle(".NaN"));
        }

        [TestMethod]
        public void LexesStrings()
        {
            // Ascii text
            Assert.AreEqual(TokenString("\"a\""), LexSingle("\"a\""));
            Assert.AreEqual(TokenString("\"b\\\"\""), LexSingle("\"b\\\"\""));
            Assert.AreEqual(TokenString("\"b'\""), LexSingle("\"b'\""));
            Assert.AreEqual(TokenString("'a'"), LexSingle("'a'"));
            Assert.AreEqual(TokenString("'b\\''"), LexSingle("'b\\''"));
            Assert.AreEqual(TokenString("'b\"'"), LexSingle("'b\"'"));

            // Escapes
            Assert.AreEqual(TokenString("'\\x00'"), LexSingle("'\\x00'"));
            Assert.AreEqual(TokenString("'\\x0F'"), LexSingle("'\\x0F'"));
            Assert.AreEqual(TokenString("'\\xF0'"), LexSingle("'\\xF0'"));
            Assert.AreEqual(TokenString("'\\xFF'"), LexSingle("'\\xFF'"));
            Assert.AreEqual(TokenString("'\\u0000'"), LexSingle("'\\u0000'"));
            Assert.AreEqual(TokenString("'\\u000F'"), LexSingle("'\\u000F'"));
            Assert.AreEqual(TokenString("'\\uF000'"), LexSingle("'\\uF000'"));
            Assert.AreEqual(TokenString("'\\uF00F'"), LexSingle("'\\uF00F'"));

            // Unicode
            Assert.AreEqual(
                TokenString(
                    "'\uD800\uDF3C\uD800\uDF30\uD800\uDF32 \uD800\uDF32\uD800\uDF3B\uD800\uDF34\uD800\uDF43 \uD800\uDF39̈\uD800\uDF44\uD800\uDF30\uD800\uDF3D, \uD800\uDF3D\uD800\uDF39 \uD800\uDF3C\uD800\uDF39\uD800\uDF43 \uD800\uDF45\uD800\uDF3F \uD800\uDF3D\uD800\uDF33\uD800\uDF30\uD800\uDF3D \uD800\uDF31\uD800\uDF42\uD800\uDF39\uD800\uDF32\uD800\uDF32\uD800\uDF39\uD800\uDF38.'"),
                LexSingle(
                    "'\uD800\uDF3C\uD800\uDF30\uD800\uDF32 \uD800\uDF32\uD800\uDF3B\uD800\uDF34\uD800\uDF43 \uD800\uDF39̈\uD800\uDF44\uD800\uDF30\uD800\uDF3D, \uD800\uDF3D\uD800\uDF39 \uD800\uDF3C\uD800\uDF39\uD800\uDF43 \uD800\uDF45\uD800\uDF3F \uD800\uDF3D\uD800\uDF33\uD800\uDF30\uD800\uDF3D \uD800\uDF31\uD800\uDF42\uD800\uDF39\uD800\uDF32\uD800\uDF32\uD800\uDF39\uD800\uDF38.'"));
        }

        [TestMethod]
        public void LexesKeywords()
        {
            Assert.AreEqual(TokenBooleanTrue(), LexSingle("true"));
            Assert.AreEqual(TokenBooleanFalse(), LexSingle("false"));
            Assert.AreEqual(TokenNull(), LexSingle("null"));
            Assert.AreEqual(TokenNil(), LexSingle("nil"));
        }

        [TestMethod]
        public void LexesSymbols()
        {
            Assert.AreEqual(TokenLBrace(), LexSingle("{"));
            Assert.AreEqual(TokenRBrace(), LexSingle("}"));
            Assert.AreEqual(TokenComma(), LexSingle(","));
            Assert.AreEqual(TokenColon(), LexSingle(":"));
            Assert.AreEqual(TokenEquals(), LexSingle("="));
        }

        [TestMethod]
        public void KeepsComments()
        {
            Token expected = Token(TokenType.LBrace, "{", new ParsingContext("TEST", 1, 1), "this is a comment\n");
            Token actual = LexSingle("#this is a comment\n{");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void KeepsMultipleCommentLines()
        {
            Token expected = Token(TokenType.LBrace, "{", new ParsingContext("TEST", 1, 1), "this\nis\na\ncomment\n");
            Token actual = LexSingle("#this\n#is\n#a\n#comment\n{");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IgnoresWhitespace()
        {
            Assert.AreEqual(TokenLBrace(), LexSingle(" \t\n{ \t\n"));
        }

        // HARNESS BELOW
        // -------------

        public static Token LexSingle(string text)
        {
            var lexer = new Lexer(MakeReader(text));
            var token = lexer.Pop();
            Assert.AreEqual(TokenType.End, lexer.Peek(), $"Should find END after reading token {token}");
            return token;
        }

        public static Reader MakeReader(string text)
        {
            return new Reader(new StringReader(text), "TEST");
        }

        public static Token TokenEnd()
        {
            return Token(TokenType.End, "");
        }

        public static Token TokenInt(string number)
        {
            return Token(TokenType.IntegerLiteral, number);
        }

        public static Token TokenFloat(string number)
        {
            return Token(TokenType.DecimalLiteral, number);
        }

        public static Token TokenHexInt(string number)
        {
            return Token(TokenType.HexIntLiteral, number);
        }

        public static Token TokenString(string text)
        {
            return Token(TokenType.StringLiteral, text);
        }

        public static Token TokenBooleanTrue()
        {
            return Token(TokenType.True, "true");
        }

        public static Token TokenBooleanFalse()
        {
            return Token(TokenType.False, "false");
        }

        public static Token TokenNil()
        {
            return Token(TokenType.Nil, "nil");
        }

        public static Token TokenNull()
        {
            return Token(TokenType.Null, "null");
        }

        public static Token TokenLBrace()
        {
            return Token(TokenType.LBrace, "{");
        }

        public static Token TokenRBrace()
        {
            return Token(TokenType.RBrace, "}");
        }

        public static Token TokenColon()
        {
            return Token(TokenType.Colon, ":");
        }

        public static Token TokenComma()
        {
            return Token(TokenType.Comma, ",");
        }

        public static Token TokenEquals()
        {
            return Token(TokenType.EqualSign, "=");
        }

        public static Token TokenIdentifier(string text)
        {
            return Token(TokenType.Identifier, text);
        }

        public static Token Token(TokenType name, string text)
        {
            return Token(name, text, new ParsingContext("TEST", 1, 1));
        }

        public static Token Token(TokenType name, string text, ParsingContext context)
        {
            return Token(name, text, context, "", "");
        }

        public static Token Token(TokenType name, string text, ParsingContext context, string comment)
        {
            return Token(name, text, context, comment, "");
        }

        public static Token Token(TokenType name, string text, ParsingContext context, string comment,
            string whitespace)
        {
            return new Token(name, text, context, comment, whitespace);
        }
    }
}