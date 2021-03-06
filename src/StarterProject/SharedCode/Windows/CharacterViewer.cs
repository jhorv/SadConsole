﻿using Microsoft.Xna.Framework;
using ColorHelper = Microsoft.Xna.Framework.Color;

using System;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Consoles;
using SadConsole.Effects;

namespace StarterProject.Windows
{
    class CharacterViewer : Window
    {
#region Control Declares
        private SadConsole.Controls.ScrollBar _charScrollBar;
        private Button _closeButton;
#endregion

        private Rectangle TrackedRegion = new Rectangle(1, 1, 16, 16);
        private SadConsole.Input.MouseInfo _lastInfo = null;
        private SadConsole.Effects.Recolor _highlightedCellEffect = new SadConsole.Effects.Recolor();
        private int _fontRowOffset = 0;
        private EffectsManager effects;

        public event EventHandler ColorsChanged;
        public int SelectedCharacterIndex = 0;
        public Color Foreground = Color.White;
        public Color Background = Color.Black;

#region Constructors
        public CharacterViewer()
            : base(27, 20)
        {
            //DefaultShowLocation = StartupLocation.CenterScreen;
            //Fill(Color.White, Color.Black, 0, null);
            Title = (char)198 + "Character" + (char)198;
            TitleAlignment = System.Windows.HorizontalAlignment.Left;
            //SetTitle(" Characters ", System.Windows.HorizontalAlignment.Center, Color.Blue, Color.LightGray);
            CloseOnESC = true;

            // CHARACTER SCROLL
            _charScrollBar = ScrollBar.Create(System.Windows.Controls.Orientation.Vertical, 16);
            _charScrollBar.Position = new Point(17, 1);
            _charScrollBar.Name = "ScrollBar";
            _charScrollBar.Maximum = textSurface.Font.Rows - 16;
            _charScrollBar.Value = 0;
            _charScrollBar.ValueChanged += new EventHandler(_charScrollBar_ValueChanged);
            _charScrollBar.IsEnabled = textSurface.Font.Rows > 16;

            // Effects
            effects = new EffectsManager(textSurface);

            // Add all controls
            this.Add(_charScrollBar);

            _closeButton = new Button(6) { Text = "Ok", Position = new Point(19, 1) }; Add(_closeButton); _closeButton.Click += (sender, e) => { DialogResult = true; Hide(); };

            _highlightedCellEffect.Foreground = Color.Blue;
            _highlightedCellEffect.Background = ColorHelper.DarkGray;

            // The frame will have been drawn by the base class, so redraw and our close button will be put on top of it
            Redraw();
        }

#endregion


        void _charScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _fontRowOffset = ((SadConsole.Controls.ScrollBar)sender).Value;

            int charIndex = _fontRowOffset * 16;
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetGlyph(x, y, charIndex);
                    charIndex++;
                }
            }
        }

        public override void Redraw()
        {
            base.Redraw();
            // Draw the border between char sheet area and the text area
            SetGlyph(0, textSurface.Height - 3, 204);
            SetGlyph(textSurface.Width - 1, textSurface.Height - 3, 185);
            for (int i = 1; i < textSurface.Width - 1; i++)
            {
                SetGlyph(i, textSurface.Height - 3, 205);
            }
            //SetCharacter(this.Width - 1, 0, 256);

            int charIndex = 0;
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetGlyph(x, y, charIndex);
                    charIndex++;
                }
            }
        }

        protected override void OnMouseLeftClicked(SadConsole.Input.MouseInfo data)
        {
            if (data.Cell != null && TrackedRegion.Contains(data.ConsoleLocation.X, data.ConsoleLocation.Y))
            {
                SelectedCharacterIndex = data.Cell.GlyphIndex;
            }
            else if (data.ConsoleLocation.X == textSurface.Width - 1 && data.ConsoleLocation.Y == 0)
                Hide();

            base.OnMouseLeftClicked(data);
        }

        protected override void OnMouseIn(SadConsole.Input.MouseInfo data)
        {
            if (data.Cell != null && TrackedRegion.Contains(data.ConsoleLocation.X, data.ConsoleLocation.Y))
            {
                // Draw the character index and value in the status area
                string[] items = new string[] { "Index: ", data.Cell.GlyphIndex.ToString() + " ", ((char)data.Cell.GlyphIndex).ToString() };

                items[2] = items[2].PadRight(textSurface.Width - 2 - (items[0].Length + items[1].Length));

                var text = items[0].CreateColored(ColorHelper.LightBlue, Theme.BorderStyle.Background, null) +
                           items[1].CreateColored(ColorHelper.LightCoral, Color.Black, null) +
                           items[2].CreateColored(ColorHelper.LightCyan, Color.Black, null);

                text.IgnoreBackground = true;
                text.IgnoreEffect = true;

                Print(1, textSurface.Height - 2, text);

                // Set the special effect on the current known character and clear it on the last known
                if (_lastInfo == null)
                {
                }
                else if (_lastInfo.ConsoleLocation != data.ConsoleLocation)
                {
                    effects.SetEffect(_lastInfo.Cell,
                    new SadConsole.Effects.Fade()
                    {
                        FadeBackground = true,
                        FadeForeground = true,
                        DestinationForeground = new ColorGradient(_highlightedCellEffect.Foreground, _lastInfo.Cell.Foreground),
                        DestinationBackground = new ColorGradient(_highlightedCellEffect.Background, _lastInfo.Cell.Background),
                        FadeDuration = 0.3d,
                        RemoveOnFinished = true,
                        UseCellBackground = false,
                        UseCellForeground = false,
                        CloneOnApply = true
                    }
                    );
                }

                effects.SetEffect(data.Cell, _highlightedCellEffect);
                _lastInfo = data.Clone();
            }
            else
            {
                DrawSelectedItemString();

                // Clear the special effect on the last known character
                if (_lastInfo != null)
                {
                    effects.SetEffect(_lastInfo.Cell, null);
                    _lastInfo = null;
                }
            }

            base.OnMouseIn(data);
        }

        private void DrawSelectedItemString()
        {
            // Clear the information area and redraw
            Print(1, textSurface.Height - 2, "".PadRight(textSurface.Width - 2));

            //string[] items = new string[] { "Current Index:", SelectedCharacterIndex.ToString() + " ", ((char)SelectedCharacterIndex).ToString() };
            string[] items = new string[] { "Selected: ", ((char)SelectedCharacterIndex).ToString(), " (", SelectedCharacterIndex.ToString(), ")" };
            items[4] = items[4].PadRight(textSurface.Width - 2 - (items[0].Length + items[1].Length + items[2].Length + items[3].Length));

            var text = items[0].CreateColored(ColorHelper.LightBlue, Theme.BorderStyle.Background, null) +
                       items[1].CreateColored(ColorHelper.LightCoral, Theme.BorderStyle.Background, null) +
                       items[2].CreateColored(ColorHelper.LightCyan, Theme.BorderStyle.Background, null) +
                       items[3].CreateColored(ColorHelper.LightCoral, Theme.BorderStyle.Background, null) +
                       items[4].CreateColored(ColorHelper.LightCyan, Theme.BorderStyle.Background, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, textSurface.Height - 2, text);
        }

        protected override void OnMouseExit(SadConsole.Input.MouseInfo info)
        {
            if (_lastInfo != null)
            {
                _lastInfo.Cell.Effect = null;
            }

            DrawSelectedItemString();

            base.OnMouseExit(info);
        }

        protected override void OnMouseEnter(SadConsole.Input.MouseInfo info)
        {
            if (_lastInfo != null)
            {
                _lastInfo.Cell.Effect = null;
            }

            base.OnMouseEnter(info);
        }

        public void UpdateCharSheetColors()
        {
            for (int y = 1; y < 17; y++)
            {
                for (int x = 1; x < 17; x++)
                {
                    SetForeground(x, y, Foreground);
                    SetBackground(x, y, Background);
                }
            }

            if (ColorsChanged != null)
                ColorsChanged(this, EventArgs.Empty);
        }

        public override void Show(bool modal)
        {
            UpdateCharSheetColors();

            string[] items = new string[] { "Current Index:", SelectedCharacterIndex.ToString() + " ", ((char)SelectedCharacterIndex).ToString() };

            items[2] = items[2].PadRight(textSurface.Width - 2 - (items[0].Length + items[1].Length));

            var text = items[0].CreateColored(ColorHelper.LightBlue, Color.Black, null) +
                       items[1].CreateColored(ColorHelper.LightCoral, Color.Black, null) +
                       items[2].CreateColored(ColorHelper.LightCyan, Color.Black, null);

            text.IgnoreBackground = true;
            text.IgnoreEffect = true;

            Print(1, textSurface.Height - 2, text);

            Center();

            base.Show(modal);
        }

        public override void Update()
        {
            effects.UpdateEffects(Engine.GameTimeElapsedUpdate);
            base.Update();
        }

    }
}
