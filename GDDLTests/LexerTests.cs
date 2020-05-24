using Microsoft.VisualStudio.TestTools.UnitTesting;
using GDDL;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GDDL.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void lexesIntegers()
        {
            Assert.AreEqual(tokenInt("1"), lexSingle("1"));
            Assert.AreEqual(tokenInt("-1"), lexSingle("-1"));
            Assert.AreEqual(tokenHexInt("0x1"), lexSingle("0x1"));
            Assert.AreEqual(tokenHexInt("-0x1"), lexSingle("-0x1"));
        }

        [TestMethod]
        public void lexesFloats()
        {
            Assert.AreEqual(tokenFloat("1.0"), lexSingle("1.0"));
            Assert.AreEqual(tokenFloat("1."), lexSingle("1."));
            Assert.AreEqual(tokenFloat(".1"), lexSingle(".1"));
            Assert.AreEqual(tokenFloat("1.0e1"), lexSingle("1.0e1"));
            Assert.AreEqual(tokenFloat("1.0e+1"), lexSingle("1.0e+1"));
            Assert.AreEqual(tokenFloat("1.0e-1"), lexSingle("1.0e-1"));
            Assert.AreEqual(tokenFloat("1.e1"), lexSingle("1.e1"));
            Assert.AreEqual(tokenFloat(".1e1"), lexSingle(".1e1"));

            Assert.AreEqual(tokenFloat("-1.0"), lexSingle("-1.0"));
            Assert.AreEqual(tokenFloat("-1."), lexSingle("-1."));
            Assert.AreEqual(tokenFloat("-.1"), lexSingle("-.1"));
            Assert.AreEqual(tokenFloat("-1.0e1"), lexSingle("-1.0e1"));
            Assert.AreEqual(tokenFloat("-1.0e+1"), lexSingle("-1.0e+1"));
            Assert.AreEqual(tokenFloat("-1.0e-1"), lexSingle("-1.0e-1"));
            Assert.AreEqual(tokenFloat("-1.e1"), lexSingle("-1.e1"));
            Assert.AreEqual(tokenFloat("-.1e1"), lexSingle("-.1e1"));
        }

        [TestMethod]
        public void lexesStrings()
        {
            Assert.AreEqual(tokenString("\"a\""), lexSingle("\"a\""));
            Assert.AreEqual(tokenString("\"b\\\"\""), lexSingle("\"b\\\"\""));
            Assert.AreEqual(tokenString("\"b'\""), lexSingle("\"b'\""));
            Assert.AreEqual(tokenString("'a'"), lexSingle("'a'"));
            Assert.AreEqual(tokenString("'b\\''"), lexSingle("'b\\''"));
            Assert.AreEqual(tokenString("'b\"'"), lexSingle("'b\"'"));
            Assert.AreEqual(tokenString("'\\x00'"), lexSingle("'\\x00'"));
            Assert.AreEqual(tokenString("'\\x0F'"), lexSingle("'\\x0F'"));
            Assert.AreEqual(tokenString("'\\xF0'"), lexSingle("'\\xF0'"));
            Assert.AreEqual(tokenString("'\\xFF'"), lexSingle("'\\xFF'"));
            Assert.AreEqual(tokenString("'\\u0000'"), lexSingle("'\\u0000'"));
            Assert.AreEqual(tokenString("'\\u000F'"), lexSingle("'\\u000F'"));
            Assert.AreEqual(tokenString("'\\uF000'"), lexSingle("'\\uF000'"));
            Assert.AreEqual(tokenString("'\\uF00F'"), lexSingle("'\\uF00F'"));
        }

        [TestMethod]
        public void lexesKeywords()
        {
            Assert.AreEqual(tokenBooleanTrue(), lexSingle("true"));
            Assert.AreEqual(tokenBooleanFalse(), lexSingle("false"));
            Assert.AreEqual(tokenNull(), lexSingle("null"));
            Assert.AreEqual(tokenNil(), lexSingle("nil"));
        }

        [TestMethod]
        public void lexesSymbols()
        {
            Assert.AreEqual(tokenLBrace(), lexSingle("{"));
            Assert.AreEqual(tokenRBrace(), lexSingle("}"));
            Assert.AreEqual(tokenComma(), lexSingle(","));
            Assert.AreEqual(tokenColon(), lexSingle(":"));
            Assert.AreEqual(tokenEquals(), lexSingle("="));
        }

        [TestMethod]
        public void keepsComments()
        {
            Token expected = token(TokenType.LBrace, "{", new ParsingContext("TEST", 1, 1), "this is a comment\n");
            Token actual = lexSingle("#this is a comment\n{");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void keepsMultipleCommentLines()
        {
            Token expected = token(TokenType.LBrace, "{", new ParsingContext("TEST", 1, 1), "this\nis\na\ncomment\n");
            Token actual = lexSingle("#this\n#is\n#a\n#comment\n{");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ignoresWhitespace()
        {
            Assert.AreEqual(tokenLBrace(), lexSingle(" \t\n{ \t\n"));
        }

        // HARNESS BELOW
        // -------------

        public static Token lexSingle(String text)
        {
            Lexer lexer = new Lexer(makeReader(text));
            Token token = lexer.Pop();
            Assert.AreEqual(TokenType.End, lexer.Peek());
            return token;
        }


        public static Reader makeReader(String text)
        {
            return new Reader(new StringReader(text), "TEST");
        }

        public static Token tokenEnd()
        {
            return token(TokenType.End, "");
        }

        public static Token tokenInt(String number)
        {
            return token(TokenType.Integer, number);
        }

        public static Token tokenFloat(String number)
        {
            return token(TokenType.Double, number);
        }

        public static Token tokenHexInt(String number)
        {
            return token(TokenType.HexInt, number);
        }

        public static Token tokenString(String text)
        {
            return token(TokenType.String, text);
        }

        public static Token tokenBooleanTrue()
        {
            return token(TokenType.True, "true");
        }

        public static Token tokenBooleanFalse()
        {
            return token(TokenType.False, "false");
        }

        public static Token tokenNil()
        {
            return token(TokenType.Nil, "nil");
        }

        public static Token tokenNull()
        {
            return token(TokenType.Null, "null");
        }

        public static Token tokenLBrace()
        {
            return token(TokenType.LBrace, "{");
        }

        public static Token tokenRBrace()
        {
            return token(TokenType.RBrace, "}");
        }

        public static Token tokenColon()
        {
            return token(TokenType.Colon, ":");
        }

        public static Token tokenComma()
        {
            return token(TokenType.Comma, ",");
        }

        public static Token tokenEquals()
        {
            return token(TokenType.EqualSign, "=");
        }

        public static Token tokenIdentifier(String text)
        {
            return token(TokenType.Ident, text);
        }

        public static Token token(TokenType name, String text)
        {
            return token(name, text, new ParsingContext("TEST", 1, 1));
        }

        public static Token token(TokenType name, String text, ParsingContext context)
        {
            return token(name, text, context, "");
        }

        public static Token token(TokenType name, String text, ParsingContext context, String comment)
        {
            return new Token(name, text, context, comment);
        }

    }
}