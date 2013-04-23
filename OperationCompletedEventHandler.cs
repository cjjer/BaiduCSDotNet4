using System;

namespace CloudAPI
{
    public delegate void OperationCompletedEventHandler(object sender, OperationCompletedEventArgs e);

    public class OperationCompletedEventArgs : EventArgs
    {
        public string QueryText { get; set; }
        public int ResourcesReturned { get; set; }
        public TimeSpan TimeTaken { get; set; }
    }
}
