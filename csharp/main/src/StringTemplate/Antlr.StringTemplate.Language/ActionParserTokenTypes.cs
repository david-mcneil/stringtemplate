// $ANTLR 2.7.6 (2005-12-22): "action.g" -> "ActionParser.cs"$

/*
 [The "BSD licence"]
 Copyright (c) 2003-2004 Terence Parr
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions
 are met:
 1. Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.
 2. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.
 3. The name of the author may not be used to endorse or promote products
    derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System.Collections;

namespace Antlr.StringTemplate.Language
{
	public class ActionParserTokenTypes
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int APPLY = 4;
		public const int MULTI_APPLY = 5;
		public const int ARGS = 6;
		public const int INCLUDE = 7;
		public const int CONDITIONAL = 8;
		public const int VALUE = 9;
		public const int TEMPLATE = 10;
		public const int FUNCTION = 11;
		public const int SINGLEVALUEARG = 12;
		public const int LIST = 13;
		public const int SEMI = 14;
		public const int LPAREN = 15;
		public const int RPAREN = 16;
		public const int COMMA = 17;
		public const int ID = 18;
		public const int ASSIGN = 19;
		public const int COLON = 20;
		public const int NOT = 21;
		public const int PLUS = 22;
		public const int LITERAL_super = 23;
		public const int DOT = 24;
		public const int LITERAL_first = 25;
		public const int LITERAL_rest = 26;
		public const int LITERAL_last = 27;
		public const int LITERAL_length = 28;
		public const int LITERAL_strip = 29;
		public const int LITERAL_trunc = 30;
		public const int ANONYMOUS_TEMPLATE = 31;
		public const int STRING = 32;
		public const int INT = 33;
		public const int LBRACK = 34;
		public const int RBRACK = 35;
		public const int DOTDOTDOT = 36;
		public const int TEMPLATE_ARGS = 37;
		public const int NESTED_ANONYMOUS_TEMPLATE = 38;
		public const int ESC_CHAR = 39;
		public const int WS = 40;
		public const int WS_CHAR = 41;
		
	}
}
