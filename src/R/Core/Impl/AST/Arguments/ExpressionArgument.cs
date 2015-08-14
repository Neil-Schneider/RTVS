﻿using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Arguments
{
    [DebuggerDisplay("[{Name}]")]
    public class ExpressionArgument : CommaSeparatedItem
    {
        public Expression ArgumentValue { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            this.ArgumentValue = new Expression();
            if (this.ArgumentValue.Parse(context, this))
            {
                return base.Parse(context, parent);
            }

            return false;
        }
    }
}