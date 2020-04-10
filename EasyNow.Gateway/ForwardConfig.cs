using System;

namespace EasyNow.Gateway
{
    public class ForwardConfig
    {
        public string Path { get; set; }
        public Uri Target { get; set; }
        public string TargetPath { get; set; }
    }
}