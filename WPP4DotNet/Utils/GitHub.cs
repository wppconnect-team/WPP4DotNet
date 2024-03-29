﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using static WPP4DotNet.Models.GitHubModels;

namespace WPP4DotNet.Utils
{
    internal class GitHub
    {
        /// <summary>
        /// This method checks if you have WPPJS updates on GitHUb, if you do, download and update.
        /// </summary>
        /// <param name="directory">Set the directory if you want to change the default location.</param>
        /// <returns>Return True or False</returns>
        internal bool CheckUpdate(string directory = "")
        {
            try
            {
                var path = string.IsNullOrEmpty(directory) ? Path.Combine(Environment.CurrentDirectory, "WppConnect"): Path.Combine(directory, "WppConnect");
                var package = Path.Combine(path, "package.json");

                Release release = CheckRelease("wppconnect-team", "wa-js");
                bool update = false;
                if (File.Exists(package))
                {
                    using (StreamReader file = new StreamReader(Path.Combine(path, "package.json")))
                    {
                        string json = file.ReadToEnd();
                        package? items = JsonConvert.DeserializeObject<package>(json);
                        if (items != null)
                        {
                            if (release.published_at > items.published_at)
                            {
                                update = true;
                            }
                        }
                    }
                }
                else
                {
                    update = true;
                }
                if (update)
                {
                    var down = Download(release.assets[0].browser_download_url, path, release.assets[0].name);
                    if (down)
                    {
                        package _package = new package()
                        {
                            name = release.name,
                            tag_name = release.tag_name,
                            file_name = release.assets[0].name,
                            file_url = release.assets[0].browser_download_url,
                            created_at = release.created_at,
                            published_at = release.published_at
                        };
                        using (StreamWriter file = File.CreateText(Path.Combine(path, "package.json")))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(file, _package);
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method validates which is the last release generated on GitHub.
        /// </summary>
        /// <param name="username">Set the user name.</param>
        /// <param name="repository">Set the repository.</param>
        /// <param name="tag">Set the tag, by default it uses "latest".</param>
        /// <returns></returns>
        internal Release CheckRelease(string username, string repository, string tag="")
        {
            try
            {
                string url = string.Format("https://api.github.com/repos/{0}/{1}/releases/", username, repository);
                url = string.IsNullOrEmpty(tag) ? url + "latest" : url + "tags/" + tag;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(url);
                RestRequest request = new RestRequest();
                request.Method = Method.Get;
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                RestResponse response = client.GetAsync(request).Result;
                if (response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                {
                    Release? release = JsonConvert.DeserializeObject<Release>(response.Content);
                    return release == null ? new Release():release;
                }
                return new Release();
            }
            catch (Exception)
            {
                return new Release();
            }
        }

        /// <summary>
        /// This method downloads and decompresses files.
        /// </summary>
        /// <param name="url">Set the URL.</param>
        /// <param name="path">Set the Path.</param>
        /// <param name="fileName">Set the file name.</param>
        /// <param name="unZip">Set true or false if you want to unzip.</param>
        /// <param name="deleteZip">set true or false if you want to delete.</param>
        /// <returns>Return True or False</returns>
        private bool Download(string url, string path, string fileName, bool unZip = false, bool deleteZip = false)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var filePath = Path.Combine(path, fileName);
                WebClient wb = new WebClient();
                wb.DownloadFile(new Uri(url), filePath);

                if (unZip)
                {
                    Unzip(filePath, path, true);
                    if (deleteZip)
                        File.Delete(path);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method decompresses ZIP files.
        /// </summary>
        /// <param name="archive">Set the archive.</param>
        /// <param name="destination">Set the destination.</param>
        /// <param name="overwrite">Set true or false if you want to override.</param>
        /// <returns>Return True or False</returns>
        private bool Unzip(string archive, string destination, bool overwrite)
        {
            try
            {
                if (!overwrite)
                {
                    ZipFile.ExtractToDirectory(archive, destination);
                    return true;
                }
                using (FileStream zipToOpen = new FileStream(archive, FileMode.Open))
                {
                    using (ZipArchive zipfile = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(destination);
                        string destinationDirectoryFullPath = di.FullName;

                        foreach (ZipArchiveEntry file in zipfile.Entries)
                        {
                            string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                            if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new IOException("Trying to extract file outside of destination directory.");
                            }

                            if (file.Name == "")
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                                continue;
                            }
                            file.ExtractToFile(completeFileName, true);
                        }
                        zipToOpen.Close();
                    }
                    zipToOpen.Dispose();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
