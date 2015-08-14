﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Controller;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Commands.Factory
{
    [Export(typeof(ICommandFactory))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class HtmlCommandFactory: ICommandFactory
    {
        public IEnumerable<ICommand> GetCommands(ITextView textView, ITextBuffer textBuffer)
        {
            List<ICommand> commands = new List<ICommand>();

            //commands.Add(new CommentSelectionCommand(textView, textBuffer));
            //commands.Add(new UncommentSelectionCommand(textView, textBuffer));
            //commands.Add(new FormatDocumentCommand(textView, textBuffer));
            //commands.Add(new FormatSelectionCommand(textView, textBuffer));
            commands.Add(new RTypingCommandHandler(textView));
            commands.Add(new RCompletionCommandHandler(textView, textBuffer));

            return commands;
        }
    }
}