﻿using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Statement that is based on expression. Expression 
    /// can be mathematical, conditional, assignment, function 
    /// or operator definition.
    /// </summary>
    public sealed class ExpressionStatement : Statement, IExpressionStatement
    {
        private string _terminatingKeyword;

        public IExpression Expression { get; private set; }

        public ExpressionStatement()
        {
        }

        public ExpressionStatement(string terminatingKeyword)
        {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            this.Expression = new Expression(true, _terminatingKeyword);
            if (this.Expression.Parse(context, this))
            {
                if(this.Expression.Children.Count == 1 && this.Expression.Children[0] is Expression)
                {
                    // Promote up
                    Expression = this.Expression.Children[0] as Expression;
                    Expression.Parent = null;
                    _children.RemoveAt(0);
                    Expression.Parent = this;
                }

                return base.Parse(context, parent);
            }

            return false;
        }

        public override string ToString()
        {
            return this.Expression.ToString() + base.ToString();
        }
    }
}