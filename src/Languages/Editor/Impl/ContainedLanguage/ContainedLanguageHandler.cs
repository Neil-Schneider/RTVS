﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public abstract class ContainedLanguageHandler: IContainedLanguageHandler {
        protected TextRangeCollection<ITextRange> Blocks { get; } = new TextRangeCollection<ITextRange>();
        protected ITextBuffer TextBuffer { get; }

        private int _cachedPosition = -1;
        private ITextRange _cachedLanguageBlock;

        public ContainedLanguageHandler(ITextBuffer textBuffer) {
            TextBuffer = textBuffer;
            TextBuffer.Changed += OnTextBufferChanged;
        }

        protected abstract void OnTextBufferChanged(object sender, TextContentChangedEventArgs e);

        #region IContainedLanguageHandler
        /// <summary>
        /// Retrieves contained command target for a given location in the buffer.
        /// </summary>
        /// <param name="position">Position in the document buffer</param>
        /// <returns>Command target or null if location appears to be primary</returns>
        public abstract ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition);
        #endregion

        protected ITextRange GetLanguageBlockOfLocation(int bufferPosition) {
            if (_cachedPosition != bufferPosition) {
                var index = Blocks.GetItemAtPosition(bufferPosition);
                _cachedLanguageBlock = index >= 0 ? Blocks[index] : null;
                _cachedPosition = bufferPosition;
            }
            return _cachedLanguageBlock;
        }
    }
}
