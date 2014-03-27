using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Sharush.Enums;
using Sharush.DataTypes;
using Sharush.DataTypes.Flags;
using Sharush.DataTypes.Metadata;

namespace Sharush
{
    public static class Sharush
    {
        private static string UserAgent { get { return string.Format("Sharus .NET library ({0})", Environment.OSVersion.Platform); } }

        public static string ApplicationUrl { get; set; }

        static Sharush()
        {
            ApplicationUrl = "https://mediacru.sh";
        }

        public static KeyValuePair<AlbumMakeStatus, string> MakeAlbum(string[] hashes)
        {
            if (hashes == null || hashes.Count() == 0)
            {
                throw new ArgumentNullException("Argument 'hashes' cannot be null or empty");
            }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/album/create", ApplicationUrl));

            wr.Headers.Add("X-CORS-Status", "1");
            wr.UserAgent = UserAgent;
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashes.Length; i++)
            {
                sb.Append(hashes[i]);
                if (i != hashes.Length - 1)
                {
                    sb.Append(",");
                }
            }

            using (Stream requestStream = wr.GetRequestStream())
            {
                string y = string.Format("{0}={1}", "list", sb.ToString());
                byte[] temp = Encoding.ASCII.GetBytes(y);
                requestStream.Write(temp, 0, temp.Length);
            }

            string response_text;

            using (WebResponse response = wr.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        {
                            responseStream.CopyTo(stream);
                            response_text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
            }

            AlbumMakeStatus ams = AlbumMakeStatus.NullResponse;

            if (!string.IsNullOrEmpty(response_text))
            {
                Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response_text);

                if (data.ContainsKey("x-status"))
                {
                    switch (Convert.ToString(data["x-status"]))
                    {
                        case "200":
                            ams = AlbumMakeStatus.Success;
                            break;
                        case "404":
                            ams = AlbumMakeStatus.MissingHash;
                            break;
                        case "413":
                            ams = AlbumMakeStatus.AlbumTooLarge;
                            break;
                        case "415":
                            ams = AlbumMakeStatus.InvalidHash;
                            break;
                        default:
                            ams = AlbumMakeStatus.Unkown;
                            break;
                    }

                    string hash = null;

                    if (data.ContainsKey("hash"))
                    {
                        hash = Convert.ToString(data["hash"]);
                    }

                    return new KeyValuePair<AlbumMakeStatus, string>(ams, hash);
                }
                else { return new KeyValuePair<AlbumMakeStatus, string>(AlbumMakeStatus.NullResponse, null); }

            }

            return new KeyValuePair<AlbumMakeStatus, string>(AlbumMakeStatus.Unkown, null); ;
        }

        public static HashInfo GetHashInfo(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                string data = "";

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
                    wc.Headers.Add("X-CORS-Status", "1");

                    data = wc.DownloadString(string.Format("{0}/{1}.json", ApplicationUrl, hash));
                }

                if (!string.IsNullOrEmpty(hash))
                {

                    JObject json = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(data);

                    return parse_hashinfo(json);

                }
            }

            return null;
        }

        public static HashInfo[] GetHashesInfo(string[] hashes)
        {
            if (hashes == null || hashes.Count() == 0)
            {
                throw new ArgumentNullException("Argument 'hashes' cannot be null or empty");
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashes.Length; i++)
            {
                sb.Append(hashes[i]);
                if (i != hashes.Length - 1)
                {
                    sb.Append(",");
                }
            }

            string data;

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
                wc.Headers.Add("X-CORS-Status", "1");
                data = wc.DownloadString(string.Format("{0}/api/info?list={1}", ApplicationUrl, sb.ToString()));
            }

            if (!string.IsNullOrEmpty(data))
            {
                JObject jsons = JsonConvert.DeserializeObject<JObject>(data);

                List<HashInfo> his = new List<HashInfo>(jsons.Count);

                foreach (string hash in hashes)
                {
                    if (jsons[hash].Type == JTokenType.Null) { continue; }
                    if (jsons[hash] != null)
                    {
                        his.Add(parse_hashinfo((JObject)(jsons[hash])));
                    }
                }

                return his.ToArray();
            }

            return null;
        }

        public static HashInfo GetHashInfoByURL(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                HashInfo[] a = GetHashesInfoByURL(new string[] { url });

                if (a.Length > 0)
                {
                    return a[0];
                }
            }

            return null;
        }

        public static HashInfo[] GetHashesInfoByURL(string[] urls)
        {
            if (urls == null || urls.Count() == 0)
            {
                throw new ArgumentNullException("Argument 'hashes' cannot be null or empty");
            }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/url/info", ApplicationUrl));

            wr.Headers.Add("X-CORS-Status", "1");
            wr.UserAgent = UserAgent;
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < urls.Length; i++)
            {
                sb.Append(urls[i]);
                if (i != urls.Length - 1)
                {
                    sb.Append(",");
                }
            }

            using (Stream requestStream = wr.GetRequestStream())
            {
                string y = string.Format("{0}={1}", "list", sb.ToString());
                byte[] temp = Encoding.ASCII.GetBytes(y);
                requestStream.Write(temp, 0, temp.Length);
            }

            string response_text;

            using (WebResponse response = wr.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        {
                            responseStream.CopyTo(stream);
                            response_text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(response_text))
            {
                JObject jsons = JsonConvert.DeserializeObject<JObject>(response_text);

                List<HashInfo> his = new List<HashInfo>(jsons.Count);

                foreach (string url in urls)
                {
                    if (jsons[url].Type == JTokenType.Null) { continue; }

                    if (jsons[url] != null)
                    {
                        his.Add(parse_hashinfo((JObject)(jsons[url])));
                    }
                }

                return his.ToArray();
            }

            return null;
        }

        public static bool HashExist(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                using (WebClient wc = new WebClient())
                {
                    string data;

                    wc.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
                    wc.Headers.Add("X-CORS-Status", "1");

                    data = wc.DownloadString(string.Format("{0}/api/{1}/exists", ApplicationUrl, hash));

                    if (!string.IsNullOrEmpty(data))
                    {
                        JObject json = JsonConvert.DeserializeObject<JObject>(data);

                        return Convert.ToBoolean(json["exists"]);
                    }
                    else { throw new Exception("Empty server response"); }
                }
            }
            else { throw new ArgumentNullException("Hash cannot be null"); }
        }

        public static KeyValuePair<HashStatus, HashInfo> GetHashStatus(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                Dictionary<string, KeyValuePair<HashStatus, HashInfo>> t = GetHashesStatus(new string[] { hash });
                if (t.ContainsKey(hash))
                {
                    return t[hash];
                }
                else
                {
                    new KeyValuePair<HashStatus, HashInfo>(HashStatus.Unknown, null);
                }
            }
            { throw new ArgumentNullException("Hash cannot be null"); }
        }

        /// <summary>
        /// Returns a dictionary with the hashes supplied as keys, and 
        /// the KeyValuePairs contain the HashStatus and the HashInfo if
        /// the HashStatus is HashStatus.Done, otherwise the HashInfo is null
        /// </summary>
        /// <param name="hashes"></param>
        /// <returns></returns>
        public static Dictionary<string, KeyValuePair<HashStatus, HashInfo>> GetHashesStatus(string[] hashes)
        {
            if (hashes == null || hashes.Count() == 0)
            {
                throw new ArgumentNullException("Argument 'hashes' cannot be null or empty");
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashes.Length; i++)
            {
                sb.Append(hashes[i]);
                if (i != hashes.Length - 1)
                {
                    sb.Append(",");
                }
            }

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
                string data = wc.DownloadString(string.Format("{0}/api/status?list={1}", ApplicationUrl, sb.ToString()));

                if (!string.IsNullOrEmpty(data))
                {
                    JObject jsons = JsonConvert.DeserializeObject<JObject>(data);

                    Dictionary<string, KeyValuePair<HashStatus, HashInfo>> dic = new Dictionary<string, KeyValuePair<HashStatus, HashInfo>>();

                    foreach (string hash in hashes)
                    {
                        JObject json = (JObject)jsons[hash];

                        if (json.Type == JTokenType.Null) { continue; }

                        string status = Convert.ToString(json["status"]);

                        HashStatus hs = HashStatus.Unknown;
                        HashInfo hi = null;

                        switch (status)
                        {
                            case "done":
                                hs = HashStatus.Done;
                                hi = parse_hashinfo((JObject)json["file"]);
                                break;
                            case "ready":
                                hs = HashStatus.Ready; break;
                            case "pending":
                                hs = HashStatus.Pending; break;
                            case "error":
                                hs = HashStatus.Error; break;
                            case "timeout":
                                hs = HashStatus.Timeout; break;
                            case "unrecognised":
                                hs = HashStatus.Unrecognised; break;
                            case "internal_error":
                                hs = HashStatus.InternalError; break;
                            default:
                                hs = HashStatus.Unknown; break;
                        }

                        dic.Add(hash, new KeyValuePair<HashStatus, HashInfo>(hs, hi));
                    }

                    return dic;
                }
                else { throw new Exception("Empty server response"); }
            }
        }

        public static IFlags GetHashFlags(string hash)
        {
            if (!string.IsNullOrEmpty(hash))
            {
                using (WebClient wc = new WebClient())
                {
                    string data = wc.DownloadString(string.Format("{0}/api/{1}/flags", ApplicationUrl, hash));

                    if (!string.IsNullOrEmpty(data))
                    {
                        JObject json = JsonConvert.DeserializeObject<JObject>(data);

                        if (json["flags"] != null)
                        {
                            JObject flags = (JObject)json["flags"];

                            if (flags["autoplay"] != null && flags["loop"] != null && flags["mute"] != null)
                            {
                                return parse_video_flags(flags);
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static KeyValuePair<FlagsManipulationResponse, IFlags> SetHashFlags(string hash, KeyValuePair<string, object>[] flags)
        {
            if (flags == null || flags.Length == 0) { throw new ArgumentNullException("Flag list cannot be null or empty"); }
            if (string.IsNullOrEmpty(hash)) { throw new ArgumentNullException("Hash cannot be null"); }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/{1}/flags", ApplicationUrl, hash));

            wr.Headers.Add("X-CORS-Status", "1");
            wr.UserAgent = UserAgent;
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";


            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}={1}", flags[0].Key, flags[0].Value);

            for (int i = 1; i < flags.Length; i++)
            {
                sb.AppendFormat("&{0}={1}", flags[i].Key, flags[i].Value);
            }

            using (Stream requestStream = wr.GetRequestStream())
            {
                byte[] temp = Encoding.ASCII.GetBytes(sb.ToString());
                requestStream.Write(temp, 0, temp.Length);
            }

            string response_text;

            using (WebResponse response = wr.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        {
                            responseStream.CopyTo(stream);
                            response_text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(response_text))
            {

                JObject j = JsonConvert.DeserializeObject<JObject>(response_text);

                FlagsManipulationResponse fmr = FlagsManipulationResponse.Unknown;

                if (j["x-status"] != null)
                {
                    switch (Convert.ToString(j["x-status"]))
                    {
                        case "200":
                            fmr = FlagsManipulationResponse.UpdateSuccessful;
                            break;
                        case "401":
                            fmr = FlagsManipulationResponse.IpMismatch;
                            break;
                        case "404":
                            fmr = FlagsManipulationResponse.InvalidHash;
                            break;
                        case "415":
                            fmr = FlagsManipulationResponse.InvalidParameters;
                            break;
                        default:
                            fmr = FlagsManipulationResponse.Unknown;
                            break;
                    }

                    IFlags new_flags = null;

                    if (fmr == FlagsManipulationResponse.UpdateSuccessful)
                    {
                        JObject new_flags_json = (JObject)j["flags"];

                        if (new_flags_json["autoplay"] != null && new_flags_json["loop"] != null && new_flags_json["mute"] != null)
                        {
                            new_flags = parse_video_flags(new_flags_json);
                        }
                    }

                    return new KeyValuePair<FlagsManipulationResponse, IFlags>(FlagsManipulationResponse.EmptyResponse, new_flags);
                }
            }
            else
            {
                return new KeyValuePair<FlagsManipulationResponse, IFlags>(FlagsManipulationResponse.EmptyResponse, null);
            }

            return new KeyValuePair<FlagsManipulationResponse, IFlags>(FlagsManipulationResponse.Unknown, null);
        }

        public static DeleteHashStatus DeleteHash(string hash)
        {
            if (string.IsNullOrEmpty(hash)) { throw new ArgumentNullException("Hash cannot be null"); }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/{1}", ApplicationUrl, hash));

            wr.Headers.Add("X-CORS-Status", "1");
            wr.UserAgent = UserAgent;
            wr.Method = "DELETE";
            // wr.ContentType = "application/x-www-form-urlencoded";

            string response_text;

            using (WebResponse response = wr.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        {
                            responseStream.CopyTo(stream);
                            response_text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(response_text))
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(response_text);

                if (json["x-status"] != null)
                {
                    switch (Convert.ToString(json["x-status"]))
                    {
                        case "200":
                            return DeleteHashStatus.Success;
                        case "401":
                            return DeleteHashStatus.IpMismatch;
                        case "404":
                            return DeleteHashStatus.NoSuchHash;
                        default:
                            return DeleteHashStatus.Unknown;
                    }
                }


            }
            else
            {
                return DeleteHashStatus.NoResponse;
            }

            return DeleteHashStatus.Unknown;
        }

        /// <summary>
        /// Return a KeyValuePair with the response status as the key and another
        /// KeyValuePair as the value. The second KeyValuePair has the file hash as the key 
        /// (when the response status is UploadFileStatus.Success or UploadFileStatus.AlreadyUploaded),
        /// and the value is the HashInfo only when the response status is UploadFileStatus.AlreadyUploaded.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>> UploadFile(string filename)
        {
            FileInfo fi;

            if (File.Exists(filename))
            {
                fi = new FileInfo(filename);
            }
            else
            {
                throw new Exception("Invalid file path or the file does not exist");
            }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/upload/file", ApplicationUrl));

            wr.Headers.Add("X-CORS-Status", "1");
            wr.UserAgent = UserAgent;
            wr.Method = "POST";

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", System.Globalization.NumberFormatInfo.InvariantInfo);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using (Stream requestStream = wr.GetRequestStream())
            {
                var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                requestStream.Write(buffer, 0, buffer.Length);

                buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", fi.Name, Environment.NewLine));
                requestStream.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", get_mime_type(fi.Extension), Environment.NewLine));
                requestStream.Write(buffer, 0, buffer.Length);

                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(requestStream);
                }

                buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                requestStream.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(boundary + "--");
                requestStream.Write(buffer, 0, buffer.Length);
            }

            string response_text;

            using (WebResponse response = wr.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        {
                            responseStream.CopyTo(stream);
                            response_text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(response_text))
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(response_text);

                UploadFileStatus ufs = UploadFileStatus.Unknown;

                if (json["x-status"] != null)
                {
                    ufs = parse_uploadfile_status(Convert.ToString(json["x-status"]));

                    if (ufs == UploadFileStatus.Success || ufs == UploadFileStatus.AlreadyUploaded)
                    {
                        string hash = Convert.ToString(json["hash"]);

                        HashInfo hi = null;

                        if (json[hash] != null)
                        {
                            hi = parse_hashinfo((JObject)json[hash]);
                        }

                        return new KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>(ufs, new KeyValuePair<string, HashInfo>(hash, hi));
                    }
                }
            }
            else
            {
                return new KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>(UploadFileStatus.NoResponse, new KeyValuePair<string, HashInfo>());
            }

            return new KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>(UploadFileStatus.Unknown, new KeyValuePair<string, HashInfo>());
        }

        /// <summary>
        /// Return a KeyValuePair with the response status as the key and another
        /// KeyValuePair as the value. The second KeyValuePair has the file hash as the key 
        /// (when the response status is UploadFileStatus.Success or UploadFileStatus.AlreadyUploaded),
        /// and the value is the HashInfo only when the response status is UploadFileStatus.AlreadyUploaded.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>> UploadFileByURL(string url)
        {
            if (string.IsNullOrEmpty(url)) { throw new ArgumentNullException("URL cannot be empty"); }

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/upload/url", ApplicationUrl));

            wr.Headers.Add("X-CORS-Status", "1");
            wr.UserAgent = UserAgent;
            wr.Method = "POST";
            wr.ContentType = "application/x-www-form-urlencoded";

            using (Stream requestStream = wr.GetRequestStream())
            {
                byte[] temp = Encoding.ASCII.GetBytes(string.Format("{0}={1}", "url", url));
                requestStream.Write(temp, 0, temp.Length);
            }

            string response_text;

            using (WebResponse response = wr.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        {
                            responseStream.CopyTo(stream);
                            response_text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(response_text))
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(response_text);

                UploadFileStatus ufs = UploadFileStatus.Unknown;

                if (json["x-status"] != null)
                {
                    ufs = parse_uploadfile_status(Convert.ToString(json["x-status"]));

                    if (ufs == UploadFileStatus.Success || ufs == UploadFileStatus.AlreadyUploaded)
                    {
                        string hash = Convert.ToString(json["hash"]);

                        HashInfo hi = null;

                        if (json[hash] != null)
                        {
                            hi = parse_hashinfo((JObject)json[hash]);
                        }

                        return new KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>(ufs, new KeyValuePair<string, HashInfo>(hash, hi));
                    }
                }
            }
            else
            {
                return new KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>(UploadFileStatus.NoResponse, new KeyValuePair<string, HashInfo>());
            }

            return new KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>(UploadFileStatus.Unknown, new KeyValuePair<string, HashInfo>());
        }

        #region Private Parsers

        private static UploadFileStatus parse_uploadfile_status(string status)
        {
            switch (status)
            {
                case "200":
                    return UploadFileStatus.Success;
                case "409":
                    return UploadFileStatus.AlreadyUploaded;
                case "400":
                    return UploadFileStatus.InvalidURL;
                case "420":
                    return UploadFileStatus.RateLimitExceeded;
                case "415":
                    return UploadFileStatus.UnacceptableFileExtension;
                case "404":
                    return UploadFileStatus.URLNotFound;
                default:
                    return UploadFileStatus.Unknown;
            }
        }

        private static string get_mime_type(string ext)
        {
            return "application/octet-stream";
        }

        private static HashInfo parse_hashinfo(JObject json)
        {
            if (json["blob_type"] != null)
            {
                HashInfo hs = new HashInfo(Convert.ToString(json["hash"]));

                switch (Convert.ToString(json["blob_type"]))
                {
                    case "image":
                        hs.Type = HashInfo.BlobType.Image;
                        break;
                    case "audio":
                        hs.Type = HashInfo.BlobType.Audio;
                        break;
                    case "video":
                        hs.Type = HashInfo.BlobType.Video;
                        break;
                    default:
                        break;
                }

                if (json["compression"] != null)
                {
                    hs.CompressionRatio = Convert.ToDouble(json["compression"]);
                }


                if (json["files"] != null)
                {
                    hs.Files = parse_files((Newtonsoft.Json.Linq.JArray)json["files"]);
                }

                if (json["extras"] != null)
                {
                    hs.ExtraFiles = parse_files((Newtonsoft.Json.Linq.JArray)json["extras"]);
                }

                if (json["metadata"] != null && json["metadata"].Type != JTokenType.Null)
                {
                    hs.Metadata = parse_meta((Newtonsoft.Json.Linq.JObject)json["metadata"]);
                }

                if (json["original"] != null)
                {
                    hs.Original = Convert.ToString(json["original"]);
                }

                if (json["type"] != null)
                {
                    hs.OriginalMimeType = Convert.ToString(json["type"]);
                }

                if (hs.Type == HashInfo.BlobType.Video)
                {
                    if (json["flags"] != null)
                    {
                        hs.Flags = parse_video_flags((Newtonsoft.Json.Linq.JObject)json["flags"]);
                    }
                }

                return hs;
            }

            return null;
        }

        private static FileEntry[] parse_files(JArray files)
        {
            if (files.Count > 0)
            {
                List<FileEntry> l = new List<FileEntry>(files.Count);
                foreach (JObject obj in files)
                {
                    l.Add(new FileEntry()
                    {
                        File = Convert.ToString(obj["file"]),
                        MimeType = Convert.ToString(obj["type"]),
                        Url = Convert.ToString(obj["url"])
                    });
                }
                return l.ToArray();
            }
            return null;
        }

        private static IMetadata[] parse_meta(JObject meta)
        {
            if (meta.Count > 0)
            {
                List<IMetadata> l = new List<IMetadata>(meta.Count);

                if (meta["dimensions"] != null)
                {
                    l.Add(new DimensionMetaData()
                    {
                        Height = Convert.ToInt32(meta["dimensions"]["height"]),
                        Width = Convert.ToInt32(meta["dimensions"]["width"])
                    });
                }

                if (meta["has_audio"] != null)
                {
                    if (Convert.ToBoolean(meta["has_audio"]))
                    {
                        l.Add(new AudioAvailableMetaData());
                    }
                }

                if (meta["has_video"] != null)
                {
                    if (Convert.ToBoolean(meta["has_video"]))
                    {
                        l.Add(new VideoAvailableMetaData());
                    }
                }

                return l.ToArray();
            }

            return null;
        }

        private static VideoFlags parse_video_flags(JObject flags)
        {
            if (flags.Count > 0)
            {
                VideoFlags f = new VideoFlags();

                if (flags["autoplay"] != null)
                {
                    f.AutoPlay = Convert.ToBoolean(flags["autoplay"]);
                }

                if (flags["loop"] != null)
                {
                    f.Loop = Convert.ToBoolean(flags["loop"]);
                }

                if (flags["mute"] != null)
                {
                    f.Mute = Convert.ToBoolean(flags["mute"]);
                }

                return f;
            }

            return null;
        }

        #endregion

    }
}