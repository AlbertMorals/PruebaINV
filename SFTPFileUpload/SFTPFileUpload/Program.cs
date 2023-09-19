using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.Security.Cryptography;

namespace SFTPFileUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Create client Object");
            //using (SftpClient sftpClient = new SftpClient(getSftpConnection("10.91.34.7", "UsrCompas", 22, "filePath")))
            using (SftpClient sftpClient = new SftpClient("10.91.34.7",22, "UsrCompas", "Compas123"))
            {
                Console.WriteLine("Connect to server");
                sftpClient.Connect();
                Console.WriteLine("Creating FileStream object to stream a file");
                using (FileStream fs = new FileStream("filePath.txt", FileMode.Open))
                {
                    sftpClient.BufferSize = 1024;
                    sftpClient.UploadFile(fs, Path.GetFileName("filePath.txt"));
                }
                IEnumerable<SftpFile> ListFiles;

                ListFiles = sftpClient.ListDirectory("");

                string path = @"C:\descargasSFTP\";

                foreach (var item in ListFiles)
                {
                    //Console.Write(item.LastWriteTime.Value.ToString("u"));
                    //Console.Write(item.Length.ToString().PadLeft(10, ' '));
                    if (item.Name.Length>3 && item.Name.Substring(item.Name.Length-3) == "pdf" )
					{
                        Console.Write(" {0}", item.Name);
                        Console.ReadLine();

                        byte[] tmplnewf;

                        string newfile = path + item.Name;

                        using (var file = File.OpenWrite(path + item.Name))
                        {
                            sftpClient.DownloadFile(item.Name, file);

                            
                        }

                        tmplnewf = FileToByteArray(newfile);

                        //var checksumAlgorithm = ChecksumAlgorithm.SHA256;

                        //var MD5Hash = HashHelper.ComputeMD5(fpath);

                        var remoteChecksum = sftpClient.GetAttributes(item.Name);

                        var fileC = new FileInfo(path + item.Name);
                        var localChecksum = fileC.GetHashCode();

                        byte[] tmpsftpf = sftpClient.ReadAllBytes(item.Name);

                        //Hash archivo guardado
                        byte[] tmpNewHash;
                        
                        tmpNewHash = GetHashSha256(newfile);

                        //Si el hash MD5 es igual se borra el archivo que esta en el SFTP
                        if (GetMD5checksum(tmpsftpf) == GetMD5checksum(tmplnewf))
						{
                            //Borrar archivo del SFTP
                            //sftpClient.DeleteFile(item.Name);

                        }
						else
						{
                            //Borrar archivo local descargado del SFTP
                            File.Delete(newfile);
                        }
                    }

                    
                    
                }

                sftpClient.Dispose();
            }
            Console.ReadLine();
        }

        // The cryptographic service provider.
        public SHA256 Sha256 = SHA256.Create();

        // Compute the file's hash.
        private static byte[] GetHashSha256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                SHA256 Sha2562 = SHA256.Create();
                return Sha2562.ComputeHash(stream);
            }
        }

        public static byte[] FileToByteArray(string fileName)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(fileName).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        public static string GetMD5checksum(byte[] inputData)
        {

            //convert byte array to stream
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            stream.Write(inputData, 0, inputData.Length);

            //important: get back to start of stream
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            //get a string value for the MD5 hash.
            using (var md5Instance = System.Security.Cryptography.MD5.Create())
            {
                var hashResult = md5Instance.ComputeHash(stream);

                //***I did some formatting here, you may not want to remove the dashes, or use lower case depending on your application
                return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
            }
        }

        public static ConnectionInfo getSftpConnection(string host, string username, int port, string publicKeyPath)
        {
            return new ConnectionInfo(host, port, username, privateKeyObject(username, publicKeyPath));
        }

        private static AuthenticationMethod[] privateKeyObject(string username, string publicKeyPath)
        {
            PrivateKeyFile privateKeyFile = new PrivateKeyFile(publicKeyPath);
            PrivateKeyAuthenticationMethod privateKeyAuthenticationMethod = new PrivateKeyAuthenticationMethod(username, privateKeyFile);
            return new AuthenticationMethod[] { privateKeyAuthenticationMethod };
        }
       
    }
}
