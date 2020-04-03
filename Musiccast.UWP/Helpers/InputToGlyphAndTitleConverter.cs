using Musiccast.Model;
using Musiccast.Models;
using System;

namespace Musiccast.Helpers
{
    public class InputToGlyphAndTitleConverter
    {
        public static Input ConvertInput(string input)
        {
            var output = Inputs.non;
            var icon = "\uE700";
            var name = input;

            if(Enum.TryParse(input, out output))
            {
                switch (output)
                {
                    case Inputs.cd:
                        icon = "\uE958";
                        name = "CD";
                        break;
                    case Inputs.tuner:
                        icon = "\uE720";
                        name = "Tuner";
                        break;
                    case Inputs.multi_ch:
                        icon = "\uE700";
                        name = "Multi Channel";
                        break;
                    case Inputs.phono:
                        icon = "\uE700";
                        name = "Phono";
                        break;
                    case Inputs.hdmi1:
                        icon = "\uE700";
                        name = "Hdmi 1";
                        break;
                    case Inputs.hdmi2:
                        icon = "\uE700";
                        name = "Hdmi 2";
                        break;
                    case Inputs.hdmi3:
                        icon = "\uE700";
                        name = "Hdmi 3";
                        break;
                    case Inputs.hdmi4:
                        icon = "\uE700";
                        name = "Hdmi 4";
                        break;
                    case Inputs.hdmi5:
                        icon = "\uE700";
                        name = "Hdmi 5";
                        break;
                    case Inputs.hdmi6:
                        icon = "\uE700";
                        name = "Hdmi 6";
                        break;
                    case Inputs.hdmi7:
                        icon = "\uE700";
                        name = "Hdmi 7";
                        break;
                    case Inputs.hdmi8:
                        icon = "\uE700";
                        name = "Hdmi 8";
                        break;
                    case Inputs.hdmi:
                        icon = "\uE700";
                        name = "Hdmi";
                        break;
                    case Inputs.av1:
                        icon = "\uE700";
                        name = "Tuner";
                        break;
                    case Inputs.av2:
                        icon = "\uE700";
                        name = "Tuner";
                        break;
                    case Inputs.av3:
                        icon = "\uE700";
                        name = "Av 3";
                        break;
                    case Inputs.av4:
                        icon = "\uE700";
                        name = "Av 4";
                        break;
                    case Inputs.av5:
                        icon = "\uE700";
                        name = "Av 5";
                        break;
                    case Inputs.av6:
                        icon = "\uE700";
                        name = "Av 6";
                        break;
                    case Inputs.av7:
                        icon = "\uE700";
                        name = "Av 7";
                        break;
                    case Inputs.v_aux:
                        icon = "\uE700";
                        name = "V Aux";
                        break;
                    case Inputs.aux1:
                        icon = "\uE700";
                        name = "Aux 1";
                        break;
                    case Inputs.aux2:
                        icon = "\uE700";
                        name = "Aux 2";
                        break;
                    case Inputs.aux:
                        icon = "\uE700";
                        name = "Aux";
                        break;
                    case Inputs.audio1:
                        icon = "\uE700";
                        name = "Audio 1";
                        break;
                    case Inputs.audio2:
                        icon = "\uE700";
                        name = "Audio 2";
                        break;
                    case Inputs.audio3:
                        icon = "\uE700";
                        name = "Audio 3";
                        break;
                    case Inputs.audio4:
                        icon = "\uE700";
                        name = "Audio 4";
                        break;
                    case Inputs.audio_cd:
                        icon = "\uE700";
                        name = "Audio CD";
                        break;
                    case Inputs.audio:
                        icon = "\uE700";
                        name = "Audio";
                        break;
                    case Inputs.optical1:
                        icon = "\uE700";
                        name = "Optical 1";
                        break;
                    case Inputs.optical2:
                        icon = "\uE700";
                        name = "Optical 2";
                        break;
                    case Inputs.optical:
                        icon = "\uE700";
                        name = "Optical";
                        break;
                    case Inputs.coaxial1:
                        icon = "\uE700";
                        name = "Coaxial 1";
                        break;
                    case Inputs.coaxial2:
                        icon = "\uE700";
                        name = "Coaxial 2";
                        break;
                    case Inputs.coaxial:
                        icon = "\uE700";
                        name = "Coaxial";
                        break;
                    case Inputs.digital1:
                        icon = "\uE700";
                        name = "Digital 1";
                        break;
                    case Inputs.digital2:
                        icon = "\uE700";
                        name = "Digital 2";
                        break;
                    case Inputs.digital:
                        icon = "\uE700";
                        name = "Digital";
                        break;
                    case Inputs.line1:
                        icon = "\uE700";
                        name = "Line 1";
                        break;
                    case Inputs.line2:
                        icon = "\uE700";
                        name = "Line 2";
                        break;
                    case Inputs.line3:
                        icon = "\uE700";
                        name = "Line 3";
                        break;
                    case Inputs.line_cd:
                        icon = "\uE700";
                        name = "Line CD";
                        break;
                    case Inputs.analog:
                        icon = "\uE700";
                        name = "Analog";
                        break;
                    case Inputs.tv:
                        icon = "\uE700";
                        name = "TV";
                        break;
                    case Inputs.bd_dvd:
                        icon = "\uE700";
                        name = "BD DVD";
                        break;
                    case Inputs.usb_dac:
                        icon = "\uECF0";
                        name = "USB DAC";
                        break;
                    case Inputs.usb:
                        icon = "\uECF0";
                        name = "USB";
                        break;
                    case Inputs.bluetooth:
                        icon = "\uEC41";
                        name = "Bluetooth";
                        break;
                    case Inputs.server:
                        icon = "\uE700";
                        name = "Server";
                        break;
                    case Inputs.net_radio:
                        icon = "\uE700";
                        name = "NET Radio";
                        break;
                    case Inputs.rhapsody:
                        icon = "\uE700";
                        name = "Rhapsody";
                        break;
                    case Inputs.napster:
                        icon = "\uE700";
                        name = "Napster";
                        break;
                    case Inputs.pandora:
                        icon = "\uE700";
                        name = "Pandora";
                        break;
                    case Inputs.siriusxm:
                        icon = "\uE700";
                        name = "Sirius XM";
                        break;
                    case Inputs.spotify:
                        icon = "\uE700";
                        name = "Spotify";
                        break;
                    case Inputs.juke:
                        icon = "\uE700";
                        name = "Juke";
                        break;
                    case Inputs.airplay:
                        icon = "\uED5C";
                        name = "Airplay";
                        break;
                    case Inputs.radiko:
                        icon = "\uE700";
                        name = "RADIKO";
                        break;
                    case Inputs.qobuz:
                        icon = "\uE700";
                        name = "QOBUZ";
                        break;
                    case Inputs.mc_link:
                        icon = "\uE71B";
                        name = "MC Link";
                        break;
                    case Inputs.main_sync:
                        icon = "\uE700";
                        name = "Main Sync";
                        break;
                    case Inputs.non:
                        icon = "\uE700";
                        name = "NONE";
                        break;
                }
            }

            return new Input()
            {
                Id = input,
                Icon = icon,
                Name = name
            };
        }
    }
}
