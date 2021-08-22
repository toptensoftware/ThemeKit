using Topten.ThemeKit.Ast;
using System;
using Topten.ThemeKit.Runtime;

namespace Topten.ThemeKit
{
    internal class Parser
    {
        public Parser(StringSource Source)
        {
            _tokenizer = new Tokenizer(Source);
        }

        Tokenizer _tokenizer;

        public AstDocument Parse()
        {
            // Create the root document element
            var doc = new AstDocument(_tokenizer.Source.CreatePosition(0));

            // Parse everything
            while (_tokenizer.Token != Token.EOF)
            {
                if (_tokenizer.Token == Token.Identifier)
                {
                    switch (_tokenizer.TokenIdentifier)
                    {
                        case "var":
                            doc.Add(ParseVariableDeclaration());
                            continue;

                        case "import":
                            doc.Add(ParseImportDeclaration());
                            continue;

                        case "include":
                            var subDoc = ParseIncludeDirective();
                            foreach (var e in subDoc.Elements)
                            {
                                doc.Add(e);
                            }
                            continue;

                        case "class":
                            doc.Add(ParseClassDeclaration());
                            continue;
                    }
                }

                // What was it?
                throw _tokenizer.Unexpected("var, import or class keyword");
            }

            // Done
            return doc;
        }

        AstElement ParseVariableDeclaration()
        {
            // Skip "var"
            var pos = _tokenizer.TokenPosition;
            _tokenizer.Next();

            // Variable name
            _tokenizer.CheckToken(Token.Identifier);
            var name = _tokenizer.TokenIdentifier;
            _tokenizer.Next();

            // Assignment
            _tokenizer.SkipToken(Token.Assign);

            // Expression
            var value = ParseExpression();

            // Must end with a semi-colon
            _tokenizer.SkipToken(Token.SemiColon);

            return new AstVariableDeclaration(pos, name, value);
        }

        AstElement ParseImportDeclaration()
        {
            var pos = _tokenizer.TokenPosition;
            _tokenizer.Next();
            _tokenizer.SkipToken(Token.OpenRound);
            _tokenizer.CheckToken(Token.Literal);
            if (!(_tokenizer.TokenLiteral is string))
            {
                throw new CodeException("Expected string import declaration", _tokenizer.TokenPosition);
            }
            var node = new AstImportDeclaration(pos, (string)_tokenizer.TokenLiteral);
            _tokenizer.Next();
            _tokenizer.SkipToken(Token.CloseRound);
            _tokenizer.SkipToken(Token.SemiColon);
            return node;
        }

        AstDocument ParseIncludeDirective()
        {
            var pos = _tokenizer.TokenPosition;

            if (ResolveInclude == null)
                throw new CodeException("Can't resolve includes", pos);

            _tokenizer.Next();
            _tokenizer.SkipToken(Token.OpenRound);
            _tokenizer.CheckToken(Token.Literal);
            if (!(_tokenizer.TokenLiteral is string))
            {
                throw new CodeException("Expected string include declaration", _tokenizer.TokenPosition);
            }
            var name = (string)_tokenizer.TokenLiteral;
            _tokenizer.Next();
            _tokenizer.SkipToken(Token.CloseRound);
            _tokenizer.SkipToken(Token.SemiColon);

            // Load the included document
            StringSource source;
            try
            {
                source = ResolveInclude(pos, name);
            }
            catch (Exception x)
            {
                throw new CodeException($"Failed to resolve include '{name}' - {x.Message}", pos);
            }

            // Parse it
            var parser = new Parser(source);
            parser.ResolveInclude = this.ResolveInclude;
            return parser.Parse();
        }

        public Func<SourcePosition, string, StringSource> ResolveInclude;

        void ParseDottedName(out string className)
        {
            // Get class name
            _tokenizer.CheckToken(Token.Identifier);
            className = _tokenizer.TokenIdentifier;
            _tokenizer.Next();

            /*
            styleName = null;
            if (_tokenizer.TrySkipToken(Token.Period))
            {
                _tokenizer.CheckToken(Token.Identifier);
                styleName = _tokenizer.TokenIdentifier;
                _tokenizer.Next();
            }
            */
        }

        AstElement ParseClassDeclaration()
        {
            // Skip "class"
            var pos = _tokenizer.TokenPosition;
            _tokenizer.Next();

            // Parse class name
            string className;
            ParseDottedName(out className);

            // Create node
            var node = new AstClass(pos, className);

            // Base classes?
            if (_tokenizer.TrySkipToken(Token.Colon))
            {
                while (true)
                {
                    // Parse the base class name
                    ParseDottedName(out className);

                    // Add it
                    node.AddBase(className);

                    if (!_tokenizer.TrySkipToken(Token.Comma))
                        break;
                }
            }

            // Open brace
            _tokenizer.SkipToken(Token.OpenBrace);

            // Parse members
            while (_tokenizer.Token != Token.CloseBrace)
            {
                // Key
                var keyPos = _tokenizer.TokenPosition;
                var key = ParseExpression();

                // Colon
                _tokenizer.SkipToken(Token.Colon);

                // Value
                var value = ParseExpression();

                // Add it
                node.Add(keyPos, key, value);

                // Next?
                if (!_tokenizer.TrySkipToken(Token.Comma))
                    break;
            }

            // Skip close brace
            _tokenizer.SkipToken(Token.CloseBrace);

            return node;
        }

        // Parse a series of expressions and wrap them in
        // an unnamed AstExprNodeFunctionCall
        AstExprNode ParseExpressions()
        {
            var expr = ParseExpression();
            AstExprNodeFunctionCall fnCall = null;

            while (_tokenizer.Token != Token.Comma && _tokenizer.Token != Token.CloseBrace)
            {
                if (fnCall == null)
                {
                    fnCall = new AstExprNodeFunctionCall(expr.Position, null);
                    fnCall.AddArgument(expr);
                }
                fnCall.AddArgument(ParseExpression());
            }

            return fnCall ?? expr;
        }

        AstExprNode ParseExpression()
        {
            return ParseTernary();
        }

        AstExprNode ParseExprLeafNode()
        {
            var pos = _tokenizer.TokenPosition;
            AstExprNode node;
            switch (_tokenizer.Token)
            {
                case Token.Literal:
                    node = new AstExprNodeLiteral(pos, _tokenizer.TokenLiteral);
                    _tokenizer.Next();
                    break;

                case Token.ColorLiteral:
                    node = new AstExprNodeLiteral(pos, new ColorLiteral((uint)_tokenizer.TokenLiteral));
                    _tokenizer.Next();
                    break;

                case Token.OpenBrace:
                    {
                        // Create dictionary node
                        var dictnode = new AstExprNodeDictionary(pos);
                        node = dictnode;

                        // Skip open brace
                        _tokenizer.Next();

                        // Parse members
                        while (_tokenizer.Token != Token.CloseBrace)
                        {
                            var elPos = _tokenizer.TokenPosition;

                            // Get name
                            var key = ParseExpression();

                            // Colon
                            _tokenizer.SkipToken(Token.Colon);

                            // Value
                            var value = ParseExpression();

                            // Add it
                            dictnode.Add(elPos, key, value);

                            // Next?
                            if (!_tokenizer.TrySkipToken(Token.Comma))
                                break;
                        }

                        // Skip close brace
                        _tokenizer.SkipToken(Token.CloseBrace);
                        break;
                    }

                case Token.OpenSquare:
                    {
                        // Create dictionary node
                        var listnode = new AstExprNodeList(pos);
                        node = listnode;

                        // Skip open brace
                        _tokenizer.Next();

                        // Parse members
                        if (_tokenizer.Token != Token.CloseSquare)
                        {
                            while (true)
                            {
                                // Value
                                var value = ParseExpression();

                                // Add it
                                listnode.Add(value);

                                if (!_tokenizer.TrySkipToken(Token.Comma))
                                    break;
                            }
                        }

                        // Skip close brace
                        _tokenizer.SkipToken(Token.CloseSquare);
                        break;
                    }

                case Token.Identifier:
                    {
                        var name = _tokenizer.TokenIdentifier;
                        _tokenizer.Next();

                        if (_tokenizer.Token == Token.OpenRound)
                        {
                            // Create function call node
                            var fnnode = new AstExprNodeFunctionCall(pos, name);
                            node = fnnode;

                            // Skip open round
                            _tokenizer.Next();

                            // Parse arguments
                            while (_tokenizer.Token != Token.CloseRound)
                            {
                                fnnode.AddArgument(ParseExpression());
                                if (!_tokenizer.TrySkipToken(Token.Comma))
                                    break;
                            }

                            // Skip closing round
                            _tokenizer.SkipToken(Token.CloseRound);
                        }
                        else
                        {
                            node = new AstExprNodeIdentifier(pos, name);
                            /*
                            while (_tokenizer.TrySkipToken(Token.Period))
                            {
                                _tokenizer.CheckToken(Token.Identifier);
                                var rhs = new AstExprNodeIdentifier(pos, _tokenizer.TokenIdentifier);
                                rhs.LHS = node;
                                node = rhs;
                                _tokenizer.Next();
                            }
                            */
                        }
                        break;
                    }

                case Token.OpenRound:
                    _tokenizer.Next();
                    node = ParseTernary();
                    _tokenizer.SkipToken(Token.CloseRound);
                    break;

                default:
                    throw _tokenizer.Unexpected("an expression");        // Will throw
            }

            return node;
        }


        AstExprNode ParseUnary()
        {
            while (true)
            {
                var pos = _tokenizer.TokenPosition;
                switch (_tokenizer.Token)
                {
                    case Token.Minus:
                        _tokenizer.Next();
                        return new AstExprNodeUnaryOp(pos)
                        {
                            Operator = OperatorType.Negate,
                            RHS = ParseUnary(),
                        };

                    case Token.LogicalNot:
                        _tokenizer.Next();
                        return new AstExprNodeUnaryOp(pos)
                        {
                            Operator = OperatorType.LogicalNot,
                            RHS = ParseUnary(),
                        };

                    default:
                        return ParseExprLeafNode();
                }
            }
        }

        AstExprNode ParseBinaryOp(AstExprNode lhs, OperatorType op, Func<AstExprNode> rhs)
        {
            var pos = _tokenizer.TokenPosition;
            _tokenizer.Next();
            return new AstExprNodeBinaryOp(pos)
            {
                Operator = op,
                LHS = lhs,
                RHS = rhs(),
            };
        }

        AstExprNode ParseMulDiv()
        {
            var lhs = ParseUnary();

            while (true)
            {
                switch (_tokenizer.Token)
                {
                    case Token.Multiply:
                        lhs = ParseBinaryOp(lhs, OperatorType.Multiply, ParseUnary);
                        break;

                    case Token.Divide:
                        lhs = ParseBinaryOp(lhs, OperatorType.Divide, ParseUnary);
                        break;
                    default:
                        return lhs;
                }
            }
        }

        AstExprNode ParseAddSub()
        {
            var lhs = ParseMulDiv();

            while (true)
            {
                switch (_tokenizer.Token)
                {
                    case Token.Plus:
                        lhs = ParseBinaryOp(lhs, OperatorType.Add, ParseMulDiv);
                        break;

                    case Token.Minus:
                        lhs = ParseBinaryOp(lhs, OperatorType.Subtract, ParseMulDiv);
                        break;

                    default:
                        return lhs;
                }
            }
        }


        AstExprNode ParseShift()
        {
            var lhs = ParseAddSub();

            while (true)
            {
                switch (_tokenizer.Token)
                {
                    case Token.Shl:
                        lhs = ParseBinaryOp(lhs, OperatorType.ShiftLeft, ParseAddSub);
                        break;

                    case Token.Shr:
                        lhs = ParseBinaryOp(lhs, OperatorType.ShiftRight, ParseAddSub);
                        break;

                    default:
                        return lhs;
                }
            }
        }

        AstExprNode ParseRelational()
        {
            var lhs = ParseShift();

            while (true)
            {
                switch (_tokenizer.Token)
                {
                    case Token.LT:
                        lhs = ParseBinaryOp(lhs, OperatorType.CompareLT, ParseShift);
                        break;

                    case Token.LE:
                        lhs = ParseBinaryOp(lhs, OperatorType.CompareLE, ParseShift);
                        break;

                    case Token.GT:
                        lhs = ParseBinaryOp(lhs, OperatorType.CompareGT, ParseShift);
                        break;

                    case Token.GE:
                        lhs = ParseBinaryOp(lhs, OperatorType.CompareGE, ParseShift);
                        break;

                    default:
                        return lhs;
                }
            }
        }

        AstExprNode ParseEquality()
        {
            var lhs = ParseRelational();

            while (true)
            {
                switch (_tokenizer.Token)
                {
                    case Token.EQ:
                        lhs = ParseBinaryOp(lhs, OperatorType.CompareEQ, ParseRelational);
                        break;

                    case Token.NE:
                        lhs = ParseBinaryOp(lhs, OperatorType.CompareNE, ParseRelational);
                        break;

                    default:
                        return lhs;
                }
            }
        }

        AstExprNode ParseBitwiseAnd()
        {
            var lhs = ParseEquality();

            while (_tokenizer.Token == Token.BitwiseAnd)
            {
                lhs = ParseBinaryOp(lhs, OperatorType.BitwiseAnd, ParseEquality);
            }

            return lhs;
        }

        AstExprNode ParseBitwiseXor()
        {
            var lhs = ParseBitwiseAnd();

            while (_tokenizer.Token == Token.BitwiseXor)
            {
                lhs = ParseBinaryOp(lhs,  OperatorType.BitwiseXor, ParseBitwiseAnd);
            }

            return lhs;
        }

        AstExprNode ParseBitwiseOr()
        {
            var lhs = ParseBitwiseXor();

            while (_tokenizer.Token == Token.BitwiseOr)
            {
                lhs = ParseBinaryOp(lhs, OperatorType.BitwiseOr, ParseBitwiseXor);
            }

            return lhs;
        }

        AstExprNode ParseLogicalAnd()
        {
            var lhs = ParseBitwiseOr();

            while (_tokenizer.Token == Token.LogicalAnd)
            {
                lhs = ParseBinaryOp(lhs, OperatorType.LogicalAnd, ParseBitwiseOr);
            }

            return lhs;
        }

        AstExprNode ParseLogicalOr()
        {
            var lhs = ParseLogicalAnd();

            while (_tokenizer.Token == Token.LogicalOr)
            {
                lhs = ParseBinaryOp(lhs, OperatorType.LogicalOr, ParseLogicalAnd);
                break;
            }

            return lhs;
        }

        AstExprNode ParseTernary()
        {
            var lhs = ParseLogicalOr();

            var position = _tokenizer.TokenPosition;

            if (!_tokenizer.TrySkipToken(Token.Question))
                return lhs;

            var trueClause = ParseTernary();

            _tokenizer.SkipToken(Token.Colon);

            var falseClause = ParseTernary();

            return new AstExprNodeTernaryOp(position)
            {
                Condition = lhs,
                TrueNode = trueClause,
                FalseNode = falseClause,
            };
        }

    }
}

