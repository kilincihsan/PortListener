using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortListener
{
    internal class LogManager : IDisposable
    {
        private TextBox textBox;
        private StringBuilder buffer;

        public LogManager(TextBox textBox)
        {
            this.textBox = textBox;
            buffer = new StringBuilder();
        }

        public void Log(string message)
        {
            textBox.Invoke(() =>
            {
                buffer.Append(DateTime.Now.ToString("[HH:mm:ss] "));
                buffer.Append(message);
                buffer.Append(Environment.NewLine);
                textBox.Text = buffer.ToString();

                //scroll to end
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();

                return Task.CompletedTask;
            });
        }
        public void ClearLog()
        {
            buffer.Clear();
            textBox.Text = buffer.ToString();
        }

        public void Dispose()
        {
            textBox?.Dispose();
            buffer?.Clear();
        }
    }
}
