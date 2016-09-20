﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Plots.Implementation.Commands {
    internal sealed class PlotHistoryZoomOutCommand : PlotHistoryCommand, IAsyncCommand {
        public PlotHistoryZoomOutCommand(IRInteractiveWorkflow interactiveWorkflow, IRPlotHistoryVisualComponent visualComponent) :
            base(interactiveWorkflow, visualComponent) {
        }

        public CommandStatus Status {
            get {
                if (VisualComponent.CanDecreaseThumbnailSize) {
                    return CommandStatus.SupportedAndEnabled;
                }

                return CommandStatus.Supported;
            }
        }

        public Task<CommandResult> InvokeAsync() {
            VisualComponent.DecreaseThumbnailSize();
            return Task.FromResult(CommandResult.Executed);
        }
    }
}
