using System;

namespace Musiccast.Model
{
    public class MusicCastDevice
    {
        public string FriendlyName { get; set; }
        public string UDN { get; set; }
        public string ImagePath { get; set; }
        public byte ImageSize { get; set; }
        public string Power { get; set; }
        public Inputs Input { get; set; }
        public string Location { get; set; }
        public string Id { get; set; }
        public string ModelName { get; set; }
        public string Zone { get; set; }
        public string NowPlayingInformation { get; set; }
        public int Volume { get; set; }
        public int MaxVolume { get; set; }
        public Uri BaseUri { get; set; }
    }
}
