/*  PP.PdfBoss.ViewModels\Home\Dialogs\ProcessingDialogViewModel.cs
 *
 *  Copyright 2024 Paulo Pocinho.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using PP.PdfBoss.Core.MessageTypes;

namespace PP.PdfBoss.ViewModels.Home.Dialogs;

public partial class ProcessingDialogViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string? _processingStatus;

    protected override void OnActivated()
    {
        base.OnActivated();

        WeakReferenceMessenger.Default.Register<StatusOperationMessage>(this, (r, m)
            => ProcessingStatus = m.Value);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        WeakReferenceMessenger.Default.Unregister<StatusOperationMessage>(this);
    }
}
