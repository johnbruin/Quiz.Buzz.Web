using System;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;

namespace Quiz.Buzz.SystemTrayApp
{
    public class ViewManager
    {
        private BackgroundService _backgroundService;

        public ViewManager(BackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
            _backgroundService.LogChanged += _backgroundService_LogChanged;
            
            _components = new System.ComponentModel.Container();
            
            _notifyIcon = new NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = Properties.Resources.QuizBuzzIcon,
                Text = _backgroundService.BuzzerManager.Buzzers.Count * 4 + " buzzers connected",
                Visible = true,
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            _notifyIcon.MouseUp += NotifyIcon_MouseUp;

            _statusViewModel = new ViewModel.StatusViewModel
            {
                Icon = AppIcon
            };

            _hiddenWindow = new System.Windows.Window();
            _hiddenWindow.Closed += _hiddenWindow_Closed;
            _hiddenWindow.Hide();
        }

        private void _hiddenWindow_Closed(object sender, EventArgs e)
        {
            Stop();
        }

        private void _backgroundService_LogChanged(object sender, LogEventArgs e)
        {
            _statusItems.Add(new KeyValuePair<string, string>(DateTime.Now.ToString("HH:mm:ss:fff"), e.Message));
            _statusViewModel.SetStatusFlags(_statusItems);
        }

        System.Windows.Media.ImageSource AppIcon
        {
            get
            {
                System.Drawing.Icon icon = Properties.Resources.QuizBuzzIcon;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle, 
                    System.Windows.Int32Rect.Empty, 
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }

        // This allows code to be run on a GUI thread
        private System.Windows.Window _hiddenWindow;

        private System.ComponentModel.IContainer _components;

        // The Windows system tray class
        private NotifyIcon _notifyIcon;

        private List<KeyValuePair<string, string>> _statusItems = new List<KeyValuePair<string, string>>();
        private View.StatusView _statusView;
        private ViewModel.StatusViewModel _statusViewModel;

        private ToolStripMenuItem _exitMenuItem;

        private void DisplayStatusMessage(string text)
        {
            _hiddenWindow.Dispatcher.Invoke(delegate
            {
                _notifyIcon.BalloonTipText = text;
                // The timeout is ignored on recent Windows
                _notifyIcon.ShowBalloonTip(3000);
            });
        }
    
        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, string tooltipText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null)
            {
                item.Click += eventHandler;
            }

            item.ToolTipText = tooltipText;
            return item;
        }
        
        private void ShowStatusView()
        {
            if (_statusView == null)
            {
                _statusView = new View.StatusView
                {
                    DataContext = _statusViewModel
                };
                
                _statusView.Closing += ((arg_1, arg_2) => _statusView = null);
                _statusView.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                _statusView.Show();
            }
            else
            {
                _statusView.Activate();
            }
            _statusView.Icon = AppIcon;
        }

        private void ShowStatusItem_Click(object sender, EventArgs e)
        {
            ShowStatusView();
        }
       
        public void Stop()
        {
            _notifyIcon.Visible = false;
            _backgroundService.Stop();
            _backgroundService = null;
            Application.Exit();
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowStatusView();
        }

        private void NotifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(_notifyIcon, null);
            }
        }        

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (_notifyIcon.ContextMenuStrip.Items.Count == 0)
            {
                _notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("Device S&tatus", "Shows the device status dialog", ShowStatusItem_Click));
                _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                _exitMenuItem = ToolStripMenuItemWithHandler("&Exit", "Exits System Tray App", ExitItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
            }
        }
    }
}