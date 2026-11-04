// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using Avalonia.Controls;
using Avalonia.Input;

namespace WinGetXAvalonia.Controls.Shared;

public partial class FounderSignature : UserControl
{
    private const double RestingOpacity = 0.15;
    private const double RevealOpacity = 0.35;

    public FounderSignature()
    {
        InitializeComponent();

        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
        => SignatureText.Opacity = RevealOpacity;

    private void OnPointerExited(object? sender, PointerEventArgs e)
        => SignatureText.Opacity = RestingOpacity;
}