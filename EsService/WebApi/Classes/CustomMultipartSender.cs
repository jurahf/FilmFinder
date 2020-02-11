/// FORM https://stackoverflow.com/questions/19954287/how-to-upload-file-to-server-with-http-post-multipart-form-data


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebApi.Classes
{
    public class CustomMultipartSender
    {
        /// <summary>
        /// Creates HTTP POST request & uploads database to server.
        /// </summary>
        public string UploadFilesToServer(Uri uri, string fileName, byte[] fileData, string fileContentType = "application/octet-stream")
        {
            string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            httpWebRequest.Method = "POST";

            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                WriteMultipartForm(requestStream, boundary, fileName, fileContentType, fileData);
            }

            var response = httpWebRequest.GetResponse();

            string responseString = "";
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                responseString = sr.ReadToEnd();
            }

            return responseString;
        }

        /// <summary>
        /// Writes multi part HTTP POST request. 
        /// </summary>
        private void WriteMultipartForm(Stream s, string boundary, string fileName, string fileContentType, byte[] fileData)
        {
            /// The first boundary
            byte[] boundarybytes = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
            /// the last boundary.
            byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            /// the form-data file upload, properly formatted
            string fileheaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n";

            WriteToStream(s, boundarybytes);
            WriteToStream(s, string.Format(fileheaderTemplate, "photo", fileName, fileContentType));
            /// Write the file data to the stream.
            WriteToStream(s, fileData);
            WriteToStream(s, trailer);
        }

        /// <summary>
        /// Writes string to stream. 
        /// </summary>
        private void WriteToStream(Stream s, string txt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(txt);
            s.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes byte array to stream. 
        /// </summary>
        private void WriteToStream(Stream s, byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
        }


    }
}