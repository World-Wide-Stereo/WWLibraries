using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Controls
{
    public class SlidingPanel
    {
        Panel slidingPanel;
        Button actionButton;
        bool horizontal;
        bool hidden;
        string hideText;
        string showText;
        int size;
        Timer timer;
        public bool IsHidden { get { return hidden; } }

        public SlidingPanel(Panel dPanel, Button dButton, bool dHorizontal, string dHideText, string dShowText, bool dStartHidden = true)
        {
            slidingPanel = dPanel;
            actionButton = dButton;
            horizontal = dHorizontal;
            hidden = false;
            hideText = dHideText;
            showText = dShowText;

            if (horizontal) size = dPanel.Height;
            else size = dPanel.Width;

            dButton.Click += new EventHandler(ActionButton_Clicked);

            timer = new Timer();
            timer.Interval = 30;
            timer.Tick += new EventHandler(Timer_Tick);

            MenuStartPosition(dStartHidden);
        }

        private void MenuStartPosition(bool dStartHidden)
        {
            if (dStartHidden)
            {
                slidingPanel.Visible = false;
                actionButtonClicked();
                slidingPanel.Visible = true;
            }
        }

        void ChangeSize(int val)
        {
            if (horizontal)
            {
                slidingPanel.Height += val;

                if (slidingPanel.Height >= size || slidingPanel.Height <= 0)
                {
                    timer.Stop();
                    hidden = !hidden;
                }
            }
            else
            {
                slidingPanel.Width += val;

                if (slidingPanel.Width >= size || slidingPanel.Width <= 0)
                {
                    timer.Stop();
                    hidden = !hidden;
                }

            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (hidden)
            {
                ChangeSize(+30);
            }
            else
            {
                ChangeSize(-30);
            }
        }

        private void ActionButton_Clicked(object sender, EventArgs e)
        {
            actionButtonClicked();
        }

        private void actionButtonClicked()
        {
            if (hidden)
            {
                actionButton.Text = hideText;
            }
            else
            {
                actionButton.Text = showText;
            }

            timer.Start();
        }

        public void ExternalActionClick()
        {
            actionButtonClicked();
        }
    }
}
