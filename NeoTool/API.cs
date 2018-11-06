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
        public string username;
        public string password;

        public API() {
            try {
                Process.Start(new ProcessStartInfo("curl.exe")).Start();
            } catch (Win32Exception) {
                MessageBox.Show("NeoTool could not find curl.exe. Please make sure that curl.exe is in the same directory as NeoTool.", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(2);
            }
        }

        public void Upload(string localFilePath, string newFilePath) {
            try { MakeRequest("upload", true, string.Format("-F \"{0}=@{1}\"", newFilePath.Replace('\\', '/'), localFilePath)); } catch (APIException e) { throw e; }
        }

        public void Delete(string filePath) {
            try { MakeRequest("delete", true, string.Format("-d \"filenames[]={0}\"", filePath)); } catch (APIException e) { throw e; }
        }

        public string GetFile(string filePath) {
            ProcessStartInfo s = new ProcessStartInfo("curl.exe", string.Format("-k -s https://{0}.neocities.org/{1}", username, filePath)) {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process p = Process.Start(s);
            string str = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return str;
        }

        public List<APIFileInfo> GetFileInfos() {
            APIResponse r;
            try { r = MakeRequest("list", true); } catch (APIException e) { throw e; }

            return r.Files;
        }

        public string GetKey() {
            APIResponse r;
            try { r = MakeRequest("key", true); } catch (APIException e) { throw e; }

            return r.APIKey;
        }

        private APIResponse MakeRequest(string apiCall, bool useAuth, string extraArgs = "") {
            ProcessStartInfo s = new ProcessStartInfo("curl.exe", string.Format("{0} -k -s {1} https://neocities.org/api/{2}", extraArgs, useAuth == true ? "-u " + username + ":" + password : "", apiCall)) {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process p = Process.Start(s);
            string str = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            APIResponse r = JsonConvert.DeserializeObject<APIResponse>(str);

            if (r.Result == "error") {
                if (r.ErrorType == "not_found") MessageBox.Show("The requested API call doesn't exist. This shouldn't happen at all, so please let lempamo know.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (r.ErrorType == "server_error") MessageBox.Show("Something went wrong on Neocities's end. Please try again later.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else if (r.ErrorType == "invalid_auth") MessageBox.Show("Inavlid username and/or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                throw new APIException();
            }
            return r;
        }
    }

    public class APIException : Exception {
        public override string Message => "Something went wrong with calling the NeoCities API.";
    }

    public class APIResponse {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("error_type")]
        public string ErrorType { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("files")]
        public List<APIFileInfo> Files { get; set; }

        [JsonProperty("api_key")]
        public string APIKey { get; set; }
    }

    public class APIFileInfo {
        [JsonProperty("path")]
        public string FilePath { get; set; }

        [JsonProperty("is_directory")]
        public bool IsDirectory { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }
    }
}
