using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Avalonia.VTheme.Controls {
    /// <summary>
    /// A window-embeded notification popup.
    /// </summary>
    [TemplatePart("PART_MessageType", typeof(TextBlock))]
    [TemplatePart("PART_MessageText", typeof(TextBlock))]
    [TemplatePart("PART_Time", typeof(ProgressBar))]
    [PseudoClasses(":popupshow", ":popuphide")]
    public partial class NotifyPopup : TemplatedControl {
        private const int ANIMATION_TIME = 1000;

        private readonly List<NotifyPopupInfo> _queue;
        private bool _isDisplaying;

        private TextBlock _partMessageType;
        private TextBlock _partMessageText;
        private ProgressBar _partTime;

        /// <summary>
        /// Constructs a <see cref="NotifyPopup"/>.
        /// </summary>
        public NotifyPopup() {
            _queue = [];
            _isDisplaying = false;

            _partMessageType = null!;
            _partMessageText = null!;
            _partTime = null!;
        }

        /// <summary>
        /// Gets the <see cref="NotifyPopup"/> of a window through its <see cref="TemplateAppliedEventArgs"/>.
        /// </summary>
        /// <param name="e">The template event.</param>
        /// <returns>The NotifyPopup, if <c>PART_NotifyPopup</c> is found; Otherwise, returns <c>null</c>.</returns>
        public static NotifyPopup? GetPopup(TemplateAppliedEventArgs e) {
            return e.NameScope.Find<NotifyPopup>("PART_NotifyPopup");
        }

        /// <summary>
        /// Queues a notification to be displayed.
        /// </summary>
        /// <param name="info">The notification information.</param>
        public void Queue(NotifyPopupInfo info) {
            _queue.Add(info);
            DisplayAllPopups();
        }

        private async void DisplayAllPopups() {
            if (_isDisplaying)
                return;

            _isDisplaying = true;

            for (int i = 0; i < _queue.Count; i++) {
                var info = _queue[i];

                _partMessageType.Text = GetTypeSymbol(info);
                _partMessageText.Text = info.Text;

                var duration = Math.Max(info.DisplayTime, 1000);
                _partTime.Minimum = 0;
                _partTime.Maximum = duration;
                _partTime.Value = duration;

                PseudoClasses.Set(":popuphide", false);
                PseudoClasses.Set(":popupshow", true);
                await Task.Delay(ANIMATION_TIME);

                var sw = Stopwatch.StartNew();

                while (sw.ElapsedMilliseconds < duration) {
                    _partTime.Value = duration - sw.ElapsedMilliseconds;
                    await Task.Delay(8);
                }

                _partTime.Value = 0;
                PseudoClasses.Set(":popupshow", false);
                PseudoClasses.Set(":popuphide", true);
                await Task.Delay(ANIMATION_TIME);
            }

            _queue.Clear();
            _isDisplaying = false;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);

            _partMessageType = e.NameScope.Find<TextBlock>("PART_MessageType") ?? throw new KeyNotFoundException("PART_MessageType not found.");
            _partMessageText = e.NameScope.Find<TextBlock>("PART_MessageText") ?? throw new KeyNotFoundException("PART_MessageText not found.");
            _partTime = e.NameScope.Find<ProgressBar>("PART_Time") ?? throw new KeyNotFoundException("PART_Time not found.");
        }

        private static string GetTypeSymbol(NotifyPopupInfo info) {
            return info.Icon switch {
                NotifyIcon.Information => "\uea74",
                NotifyIcon.Warning => "\uea6c",
                NotifyIcon.Error => "\uea87",
                _ => string.Empty,
            };
        }
    }

    /// <summary>
    /// A notification for a <see cref="NotifyPopup"/>.
    /// </summary>
    public class NotifyPopupInfo {
        /// <summary>
        /// The icon to display.
        /// </summary>
        public NotifyIcon Icon { get; init; }

        /// <summary>
        /// The message to show.
        /// </summary>
        public string Text { get; init; } = string.Empty;

        /// <summary>
        /// How long this message should be displayed for, in milliseconds.
        /// </summary>
        public int DisplayTime { get; init; } = 1000;
    }

    /// <summary>
    /// The icon to display in a <see cref="NotifyPopup"/> notification.
    /// </summary>
    public enum NotifyIcon {
        Information,
        Warning,
        Error
    }
}