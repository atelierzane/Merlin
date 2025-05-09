using System;
using System.Windows;
using MerlinPointOfSale.Helpers;
using MerlinPointOfSale.Style.Class;
using System.Windows.Input;

namespace MerlinPointOfSale.Helpers
{
    public class InputHelper
    {
        private readonly Window targetWindow;
        private readonly VisualEffectsHelper visualEffectsHelper;

        public InputHelper(Window window, VisualEffectsHelper visualEffectsHelper)
        {
            targetWindow = window;
            this.visualEffectsHelper = visualEffectsHelper;

            targetWindow.MouseMove += OnWindowMouseMove;
            targetWindow.MouseLeave += OnWindowMouseLeave;
            targetWindow.Activated += OnWindowActivated;
            targetWindow.Deactivated += OnWindowDeactivated;
        }

        private void OnWindowMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(targetWindow);
            visualEffectsHelper.AdjustBorderGlow(mousePosition);
        }

        private void OnWindowMouseLeave(object sender, MouseEventArgs e)
        {
            visualEffectsHelper.FadeOutGlowEffect();
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            visualEffectsHelper.AnimateWindowOpacity(0.45, 1.0);
            visualEffectsHelper.AnimateBlurEffect(2.5, 0);
        }

        private void OnWindowDeactivated(object sender, EventArgs e)
        {
            visualEffectsHelper.AnimateWindowOpacity(1.0, 0.45);
            visualEffectsHelper.AnimateBlurEffect(0, 2.5);
        }

        public void UnsubscribeEvents()
        {
            targetWindow.MouseMove -= OnWindowMouseMove;
            targetWindow.MouseLeave -= OnWindowMouseLeave;
            targetWindow.Activated -= OnWindowActivated;
            targetWindow.Deactivated -= OnWindowDeactivated;
        }

        public void CloseWindow()
        {
            targetWindow.Close();
        }

        public void MinimizeWindow()
        {
            targetWindow.WindowState = WindowState.Minimized;
        }

        public void MaximizeWindow()
        {
            if (targetWindow.WindowState != WindowState.Maximized)
            {
                targetWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                targetWindow.WindowState = WindowState.Normal;
            }
        }
    }
}
