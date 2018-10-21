using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoTool {
    public class API {
        public String username;
        public String password;

        public API() {
            try {
                Process.Start(new ProcessStartInfo("curl.exe")).Start();
            } catch (Win32Exception e) {
                MessageBox.Show("NeoTool could not find curl.exe. Please make sure that curl.exe is in the same directory as NeoTool.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(2);
            }
        }

        public void Upload(String localFilePath, String newFilePath) {

        }

        public void Delete(String filePath) {

        }

        public String GetKey() {
            APIResponse r = MakeRequest("key", true);
            if (r == null) return null;

            return r.APIKey;
        }

        private APIResponse MakeRequest(String apiCall, bool useAuth, String extraArgs = "") {
            ProcessStartInfo s = new ProcessStartInfo("curl.exe", String.Format("{0} -k -s https://{1}neocities.org/api/{2}", extraArgs, useAuth == true ? username + ":" + password + "@" : "", apiCall));
            s.CreateNoWindow = true;
            s.UseShellExecute = false;
            s.RedirectStandardOutput = true;
            s.WindowStyle = ProcessWindowStyle.Hidden;
            Process p = Process.Start(s);
            p.WaitForExit();
            APIResponse r = JsonConvert.DeserializeObject<APIResponse>(p.StandardOutput.ReadToEnd());

            if (r.Result == "error") {
                if (r.ErrorType == "not_found") MessageBox.Show("The requested API call doesn't exist. This shouldn't happen at all, so please let lempamo know.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (r.ErrorType == "server_error") MessageBox.Show("Something went wrong on Neocities's end. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (r.ErrorType == "invalid_auth") MessageBox.Show("Inavlid username and/or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return null;
            }
            return r;
        }
    }

    public class APIResponse {
        [JsonProperty("result")]
        public String Result { get; set; }

        [JsonProperty("error_type")]
        public String ErrorType { get; set; }

        [JsonProperty("message")]
        public String ErrorMessage { get; set; }

        [JsonProperty("files")]
        public List<APIFileInfo> Files { get; set; }

        [JsonProperty("api_key")]
        public String APIKey { get; set; }
    }

    public class APIFileInfo {
        [JsonProperty("path")]
        public String FilePath { get; set; }

        [JsonProperty("is_directory")]
        public bool IsDirectory { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("date")]
        public String Date { get; set; }
    }
}
