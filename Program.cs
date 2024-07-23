using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PresetsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateXML(AnalyzeAllPresets());
        }

        


        static List<PresetEntry> AnalyzeAllPresets()
        {
            List<PresetEntry> list = new List<PresetEntry>();

            var dirs = Directory.GetDirectories("./presets");

            Trace($"found {dirs.Length} directories");

            foreach (var dir in dirs)
            {
                Trace("reading dir --> " + dir);

                var files = Directory.GetFiles(dir);

                Trace($"found {files.Length} files");

                foreach (var file in files)
                {
                    if (file.EndsWith(".txt"))
                    {
                        var content = File.ReadAllText(file);

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var name = ReadValue(content, "name");
                            var ip = ReadValue(content, "ip");
                            var port = ReadValue(content, "port");
                            var clientVersion = ReadValue(content, "client_version");
                            var encryption = ReadValue(content, "encryption");

                            bool error = false;

                            if (name.IsEmpty)
                            {
                                Error("invalid name value");
                                error = true;
                            }
                            
                            if (ip.IsEmpty)
                            {
                                Error("invalid ip value");
                                error = true;
                            }

                            if (port.IsEmpty || !ushort.TryParse(port, out var _))
                            {
                                Error("invalid port value");
                                error = true;
                            }

                            if (clientVersion.IsEmpty)
                            {
                                Error("invalid client_version value");
                                error = true;
                            }

                            bool useEncryption = false;
                            if (encryption == "yes")
                            {
                                useEncryption = true;
                            }
                            else if (encryption == "no")
                            {
                                useEncryption = false;
                            }
                            else if (encryption.IsEmpty || !bool.TryParse(encryption, out useEncryption))
                            {
                                Warn("invalid encryption value. Value will be set to 'false'");
                            }

                            if (!error)
                            {
                                Trace("storing data to the list");

                                list.Add(new PresetEntry()
                                {
                                    Name = name.ToString(),
                                    IP = ip.ToString(),
                                    Port = port.ToString(),
                                    ClientVersion = clientVersion.ToString(),
                                    Encryption = useEncryption
                                });
                            }
                            else
                            {
                                Warn("error :(");
                            }
                        }
                        else
                        {
                            Error($"file '{file}' does not contains valid data");
                        }
                    }
                }
            }

            return list;
        }

        static void GenerateXML(List<PresetEntry> list)
        {
            using System.Xml.XmlTextWriter xml = new XmlTextWriter("presets.xml", Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                IndentChar = '\t',
                Indentation = 1
            };

            xml.WriteStartDocument(true);

            xml.WriteStartElement("presets");

            foreach (var e in list)
            {
                xml.WriteStartElement("preset");

                xml.WriteAttributeString("name", e.Name);
                xml.WriteAttributeString("ip", e.IP);
                xml.WriteAttributeString("port", e.Port);
                xml.WriteAttributeString("client_version", e.ClientVersion);
                xml.WriteAttributeString("encryption", e.Encryption.ToString());

                xml.WriteEndElement();
            }

            xml.WriteEndElement();

            xml.WriteEndDocument();
        }


        static ReadOnlySpan<char> ReadValue(string entry, string key)
        {
            int keyIdx = entry.IndexOf(key);
            if (keyIdx >= 0)
            {
                int valueIdx = entry.IndexOf('=', keyIdx);

                if (valueIdx >= 0)
                {
                    valueIdx++;
                    int endOfLineIdx = entry.IndexOf('\n', valueIdx);
                    
                    if (endOfLineIdx <= 0)
                    {
                        endOfLineIdx = entry.Length;
                    }

                    var span = entry.AsSpan(valueIdx, endOfLineIdx - valueIdx);

                    int rnIdx = span.IndexOf('\r');
                    if (rnIdx >= 0)
                    {
                        span = span.Slice(0, rnIdx);
                    }
                
                    if (!span.IsWhiteSpace())
                    {
                        return span;
                    }
                }
            }       

            return default; 
        }

        class PresetEntry
        {
            public string Name = string.Empty;
            public string IP = string.Empty;
            public string Port = string.Empty;
            public string ClientVersion = string.Empty;
            public bool Encryption;
        }


        static void Trace(string msg) => Console.WriteLine("[TRACE] {0}", msg);
        static void Warn(string msg) => Console.WriteLine("[WARN] {0}", msg);
        static void Error(string msg) => Console.WriteLine("[ERROR] {0}", msg);
    }
}
