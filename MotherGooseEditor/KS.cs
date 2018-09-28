using System.Collections.Generic;
using System.Text;

namespace MotherGooseEditor {
    public class KS {
        public Encoding Encoding = Encoding.GetEncoding(932);
        string[] Script;
        public KS(byte[] Script) {
            this.Script = Encoding.GetString(Script).Replace("\r\n", "\n").Split('\n');
        }

        const string OpenStr = "[message";
        const string CloseStr = "[/message]";
        public string[] Import() {
            List<string> Strings = new List<string>();

            bool InMessage = false;
            string Buffer = string.Empty;
            for (uint i = 0; i < Script.Length; i++) {
                string Line = Script[i].Trim();
                if (Line.ToLower().StartsWith(OpenStr)) {
                    InMessage = true;
                    continue;
                }
                if (Line.ToLower().StartsWith(CloseStr)) {
                    if (InMessage) {
                        Strings.Add(Buffer.Substring(0, Buffer.Length - 1));//Remove the last \n
                    }
                    Buffer = string.Empty;
                    InMessage = false;
                }
                if (!InMessage || Line.StartsWith(";"))
                    continue;

                Buffer += Line + "\n";
            }

            return Strings.ToArray();
        }

        public byte[] Export(string[] Lines) {
            StringBuilder SB = new StringBuilder();
            bool InMessage = false;
            for (uint i = 0, x = 0; i < Script.Length; i++) {
                string Line = Script[i].Trim();
                if (Line.ToLower().StartsWith(OpenStr)) {
                    SB.AppendLine(Script[i]);
                    InMessage = true;
                    continue;
                }
                if (Line.ToLower().StartsWith(CloseStr)) {
                    if (InMessage) {
                        string[] LNs = Lines[x++].Split('\n');
                        foreach (string Ln in LNs) {
                            SB.AppendLine("\t" + Ln);
                        }
                    }
                    InMessage = false;
                }
                if (!InMessage || Line.StartsWith(";")) {
                    SB.AppendLine(Script[i]);
                    continue;
                }
            }

            return Encoding.GetBytes(SB.ToString());
        }
    }
}
